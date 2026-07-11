namespace Floundroid

open System
open System.Threading

module Perft =
    /// Converts a move to Standard Algebraic Notation (SAN) based on the current board state.
    let toSan (b: Board) (m: int) =
        match Move.kind m with
        | 3 -> "O-O"
        | 4 -> "O-O-O"
        | _ ->
            // Use Board.tryGetPiece instead of b.Pieces.Value
            let piece = Board.tryGetPiece b (Move.fromSq m)

            let isCapture =
                match Move.kind m with
                | 1
                | 2 -> true
                // Use Board.isOccupied instead of b.Pieces.ContainsKey
                | _ -> Board.isOccupied b (Move.toSq m)

            let nextBoard = Board.applyMove m b
            let isCheck = Board.isInCheck nextBoard
            let isMate = isCheck && (MoveGen.getLegalMoves nextBoard).Length = 0

            let moveStr =
                if Piece.kind piece = PieceType.Pawn then
                    let prefix =
                        if isCapture then
                            sprintf "%cx" (File.toChar (Square.file (Move.fromSq m)))
                        else
                            ""

                    let prom =
                        match Move.kind m with
                        | 5 -> sprintf "=%c" (Char.ToUpper(PieceType.toChar (Move.promo m)))
                        | _ -> ""

                    sprintf "%s%s%s" prefix (Square.toString (Move.toSq m)) prom
                else
                    let pChar = Char.ToUpper(PieceType.toChar (Piece.kind piece))

                    let others =
                        MoveGen.getLegalMoves b
                        |> Array.filter (fun alt ->
                            // Use Board.tryGetPiece to identify the piece on the alternative square
                            let altPiece = Board.tryGetPiece b (Move.fromSq alt)
                            if altPiece <> -1 then
                                (Move.fromSq alt) <> (Move.fromSq m) && (Move.toSq alt) = (Move.toSq m) && Piece.kind altPiece = Piece.kind piece
                            else
                                false)

                    let disambiguator =
                        if others.Length = 0 then
                            ""
                        else
                            let sameFile =
                                others |> Array.exists (fun alt -> Square.file (Move.fromSq alt) = Square.file (Move.fromSq m))

                            let sameRank =
                                others |> Array.exists (fun alt -> Square.rank (Move.fromSq alt) = Square.rank (Move.fromSq m))

                            if not sameFile then
                                sprintf "%c" (File.toChar (Square.file (Move.fromSq m)))
                            elif not sameRank then
                                sprintf "%c" (Rank.toChar (Square.rank (Move.fromSq m)))
                            else
                                Square.toString (Move.fromSq m)

                    let cap = if isCapture then "x" else ""
                    sprintf "%c%s%s%s" pChar disambiguator cap (Square.toString (Move.toSq m))

            let suffix =
                if isMate then "#"
                elif isCheck then "+"
                else ""

            moveStr + suffix

    /// Counts the number of leaf nodes at a given depth from the current board state.
    let rec countNodes depth b =
        if depth = 0 then
            1uL
        else
            let moves = MoveGen.getLegalMoves b

            if depth = 1 then
                uint64 moves.Length
            else
                let mutable total = 0uL

                for i in 0 .. moves.Length - 1 do
                    total <- total + countNodes (depth - 1) (Board.applyMove moves.[i] b)

                total

    /// Divides the perft calculation for a given depth and board state.
    let divide depth b =
        let sw = Diagnostics.Stopwatch.StartNew()
        let moves = MoveGen.getLegalMoves b |> Array.sortBy Move.toUci
        let mutable total = 0uL

        printfn "Perft results for depth %d:" depth

        for m in moves do
            let n = countNodes (depth - 1) (Board.applyMove m b)
            // Use the San module we just built!
            printfn "%s (%s): %d" (Move.toUci m) (toSan b m) n
            total <- total + n

        sw.Stop()
        let ms = sw.ElapsedMilliseconds
        let nps = if ms > 0L then (total * 1000uL) / uint64 ms else 0uL

        printfn "\nTotal: %d | Time: %d ms | NPS: %d" total ms nps
        total

    let suites =
        [ { Name = "Initial Position"
            Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
            Expected = [ 1uL; 20uL; 400uL; 8902uL; 197281uL; 4865609uL; 119060324uL ] }

          { Name = "Kiwipete"
            Fen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1"
            Expected = [ 1uL; 48uL; 2039uL; 97862uL; 4085603uL; 193690690uL ] }

          { Name = "Endgame/EP"
            Fen = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1"
            Expected = [ 1uL; 14uL; 191uL; 2812uL; 43238uL; 674624uL ] }

          { Name = "Promotion Stress Test"
            Fen = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8"
            Expected = [ 1uL; 44uL; 1486uL; 62379uL; 2103487uL; 89941194uL ] } ]

    /// Runs the full perft suite up to a specified maximum depth, comparing results against expected values.
    let runFullSuite (maxDepth: int) =
        printfn "Starting Perft Regression Suite (Max Depth: %d)" maxDepth
        printfn "------------------------------------------------"
        let totalSw = Diagnostics.Stopwatch.StartNew()

        for suite in suites do
            printfn "Testing: %s" suite.Name
            let b = Board.fromFen suite.Fen
            let depthsToTest = Math.Min(maxDepth, suite.Expected.Length - 1)

            for d in 1..depthsToTest do
                let expected = suite.Expected.[d]
                let sw = Diagnostics.Stopwatch.StartNew()
                let actual = countNodes d b
                sw.Stop()

                if actual = expected then
                    printfn "  Depth %d: PASS (%d nodes) in %dms" d actual sw.ElapsedMilliseconds
                else
                    printfn "  Depth %d: FAILED! Expected %d, got %d" d expected actual

            printfn ""

        totalSw.Stop()
        printfn "Full Suite Finished in %d ms" totalSw.ElapsedMilliseconds

