namespace Floundroid.Tests

open Xunit
open FsCheck.Xunit
open FsCheck.FSharp
open Floundroid

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

module BitboardSetTests =

    // Helpers
    let allSquares:int list = [0 .. 63]

    let allPieces =
        [ for c in [Colour.White; Colour.Black] do
            for k in [ PieceType.Pawn; PieceType.Knight; PieceType.Bishop
                       PieceType.Rook; PieceType.Queen; PieceType.King ] ->
                (c <<< 3) ||| k ]

    // ------------------------------------------------------------
    // Basic behaviour tests
    // ------------------------------------------------------------

    [<Fact>]
    let ``empty has no pieces`` () =
        for sq in allSquares do
            Assert.Equal(-1, BitboardSet.getPieceAt sq BitboardSet.empty)

    [<Fact>]
    let ``togglePiece adds then removes piece`` () =
        let sq = 12
        let p = (Colour.White <<< 3) ||| PieceType.Knight

        let b1 = BitboardSet.togglePiece p sq BitboardSet.empty
        Assert.Equal(p, BitboardSet.getPieceAt sq b1)

        let b2 = BitboardSet.togglePiece p sq b1
        Assert.Equal(-1, BitboardSet.getPieceAt sq b2)

    [<Fact>]
    let ``togglePiece updates totals and occupancy`` () =
        let sq = 5
        let p = (Colour.Black <<< 3) ||| PieceType.Queen

        let b1 = BitboardSet.togglePiece p sq BitboardSet.empty

        Assert.True((b1.BlackTotal &&& (1UL <<< sq)) <> 0UL)
        Assert.True((b1.Occupancy &&& (1UL <<< sq)) <> 0UL)

        let b2 = BitboardSet.togglePiece p sq b1

        Assert.True((b2.BlackTotal &&& (1UL <<< sq)) = 0UL)
        Assert.True((b2.Occupancy &&& (1UL <<< sq)) = 0UL)

    // ------------------------------------------------------------
    // getPieceAt correctness
    // ------------------------------------------------------------

    [<Fact>]
    let ``getPieceAt returns correct piece after toggle`` () =
        for p in allPieces do
            for sq in allSquares do
                let b = BitboardSet.togglePiece p sq BitboardSet.empty
                Assert.Equal(p, BitboardSet.getPieceAt sq b)

    [<Fact>]
    let ``getPieceAt returns -1 for untouched squares`` () =
        let p = (Colour.White <<< 3) ||| PieceType.Bishop
        let sq = 30
        let b = BitboardSet.togglePiece p sq BitboardSet.empty

        for otherSq in allSquares do
            if otherSq <> sq then
                Assert.Equal(-1, BitboardSet.getPieceAt otherSq b)

    // ------------------------------------------------------------
    // allPieces roundtrip
    // ------------------------------------------------------------

    [<Fact>]
    let ``allPieces returns exactly the toggled pieces`` () =
        let placements: (int * int) list =
            [ (0, (Colour.White <<< 3) ||| PieceType.Pawn)
              (7, (Colour.Black <<< 3) ||| PieceType.Knight)
              (55, (Colour.White <<< 3) ||| PieceType.Queen) ]
            |> List.sortBy fst

        let board =
            placements
            |> List.fold (fun b (sq, p) -> BitboardSet.togglePiece p sq b) BitboardSet.empty

        let pieces: (int * int) list = BitboardSet.allPieces board |> Seq.toList|> List.sortBy fst

        Assert.Equal<(int * int) list>(placements, pieces)

    // ------------------------------------------------------------
    // Property-based tests
    // ------------------------------------------------------------

    type PieceGen =
        static member Piece() =
            Gen.elements allPieces |> Arb.fromGen

    type SquareGen =
        static member Square() =
            Gen.choose(0, 63)
            |> Gen.map (fun i -> i : int)
            |> Arb.fromGen

    [<Property(Arbitrary=[| typeof<PieceGen>; typeof<SquareGen> |])>]
    let ``prop - toggle twice restores board`` (p: int) (sq: int) =
        let b0 = BitboardSet.empty
        let b1 = BitboardSet.togglePiece p sq b0
        let b2 = BitboardSet.togglePiece p sq b1
        b2 = b0

    [<Property(Arbitrary=[| typeof<PieceGen>; typeof<SquareGen> |])>]
    let ``prop - getPieceAt after toggle returns piece`` (p: int) (sq: int) =
        let b = BitboardSet.togglePiece p sq BitboardSet.empty
        BitboardSet.getPieceAt sq b = p

    [<Property(Arbitrary=[| typeof<PieceGen>; typeof<SquareGen> |])>]
    let ``prop - allPieces contains toggled piece`` (p: int) (sq: int) =
        let b = BitboardSet.togglePiece p sq BitboardSet.empty
        BitboardSet.allPieces b |> Seq.exists (fun (s, pc) -> s = sq && pc = p)

