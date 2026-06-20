module Types

open System
open System.Text

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

type PieceType =
    | Pawn
    | Knight
    | Bishop
    | Rook
    | Queen
    | King

module PieceType =
    /// Converts a PieceType to its character representation ('p', 'n', 'b', 'r', 'q', 'k').
    let toChar =
        function
        | Pawn -> 'p'
        | Knight -> 'n'
        | Bishop -> 'b'
        | Rook -> 'r'
        | Queen -> 'q'
        | King -> 'k'

    /// Converts a character to a PieceType.
    let fromChar =
        function
        | 'p'
        | 'P' -> Pawn
        | 'n'
        | 'N' -> Knight
        | 'b'
        | 'B' -> Bishop
        | 'r'
        | 'R' -> Rook
        | 'q'
        | 'Q' -> Queen
        | 'k'
        | 'K' -> King
        | c -> invalidArg "c" $"{c}"

/// Bitboards are 64-bit unsigned integers where each bit represents a square.
/// Bit 0 is a1, Bit 7 is h1, Bit 63 is h8.
type Bitboard = uint64

module Bitboard =
    let empty: Bitboard = 0uL
    let all: Bitboard = 0xFFFFFFFFFFFFFFFFuL

    /// Sets the bit at the given square.
    let inline set (sq: Square) (bb: Bitboard) : Bitboard = bb ||| (1uL <<< sq)

    /// Clears the bit at the given square.
    let inline clear (sq: Square) (bb: Bitboard) : Bitboard = bb &&& ~~~(1uL <<< sq)

    /// Checks if a square is set.
    let inline contains (sq: Square) (bb: Bitboard) : bool = (bb &&& (1uL <<< sq)) <> 0uL

    /// Returns the number of set bits (population count).
    let inline count (bb: Bitboard) : int =
        System.Numerics.BitOperations.PopCount(bb)

    /// Returns the index of the least significant bit (0-63) and clears it from the bitboard.
    /// This is a high-performance way to iterate through pieces.
    let inline popLsb (bb: byref<Bitboard>) : Square =
        let lsb = System.Numerics.BitOperations.TrailingZeroCount(bb)
        bb <- bb &&& (bb - 1uL)
        lsb

    /// Visualizes the bitboard as an 8x8 grid for debugging.
    let toString (bb: Bitboard) =
        let sb = StringBuilder()

        for r in 7..-1..0 do
            sb.Append(sprintf "%d " (r + 1)) |> ignore

            for f in 0..7 do
                let sq = r * 8 + f
                sb.Append(if contains sq bb then "1 " else ". ") |> ignore

            sb.Append("\n") |> ignore

        sb.Append("  a b c d e f g h").ToString()

/// A Piece consists of a Colour and a PieceType.
type Piece = { Colour: Colour; Kind: PieceType }

module Piece =
    /// Converts a Piece to its character representation (uppercase for White, lowercase for Black).
    let toChar (p: Piece) =
        let c = PieceType.toChar p.Kind in if p.Colour = Colour.White then Char.ToUpper c else c

    /// Converts a character to a Piece, determining colour from case (uppercase = White, lowercase = Black).
    let fromChar c =
        { Colour = (if Char.IsUpper c then Colour.White else Colour.Black)
          Kind = PieceType.fromChar c }

/// Castling rights are represented as a record with four boolean fields.
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

    /// Converts a string representation of castling rights to a CastlingRights value.
    let fromString (s: string) =
        if s = "-" then
            none
        else
            { WhiteKingSide = s.Contains "K"
              WhiteQueenSide = s.Contains "Q"
              BlackKingSide = s.Contains "k"
              BlackQueenSide = s.Contains "q" }

    /// Converts a CastlingRights value to its string representation.
    let toString cr =
        let sb = StringBuilder()

        if cr.WhiteKingSide then
            sb.Append("K") |> ignore

        if cr.WhiteQueenSide then
            sb.Append("Q") |> ignore

        if cr.BlackKingSide then
            sb.Append("k") |> ignore

        if cr.BlackQueenSide then
            sb.Append("q") |> ignore

        if sb.Length = 0 then "-" else sb.ToString()

/// Move kinds represent the different types of moves in chess, including quiet moves, captures, promotions, en passant, and castling.
type MoveKind =
    | Quiet
    | Capture
    | Promotion of PieceType
    | EnPassant
    | CastleKingSide
    | CastleQueenSide

