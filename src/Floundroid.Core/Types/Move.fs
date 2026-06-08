namespace Floundroid.Core.Types

open Floundroid.Core.Types

type Move =
    | Quiet of from:Square * dest:Square
    | Capture of from:Square * dest:Square
    | Promotion of from:Square * dest:Square * promoteTo:PieceType
    | CastleKingSide
    | CastleQueenSide
    | EnPassant of from:Square * dest:Square
