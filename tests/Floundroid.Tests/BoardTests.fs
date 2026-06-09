module Floundroid.Tests.BoardTests

open Xunit
open Floundroid.Core.Types

[<Fact>]
let ``Empty board has no pieces`` () =
    let b = Board.empty
    for sq in 0..63 do
        Assert.True(Board.tryGetPiece b sq |> Option.isNone)

[<Fact>]
let ``Set and get piece works`` () =
    let b = Board.empty
    let sq = Square.fromString "e4"
    let p = Piece.fromChar 'Q'

    let b2 = Board.setPiece b sq (Some p)

    Assert.Equal(Some p, Board.tryGetPiece b2 sq)

[<Fact>]
let ``Remove piece works`` () =
    let b = Board.empty
    let sq = Square.fromString "e4"
    let p = Piece.fromChar 'Q'

    let b2 = Board.setPiece b sq (Some p)
    let b3 = Board.setPiece b2 sq None

    Assert.True(Board.tryGetPiece b3 sq |> Option.isNone)
