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
    let tryGetPiece (b: Board) (sq: Square) =
        b.Pieces |> Map.tryFind sq

    /// Check if a square is occupied.
    let isOccupied (b: Board) (sq: Square) =
        b.Pieces |> Map.containsKey sq

    /// Place or remove a piece on a square.
    let setPiece (b: Board) (sq: Square) (pieceOpt: Piece option) =
        match pieceOpt with
        | Some piece -> { b with Pieces = b.Pieces.Add(sq, piece) }
        | None -> { b with Pieces = b.Pieces.Remove sq }

    /// Remove a piece from a square.
    let removePiece (b: Board) (sq: Square) =
        { b with Pieces = b.Pieces.Remove sq }

    /// Move a piece from one square to another (no legality checks).
    let applyMove (m: Move) (b: Board) =
        match tryGetPiece b m.From with
        | None ->
            invalidArg "m" $"No piece on {Square.toString m.From}"

        | Some piece ->
            let b1 = removePiece b m.From
            let b2 =
                match m.Kind with
                | Promotion pt ->
                    let promoted = { Colour = piece.Colour; Kind = pt }
                    setPiece b1 m.To (Some promoted)
                | _ ->
                    setPiece b1 m.To (Some piece)

            let nextSide = Colour.opposite b.SideToMove

            { b2 with
                SideToMove = nextSide
                EnPassantSquare = None
                HalfmoveClock =
                    match piece.Kind, m.Kind with
                    | Pawn, _ -> 0
                    | _, Capture -> 0
                    | _ -> b.HalfmoveClock + 1
                FullmoveNumber =
                    if b.SideToMove = Black then b.FullmoveNumber + 1
                    else b.FullmoveNumber }
