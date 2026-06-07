namespace Chess.Core

module Perft =
    let rec countMoves (depth: int) (state: BoardState) : int64 =
        if depth = 0 then 1L
        else
            let moves = MoveGen.getLegalMoves state
            moves 
            |> List.map (fun m -> 
                let newState = Board.applyMove m state
                countMoves (depth - 1) newState)
            |> List.sum