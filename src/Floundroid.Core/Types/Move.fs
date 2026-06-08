namespace Floundroid.Core.Types

open Floundroid.Core.Types

/// Represents the type of a move (quiet, capture, promotion, castling, etc.)
type MoveKind =
    | Quiet
    | Capture
    | Promotion of PieceType
    | EnPassant
    | CastleKingSide
    | CastleQueenSide

/// Represents a chess move from one square to another.
type Move =
    { From : Square
      To   : Square
      Kind : MoveKind }

module Move =

    /// Convert a move to UCI notation (e.g. "e2e4", "e7e8q").
    let toUci (m: Move) =
        let baseStr = Square.toString m.From + Square.toString m.To
        match m.Kind with
        | Promotion pt ->
            let promoChar = PieceType.toChar pt |> System.Char.ToLower
            baseStr + string promoChar
        | _ -> baseStr

    /// Parse a move from UCI notation.
    /// Note: This does NOT validate legality — that is handled in movegen.
    let fromUci (s: string) =
        if s.Length < 4 then
            invalidArg "s" "UCI move must be at least 4 characters"

        let fromSq = Square.fromString (s.Substring(0, 2))
        let toSq   = Square.fromString (s.Substring(2, 2))

        let kind =
            if s.Length = 5 then
                // Promotion
                let promoChar = s[4]
                let pt = PieceType.fromChar promoChar
                Promotion pt
            else
                // Quiet by default — movegen will refine this
                Quiet

        { From = fromSq; To = toSq; Kind = kind }
