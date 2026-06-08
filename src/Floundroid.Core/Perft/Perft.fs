namespace Floundroid.Core.Perft

open Floundroid.Core.Types
open Floundroid.Core.MoveGen

module Perft =
    let rec perft depth (board: Board) : int64 =
        if depth = 0 then 1L
        else
            LegalMoves.generateLegal board
            |> List.sumBy (fun _ -> 0L)
