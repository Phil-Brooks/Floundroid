namespace Chess.Core
module MoveGen =
    let getPseudoLegalMoves (state: BoardState) : Move list =
        state.Squares 
        |> Array.mapi (fun i p -> (Square.Index i, p))
        |> Array.choose (fun (sq, p) -> 
            match p with 
            | Some piece when piece.Color = state.Turn -> Some (sq, piece)
            | _ -> None)
        |> Array.toList
        |> List.collect (fun (sq, piece) -> 
            // Match piece.Type and calculate target squares
            [])

    let isSquareAttacked (target: Square) (byColor: Color) (state: BoardState) =
        // Logic: "If I put a Knight on target, does it hit a Knight of byColor?"
        // This is crucial for verifying if a move is legal (King safety).
        false 

    let getLegalMoves (state: BoardState) : Move list =
        getPseudoLegalMoves state
        |> List.filter (fun m -> 
            // 1. Simulate the move
            // 2. Check if King is attacked by opponent
            // 3. Keep if King is safe
            true)

