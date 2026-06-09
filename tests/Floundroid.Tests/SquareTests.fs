module Floundroid.Tests.SquareTests

open Xunit
open Floundroid.Core.Types

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
        [ for f in ['a'..'h'] do
            for r in ['1'..'8'] do
                $"{f}{r}" ]

    for s in allSquares do
        let sq = Square.fromString s
        Assert.Equal(s, Square.toString sq)
