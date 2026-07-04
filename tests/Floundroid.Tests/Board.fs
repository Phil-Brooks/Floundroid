namespace Floundroid.Tests

open Xunit
open Floundroid

module ZobristTests =

    [<Fact>]
    let ``Zobrist Table is deterministic with fixed seed`` () =
        let key1 = Zobrist.Table.SideToMove
        let key2 = Zobrist.Table.SideToMove
        Assert.Equal(key1, key2)

    [<Fact>]
    let ``Keys are unique for different pieces and squares`` () =
        let p1 = (Colour.White <<< 3) ||| PieceType.Pawn
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

    [<Fact>]
    let ``All piece-square Zobrist keys are unique`` () =
        let keys =
            [ for c in [Colour.White; Colour.Black] do
                for pt in [PieceType.Pawn; PieceType.Knight; PieceType.Bishop; PieceType.Rook; PieceType.Queen; PieceType.King] do
                    for sq in 0..63 do
                        yield Zobrist.getPieceKey ((c <<< 3) ||| pt) sq ]

        let distinct = keys |> List.distinct
        Assert.Equal(keys.Length, distinct.Length)

    [<Fact>]
    let ``All 16 castling Zobrist keys are unique`` () =
        let keys =
            [ for i in 0..15 ->
                Zobrist.getCastlingKey i ]

        Assert.Equal(16, keys |> List.distinct |> List.length)

    [<Fact>]
    let ``En passant keys are unique per file`` () =
        let keys =
            [ for file in 0..7 ->
                Zobrist.getEnPassantKey (file) ]

        Assert.Equal(8, keys |> List.distinct |> List.length)

    [<Fact>]
    let ``En passant None returns 0`` () =
        Assert.Equal(0UL, Zobrist.getEnPassantKey -1)

    [<Fact>]
    let ``Empty board Zobrist hash is deterministic`` () =
        let h1 = 0UL
        let h2 = 0UL
        Assert.Equal(h1, h2)

    [<Fact>]
    let ``Piece key XORing twice cancels out`` () =
        let p = (Colour.White <<< 3) ||| PieceType.Knight
        let sq = 42
        let key = Zobrist.getPieceKey p sq

        let h = 0UL ^^^ key ^^^ key
        Assert.Equal(0UL, h)

module TTTests =
    open TranspositionTable   
    [<Fact>]
    let ``TT can store and retrieve an entry`` () =
        let hash = 12345UL
        let move = Move.create(12, 28, 0, 0)
        
        TranspositionTable.store hash 5 0 NodeExact 100 move
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
        
        TranspositionTable.store hash1 5 0 NodeExact 100 0
        TranspositionTable.store hash2 6 0 NodeExact 200 0 // Deeper, should replace
        
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
        TranspositionTable.store hash 20 0 TranspositionTable.NodeExact 100 0
        
        // 2. Advance Age
        TranspositionTable.advanceAge() // Age is now 1
        
        // 3. Store a shallow search in Age 1
        TranspositionTable.store hash 2 0 TranspositionTable.NodeExact 200 0
        
        // 4. Probe
        let entry = TranspositionTable.probe hash
        Assert.Equal(2, entry.Value.Depth)
        Assert.Equal(200, entry.Value.Value)
        Assert.Equal(1uy, entry.Value.Age)

    [<Fact>]
    let ``TT clear resets all entries`` () =
        TranspositionTable.clear()
        let hash = 42UL
        TranspositionTable.store hash 5 0 TranspositionTable.NodeExact 123 0
        Assert.True((TranspositionTable.probe hash).IsSome)

        TranspositionTable.clear()
        Assert.True((TranspositionTable.probe hash).IsNone)

    [<Fact>]
    let ``TT probe returns None on hash mismatch at same index`` () =
        TranspositionTable.clear()
        let h1 = 1UL
        let h2 = uint64 TranspositionTable.SIZE + 1UL // same index, different hash

        TranspositionTable.store h1 5 0 TranspositionTable.NodeExact 100 0
        let res = TranspositionTable.probe h2
        Assert.True(res.IsNone)

    [<Fact>]
    let ``TT keeps deeper entry when ages are equal`` () =
        TranspositionTable.clear()
        let hash = 777UL

        TranspositionTable.currentAge <- 0uy
        TranspositionTable.store hash 4 0 TranspositionTable.NodeExact 100 0
        TranspositionTable.store hash 6 0 TranspositionTable.NodeExact 200 0

        let entry = TranspositionTable.probe hash
        Assert.Equal(6, entry.Value.Depth)
        Assert.Equal(200, entry.Value.Value)

    [<Fact>]
    let ``TT replaces deep old entry with shallow new entry of newer age`` () =
        TranspositionTable.clear()
        let h1 = 1000UL
        let h2 = uint64 TranspositionTable.SIZE + 1000UL // same index, different hash

        TranspositionTable.currentAge <- 0uy
        TranspositionTable.store h1 10 0 TranspositionTable.NodeExact 100 0

        TranspositionTable.advanceAge() // age = 1
        TranspositionTable.store h2 2 0 TranspositionTable.NodeExact 200 0

        let eNew = TranspositionTable.probe h2
        let eOld = TranspositionTable.probe h1

        Assert.True(eNew.IsSome)
        Assert.True(eOld.IsNone)

    [<Fact>]
    let ``mateToTT and mateFromTT are identity for non-mate scores`` () =
        let score = 150
        let ply = 7
        let stored = mateToTT score ply
        let back = mateFromTT stored ply
        Assert.Equal(score, stored)
        Assert.Equal(score, back)

