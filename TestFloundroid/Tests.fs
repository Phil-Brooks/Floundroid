namespace TestFloundroid

open System
open Xunit
open Floundroid

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
        for i in 0..7 do
            let f = File.fromInt i
            Assert.Equal(i, File.toInt f)

    [<Fact>]
    let ``File char roundtrip`` () =
        for c in [ 'a' .. 'h' ] do
            let f = File.fromChar c
            Assert.Equal(c, File.toChar f)


module RankTests =

    [<Fact>]
    let ``Rank int roundtrip`` () =
        for i in 0..7 do
            let r = Rank.fromInt i
            Assert.Equal(i, Rank.toInt r)

    [<Fact>]
    let ``Rank char roundtrip`` () =
        for c in [ '1' .. '8' ] do
            let r = Rank.fromChar c
            Assert.Equal(c, Rank.toChar r)


module SquareTests =

    [<Fact>]
    let ``Square file/rank roundtrip`` () =
        for f in 0..7 do
            for r in 0..7 do
                let sq = Square.ofFileRank (File.fromInt f) (Rank.fromInt r)
                Assert.Equal(f, Square.file sq |> File.toInt)
                Assert.Equal(r, Square.rank sq |> Rank.toInt)

    [<Fact>]
    let ``Square string roundtrip`` () =
        let allSquares =
            [ for f in [ 'a' .. 'h' ] do
                  for r in [ '1' .. '8' ] do
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

        for sq in 0..63 do
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
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"

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
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/P7/1PPPPPPP/RNBQKBNR b KQkq - 0 1"

        let m =
            { From = Square.fromString "a7"
              To = Square.fromString "a6"
              Kind = Quiet }

        let nextB = Board.applyMove m b
        Assert.Equal(2, nextB.FullmoveNumber)

    [<Fact>]
    let ``Kingside castling moves rook`` () =
        let b = Board.fromFen "4k3/8/8/8/8/8/8/R3K2R w KQ - 0 1"

        let m =
            { From = Square.fromString "e1"
              To = Square.fromString "g1"
              Kind = CastleKingSide }

        let b2 = Board.applyMove m b

        Assert.True(Board.isOccupied b2 (Square.fromString "f1"))
        Assert.False(Board.isOccupied b2 (Square.fromString "h1"))

    [<Fact>]
    let ``En passant removes captured pawn`` () =
        let b = Board.fromFen "8/8/8/3pP3/8/8/8/8 w - d6 0 1"

        let m =
            { From = Square.fromString "e5"
              To = Square.fromString "d6"
              Kind = EnPassant }

        let b2 = Board.applyMove m b

        Assert.False(Board.isOccupied b2 (Square.fromString "d5"))
        Assert.True(Board.isOccupied b2 (Square.fromString "d6"))

    [<Fact>]
    let ``Promotion creates promoted piece`` () =
        let b = Board.fromFen "8/4P3/8/8/8/8/8/8 w - - 0 1"

        let m =
            { From = Square.fromString "e7"
              To = Square.fromString "e8"
              Kind = Promotion Queen }

        let b2 = Board.applyMove m b

        match Board.tryGetPiece b2 (Square.fromString "e8") with
        | Some p -> Assert.Equal(Queen, p.Kind)
        | None -> Assert.True(false)

    [<Fact>]
    let ``Moving rook removes kingside castling rights`` () =
        let b = Board.fromFen "4k3/8/8/8/8/8/8/4K2R w K - 0 1"

        let m =
            { From = Square.fromString "h1"
              To = Square.fromString "h2"
              Kind = Quiet }

        let b2 = Board.applyMove m b

        Assert.False(b2.CastlingRights.WhiteKingSide)

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
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"

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

        let captures = moves |> Array.filter (fun m -> m.Kind = Capture)

        Assert.Equal(2, captures.Length)

    [<Fact>]
    let ``Pawn En Passant is detected`` () =
        let b = Board.fromFen "8/8/8/3pP3/8/8/8/8 w - d6 0 1"

        let moves =
            MoveGen.getPseudoLegalMoves b
            |> Array.filter (fun m -> m.From = Square.fromString "e5")

        Assert.Contains(moves, fun m -> m.Kind = EnPassant && Square.toString m.To = "d6")

    [<Fact>]
    let ``Slider logic stops at edge and captures enemies`` () =
        let b = Board.fromFen "8/8/4p3/8/4R3/8/4P3/8 w - - 0 1"

        let moves =
            MoveGen.getPseudoLegalMoves b
            |> Array.filter (fun m -> m.From = Square.fromString "e4")

        Assert.Equal(10, moves.Length)
        Assert.Contains(moves, fun m -> Square.toString m.To = "e6" && m.Kind = Capture)

    [<Fact>]
    let ``Starting position has 20 legal moves`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"

        let moves = MoveGen.getLegalMoves b

        Assert.Equal(20, moves.Length)

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
            |> Array.filter (fun m -> m.From = Square.fromString "e7" && m.To = Square.fromString "e8")

        Assert.Equal(4, promos.Length)

    [<Fact>]
    let ``White pawn generates 4 capture promotion moves`` () =
        let b = Board.fromFen "3p4/4P3/8/8/8/8/8/8 w - - 0 1"
        let moves = MoveGen.getPseudoLegalMoves b

        let promos =
            moves
            |> Array.filter (fun m -> m.From = Square.fromString "e7" && m.To = Square.fromString "d8")

        Assert.Equal(4, promos.Length)

    [<Fact>]
    let ``Black pawn generates 4 quiet promotion moves`` () =
        let b = Board.fromFen "8/8/8/8/8/8/4p3/8 b - - 0 1"
        let moves = MoveGen.getPseudoLegalMoves b

        let promos =
            moves
            |> Array.filter (fun m -> m.From = Square.fromString "e2" && m.To = Square.fromString "e1")

        Assert.Equal(4, promos.Length)

    [<Fact>]
    let ``Black pawn generates 4 capture promotion moves`` () =
        let b = Board.fromFen "8/8/8/8/8/8/4p3/5P2 b - - 0 1"
        let moves = MoveGen.getPseudoLegalMoves b

        let promos =
            moves
            |> Array.filter (fun m -> m.From = Square.fromString "e2" && m.To = Square.fromString "f1")

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

    [<Fact>]
    let ``White pawn attacks upwards (should be detected but isSquareAttacked returns false)`` () =
        // White pawn on e4 should attack d5 and f5.
        let b = Board.fromFen "8/8/8/3p4/4P3/8/8/8 w - - 0 1"

        // d5 is attacked by the pawn on e4.
        let target = Square.fromString "d5"

        // EXPECTED: true
        Assert.True(Attack.isSquareAttacked b target White)


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
        Assert.DoesNotContain(moves, fun m -> m.From = Square.fromString "e2")

    [<Fact>]
    let ``King cannot move into check`` () =
        let b = Board.fromFen "4k3/8/8/4r3/8/8/8/4K3 w - - 0 1"
        let moves = MoveGen.getLegalMoves b
        Assert.DoesNotContain(moves, fun m -> m.To = Square.fromString "e2")

    [<Fact>]
    let ``Illegal en passant exposing king is filtered out`` () =
        let b = Board.fromFen "4k3/8/8/8/3pP3/8/8/4K3 w - d6 0 1"
        let moves = MoveGen.getLegalMoves b
        Assert.DoesNotContain(moves, fun m -> m.Kind = EnPassant)

    [<Fact>]
    let ``Cannot castle into attacked destination square`` () =
        let b = Board.fromFen "6rk/8/8/8/8/8/8/R3K2R w KQ - 0 1"

        let moves = MoveGen.getLegalMoves b

        Assert.DoesNotContain(moves, fun m -> m.Kind = CastleKingSide)

// =========================
// === SAN NOTATION TESTS ===
// =========================

module SanTests =

    let getSan (fen: string) (uci: string) =
        let b = Board.fromFen fen
        let m = Move.fromUci uci
        let moves = MoveGen.getLegalMoves b

        match moves |> Array.tryFind (fun lm -> lm.From = m.From && lm.To = m.To) with
        | Some actualMove -> San.toSan b actualMove
        | None ->
            let legalUcis = moves |> Array.map Move.toUci |> String.concat ", "
            failwithf "Move %s is ILLEGAL in FEN: %s. Legal moves are: %s" uci fen legalUcis

    [<Theory>]
    [<InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", "e2e4", "e4")>]
    [<InlineData("rnbqkbnr/ppp1pppp/8/3p4/4P3/8/PPPP1PPP/RNBQKBNR w KQkq - 0 2", "e4d5", "exd5")>]
    [<InlineData("r1bqkb1r/pppp1ppp/2n2n2/4p2Q/2B1P3/8/PPPP1PPP/RNB1K1NR w KQkq - 4 4", "h5f7", "Qxf7#")>]
    [<InlineData("r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1", "e1g1", "O-O")>]
    [<InlineData("8/4P3/8/8/8/8/k6K/8 w - - 0 1", "e7e8q", "e8=Q")>]
    let ``SAN basic notation works`` (fen: string) (uci: string) (expected: string) =
        Assert.Equal(expected, getSan fen uci)

    [<Fact>]
    let ``SAN disambiguates by file`` () =
        let fen = "7k/8/8/8/8/5N2/8/1N5K w - - 0 1"
        Assert.Equal("Nbd2", getSan fen "b1d2")

    [<Fact>]
    let ``SAN disambiguates by rank`` () =
        let fen = "7k/R7/8/8/8/8/R7/7K w - - 0 1"
        Assert.Equal("R2a4", getSan fen "a2a4")

    [<Fact>]
    let ``SAN handles promotion with capture and check`` () =
        // White pawn on e7, black rook on d8, Black King on h8
        // After exd8=Q, the Queen on d8 checks the King on h8 (same rank)
        let fen = "3r3k/4P3/8/8/8/8/8/7K w - - 0 1"
        Assert.Equal("exd8=Q+", getSan fen "e7d8q")

// =========================
// === PERFT DRIVER TESTS ===
// =========================

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