module Debug =
    /// 1.5.1 - Move list visualisation
    let displayMoves (b: Board) =
        let moves = MoveGen.getLegalMoves b
        printfn "Legal Moves (%d):" moves.Length

        let formatted =
            moves
            |> Array.map (fun m -> sprintf "%s (%s)" (Move.toUci m) (Perft.toSan b m))
            |> String.concat ", "

        printfn "%s" formatted

/// 1.5.2 - Board consistency checker
    let verify (b: Board) =
        let errors = ResizeArray<string>()
        
        // Get all pieces from the bitboards
        let allPiecesList = BitboardSet.allPieces b.Bitboards |> Seq.toList
        let pieces = allPiecesList |> List.map snd

        // 1. Check Kings
        let whiteKings =
            pieces
            |> List.filter (fun p -> Piece.colour p = Colour.White && Piece.kind p = PieceType.King)
            |> List.length

        let blackKings =
            pieces
            |> List.filter (fun p -> Piece.colour p = Colour.Black && Piece.kind p = PieceType.King)
            |> List.length

        if whiteKings <> 1 then
            errors.Add(sprintf "Invalid White King count: %d" whiteKings)

        if blackKings <> 1 then
            errors.Add(sprintf "Invalid Black King count: %d" blackKings)

        // 2. Check Pawns
        // Replaced b.Pieces loop with the allPiecesList
        for (sq, p) in allPiecesList do
            if Piece.kind p = PieceType.Pawn then
                let r = Square.rank sq

                if r = 0 || r = 7 then
                    errors.Add(sprintf "Pawn on illegal rank %d at %s" (r + 1) (Square.toString sq))

        // 3. Side not to move cannot be in check
        if Board.isInCheckFor (Colour.opposite b.SideToMove) b then
            errors.Add("Illegal state: Side NOT to move is in check.")

        if errors.Count = 0 then
            printfn "Board state is consistent."
        else
            printfn "CONSISTENCY ERRORS FOUND:"

            for err in errors do
                printfn " - %s" err

    /// 1.5.3 - Attack map visualiser
    let displayAttackMap (b: Board) (attacker: int) =
        printfn "Attack Map for %A:" attacker

        for r in 7..-1..0 do
            printf "%d " (r + 1)

            for f in 0..7 do
                let sq = Square.ofFileRank f r

                if Board.isSquareAttacked b sq attacker then
                    printf "x "
                else
                    printf ". "

            printfn ""

        printfn "  a b c d e f g h"