module MagicTests =

    [<Fact>]
    let ``Rook slow attacks are blocked correctly`` () =
        let e4 = Square.fromString "e4"
        // Place a blocker on e6 and c4
        let blockers = (1uL <<< Square.fromString "e6") ||| (1uL <<< Square.fromString "c4")
        
        let attacks = Magic.rookAttacks e4 blockers
        
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
        let attacks = Magic.bishopAttacks d4 0uL // Empty board
        
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
        let mask = Magic.rookMask e4
        
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
        let mask = Magic.bishopMask d4
        
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
        let slowResult = Magic.rookAttacks a1 blockers
        
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
        let slowResult = Magic.bishopAttacks d4 blockers
        
        let index = Magic.getIndex blockers entry.Mask
        let fastResult = Magic.bishopTable.[entry.Offset + index]
        
        Assert.Equal(slowResult, fastResult)    
    
    [<Fact>]
    let ``getIndex and getBlockers are perfect inverses for rook masks`` () =
        for sq in 0 .. 63 do
            let mask = Magic.rookMask sq
            let bits = Bitboard.count mask
            for i in 0 .. (1 <<< bits) - 1 do
                let blockers = 
                    // private function, so reimplement here
                    let mutable b = 0UL
                    let mutable temp = mask
                    for bit in 0 .. bits - 1 do
                        let idx = System.Numerics.BitOperations.TrailingZeroCount temp
                        temp <- temp &&& (temp - 1UL)
                        if (i &&& (1 <<< bit)) <> 0 then
                            b <- b ||| (1UL <<< idx)
                    b

                let idx2 = Magic.getIndex blockers mask
                Assert.Equal(i, idx2)

    [<Fact>]
    let ``getIndex and getBlockers are perfect inverses for bishop masks`` () =
        for sq in 0 .. 63 do
            let mask = Magic.bishopMask sq
            let bits = Bitboard.count mask
            for i in 0 .. (1 <<< bits) - 1 do
                let blockers = 
                    let mutable b = 0UL
                    let mutable temp = mask
                    for bit in 0 .. bits - 1 do
                        let idx = System.Numerics.BitOperations.TrailingZeroCount temp
                        temp <- temp &&& (temp - 1UL)
                        if (i &&& (1 <<< bit)) <> 0 then
                            b <- b ||| (1UL <<< idx)
                    b

                let idx2 = Magic.getIndex blockers mask
                Assert.Equal(i, idx2)

    [<Fact>]
    let ``rookMask never includes the origin square`` () =
        for sq in 0 .. 63 do
            let mask = Magic.rookMask sq
            Assert.False(Bitboard.contains sq mask)

    [<Fact>]
    let ``bishopMask never includes the origin square`` () =
        for sq in 0 .. 63 do
            let mask = Magic.bishopMask sq
            Assert.False(Bitboard.contains sq mask)

    type BlockerGen =
        static member Blockers() =
            Arb.fromGen <|
                gen {
                    let! squares = Gen.listOf (Gen.choose(0,63))
                    let mutable bb = 0UL
                    for sq in squares do
                        bb <- bb ||| (1UL <<< sq)
                    return bb
                }
    
    [<Property(Arbitrary=[| typeof<BlockerGen> |])>]
    let ``rookAttacks stops exactly at blockers`` (sq: int) (blockers: Bitboard) =
        let sq = sq % 64
        let attacks = Magic.rookAttacks sq blockers

        // For each direction, walk until blocker
        let r, f = sq / 8, sq % 8
        let dirs = [ (1,0); (-1,0); (0,1); (0,-1) ]

        for dr, df in dirs do
            let mutable nr, nf = r + dr, f + df
            let mutable stopped = false
            while nr >= 0 && nr < 8 && nf >= 0 && nf < 8 && not stopped do
                let t = nr * 8 + nf
                let bit = 1UL <<< t
                Assert.True(Bitboard.contains t attacks)
                if (blockers &&& bit) <> 0UL then
                    stopped <- true
                nr <- nr + dr
                nf <- nf + df

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

    [<Fact>]
    let ``Knight on d4 has 8 attacks`` () =
        let d4 = Square.fromString "d4"
        let attacks = BitboardGen.knightAttacks.[d4]
        Assert.Equal(8, Bitboard.count attacks)

    [<Fact>]
    let ``King on d4 has 8 attacks`` () =
        let d4 = Square.fromString "d4"
        let attacks = BitboardGen.kingAttacks.[d4]
        Assert.Equal(8, Bitboard.count attacks)

    [<Fact>]
    let ``White pawn on 7th rank has no forward attacks`` () =
        let e7 = Square.fromString "e7"
        let attacks = BitboardGen.pawnAttacks.[0, e7]
        Assert.Equal(2, Bitboard.count attacks)
        Assert.True(Bitboard.contains (Square.fromString "d8") attacks)
        Assert.True(Bitboard.contains (Square.fromString "f8") attacks)

    [<Fact>]
    let ``Black pawn on 2nd rank has no forward attacks`` () =
        let d2 = Square.fromString "d2"
        let attacks = BitboardGen.pawnAttacks.[1, d2]
        Assert.Equal(2, Bitboard.count attacks)
        Assert.True(Bitboard.contains (Square.fromString "c1") attacks)
        Assert.True(Bitboard.contains (Square.fromString "e1") attacks)

    [<Property>]
    let ``Knight attacks never wrap off the board`` (sq: int) =
        let sq = (sq % 64 + 64) % 64
        let attacks = BitboardGen.knightAttacks.[sq]

        // For every bit set in attacks, ensure it's a legal knight move
        for target in Bitboard.bits attacks do
            let df = abs ((target % 8) - (sq % 8))
            let dr = abs ((target / 8) - (sq / 8))
            Assert.True((df = 1 && dr = 2) || (df = 2 && dr = 1))

    [<Property>]
    let ``King attacks are always 1 square away`` (sq: int) =
        let sq = (sq % 64 + 64) % 64
        let attacks = BitboardGen.kingAttacks.[sq]

        for target in Bitboard.bits attacks do
            let df = abs ((target % 8) - (sq % 8))
            let dr = abs ((target / 8) - (sq / 8))
            Assert.True(df <= 1 && dr <= 1 && not (df = 0 && dr = 0))

    [<Property>]
    let ``White pawn attacks are only NE or NW`` (sq: int) =
        let sq = (sq % 64 + 64) % 64
        let attacks = BitboardGen.pawnAttacks.[0, sq]

        for target in Bitboard.bits attacks do
            let df = (target % 8) - (sq % 8)
            let dr = (target / 8) - (sq / 8)
            Assert.True(dr = 1 && (df = 1 || df = -1))

    [<Property>]
    let ``Black pawn attacks are only SE or SW`` (sq: int) =
        let sq = (sq % 64 + 64) % 64
        let attacks = BitboardGen.pawnAttacks.[1, sq]

        for target in Bitboard.bits attacks do
            let df = (target % 8) - (sq % 8)
            let dr = (target / 8) - (sq / 8)
            Assert.True(dr = -1 && (df = 1 || df = -1))

    [<Fact>]
    let ``Knight on h8 has 2 attacks`` () =
        let h8 = Square.fromString "h8"
        Assert.Equal(2, Bitboard.count BitboardGen.knightAttacks.[h8])

    [<Fact>]
    let ``King on h8 has 3 attacks`` () =
        let h8 = Square.fromString "h8"
        Assert.Equal(3, Bitboard.count BitboardGen.kingAttacks.[h8])

    [<Fact>]
    let ``White pawn on a7 only attacks b8`` () =
        let a7 = Square.fromString "a7"
        let attacks = BitboardGen.pawnAttacks.[0, a7]
        Assert.Equal(1, Bitboard.count attacks)
        Assert.True(Bitboard.contains (Square.fromString "b8") attacks)