/// A Move consists of a source square, a destination square, and a MoveKind indicating the type of move.
type Move =
    { From: Square
      To: Square
      Kind: MoveKind }

module Move =
    /// Converts a Move to its UCI string representation.
    let toUci (m: Move) =
        let baseStr = Square.toString m.From + Square.toString m.To

        match m.Kind with
        | Promotion pt -> baseStr + (PieceType.toChar pt |> string)
        | _ -> baseStr

    /// Converts a UCI string representation of a move to a Move value.
    let fromUci (s: string) =
        if s.Length < 4 then
            invalidArg "s" "UCI move string too short"

        let fromSq = Square.fromString (s.Substring(0, 2))
        let toSq = Square.fromString (s.Substring(2, 2))

        let kind =
            if s.Length = 5 then
                Promotion(PieceType.fromChar s.[4])
            else
                Quiet

        { From = fromSq
          To = toSq
          Kind = kind }

          
/// A collection of bitboards representing all pieces on the board.
type BitboardSet =
    { WhitePawns: Bitboard
      WhiteKnights: Bitboard
      WhiteBishops: Bitboard
      WhiteRooks: Bitboard
      WhiteQueens: Bitboard
      WhiteKings: Bitboard
      BlackPawns: Bitboard
      BlackKnights: Bitboard
      BlackBishops: Bitboard
      BlackRooks: Bitboard
      BlackQueens: Bitboard
      BlackKings: Bitboard
      // Combined layers
      WhiteTotal: Bitboard
      BlackTotal: Bitboard
      Occupancy: Bitboard }

