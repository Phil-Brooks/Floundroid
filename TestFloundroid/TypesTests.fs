namespace TypesTests

open System
open Xunit
open Types


module ColourTests =

    [<Fact>]
    /// Tests that the opposite of a colour is correctly computed.
    let ``opposite works both ways`` () =
        Assert.Equal(Colour.Black, Colour.opposite Colour.White)
        Assert.Equal(Colour.White, Colour.opposite Colour.Black)

    [<Fact>]
    /// Tests that the opposite of a colour is involutive.
    let ``opposite is involutive`` () =
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
