module Types

open System
open System.Text

/// Colour is represented as an integer 0–1, where 0 = White and 1 = Black.
[<RequireQualifiedAccess>]
type Colour =
    | White = 0
    | Black = 1

[<RequireQualifiedAccess>]
module Colour =

    let inline toChar (c: Colour) =
        if c = Colour.White then 'w' else 'b'

    let inline fromChar c =
        match c with
        | 'w' | 'W' -> Colour.White
        | 'b' | 'B' -> Colour.Black
        | _ -> invalidArg "c" $"Invalid colour char: %c{c}"

    let inline opposite (c: Colour) =
        if c = Colour.White then Colour.Black else Colour.White

[<RequireQualifiedAccess>]
type File =
    | A = 0
    | B = 1
    | C = 2
    | D = 3
    | E = 4
    | F = 5
    | G = 6
    | H = 7

[<RequireQualifiedAccess>]
module File =

    let firstChar = int 'a'
    
    /// Converts a File to its integer representation (0–7).
    let inline toInt (f: File) : int =
        int f

    /// Converts an integer (0–7) to a File.
    let inline fromInt (i: int) : File =
        if uint i <= 7u then enum<File> i
        else invalidArg "i" $"File index {i} out of range (0–7)"

    /// Converts a File to its character representation ('a'–'h').
    let inline toChar (f: File) : char =
        char (firstChar + int f)

    /// Converts a character ('a'–'h') to a File.
    let inline fromChar (c: char) : File =
        let i = int c - firstChar
        if uint i <= 7u then enum<File> i
        else invalidArg "c" $"Invalid file char: {c}"

[<RequireQualifiedAccess>]
type Rank =
    | R1 = 0
    | R2 = 1
    | R3 = 2
    | R4 = 3
    | R5 = 4
    | R6 = 5
    | R7 = 6
    | R8 = 7

[<RequireQualifiedAccess>]
module Rank =

    // Must NOT be private if inline functions use it
    let firstChar = int '1'

    /// Converts a Rank to its integer representation (0–7).
    let inline toInt (r: Rank) : int =
        int r

    /// Converts an integer (0–7) to a Rank.
    let inline fromInt (i: int) : Rank =
        if uint i <= 7u then enum<Rank> i
        else invalidArg "i" $"Rank index {i} out of range (0–7)"

    /// Converts a Rank to its character representation ('1'–'8').
    let inline toChar (r: Rank) : char =
        char (firstChar + int r)

    /// Converts a character ('1'–'8') to a Rank.
    let inline fromChar (c: char) : Rank =
        let i = int c - firstChar
        if uint i <= 7u then enum<Rank> i
        else invalidArg "c" $"Invalid rank char: {c}"

/// Squares are represented as integers 0–63, where 0 = a1 and 63 = h8.
type Square = int

[<RequireQualifiedAccess>]
module Square =

    /// Converts a File and Rank to a Square.
    let inline ofFileRank (f: File) (r: Rank) : Square =
        (Rank.toInt r <<< 3) + File.toInt f
        // same as r*8 + f, but uses bit shift

    /// Gets the File of a Square.
    let inline file (sq: Square) : File =
        File.fromInt (sq &&& 7)
        // sq % 8 → sq & 7

    /// Gets the Rank of a Square.
    let inline rank (sq: Square) : Rank =
        Rank.fromInt (sq >>> 3)
        // sq / 8 → sq >> 3

    /// Converts a Square to algebraic notation (e.g. "e4").
    let inline toString (sq: Square) : string =
        let f = file sq |> File.toChar
        let r = rank sq |> Rank.toChar
        string f + string r

    /// Converts algebraic notation (e.g. "d4") to a Square.
    let inline fromString (s: string) : Square =
        ofFileRank (File.fromChar s[0]) (Rank.fromChar s[1])

    /// Checks if a file/rank pair is on the board.
    let inline isOnBoard (f: int) (r: int) =
        uint f <= 7u && uint r <= 7u

