module Floundroid.Tests.FileTests

open Xunit
open Floundroid.Core.Types

[<Fact>]
let ``File int roundtrip`` () =
    for i in 0..7 do
        let f = File.fromInt i
        Assert.Equal(i, File.toInt f)

[<Fact>]
let ``File char roundtrip`` () =
    for c in ['a'..'h'] do
        let f = File.fromChar c
        Assert.Equal(c, File.toChar f)