module BitboardSet =
    let empty =
        { WhitePawns = 0uL
          WhiteKnights = 0uL
          WhiteBishops = 0uL
          WhiteRooks = 0uL
          WhiteQueens = 0uL
          WhiteKings = 0uL
          BlackPawns = 0uL
          BlackKnights = 0uL
          BlackBishops = 0uL
          BlackRooks = 0uL
          BlackQueens = 0uL
          BlackKings = 0uL
          WhiteTotal = 0uL
          BlackTotal = 0uL
          Occupancy = 0uL }

    /// Converts a Piece Map into a BitboardSet.
    let fromMap (pieces: Map<Square, Piece>) =
        let mutable bbs = empty

        for (KeyValue(sq, p)) in pieces do
            let bit = 1uL <<< sq

            match p.Colour, p.Kind with
            | Colour.White, Pawn ->
                bbs <-
                    { bbs with
                        WhitePawns = bbs.WhitePawns ||| bit }
            | Colour.White, Knight ->
                bbs <-
                    { bbs with
                        WhiteKnights = bbs.WhiteKnights ||| bit }
            | Colour.White, Bishop ->
                bbs <-
                    { bbs with
                        WhiteBishops = bbs.WhiteBishops ||| bit }
            | Colour.White, Rook ->
                bbs <-
                    { bbs with
                        WhiteRooks = bbs.WhiteRooks ||| bit }
            | Colour.White, Queen ->
                bbs <-
                    { bbs with
                        WhiteQueens = bbs.WhiteQueens ||| bit }
            | Colour.White, King ->
                bbs <-
                    { bbs with
                        WhiteKings = bbs.WhiteKings ||| bit }
            | Colour.Black, Pawn ->
                bbs <-
                    { bbs with
                        BlackPawns = bbs.BlackPawns ||| bit }
            | Colour.Black, Knight ->
                bbs <-
                    { bbs with
                        BlackKnights = bbs.BlackKnights ||| bit }
            | Colour.Black, Bishop ->
                bbs <-
                    { bbs with
                        BlackBishops = bbs.BlackBishops ||| bit }
            | Colour.Black, Rook ->
                bbs <-
                    { bbs with
                        BlackRooks = bbs.BlackRooks ||| bit }
            | Colour.Black, Queen ->
                bbs <-
                    { bbs with
                        BlackQueens = bbs.BlackQueens ||| bit }
            | Colour.Black, King ->
                bbs <-
                    { bbs with
                        BlackKings = bbs.BlackKings ||| bit }
            | _ -> invalidArg "p" $"Invalid colour/kind: %A{p}"
 
        let whiteTotal =
            bbs.WhitePawns
            ||| bbs.WhiteKnights
            ||| bbs.WhiteBishops
            ||| bbs.WhiteRooks
            ||| bbs.WhiteQueens
            ||| bbs.WhiteKings

        let blackTotal =
            bbs.BlackPawns
            ||| bbs.BlackKnights
            ||| bbs.BlackBishops
            ||| bbs.BlackRooks
            ||| bbs.BlackQueens
            ||| bbs.BlackKings

        { bbs with
            WhiteTotal = whiteTotal
            BlackTotal = blackTotal
            Occupancy = whiteTotal ||| blackTotal }

    /// Identifies the piece (if any) at a specific square using bitboards.
    let getPieceAt (sq: Square) (bbs: BitboardSet) : Piece option =
        let bit = 1uL <<< sq

        if (bbs.Occupancy &&& bit) = 0uL then
            None
        else
            let color = if (bbs.WhiteTotal &&& bit) <> 0uL then Colour.White else Colour.Black

            let kind =
                if (bbs.WhitePawns ||| bbs.BlackPawns) &&& bit <> 0uL then
                    Pawn
                elif (bbs.WhiteKnights ||| bbs.BlackKnights) &&& bit <> 0uL then
                    Knight
                elif (bbs.WhiteBishops ||| bbs.BlackBishops) &&& bit <> 0uL then
                    Bishop
                elif (bbs.WhiteRooks ||| bbs.BlackRooks) &&& bit <> 0uL then
                    Rook
                elif (bbs.WhiteQueens ||| bbs.BlackQueens) &&& bit <> 0uL then
                    Queen
                else
                    King

            Some { Colour = color; Kind = kind }

    /// A helper to flip a piece on/off. Essential for incremental updates.
    let togglePiece (p: Piece) (sq: Square) (bbs: BitboardSet) =
        let bit = 1uL <<< sq

        let newBbs =
            match p.Colour, p.Kind with
            | Colour.White, Pawn ->
                { bbs with
                    WhitePawns = bbs.WhitePawns ^^^ bit }
            | Colour.White, Knight ->
                { bbs with
                    WhiteKnights = bbs.WhiteKnights ^^^ bit }
            | Colour.White, Bishop ->
                { bbs with
                    WhiteBishops = bbs.WhiteBishops ^^^ bit }
            | Colour.White, Rook ->
                { bbs with
                    WhiteRooks = bbs.WhiteRooks ^^^ bit }
            | Colour.White, Queen ->
                { bbs with
                    WhiteQueens = bbs.WhiteQueens ^^^ bit }
            | Colour.White, King ->
                { bbs with
                    WhiteKings = bbs.WhiteKings ^^^ bit }
            | Colour.Black, Pawn ->
                { bbs with
                    BlackPawns = bbs.BlackPawns ^^^ bit }
            | Colour.Black, Knight ->
                { bbs with
                    BlackKnights = bbs.BlackKnights ^^^ bit }
            | Colour.Black, Bishop ->
                { bbs with
                    BlackBishops = bbs.BlackBishops ^^^ bit }
            | Colour.Black, Rook ->
                { bbs with
                    BlackRooks = bbs.BlackRooks ^^^ bit }
            | Colour.Black, Queen ->
                { bbs with
                    BlackQueens = bbs.BlackQueens ^^^ bit }
            | Colour.Black, King ->
                { bbs with
                    BlackKings = bbs.BlackKings ^^^ bit }
            | _ -> invalidArg "p" $"Invalid colour/kind: %A{p}"

        let whiteTotal =
            newBbs.WhitePawns
            ||| newBbs.WhiteKnights
            ||| newBbs.WhiteBishops
            ||| newBbs.WhiteRooks
            ||| newBbs.WhiteQueens
            ||| newBbs.WhiteKings

        let blackTotal =
            newBbs.BlackPawns
            ||| newBbs.BlackKnights
            ||| newBbs.BlackBishops
            ||| newBbs.BlackRooks
            ||| newBbs.BlackQueens
            ||| newBbs.BlackKings

        { newBbs with
            WhiteTotal = whiteTotal
            BlackTotal = blackTotal
            Occupancy = whiteTotal ||| blackTotal }

    /// Returns a sequence of all (Square, Piece) pairs currently on the board.
    let allPieces (bbs: BitboardSet) =
        seq {
            let mutable occ = bbs.Occupancy

            while occ <> 0uL do
                let sq = Bitboard.popLsb &occ
                yield (sq, getPieceAt sq bbs |> Option.get)
        }
