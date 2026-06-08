namespace Floundroid.Search

open Floundroid.Core.Types
open Evaluation
open MoveOrdering
open Floundroid.Core.MoveGen

module AlphaBeta =
    let rec search depth alpha beta (board: Board) =
        if depth = 0 then evaluate board
        else evaluate board
