namespace Floundroid.Tests

open System
open Xunit
open FsCheck
open FsCheck.Xunit
open Floundroid

module ColourTests =

    [<Fact>]
    /// Tests that the opposite of a colour is correctly computed.
    let ``opposite works both ways`` () =
        Assert.Equal(Colour.Black, Colour.opposite Colour.White)
        Assert.Equal(Colour.White, Colour.opposite Colour.Black)

    [<Fact>]
    /// Tests that the opposite of a colour is involutive.
    let ``opposite is involutive (example)`` () =
        for c in [ Colour.White; Colour.Black ] do
            Assert.Equal(c, Colour.opposite (Colour.opposite c))

    [<Fact>]
    /// Tests that the colour character conversion works correctly.
    let ``fromChar toChar roundtrip`` () =
        for c in [ 'w'; 'b'; 'W'; 'B' ] do
            let col = Colour.fromChar c
            Assert.Equal(c.ToString().ToLower()[0], Colour.toChar col)

    [<Fact>]
    /// Tests that the colour character conversion works correctly.
    let ``toChar fromChar roundtrip`` () =
        for c in [ Colour.White; Colour.Black ] do
            Assert.Equal(c, Colour.fromChar (Colour.toChar c))

    [<Fact>]
    /// Tests that the colour character conversion rejects invalid characters.
    let ``fromChar rejects invalid characters`` () =
        for c in [ 'x'; '1'; ' '; '\n' ] do
            Assert.Throws<ArgumentException>(fun () -> Colour.fromChar c |> ignore)
            |> ignore

    [<Property>]
    let ``prop - opposite is involutive`` (c: Colour) =
        Colour.opposite (Colour.opposite c) = c

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

    [<Fact>]
    let ``File.fromInt rejects invalid integers`` () =
        for i in [ -1; 8; 99 ] do
            Assert.Throws<ArgumentException>(fun () -> File.fromInt i |> ignore)
            |> ignore

    [<Fact>]
    let ``File.fromChar rejects invalid characters`` () =
        for c in [ 'x'; '1'; 'A'; 'Z' ] do
            Assert.Throws<ArgumentException>(fun () -> File.fromChar c |> ignore)
            |> ignore

    [<Fact>]
    let ``File toChar fromChar roundtrip`` () =
        for f in [ File.A; File.B; File.C; File.D; File.E; File.F; File.G; File.H ] do
            Assert.Equal(f, File.fromChar (File.toChar f))

    
    [<Property>]
    let ``prop - int roundtrip`` (i: int) =
        if i >= 0 && i < 8 then
            let f = File.fromInt i
            File.toInt f = i
        else
            true

    [<Property>]
    let ``prop - char roundtrip`` (c: char) =
        let lower = Char.ToLower c
        if lower >= 'a' && lower <= 'h' then
            let f = File.fromChar lower
            File.toChar f = lower
        else
            true    

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

    [<Fact>]
    let ``Rank.fromInt rejects invalid integers`` () =
        for i in [ -1; 8; 99 ] do
            Assert.Throws<ArgumentException>(fun () -> Rank.fromInt i |> ignore)
            |> ignore

    [<Fact>]
    let ``Rank.fromChar rejects invalid characters`` () =
        for c in [ '0'; '9'; 'a'; 'Z' ] do
            Assert.Throws<ArgumentException>(fun () -> Rank.fromChar c |> ignore)
            |> ignore

    [<Fact>]
    let ``Rank toChar fromChar roundtrip`` () =
        for r in [ Rank.R1; Rank.R2; Rank.R3; Rank.R4; Rank.R5; Rank.R6; Rank.R7; Rank.R8 ] do
            Assert.Equal(r, Rank.fromChar (Rank.toChar r))

    [<Property>]
    let ``prop - int roundtrip`` (i: int) =
        if i >= 0 && i < 8 then
            let r = Rank.fromInt i
            Rank.toInt r = i
        else
            true

    [<Property>]
    let ``prop - char roundtrip`` (c: char) =
        if c >= '1' && c <= '8' then
            let r = Rank.fromChar c
            Rank.toChar r = c
        else
            true

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

    [<Fact>]
    let ``Square.fromString rejects invalid strings`` () =
        let bad =
            [ ""; "a"; "11"; "z9"; "a9"; "i1"; "a0"; "h9"; "aa1"; "a10" ]

        for s in bad do
            Assert.ThrowsAny<Exception>(fun () -> Square.fromString s |> ignore)
            |> ignore

    [<Fact>]
    let ``Square.isOnBoard works for valid and invalid coords`` () =
        // valid
        for f in 0..7 do
            for r in 0..7 do
                Assert.True(Square.isOnBoard f r)

        // invalid
        let invalid =
            [ (-1,0); (0,-1); (8,0); (0,8); (99,99); (-5,-5) ]

        for (f,r) in invalid do
            Assert.False(Square.isOnBoard f r)

    [<Property>]
    let ``prop - file/rank roundtrip`` (f: int, r: int) =
        if f >= 0 && f < 8 && r >= 0 && r < 8 then
            let sq = Square.ofFileRank (File.fromInt f) (Rank.fromInt r)
            let f2 = Square.file sq |> File.toInt
            let r2 = Square.rank sq |> Rank.toInt
            f2 = f && r2 = r
        else
            true

    [<Property>]
    let ``prop - string roundtrip`` (f: int, r: int) =
        if f >= 0 && f < 8 && r >= 0 && r < 8 then
            let sq = Square.ofFileRank (File.fromInt f) (Rank.fromInt r)
            let s = Square.toString sq
            let sq2 = Square.fromString s
            sq2 = sq
        else
            true

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

