namespace Floundroid.Tests

open System
open Xunit
open Floundroid

module PerftTests =

    [<Fact>]
    let ``Initial position depth 1 is 20`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        let nodes = Perft.countNodes 1 b
        Assert.Equal(20uL, nodes)

    [<Fact>]
    let ``Initial position depth 2 is 400`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        let nodes = Perft.countNodes 2 b
        Assert.Equal(400uL, nodes)

    [<Fact>]
    let ``Kiwipete depth 1 is 48`` () =
        // Kiwipete is a famous perft test position with many captures and castling
        let b =
            Board.fromFen "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1"

        let nodes = Perft.countNodes 1 b
        Assert.Equal(48uL, nodes)

    [<Fact>]
    let ``Perft Position 3 Depth 3 is 2812`` () =
        // This position tests specific pawn/rook interactions
        let b = Board.fromFen "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1"
        Assert.Equal(2812uL, Perft.countNodes 3 b)

    [<Fact>]
    let ``Initial position depth 3 is 8902`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        Assert.Equal(8902uL, Perft.countNodes 3 b)

    [<Fact>]
    let ``Initial position perft divide depth 2 e2e4 is 20`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        let m = Move.create(Square.fromString "e2", Square.fromString "e4", 0, 0)
        Assert.Equal(20uL, Perft.countNodes 1 (Board.applyMove m b))

    [<Fact>]
    let ``SAN for e2e4 is e4`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        let m = Move.create(Square.fromString "e2", Square.fromString "e4", 0, 0)
        Assert.Equal("e4", Perft.toSan b m)

module DebugTests =

    [<Fact>]
    let ``Debug.verify accepts a legal starting position`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"

        let originalOut = Console.Out
        use sw = new System.IO.StringWriter()
        Console.SetOut(sw)

        try
            Debug.verify b
            let output = sw.ToString()
            Assert.Contains("consistent", output)
        finally
            Console.SetOut(originalOut)

    [<Fact>]
    let ``Debug verify detects two white kings`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQK1KR w KQkq - 0 1"
        let originalOut = Console.Out
        use sw = new System.IO.StringWriter()
        Console.SetOut(sw)

        try
            Debug.verify b
            let output = sw.ToString()
            Assert.Contains("Invalid White King count", output)
        finally
            Console.SetOut(originalOut)

    [<Fact>]
    let ``Debug verify detects pawn on illegal rank`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/8/P7 w KQkq - 0 1"
        let originalOut = Console.Out
        use sw = new System.IO.StringWriter()
        Console.SetOut(sw)

        try
            Debug.verify b
            let output = sw.ToString()
            Assert.Contains("Pawn on illegal rank", output)
        finally
            Console.SetOut(originalOut)

    [<Fact>]
    let ``Debug verify detects illegal check on side not to move`` () =
        // Black king is in check but it's WHITE to move → illegal
        let b = Board.fromFen "4k3/8/8/8/8/8/4Q3/4K3 w - - 0 1"
        let originalOut = Console.Out
        use sw = new System.IO.StringWriter()
        Console.SetOut(sw)

        try
            Debug.verify b
            let output = sw.ToString()
            Assert.Contains("Side NOT to move is in check", output)
        finally
            Console.SetOut(originalOut)

    [<Fact>]
    let ``Debug displayMoves prints legal moves with SAN`` () =
        let b = Board.fromFen "8/8/8/8/8/8/8/K6R w - - 0 1"
        let originalOut = Console.Out
        use sw = new System.IO.StringWriter()
        Console.SetOut(sw)

        try
            Debug.displayMoves b
            let output = sw.ToString()
            Assert.Contains("Legal Moves", output)
            Assert.Contains("h1h8", output)   // UCI
            Assert.Contains("Rh8", output)    // SAN
        finally
            Console.SetOut(originalOut)

    [<Fact>]
    let ``Debug displayAttackMap shows rook attack pattern`` () =
        let b = Board.fromFen "8/8/8/8/8/8/8/R6K w - - 0 1"
        let originalOut = Console.Out
        use sw = new System.IO.StringWriter()
        Console.SetOut(sw)

        try
            Debug.displayAttackMap b Colour.White
            let output = sw.ToString()
            // Rook on a1 attacks a2..a8 and b1..h1
            Assert.Contains("x . . . . . . .", output)  // rank 1
            Assert.Contains("x . . . . . . .", output)  // rank 2
        finally
            Console.SetOut(originalOut)

module UciTests =

    [<Fact>]
    let ``Position startpos moves e2e4 results in correct board`` () =
        let startFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        let mutable board = Board.fromFen startFen
        let moves = [ "e2e4" ]

        for mStr in moves do
            let legals = MoveGen.getLegalMoves board
            let m = legals |> Array.find (fun x -> Move.toUci x = mStr)
            board <- Board.applyMove m board

        Assert.Equal(Colour.Black, board.SideToMove)
        Assert.True(Board.isOccupied board (Square.fromString "e4"))
        Assert.False(Board.isOccupied board (Square.fromString "e2"))

    [<Fact>]
    let ``Go time target includes white increment when white is to move`` () =
        let args = [ "wtime"; "60000"; "btime"; "40000"; "winc"; "2000"; "binc"; "500" ]

        let targetTime = UciLoop.calculateTargetTime Colour.White args

        Assert.Equal(4000, targetTime)

    [<Fact>]
    let ``Go time target includes black increment when black is to move`` () =
        let args = [ "wtime"; "60000"; "btime"; "40000"; "winc"; "2000"; "binc"; "500" ]

        let targetTime = UciLoop.calculateTargetTime Colour.Black args

        Assert.Equal(2250, targetTime)

    [<Fact>]
    let ``tryGetIntArg extracts integer after key`` () =
        let args = [ "wtime"; "60000"; "btime"; "40000" ]
        Assert.Equal(Some 60000, UciLoop.tryGetIntArg "wtime" args)

    [<Fact>]
    let ``tryGetIntArg returns None when key missing`` () =
        let args = [ "wtime"; "60000" ]
        Assert.Equal(None, UciLoop.tryGetIntArg "depth" args)