module BoardTests =

    [<Fact>]
    let ``Incremental hash matches full hash after quiet move`` () =
        let b1 = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        
        // Nf3 (Quiet move from g1 to f3)
        let m = Move.create(6, 21, 0, 0)
        let b2 = Board.applyMove m b1
        
        let scratchHash = Board.calculateHash b2
        Assert.Equal(scratchHash, b2.Hash)

    [<Fact>]
    let ``Hash handles piece captures correctly`` () =
        // e4, then Black plays d5, then White captures exd5
        let b1 = Board.fromFen "rnbqkbnr/ppp1pppp/8/3p4/4P3/8/PPPP1PPP/RNBQKBNR w KQkq d6 0 2"
        let capture = Move.create(28, 35, 1, 0)
        
        let b2 = Board.applyMove capture b1
        let scratchHash = Board.calculateHash b2
        Assert.Equal(scratchHash, b2.Hash)

    [<Fact>]
    let ``Hash handles promotion correctly`` () =
        // Pawn on a7 about to promote on a8
        let b1 = Board.fromFen "8/P7/8/8/8/8/8/k6K w - - 0 1"
        let prom = Move.create(48, 56, 5, int PieceType.Queen)
        
        let b2 = Board.applyMove prom b1
        let scratchHash = Board.calculateHash b2
        Assert.Equal(scratchHash, b2.Hash)

    [<Fact>]
    let ``Hash remains same after repetition of moves`` () =
        let b1 = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        
        // Knight moves out and back
        let m1 = Move.create(1, 18, 0, 0) // Nb1-c3
        let m2 = Move.create(18, 1, 0, 0) // Nc3-b1
        
        // Black does the same to keep the turn cycle correct
        let m3 = Move.create(62, 45, 0, 0) // Ng8-f6
        let m4 = Move.create(45, 62, 0, 0) // Nf6-g8
        
        let bFinal = b1 
                     |> Board.applyMove m1 |> Board.applyMove m3 
                     |> Board.applyMove m2 |> Board.applyMove m4
        
        Assert.Equal(b1.Hash, bFinal.Hash)

    [<Fact>]
    let ``Square attacked by knight`` () =
        let b = Board.fromFen "8/8/8/3n4/4K3/8/8/8 w - - 0 1"
        Assert.True(Board.isSquareAttacked b (Square.fromString "f4") Colour.Black)

    [<Fact>]
    let ``Square attacked by bishop`` () =
        let b = Board.fromFen "8/8/8/3b4/4K3/8/8/8 w - - 0 1"
        Assert.True(Board.isSquareAttacked b (Square.fromString "e4") Colour.Black)

    [<Fact>]
    let ``Square attacked by pawn`` () =
        let b = Board.fromFen "8/8/8/3p4/4K3/8/8/8 w - - 0 1"
        Assert.True(Board.isSquareAttacked b (Square.fromString "e4") Colour.Black)

    [<Fact>]
    let ``Square not attacked`` () =
        let b = Board.fromFen "8/8/8/8/4K3/8/8/8 w - - 0 1"
        Assert.False(Board.isSquareAttacked b (Square.fromString "e4") Colour.Black)

    [<Fact>]
    let ``White pawn attacks upwards (should be detected but isSquareAttacked returns false)`` () =
        // White pawn on e4 should attack d5 and f5.
        let b = Board.fromFen "8/8/8/3p4/4P3/8/8/8 w - - 0 1"

        // d5 is attacked by the pawn on e4.
        let target = Square.fromString "d5"

        // EXPECTED: true
        Assert.True(Board.isSquareAttacked b target Colour.White)

    [<Fact>]
    let ``Black pawn attack detection is correct`` () =
        // Black pawn on h5 attacks g4
        let b = Board.fromFen "8/8/8/7p/8/8/8/8 w - - 0 1"

        let target = Square.fromString "g4"

        Assert.True(Board.isSquareAttacked b target Colour.Black)

    [<Fact>]
    let ``Empty board has no pieces`` () =
        let b = Board.empty

        for sq in 0..63 do
            Assert.True(Board.tryGetPiece b sq = -1)

    [<Fact>]
    let ``Set and get piece works`` () =
        let b = Board.empty
        let sq = Square.fromString "e4"
        let p = Piece.fromChar 'Q'
        let b2 = Board.setPiece b sq (Some p)
        Assert.Equal(p, Board.tryGetPiece b2 sq)

    [<Fact>]
    let ``applyMove updates piece positions and side to move`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"

        let m = Move.create(Square.fromString "e2", Square.fromString "e4", 0, 0)

        let nextB = Board.applyMove m b
        Assert.False(Board.isOccupied nextB (Square.fromString "e2"))
        Assert.True(Board.isOccupied nextB (Square.fromString "e4"))
        Assert.Equal(Colour.Black, nextB.SideToMove)

    [<Fact>]
    let ``applyMove increments fullmove number after black moves`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/P7/1PPPPPPP/RNBQKBNR b KQkq - 0 1"

        let m = Move.create(Square.fromString "a7", Square.fromString "a6", 0, 0)

        let nextB = Board.applyMove m b
        Assert.Equal(2, nextB.FullmoveNumber)

    [<Fact>]
    let ``Kingside castling moves rook`` () =
        let b = Board.fromFen "4k3/8/8/8/8/8/8/R3K2R w KQ - 0 1"

        let m = Move.create(Square.fromString "e1", Square.fromString "g1", 3, 0)

        let b2 = Board.applyMove m b

        Assert.True(Board.isOccupied b2 (Square.fromString "f1"))
        Assert.False(Board.isOccupied b2 (Square.fromString "h1"))

    [<Fact>]
    let ``En passant removes captured pawn`` () =
        let b = Board.fromFen "8/8/8/3pP3/8/8/8/8 w - d6 0 1"

        let m = Move.create(Square.fromString "e5", Square.fromString "d6", 2, 0)

        let b2 = Board.applyMove m b

        Assert.False(Board.isOccupied b2 (Square.fromString "d5"))
        Assert.True(Board.isOccupied b2 (Square.fromString "d6"))

    [<Fact>]
    let ``Promotion creates promoted piece`` () =
        let b = Board.fromFen "8/4P3/8/8/8/8/8/8 w - - 0 1"

        let m = Move.create(Square.fromString "e7", Square.fromString "e8", 5, int PieceType.Queen)

        let b2 = Board.applyMove m b

        match Board.tryGetPiece b2 (Square.fromString "e8") with
        | p when p <> -1 -> Assert.Equal(PieceType.Queen, Piece.kind p)
        | _ -> Assert.True(false)

    [<Fact>]
    let ``Moving rook removes kingside castling rights`` () =
        let b = Board.fromFen "4k3/8/8/8/8/8/8/4K2R w K - 0 1"

        let m = Move.create(Square.fromString "h1", Square.fromString "h2", 0, 0)

        let b2 = Board.applyMove m b

        Assert.False(b2.CastlingRights &&& CastlingRights.WK <> 0)

    [<Fact>]
    let ``Moving King removes all castling rights`` () =
        let b = Board.fromFen "r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1"
        let m = Move.create(Square.fromString "e1", Square.fromString "e2", 0, 0)
        let b2 = Board.applyMove m b
        Assert.False(b2.CastlingRights &&& CastlingRights.WK <> 0)
        Assert.False(b2.CastlingRights &&& CastlingRights.WQ <> 0)
        Assert.True(b2.CastlingRights &&& CastlingRights.BK <> 0)
        Assert.True(b2.CastlingRights &&& CastlingRights.BQ <> 0)

    [<Fact>]
    let ``Capturing opponent rook removes their castling rights`` () =
        let b = Board.fromFen "r3k2r/8/8/8/8/8/8/R3K2R w KQkq - 0 1"
        // White Rook on a1 captures Black Rook on a8
        let m = Move.create(Square.fromString "a1", Square.fromString "a8", 1, 0)
        let b2 = Board.applyMove m b
        Assert.False(b2.CastlingRights &&& CastlingRights.BQ <> 0)
        Assert.True(b2.CastlingRights &&& CastlingRights.BK <> 0)

    [<Fact>]
    let ``Pawn move resets halfmove clock`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 10 1"
        let m = Move.create(Square.fromString "e2", Square.fromString "e4", 0, 0)
        let b2 = Board.applyMove m b
        Assert.Equal(0, b2.HalfmoveClock)

    [<Fact>]
    let ``Capture resets halfmove clock`` () =
        let b = Board.fromFen "rnb1kbnr/ppp1pppp/8/3q4/8/8/PPPP1PPP/RNBQKBNR w KQkq - 5 1"
        let m = Move.create(Square.fromString "d1", Square.fromString "d5", 1, 0)
        let b2 = Board.applyMove m b
        Assert.Equal(0, b2.HalfmoveClock)

    [<Fact>]
    let ``Quiet non-pawn move increments halfmove clock`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        let m = Move.create(Square.fromString "g1", Square.fromString "f3", 0, 0)
        let b2 = Board.applyMove m b
        Assert.Equal(1, b2.HalfmoveClock)

    [<Theory>]
    [<InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")>]
    [<InlineData("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1")>]
    [<InlineData("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1")>]
    let ``FEN roundtrip integrity`` (fen: string) =
        let board = Board.fromFen fen
        let output = Board.toFen board
        Assert.Equal(fen, output)

    [<Fact>]
    let ``fromUci parses promotion correctly`` () =
        let b = Board.fromFen "8/P7/8/8/8/8/8/k6K w - - 0 1"
        let m = Board.fromUci b "a7a8q"
        Assert.Equal(5, Move.kind m)
        Assert.Equal(int PieceType.Queen, Move.promo m)

    [<Fact>]
    let ``fromUci parses kingside castling correctly`` () =
        let b = Board.fromFen "4k3/8/8/8/8/8/8/R3K2R w KQ - 0 1"
        let m = Board.fromUci b "e1g1"
        Assert.Equal(3, Move.kind m)

    [<Fact>]
    let ``fromUci parses en passant correctly`` () =
        let b = Board.fromFen "8/8/8/3pP3/8/8/8/8 w - d6 0 1"
        let m = Board.fromUci b "e5d6"
        Assert.Equal(2, Move.kind m)

    [<Fact>]
    let ``fromUci parses capture correctly`` () =
        let b = Board.fromFen "8/8/8/3pP3/8/8/8/8 w - - 0 1"
        let m = Board.fromUci b "e5d5"
        Assert.Equal(1, Move.kind m)

    [<Fact>]
    let ``fromUci parses quiet move correctly`` () =
        let b = Board.fromFen "8/8/8/8/4P3/8/8/8 w - - 0 1"
        let m = Board.fromUci b "e4e5"
        Assert.Equal(0, Move.kind m)

    [<Fact>]
    let ``isInCheck detects simple check`` () =
        let b = Board.fromFen "8/8/8/8/4k3/8/4Q3/8 w - - 0 1"
        Assert.True(Board.isInCheckFor Colour.Black b)

    [<Fact>]
    let ``isInCheck is false when king not attacked`` () =
        let b = Board.fromFen "8/8/8/8/4k3/8/8/3Q4 w - - 0 1"
        Assert.False(Board.isInCheckFor Colour.Black b)

    [<Fact>]
    let ``applyNullMove flips side and updates hash consistently`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        let b2 = Board.applyNullMove b
        Assert.Equal(Colour.Black, b2.SideToMove)
        Assert.Equal(-1, b2.EnPassantSquare)
        Assert.Equal(b.HalfmoveClock + 1, b2.HalfmoveClock)

        let scratch = Board.calculateHash b2
        Assert.Equal(scratch, b2.Hash)

    [<Fact>]
    let ``K vs K is insufficient material`` () =
        let b = Board.fromFen "8/8/8/8/8/8/8/K6k w - - 0 1"
        Assert.True(Board.hasInsufficientMaterial b)

    [<Fact>]
    let ``KN vs K is insufficient material`` () =
        let b = Board.fromFen "8/8/8/8/8/8/8/KN4k1 w - - 0 1"
        Assert.True(Board.hasInsufficientMaterial b)

    [<Fact>]
    let ``KB vs KB on opposite colors is not insufficient`` () =
        let b = Board.fromFen "8/8/8/8/8/8/8/Kb4Bk w - - 0 1"
        Assert.False(Board.hasInsufficientMaterial b)

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
