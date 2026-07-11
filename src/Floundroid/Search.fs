namespace Floundroid

open System
open System.Threading
open System.Runtime.CompilerServices
open Microsoft.FSharp.NativeInterop

#nowarn "9" // For NativePtr usage

module Search =
    // Tunining Parameters
    // RFP
    let RFP_Margin = 60
    let RFP_MaxDepth = 5
    
    // NMP
    let NMP_MinDepth = 2
    let NMP_DepthThreshold = 6
    let NMP_BaseReduction = 5
    let NMP_DeepReduction = 5
    
    // LMR
    let LMR_MinDepth = 2
    let LMR_MinMoves = 7
    let LMR_Reduction = 2
    let LMR_DeepReduction = 3
    let LMR_Deep_Move_Threshold = 16
    
    // Move Ordering
    let Ordering_MVV_Multiplier = 15
    let Ordering_Killer_1 = 8800
    let Ordering_Killer_2 = 6800
    let Ordering_History_Max = 5600
    let Ordering_Capture_Base = 10400 
    let Ordering_Promo_Base = 9000 
    let Ordering_History_Bonus_Multiplier = 1.0 

    // Aspiration
    let Aspiration_Initial_Delta = 50

    // ProbCut
    let ProbCut_Margin = 380

    //Singule Extensions
    let Singular_Beta_Margin = 5

    // History Gravity
    let History_Gravity = 32768

    
    let mutable nodes = 0uL
    let MATE_VALUE = 30000
    let INF = 1000000

    // Stores two quiet moves per ply that caused a beta cutoff
    let killerMoves: int [,] = Array2D.create 2 256 0

    // Add this to clear killers at the start of every search
    let clearKillers () =
        for i in 0 .. 1 do
            for j in 0 .. 255 do
                killerMoves.[i, j] <- 0
    
    // History table: [Side][From][To]
    let historyTable = Array3D.create 2 64 64 0

    let clearHistory () =
        for c in 0..1 do
            for f in 0..63 do
                for t in 0..63 do
                    historyTable.[c, f, t] <- 0    
    
    let isRepetition (hash: uint64) (history: uint64 list) =
        history |> List.contains hash
    
     // History Update Helper (with Gravity) ---
    let updateHistory (side: int) (m: int) (depth: int) (isBonus: bool) =
        let fromSq, toSq = Move.fromSq m, Move.toSq m
        let bonus = if isBonus then depth * depth else -(depth * depth)
        let current = historyTable.[side, fromSq, toSq]
    
        // Gravity formula: ensures values don't hit the cap and stay relevant
        let newValue = current + bonus - (current * Math.Abs(bonus) / History_Gravity)
        historyTable.[side, fromSq, toSq] <- Math.Clamp(newValue, -Ordering_History_Max, Ordering_History_Max)
 
    /// Quiescence search: plays out tactical moves until the position is stable.
    let rec quiesce (b: Board) (ply: int) (alpha: int) (beta: int) (ct: CancellationToken) : int =
        nodes <- nodes + 1uL
        if ct.IsCancellationRequested then alpha
        else
            let sideMult = if b.SideToMove = Colour.White then 1 else -1
            let standPat = Evaluation.evaluate b * sideMult

            if standPat >= beta then beta
            else
                let mutable currentAlpha = Math.Max(alpha, standPat)

                // 1. Stack Allocation for Moves and Scores (Max 256 moves)
                let movePtr = NativePtr.stackalloc<int> 256
                let scorePtr = NativePtr.stackalloc<int> 256
                let moveSpan = Span<int>(NativePtr.toVoidPtr movePtr, 256)
                let scoreSpan = Span<int>(NativePtr.toVoidPtr scorePtr, 256)

                // 2. Generate moves directly into the stack span
                let moveCount = MoveGen.getCaptureMoves b moveSpan
            
                // 3. Score moves (MVV-LVA)
                for i in 0 .. moveCount - 1 do
                    let m = moveSpan.[i]
                    let victimVal = 
                        let victim = Board.tryGetPiece b (Move.toSq m)
                        if victim <> -1 then Pst.matsMG[Piece.kind victim] else 100
                    let attackerVal = 
                        let attacker = Board.tryGetPiece b (Move.fromSq m)
                        if attacker <> -1 then Pst.matsMG[Piece.kind attacker] else 0
                    // Simple score: high victim value first, then low attacker value
                    scoreSpan.[i] <- (victimVal * 100) - attackerVal

                // 4. Search Loop with Selection Sort (Pick Best)
                let mutable i = 0
                let mutable exitLoop = false

                while i < moveCount && not exitLoop do
                    // --- Selection Pick ---
                    // Instead of sorting the whole array, we find the best move 
                    // from the remaining unsorted part of the span.
                    let mutable bestIdx = i
                    for j in i + 1 .. moveCount - 1 do
                        if scoreSpan.[j] > scoreSpan.[bestIdx] then
                            bestIdx <- j
                
                    // Swap move and score to "position i"
                    let tempMove = moveSpan.[i]
                    moveSpan.[i] <- moveSpan.[bestIdx]
                    moveSpan.[bestIdx] <- tempMove
                
                    let tempScore = scoreSpan.[i]
                    scoreSpan.[i] <- scoreSpan.[bestIdx]
                    scoreSpan.[bestIdx] <- tempScore
                
                    let m = moveSpan.[i]
                    // -----------------------

                    let nextB = Board.applyMove m b
                    if Board.isInCheckFor b.SideToMove nextB then
                        i <- i + 1
                    else
                        let score = -quiesce nextB (ply + 1) (-beta) (-currentAlpha) ct

                        if score >= beta then
                            currentAlpha <- beta
                            exitLoop <- true
                        else
                            if score > currentAlpha then
                                currentAlpha <- score
                            i <- i + 1

                currentAlpha    
    
    /// Internal negamax implementation with advanced pruning, extensions, and zero-allocation move ordering.
    let rec negamaxInternal (b: Board) (depth: int) (ply: int) (alpha: int) (beta: int) (allowNull: bool) (excludedMove: int) (history: uint64 list) (ct: CancellationToken) : int * int =
        nodes <- nodes + 1uL
    
        // 1. Terminal Conditions
        if ct.IsCancellationRequested then (0, 0)
        elif ply > 0 && (isRepetition b.Hash history || b.HalfmoveClock >= 100 || Board.hasInsufficientMaterial b) then (0, 0)
        else
            // 2. Transposition Table Probe
            let ttEntry = TranspositionTable.probe b.Hash
            let mutable ttMove = 0
            let mutable ttValue = -INF
            let mutable ttResult = None

            match ttEntry with
            | Some entry ->
                ttMove <- entry.Move
                ttValue <- TranspositionTable.mateFromTT entry.Value ply
                // We skip TT cutoffs if we are currently searching for Singular Extensions (excludedMove <> 0)
                if excludedMove = 0 && entry.Depth >= depth then
                    match entry.Flag with
                    | TranspositionTable.NodeExact -> ttResult <- Some (ttValue, entry.Move)
                    | TranspositionTable.NodeAlpha when ttValue <= alpha -> ttResult <- Some (alpha, entry.Move)
                    | TranspositionTable.NodeBeta when ttValue >= beta -> ttResult <- Some (beta, entry.Move)
                    | _ -> ()
            | None -> ()

            if ttResult.IsSome then ttResult.Value
            elif depth <= 0 then (quiesce b ply alpha beta ct, 0)
            else
                let inCheck = Board.isInCheck b
                let sideMult = if b.SideToMove = Colour.White then 1 else -1
                let staticEval = Evaluation.evaluate b * sideMult
                let sideIdx = if b.SideToMove = Colour.White then 0 else 1

                // --- 3. ProbCut ---
                let mutable probCutoff = false
                if not inCheck && depth >= 5 && Math.Abs(beta) < MATE_VALUE - 100 then
                    let pcBeta = Math.Min(beta + ProbCut_Margin, MATE_VALUE - 1)
                    // Search at significantly reduced depth (Depth - 4)
                    let (pcScore, _) = negamaxInternal b (depth - 4) ply (pcBeta - 1) pcBeta true 0 history ct
                    if not ct.IsCancellationRequested && pcScore >= pcBeta then
                        probCutoff <- true

                if probCutoff then (beta, 0)
                else
                    // --- 4. Reverse Futility Pruning (RFP) ---
                    if excludedMove = 0 && not inCheck && depth <= RFP_MaxDepth && abs beta < (MATE_VALUE - 100) && staticEval - (RFP_Margin * depth) >= beta then
                        (beta, 0)
                    else
                        // --- 5. Null-Move Pruning (NMP) ---
                        let mutable nmpCutoffFound = false
                        if allowNull && excludedMove = 0 && depth >= NMP_MinDepth && not inCheck then
                            let hasMaterial = 
                                let bbs = b.Bitboards
                                if b.SideToMove = Colour.White then (bbs.WhiteTotal ^^^ bbs.WhitePawns) <> 0uL
                                else (bbs.BlackTotal ^^^ bbs.BlackPawns) <> 0uL
                    
                            if hasMaterial then
                                let R = if depth > NMP_DepthThreshold then NMP_DeepReduction else NMP_BaseReduction
                                let (nullValue, _) = negamaxInternal (Board.applyNullMove b) (depth - 1 - R) (ply + 1) (-beta) (-beta + 1) false 0 history ct
                                if not ct.IsCancellationRequested && -nullValue >= beta then
                                    nmpCutoffFound <- true

                        if nmpCutoffFound then (beta, 0)
                        else
                            // --- 6. Singular Extensions ---
                            let mutable extension = 0
                            if excludedMove = 0 && depth >= 8 && ttMove <> 0 && Math.Abs(ttValue) < MATE_VALUE - 100 then
                                let entry = ttEntry.Value
                                // If TT depth is sufficient, check if the move is singular
                                if entry.Depth >= depth - 3 && entry.Flag <> TranspositionTable.NodeAlpha then
                                    let singularBeta = ttValue - (depth * Singular_Beta_Margin)
                                    let (sScore, _) = negamaxInternal b (depth - 3) ply (singularBeta - 1) singularBeta true ttMove history ct
                                    if sScore < singularBeta then
                                        extension <- 1

                            // 7. Move Generation & Ordering (Zero-Allocation)
                            let movePtr = NativePtr.stackalloc<int> 256
                            let scorePtr = NativePtr.stackalloc<int> 256
                            let moveSpan = Span<int>(NativePtr.toVoidPtr movePtr, 256)
                            let scoreSpan = Span<int>(NativePtr.toVoidPtr scorePtr, 256)
                            let moveCount = MoveGen.getPseudoLegalMoves b moveSpan

                            // Buffer for History Malus: tracks quiet moves that didn't cause a cutoff
                            let triedQuietPtr = NativePtr.stackalloc<int> 256
                            let triedQuietSpan = Span<int>(NativePtr.toVoidPtr triedQuietPtr, 256)
                            let mutable triedQuietCount = 0

                            for i in 0 .. moveCount - 1 do
                                let m = moveSpan.[i]
                                scoreSpan.[i] <- 
                                    if m = ttMove then 1000000 
                                    else
                                        match Move.kind m with
                                        | 1 | 2 -> // Captures
                                            let victim = Board.tryGetPiece b (Move.toSq m)
                                            let victimVal = if victim <> -1 then Pst.matsMG[Piece.kind victim] else 100
                                            let attacker = Board.tryGetPiece b (Move.fromSq m)
                                            let attackerVal = if attacker <> -1 then Pst.matsMG[Piece.kind attacker] else 0
                                            Ordering_Capture_Base + (victimVal * Ordering_MVV_Multiplier) - attackerVal
                                        | 5 -> Ordering_Promo_Base + Pst.matsMG[Move.promo m]
                                        | _ -> // Quiet Moves
                                            if m = killerMoves.[0, ply] then Ordering_Killer_1
                                            elif m = killerMoves.[1, ply] then Ordering_Killer_2
                                            else Math.Min(historyTable.[sideIdx, (Move.fromSq m), (Move.toSq m)], Ordering_History_Max)

                            // 8. Search Loop
                            let mutable i = 0
                            let mutable currentAlpha = alpha
                            let originalAlpha = alpha
                            let mutable bestScore = -INF
                            let mutable bestMove = 0
                            let mutable legalMovesFound = 0
                            let mutable cutoffFound = false

                            while i < moveCount && not cutoffFound do
                                // --- Selection Pick ---
                                let mutable bestIdx = i
                                for j in i + 1 .. moveCount - 1 do
                                    if scoreSpan.[j] > scoreSpan.[bestIdx] then bestIdx <- j
                                let m = moveSpan.[bestIdx]
                                // Swap used move/score to end of 'already checked' section
                                moveSpan.[bestIdx] <- moveSpan.[i]; scoreSpan.[bestIdx] <- scoreSpan.[i] 

                                if m = excludedMove then 
                                    i <- i + 1
                                else
                                    // Legality Checks
                                    let isIllegalCastle = 
                                        match Move.kind m with
                                        | 3 | 4 -> inCheck || 
                                                   let rnk = if b.SideToMove = Colour.White then Rank.R1 else Rank.R8
                                                   let midFile = if Move.kind m = 3 then File.F else File.D
                                                   Board.isSquareAttacked b (Square.ofFileRank midFile rnk) (Colour.opposite b.SideToMove)
                                        | _ -> false

                                    if isIllegalCastle then i <- i + 1
                                    else
                                        let nextB = Board.applyMove m b
                                        if Board.isInCheckFor b.SideToMove nextB then i <- i + 1
                                        else
                                            legalMovesFound <- legalMovesFound + 1
                                            let isQuiet = Move.kind m = 0 || Move.kind m = 3 || Move.kind m = 4
                                            if isQuiet then 
                                                triedQuietSpan.[triedQuietCount] <- m
                                                triedQuietCount <- triedQuietCount + 1

                                            let mutable moveScore = -INF
                                            let searchDepth = depth - 1 + extension

                                            // PVS (Principal Variation Search) and LMR (Late Move Reduction)
                                            if depth >= LMR_MinDepth && legalMovesFound > LMR_MinMoves && not inCheck && isQuiet then
                                                let reduction = if legalMovesFound > LMR_Deep_Move_Threshold then LMR_DeepReduction else LMR_Reduction
                                                let (sLMR, _) = negamaxInternal nextB (searchDepth - reduction) (ply + 1) (-currentAlpha - 1) (-currentAlpha) true 0 (b.Hash :: history) ct
                                                moveScore <- -sLMR
                                                if moveScore > currentAlpha then
                                                    let (sFull, _) = negamaxInternal nextB searchDepth (ply + 1) (-currentAlpha - 1) (-currentAlpha) true 0 (b.Hash :: history) ct
                                                    moveScore <- -sFull
                                            elif legalMovesFound > 1 then
                                                let (sPVS, _) = negamaxInternal nextB searchDepth (ply + 1) (-currentAlpha - 1) (-currentAlpha) true 0 (b.Hash :: history) ct
                                                moveScore <- -sPVS
                                    
                                            if moveScore > currentAlpha || legalMovesFound = 1 then
                                                let (sFullWindow, _) = negamaxInternal nextB searchDepth (ply + 1) (-beta) (-currentAlpha) true 0 (b.Hash :: history) ct
                                                moveScore <- -sFullWindow

                                            if moveScore > bestScore then
                                                bestScore <- moveScore
                                                bestMove <- m
                                                if moveScore > currentAlpha then currentAlpha <- moveScore

                                            if currentAlpha >= beta then
                                                // --- 9. History Refinement (Bonus + Malus) ---
                                                if isQuiet then
                                                    // Standard Killer Update
                                                    if killerMoves.[0, ply] <> m then
                                                        killerMoves.[1, ply] <- killerMoves.[0, ply]
                                                        killerMoves.[0, ply] <- m
                                                
                                                    // Bonus for the move that caused the cutoff
                                                    updateHistory sideIdx m depth true
                                                    // Malus for all quiet moves that were tried before this one and failed
                                                    for j in 0 .. triedQuietCount - 2 do
                                                        updateHistory sideIdx triedQuietSpan.[j] depth false
                                            
                                                cutoffFound <- true
                                            else i <- i + 1           

                            // 10. Final Scoring & TT Storage
                            if legalMovesFound = 0 then
                                if inCheck then (-MATE_VALUE + ply, 0) else (0, 0)
                            else
                                if not ct.IsCancellationRequested && excludedMove = 0 then
                                    let flag = if bestScore <= originalAlpha then TranspositionTable.NodeAlpha elif bestScore >= beta then TranspositionTable.NodeBeta else TranspositionTable.NodeExact
                                    TranspositionTable.store b.Hash depth ply flag bestScore bestMove
                                (bestScore, bestMove)

    /// Negamax search with alpha-beta pruning and Transposition Table integration.
    let negamax (b: Board) (depth: int) (ply: int) (alpha: int) (beta: int) (history: uint64 list) (ct: CancellationToken) : int * int =
        negamaxInternal b depth ply alpha beta true 0 history ct

    /// Iterative Deepening with Aspiration Windows
    let findBestMove (b: Board) (maxDepth: int) (targetTimeMs: int) (history: uint64 list) (ct: CancellationToken) =
        async {
            do! Async.SwitchToThreadPool()

            nodes <- 0uL
            clearKillers()
            clearHistory()
        
            let sw = Diagnostics.Stopwatch.StartNew()
            let legalMoves = MoveGen.getLegalMoves b
            let mutable absoluteBestMove = if legalMoves.Length > 0 then legalMoves.[0] else 0

            let mutable d = 1
            let mutable lastScore = 0
        
            // 1. Use your tuned variable here
            let window = Aspiration_Initial_Delta 

            while d <= maxDepth && not ct.IsCancellationRequested do
                let mutable alpha = -INF
                let mutable beta = INF

                // 2. Initial Window Logic
                // We only narrow the bounds if we are deep enough (usually depth 5+)
                if d >= 5 then
                    alpha <- lastScore - window
                    beta <- lastScore + window

                let mutable (score, move) = negamax b d 0 alpha beta history ct 

                // 3. Check for Window Failure
                // LOGIC FIX: Only re-search if we actually set a window (d >= 5)
                if not ct.IsCancellationRequested && d >= 5 && (score <= alpha || score >= beta) then
                    // Fail high/low: reset to full bounds and search again
                    alpha <- -INF
                    beta <- INF
                    let (rescore, remove) = negamax b d 0 alpha beta history ct
                    score <- rescore
                    move <- remove

                // 4. Process Results
                if not ct.IsCancellationRequested then
                    lastScore <- score
                    let elapsed = sw.Elapsed.TotalSeconds
                    let nps = if elapsed > 0.001 then uint64 (float nodes / elapsed) else 0uL
                    if move <> 0 then
                        absoluteBestMove <- move
                        // Standard UCI output
                        printfn "info depth %d score cp %d nodes %d nps %d pv %s" d score nodes nps (Move.toUci move)

                // 5. Timer check
                let totalElapsed = sw.ElapsedMilliseconds
            
                // OPTIONAL: You can even tune this '0.6' factor later!
                if totalElapsed > int64 (float targetTimeMs * 0.6) then
                    d <- maxDepth + 1 
                else
                    d <- d + 1

            if absoluteBestMove = 0 && legalMoves.Length > 0 then
                absoluteBestMove <- legalMoves.[0]

            return absoluteBestMove
        }    
    
    
