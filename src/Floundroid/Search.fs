namespace Floundroid

open System
open System.Threading

module Search =
    let mutable nodes = 0uL
    let MATE_VALUE = 30000
    let INF = 1000000

    // Stores two quiet moves per ply that caused a beta cutoff
    let killerMoves: int option[,] = Array2D.create 2 256 None

    // Add this to clear killers at the start of every search
    let clearKillers () =
        for i in 0 .. 1 do
            for j in 0 .. 255 do
                killerMoves.[i, j] <- None
    
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
                        if victim <> -1 then Pst.matsMG[int (Piece.kind victim)] else 100 // En Passant
                    let attackerVal = 
                        let attacker = Board.tryGetPiece b (Move.fromSq m)
                        if attacker <> -1 then Pst.matsMG[int (Piece.kind attacker)] else 0
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
    let rec negamaxInternal (b: Board) (depth: int) (ply: int) (alpha: int) (beta: int) (allowNull: bool) (history: uint64 list) (ct: CancellationToken) : int * int option =
        nodes <- nodes + 1uL
        
        // 1. Terminal Conditions
        if ct.IsCancellationRequested then 
            (0, None)
        elif ply > 0 && (isRepetition b.Hash history || b.HalfmoveClock >= 100 || Board.hasInsufficientMaterial b) then 
            (0, None)
        else
            // 2. Transposition Table Probe
            let ttEntry = TranspositionTable.probe b.Hash
            let mutable ttMove = None
            let mutable ttResult = None // Stores (value, flag) if we find a cutoff

            match ttEntry with
            | Some entry ->
                ttMove <- Some(entry.Move)
                let value = TranspositionTable.mateFromTT entry.Value ply
                if entry.Depth >= depth then
                    match entry.Flag with
                    | TranspositionTable.NodeExact -> ttResult <- Some (value, Some(entry.Move))
                    | TranspositionTable.NodeAlpha when value <= alpha -> ttResult <- Some (alpha, Some(entry.Move))
                    | TranspositionTable.NodeBeta when value >= beta -> ttResult <- Some (beta, Some(entry.Move))
                    | _ -> ()
            | None -> ()

            if ttResult.IsSome then 
                ttResult.Value
            elif depth <= 0 then 
                (quiesce b ply alpha beta ct, None)
            else
                let inCheck = Board.isInCheck b
                let sideMult = if b.SideToMove = Colour.White then 1 else -1
                let staticEval = Evaluation.evaluate b * sideMult

                // --- Reverse Futility Pruning (RFP) ---
                if not inCheck && depth <= 3 && abs beta < (MATE_VALUE - 100) && staticEval - (120 * depth) >= beta then
                    (beta, None) // This is the "return" value for this branch
                else
                    let mutable nmpCutoff = false

                    // 3. Null-Move Pruning
                    if allowNull && depth >= 3 && not inCheck then
                        let bbs = b.Bitboards
                        let hasMaterial = 
                            if b.SideToMove = Colour.White then
                                bbs.WhiteKnights <> 0uL || bbs.WhiteBishops <> 0uL || bbs.WhiteRooks <> 0uL || bbs.WhiteQueens <> 0uL
                            else
                                bbs.BlackKnights <> 0uL || bbs.BlackBishops <> 0uL || bbs.BlackRooks <> 0uL || bbs.BlackQueens <> 0uL
                    
                        if hasMaterial then
                            let R = if depth > 6 then 3 else 2
                            let nullBoard = Board.applyNullMove b
                            let (nullValue, _) = negamaxInternal nullBoard (depth - 1 - R) (ply + 1) (-beta) (-beta + 1) false history ct
                            if not ct.IsCancellationRequested && -nullValue >= beta then
                                nmpCutoff <- true

                    if nmpCutoff then 
                        (beta, None)
                    else
                        // 4. Move Ordering
                        let moves = MoveGen.getPseudoLegalMoves b
                        let mutable bestScore = -INF
                        let mutable bestMove = None
                        let mutable currentAlpha = alpha
                        let originalAlpha = alpha
                        let mutable legalMovesFound = 0
                        let sideIdx = if b.SideToMove = Colour.White then 0 else 1

                        let scores = Array.zeroCreate moves.Length
                        for i in 0 .. moves.Length - 1 do
                            let m = moves.[i]
                            let score = 
                                if Some m = ttMove then 1000000 
                                else
                                    match Move.kind m with
                                    | 1 | 2 -> 
                                        let victimVal = let victim = Board.tryGetPiece b (Move.toSq m) in if victim <> -1 then Pst.matsMG[int (Piece.kind victim)] else 100
                                        let attackerVal = let attacker = Board.tryGetPiece b (Move.fromSq m) in if attacker <> -1 then Pst.matsMG[int (Piece.kind attacker)] else 0
                                        10000 + (victimVal * 10) - attackerVal
                                    | 5 -> 9000 + Pst.matsMG[Move.promo m]
                                    | _ -> 
                                        if Some m = killerMoves.[0, ply] then 8000
                                        elif Some m = killerMoves.[1, ply] then 7000
                                        else Math.Min(historyTable.[sideIdx, (Move.fromSq m), (Move.toSq m)], 6000)
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
                                    if depth >= 3 && legalMovesFound > 4 && not inCheck && (Move.kind m = 0) then
                                        let reduction = if legalMovesFound > 12 then 2 else 1
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
                                        bestMove <- Some m
                                        if moveScore > currentAlpha then currentAlpha <- moveScore

                                    if currentAlpha >= beta then
                                        if Move.kind m = 0 || Move.kind m = 3 || Move.kind m = 4 then
                                            if killerMoves.[0, ply] <> Some m then
                                                killerMoves.[1, ply] <- killerMoves.[0, ply]
                                                killerMoves.[0, ply] <- Some m
                                            historyTable.[sideIdx, (Move.fromSq m), (Move.toSq m)] <- historyTable.[sideIdx, (Move.fromSq m), (Move.toSq m)] + (depth * depth)
                                        cutoffFound <- true
                                    else
                                        i <- i + 1           

                        // 6. Final Result Scoring
                        if legalMovesFound = 0 then
                            if inCheck then (-MATE_VALUE + ply, None) else (0, None)
                        else
                            if not ct.IsCancellationRequested then
                                let flag = if bestScore <= originalAlpha then TranspositionTable.NodeAlpha elif bestScore >= beta then TranspositionTable.NodeBeta else TranspositionTable.NodeExact
                                TranspositionTable.store b.Hash depth ply flag bestScore bestMove.Value
                            (bestScore, bestMove)

    /// Negamax search with alpha-beta pruning and Transposition Table integration.
    let negamax (b: Board) (depth: int) (ply: int) (alpha: int) (beta: int) (history: uint64 list) (ct: CancellationToken) : int * int option =
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
            let mutable absoluteBestMove = if legalMoves.Length > 0 then Some legalMoves.[0] else None

            let mutable d = 1
            let mutable lastScore = 0
            let window = 60 // Starting margin 

            while d <= maxDepth && not ct.IsCancellationRequested do
                let mutable alpha = -INF
                let mutable beta = INF

                // 1. Set Aspiration Window
                // Only use windows after depth as early depths are too volatile
                if d >= 5 then
                    alpha <- lastScore - window
                    beta <- lastScore + window

                let mutable (score, moveOpt) = negamax b d 0 alpha beta history ct

                // 2. Check for Window Failure
                // If the score is outside our window, we must re-search with full bounds
                if not ct.IsCancellationRequested && d >= 3 && (score <= alpha || score >= beta) then
                    alpha <- -INF
                    beta <- INF
                    let (rescore, remove) = negamax b d 0 alpha beta history ct
                    score <- rescore
                    moveOpt <- remove

                // 3. Process Results
                if not ct.IsCancellationRequested then
                    lastScore <- score
                    let elapsed = sw.Elapsed.TotalSeconds
                    let nps = if elapsed > 0.001 then uint64 (float nodes / elapsed) else 0uL
                    match moveOpt with
                    | Some m ->
                        absoluteBestMove <- Some m
                        printfn "info depth %d score cp %d nodes %d nps %d pv %s" d score nodes nps (Move.toUci m)
                    | None -> ()

                // 4. Timer check
                let totalElapsed = sw.ElapsedMilliseconds
                if totalElapsed > int64 (targetTimeMs * 6 / 10) then
                    d <- maxDepth + 1 
                else
                    d <- d + 1

            // Fallback for safety
            if absoluteBestMove.IsNone && legalMoves.Length > 0 then
                absoluteBestMove <- Some legalMoves.[0]

            return absoluteBestMove
        }