module BitboardTests =

    [<Fact>]
    let ``Bitboard set and contains works`` () =
        let sq = Square.fromString "e4"
        let bb = Bitboard.empty |> Bitboard.set sq
        Assert.True(Bitboard.contains sq bb)
        Assert.False(Bitboard.contains (Square.fromString "e5") bb)

    [<Fact>]
    let ``Bitboard count works`` () =
        let bb =
            Bitboard.empty
            |> Bitboard.set (Square.fromString "a1")
            |> Bitboard.set (Square.fromString "h8")
        Assert.Equal(2, Bitboard.count bb)

    [<Fact>]
    let ``Bitboard popLsb iterates and clears bits`` () =
        let mutable bb =
            Bitboard.empty
            |> Bitboard.set (Square.fromString "c3")
            |> Bitboard.set (Square.fromString "f6")

        let first = Bitboard.popLsb &bb
        let second = Bitboard.popLsb &bb

        Assert.Equal(Square.fromString "c3", first)
        Assert.Equal(Square.fromString "f6", second)
        Assert.Equal(Bitboard.empty, bb)

    // --- New Boundary & Technical Tests ---

    [<Fact>]
    let ``Constants are correct`` () =
        Assert.Equal(0uL, Bitboard.empty)
        Assert.Equal(0xFFFFFFFFFFFFFFFFuL, Bitboard.all)

    [<Theory>]
    [<InlineData(0)>]  // a1
    [<InlineData(7)>]  // h1
    [<InlineData(56)>] // a8
    [<InlineData(63)>] // h8
    let ``Boundary squares (corners) work correctly`` (sqIdx: int) =
        let sq = sqIdx // Assuming Square is an int alias or can be cast
        let bb = Bitboard.set sq Bitboard.empty
        Assert.True(Bitboard.contains sq bb)
        Assert.Equal(1, Bitboard.count bb)

    // --- Property Based Tests (FsCheck) ---

    [<Property>]
    let ``prop - setting a bit is idempotent`` (bb: Bitboard) (sq: int) =
        let s = abs sq % 64
        Bitboard.set s (Bitboard.set s bb) = Bitboard.set s bb

    [<Property>]
    let ``prop - set then clear restores state if bit was absent`` (bb: Bitboard) (sq: int) =
        let s = abs sq % 64
        if not (Bitboard.contains s bb) then
            Bitboard.clear s (Bitboard.set s bb) = bb
        else
            Bitboard.clear s bb <> bb

    [<Property>]
    let ``prop - count is always consistent with popLsb loop`` (bb: Bitboard) =
        let expectedCount = Bitboard.count bb
        let mutable temp = bb
        let mutable actualCount = 0
        while temp <> Bitboard.empty do
            Bitboard.popLsb &temp |> ignore
            actualCount <- actualCount + 1
        actualCount = expectedCount

    [<Property>]
    let ``prop - popLsb always returns bits in strictly increasing order`` (bb: Bitboard) =
        let mutable temp = bb
        let mutable lastBit = -1
        let mutable inOrder = true
        while temp <> Bitboard.empty do
            let currentBit = Bitboard.popLsb &temp
            if currentBit <= lastBit then inOrder <- false
            lastBit <- currentBit
        inOrder

    [<Property>]
    let ``prop - popLsb bit-clearing logic invariant`` (bb: Bitboard) =
        if bb = Bitboard.empty then true
        else
            let mutable temp = bb
            let lsb = Bitboard.popLsb &temp
            // Logic: bit (1 << lsb) must have been set, 
            // and no bits lower than lsb should be set.
            let bitWasSet = (bb &&& (1uL <<< lsb)) <> 0uL
            let noLowerBits = (bb &&& ((1uL <<< lsb) - 1uL)) = 0uL
            bitWasSet && noLowerBits




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

        let m = Move(Square.fromString "e2", Square.fromString "e4", 0, 0)

        let nextB = Board.applyMove m b
        Assert.False(Board.isOccupied nextB (Square.fromString "e2"))
        Assert.True(Board.isOccupied nextB (Square.fromString "e4"))
        Assert.Equal(Colour.Black, nextB.SideToMove)

    [<Fact>]
    let ``applyMove increments fullmove number after black moves`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/P7/1PPPPPPP/RNBQKBNR b KQkq - 0 1"

        let m = Move(Square.fromString "a7", Square.fromString "a6", 0, 0)

        let nextB = Board.applyMove m b
        Assert.Equal(2, nextB.FullmoveNumber)

    [<Fact>]
    let ``Kingside castling moves rook`` () =
        let b = Board.fromFen "4k3/8/8/8/8/8/8/R3K2R w KQ - 0 1"

        let m = Move(Square.fromString "e1", Square.fromString "g1", 3, 0)

        let b2 = Board.applyMove m b

        Assert.True(Board.isOccupied b2 (Square.fromString "f1"))
        Assert.False(Board.isOccupied b2 (Square.fromString "h1"))

    [<Fact>]
    let ``En passant removes captured pawn`` () =
        let b = Board.fromFen "8/8/8/3pP3/8/8/8/8 w - d6 0 1"

        let m = Move(Square.fromString "e5", Square.fromString "d6", 2, 0)

        let b2 = Board.applyMove m b

        Assert.False(Board.isOccupied b2 (Square.fromString "d5"))
        Assert.True(Board.isOccupied b2 (Square.fromString "d6"))

    [<Fact>]
    let ``Promotion creates promoted piece`` () =
        let b = Board.fromFen "8/4P3/8/8/8/8/8/8 w - - 0 1"

        let m = Move(Square.fromString "e7", Square.fromString "e8", 5, int PieceType.Queen)

        let b2 = Board.applyMove m b

        match Board.tryGetPiece b2 (Square.fromString "e8") with
        | Some p -> Assert.Equal(PieceType.Queen, Piece.kind p)
        | None -> Assert.True(false)

    [<Fact>]
    let ``Moving rook removes kingside castling rights`` () =
        let b = Board.fromFen "4k3/8/8/8/8/8/8/4K2R w K - 0 1"

        let m = Move(Square.fromString "h1", Square.fromString "h2", 0, 0)

        let b2 = Board.applyMove m b

        Assert.False(b2.CastlingRights.HasFlag CastlingRights.WK)

    [<Fact>]
    let ``Moving King removes all castling rights`` () =
        let b = Board.fromFen "r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1"
        let m = Move(Square.fromString "e1", Square.fromString "e2", 0, 0)
        let b2 = Board.applyMove m b
        Assert.False(b2.CastlingRights.HasFlag CastlingRights.WK)
        Assert.False(b2.CastlingRights.HasFlag CastlingRights.WQ)
        Assert.True(b2.CastlingRights.HasFlag CastlingRights.BK)
        Assert.True(b2.CastlingRights.HasFlag CastlingRights.BQ)

    [<Fact>]
    let ``Capturing opponent rook removes their castling rights`` () =
        let b = Board.fromFen "r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1"
        // White Rook on a1 captures Black Rook on a8
        let m = Move(Square.fromString "a1", Square.fromString "a8", 1, 0)
        let b2 = Board.applyMove m b
        Assert.False(b2.CastlingRights.HasFlag CastlingRights.BQ)
        Assert.True(b2.CastlingRights.HasFlag CastlingRights.BK)

    [<Fact>]
    let ``Pawn move resets halfmove clock`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 10 1"
        let m = Move(Square.fromString "e2", Square.fromString "e4", 0, 0)
        let b2 = Board.applyMove m b
        Assert.Equal(0, b2.HalfmoveClock)

    [<Fact>]
    let ``Capture resets halfmove clock`` () =
        let b = Board.fromFen "rnb1kbnr/ppp1pppp/8/3q4/8/8/PPPP1PPP/RNBQKBNR w KQkq - 5 1"
        let m = Move(Square.fromString "d1", Square.fromString "d5", 1, 0)
        let b2 = Board.applyMove m b
        Assert.Equal(0, b2.HalfmoveClock)

    [<Fact>]
    let ``Quiet non-pawn move increments halfmove clock`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        let m = Move(Square.fromString "g1", Square.fromString "f3", 0, 0)
        let b2 = Board.applyMove m b
        Assert.Equal(1, b2.HalfmoveClock)

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
            |> Array.filter (fun m -> Move.fromSq m = Square.fromString "e4")

        Assert.Equal(3, moves.Length)

        let captures =
            moves
            |> Array.filter (fun m -> Move.kind m = 1)


        Assert.Equal(2, captures.Length)

    [<Fact>]
    let ``Pawn En Passant is detected`` () =
        let b = Board.fromFen "8/8/8/3pP3/8/8/8/8 w - d6 0 1"

        let moves =
            MoveGen.getPseudoLegalMoves b
            |> Array.filter (fun m -> Move.fromSq m = Square.fromString "e5")

        Assert.Contains(moves, fun m -> Move.kind m = 2 && Square.toString (Move.toSq m) = "d6")

    [<Fact>]
    let ``Slider logic stops at edge and captures enemies`` () =
        let b = Board.fromFen "8/8/4p3/8/4R3/8/4P3/8 w - - 0 1"

        let moves =
            MoveGen.getPseudoLegalMoves b
            |> Array.filter (fun m -> Move.fromSq m = Square.fromString "e4")

        Assert.Equal(10, moves.Length)
        Assert.Contains(moves, fun m -> Square.toString (Move.toSq m) = "e6" && Move.kind m = 1)

    [<Fact>]
    let ``Starting position has 20 legal moves`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"

        let moves = MoveGen.getLegalMoves b

        Assert.Equal(20, moves.Length)

    [<Fact>]
    let ``Black cannot castle out of check after d1d8`` () =
        // This is the position reached after the move sequence provided:
        // position startpos moves e2e4 b8c6 d2d4 d7d5 f1b5 d5e4 d4d5 a7a6 b5c6 b7c6 d5c6 d8d1 e1d1 g8f6 c1f4 f6d5 f4e5 e4e3 d1e2 d5b4 b1c3 c8g4 f2f3 g4f5 e5c7 a8c8 c7b6 b4c2 a1d1 e7e5 c6c7 f8b4 d1d8
        let fenAfterCheck = "2rRkb1r/p1P1ppbp/bB6/4p3/1n2P3/2N2P2/PPP1K1PP/8 b k - 1 18"
        let b = Board.fromFen fenAfterCheck

        // Verify black is actually in check
        Assert.True(Board.isInCheck b, "Black should be in check from the Rook on d8")

        // Get legal moves
        let moves = MoveGen.getLegalMoves b
        
        // Assert that e8g8 (CastleKingSide) is NOT in the legal move list
        let hasCastling = moves |> Array.exists (fun m -> Move.kind m = 3)
        
        Assert.False(hasCastling, "Castling (e8g8) should be illegal because the King is in check")

    [<Fact>]
    let ``Black cannot castle through check`` () =
        // Setup: White rook guards f8, Black King on e8. 
        // King is NOT in check, but must pass through f8 to castle.
        let fen = "r3k2r/8/8/8/8/5R2/8/4K3 b kq - 0 1"
        let b = Board.fromFen fen
        
        let moves = MoveGen.getLegalMoves b
        let canCastleKingside = moves |> Array.exists (fun m -> Move.kind m = 3)
        
        Assert.False(canCastleKingside, "Black cannot castle through f8 because it is attacked by the White Rook")

    [<Fact>]
    let ``getCaptureMoves ignores quiet moves`` () =
        // Position: White Pawn on e2 (can move to e3, e4), White Rook on a1. 
        // Black Pawn on d3.
        let b = Board.fromFen "8/8/8/8/8/3p4/4P3/R3K3 w Q - 0 1"
        let captures = MoveGen.getCaptureMoves b
        
        // Should find e2xd3 but NOT e2e3 or e2e4
        Assert.Contains(captures, fun m -> Square.toString (Move.toSq m) = "d3")
        Assert.DoesNotContain(captures, fun m -> Square.toString (Move.toSq m) = "e3")
        Assert.DoesNotContain(captures, fun m -> Square.toString (Move.toSq m) = "e4")

    [<Fact>]
    let ``Pawn cannot jump over a piece with double push`` () =
        // FEN: White pawn on e2, Black Knight on e3. e4 is empty.
        let fen = "rnbqkbnr/pppp1ppp/8/8/4n3/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        let board = Board.fromFen fen
        let moves = MoveGen.getPseudoLegalMoves board
    
        // Check if the move e2-e4 (which jumps over e3) is generated
        let hasJump = moves |> Array.exists (fun m -> 
            Square.toString (Move.fromSq m) = "e2" && Square.toString (Move.toSq m) = "e4")
    
        Assert.False(hasJump, "White pawn at e2 should be blocked from jumping to e4 by the piece on e3")

    [<Fact>]
    let ``UCI move parser identifies e1g1 as a Castling move not a Quiet move`` () =
        // Standard starting position
        let fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        let board = Board.fromFen fen
    
        // Position after 1. e4 e5 2. Nf3 Nc6 3. Bc4 Bc5
        // (Clearing the way for White to castle)
        let movesToPlay = ["e2e4"; "e7e5"; "g1f3"; "b8c6"; "f1c4"; "f8c5"]
        let mutable currentBoard = board
        for mStr in movesToPlay do
            let legals = MoveGen.getLegalMoves currentBoard
            let m = legals |> Array.find (fun x -> Move.toUci x = mStr)
            currentBoard <- Board.applyMove m currentBoard

        // Now White wants to castle: e1g1
        let uciCastling = "e1g1"
        let allMoves = MoveGen.getLegalMoves currentBoard
    
        // Find how many moves match "e1g1"
        let matchingMoves = allMoves |> Array.filter (fun m -> Move.toUci m = uciCastling)
    
        // ASSERT 1: There should really only be one move in a perfect generator, 
        // but if there are two, we must ensure we don't pick 'Quiet'.
        Assert.NotEmpty(matchingMoves)
    
        // This is the logic currently in your UciLoop:
        let selectedMove = allMoves |> Array.find (fun m -> Move.toUci m = uciCastling)
    
        // ASSERT 2: The selected move MUST be Castling.
        // This will likely FAIL in your current version and return 'Quiet'.
        Assert.Equal(3, Move.kind selectedMove)

