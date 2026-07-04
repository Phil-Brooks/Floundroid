namespace Floundroid

open System

module Colour =
    let [<Literal>] White = 0
    let [<Literal>] Black = 1

    /// Converts a Colour to its character representation ('w' or 'b').
    let toChar (c: int) =
        if c = White then 'w' else 'b'

    /// Converts a character ('w' or 'b') to a Colour.
    let fromChar c =
        match c with
        | 'w' | 'W' -> White
        | 'b' | 'B' -> Black
        | _ -> invalidArg "c" $"Invalid colour char: %c{c}"

    /// Returns the opposite colour.
    let opposite (c: int) =  c ^^^ 1

module File =
    let [<Literal>] A = 0
    let [<Literal>] B = 1
    let [<Literal>] C = 2
    let [<Literal>] D = 3
    let [<Literal>] E = 4
    let [<Literal>] F = 5
    let [<Literal>] G = 6
    let [<Literal>] H = 7

    let firstChar = int 'a'
    
    /// Converts a File to its character representation ('a'–'h').
    let toChar (f: int) : char =
        char (firstChar + f)

    /// Converts a character ('a'–'h') to a File.
    let fromChar (c: char) : int =
        int c - firstChar

module Rank =
    let [<Literal>] R1 = 0
    let [<Literal>] R2 = 1
    let [<Literal>] R3 = 2
    let [<Literal>] R4 = 3
    let [<Literal>] R5 = 4
    let [<Literal>] R6 = 5
    let [<Literal>] R7 = 6
    let [<Literal>] R8 = 7

    // Must NOT be private if inline functions use it
    let firstChar = int '1'

    /// Converts a Rank to its character representation ('1'–'8').
    let toChar (r: int) : char =
        char (firstChar + int r)

    /// Converts a character ('1'–'8') to a Rank.
    let fromChar (c: char) : int =
        int c - firstChar

module Square =

    /// Converts a File and Rank to a Square.
    let ofFileRank (f: int) (r: int) : int =
        (r <<< 3) + f
        // same as r*8 + f, but uses bit shift

    /// Gets the File of a Square.
    let file (sq: int) : int =
        sq &&& 7

    /// Gets the Rank of a Square.
    let rank (sq: int) : int =
        sq >>> 3

    /// Converts a Square to algebraic notation (e.g. "e4").
    let toString (sq: int) : string =
        let f = file sq |> File.toChar
        let r = rank sq |> Rank.toChar
        string f + string r

    /// Converts algebraic notation (e.g. "d4") to a Square.
    let fromString (s: string) : int =
        if s.Length <> 2 then
            invalidArg "s" $"Invalid square string: {s}"
        let f = File.fromChar s[0]
        let r = Rank.fromChar s[1]
        ofFileRank f r

    /// Checks if a file/rank pair is on the board.
    let isOnBoard (f: int) (r: int) =
        uint f <= 7u && uint r <= 7u

module PieceType =
    let [<Literal>] Pawn = 0
    let [<Literal>] Knight = 1
    let [<Literal>] Bishop = 2
    let [<Literal>] Rook = 3
    let [<Literal>] Queen = 4
    let [<Literal>] King = 5

    // Precomputed char table for speed
    let chars = [| 'p'; 'n'; 'b'; 'r'; 'q'; 'k' |]

    /// Converts a PieceType to its character representation ('p'..'k').
    let toChar (pt: int) : char =
        chars[pt]

    /// Converts a character to a PieceType.
    let fromChar (c: char) : int =
        match c with
        | 'p' | 'P' -> Pawn
        | 'n' | 'N' -> Knight
        | 'b' | 'B' -> Bishop
        | 'r' | 'R' -> Rook
        | 'q' | 'Q' -> Queen
        | 'k' | 'K' -> King
        | _ -> invalidArg "c" $"Invalid piece type char: {c}"

module Piece =
    let [<Literal>] WPawn = 0
    let [<Literal>] WKnight = 1
    let [<Literal>] WBishop = 2
    let [<Literal>] WRook = 3
    let [<Literal>] WQueen = 4
    let [<Literal>] WKing = 5
    let [<Literal>] BPawn = 8
    let [<Literal>] BKnight = 9
    let [<Literal>] BBishop = 10
    let [<Literal>] BRook = 12
    let [<Literal>] BQueen = 12
    let [<Literal>] BKing = 13

    let colour (p: int) : int =
        int p >>> 3

    let kind (p: int) : int =
        int p &&& 0b111

    let toChar (p: int) : char =
        let c = PieceType.toChar (kind p)
        if colour p = Colour.White then Char.ToUpper c else c

    let fromChar (c: char) : int =
        let col = if Char.IsUpper c then Colour.White else Colour.Black
        let kind = PieceType.fromChar c
        (col <<< 3) ||| kind

module CastlingRights =
    let [<Literal>] None = 0b0000
    let [<Literal>] WK   = 0b0001
    let [<Literal>] WQ   = 0b0010
    let [<Literal>] BK   = 0b0100
    let [<Literal>] BQ   = 0b1000
    
    let [<Literal>] White = 0b0011 // (WK | WQ)
    let [<Literal>] Black = 0b1100 // (BK | BQ)
    let [<Literal>] All   = 0b1111

    let fromString (s: string) =
        if s = "-" then None
        else
            (if s.Contains "K" then WK else None)
            ||| (if s.Contains "Q" then WQ else None)
            ||| (if s.Contains "k" then BK else None)
            ||| (if s.Contains "q" then BQ else None)

    let toString (rights: int) =
        if rights = None then "-"
        else
            let k = if rights &&& WK <> 0 then "K" else ""
            let q = if rights &&& WQ <> 0 then "Q" else ""
            let bk = if rights &&& BK <> 0 then "k" else ""
            let bq = if rights &&& BQ <> 0 then "q" else ""
            k + q + bk + bq

module Move =
    let [<Literal>] Quiet = 0
    let [<Literal>] Capture = 1
    let [<Literal>] EnPassant = 2
    let [<Literal>] KingsideCastling = 3
    let [<Literal>] QueensideCastling = 4
    let [<Literal>] Promotion = 5

    let create (fromSq: int, toSq: int, kind: int, promo: int) = 
        fromSq ||| (toSq <<< 6) ||| (kind <<< 12) ||| (promo <<< 16)

    let fromSq (m: int) =
        m &&& 63

    let toSq (m: int) =
        (m >>> 6) &&& 63

    let kind (m: int) =
        (m >>> 12) &&& 15

    let promo (m: int) =
        (m >>> 16) &&& 15

    let toUci (m: int) =
        let baseStr =
            Square.toString (fromSq m) +
            Square.toString (toSq m)

        match kind m with
        | 5 ->   // promotion
            let pt = promo m
            baseStr + (PieceType.toChar pt |> Char.ToLower |> string)
        | _ ->
            baseStr
