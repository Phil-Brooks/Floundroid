namespace Floundroid.Core.Types

open Floundroid.Core.Types

type Board =
    { Pieces: Map<Square, Piece>
      SideToMove: Color
      CastlingRights: string
      EnPassantSquare: Square option
      HalfmoveClock: int
      FullmoveNumber: int }