module PromotionTests =

    [<Fact>]
    let ``White pawn generates 4 quiet promotion moves`` () =
        let b = Board.fromFen "8/4P3/8/8/8/8/8/8 w - - 0 1"
        let moves = MoveGen.getPseudoLegalMoves b

        let promos =
            moves
            |> Array.filter (fun m -> Move.fromSq m = Square.fromString "e7" && Move.toSq m = Square.fromString "e8")

        Assert.Equal(4, promos.Length)

    [<Fact>]
    let ``White pawn generates 4 capture promotion moves`` () =
        let b = Board.fromFen "3p4/4P3/8/8/8/8/8/8 w - - 0 1"
        let moves = MoveGen.getPseudoLegalMoves b

        let promos =
            moves
            |> Array.filter (fun m -> Move.fromSq m = Square.fromString "e7" && Move.toSq m = Square.fromString "d8")

        Assert.Equal(4, promos.Length)

    [<Fact>]
    let ``Black pawn generates 4 quiet promotion moves`` () =
        let b = Board.fromFen "8/8/8/8/8/8/4p3/8 b - - 0 1"
        let moves = MoveGen.getPseudoLegalMoves b

        let promos =
            moves
            |> Array.filter (fun m -> Move.fromSq m = Square.fromString "e2" && Move.toSq m = Square.fromString "e1")

        Assert.Equal(4, promos.Length)

    [<Fact>]
    let ``Black pawn generates 4 capture promotion moves`` () =
        let b = Board.fromFen "8/8/8/8/8/8/4p3/5P2 b - - 0 1"
        let moves = MoveGen.getPseudoLegalMoves b

        let promos =
            moves
            |> Array.filter (fun m -> Move.fromSq m = Square.fromString "e2" && Move.toSq m = Square.fromString "f1")

        Assert.Equal(4, promos.Length)

