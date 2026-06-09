module Floundroid.Tests.PieceTests

open Xunit
open Floundroid.Core.Types

[<Fact>]
let ``Piece char roundtrip`` () =
    let chars = [ 'p'; 'n'; 'b'; 'r'; 'q'; 'k'; 'P'; 'N'; 'B'; 'R'; 'Q'; 'K' ]
    for c in chars do
        let p = Piece.fromChar c
        Assert.Equal(c, Piece.toChar p)
