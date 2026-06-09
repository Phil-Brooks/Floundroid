module Floundroid.Tests.BoardTests

open Xunit
open Floundroid.Core.Types

[<Fact>]
let ``Empty board has no pieces`` () =
    let b = Board.empty
    Assert.True(b.Pieces.IsEmpty)

[<Fact>]
let ``Add piece to board using record update`` () =
    let b = Board.empty
    let sq = Square.fromString "e4"
    let p = Piece.fromChar 'Q'

    let b2 = { b with Pieces = b.Pieces.Add(sq, p) }

    Assert.Equal(Some p, b2.Pieces.TryFind sq)

[<Fact>]
let ``Remove piece using record update`` () =
    let b = Board.empty
    let sq = Square.fromString "e4"
    let p = Piece.fromChar 'Q'

    let b2 = { b with Pieces = b.Pieces.Add(sq, p) }
    let b3 = { b2 with Pieces = b2.Pieces.Remove sq }

    Assert.True(b3.Pieces.ContainsKey sq |> not)
