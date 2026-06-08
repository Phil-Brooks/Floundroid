namespace Floundroid.Core.Types

open Floundroid.Core.Types

/// Represents the type of a chess piece.
type PieceType =
    | Pawn
    | Knight
    | Bishop
    | Rook
    | Queen
    | King

module PieceType =
    let toChar = function
        | Pawn   -> 'p'
        | Knight -> 'n'
        | Bishop -> 'b'
        | Rook   -> 'r'
        | Queen  -> 'q'
        | King   -> 'k'

    let fromChar = function
        | 'p' | 'P' -> Pawn
        | 'n' | 'N' -> Knight
        | 'b' | 'B' -> Bishop
        | 'r' | 'R' -> Rook
        | 'q' | 'Q' -> Queen
        | 'k' | 'K' -> King
        | c -> invalidArg "c" $"Invalid piece type character: {c}"

/// Represents a chess piece with colour and type.
type Piece =
    { Colour : Colour
      Kind   : PieceType }

module Piece =
    let toChar (p: Piece) =
        let baseChar = PieceType.toChar p.Kind
        match p.Colour with
        | White -> System.Char.ToUpper baseChar
        | Black -> baseChar

    let fromChar = function
        | 'p' -> { Colour = Black; Kind = Pawn }
        | 'n' -> { Colour = Black; Kind = Knight }
        | 'b' -> { Colour = Black; Kind = Bishop }
        | 'r' -> { Colour = Black; Kind = Rook }
        | 'q' -> { Colour = Black; Kind = Queen }
        | 'k' -> { Colour = Black; Kind = King }
        | 'P' -> { Colour = White; Kind = Pawn }
        | 'N' -> { Colour = White; Kind = Knight }
        | 'B' -> { Colour = White; Kind = Bishop }
        | 'R' -> { Colour = White; Kind = Rook }
        | 'Q' -> { Colour = White; Kind = Queen }
        | 'K' -> { Colour = White; Kind = King }
        | c -> invalidArg "c" $"Invalid piece character: {c}"