[<RequireQualifiedAccess>]
type PieceType =
    | Pawn   = 0
    | Knight = 1
    | Bishop = 2
    | Rook   = 3
    | Queen  = 4
    | King   = 5

[<RequireQualifiedAccess>]
module PieceType =

    // Precomputed char table for speed
    let chars = [| 'p'; 'n'; 'b'; 'r'; 'q'; 'k' |]

    /// Converts a PieceType to its character representation ('p'..'k').
    let inline toChar (pt: PieceType) : char =
        chars[int pt]

    /// Converts a character to a PieceType.
    let inline fromChar (c: char) : PieceType =
        match c with
        | 'p' | 'P' -> PieceType.Pawn
        | 'n' | 'N' -> PieceType.Knight
        | 'b' | 'B' -> PieceType.Bishop
        | 'r' | 'R' -> PieceType.Rook
        | 'q' | 'Q' -> PieceType.Queen
        | 'k' | 'K' -> PieceType.King
        | _ -> invalidArg "c" $"Invalid piece type char: {c}"

[<Struct>]
type Piece =
    val data: byte
    new (colour: Colour, kind: PieceType) =
        { data = byte ((int colour <<< 3) ||| int kind) }

[<RequireQualifiedAccess>]
module Piece =

    let inline colour (p: Piece) : Colour =
        enum<Colour> (int p.data >>> 3)

    let inline kind (p: Piece) : PieceType =
        enum<PieceType> (int p.data &&& 0b111)

    let inline toChar (p: Piece) : char =
        let c = PieceType.toChar (kind p)
        if colour p = Colour.White then Char.ToUpper c else c

    let inline fromChar (c: char) : Piece =
        let col = if Char.IsUpper c then Colour.White else Colour.Black
        let kind = PieceType.fromChar c
        Piece(col, kind)

[<System.Flags>]
type CastlingRights =
    | None = 0b0000
    | WK   = 0b0001
    | WQ   = 0b0010
    | BK   = 0b0100
    | BQ   = 0b1000

[<RequireQualifiedAccess>]
module CastlingRights =

    let inline fromString (s: string) =
        if s = "-" then CastlingRights.None
        else
            (if s.Contains "K" then CastlingRights.WK else CastlingRights.None)
            ||| (if s.Contains "Q" then CastlingRights.WQ else CastlingRights.None)
            ||| (if s.Contains "k" then CastlingRights.BK else CastlingRights.None)
            ||| (if s.Contains "q" then CastlingRights.BQ else CastlingRights.None)

    let inline toString (cr: CastlingRights) =
        let sb = System.Text.StringBuilder()

        if cr.HasFlag CastlingRights.WK then sb.Append('K') |> ignore
        if cr.HasFlag CastlingRights.WQ then sb.Append('Q') |> ignore
        if cr.HasFlag CastlingRights.BK then sb.Append('k') |> ignore
        if cr.HasFlag CastlingRights.BQ then sb.Append('q') |> ignore

        if sb.Length = 0 then "-" else sb.ToString()

[<Struct>]
type Move =
    val data: uint32

    new (fromSq: Square, toSq: Square, kind: int, promo: int) =
        { data =
            uint32 fromSq
            ||| (uint32 toSq <<< 6)
            ||| (uint32 kind <<< 12)
            ||| (uint32 promo <<< 16) }

[<RequireQualifiedAccess>]
module Move =

    let inline fromSq (m: Move) =
        int (m.data &&& 0b111111u)

    let inline toSq (m: Move) =
        int ((m.data >>> 6) &&& 0b111111u)

    let inline kind (m: Move) =
        int ((m.data >>> 12) &&& 0b1111u)

    let inline promo (m: Move) =
        int ((m.data >>> 16) &&& 0b1111u)

    let toUci (m: Move) =
        let baseStr =
            Square.toString (fromSq m) +
            Square.toString (toSq m)

        match kind m with
        | 5 ->   // promotion
            let pt = enum<PieceType> (promo m)
            baseStr + (PieceType.toChar pt |> Char.ToUpper |> string)
        | _ ->
            baseStr


