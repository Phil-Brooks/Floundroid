namespace TestFloundroid

open System
open Xunit
open Program

// =========================
// === CORE TYPE TESTS   ===
// =========================

module ColourTests =

    [<Fact>]
    let ``Colour opposite works`` () =
        Assert.Equal(Black, Colour.opposite White)
        Assert.Equal(White, Colour.opposite Black)

    [<Fact>]
    let ``Colour char roundtrip`` () =
        let chars = [ 'w'; 'b'; 'W'; 'B' ]
        for c in chars do
            let col = Colour.fromChar c
            Assert.Equal(c.ToString().ToLower()[0], Colour.toChar col)


module FileTests =

    [<Fact>]
    let ``File int roundtrip`` () =
        for i in 0 .. 7 do
            let f = File.fromInt i
            Assert.Equal(i, File.toInt f)

    [<Fact>]
    let ``File char roundtrip`` () =
        for c in ['a' .. 'h'] do
            let f = File.fromChar c
            Assert.Equal(c, File.toChar f)


module RankTests =

    [<Fact>]
    let ``Rank int roundtrip`` () =
        for i in 0 .. 7 do
            let r = Rank.fromInt i
            Assert.Equal(i, Rank.toInt r)

    [<Fact>]
    let ``Rank char roundtrip`` () =
        for c in ['1' .. '8'] do
            let r = Rank.fromChar c
            Assert.Equal(c, Rank.toChar r)


module SquareTests =

    [<Fact>]
    let ``Square file/rank roundtrip`` () =
        for f in 0 .. 7 do
            for r in 0 .. 7 do
                let sq = Square.ofFileRank (File.fromInt f) (Rank.fromInt r)
                Assert.Equal(f, Square.file sq |> File.toInt)
                Assert.Equal(r, Square.rank sq |> Rank.toInt)

    [<Fact>]
    let ``Square string roundtrip`` () =
        let allSquares =
            [ for f in ['a' .. 'h'] do
                for r in ['1' .. '8'] do
                    $"{f}{r}" ]

        for s in allSquares do
            let sq = Square.fromString s
            Assert.Equal(s, Square.toString sq)


module PieceTypeTests =

    [<Fact>]
    let ``PieceType char roundtrip`` () =
        let chars = [ 'p'; 'n'; 'b'; 'r'; 'q'; 'k' ]
        for c in chars do
            let pt = PieceType.fromChar c
            Assert.Equal(c, PieceType.toChar pt)


module PieceTests =

    [<Fact>]
    let ``Piece char roundtrip`` () =
        let chars = [ 'p'; 'n'; 'b'; 'r'; 'q'; 'k'; 'P'; 'N'; 'B'; 'R'; 'Q'; 'K' ]
        for c in chars do
            let p = Piece.fromChar c
            Assert.Equal(c, Piece.toChar p)


// =========================
// === MOVE & BOARD TESTS ===
// =========================

module MoveTests =

    [<Theory>]
    [<InlineData("e2e4", "e2", "e4")>]
    [<InlineData("g1f3", "g1", "f3")>]
    [<InlineData("a7a8q", "a7", "a8")>]
    let ``UCI move string roundtrip`` (uci: string) (fromS: string) (toS: string) =
        let m = Move.fromUci uci
        Assert.Equal(Square.fromString fromS, m.From)
        Assert.Equal(Square.fromString toS, m.To)

        if uci.Length = 4 then
            Assert.Equal(Quiet, m.Kind)


module BoardTests =

    [<Fact>]
    let ``Empty board has no pieces`` () =
        let b = Board.empty
        for sq in 0 .. 63 do
            Assert.True(Board.tryGetPiece b sq |> Option.isNone)

    [<Fact>]
    let ``Set and get piece works`` () =
        let b = Board.empty
        let sq = Square.fromString "e4"
        let p = Piece.fromChar 'Q'
        let b2 = Board.setPiece b sq (Some p)
        Assert.Equal(Some p, Board.tryGetPiece b2 sq)

    [<Fact>]
    let ``applyMove updates piece positions and side to move`` () =
        let b =
            Board.fromFen
                "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"

        let m =
            { From = Square.fromString "e2"
              To = Square.fromString "e4"
              Kind = Quiet }

        let nextB = Board.applyMove m b
        Assert.False(Board.isOccupied nextB (Square.fromString "e2"))
        Assert.True(Board.isOccupied nextB (Square.fromString "e4"))
        Assert.Equal(Black, nextB.SideToMove)

    [<Fact>]
    let ``applyMove increments fullmove number after black moves`` () =
        let b =
            Board.fromFen
                "rnbqkbnr/pppppppp/8/8/8/P7/1PPPPPPP/RNBQKBNR b KQkq - 0 1"

        let m =
            { From = Square.fromString "a7"
              To = Square.fromString "a6"
              Kind = Quiet }

        let nextB = Board.applyMove m b
        Assert.Equal(2, nextB.FullmoveNumber)


