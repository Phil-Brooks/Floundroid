namespace Floundroid.Core.Types

/// Represents the side to move or the colour of a piece.
type Colour =
    | White
    | Black

module Colour =
    let opposite = function
        | White -> Black
        | Black -> White

/// Represents the file (column) of a square: a–h.
type File =
    | A | B | C | D | E | F | G | H

module File =
    let toInt = function
        | A -> 0 | B -> 1 | C -> 2 | D -> 3
        | E -> 4 | F -> 5 | G -> 6 | H -> 7

    let fromInt = function
        | 0 -> A | 1 -> B | 2 -> C | 3 -> D
        | 4 -> E | 5 -> F | 6 -> G | 7 -> H
        | _ -> invalidArg "i" "File index must be 0–7"

    let toChar f =
        "abcdefgh".[toInt f]

    let fromChar = function
        | 'a' -> A | 'b' -> B | 'c' -> C | 'd' -> D
        | 'e' -> E | 'f' -> F | 'g' -> G | 'h' -> H
        | c -> invalidArg "c" $"Invalid file character: {c}"

/// Represents the rank (row) of a square: 1–8.
type Rank =
    | R1 | R2 | R3 | R4 | R5 | R6 | R7 | R8

module Rank =
    let toInt = function
        | R1 -> 0 | R2 -> 1 | R3 -> 2 | R4 -> 3
        | R5 -> 4 | R6 -> 5 | R7 -> 6 | R8 -> 7

    let fromInt = function
        | 0 -> R1 | 1 -> R2 | 2 -> R3 | 3 -> R4
        | 4 -> R5 | 5 -> R6 | 6 -> R7 | 7 -> R8
        | _ -> invalidArg "i" "Rank index must be 0–7"

    let toChar r =
        "12345678".[toInt r]

    let fromChar = function
        | '1' -> R1 | '2' -> R2 | '3' -> R3 | '4' -> R4
        | '5' -> R5 | '6' -> R6 | '7' -> R7 | '8' -> R8
        | c -> invalidArg "c" $"Invalid rank character: {c}"

/// Represents castling rights for both sides.
type CastlingRights =
    { WhiteKingSide: bool
      WhiteQueenSide: bool
      BlackKingSide: bool
      BlackQueenSide: bool }

module CastlingRights =
    let none =
        { WhiteKingSide = false
          WhiteQueenSide = false
          BlackKingSide = false
          BlackQueenSide = false }

    let fromString (s: string) =
        if s = "-" then none
        else
            { WhiteKingSide = s.Contains "K"
              WhiteQueenSide = s.Contains "Q"
              BlackKingSide = s.Contains "k"
              BlackQueenSide = s.Contains "q" }

    let toString cr =
        let sb = System.Text.StringBuilder()
        if cr.WhiteKingSide then sb.Append("K") |> ignore
        if cr.WhiteQueenSide then sb.Append("Q") |> ignore
        if cr.BlackKingSide then sb.Append("k") |> ignore
        if cr.BlackQueenSide then sb.Append("q") |> ignore
        if sb.Length = 0 then "-" else sb.ToString()
