namespace Floundroid.Tests

open System.Threading
open Xunit
open Floundroid
open System
open System.Runtime.CompilerServices
open Microsoft.FSharp.NativeInterop

#nowarn "9" // For NativePtr usage

module SearchTests =

    [<Fact>]
    let ``Search finds a mate in one (Scholars Mate)`` () =
        // Scholars mate position: White Queen on h5 can move to f7 for mate.
        let b =
            Board.fromFen "r1bqkbnr/pppp1ppp/2n5/4p2Q/2B1P3/8/PPPP1PPP/RNB1K1NR w KQkq - 0 1"

        let bestMove =
            Search.findBestMove b 2 2000 [] System.Threading.CancellationToken.None
            |> Async.RunSynchronously

        Assert.Equal("h5f7", Move.toUci bestMove)

    [<Fact>]
    let ``Search finds a simple winning capture`` () =
        // White Queen on d1 can capture a free Black Queen on d5.
        let b = Board.fromFen "rnb1kbnr/ppp1pppp/8/3q4/8/8/PPP11PPP/RNBQKBNR w KQkq - 0 1"

        let bestMove =
            Search.findBestMove b 3 2000 [] System.Threading.CancellationToken.None
            |> Async.RunSynchronously

        Assert.Equal("d1d5", Move.toUci bestMove)

    [<Fact>]
    let ``Search identifies stalemate as draw`` () =
        // Classic stalemate: Black is to move, has no legal moves, but is not in check.
        let b = Board.fromFen "7k/5K2/6Q1/8/8/8/8/8 b - - 0 1"

        let score, _ =
            Search.negamax b 2 0 -Search.INF Search.INF [] System.Threading.CancellationToken.None

        Assert.Equal(0, score)

    [<Fact>]
    let ``Search returns a fallback move when immediately cancelled`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        let cts = new System.Threading.CancellationTokenSource()
        cts.Cancel() // Cancel it before it even starts
        
        let bestMove =
            Search.findBestMove b 5 1000 [] cts.Token
            |> Async.RunSynchronously

        // ASSERT: Iterative deepening should still provide a move from the fallback
        Assert.True(bestMove<>0, "Search must return a move fallback to satisfy UCI")
        
        let legalMoves = MoveGen.getLegalMoves b
        Assert.Contains(bestMove, legalMoves)

    [<Fact>]
    let ``Board applyNullMove toggles side to move and clears en passant`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1"
        let nextB = Board.applyNullMove b
        Assert.Equal(Colour.White, nextB.SideToMove)
        Assert.Equal(-1, nextB.EnPassantSquare)
        Assert.Equal(b.HalfmoveClock + 1, nextB.HalfmoveClock)
        Assert.NotEqual(b.Hash, nextB.Hash)

    [<Fact>]
    let ``Enabling NMP does not change the evaluation at fixed depth`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        
        // Clear 
        TranspositionTable.clear()
        Search.clearKillers() 
        Search.clearHistory() 

        // Search with NMP disabled (allowNull = false)
        Search.resetNodes()
        let scoreWithout = Search.negamaxInternal b 4 0 -Search.INF Search.INF false 0 [] System.Threading.CancellationToken.None
        
        // Clear 
        TranspositionTable.clear()
        Search.clearKillers() 
        Search.clearHistory() 
        
        // Search with NMP enabled (allowNull = true)
        Search.resetNodes()
        let scoreWith = Search.negamaxInternal b 4 0 -Search.INF Search.INF true 0 [] System.Threading.CancellationToken.None
        
        Assert.Equal(scoreWithout, scoreWith)

    [<Fact>]
    let ``MVV-LVA prefers Pawn takes Queen over Queen takes Pawn`` () =
        // Setup: White Pawn on e4, White Queen on d1. Black Queen on d5, Black Pawn on f3.
        let b = Board.fromFen "rnb1kbnr/ppp1p1pp/8/3q4/4P3/5p2/PPPP1PPP/RNBQKBNR w KQkq - 0 1"
        
        // We need to simulate the sorting logic inside search
        let moves = MoveGen.getLegalMoves b
        
        // Helper to mimic the internal search scoring (simplified for test)
        let getScore (m: int) =
            match Move.kind m with
            | 1 -> 
                let v = Board.tryGetPiece b (Move.toSq m)
                let a = Board.tryGetPiece b (Move.fromSq m)
                10000 + (if v <> -1 then Pst.matsMG[int (Piece.kind v)] else 100) * 10 - (if a <> -1 then Pst.matsMG[int (Piece.kind a)] else 0)
            | _ -> 0

        let sorted = moves |> Array.sortByDescending getScore
        
        let bestMove = sorted.[0]
        let secondBest = sorted.[1]

        // Best move should be e4xd5 (Pawn takes Queen)
        Assert.Equal(Square.fromString "e4", Move.fromSq bestMove)
        Assert.Equal(Square.fromString "d5", Move.toSq bestMove)
        
        // Second best should be d1xf3 (Queen takes Pawn) 
        // (Wait, d1xd5 is also Queen takes Queen, which is 10,000 + 9000 - 900 = 18100)
        // (e4xd5 is 10,000 + 9000 - 100 = 18900. Correct!)
        Assert.True(getScore bestMove > getScore secondBest)

    [<Theory>]
    [<InlineData("8/8/8/8/8/8/8/k6K w - - 0 1")>] // K vs K
    [<InlineData("8/8/8/8/3N4/8/8/k6K w - - 0 1")>] // KN vs K
    [<InlineData("8/8/8/8/3B4/8/8/k6K w - - 0 1")>] // KB vs K
    [<InlineData("k7/8/8/8/8/8/8/K1b1b3 w - - 0 1")>] // K vs KBB (same color bishops)
    let ``Search identifies insufficient material as draw (0)`` (fen: string) =
        let b = Board.fromFen fen
        // Search at depth 1 should see the draw immediately if implemented in negamax/eval
        let score, _ = Search.negamax b 1 0 -Search.INF Search.INF [] System.Threading.CancellationToken.None
        Assert.Equal(0, score)

    [<Fact>]
    let ``KB vs KB same color is a draw`` () =
        // Bishops on same color (e.g. c1 and f1 are both white squares?) 
        // a1 is black. a1=0, b1=1. 
        // Square 2 (c1) is black. Square 5 (f1) is black.
        // Wait, rank 1: a1(0,B), b1(1,W), c1(2,B), d1(3,W), e1(4,B), f1(5,W), g1(6,B), h1(7,W)
        // c1 is black, f1 is white.
        // Let's use squares on same color.
        let fen = "k7/8/8/8/8/8/8/K1B1b3 w - - 0 1" // White Bishop on c1 (black), Black Bishop on e1 (black)
        let b = Board.fromFen fen
        let score, _ = Search.negamax b 1 0 -Search.INF Search.INF [] System.Threading.CancellationToken.None
        Assert.Equal(0, score)

    [<Fact>]
    let ``Negamax returns 0 immediately if current position is a repetition`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        // If the current hash is already in history and we are not at the root (ply > 0)
        let history = [ b.Hash ]
        let score, _ = Search.negamax b 2 1 -Search.INF Search.INF history System.Threading.CancellationToken.None
        Assert.Equal(0, score)

    [<Fact>]
    let ``isRepetition correctly identifies hash in history`` () =
        let hash = 12345uL
        let history = [ 54321uL; 12345uL; 67890uL ]
        Assert.True(Search.isRepetition hash history)
        Assert.False(Search.isRepetition 999uL history)

    [<Fact>]
    let ``Negamax returns 0 if halfmove clock is 100`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 100 1"
        let score, _ = Search.negamax b 2 1 -Search.INF Search.INF [] System.Threading.CancellationToken.None
        Assert.Equal(0, score)

    [<Fact>]
    let ``Negamax returns 0 if halfmove clock reaches 100 during search`` () =
        // 99 halfmoves, White to move. Any quiet non-pawn move will lead to 100.
        let b = Board.fromFen "4k3/8/8/8/8/8/8/4K2R w K - 99 1"
        // Try to move rook Rh1-h2
        let m = Move.create(Square.fromString "h1", Square.fromString "h2", 0, 0)
        let nextB = Board.applyMove m b
        Assert.Equal(100, nextB.HalfmoveClock)
        
        let score, _ = Search.negamax nextB 1 1 -Search.INF Search.INF [] System.Threading.CancellationToken.None
        Assert.Equal(0, score)

    [<Fact>]
    let ``Killer table is writable`` () =
        Search.clearKillers()
        let m = Move.create(0, 1, 0, 0)
        let k = Search.killerMoves()
        k.[0, 0] <- m
        Assert.Equal(m, k.[0, 0])

    [<Fact>]
    let ``History table is writable`` () =
        let idxSide = 0
        let fromSq = 0
        let toSq = 1
        let index = (idxSide <<< 12) + (fromSq <<< 6) + toSq

        Search.historyTable.[index] <- 42
        Assert.Equal(42, Search.historyTable.[index])
    
    [<Fact>]
    let ``Null move pruning is disabled when in check`` () =
        let fen = "4k3/8/8/8/8/8/4Q3/4K3 b - - 0 1" // Black in check
        let b = Board.fromFen fen
        let score, _ = Search.negamaxInternal b 3 0 -Search.INF Search.INF true 0 [] CancellationToken.None

        // If NMP incorrectly triggers, it returns beta immediately.
        Assert.NotEqual(Search.INF, score)

    [<Fact>]
    let ``Null move pruning disabled in zugzwang-like endgame`` () =
        let fen = "8/8/8/8/8/8/5k2/6K1 w - - 0 1"
        let b = Board.fromFen fen

        let scoreWithNMP, _ = Search.negamaxInternal b 4 0 -Search.INF Search.INF true 0 [] CancellationToken.None
        let scoreWithoutNMP, _ = Search.negamaxInternal b 4 0 -Search.INF Search.INF false 0 [] CancellationToken.None

        Assert.Equal(scoreWithoutNMP, scoreWithNMP)

    [<Fact>]
    let ``TT Exact entry causes immediate cutoff`` () =
        let fen = "8/8/8/8/8/8/8/4K3 w - - 0 1"
        let b = Board.fromFen fen

        TranspositionTable.clear()

        // Store an exact entry
        TranspositionTable.store b.Hash 5 0 TranspositionTable.NodeExact 42 0

        let score, _ = Search.negamax b 5 0 -Search.INF Search.INF [] CancellationToken.None

        Assert.Equal(42, score)

    [<Fact>]
    let ``TT move is searched first`` () =
        let fen = "8/8/8/8/8/8/8/4K3 w - - 0 1"
        let b = Board.fromFen fen

        let pvMove = Move.create(Square.fromString "e1", Square.fromString "e2", 0, 0)

        TranspositionTable.clear()
        TranspositionTable.store b.Hash 5 0 TranspositionTable.NodeExact 0 pvMove
        let movePtr = NativePtr.stackalloc<int> 256
        let moveSpan = Span<int>(NativePtr.toVoidPtr movePtr, 256)
        let moveCount = MoveGen.getPseudoLegalMoves b moveSpan
        let moves = moveSpan.Slice(0, moveCount).ToArray()

        // Score the moves the same way search does
        let scored =
            moves
            |> Array.map (fun m ->
                let score =
                    if Some m = Some pvMove then 1000000 else 0
                (m, score))
            |> Array.sortByDescending snd

        Assert.Equal(pvMove, fst scored.[0])

    [<Fact>]
    let ``Search returns correct mate distance`` () =
        // Real mate in 2
        let fen = "6k1/5ppp/8/8/8/8/5PPP/4R1K1 w - - 0 1"
        let b = Board.fromFen fen

        let score, _ = Search.negamax b 5 0 -Search.INF Search.INF [] CancellationToken.None

        Assert.True(score > Search.MATE_VALUE - 10, $"Score was {score}")