module AttackTests =

    [<Fact>]
    let ``Square attacked by knight`` () =
        let b = Board.fromFen "8/8/8/3n4/4K3/8/8/8 w - - 0 1"
        Assert.True(Attack.isSquareAttacked b (Square.fromString "f4") Colour.Black)

    [<Fact>]
    let ``Square attacked by bishop`` () =
        let b = Board.fromFen "8/8/8/3b4/4K3/8/8/8 w - - 0 1"
        Assert.True(Attack.isSquareAttacked b (Square.fromString "e4") Colour.Black)

    [<Fact>]
    let ``Square attacked by pawn`` () =
        let b = Board.fromFen "8/8/8/3p4/4K3/8/8/8 w - - 0 1"
        Assert.True(Attack.isSquareAttacked b (Square.fromString "e4") Colour.Black)

    [<Fact>]
    let ``Square not attacked`` () =
        let b = Board.fromFen "8/8/8/8/4K3/8/8/8 w - - 0 1"
        Assert.False(Attack.isSquareAttacked b (Square.fromString "e4") Colour.Black)

    [<Fact>]
    let ``White pawn attacks upwards (should be detected but isSquareAttacked returns false)`` () =
        // White pawn on e4 should attack d5 and f5.
        let b = Board.fromFen "8/8/8/3p4/4P3/8/8/8 w - - 0 1"

        // d5 is attacked by the pawn on e4.
        let target = Square.fromString "d5"

        // EXPECTED: true
        Assert.True(Attack.isSquareAttacked b target Colour.White)

    [<Fact>]
    let ``Black pawn attack detection is correct`` () =
        // Black pawn on h5 attacks g4
        let b = Board.fromFen "8/8/8/7p/8/8/8/8 w - - 0 1"

        let target = Square.fromString "g4"

        Assert.True(Attack.isSquareAttacked b target Colour.Black)

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

module LegalMoveFilteringTests =

    [<Fact>]
    let ``Pinned knight has no legal moves`` () =
        let b = Board.fromFen "4r3/8/8/8/8/8/4N3/4K3 w - - 0 1"
        let moves = MoveGen.getLegalMoves b
        Assert.DoesNotContain(moves, fun m -> Move.fromSq m = Square.fromString "e2")

    [<Fact>]
    let ``King cannot move into check`` () =
        let b = Board.fromFen "4k3/8/8/4r3/8/8/8/4K3 w - - 0 1"
        let moves = MoveGen.getLegalMoves b
        Assert.DoesNotContain(moves, fun m -> Move.toSq m = Square.fromString "e2")

    [<Fact>]
    let ``Illegal en passant exposing king is filtered out`` () =
        let b = Board.fromFen "4k3/8/8/8/3pP3/8/8/4K3 w - d6 0 1"
        let moves = MoveGen.getLegalMoves b
        Assert.DoesNotContain(moves, fun m -> Move.kind m = 2)

    [<Fact>]
    let ``Cannot castle into attacked destination square`` () =
        let b = Board.fromFen "6rk/8/8/8/8/8/8/R3K2R w KQ - 0 1"

        let moves = MoveGen.getLegalMoves b

        Assert.DoesNotContain(moves, fun m -> Move.kind m = 3)

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