module UciLoop =
    let mutable board = Board.start
    let mutable positionHistory: uint64 list = []
    // Track the current search task and its cancellation token
    let mutable searchCts = new CancellationTokenSource()
    let mutable currentSearchId = 0

    let tryGetIntArg (key: string) (args: string list) =
        args
        |> List.tryFindIndex ((=) key)
        |> Option.bind (fun i ->
            if i < args.Length - 1 then
                match Int32.TryParse(args.[i + 1]) with
                | true, value -> Some value
                | _ -> None
            else
                None)

    let calculateTargetTime (sideToMove: int) (args: string list) =
        let wtime = tryGetIntArg "wtime" args |> Option.defaultValue 100000
        let btime = tryGetIntArg "btime" args |> Option.defaultValue 100000
        let winc = tryGetIntArg "winc" args |> Option.defaultValue 0
        let binc = tryGetIntArg "binc" args |> Option.defaultValue 0

        let myTime, myIncrement =
            if sideToMove = Colour.White then
                wtime, winc
            else
                btime, binc

        Math.Max(1, (myTime / 20) + (myIncrement / 2))

    let rec run () =
        let line = Console.ReadLine()

        if line <> null then
            let ts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries) |> Array.toList

            match ts with
            | "uci" :: _ ->
                printfn "id name Floundroid"
                printfn "id author Phil Brooks"
                // Send options for tuning
                //printfn "option name RFP_Margin type spin default %d min 0 max 500" Search.RFP_Margin
                //printfn "option name RFP_MaxDepth type spin default %d min 1 max 10" Search.RFP_MaxDepth
                //printfn "option name NMP_MinDepth type spin default %d min 1 max 10" Search.NMP_MinDepth
                //printfn "option name NMP_DepthThreshold type spin default %d min 1 max 12" Search.NMP_DepthThreshold
                //printfn "option name NMP_BaseReduction type spin default %d min 1 max 5" Search.NMP_BaseReduction
                //printfn "option name NMP_DeepReduction type spin default %d min 1 max 7" Search.NMP_DeepReduction
                //printfn "option name LMR_MinDepth type spin default %d min 1 max 10" Search.LMR_MinDepth
                //printfn "option name LMR_MinMoves type spin default %d min 1 max 20" Search.LMR_MinMoves
                //printfn "option name LMR_Reduction type spin default %d min 1 max 5" Search.LMR_Reduction
                //printfn "option name LMR_DeepReduction type spin default %d min 1 max 7" Search.LMR_DeepReduction
                //printfn "option name LMR_Deep_Move_Threshold type spin default %d min 1 max 30" Search.LMR_Deep_Move_Threshold
                //printfn "option name Ordering_MVV_Multiplier type spin default %d min 1 max 100" Search.Ordering_MVV_Multiplier
                //printfn "option name Ordering_Killer_1 type spin default %d min 1000 max 20000" Search.Ordering_Killer_1
                //printfn "option name Ordering_Killer_2 type spin default %d min 1000 max 20000" Search.Ordering_Killer_2
                //printfn "option name Ordering_History_Max type spin default %d min 1000 max 20000" Search.Ordering_History_Max
                //printfn "option name Ordering_Capture_Base type spin default %d min 5000 max 30000" Search.Ordering_Capture_Base
                //printfn "option name Ordering_Promo_Base type spin default %d min 5000 max 30000" Search.Ordering_Promo_Base
                //printfn "option name Ordering_History_Bonus_Multiplier type string default %f" Search.Ordering_History_Bonus_Multiplier
                //printfn "option name Aspiration_Initial_Delta type spin default %d min 1 max 200" Search.Aspiration_Initial_Delta
                //printfn "option name ProbCut_Margin type spin default %d min 0 max 500" Search.ProbCut_Margin
                //printfn "option name Singular_Beta_Margin type spin default %d min 0 max 10" Search.Singular_Beta_Margin
                //printfn "option name History_Gravity type spin default %d min 1000 max 32768" Search.History_Gravity
                printfn "uciok"            
            | "setoption" :: "name" :: "Hash" :: "value" :: _ -> 
                () // Just ignore it for now, but it stops the warning
            | "setoption" :: "name" :: name :: "value" :: value :: _ ->
                let inline parseF (v: string) = 
                    match System.Double.TryParse v with 
                    | true, f -> f 
                    | _ -> 0.0

                match name with
                //| "RFP_Margin" -> Search.RFP_Margin <- int (System.Math.Round(parseF value))
                //| "RFP_MaxDepth" -> Search.RFP_MaxDepth <- int (System.Math.Round(parseF value))
                //| "NMP_MinDepth" -> Search.NMP_MinDepth <- int (System.Math.Round(parseF value))
                //| "NMP_DepthThreshold" -> Search.NMP_DepthThreshold <- int (System.Math.Round(parseF value))
                //| "NMP_BaseReduction" -> Search.NMP_BaseReduction <- int (System.Math.Round(parseF value))
                //| "NMP_DeepReduction" -> Search.NMP_DeepReduction <- int (System.Math.Round(parseF value))
                //| "LMR_MinDepth" -> Search.LMR_MinDepth <- int (System.Math.Round(parseF value))
                //| "LMR_MinMoves" -> Search.LMR_MinMoves <- int (System.Math.Round(parseF value))
                //| "LMR_Reduction" -> Search.LMR_Reduction <- int (System.Math.Round(parseF value))
                //| "LMR_DeepReduction" -> Search.LMR_DeepReduction <- int (System.Math.Round(parseF value))
                //| "LMR_Deep_Move_Threshold" -> Search.LMR_Deep_Move_Threshold <- int (System.Math.Round(parseF value))
                //| "Ordering_MVV_Multiplier" -> Search.Ordering_MVV_Multiplier <- int (System.Math.Round(parseF value))
                //| "Ordering_Killer_1" -> Search.Ordering_Killer_1 <- int (System.Math.Round(parseF value))
                //| "Ordering_Killer_2" -> Search.Ordering_Killer_2 <- int (System.Math.Round(parseF value))
                //| "Ordering_History_Max" -> Search.Ordering_History_Max <- int (System.Math.Round(parseF value))
                //| "Ordering_Capture_Base" -> Search.Ordering_Capture_Base <- int (System.Math.Round(parseF value))
                //| "Ordering_Promo_Base" -> Search.Ordering_Promo_Base <- int (System.Math.Round(parseF value))
                //| "Ordering_History_Bonus_Multiplier" -> Search.Ordering_History_Bonus_Multiplier <- parseF value
                //| "Aspiration_Initial_Delta" -> Search.Aspiration_Initial_Delta <- int (System.Math.Round(parseF value))
                //| "ProbCut_Margin" -> Search.ProbCut_Margin <- int (System.Math.Round(parseF value))
                //| "Singular_Beta_Margin" -> Search.Singular_Beta_Margin <- int (System.Math.Round(parseF value))
                //| "History_Gravity" -> Search.History_Gravity <- int (System.Math.Round(parseF value))
                | _ -> ()            
            
            | "isready" :: _ -> printfn "readyok"
            | "ucinewgame" :: _ -> 
                searchCts.Cancel() // Stop any running search immediately
                TranspositionTable.clear()
                Search.clearKillers() 
                Search.clearHistory() 
                board <- Board.start            
                positionHistory <- []
            
            | "position" :: rest ->
                let (bd, moveParts) =
                    match rest with
                    | "startpos" :: "moves" :: m -> (Board.start, m)
                    | "startpos" :: _ -> (Board.start, [])
                    | "fen" :: fParts ->
                        // Find where "moves" starts, if it exists
                        let movesIdx = fParts |> List.tryFindIndex (fun s -> s = "moves")

                        match movesIdx with
                        | Some i ->
                            let f = fParts |> List.take i |> String.concat " "
                            let m = fParts |> List.skip (i + 1)
                            (Board.fromFen f, m)
                        | None -> (Board.fromFen (String.concat " " fParts), [])
                    | _ -> (Board.start, [])

                board <- bd
                positionHistory <- [ board.Hash ]

                for mStr in moveParts do
                    let m = Board.fromUci board mStr
                    board <- Board.applyMove m board
                    positionHistory <- board.Hash :: positionHistory

                TranspositionTable.advanceAge()
            
            | "go" :: rest ->
                // 1. Calculate time from remaining clock plus side-to-move increment.
                let targetTime = calculateTargetTime board.SideToMove rest

                // 3. Prepare Cancellation
                searchCts.Cancel()
                searchCts <- new CancellationTokenSource()
                currentSearchId <- currentSearchId + 1
                // 2. CAPTURE CURRENT STATE FOR THE ASYNC THREAD
                let mySearchId = currentSearchId
                let currentCts = searchCts 
                let searchBoard = board 
                let currentHistory = positionHistory

                // --- THE ALARM CLOCK ---
                // This tells the token to automatically trigger 'Cancel' after targetTime ms
                let depthIdx = rest |> List.tryFindIndex (fun s -> s = "depth")
                if depthIdx.IsNone then
                    searchCts.CancelAfter(targetTime)

                let depth =
                    match depthIdx with
                    | Some i when i < rest.Length - 1 ->
                        match Int32.TryParse(rest.[i + 1]) with
                        | true, d -> d | _ -> 4
                    | _ -> 20

                Async.Start(
                    async {
                        try                        
                            // Use the token for the search itself
                            let! result = Search.findBestMove searchBoard depth targetTime currentHistory currentCts.Token
        
                            if mySearchId = currentSearchId then
                                if result <> 0 then
                                    printfn "bestmove %s" (Move.toUci result)
                                else
                                    // Fallback: If search failed, try to find any legal move
                                    let legals = MoveGen.getLegalMoves searchBoard
                                    if legals.Length > 0 then
                                        printfn "bestmove %s" (Move.toUci legals.[0])
                                    else
                                        printfn "bestmove (none)" // UCI standard for no legal moves
                        with
                        | ex -> printfn "info string Error in search: %s" ex.Message
                    }
                )                
            
            | "perft" :: rest ->
                match rest with
                | "suite" :: d :: _ ->
                    match Int32.TryParse d with
                    | true, depth -> Perft.runFullSuite depth
                    | _ -> printfn "Invalid depth: %s" d
                | "suite" :: _ -> Perft.runFullSuite 4
                | d :: _ ->
                    match Int32.TryParse d with
                    | true, depth -> Perft.divide depth board |> ignore
                    | _ -> printfn "Invalid depth: %s" d
                | [] ->
                    // Handles just the word "perft"
                    Perft.divide 1 board |> ignore

            | "print" :: _ -> Board.prettyPrint board

            | "moves" :: _ -> Debug.displayMoves board
            | "verify" :: _ -> Debug.verify board
            | "attacks" :: "white" :: _ -> Debug.displayAttackMap board Colour.White
            | "attacks" :: "black" :: _ -> Debug.displayAttackMap board Colour.Black
            | "testbb" :: _ ->
                let mutable bb = Bitboard.empty
                bb <- Bitboard.set (Square.fromString "e4") bb
                bb <- Bitboard.set (Square.fromString "d5") bb
                printfn "Bitboard counts: %d" (Bitboard.count bb)
                printfn "%s" (Bitboard.toString bb)

            | "stop" :: _ -> searchCts.Cancel() // This tells the search to die
            | "quit" :: _ ->
                searchCts.Cancel()
                Environment.Exit(0)
            | _ -> ()

            run ()
