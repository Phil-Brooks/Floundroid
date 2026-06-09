module Floundroid.Tests.MoveTests

open Xunit
open Floundroid.Core.Types

[<Fact>]
let ``Move stores fields correctly`` () =
    let fromSq = Square.fromString "e2"
    let toSq = Square.fromString "e4"
    let mv =
        { From = fromSq
          To = toSq
          Kind = Quiet }

    Assert.Equal(fromSq, mv.From)
    Assert.Equal(toSq, mv.To)
    Assert.Equal(Quiet, mv.Kind)
