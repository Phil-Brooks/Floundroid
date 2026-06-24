namespace TypesTests

open System
open Xunit
open FsCheck
open FsCheck.Xunit
open Types


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

