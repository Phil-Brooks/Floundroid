namespace Floundroid.Core.Types

open Floundroid.Core.Types

/// A square is represented as an integer 0–63 (a1 = 0, h8 = 63).
/// File = sq % 8, Rank = sq / 8
type Square = int

module Square =

    /// Create a square from file and rank.
    let ofFileRank (file: File) (rank: Rank) : Square =
        Rank.toInt rank * 8 + File.toInt file

    /// Extract the file from a square.
    let file (sq: Square) : File =
        File.fromInt (sq % 8)

    /// Extract the rank from a square.
    let rank (sq: Square) : Rank =
        Rank.fromInt (sq / 8)

    /// Convert a square to algebraic notation, e.g. "e4".
    let toString (sq: Square) : string =
        let f = file sq |> File.toChar
        let r = rank sq |> Rank.toChar
        $"{f}{r}"

    /// Parse a square from algebraic notation, e.g. "e4".
    let fromString (s: string) : Square =
        if s.Length <> 2 then
            invalidArg "s" "Square must be exactly 2 characters (e.g. 'e4')"

        let f = File.fromChar s[0]
        let r = Rank.fromChar s[1]
        ofFileRank f r

    /// Check whether a square index is valid (0–63).
    let isValid (sq: Square) =
        sq >= 0 && sq < 64
