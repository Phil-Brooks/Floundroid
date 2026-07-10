namespace Floundroid

open System
open System.Threading

module Search =
    //SPSA Tunining Parameters
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
    let mutable Ordering_MVV_Multiplier = 10
    let mutable Ordering_Killer_1 = 8000
    let mutable Ordering_Killer_2 = 7000
    let mutable Ordering_History_Max = 6000
    let mutable Ordering_Capture_Base = 10000 
    let mutable Ordering_Promo_Base = 9000 
    let mutable Ordering_History_Bonus_Multiplier = 1.0 

    // Aspiration
    let mutable Aspiration_Initial_Delta = 60

    
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
    
    /// Quiescence search: plays out tactical moves until the position is stable.
    let rec quiesce (b: Board) (ply: int) (alpha: int) (beta: int) (ct: CancellationToken)  : int =
        nodes <- nodes + 1uL
        if ct.IsCancellationRequested then alpha
        else
            let sideMult = if b.SideToMove = Colour.White then 1 else -1
            let standPat = Evaluation.evaluate b * sideMult

            if standPat >= beta then beta
            else
                let mutable currentAlpha = Math.Max(alpha, standPat)

                // 1. Generate only tactical moves
                let moves = MoveGen.getCaptureMoves b
                
                // 2. Simple MVV-LVA Scoring for QS
                let scores = Array.zeroCreate moves.Length
                for i in 0 .. moves.Length - 1 do
                    let m = moves.[i]
                    let victimVal = 
                        let victim = Board.tryGetPiece b (Move.toSq m)
                        if victim <> -1 then Pst.matsMG[Piece.kind victim] else 100 // En Passant
                    let attackerVal = 
                        let attacker = Board.tryGetPiece b (Move.fromSq m)
                        if attacker <> -1 then Pst.matsMG[Piece.kind attacker] else 0
                    scores.[i] <- -(10000 + (victimVal * 10) - attackerVal)

                System.Array.Sort(scores, moves)

                let mutable i = 0
                let mutable exitLoop = false

                while i < moves.Length && not exitLoop do
                    let m = moves.[i]
                    let nextB = Board.applyMove m b
                    
                    // 3. Legality Check
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
    
    /// Internal negamax implementation with Null-Move Pruning (allowNull).
    let rec negamaxInternal (b: Board) (depth: int) (ply: int) (alpha: int) (beta: int) (allowNull: bool) (history: uint64 list) (ct: CancellationToken) : int * int =
        nodes <- nodes + 1uL
        
        // 1. Terminal Conditions
        if ct.IsCancellationRequested then 
            (0, 0)
        elif ply > 0 && (isRepetition b.Hash history || b.HalfmoveClock >= 100 || Board.hasInsufficientMaterial b) then 
            (0, 0)
        else
            // 2. Transposition Table Probe
            let ttEntry = TranspositionTable.probe b.Hash
            let mutable ttMove = 0
            let mutable ttResult = None // Stores (value, flag) if we find a cutoff

            match ttEntry with
            | Some entry ->
                ttMove <- entry.Move
                let value = TranspositionTable.mateFromTT entry.Value ply
                if entry.Depth >= depth then
                    match entry.Flag with
                    | TranspositionTable.NodeExact -> ttResult <- Some (value, entry.Move)
                    | TranspositionTable.NodeAlpha when value <= alpha -> ttResult <- Some (alpha, entry.Move)
                    | TranspositionTable.NodeBeta when value >= beta -> ttResult <- Some (beta, entry.Move)
                    | _ -> ()
            | None -> ()

            if ttResult.IsSome then 
                ttResult.Value
            elif depth <= 0 then 
                (quiesce b ply alpha beta ct, 0)
            else
                let inCheck = Board.isInCheck b
                let sideMult = if b.SideToMove = Colour.White then 1 else -1
                let staticEval = Evaluation.evaluate b * sideMult

                // --- Reverse Futility Pruning (RFP) ---
                if not inCheck && depth <= RFP_MaxDepth && abs beta < (MATE_VALUE - 100) && staticEval - (RFP_Margin * depth) >= beta then
                    (beta, 0) // This is the "return" value for this branch
                else
                    let mutable nmpCutoff = false

                    // 3. Null-Move Pruning
                    if allowNull && depth >= NMP_MinDepth && not inCheck then
                        let bbs = b.Bitboards
                        let hasMaterial = 
                            if b.SideToMove = Colour.White then
                                bbs.WhiteKnights <> 0uL || bbs.WhiteBishops <> 0uL || bbs.WhiteRooks <> 0uL || bbs.WhiteQueens <> 0uL
                            else
                                bbs.BlackKnights <> 0uL || bbs.BlackBishops <> 0uL || bbs.BlackRooks <> 0uL || bbs.BlackQueens <> 0uL
                    
                        if hasMaterial then
                            let R = if depth > NMP_DepthThreshold then NMP_DeepReduction else NMP_BaseReduction
                            let nullBoard = Board.applyNullMove b
                            let (nullValue, _) = negamaxInternal nullBoard (depth - 1 - R) (ply + 1) (-beta) (-beta + 1) false history ct
                            if not ct.IsCancellationRequested && -nullValue >= beta then
                                nmpCutoff <- true

                    if nmpCutoff then 
                        (beta, 0)
                    else
                        // 4. Move Ordering
                        let moves = MoveGen.getPseudoLegalMoves b
                        let mutable bestScore = -INF
                        let mutable bestMove = 0
                        let mutable currentAlpha = alpha
                        let originalAlpha = alpha
                        let mutable legalMovesFound = 0
                        let sideIdx = if b.SideToMove = Colour.White then 0 else 1

                        let scores = Array.zeroCreate moves.Length
                        for i in 0 .. moves.Length - 1 do
                            let m = moves.[i]
                            let score = 
                                if m = ttMove then 1000000 
                                else
                                    match Move.kind m with
                                    | 1 | 2 -> 
                                        let victimVal = let victim = Board.tryGetPiece b (Move.toSq m) in if victim <> -1 then Pst.matsMG[Piece.kind victim] else 100
                                        let attackerVal = let attacker = Board.tryGetPiece b (Move.fromSq m) in if attacker <> -1 then Pst.matsMG[Piece.kind attacker] else 0
                                        Ordering_Capture_Base + (victimVal * Ordering_MVV_Multiplier) - attackerVal
                                    | 5 -> Ordering_Promo_Base + Pst.matsMG[Move.promo m]
                                    | _ -> 
                                        if Ordering_Killer_1 <= Ordering_Killer_2 then Ordering_Killer_1 <- Ordering_Killer_2 + 1 // Ensure killer1 is bigger
                                        if m = killerMoves.[0, ply] then Ordering_Killer_1
                                        elif m = killerMoves.[1, ply] then Ordering_Killer_2
                                        else Math.Min(historyTable.[sideIdx, (Move.fromSq m), (Move.toSq m)], Ordering_History_Max)
                            scores.[i] <- -score 

                        System.Array.Sort(scores, moves)

                        // 5. Search Loop
                        let mutable i = 0
                        let mutable cutoffFound = false
                    
                        while i < moves.Length && not cutoffFound do
                            let m = moves.[i]
                            let isIllegalCastle = 
                                match Move.kind m with
                                | 3 | 4 -> inCheck || 
                                           let rnk = if b.SideToMove = Colour.White then Rank.R1 else Rank.R8
                                           let midFile = if Move.kind m = 3 then File.F else File.D
                                           Board.isSquareAttacked b (Square.ofFileRank midFile rnk) (Colour.opposite b.SideToMove)
                                | _ -> false

                            if isIllegalCastle then
                                i <- i + 1
                            else
                                let nextB = Board.applyMove m b
                                if Board.isInCheckFor b.SideToMove nextB then
                                    i <- i + 1
                                else
                                    legalMovesFound <- legalMovesFound + 1
                                    let mutable moveScore = -INF

                                    // LMR and PVS
                                    if depth >= LMR_MinDepth && legalMovesFound > LMR_MinMoves && not inCheck && (Move.kind m = 0) then
                                        let reduction = if legalMovesFound > LMR_Deep_Move_Threshold then LMR_DeepReduction else LMR_Reduction
                                        let (sLMR, _) = negamaxInternal nextB (depth - 1 - reduction) (ply + 1) (-currentAlpha - 1) (-currentAlpha) true (b.Hash :: history) ct
                                        moveScore <- -sLMR
                                        if moveScore > currentAlpha then
                                            let (sFull, _) = negamaxInternal nextB (depth - 1) (ply + 1) (-currentAlpha - 1) (-currentAlpha) true (b.Hash :: history) ct
                                            moveScore <- -sFull
                                    elif legalMovesFound > 1 then
                                        let (sPVS, _) = negamaxInternal nextB (depth - 1) (ply + 1) (-currentAlpha - 1) (-currentAlpha) true (b.Hash :: history) ct
                                        moveScore <- -sPVS
                                
                                    if moveScore > currentAlpha || legalMovesFound = 1 then
                                        let (sFullWindow, _) = negamaxInternal nextB (depth - 1) (ply + 1) (-beta) (-currentAlpha) true (b.Hash :: history) ct
                                        moveScore <- -sFullWindow

                                    if moveScore > bestScore then
                                        bestScore <- moveScore
                                        bestMove <- m
                                        if moveScore > currentAlpha then currentAlpha <- moveScore

                                    if currentAlpha >= beta then
                                        if Move.kind m = 0 || Move.kind m = 3 || Move.kind m = 4 then
                                            if killerMoves.[0, ply] <> m then
                                                killerMoves.[1, ply] <- killerMoves.[0, ply]
                                                killerMoves.[0, ply] <- m
                                            
                                            // We multiply the depth-based reward by your tunable multiplier
                                            let bonus = int (float (depth * depth) * Ordering_History_Bonus_Multiplier)
                                            historyTable.[sideIdx, (Move.fromSq m), (Move.toSq m)] <- historyTable.[sideIdx, (Move.fromSq m), (Move.toSq m)] + bonus
                                        cutoffFound <- true
                                    else
                                        i <- i + 1           

                        // 6. Final Result Scoring
                        if legalMovesFound = 0 then
                            if inCheck then (-MATE_VALUE + ply, 0) else (0, 0)
                        else
                            if not ct.IsCancellationRequested then
                                let flag = if bestScore <= originalAlpha then TranspositionTable.NodeAlpha elif bestScore >= beta then TranspositionTable.NodeBeta else TranspositionTable.NodeExact
                                TranspositionTable.store b.Hash depth ply flag bestScore bestMove
                            (bestScore, bestMove)

    /// Negamax search with alpha-beta pruning and Transposition Table integration.
    let negamax (b: Board) (depth: int) (ply: int) (alpha: int) (beta: int) (history: uint64 list) (ct: CancellationToken) : int * int =
        negamaxInternal b depth ply alpha beta true history ct

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
    
    
