namespace Floundroid.Core.Types

open Floundroid.Core.Types

/// Represents the full state of a chess position.
/// This is equivalent to GameState in most engines.
type Board =
    { Pieces          : Map<Square, Piece>
      SideToMove      : Colour
      CastlingRights  : CastlingRights
      EnPassantSquare : Square option
      HalfmoveClock   : int
      FullmoveNumber  : int }

module Board =

    /// Create an empty board (no pieces, no rights).
    let empty =
        { Pieces = Map.empty
          SideToMove = White
          CastlingRights = CastlingRights.none
          EnPassantSquare = None
          HalfmoveClock = 0
          FullmoveNumber = 1 }

    /// Try to get the piece on a given square.
    let tryGetPiece (sq: Square) (b: Board) =
        b.Pieces |> Map.tryFind sq

    /// Check if a square is occupied.
    let isOccupied (sq: Square) (b: Board) =
        b.Pieces |> Map.containsKey sq

    /// Place or replace a piece on a square.
    let setPiece (sq: Square) (piece: Piece) (b: Board) =
        { b with Pieces = b.Pieces |> Map.add sq piece }

    /// Remove a piece from a square.
    let removePiece (sq: Square) (b: Board) =
        { b with Pieces = b.Pieces |> Map.remove sq }

    /// Move a piece from one square to another (no legality checks).
    let applyMove (m: Move) (b: Board) =
        match tryGetPiece m.From b with
        | None ->
            invalidArg "m" $"No piece on {Square.toString m.From}"

        | Some piece ->
            let b1 = removePiece m.From b
            let b2 =
                match m.Kind with
                | Promotion pt ->
                    let promoted = { Colour = piece.Colour; Kind = pt }
                    setPiece m.To promoted b1
                | _ ->
                    setPiece m.To piece b1

            // Update metadata (side to move, clocks, etc.)
            let nextSide = Colour.opposite b.SideToMove

            { b2 with
                SideToMove = nextSide
                EnPassantSquare = None // movegen will set this when needed
                HalfmoveClock =
                    match piece.Kind, m.Kind with
                    | Pawn, _ -> 0
                    | _, Capture -> 0
                    | _ -> b.HalfmoveClock + 1
                FullmoveNumber =
                    if b.SideToMove = Black then b.FullmoveNumber + 1
                    else b.FullmoveNumber }