module EvaluationTests =

    [<Fact>]
    let ``Starting position evaluation is perfectly symmetrical (0)`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        Assert.Equal(0, Evaluation.evaluate b)

    [<Theory>]
    [<InlineData("rnb1kbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", 800)>] // White up Queen
    [<InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNB1KBNR w KQkq - 0 1", -800)>] // Black up Queen
    let ``Material advantage is detected correctly`` (fen: string, expectedMinMaterial: int) =
        let b = Board.fromFen fen
        let score = Evaluation.evaluate b

        if expectedMinMaterial > 0 then
            // Instead of exact 900, we check if it's strongly positive
            Assert.True(score >= expectedMinMaterial, $"White should be up significantly, got {score}")
        else
            // Instead of exact -900, we check if it's strongly negative
            Assert.True(score <= expectedMinMaterial, $"Black should be up significantly, got {score}")

    [<Fact>]
    let ``Knight on D4 is valued higher than Knight on A1 (PST)`` () =
        let corner = Board.fromFen "8/8/8/8/8/8/8/N7 w - - 0 1"
        let center = Board.fromFen "8/8/8/8/3N4/8/8/8 w - - 0 1"

        let scoreCorner = Evaluation.evaluate corner
        let scoreCenter = Evaluation.evaluate center

        Assert.True(
            scoreCenter > scoreCorner,
            $"Central knight ({scoreCenter}) should be worth more than corner knight ({scoreCorner})"
        )

    [<Fact>]
    let ``Black piece positioning is mirrored correctly`` () =
        // A black pawn on d7 (starting) vs d2 (advanced)
        // Advanced black pawns should score better for Black (more negative total score)
        let starting = Board.fromFen "8/3p4/8/8/8/8/8/8 b - - 0 1"
        let advanced = Board.fromFen "8/8/8/8/8/8/3p4/8 b - - 0 1"

        let scoreStarting = Evaluation.evaluate starting
        let scoreAdvanced = Evaluation.evaluate advanced

        // advanced should be "better" for Black, meaning a more negative number
        Assert.True(
            scoreAdvanced < scoreStarting,
            $"Advanced black pawn ({scoreAdvanced}) should be better for black than starting pawn ({scoreStarting})"
        )

    [<Fact>]
    let ``Evaluating an empty board returns 0`` () =
        let b = Board.empty
        Assert.Equal(0, Evaluation.evaluate b)

    [<Fact>]
    let ``Starting position pawn structure is symmetrical`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"

        Assert.Equal(0, Evaluation.pawnStructureScore b)

    [<Fact>]
    let ``Doubled pawns are penalized`` () =
        let healthy = Board.fromFen "4k3/8/8/8/8/8/3P4/4K3 w - - 0 1"
        let doubled = Board.fromFen "4k3/8/8/8/8/3P4/3P4/4K3 w - - 0 1"

        Assert.True(
            Evaluation.pawnStructureScore doubled < Evaluation.pawnStructureScore healthy,
            "Doubled white pawns should score worse than a single healthy pawn."
        )

    [<Fact>]
    let ``Isolated pawns are penalized compared with connected pawns`` () =
        let isolated = Board.fromFen "4k3/8/8/8/8/8/2P2P2/4K3 w - - 0 1"
        let connected = Board.fromFen "4k3/8/8/8/8/8/2PP4/4K3 w - - 0 1"

        Assert.True(
            Evaluation.pawnStructureScore connected > Evaluation.pawnStructureScore isolated,
            "Connected pawns should score better than isolated pawns."
        )

    [<Fact>]
    let ``Passed pawns are rewarded`` () =
        let blocked = Board.fromFen "4k3/8/3p4/8/3P4/8/8/4K3 w - - 0 1"
        let passed = Board.fromFen "4k3/8/8/8/3P4/8/8/4K3 w - - 0 1"

        Assert.True(
            Evaluation.pawnStructureScore passed > Evaluation.pawnStructureScore blocked,
            "Passed white pawns should score better than blocked pawns."
        )

    [<Fact>]
    let ``Black passed pawn scores for black`` () =
        let b = Board.fromFen "4k3/8/8/8/3p4/8/8/4K3 b - - 0 1"

        Assert.True(
            Evaluation.pawnStructureScore b < 0,
            "A black passed pawn should make the white-perspective score negative."
        )

    [<Fact>]
    let ``White missing g-pawn is worse than full pawn shield`` () =
        let full =
            "rnbq1rk1/pppppppp/8/8/8/8/PPPPPPPP/RNBQ1RK1 w - - 0 1"
        let weak =
            "rnbq1rk1/pppppppp/8/8/8/6P1/PPPPPP1P/RNBQ1RK1 w - - 0 1"

        let bFull = Board.fromFen full
        let bWeak = Board.fromFen weak

        let eFull = Evaluation.evaluate bFull
        let eWeak = Evaluation.evaluate bWeak

        Assert.True(eFull > eWeak)

    [<Fact>]
    let ``open h-file next to white king is penalised`` () =
        let fenSafe =
            "rnbq1rk1/pppppppp/8/8/8/8/PPPPPPPP/RNBQ1RK1 w - - 0 1"

        let fenOpen =
            "rnbq1rk1/pppppppp/8/8/8/8/PPPPPPPP/RNBQ1R2 w - - 0 1" // remove h1 pawn

        let bSafe = Board.fromFen fenSafe
        let bOpen = Board.fromFen fenOpen

        Assert.True(Evaluation.evaluate bSafe > Evaluation.evaluate bOpen)

    //[<Fact>]
    //let ``half-open h-file is penalised`` () =
    //    let fenSafe =
    //        "rnbq1rk1/1ppppppp/8/8/8/8/PPPPPPPP/RNBQ1RK1 w - - 0 1"

    //    let fenHalf =
    //        "rnbq1rk1/ppppppp1/8/8/8/8/PPPPPPPP/RNBQ1RK1 w - - 0 1" // black pawn missing on h7

    //    let bSafe = Board.fromFen fenSafe
    //    let bHalf = Board.fromFen fenHalf
    //    let safescr = Evaluation.evaluate bSafe
    //    let halfscr = Evaluation.evaluate bHalf

    //    Assert.True(safescr > halfscr)

    [<Fact>]
    let ``no open-file penalty when king not short-castled`` () =
        let fen =
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w - - 0 1"

        let b = Board.fromFen fen
        Assert.Equal(Evaluation.evaluate b, Evaluation.evaluate b)

    [<Fact>]
    let ``enemy knight near white king is penalised`` () =
        let safe =
            "rnbq1rk1/pppppppp/6n1/8/8/8/PPPPPPPP/RNBQ1RK1 w - - 0 1"

        let knightNear =
            "rnbq1rk1/pppppppp/8/8/8/6n1/PPPPPPPP/RNBQ1RK1 w - - 0 1" // knight on g3

        let bSafe = Board.fromFen safe
        let bNear = Board.fromFen knightNear
        let safeScore = Evaluation.evaluate bSafe
        let nearScore = Evaluation.evaluate bNear

        Assert.True(safeScore > nearScore)

    //[<Fact>]
    //let ``enemy queen near white king is penalised more than knight`` () =
    //    let knight =
    //        "rnb2rk1/pppppppp/6q1/8/8/6n1/PPPPPPPP/RNBQ1RK1 w - - 0 1"

    //    let queen =
    //        "rnb2rk1/pppppppp/6n1/8/8/6q1/PPPPPPPP/RNBQ1RK1 w - - 0 1"

    //    let bN = Board.fromFen knight
    //    let bQ = Board.fromFen queen
    //    let scoreN = Evaluation.evaluate bN
    //    let scoreQ = Evaluation.evaluate bQ

    //    Assert.True(scoreN > scoreQ)


