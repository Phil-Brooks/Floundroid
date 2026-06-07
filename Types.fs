namespace Chess.Core

[<Struct>]
type Color = White | Black
    with member x.Opposite = if x = White then Black else White

type PieceType = Pawn | Knight | Bishop | Rook | Queen | King

[<Struct>]
type Piece = { Type: PieceType; Color: Color }

[<Struct>]
type Square = 
    | Index of int // 0 to 63
    static member FromFileRank f r = Index (r * 8 + f)
    member x.Value = match x with Index i -> i
    member x.File = x.Value % 8
    member x.Rank = x.Value / 8

// High-level move representation
[<Struct>]
type Move = {
    From: Square
    To: Square
    Promotion: PieceType option
}