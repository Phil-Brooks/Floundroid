module Floundroid.Tests.PerftTests

open Expecto
open Floundroid.Core.Types
open Floundroid.Core.Perft

[<Tests>]
let perftSuite =
    testList "Perft" [
        test "Dummy perft" {
            let dummyBoard =
                { Pieces = Map.empty
                  SideToMove = Color.White
                  CastlingRights = ""
                  EnPassantSquare = None
                  HalfmoveClock = 0
                  FullmoveNumber = 1 }
            let result = Perft.perft 0 dummyBoard
            Expect.equal result 1L "Perft(0) should be 1"
        }
    ]
