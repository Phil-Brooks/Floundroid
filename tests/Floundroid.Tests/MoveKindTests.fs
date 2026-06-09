module Floundroid.Tests.MoveKindTests

open Xunit
open Floundroid.Core.Types

[<Fact>]
let ``Quiet move kind is stable and matches itself`` () =
    let a = Quiet
    let b = Quiet
    Assert.Equal(a, b)

[<Fact>]
let ``MoveKind pattern match on Quiet works`` () =
    let mk : MoveKind = Quiet
    match mk with
    | Quiet -> Assert.True(true)
    | _ -> Assert.True(false, "Expected Quiet move kind")