// =========================
// === FEN & MOVEGEN TESTS ==
// =========================

module FenTests =

    [<Theory>]
    [<InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")>]
    [<InlineData("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1")>]
    [<InlineData("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1")>]
    let ``FEN roundtrip integrity`` (fen: string) =
        let board = Board.fromFen fen
        let output = Board.toFen board
        Assert.Equal(fen, output)


module MoveGenTests =

    [<Fact>]
    let ``Starting position has 20 pseudo-legal moves`` () =
        let b =
            Board.fromFen
                "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"

        let moves = MoveGen.getPseudoLegalMoves b
        Assert.Equal(20, moves.Length)

    [<Fact>]
    let ``Knight in center has 8 moves`` () =
        let b = Board.fromFen "8/8/8/8/4N3/8/8/8 w - - 0 1"
        let moves = MoveGen.getPseudoLegalMoves b
        Assert.Equal(8, moves.Length)

    [<Fact>]
    let ``Pawn captures correctly`` () =
        let b = Board.fromFen "8/8/8/3p1p2/4P3/8/8/8 w - - 0 1"
        let moves =
            MoveGen.getPseudoLegalMoves b
            |> Array.filter (fun m -> m.From = Square.fromString "e4")

        Assert.Equal(3, moves.Length)

        let captures =
            moves |> Array.filter (fun m -> m.Kind = Capture)

        Assert.Equal(2, captures.Length)

    [<Fact>]
    let ``Pawn En Passant is detected`` () =
        let b = Board.fromFen "8/8/8/3pP3/8/8/8/8 w - d6 0 1"
        let moves =
            MoveGen.getPseudoLegalMoves b
            |> Array.filter (fun m -> m.From = Square.fromString "e5")

        Assert.Contains(
            moves,
            fun m -> m.Kind = EnPassant && Square.toString m.To = "d6"
        )

    [<Fact>]
    let ``Slider logic stops at edge and captures enemies`` () =
        let b = Board.fromFen "8/8/4p3/8/4R3/8/4P3/8 w - - 0 1"
        let moves =
            MoveGen.getPseudoLegalMoves b
            |> Array.filter (fun m -> m.From = Square.fromString "e4")

        Assert.Equal(10, moves.Length)
        Assert.Contains(
            moves,
            fun m -> Square.toString m.To = "e6" && m.Kind = Capture
        )


// =========================
// === PROMOTION TESTS    ===
// =========================

module PromotionTests =

    [<Fact>]
    let ``White pawn generates 4 quiet promotion moves`` () =
        let b = Board.fromFen "8/4P3/8/8/8/8/8/8 w - - 0 1"
        let moves = MoveGen.getPseudoLegalMoves b

        let promos =
            moves
            |> Array.filter (fun m ->
                m.From = Square.fromString "e7"
                && m.To = Square.fromString "e8")

        Assert.Equal(4, promos.Length)

    [<Fact>]
    let ``White pawn generates 4 capture promotion moves`` () =
        let b = Board.fromFen "3p4/4P3/8/8/8/8/8/8 w - - 0 1"
        let moves = MoveGen.getPseudoLegalMoves b

        let promos =
            moves
            |> Array.filter (fun m ->
                m.From = Square.fromString "e7"
                && m.To = Square.fromString "d8")

        Assert.Equal(4, promos.Length)

    [<Fact>]
    let ``Black pawn generates 4 quiet promotion moves`` () =
        let b = Board.fromFen "8/8/8/8/8/8/4p3/8 b - - 0 1"
        let moves = MoveGen.getPseudoLegalMoves b

        let promos =
            moves
            |> Array.filter (fun m ->
                m.From = Square.fromString "e2"
                && m.To = Square.fromString "e1")

        Assert.Equal(4, promos.Length)

    [<Fact>]
    let ``Black pawn generates 4 capture promotion moves`` () =
        let b = Board.fromFen "8/8/8/8/8/8/4p3/5P2 b - - 0 1"
        let moves = MoveGen.getPseudoLegalMoves b

        let promos =
            moves
            |> Array.filter (fun m ->
                m.From = Square.fromString "e2"
                && m.To = Square.fromString "f1")

        Assert.Equal(4, promos.Length)


// =========================
// === ATTACK TESTS       ===
// =========================

module AttackTests =

    [<Fact>]
    let ``Square attacked by knight`` () =
        let b = Board.fromFen "8/8/8/3n4/4K3/8/8/8 w - - 0 1"
        Assert.True(Attack.isSquareAttacked b (Square.fromString "f4") Black)

    [<Fact>]
    let ``Square attacked by bishop`` () =
        let b = Board.fromFen "8/8/8/3b4/4K3/8/8/8 w - - 0 1"
        Assert.True(Attack.isSquareAttacked b (Square.fromString "e4") Black)

    [<Fact>]
    let ``Square attacked by pawn`` () =
        let b = Board.fromFen "8/8/8/3p4/4K3/8/8/8 w - - 0 1"
        Assert.True(Attack.isSquareAttacked b (Square.fromString "e4") Black)

    [<Fact>]
    let ``Square not attacked`` () =
        let b = Board.fromFen "8/8/8/8/4K3/8/8/8 w - - 0 1"
        Assert.False(Attack.isSquareAttacked b (Square.fromString "e4") Black)


// =========================
// === CHECK DETECTION    ===
// =========================

module CheckDetectionTests =

    [<Fact>]
    let ``White is in check from rook`` () =
        let b = Board.fromFen "4k3/8/8/8/8/8/4R3/4K3 b - - 0 1"
        Assert.True(Board.isInCheck b)

    [<Fact>]
    let ``White is not in check`` () =
        let b = Board.fromFen "4k3/8/8/8/8/8/8/4K3 w - - 0 1"
        Assert.False(Board.isInCheck b)

    [<Fact>]
    let ``Black is in check from knight`` () =
        let b = Board.fromFen "4k3/8/3N4/8/8/8/8/4K3 b - - 0 1"
        Assert.True(Board.isInCheck b)


// =========================
// === PIN DETECTION      ===
// =========================

module PinDetectionTests =

    [<Fact>]
    let ``Simple rook pin`` () =
        let b = Board.fromFen "4r3/8/8/8/8/8/4B3/4K3 w - - 0 1"
        let pins = Board.getPins b
        Assert.True(pins.ContainsKey(Square.fromString "e2"))

    [<Fact>]
    let ``Knight is never pinned`` () =
        let b = Board.fromFen "4r3/8/8/8/8/8/3N4/4K3 w - - 0 1"
        let pins = Board.getPins b
        Assert.False(pins.ContainsKey(Square.fromString "d2"))


// =========================
// === LEGAL MOVE FILTER ===
// =========================

module LegalMoveFilteringTests =

    [<Fact>]
    let ``Pinned knight has no legal moves`` () =
        let b = Board.fromFen "4r3/8/8/8/8/8/4N3/4K3 w - - 0 1"
        let moves = MoveGen.getLegalMoves b
        Assert.DoesNotContain(
            moves,
            fun m -> m.From = Square.fromString "e2"
        )

    [<Fact>]
    let ``King cannot move into check`` () =
        let b = Board.fromFen "4k3/8/8/4r3/8/8/8/4K3 w - - 0 1"
        let moves = MoveGen.getLegalMoves b
        Assert.DoesNotContain(
            moves,
            fun m -> m.To = Square.fromString "e2"
        )

    [<Fact>]
    let ``Illegal en passant exposing king is filtered out`` () =
        let b = Board.fromFen "4k3/8/8/8/3pP3/8/8/4K3 w - d6 0 1"
        let moves = MoveGen.getLegalMoves b
        Assert.DoesNotContain(
            moves,
            fun m -> m.Kind = EnPassant
        )
