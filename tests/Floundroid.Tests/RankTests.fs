module Floundroid.Tests.RankTests

open Xunit
open Floundroid.Core.Types

[<Fact>]
let ``Rank int roundtrip`` () =
    for i in 0..7 do
        let r = Rank.fromInt i
        Assert.Equal(i, Rank.toInt r)

[<Fact>]
let ``Rank char roundtrip`` () =
    for c in ['1'..'8'] do
        let r = Rank.fromChar c
        Assert.Equal(c, Rank.toChar r)
