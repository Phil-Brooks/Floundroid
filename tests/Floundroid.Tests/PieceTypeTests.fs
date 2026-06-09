module Floundroid.Tests.PieceTypeTests

open Xunit
open Floundroid.Core.Types

[<Fact>]
let ``PieceType char roundtrip`` () =
    let chars = [ 'p'; 'n'; 'b'; 'r'; 'q'; 'k' ]
    for c in chars do
        let pt = PieceType.fromChar c
        Assert.Equal(c, PieceType.toChar pt)
