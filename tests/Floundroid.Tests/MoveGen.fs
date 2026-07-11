namespace Floundroid.Tests

open Xunit
open Floundroid
open System
open System.Runtime.CompilerServices
open Microsoft.FSharp.NativeInterop

#nowarn "9" // For NativePtr usage

module MoveGenTests =

    [<Fact>]
    let ``Starting position has 20 pseudo-legal moves`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        let movePtr = NativePtr.stackalloc<int> 256
        let moveSpan = Span<int>(NativePtr.toVoidPtr movePtr, 256)
        let moveCount = MoveGen.getPseudoLegalMoves b moveSpan
        let moves = moveSpan.Slice(0, moveCount).ToArray()
        Assert.Equal(20, moves.Length)

    [<Fact>]
    let ``Knight in center has 8 moves`` () =
        let b = Board.fromFen "8/8/8/8/4N3/8/8/8 w - - 0 1"
        let movePtr = NativePtr.stackalloc<int> 256
        let moveSpan = Span<int>(NativePtr.toVoidPtr movePtr, 256)
        let moveCount = MoveGen.getPseudoLegalMoves b moveSpan
        let moves = moveSpan.Slice(0, moveCount).ToArray()
        Assert.Equal(8, moves.Length)

    [<Fact>]
    let ``Pawn captures correctly`` () =
        let b = Board.fromFen "8/8/8/3p1p2/4P3/8/8/8 w - - 0 1"

        let movePtr = NativePtr.stackalloc<int> 256
        let moveSpan = Span<int>(NativePtr.toVoidPtr movePtr, 256)
        let moveCount = MoveGen.getPseudoLegalMoves b moveSpan
        let moves = moveSpan.Slice(0, moveCount).ToArray()

        Assert.Equal(3, moves.Length)

        let captures =
            moves
            |> Array.filter (fun m -> Move.kind m = 1)


        Assert.Equal(2, captures.Length)

    [<Fact>]
    let ``Pawn En Passant is detected`` () =
        let b = Board.fromFen "8/8/8/3pP3/8/8/8/8 w - d6 0 1"

        let movePtr = NativePtr.stackalloc<int> 256
        let moveSpan = Span<int>(NativePtr.toVoidPtr movePtr, 256)
        let moveCount = MoveGen.getPseudoLegalMoves b moveSpan
        let moves = moveSpan.Slice(0, moveCount).ToArray()

        Assert.Contains(moves, fun m -> Move.kind m = 2 && Square.toString (Move.toSq m) = "d6")

    [<Fact>]
    let ``Slider logic stops at edge and captures enemies`` () =
        let b = Board.fromFen "8/8/4p3/8/4R3/8/4P3/8 w - - 0 1"
        let movePtr = NativePtr.stackalloc<int> 256
        let moveSpan = Span<int>(NativePtr.toVoidPtr movePtr, 256)
        let moveCount = MoveGen.getPseudoLegalMoves b moveSpan
        let moves = 
            moveSpan.Slice(0, moveCount).ToArray()
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
        let movePtr = NativePtr.stackalloc<int> 256
        let moveSpan = Span<int>(NativePtr.toVoidPtr movePtr, 256)
        let moveCount = MoveGen.getCaptureMoves b moveSpan
        
        let captures = moveSpan.Slice(0, moveCount).ToArray()
        
        // Should find e2xd3 but NOT e2e3 or e2e4
        Assert.Contains(captures, fun m -> Square.toString (Move.toSq m) = "d3")
        Assert.DoesNotContain(captures, fun m -> Square.toString (Move.toSq m) = "e3")
        Assert.DoesNotContain(captures, fun m -> Square.toString (Move.toSq m) = "e4")

    [<Fact>]
    let ``Pawn cannot jump over a piece with double push`` () =
        // FEN: White pawn on e2, Black Knight on e3. e4 is empty.
        let fen = "rnbqkbnr/pppp1ppp/8/8/4n3/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        let board = Board.fromFen fen
        let movePtr = NativePtr.stackalloc<int> 256
        let moveSpan = Span<int>(NativePtr.toVoidPtr movePtr, 256)
        let moveCount = MoveGen.getPseudoLegalMoves board moveSpan
        let moves = moveSpan.Slice(0, moveCount).ToArray()

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

    [<Fact>]
    let ``White pawn generates 4 quiet promotion moves`` () =
        let b = Board.fromFen "8/4P3/8/8/8/8/8/8 w - - 0 1"
        let movePtr = NativePtr.stackalloc<int> 256
        let moveSpan = Span<int>(NativePtr.toVoidPtr movePtr, 256)
        let moveCount = MoveGen.getPseudoLegalMoves b moveSpan
        let moves = moveSpan.Slice(0, moveCount).ToArray()

        let promos =
            moves
            |> Array.filter (fun m -> Move.fromSq m = Square.fromString "e7" && Move.toSq m = Square.fromString "e8")

        Assert.Equal(4, promos.Length)

    [<Fact>]
    let ``White pawn generates 4 capture promotion moves`` () =
        let b = Board.fromFen "3p4/4P3/8/8/8/8/8/8 w - - 0 1"
        let movePtr = NativePtr.stackalloc<int> 256
        let moveSpan = Span<int>(NativePtr.toVoidPtr movePtr, 256)
        let moveCount = MoveGen.getPseudoLegalMoves b moveSpan
        let moves = moveSpan.Slice(0, moveCount).ToArray()

        let promos =
            moves
            |> Array.filter (fun m -> Move.fromSq m = Square.fromString "e7" && Move.toSq m = Square.fromString "d8")

        Assert.Equal(4, promos.Length)

    [<Fact>]
    let ``Black pawn generates 4 quiet promotion moves`` () =
        let b = Board.fromFen "8/8/8/8/8/8/4p3/8 b - - 0 1"
        let movePtr = NativePtr.stackalloc<int> 256
        let moveSpan = Span<int>(NativePtr.toVoidPtr movePtr, 256)
        let moveCount = MoveGen.getPseudoLegalMoves b moveSpan
        let moves = moveSpan.Slice(0, moveCount).ToArray()

        let promos =
            moves
            |> Array.filter (fun m -> Move.fromSq m = Square.fromString "e2" && Move.toSq m = Square.fromString "e1")

        Assert.Equal(4, promos.Length)

    [<Fact>]
    let ``Black pawn generates 4 capture promotion moves`` () =
        let b = Board.fromFen "8/8/8/8/8/8/4p3/5P2 b - - 0 1"
        let movePtr = NativePtr.stackalloc<int> 256
        let moveSpan = Span<int>(NativePtr.toVoidPtr movePtr, 256)
        let moveCount = MoveGen.getPseudoLegalMoves b moveSpan
        let moves = moveSpan.Slice(0, moveCount).ToArray()

        let promos =
            moves
            |> Array.filter (fun m -> Move.fromSq m = Square.fromString "e2" && Move.toSq m = Square.fromString "f1")

        Assert.Equal(4, promos.Length)

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

    [<Fact>]
    let ``All legal moves are a subset of pseudo-legal moves`` () =
        let fens = 
            [ "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
              "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1"
              "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1" ]

        for fen in fens do
            let b = Board.fromFen fen
            let movePtr = NativePtr.stackalloc<int> 256
            let moveSpan = Span<int>(NativePtr.toVoidPtr movePtr, 256)
            let moveCount = MoveGen.getPseudoLegalMoves b moveSpan
            let pseudo = Set.ofArray (moveSpan.Slice(0, moveCount).ToArray())
            let legal = MoveGen.getLegalMoves b |> Set.ofArray
            Assert.True(Set.isSubset legal pseudo, $"Legal not subset of pseudo for FEN: {fen}")

    [<Fact>]
    let ``Checkmated side has no legal moves`` () =
        // Fool's mate
        let fen = "rnb1kbnr/pppp1ppp/8/4p3/6Pq/5P2/PPPPP2P/RNBQKBNR w KQkq - 0 3"
        let b = Board.fromFen fen
        Assert.True(Board.isInCheck b)
        let moves = MoveGen.getLegalMoves b
        Assert.Empty(moves)

    [<Fact>]
    let ``Stalemated side has no legal moves and is not in check`` () =
        let fen = "7k/5Q2/6K1/8/8/8/8/8 b - - 0 1"
        let b = Board.fromFen fen
        Assert.False(Board.isInCheck b)
        let moves = MoveGen.getLegalMoves b
        Assert.Empty(moves)

    [<Fact>]
    let ``getCaptureMoves only returns legal captures`` () =
        let fen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1"
        let b = Board.fromFen fen
        let legal = MoveGen.getLegalMoves b |> Set.ofArray
        let movePtr = NativePtr.stackalloc<int> 256
        let moveSpan = Span<int>(NativePtr.toVoidPtr movePtr, 256)
        let moveCount = MoveGen.getCaptureMoves b moveSpan
        
        let caps = moveSpan.Slice(0, moveCount).ToArray()

        // All capture moves must be legal
        for m in caps do
            Assert.True(Set.contains m legal, $"Illegal capture in QS: {Move.toUci m}")
            Assert.True(Move.kind m = 1 || Move.kind m = 2 || Move.kind m = 5)

    [<Fact>]
    let ``Starting position perft(1) and perft(2) via getLegalMoves`` () =
        let b0 = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"

        let moves1 = MoveGen.getLegalMoves b0
        Assert.Equal(20, moves1.Length)

        let mutable total2 = 0
        for m in moves1 do
            let b1 = Board.applyMove m b0
            total2 <- total2 + (MoveGen.getLegalMoves b1).Length

        Assert.Equal(400, total2)
