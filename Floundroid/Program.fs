open System


/// Represents the side to move or the colour of a piece.
type Colour =
    | White
    | Black

module Colour =
    let toChar = function
        | White -> 'w'
        | Black -> 'b'

    let fromChar = function
        | 'w' | 'W' -> White
        | 'b' | 'B' -> Black
        | c -> failwithf "Invalid colour char: %c" c
    
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

/// Represents the full state of a chess position.
/// This is equivalent to GameState in most engines.
type Board =
    { Pieces          : Map<Square, Piece>
      SideToMove      : Colour
      CastlingRights  : CastlingRights
      EnPassantSquare : Square option
      HalfmoveClock   : int
      FullmoveNumber  : int }

module Board =

    /// Create an empty board (no pieces, no rights).
    let empty =
        { Pieces = Map.empty
          SideToMove = White
          CastlingRights = CastlingRights.none
          EnPassantSquare = None
          HalfmoveClock = 0
          FullmoveNumber = 1 }

    /// Try to get the piece on a given square.
    let tryGetPiece (b: Board) (sq: Square) =
        b.Pieces |> Map.tryFind sq

    /// Check if a square is occupied.
    let isOccupied (b: Board) (sq: Square) =
        b.Pieces |> Map.containsKey sq

    /// Place or remove a piece on a square.
    let setPiece (b: Board) (sq: Square) (pieceOpt: Piece option) =
        match pieceOpt with
        | Some piece -> { b with Pieces = b.Pieces.Add(sq, piece) }
        | None -> { b with Pieces = b.Pieces.Remove sq }

    /// Remove a piece from a square.
    let removePiece (b: Board) (sq: Square) =
        { b with Pieces = b.Pieces.Remove sq }

    /// Move a piece from one square to another (no legality checks).
    let applyMove (m: Move) (b: Board) =
        match tryGetPiece b m.From with
        | None ->
            invalidArg "m" $"No piece on {Square.toString m.From}"

        | Some piece ->
            let b1 = removePiece b m.From
            let b2 =
                match m.Kind with
                | Promotion pt ->
                    let promoted = { Colour = piece.Colour; Kind = pt }
                    setPiece b1 m.To (Some promoted)
                | _ ->
                    setPiece b1 m.To (Some piece)

            let nextSide = Colour.opposite b.SideToMove

            { b2 with
                SideToMove = nextSide
                EnPassantSquare = None
                HalfmoveClock =
                    match piece.Kind, m.Kind with
                    | Pawn, _ -> 0
                    | _, Capture -> 0
                    | _ -> b.HalfmoveClock + 1
                FullmoveNumber =
                    if b.SideToMove = Black then b.FullmoveNumber + 1
                    else b.FullmoveNumber }








module CommandParser =
    let tokenize (line: string) =
        line.Split(' ', System.StringSplitOptions.RemoveEmptyEntries)
        |> Array.toList
open CommandParser

module UciProtocol =
    let printId () =
        printfn "id name Floundroid"
        printfn "id author Phil Brooks"

    let printUciOk () =
        printfn "uciok"
open UciProtocol

module UciLoop =
    let rec run () =
        let line = Console.ReadLine()
        match line with
        | null -> ()
        | cmd ->
            match tokenize cmd with
            | "uci"::_ ->
                printId ()
                printUciOk ()
            | "isready"::_ ->
                printfn "readyok"
            | "quit"::_ -> ()
            | _ -> ()
            run ()


[<EntryPoint>]
let main _ =
    UciLoop.run()
    0
