namespace Floundroid.Core.Types

type Color = White | Black

type PieceType =
    | Pawn | Knight | Bishop | Rook | Queen | King

type Piece =
    { Color: Color
      Kind: PieceType }
