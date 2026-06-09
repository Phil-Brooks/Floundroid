module Floundroid.Tests.ColourTests

open Xunit
open Floundroid.Core.Types

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