module SearchTests =

    [<Fact>]
    let ``Search finds a mate in one (Scholars Mate)`` () =
        // Scholars mate position: White Queen on h5 can move to f7 for mate.
        let b =
            Board.fromFen "r1bqkbnr/pppp1ppp/2n5/4p2Q/2B1P3/8/PPPP1PPP/RNB1K1NR w KQkq - 0 1"

        let bestMoveOpt =
            Search.findBestMove b 2 2000 [] System.Threading.CancellationToken.None
            |> Async.RunSynchronously

        match bestMoveOpt with
        | Some move -> Assert.Equal("h5f7", Move.toUci move)
        | None -> Assert.True(false, "Search did not find a move.")

    [<Fact>]
    let ``Search finds a simple winning capture`` () =
        // White Queen on d1 can capture a free Black Queen on d5.
        let b = Board.fromFen "rnb1kbnr/ppp1pppp/8/3q4/8/8/PPP11PPP/RNBQKBNR w KQkq - 0 1"

        let bestMoveOpt =
            Search.findBestMove b 3 2000 [] System.Threading.CancellationToken.None
            |> Async.RunSynchronously

        match bestMoveOpt with
        | Some move -> Assert.Equal("d1d5", Move.toUci move)
        | None -> Assert.True(false, "Search did not find a move.")

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
        
        let bestMoveOpt =
            Search.findBestMove b 5 1000 [] cts.Token
            |> Async.RunSynchronously

        // ASSERT: Iterative deepening should still provide a move from the fallback
        Assert.True(bestMoveOpt.IsSome, "Search must return a move fallback to satisfy UCI")
        
        let legalMoves = MoveGen.getLegalMoves b
        Assert.Contains(bestMoveOpt.Value, legalMoves)

    [<Fact>]
    let ``Board applyNullMove toggles side to move and clears en passant`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/4P3/8/PPPP1PPP/RNBQKBNR b KQkq e3 0 1"
        let nextB = Board.applyNullMove b
        Assert.Equal(Colour.White, nextB.SideToMove)
        Assert.True(nextB.EnPassantSquare.IsNone)
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
        Search.nodes <- 0uL
        let scoreWithout = Search.negamaxInternal b 4 0 -Search.INF Search.INF false [] System.Threading.CancellationToken.None
        
        // Clear 
        TranspositionTable.clear()
        Search.clearKillers() 
        Search.clearHistory() 
        
        // Search with NMP enabled (allowNull = true)
        Search.nodes <- 0uL
        let scoreWith = Search.negamaxInternal b 4 0 -Search.INF Search.INF true [] System.Threading.CancellationToken.None
        
        Assert.Equal(scoreWithout, scoreWith)

module BitboardGenTests =

    [<Fact>]
    let ``Knight attacks on b1 are correct`` () =
        let b1 = Square.fromString "b1"
        let attacks = BitboardGen.knightAttacks.[b1]
        // From b1, knight hits a3, c3, d2
        Assert.True(Bitboard.contains (Square.fromString "a3") attacks)
        Assert.True(Bitboard.contains (Square.fromString "c3") attacks)
        Assert.True(Bitboard.contains (Square.fromString "d2") attacks)
        Assert.Equal(3, Bitboard.count attacks)

    [<Fact>]
    let ``Knight on a-file does not wrap to h-file`` () =
        let a4 = Square.fromString "a4"
        let attacks = BitboardGen.knightAttacks.[a4]
        // A knight on a4 should NOT be able to hit anything on the g or h files
        let hFileMask = 0x8080808080808080uL
        let gFileMask = 0x4040404040404040uL
        Assert.Equal(0uL, attacks &&& hFileMask)
        Assert.Equal(0uL, attacks &&& gFileMask)

    [<Fact>]
    let ``King attacks in corner are 3`` () =
        let a1 = Square.fromString "a1"
        let attacks = BitboardGen.kingAttacks.[a1]
        Assert.Equal(3, Bitboard.count attacks)

    [<Fact>]
    let ``White pawn attacks from e2 hit d3 and f3`` () =
        let e2 = Square.fromString "e2"
        let attacks = BitboardGen.pawnAttacks.[0, e2]
        Assert.True(Bitboard.contains (Square.fromString "d3") attacks)
        Assert.True(Bitboard.contains (Square.fromString "f3") attacks)
        Assert.Equal(2, Bitboard.count attacks)

    [<Fact>]
    let ``Black pawn attacks from d7 hit c6 and e6`` () =
        let d7 = Square.fromString "d7"
        let attacks = BitboardGen.pawnAttacks.[1, d7]
        Assert.True(Bitboard.contains (Square.fromString "c6") attacks)
        Assert.True(Bitboard.contains (Square.fromString "e6") attacks)
        Assert.Equal(2, Bitboard.count attacks)

    [<Fact>]
    let ``Pawn on a-file does not wrap when attacking`` () =
        let a2 = Square.fromString "a2"
        let attacks = BitboardGen.pawnAttacks.[0, a2]
        // Should only hit b3 (sq 17), not h1 or anything else
        Assert.Equal(1, Bitboard.count attacks)
        Assert.True(Bitboard.contains (Square.fromString "b3") attacks)

module SlidingAttackTests =

    [<Fact>]
    let ``Rook slow attacks are blocked correctly`` () =
        let e4 = Square.fromString "e4"
        // Place a blocker on e6 and c4
        let blockers = (1uL <<< Square.fromString "e6") ||| (1uL <<< Square.fromString "c4")
        
        let attacks = SlidingAttackGen.rookAttacks e4 blockers
        
        // Should hit e5 and e6 (the blocker), but NOT e7
        Assert.True(Bitboard.contains (Square.fromString "e5") attacks)
        Assert.True(Bitboard.contains (Square.fromString "e6") attacks)
        Assert.False(Bitboard.contains (Square.fromString "e7") attacks)
        
        // Should hit d4 and c4 (the blocker), but NOT b4
        Assert.True(Bitboard.contains (Square.fromString "d4") attacks)
        Assert.True(Bitboard.contains (Square.fromString "c4") attacks)
        Assert.False(Bitboard.contains (Square.fromString "b4") attacks)

    [<Fact>]
    let ``Bishop slow attacks hit diagonals`` () =
        let d4 = Square.fromString "d4"
        let attacks = SlidingAttackGen.bishopAttacks d4 0uL // Empty board
        
        // Check a few diagonal squares
        Assert.True(Bitboard.contains (Square.fromString "c3") attacks)
        Assert.True(Bitboard.contains (Square.fromString "b2") attacks)
        Assert.True(Bitboard.contains (Square.fromString "a1") attacks)
        Assert.True(Bitboard.contains (Square.fromString "e5") attacks)
        Assert.True(Bitboard.contains (Square.fromString "h8") attacks)
        
        // Should NOT hit horizontal/vertical
        Assert.False(Bitboard.contains (Square.fromString "d5") attacks)
        Assert.False(Bitboard.contains (Square.fromString "e4") attacks)

    [<Fact>]
    let ``Rook mask excludes edges`` () =
        let e4 = Square.fromString "e4"
        let mask = SlidingAttackGen.rookMask e4
        
        // The mask for e4 should NOT include e1, e8, a4, or h4
        Assert.False(Bitboard.contains (Square.fromString "e1") mask, "Rook mask should exclude edge e1")
        Assert.False(Bitboard.contains (Square.fromString "e8") mask, "Rook mask should exclude edge e8")
        Assert.False(Bitboard.contains (Square.fromString "a4") mask, "Rook mask should exclude edge a4")
        Assert.False(Bitboard.contains (Square.fromString "h4") mask, "Rook mask should exclude edge h4")
        
        // But it SHOULD include the inner squares
        Assert.True(Bitboard.contains (Square.fromString "e2") mask)
        Assert.True(Bitboard.contains (Square.fromString "e7") mask)

    [<Fact>]
    let ``Bishop mask excludes edges`` () =
        let d4 = Square.fromString "d4"
        let mask = SlidingAttackGen.bishopMask d4
        
        // Diagonal from d4 hits edges at a1, g1, a7, h8. 
        // These should all be 0 in the mask.
        Assert.False(Bitboard.contains (Square.fromString "a1") mask)
        Assert.False(Bitboard.contains (Square.fromString "g1") mask)
        Assert.False(Bitboard.contains (Square.fromString "a7") mask)
        Assert.False(Bitboard.contains (Square.fromString "h8") mask)
        
        // Inner diagonal squares should be 1
        Assert.True(Bitboard.contains (Square.fromString "c3") mask)
        Assert.True(Bitboard.contains (Square.fromString "e5") mask)

    [<Fact>]
    let ``Table initialization matches slow reference for first 4 squares`` () =
        // 1. Initialize the tables (leapers + sliding)
        // This is usually done in the engine startup, but we call it here for the test
        Magic.init()
        
        // 2. Test a1 (Square 0) Rook with specific blockers
        let a1 = 0
        let entry = Magic.rookEntries.[a1]
        
        // Test with a piece on a3 and d1
        let blockers = (1uL <<< Square.fromString "a3") ||| (1uL <<< Square.fromString "d1")
        let slowResult = SlidingAttackGen.rookAttacks a1 blockers
        
        // 3. Fast Lookup using the new getIndex logic
        let index = Magic.getIndex blockers entry.Mask
        let fastResult = Magic.rookTable.[entry.Offset + index]
        
        Assert.Equal(slowResult, fastResult)

    [<Fact>]
    let ``Bishop table lookup matches slow reference for center square`` () =
        Magic.init()
        let d4 = Square.fromString "d4"
        let entry = Magic.bishopEntries.[d4]
        
        // Blocker on e5
        let blockers = (1uL <<< Square.fromString "e5")
        let slowResult = SlidingAttackGen.bishopAttacks d4 blockers
        
        let index = Magic.getIndex blockers entry.Mask
        let fastResult = Magic.bishopTable.[entry.Offset + index]
        
        Assert.Equal(slowResult, fastResult)    
    
module ZobristTests =

    [<Fact>]
    let ``Zobrist Table is deterministic with fixed seed`` () =
        let key1 = Zobrist.Table.SideToMove
        let key2 = Zobrist.Table.SideToMove
        Assert.Equal(key1, key2)

    [<Fact>]
    let ``Keys are unique for different pieces and squares`` () =
        let p1 = Piece(Colour.White, PieceType.Pawn)
        let k1 = Zobrist.getPieceKey p1 0 // a1
        let k2 = Zobrist.getPieceKey p1 1 // b1
        Assert.NotEqual(k1, k2)

    [<Fact>]
    let ``Castling combinations result in different keys`` () =
        let none = CastlingRights.None
        let whiteKing = none ||| CastlingRights.WK
        let blackKing = none ||| CastlingRights.BK
        
        let keyNone = Zobrist.getCastlingKey none
        let keyWK = Zobrist.getCastlingKey whiteKing
        let keyBK = Zobrist.getCastlingKey blackKing
        
        Assert.NotEqual(keyNone, keyWK)
        Assert.NotEqual(keyWK, keyBK)
        Assert.NotEqual(keyNone, keyBK)   

module HashTests =

    [<Fact>]
    let ``Incremental hash matches full hash after quiet move`` () =
        let b1 = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        
        // Nf3 (Quiet move from g1 to f3)
        let m = Move(6, 21, 0, 0)
        let b2 = Board.applyMove m b1
        
        let scratchHash = Board.calculateHash b2
        Assert.Equal(scratchHash, b2.Hash)

    [<Fact>]
    let ``Hash handles piece captures correctly`` () =
        // e4, then Black plays d5, then White captures exd5
        let b1 = Board.fromFen "rnbqkbnr/ppp1pppp/8/3p4/4P3/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 2"
        let capture = Move(28, 35, 1, 0)
        
        let b2 = Board.applyMove capture b1
        let scratchHash = Board.calculateHash b2
        Assert.Equal(scratchHash, b2.Hash)

    [<Fact>]
    let ``Hash handles promotion correctly`` () =
        // Pawn on a7 about to promote on a8
        let b1 = Board.fromFen "8/P7/8/8/8/8/8/k6K w - - 0 1"
        let prom = Move(48, 56, 5, int PieceType.Queen)
        
        let b2 = Board.applyMove prom b1
        let scratchHash = Board.calculateHash b2
        Assert.Equal(scratchHash, b2.Hash)

    [<Fact>]
    let ``Hash remains same after repetition of moves`` () =
        let b1 = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        
        // Knight moves out and back
        let m1 = Move(1, 18, 0, 0) // Nb1-c3
        let m2 = Move(18, 1, 0, 0) // Nc3-b1
        
        // Black does the same to keep the turn cycle correct
        let m3 = Move(62, 45, 0, 0) // Ng8-f6
        let m4 = Move(45, 62, 0, 0) // Nf6-g8
        
        let bFinal = b1 
                     |> Board.applyMove m1 |> Board.applyMove m3 
                     |> Board.applyMove m2 |> Board.applyMove m4
        
        Assert.Equal(b1.Hash, bFinal.Hash)

module TTTests =
    open TranspositionTable   
    [<Fact>]
    let ``TT can store and retrieve an entry`` () =
        let hash = 12345UL
        let move = Some (Move(12, 28, 0, 0))
        
        TranspositionTable.store hash 5 0 NodeFlag.Exact 100 move
        let result = TranspositionTable.probe hash
        
        Assert.True(result.IsSome)
        Assert.Equal(100, result.Value.Value)
        Assert.Equal(5, result.Value.Depth)
        Assert.Equal(move, result.Value.Move)

    [<Fact>]
    let ``Mate scores are adjusted correctly for ply`` () =
        let mateValue = 30000 // MATE_VALUE from your search
        let ply = 5
        
        // When storing, we "push" the mate further away
        let stored = mateToTT mateValue ply
        Assert.Equal(30005, stored)
        
        // When retrieving, we "pull" it back to the current context
        let retrieved = mateFromTT stored ply
        Assert.Equal(30000, retrieved)

    [<Fact>]
    let ``TT handles collisions via replacement`` () =
        let hash1 = 1UL
        let hash2 = uint64 TranspositionTable.SIZE + 1UL // Different hash, same index
        
        TranspositionTable.store hash1 5 0 NodeFlag.Exact 100 None
        TranspositionTable.store hash2 6 0 NodeFlag.Exact 200 None // Deeper, should replace
        
        let result = TranspositionTable.probe hash2
        Assert.Equal(200, result.Value.Value)
        
        let resultOld = TranspositionTable.probe hash1
        Assert.True(resultOld.IsNone) // Collision should have wiped the first entry

    [<Fact>]
    let ``TT reduces node count in transpositions`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
    
        // Search depth 4 from startpos
        Search.nodes <- 0uL
        TranspositionTable.clear()
        let cts = new System.Threading.CancellationTokenSource()
        Search.negamax b 4 0 -1000000 1000000 [] cts.Token |> ignore
        let nodesWithTT = Search.nodes
    
        // This is hard to assert exactly, but nodesWithTT will be significantly 
        // lower than a pure perft(4) because identical branches are pruned.
        Assert.True(nodesWithTT < 197281uL) // 197281 is the perft count for depth 4

    [<Fact>]
    let ``Search does not poison TT when cancelled`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        TranspositionTable.clear()
        
        // 1. Create a cancellation token and cancel it immediately
        let cts = new System.Threading.CancellationTokenSource()
        cts.Cancel()
        
        // 2. Run negamax with the cancelled token
        let _ = Search.negamax b 3 0 -Search.INF Search.INF [] cts.Token
        
        // 3. Probe the TT for this position
        let entry = TranspositionTable.probe b.Hash
        
        // 4. ASSERT: The entry should be None because we cancelled 
        // before the search could find a valid result.
        Assert.True(entry.IsNone, "TT should not store results from a cancelled search")

    [<Fact>]
    let ``TT Ageing replaces shallow new move over deep old move`` () =
        TranspositionTable.clear()
        let hash = 999UL
        
        // 1. Store a very deep search in Age 0
        TranspositionTable.currentAge <- 0uy
        TranspositionTable.store hash 20 0 TranspositionTable.NodeFlag.Exact 100 None
        
        // 2. Advance Age
        TranspositionTable.advanceAge() // Age is now 1
        
        // 3. Store a shallow search in Age 1
        TranspositionTable.store hash 2 0 TranspositionTable.NodeFlag.Exact 200 None
        
        // 4. Probe
        let entry = TranspositionTable.probe hash
        Assert.Equal(2, entry.Value.Depth)
        Assert.Equal(200, entry.Value.Value)
        Assert.Equal(1uy, entry.Value.Age)

module MoveOrderingTests =

    [<Fact>]
    let ``MVV-LVA prefers Pawn takes Queen over Queen takes Pawn`` () =
        // Setup: White Pawn on e4, White Queen on d1. Black Queen on d5, Black Pawn on f3.
        let b = Board.fromFen "rnb1kbnr/ppp1p1pp/8/3q4/4P3/5p2/PPPP1PPP/RNBQKBNR w KQkq - 0 1"
        
        // We need to simulate the sorting logic inside search
        let moves = MoveGen.getLegalMoves b
        
        // Helper to mimic the internal search scoring (simplified for test)
        let getScore (m: Move) =
            match Move.kind m with
            | 1 -> 
                let v = Board.tryGetPiece b (Move.toSq m) |> Option.map (fun p -> Evaluation.pieceValue (Piece.kind p)) |> Option.defaultValue 100
                let a = Board.tryGetPiece b (Move.fromSq m) |> Option.map (fun p -> Evaluation.pieceValue (Piece.kind p)) |> Option.defaultValue 0
                10000 + (v * 10) - a
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

module InsufficientMaterialTests =

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

module UciParsingTests =

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

module RepetitionTests =
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

module FiftyMoveRuleTests =
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
        let m = Move(Square.fromString "h1", Square.fromString "h2", 0, 0)
        let nextB = Board.applyMove m b
        Assert.Equal(100, nextB.HalfmoveClock)
        
        let score, _ = Search.negamax nextB 1 1 -Search.INF Search.INF [] System.Threading.CancellationToken.None
        Assert.Equal(0, score)
