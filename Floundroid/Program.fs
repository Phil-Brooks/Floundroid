module Floundroid

open System
open System.Text
open System.Threading

/// Colours are represented as a discriminated union with two cases: White and Black.
type Colour =
    | White
    | Black

module Colour =
    /// Converts a Colour to its character representation ('w' for White, 'b' for Black).
    let toChar =
        function
        | White -> 'w'
        | Black -> 'b'

    /// Converts a character to a Colour ('w' for White, 'b' for Black).
    let fromChar =
        function
        | 'w'
        | 'W' -> White
        | 'b'
        | 'B' -> Black
        | c -> failwithf "Invalid colour char: %c" c

    /// Returns the opposite colour.
    let opposite =
        function
        | White -> Black
        | Black -> White

/// Files are represented as integers from 0 to 7, where 0 = file a and 7 = file h.
type File =
    | A
    | B
    | C
    | D
    | E
    | F
    | G
    | H

module File =
    /// Converts a File to its integer representation (0-7).
    let toInt =
        function
        | A -> 0
        | B -> 1
        | C -> 2
        | D -> 3
        | E -> 4
        | F -> 5
        | G -> 6
        | H -> 7

    /// Converts an integer to a File (0-7).
    let fromInt =
        function
        | 0 -> A
        | 1 -> B
        | 2 -> C
        | 3 -> D
        | 4 -> E
        | 5 -> F
        | 6 -> G
        | 7 -> H
        | i -> invalidArg "i" $"File index {i} out of range (0-7)"

    /// Converts a File to its character representation ('a'-'h').
    let toChar f = "abcdefgh".[toInt f]

    /// Converts a character to a File ('a'-'h').
    let fromChar =
        function
        | 'a' -> A
        | 'b' -> B
        | 'c' -> C
        | 'd' -> D
        | 'e' -> E
        | 'f' -> F
        | 'g' -> G
        | 'h' -> H
        | c -> invalidArg "c" $"{c}"

/// Ranks are represented as integers from 0 to 7, where 0 = rank 1 and 7 = rank 8.
type Rank =
    | R1
    | R2
    | R3
    | R4
    | R5
    | R6
    | R7
    | R8

module Rank =
    /// Converts a Rank to its integer representation (0-7).
    let toInt =
        function
        | R1 -> 0
        | R2 -> 1
        | R3 -> 2
        | R4 -> 3
        | R5 -> 4
        | R6 -> 5
        | R7 -> 6
        | R8 -> 7

    /// Converts an integer to a Rank (0-7).
    let fromInt =
        function
        | 0 -> R1
        | 1 -> R2
        | 2 -> R3
        | 3 -> R4
        | 4 -> R5
        | 5 -> R6
        | 6 -> R7
        | 7 -> R8
        | i -> invalidArg "i" $"Rank index {i} out of range (0-7)"

    /// Converts a Rank to its character representation ('1'-'8').
    let toChar r = "12345678".[toInt r]

    /// Converts a character to a Rank ('1'-'8').
    let fromChar =
        function
        | '1' -> R1
        | '2' -> R2
        | '3' -> R3
        | '4' -> R4
        | '5' -> R5
        | '6' -> R6
        | '7' -> R7
        | '8' -> R8
        | c -> invalidArg "c" $"{c}"

/// Squares are represented as integers from 0 to 63, where 0 = a1, 1 = b1, ..., 63 = h8.
type Square = int

module Square =
    /// Converts a File and Rank to a Square.
    let ofFileRank (f: File) (r: Rank) : Square = Rank.toInt r * 8 + File.toInt f

    /// Gets the File of a Square.
    let file (sq: Square) : File = File.fromInt (sq % 8)

    /// Gets the Rank of a Square.
    let rank (sq: Square) : Rank = Rank.fromInt (sq / 8)

    /// Converts a Square to its string representation.
    let toString (sq: Square) : string =
        $"{File.toChar (file sq)}{Rank.toChar (rank sq)}"

    /// Converts a string representation of a square (e.g., "d4") to a Square.
    let fromString (s: string) : Square =
        ofFileRank (File.fromChar s.[0]) (Rank.fromChar s.[1])

    /// Checks if a square is on the board.
    let isOnBoard (f: int) (r: int) = f >= 0 && f < 8 && r >= 0 && r < 8

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

/// A Piece consists of a Colour and a PieceType.
type Piece = { Colour: Colour; Kind: PieceType }

module Piece =
    /// Converts a Piece to its character representation (uppercase for White, lowercase for Black).
    let toChar (p: Piece) =
        let c = PieceType.toChar p.Kind in if p.Colour = White then Char.ToUpper c else c

    /// Converts a character to a Piece, determining colour from case (uppercase = White, lowercase = Black).
    let fromChar c =
        { Colour = (if Char.IsUpper c then White else Black)
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
            | White, Pawn ->
                bbs <-
                    { bbs with
                        WhitePawns = bbs.WhitePawns ||| bit }
            | White, Knight ->
                bbs <-
                    { bbs with
                        WhiteKnights = bbs.WhiteKnights ||| bit }
            | White, Bishop ->
                bbs <-
                    { bbs with
                        WhiteBishops = bbs.WhiteBishops ||| bit }
            | White, Rook ->
                bbs <-
                    { bbs with
                        WhiteRooks = bbs.WhiteRooks ||| bit }
            | White, Queen ->
                bbs <-
                    { bbs with
                        WhiteQueens = bbs.WhiteQueens ||| bit }
            | White, King ->
                bbs <-
                    { bbs with
                        WhiteKings = bbs.WhiteKings ||| bit }
            | Black, Pawn ->
                bbs <-
                    { bbs with
                        BlackPawns = bbs.BlackPawns ||| bit }
            | Black, Knight ->
                bbs <-
                    { bbs with
                        BlackKnights = bbs.BlackKnights ||| bit }
            | Black, Bishop ->
                bbs <-
                    { bbs with
                        BlackBishops = bbs.BlackBishops ||| bit }
            | Black, Rook ->
                bbs <-
                    { bbs with
                        BlackRooks = bbs.BlackRooks ||| bit }
            | Black, Queen ->
                bbs <-
                    { bbs with
                        BlackQueens = bbs.BlackQueens ||| bit }
            | Black, King ->
                bbs <-
                    { bbs with
                        BlackKings = bbs.BlackKings ||| bit }

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
            let color = if (bbs.WhiteTotal &&& bit) <> 0uL then White else Black

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
            | White, Pawn ->
                { bbs with
                    WhitePawns = bbs.WhitePawns ^^^ bit }
            | White, Knight ->
                { bbs with
                    WhiteKnights = bbs.WhiteKnights ^^^ bit }
            | White, Bishop ->
                { bbs with
                    WhiteBishops = bbs.WhiteBishops ^^^ bit }
            | White, Rook ->
                { bbs with
                    WhiteRooks = bbs.WhiteRooks ^^^ bit }
            | White, Queen ->
                { bbs with
                    WhiteQueens = bbs.WhiteQueens ^^^ bit }
            | White, King ->
                { bbs with
                    WhiteKings = bbs.WhiteKings ^^^ bit }
            | Black, Pawn ->
                { bbs with
                    BlackPawns = bbs.BlackPawns ^^^ bit }
            | Black, Knight ->
                { bbs with
                    BlackKnights = bbs.BlackKnights ^^^ bit }
            | Black, Bishop ->
                { bbs with
                    BlackBishops = bbs.BlackBishops ^^^ bit }
            | Black, Rook ->
                { bbs with
                    BlackRooks = bbs.BlackRooks ^^^ bit }
            | Black, Queen ->
                { bbs with
                    BlackQueens = bbs.BlackQueens ^^^ bit }
            | Black, King ->
                { bbs with
                    BlackKings = bbs.BlackKings ^^^ bit }

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

module SlidingAttackGen =
    /// Generates a bitboard of all squares a Bishop attacks from a given square, 
    /// accounting for blockers. This is the "slow" version used for table init.
    let bishopAttacks (sq: Square) (blockers: Bitboard) =
        let mutable attacks = 0uL
        let r, f = sq / 8, sq % 8
        let directions = [| (1, 1); (1, -1); (-1, 1); (-1, -1) |]
        
        for (dr, df) in directions do
            let mutable nr, nf = r + dr, f + df
            let mutable blocked = false
            while nr >= 0 && nr < 8 && nf >= 0 && nf < 8 && not blocked do
                let targetSq = nr * 8 + nf
                let bit = 1uL <<< targetSq
                attacks <- attacks ||| bit
                if (blockers &&& bit) <> 0uL then 
                    blocked <- true // Hit a piece, can't go further
                nr <- nr + dr
                nf <- nf + df
        attacks

    /// Generates a bitboard of all squares a Rook attacks from a given square, 
    /// accounting for blockers. This is the "slow" version used for table init.
    let rookAttacks (sq: Square) (blockers: Bitboard) =
        let mutable attacks = 0uL
        let r, f = sq / 8, sq % 8
        let directions = [| (1, 0); (-1, 0); (0, 1); (0, -1) |]
        
        for (dr, df) in directions do
            let mutable nr, nf = r + dr, f + df
            let mutable blocked = false
            while nr >= 0 && nr < 8 && nf >= 0 && nf < 8 && not blocked do
                let targetSq = nr * 8 + nf
                let bit = 1uL <<< targetSq
                attacks <- attacks ||| bit
                if (blockers &&& bit) <> 0uL then 
                    blocked <- true
                nr <- nr + dr
                nf <- nf + df
        attacks

    /// The "Mask" for Magic Bitboards: This excludes the very last square 
    /// on the edge of the board for every ray, because a blocker on the 
    /// edge doesn't change the attack bitboard.
    let bishopMask (sq: Square) =
        let mutable mask = 0uL
        let r, f = sq / 8, sq % 8
        let directions = [| (1, 1); (1, -1); (-1, 1); (-1, -1) |]
        for (dr, df) in directions do
            let mutable nr, nf = r + dr, f + df
            // Notice the check is > 0 and < 7 (excludes edges)
            while nr > 0 && nr < 7 && nf > 0 && nf < 7 do
                mask <- mask ||| (1uL <<< (nr * 8 + nf))
                nr <- nr + dr
                nf <- nf + df
        mask

    let rookMask (sq: Square) =
        let mutable mask = 0uL
        let r, f = sq / 8, sq % 8
        // Vertical
        for nr in r + 1 .. 6 do mask <- mask ||| (1uL <<< (nr * 8 + f))
        for nr in r - 1 .. -1 .. 1 do mask <- mask ||| (1uL <<< (nr * 8 + f))
        // Horizontal
        for nf in f + 1 .. 6 do mask <- mask ||| (1uL <<< (r * 8 + nf))
        for nf in f - 1 .. -1 .. 1 do mask <- mask ||| (1uL <<< (r * 8 + nf))
        mask

module Magic =
    /// Represents a magic entry for a square, containing the mask and the offset into the attack table.
    type MagicEntry = { Mask: Bitboard; Offset: int }

    // Table size: 64 squares * 4096 max patterns per square = 262,144 entries
    // This uses about 2MB of RAM, which is perfectly fine.
    let bishopTable = Array.zeroCreate<Bitboard> (64 * 4096)
    let rookTable = Array.zeroCreate<Bitboard> (64 * 4096)
    let bishopEntries = Array.zeroCreate<MagicEntry> 64
    let rookEntries = Array.zeroCreate<MagicEntry> 64

    /// This maps an occupancy bitboard to a unique index from 0 to 2^bits-1
    /// It is essentially a manual "PEXT" instruction.
    let getIndex (occ: Bitboard) (mask: Bitboard) : int =
        let mutable index = 0
        let mutable tempMask = mask
        let bits = Bitboard.count mask
        for i in 0 .. bits - 1 do
            let bitIdx = System.Numerics.BitOperations.TrailingZeroCount(tempMask)
            tempMask <- tempMask &&& (tempMask - 1uL)
            if (occ &&& (1uL <<< bitIdx)) <> 0uL then
                index <- index ||| (1 <<< i)
        index

    /// Generates every possible blocker pattern for a mask (reverse of getIndex)
    let private getBlockers (index: int) (mask: Bitboard) : Bitboard =
        let mutable blockers = 0uL
        let mutable tempMask = mask
        let bits = Bitboard.count mask
        for i in 0 .. bits - 1 do
            let bitIdx = System.Numerics.BitOperations.TrailingZeroCount(tempMask)
            tempMask <- tempMask &&& (tempMask - 1uL)
            if (index &&& (1 <<< i)) <> 0 then
                blockers <- blockers ||| (1uL <<< bitIdx)
        blockers

    /// Initializes the sliding attack tables for bishops and rooks.
    let init () =
        printfn "info string Initializing Sliding Attack Tables..."
        for sq in 0 .. 63 do
            // Bishops
            let bMask = SlidingAttackGen.bishopMask sq
            let bBits = Bitboard.count bMask
            bishopEntries.[sq] <- { Mask = bMask; Offset = sq * 4096 }
            for i in 0 .. (1 <<< bBits) - 1 do
                let blockers = getBlockers i bMask
                let tableIdx = (sq * 4096) + i
                bishopTable.[tableIdx] <- SlidingAttackGen.bishopAttacks sq blockers

            // Rooks
            let rMask = SlidingAttackGen.rookMask sq
            let rBits = Bitboard.count rMask
            rookEntries.[sq] <- { Mask = rMask; Offset = sq * 4096 }
            for i in 0 .. (1 <<< rBits) - 1 do
                let blockers = getBlockers i rMask
                let tableIdx = (sq * 4096) + i
                rookTable.[tableIdx] <- SlidingAttackGen.rookAttacks sq blockers
        printfn "info string Sliding Attack Tables initialized."

module BitboardGen =
    /// Pre-calculated knight attacks for every square
    let knightAttacks = Array.zeroCreate<Bitboard> 64
    /// Pre-calculated king attacks for every square
    let kingAttacks = Array.zeroCreate<Bitboard> 64
    /// Pawn attacks: [Colour index 0=White, 1=Black, Square 0-63]
    let pawnAttacks = Array2D.zeroCreate<Bitboard> 2 64

    let private initializeLeapers () =
        for sq in 0..63 do
            let f, r = sq % 8, sq / 8

            // Knight Logic
            let mutable kbb = 0uL

            let knightOffsets =
                [ (1, 2); (1, -2); (-1, 2); (-1, -2); (2, 1); (2, -1); (-2, 1); (-2, -1) ]

            for (df, dr) in knightOffsets do
                let nf, nr = f + df, r + dr

                if nf >= 0 && nf < 8 && nr >= 0 && nr < 8 then
                    kbb <- kbb ||| (1uL <<< (nr * 8 + nf))

            knightAttacks.[sq] <- kbb

            // King Logic
            let mutable kingbb = 0uL

            for df in -1 .. 1 do
                for dr in -1 .. 1 do
                    if df <> 0 || dr <> 0 then
                        let nf, nr = f + df, r + dr

                        if nf >= 0 && nf < 8 && nr >= 0 && nr < 8 then
                            kingbb <- kingbb ||| (1uL <<< (nr * 8 + nf))

            kingAttacks.[sq] <- kingbb

        // Pawn Attack Logic
        for sq in 0..63 do
            let f, r = sq % 8, sq / 8

            // White Pawn Attacks (North-East, North-West)
            let mutable wPawnAttacks = 0uL

            if r < 7 then
                if f > 0 then
                    wPawnAttacks <- wPawnAttacks ||| (1uL <<< (sq + 7))

                if f < 7 then
                    wPawnAttacks <- wPawnAttacks ||| (1uL <<< (sq + 9))

            pawnAttacks.[0, sq] <- wPawnAttacks

            // Black Pawn Attacks (South-East, South-West)
            let mutable bPawnAttacks = 0uL

            if r > 0 then
                if f > 0 then
                    bPawnAttacks <- bPawnAttacks ||| (1uL <<< (sq - 9))

                if f < 7 then
                    bPawnAttacks <- bPawnAttacks ||| (1uL <<< (sq - 7))

            pawnAttacks.[1, sq] <- bPawnAttacks


    // Initialize the tables immediately
    do 
        initializeLeapers ()
        Magic.init ()

/// The Board type represents the state of a chess game, including piece placement, side to move, castling rights, en passant target square, and move clocks.
type Board =
    { Bitboards: BitboardSet // The new performance core
      SideToMove: Colour
      CastlingRights: CastlingRights
      EnPassantSquare: Square option
      HalfmoveClock: int
      FullmoveNumber: int }

module Attack =
    let isSquareAttacked (b: Board) (sq: Square) (attacker: Colour) =
        let bbs = b.Bitboards
        let them = attacker

        // 1. Pawn, Knight, King (Keep existing logic)
        let usIdx = if them = Black then 0 else 1
        let pawnAttackMask = BitboardGen.pawnAttacks.[usIdx, sq]
        let themPawns = if them = White then bbs.WhitePawns else bbs.BlackPawns

        if (pawnAttackMask &&& themPawns) <> 0uL then true
        else
            let knightAttackMask = BitboardGen.knightAttacks.[sq]
            let themKnights = if them = White then bbs.WhiteKnights else bbs.BlackKnights
            if (knightAttackMask &&& themKnights) <> 0uL then true
            else
                let kingAttackMask = BitboardGen.kingAttacks.[sq]
                let themKing = if them = White then bbs.WhiteKings else bbs.BlackKings
                if (kingAttackMask &&& themKing) <> 0uL then true
                else
                    // Inside Attack.isSquareAttacked, replace the sliding logic with:
                    let occ = bbs.Occupancy

                    // Bishop & Queen
                    let bEntry = Magic.bishopEntries.[sq]
                    let bIdx = bEntry.Offset + Magic.getIndex occ bEntry.Mask
                    let bishopAttacks = Magic.bishopTable.[bIdx]
                    let themBishops = if them = White then (bbs.WhiteBishops ||| bbs.WhiteQueens) else (bbs.BlackBishops ||| bbs.BlackQueens)

                    if (bishopAttacks &&& themBishops) <> 0uL then true
                    else
                        // Rook & Queen
                        let rEntry = Magic.rookEntries.[sq]
                        let rIdx = rEntry.Offset + Magic.getIndex occ rEntry.Mask
                        let rookAttacks = Magic.rookTable.[rIdx]
                        let themRooks = if them = White then (bbs.WhiteRooks ||| bbs.WhiteQueens) else (bbs.BlackRooks ||| bbs.BlackQueens)
    
                        (rookAttacks &&& themRooks) <> 0uL                    

// ... after module Piece or module CastlingRights ...

module Zobrist =

    /// Storage for all random keys used for hashing.
    type ZobristTable = {
        /// [colour (2)][pieceType (6)][square (64)]
        Pieces: uint64[,,]
        /// Key to XOR if it is Black to move
        SideToMove: uint64
        /// Keys for the 16 possible combinations of castling rights
        Castling: uint64[]
        /// Keys for the 8 possible files for an En Passant target
        EnPassantFile: uint64[]
    }

    /// Maps PieceType to an index 0-5
    let private pieceIdx = function
        | PieceType.Pawn -> 0 | PieceType.Knight -> 1 | PieceType.Bishop -> 2
        | PieceType.Rook -> 3 | PieceType.Queen -> 4  | PieceType.King -> 5

    /// Maps Colour to index 0-1
    let private colourIdx = function
        | Colour.White -> 0
        | Colour.Black -> 1

    /// Pre-calculates the table with a fixed seed for reproducibility.
    let private initializeTable () =
        let seed = 1010101 
        let rng = Random(seed)
        
        let next64 () =
            let buffer = Array.zeroCreate<byte> 8
            rng.NextBytes(buffer)
            BitConverter.ToUInt64(buffer, 0)

        let pieces = Array3D.init 2 6 64 (fun _ _ _ -> next64())
        let side = next64()
        let castling = Array.init 16 (fun _ -> next64())
        let ep = Array.init 8 (fun _ -> next64())

        { Pieces = pieces; SideToMove = side; Castling = castling; EnPassantFile = ep }

    /// The global lookup table for Zobrist keys.
    let Table = initializeTable()

    /// Gets the key for a specific piece on a square.
    let getPieceKey (pc: Piece) (sq: Square) =
        Table.Pieces.[colourIdx pc.Colour, pieceIdx pc.Kind, sq]

    /// Gets the key for a specific set of castling rights.
    let getCastlingKey (cr: CastlingRights) =
        let mutable index = 0
        if cr.WhiteKingSide  then index <- index ||| 1
        if cr.WhiteQueenSide then index <- index ||| 2
        if cr.BlackKingSide  then index <- index ||| 4
        if cr.BlackQueenSide then index <- index ||| 8
        Table.Castling.[index]

    /// Gets the key for an En Passant file (0-7).
    let getEnPassantKey (sq: Square option) =
        match sq with
        | Some s -> Table.EnPassantFile.[s % 8]
        | None -> 0UL

module Board =
    let empty =
        { Bitboards = BitboardSet.empty // Placeholder
          SideToMove = White
          CastlingRights = CastlingRights.none
          EnPassantSquare = None
          HalfmoveClock = 0
          FullmoveNumber = 1 }

    /// Tries to get a piece from a square (Source of truth: Bitboards).
    let tryGetPiece (b: Board) (sq: Square) = BitboardSet.getPieceAt sq b.Bitboards

    /// Checks if a square is occupied (Source of truth: Bitboards).
    let isOccupied (b: Board) (sq: Square) =
        (b.Bitboards.Occupancy &&& (1uL <<< sq)) <> 0uL

    /// Find the king square (Needed for check detection).
    let findKing (colour: Colour) (b: Board) =
        let mutable bb =
            if colour = White then
                b.Bitboards.WhiteKings
            else
                b.Bitboards.BlackKings

        if bb = 0uL then -1 else Bitboard.popLsb &bb

    /// Sets a piece on a square and updates bitboards (Source of truth: Bitboards).
    let setPiece (b: Board) (sq: Square) (pOpt: Piece option) =
        let mutable newBbs = b.Bitboards

        // 1. If there's already a piece at this square, we must toggle it OFF first
        match BitboardSet.getPieceAt sq b.Bitboards with
        | Some oldPiece -> newBbs <- BitboardSet.togglePiece oldPiece sq newBbs
        | None -> ()

        // 2. If we are setting a new piece, toggle it ON
        match pOpt with
        | Some p -> newBbs <- BitboardSet.togglePiece p sq newBbs
        | None -> ()

        { b with
            Bitboards = newBbs }

    /// Parses a FEN string and returns a Board record representing the position.
    let fromFen (fen: string) =
        let parts = fen.Split(' ')
        let rows = parts.[0].Split('/')
        let mutable bbs = BitboardSet.empty // Start with empty bitboards

        for r in 0..7 do
            let rank, file = 7 - r, ref 0
            for char in rows.[r] do
                if Char.IsDigit char then
                    file.Value <- file.Value + (int char - int '0')
                else
                    let sq = Square.ofFileRank (File.fromInt file.Value) (Rank.fromInt rank)
                    // Directly toggle the piece on the bitboard
                    bbs <- BitboardSet.togglePiece (Piece.fromChar char) sq bbs
                    file.Value <- file.Value + 1

        { Bitboards = bbs
          SideToMove = Colour.fromChar parts.[1].[0]
          CastlingRights = CastlingRights.fromString parts.[2]
          EnPassantSquare = if parts.[3] = "-" then None else Some(Square.fromString parts.[3])
          HalfmoveClock = int parts.[4]
          FullmoveNumber = int parts.[5] }    
 
    /// Converts a Board record to its FEN string representation.
    let toFen (b: Board) =
        let sb = StringBuilder()
        for r in 7..-1..0 do
            let mutable emptyCount = 0
            for f in 0..7 do
                let sq = Square.ofFileRank (File.fromInt f) (Rank.fromInt r)
                match tryGetPiece b sq with // This now uses Bitboards!
                | Some p ->
                    if emptyCount > 0 then sb.Append(emptyCount) |> ignore
                    emptyCount <- 0
                    sb.Append(Piece.toChar p) |> ignore
                | None -> emptyCount <- emptyCount + 1
            if emptyCount > 0 then sb.Append(emptyCount) |> ignore
            if r > 0 then sb.Append('/') |> ignore

        sprintf "%O %c %s %s %d %d"
            sb
            (Colour.toChar b.SideToMove)
            (CastlingRights.toString b.CastlingRights)
            (match b.EnPassantSquare with | Some s -> Square.toString s | None -> "-")
            b.HalfmoveClock
            b.FullmoveNumber

    /// <summary>
    /// Checks if a player is in check.
    /// </summary>
    /// <param name="colour">The colour of the player to check.</param>
    /// <param name="b">The current game state.</param>
    /// <returns>True if the player is in check, false otherwise.</returns>
    let isInCheckFor (colour: Colour) (b: Board) =
        let kingSq = findKing colour b
        if kingSq = -1 then false
        else Attack.isSquareAttacked b kingSq (Colour.opposite colour)
    
    /// Checks if the side to move is currently in check.
    let isInCheck (b: Board) = isInCheckFor b.SideToMove b

    /// <summary>
    /// Executes a move on the board and returns a new immutable board state.
    /// Updates castling rights, en passant targets, and move clocks.
    /// </summary>
    /// <param name="m">The validated move to apply.</param>
    /// <param name="b">The current game state.</param>
    /// <returns>A new Board record reflecting the post-move state.</returns>
    /// <summary>
    /// Executes a move, updating both Bitboards and the Piece Map.
    /// This is the final step before the Map is removed entirely.
    /// </summary>
    let applyMove (m: Move) (b: Board) =
        match tryGetPiece b m.From with
        | None -> b
        | Some piece ->
            let us, them = b.SideToMove, Colour.opposite b.SideToMove
            let mutable newBbs = b.Bitboards
            
            // 1. Remove moving piece from source
            newBbs <- BitboardSet.togglePiece piece m.From newBbs
            
            // 2. Handle standard capture at destination square
            match tryGetPiece b m.To with
            | Some victim ->
                newBbs <- BitboardSet.togglePiece victim m.To newBbs
            | None -> ()

            // 3. Handle Special Move Kinds (En Passant and Castling)
            match m.Kind with
            | EnPassant ->
                let capturedSq = if us = White then m.To - 8 else m.To + 8
                let epVictim = { Colour = them; Kind = Pawn }
                newBbs <- BitboardSet.togglePiece epVictim capturedSq newBbs
            | CastleKingSide ->
                let rR = if us = White then Rank.R1 else Rank.R8
                let rF, rT = Square.ofFileRank File.H rR, Square.ofFileRank File.F rR
                let rook = { Colour = us; Kind = Rook }
                newBbs <- BitboardSet.togglePiece rook rF newBbs
                newBbs <- BitboardSet.togglePiece rook rT newBbs
            | CastleQueenSide ->
                let rR = if us = White then Rank.R1 else Rank.R8
                let rF, rT = Square.ofFileRank File.A rR, Square.ofFileRank File.D rR
                let rook = { Colour = us; Kind = Rook }
                newBbs <- BitboardSet.togglePiece rook rF newBbs
                newBbs <- BitboardSet.togglePiece rook rT newBbs
            | _ -> ()

            // 4. Place the piece at the destination (handling promotion)
            let pieceToPlace =
                match m.Kind with
                | Promotion pt -> { Colour = us; Kind = pt }
                | _ -> piece

            newBbs <- BitboardSet.togglePiece pieceToPlace m.To newBbs
            
            // 5. Update Castling Rights
            let updateRights (cr: CastlingRights) =
                let mutable r = cr
                // Revoke if King moves
                if piece.Kind = King then
                    if us = White then
                        r <-
                            { r with
                                WhiteKingSide = false
                                WhiteQueenSide = false }
                    else
                        r <-
                            { r with
                                BlackKingSide = false
                                BlackQueenSide = false }

                // Helper to revoke if a specific corner square is involved (move or capture)
                let revokeForSquare sq cur =
                    match Square.file sq, Square.rank sq with
                    | File.A, Rank.R1 -> { cur with WhiteQueenSide = false }
                    | File.H, Rank.R1 -> { cur with WhiteKingSide = false }
                    | File.A, Rank.R8 -> { cur with BlackQueenSide = false }
                    | File.H, Rank.R8 -> { cur with BlackKingSide = false }
                    | _ -> cur

                r <- revokeForSquare m.From r // Moved from corner
                r <- revokeForSquare m.To r // Rook captured in corner
                r

            // 6. Set En Passant target square
            let nextEp =
                if
                    piece.Kind = Pawn
                    && Math.Abs(Rank.toInt (Square.rank m.From) - Rank.toInt (Square.rank m.To)) = 2
                then
                    Some(Square.ofFileRank (Square.file m.From) (if us = White then Rank.R3 else Rank.R6))
                else
                    None

            { b with
                Bitboards = newBbs
                SideToMove = them
                CastlingRights = updateRights b.CastlingRights
                EnPassantSquare = nextEp
                HalfmoveClock =
                    if piece.Kind = Pawn || isOccupied b m.To then
                        0
                    else
                        b.HalfmoveClock + 1
                FullmoveNumber =
                    if us = Black then
                        b.FullmoveNumber + 1
                    else
                        b.FullmoveNumber }

    /// Prints the board in a human-readable format.
    let prettyPrint (b: Board) =
        for r in 7..-1..0 do
            printf "%d " (r + 1)
            for f in 0..7 do
                // Use the bitboard-powered tryGetPiece
                match tryGetPiece b (Square.ofFileRank (File.fromInt f) (Rank.fromInt r)) with
                | Some p -> printf "%c " (Piece.toChar p)
                | None -> printf ". "
            printfn ""
        printfn "  a b c d e f g h"

module MoveGen =
    let dirs =
        Map
            [ Bishop, [ (1, 1); (1, -1); (-1, 1); (-1, -1) ]
              Rook, [ (1, 0); (-1, 0); (0, 1); (0, -1) ]
              Queen, [ (1, 1); (1, -1); (-1, 1); (-1, -1); (1, 0); (-1, 0); (0, 1); (0, -1) ]
              Knight, [ (1, 2); (1, -2); (-1, 2); (-1, -2); (2, 1); (2, -1); (-2, 1); (-2, -1) ]
              King, [ (1, 1); (1, -1); (-1, 1); (-1, -1); (1, 0); (-1, 0); (0, 1); (0, -1) ] ]

    // Gets all pseudo-legal moves for the current position using Bitboards.
    let getPseudoLegalMoves (b: Board) =
        let moves = ResizeArray<Move>()
        let us, them = b.SideToMove, Colour.opposite b.SideToMove

        // Use the bitboard iterator to find all our pieces
        for (sq, p) in BitboardSet.allPieces b.Bitboards do
            if p.Colour = us then
                let f, r = Square.file sq |> File.toInt, Square.rank sq |> Rank.toInt

                match p.Kind with
                | Pawn ->
                    let d = if us = White then 1 else -1

                    // 1. Single Push
                    let nr1 = r + d
                    if nr1 >= 0 && nr1 <= 7 then
                        let p1 = Square.ofFileRank (File.fromInt f) (Rank.fromInt nr1)
                        if not (Board.isOccupied b p1) then
                            // Promotion push
                            if nr1 = (if us = White then 7 else 0) then
                                for pt in [ Queen; Rook; Bishop; Knight ] do
                                    moves.Add({ From = sq; To = p1; Kind = Promotion pt })
                            else
                                moves.Add({ From = sq; To = p1; Kind = Quiet })

                            // 2. Double push from starting rank
                            if r = (if us = White then 1 else 6) then
                                let nr2 = r + 2 * d
                                let p2 = Square.ofFileRank (File.fromInt f) (Rank.fromInt nr2)
                                if not (Board.isOccupied b p2) then
                                    moves.Add({ From = sq; To = p2; Kind = Quiet })

                    // 3. Captures
                    for df in [ -1; 1 ] do
                        let nf, nr = f + df, r + d
                        if Square.isOnBoard nf nr then
                            let cap = Square.ofFileRank (File.fromInt nf) (Rank.fromInt nr)
                            match Board.tryGetPiece b cap with
                            | Some victim when victim.Colour = them ->
                                if nr = (if us = White then 7 else 0) then
                                    for pt in [ Queen; Rook; Bishop; Knight ] do
                                        moves.Add({ From = sq; To = cap; Kind = Promotion pt })
                                else
                                    moves.Add({ From = sq; To = cap; Kind = Capture })
                            | _ -> ()

                    // 4. En passant
                    match b.EnPassantSquare with
                    | Some ep ->
                        if abs (File.toInt (Square.file ep) - f) = 1 && Rank.toInt (Square.rank ep) = r + d then
                            moves.Add({ From = sq; To = ep; Kind = EnPassant })
                    | None -> ()

                | Knight ->
                    // Use high-speed Bitboard lookup
                    let mutable attacks = BitboardGen.knightAttacks.[sq]
                    while attacks <> 0uL do
                        let t = Bitboard.popLsb &attacks
                        match Board.tryGetPiece b t with
                        | Some target ->
                            if target.Colour = them then
                                moves.Add({ From = sq; To = t; Kind = Capture })
                        | None -> moves.Add({ From = sq; To = t; Kind = Quiet })

                | King ->
                    // Use high-speed Bitboard lookup
                    let mutable attacks = BitboardGen.kingAttacks.[sq]
                    while attacks <> 0uL do
                        let t = Bitboard.popLsb &attacks
                        match Board.tryGetPiece b t with
                        | Some target ->
                            if target.Colour = them then
                                moves.Add({ From = sq; To = t; Kind = Capture })
                        | None -> moves.Add({ From = sq; To = t; Kind = Quiet })

                    // Castling
                    let rnk, cr = (if us = White then 0 else 7), b.CastlingRights
                    if (us = White && cr.WhiteKingSide) || (us = Black && cr.BlackKingSide) then
                        let f1, g1 = Square.ofFileRank File.F (Rank.fromInt rnk), Square.ofFileRank File.G (Rank.fromInt rnk)
                        if not (Board.isOccupied b f1) && not (Board.isOccupied b g1) then
                            moves.Add({ From = sq; To = g1; Kind = CastleKingSide })

                    if (us = White && cr.WhiteQueenSide) || (us = Black && cr.BlackQueenSide) then
                        let d1, c1, b1 = Square.ofFileRank File.D (Rank.fromInt rnk), 
                                         Square.ofFileRank File.C (Rank.fromInt rnk),
                                         Square.ofFileRank File.B (Rank.fromInt rnk)
                        if not (Board.isOccupied b d1) && not (Board.isOccupied b c1) && not (Board.isOccupied b b1) then
                            moves.Add({ From = sq; To = c1; Kind = CastleQueenSide })

                | Bishop | Rook | Queen as kind ->
                    let occ = b.Bitboards.Occupancy
                    let mutable combinedAttacks = 0uL

                    if kind = Bishop || kind = Queen then
                        let e = Magic.bishopEntries.[sq]
                        combinedAttacks <- combinedAttacks ||| Magic.bishopTable.[e.Offset + Magic.getIndex occ e.Mask]

                    if kind = Rook || kind = Queen then
                        let e = Magic.rookEntries.[sq]
                        combinedAttacks <- combinedAttacks ||| Magic.rookTable.[e.Offset + Magic.getIndex occ e.Mask]

                    let usTotal = if us = White then b.Bitboards.WhiteTotal else b.Bitboards.BlackTotal
                    let mutable targets = combinedAttacks &&& ~~~usTotal
    
                    while targets <> 0uL do
                        let t = Bitboard.popLsb &targets
                        if Board.isOccupied b t then moves.Add({ From = sq; To = t; Kind = Capture })
                        else moves.Add({ From = sq; To = t; Kind = Quiet })

        moves.ToArray()    

    /// Gets all legal moves for the current position.
    let getLegalMoves (b: Board) =
        let us, them = b.SideToMove, Colour.opposite b.SideToMove

        getPseudoLegalMoves b
        |> Array.filter (fun m ->
            let castlingCheck =
                match m.Kind with
                | CastleKingSide
                | CastleQueenSide ->
                    if Board.isInCheckFor us b then
                        false
                    else
                        let rnk = if us = White then Rank.R1 else Rank.R8

                        let midFile = if m.Kind = CastleKingSide then File.F else File.D

                        let destFile = if m.Kind = CastleKingSide then File.G else File.C

                        let midSquare = Square.ofFileRank midFile rnk

                        let destSquare = Square.ofFileRank destFile rnk

                        not (Attack.isSquareAttacked b midSquare them)
                        && not (Attack.isSquareAttacked b destSquare them)
                | _ -> true

            castlingCheck && not (Board.isInCheckFor us (Board.applyMove m b)))

module San =
    /// Converts a move to Standard Algebraic Notation (SAN) based on the current board state.
    let toSan (b: Board) (m: Move) =
        match m.Kind with
        | CastleKingSide -> "O-O"
        | CastleQueenSide -> "O-O-O"
        | _ ->
            // Use Board.tryGetPiece instead of b.Pieces.Value
            let piece = (Board.tryGetPiece b m.From).Value

            let isCapture =
                match m.Kind with
                | Capture
                | EnPassant -> true
                // Use Board.isOccupied instead of b.Pieces.ContainsKey
                | _ -> Board.isOccupied b m.To

            let nextBoard = Board.applyMove m b
            let isCheck = Board.isInCheck nextBoard
            let isMate = isCheck && (MoveGen.getLegalMoves nextBoard).Length = 0

            let moveStr =
                if piece.Kind = Pawn then
                    let prefix =
                        if isCapture then
                            sprintf "%cx" (File.toChar (Square.file m.From))
                        else
                            ""

                    let prom =
                        match m.Kind with
                        | Promotion pt -> sprintf "=%c" (Char.ToUpper(PieceType.toChar pt))
                        | _ -> ""

                    sprintf "%s%s%s" prefix (Square.toString m.To) prom
                else
                    let pChar = Char.ToUpper(PieceType.toChar piece.Kind)

                    let others =
                        MoveGen.getLegalMoves b
                        |> Array.filter (fun alt ->
                            // Use Board.tryGetPiece to identify the piece on the alternative square
                            match Board.tryGetPiece b alt.From with
                            | Some altPiece ->
                                alt.From <> m.From && alt.To = m.To && altPiece.Kind = piece.Kind
                            | None -> false)

                    let disambiguator =
                        if others.Length = 0 then
                            ""
                        else
                            let sameFile =
                                others |> Array.exists (fun alt -> Square.file alt.From = Square.file m.From)

                            let sameRank =
                                others |> Array.exists (fun alt -> Square.rank alt.From = Square.rank m.From)

                            if not sameFile then
                                sprintf "%c" (File.toChar (Square.file m.From))
                            elif not sameRank then
                                sprintf "%c" (Rank.toChar (Square.rank m.From))
                            else
                                Square.toString m.From

                    let cap = if isCapture then "x" else ""
                    sprintf "%c%s%s%s" pChar disambiguator cap (Square.toString m.To)

            let suffix =
                if isMate then "#"
                elif isCheck then "+"
                else ""

            moveStr + suffix

module Evaluation =

    /// Assigns a base value to each piece type for evaluation purposes.
    let pieceValue =
        function
        | Pawn -> 100
        | Knight -> 320
        | Bishop -> 330
        | Rook -> 500
        | Queen -> 900
        | King -> 20000

    // All PSTs are written as they appear on a board (Rank 8 top, Rank 1 bottom)
    // and then reversed so that index 0 = a1.

    /// The pawn PST is designed for the middle game. In a more complete engine, we would switch to a different PST in the endgame.
    let pawnPst =
        Array.rev
            [| 0
               0
               0
               0
               0
               0
               0
               0
               50
               50
               50
               50
               50
               50
               50
               50
               10
               10
               20
               30
               30
               20
               10
               10
               5
               5
               10
               25
               25
               10
               5
               5
               0
               0
               0
               20
               20
               0
               0
               0
               5
               -5
               -10
               0
               0
               -10
               -5
               5
               5
               10
               10
               -20
               -20
               10
               10
               5
               0
               0
               0
               0
               0
               0
               0
               0 |]

    /// The knight PST is designed for the middle game. In a more complete engine, we would switch to a different PST in the endgame.
    let knightPst =
        Array.rev
            [| -50
               -40
               -30
               -30
               -30
               -30
               -40
               -50
               -40
               -20
               0
               5
               5
               0
               -20
               -40
               -30
               5
               10
               15
               15
               10
               5
               -30
               -30
               0
               15
               20
               20
               15
               0
               -30
               -30
               5
               15
               20
               20
               15
               5
               -30
               -30
               0
               10
               15
               15
               10
               0
               -30
               -40
               -20
               0
               0
               0
               0
               -20
               -40
               -50
               -40
               -30
               -30
               -30
               -30
               -40
               -50 |]

    /// The bishop PST is designed for the middle game. In a more complete engine, we would switch to a different PST in the endgame.
    let bishopPst =
        Array.rev
            [| -20
               -10
               -10
               -10
               -10
               -10
               -10
               -20
               -10
               0
               0
               0
               0
               0
               0
               -10
               -10
               0
               5
               10
               10
               5
               0
               -10
               -10
               5
               5
               10
               10
               5
               5
               -10
               -10
               0
               10
               10
               10
               10
               0
               -10
               -10
               10
               10
               10
               10
               10
               10
               -10
               -10
               5
               0
               0
               0
               0
               5
               -10
               -20
               -10
               -10
               -10
               -10
               -10
               -10
               -20 |]


    /// The rook PST is designed for the middle game. In a more complete engine, we would switch to a different PST in the endgame.
    let rookPst =
        Array.rev
            [| 0
               0
               0
               0
               0
               0
               0
               0
               5
               10
               10
               10
               10
               10
               10
               5
               -5
               0
               0
               0
               0
               0
               0
               -5
               -5
               0
               0
               0
               0
               0
               0
               -5
               -5
               0
               0
               0
               0
               0
               0
               -5
               -5
               0
               0
               0
               0
               0
               0
               -5
               -5
               0
               0
               0
               0
               0
               0
               -5
               0
               0
               0
               5
               5
               0
               0
               0 |]

    /// The queen PST is designed for the middle game. In a more complete engine, we would switch to a different PST in the endgame.
    let queenPst =
        Array.rev
            [| -20
               -10
               -10
               -5
               -5
               -10
               -10
               -20
               -10
               0
               0
               0
               0
               0
               0
               -10
               -10
               0
               5
               5
               5
               5
               0
               -10
               -5
               0
               5
               5
               5
               5
               0
               -5
               0
               0
               5
               5
               5
               5
               0
               -5
               -10
               5
               5
               5
               5
               5
               0
               -10
               -10
               0
               5
               0
               0
               0
               0
               -10
               -20
               -10
               -10
               -5
               -5
               -10
               -10
               -20 |]

    /// The king PST is designed for the middle game. In a more complete engine, we would switch to a different PST in the endgame.
    let kingPst =
        Array.rev
            [| -30
               -40
               -40
               -50
               -50
               -40
               -40
               -30
               -30
               -40
               -40
               -50
               -50
               -40
               -40
               -30
               -30
               -40
               -40
               -50
               -50
               -40
               -40
               -30
               -30
               -40
               -40
               -50
               -50
               -40
               -40
               -30
               -20
               -30
               -30
               -40
               -40
               -30
               -30
               -20
               -10
               -20
               -20
               -20
               -20
               -20
               -20
               -10
               20
               20
               0
               0
               0
               0
               20
               20
               20
               30
               10
               0
               0
               10
               30
               20 |]

    /// Evaluates the board position from White's perspective. Positive scores favor White, negative scores favor Black.
    let evaluate (b: Board) =
        let mutable score = 0
        let mutable occ = b.Bitboards.Occupancy
        
        while occ <> 0uL do
            let sq = Bitboard.popLsb &occ
            // We know a piece exists here, so we call getPieceAt
            let p = (BitboardSet.getPieceAt sq b.Bitboards).Value
            
            let baseVal = pieceValue p.Kind
            let pstIndex = if p.Colour = White then sq else sq ^^^ 56
            let pstBonus = 
                match p.Kind with
                | Pawn -> pawnPst.[pstIndex]
                | Knight -> knightPst.[pstIndex]
                | Bishop -> bishopPst.[pstIndex]
                | Rook -> rookPst.[pstIndex]
                | Queen -> queenPst.[pstIndex]
                | King -> kingPst.[pstIndex]

            if p.Colour = White then score <- score + baseVal + pstBonus
            else score <- score - (baseVal + pstBonus)
        score    

module Search =
    let mutable nodes = 0uL // Global counter for the current search
    let MATE_VALUE = 30000
    let INF = 1000000

    /// Quiescence search: plays out all captures until the position is stable.
    let rec quiesce (b: Board) (alpha: int) (beta: int) (ct: CancellationToken) : int =
        // Every few nodes, check if we should stop
        if ct.IsCancellationRequested then
            alpha
        else
            let sideMult = if b.SideToMove = White then 1 else -1
            let standPat = Evaluation.evaluate b * sideMult

            if standPat >= beta then
                beta
            else
                let mutable currentAlpha = Math.Max(alpha, standPat)

                let captures =
                    MoveGen.getLegalMoves b
                    |> Array.filter (fun m ->
                        match m.Kind with
                        | Capture
                        | EnPassant
                        | Promotion _ -> true
                        | _ -> false)

                let mutable i = 0
                let mutable exitLoop = false

                while i < captures.Length && not exitLoop do
                    // Pass the token down
                    let score = -quiesce (Board.applyMove captures.[i] b) (-beta) (-currentAlpha) ct

                    if score >= beta then
                        currentAlpha <- beta
                        exitLoop <- true
                    else
                        if score > currentAlpha then
                            currentAlpha <- score

                        i <- i + 1

                currentAlpha

    /// Negamax search with alpha-beta pruning.
    let rec negamax (b: Board) (depth: int) (alpha: int) (beta: int) (ct: CancellationToken) : int * Move option =
        nodes <- nodes + 1uL // Increment on every call
        // Check for "stop" command
        if ct.IsCancellationRequested then
            (0, None)
        elif depth = 0 then
            (quiesce b alpha beta ct, None)
        else
            let moves = MoveGen.getLegalMoves b

            if moves.Length = 0 then
                if Board.isInCheck b then
                    (-MATE_VALUE - depth, None)
                else
                    (0, None)
            else
                let mutable bestScore = -INF
                let mutable bestMove = None
                let mutable currentAlpha = alpha

                let sortedMoves =
                    moves
                    |> Array.sortByDescending (fun m ->
                        match m.Kind with
                        | Capture
                        | EnPassant -> 100
                        | Promotion _ -> 90
                        | _ -> 0)

                let mutable i = 0
                let mutable exitLoop = false

                while i < sortedMoves.Length && not exitLoop do
                    if ct.IsCancellationRequested then
                        exitLoop <- true
                    else
                        let m = sortedMoves.[i]
                        let score, _ = negamax (Board.applyMove m b) (depth - 1) (-beta) (-currentAlpha) ct
                        let actualScore = -score

                        if actualScore > bestScore then
                            bestScore <- actualScore
                            bestMove <- Some m

                        currentAlpha <- Math.Max(currentAlpha, bestScore)

                        if currentAlpha >= beta then
                            exitLoop <- true
                        else
                            i <- i + 1

                (bestScore, bestMove)

    /// Iterative Deepening
    let findBestMove (b: Board) (maxDepth: int) (targetTimeMs: int) (ct: CancellationToken) =
        async {
            // This explicitly tells F# to move this work to a background thread
            do! Async.SwitchToThreadPool()

            nodes <- 0uL
            let sw = Diagnostics.Stopwatch.StartNew()
            let mutable absoluteBestMove = None
            let mutable d = 1

            // Iterative Deepening Loop
            while d <= maxDepth && not ct.IsCancellationRequested do
                let score, moveOpt = negamax b d -INF INF ct

                if not ct.IsCancellationRequested then
                    let elapsed = sw.Elapsed.TotalSeconds

                    let nps =
                        if elapsed > 0.001 then
                            uint64 (float nodes / elapsed)
                        else
                            0uL

                    match moveOpt with
                    | Some m ->
                        absoluteBestMove <- Some m
                        // Added 'nodes' and 'nps' for CuteChess to read
                        printfn "info depth %d score cp %d nodes %d nps %d pv %s" d score nodes nps (Move.toUci m)
                    | None -> ()
                // SIMPLE TIME MANAGEMENT:
                // If we've used more than 60% of our allotted time, don't start the next depth
                if sw.ElapsedMilliseconds > int64 (targetTimeMs / 2) then
                    d <- maxDepth + 1 // Exit loop
                else
                    d <- d + 1

            return absoluteBestMove
        }

/// Represents a single test case for the perft suite, including the position (FEN), expected node counts at various depths, and a name for identification.
type PerftSuiteItem =
    { Name: string
      Fen: string
      Expected: uint64 list }

module Perft =
    /// Counts the number of leaf nodes at a given depth from the current board state.
    let rec countNodes depth b =
        if depth = 0 then
            1uL
        else
            let moves = MoveGen.getLegalMoves b

            if depth = 1 then
                uint64 moves.Length
            else
                let mutable total = 0uL

                for i in 0 .. moves.Length - 1 do
                    total <- total + countNodes (depth - 1) (Board.applyMove moves.[i] b)

                total

    /// Divides the perft calculation for a given depth and board state.
    let divide depth b =
        let sw = Diagnostics.Stopwatch.StartNew()
        let moves = MoveGen.getLegalMoves b |> Array.sortBy Move.toUci
        let mutable total = 0uL

        printfn "Perft results for depth %d:" depth

        for m in moves do
            let n = countNodes (depth - 1) (Board.applyMove m b)
            // Use the San module we just built!
            printfn "%s (%s): %d" (Move.toUci m) (San.toSan b m) n
            total <- total + n

        sw.Stop()
        let ms = sw.ElapsedMilliseconds
        let nps = if ms > 0L then (total * 1000uL) / uint64 ms else 0uL

        printfn "\nTotal: %d | Time: %d ms | NPS: %d" total ms nps
        total

    let suites =
        [ { Name = "Initial Position"
            Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
            Expected = [ 1uL; 20uL; 400uL; 8902uL; 197281uL; 4865609uL; 119060324uL ] }

          { Name = "Kiwipete"
            Fen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1"
            Expected = [ 1uL; 48uL; 2039uL; 97862uL; 4085603uL; 193690690uL ] }

          { Name = "Endgame/EP"
            Fen = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1"
            Expected = [ 1uL; 14uL; 191uL; 2812uL; 43238uL; 674624uL ] }

          { Name = "Promotion Stress Test"
            Fen = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8"
            Expected = [ 1uL; 44uL; 1486uL; 62379uL; 2103487uL; 89941194uL ] } ]

    /// Runs the full perft suite up to a specified maximum depth, comparing results against expected values.
    let runFullSuite (maxDepth: int) =
        printfn "Starting Perft Regression Suite (Max Depth: %d)" maxDepth
        printfn "------------------------------------------------"
        let totalSw = Diagnostics.Stopwatch.StartNew()

        for suite in suites do
            printfn "Testing: %s" suite.Name
            let b = Board.fromFen suite.Fen
            let depthsToTest = Math.Min(maxDepth, suite.Expected.Length - 1)

            for d in 1..depthsToTest do
                let expected = suite.Expected.[d]
                let sw = Diagnostics.Stopwatch.StartNew()
                let actual = countNodes d b
                sw.Stop()

                if actual = expected then
                    printfn "  Depth %d: PASS (%d nodes) in %dms" d actual sw.ElapsedMilliseconds
                else
                    printfn "  Depth %d: FAILED! Expected %d, got %d" d expected actual

            printfn ""

        totalSw.Stop()
        printfn "Full Suite Finished in %d ms" totalSw.ElapsedMilliseconds

module Debug =
    /// 1.5.1 - Move list visualisation
    let displayMoves (b: Board) =
        let moves = MoveGen.getLegalMoves b
        printfn "Legal Moves (%d):" moves.Length

        let formatted =
            moves
            |> Array.map (fun m -> sprintf "%s (%s)" (Move.toUci m) (San.toSan b m))
            |> String.concat ", "

        printfn "%s" formatted

/// 1.5.2 - Board consistency checker
    let verify (b: Board) =
        let errors = ResizeArray<string>()
        
        // Get all pieces from the bitboards
        let allPiecesList = BitboardSet.allPieces b.Bitboards |> Seq.toList
        let pieces = allPiecesList |> List.map snd

        // 1. Check Kings
        let whiteKings =
            pieces
            |> List.filter (fun p -> p.Colour = White && p.Kind = King)
            |> List.length

        let blackKings =
            pieces
            |> List.filter (fun p -> p.Colour = Black && p.Kind = King)
            |> List.length

        if whiteKings <> 1 then
            errors.Add(sprintf "Invalid White King count: %d" whiteKings)

        if blackKings <> 1 then
            errors.Add(sprintf "Invalid Black King count: %d" blackKings)

        // 2. Check Pawns
        // Replaced b.Pieces loop with the allPiecesList
        for (sq, p) in allPiecesList do
            if p.Kind = Pawn then
                let r = Square.rank sq |> Rank.toInt

                if r = 0 || r = 7 then
                    errors.Add(sprintf "Pawn on illegal rank %d at %s" (r + 1) (Square.toString sq))

        // 3. Side not to move cannot be in check
        if Board.isInCheckFor (Colour.opposite b.SideToMove) b then
            errors.Add("Illegal state: Side NOT to move is in check.")

        if errors.Count = 0 then
            printfn "Board state is consistent."
        else
            printfn "CONSISTENCY ERRORS FOUND:"

            for err in errors do
                printfn " - %s" err

    /// 1.5.3 - Attack map visualiser
    let displayAttackMap (b: Board) (attacker: Colour) =
        printfn "Attack Map for %A:" attacker

        for r in 7..-1..0 do
            printf "%d " (r + 1)

            for f in 0..7 do
                let sq = Square.ofFileRank (File.fromInt f) (Rank.fromInt r)

                if Attack.isSquareAttacked b sq attacker then
                    printf "x "
                else
                    printf ". "

            printfn ""

        printfn "  a b c d e f g h"

module UciLoop =
    let startFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
    let mutable board = Board.fromFen startFen
    // Track the current search task and its cancellation token
    let mutable searchCts = new CancellationTokenSource()

    let rec run () =
        let line = Console.ReadLine()

        if line <> null then
            let ts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries) |> Array.toList

            match ts with
            | "uci" :: _ ->
                printfn "id name Floundroid"
                printfn "id author Phil Brooks"
                printfn "uciok"
            | "isready" :: _ -> printfn "readyok"
            | "position" :: rest ->
                let (fen, moveParts) =
                    match rest with
                    | "startpos" :: "moves" :: m -> (startFen, m)
                    | "startpos" :: _ -> (startFen, [])
                    | "fen" :: fParts ->
                        // Find where "moves" starts, if it exists
                        let movesIdx = fParts |> List.tryFindIndex (fun s -> s = "moves")

                        match movesIdx with
                        | Some i ->
                            let f = fParts |> List.take i |> String.concat " "
                            let m = fParts |> List.skip (i + 1)
                            (f, m)
                        | None -> (String.concat " " fParts, [])
                    | _ -> (startFen, [])

                board <- Board.fromFen fen

                for mStr in moveParts do
                    let legalMoves = MoveGen.getLegalMoves board

                    match legalMoves |> Array.tryFind (fun m -> Move.toUci m = mStr) with
                    | Some m -> board <- Board.applyMove m board
                    | None -> ()
            | "go" :: rest ->
                // Basic Time Management Logic
                let wtime =
                    rest
                    |> List.tryFindIndex (fun s -> s = "wtime")
                    |> Option.map (fun i -> int rest.[i + 1])
                    |> Option.defaultValue 100000

                let btime =
                    rest
                    |> List.tryFindIndex (fun s -> s = "btime")
                    |> Option.map (fun i -> int rest.[i + 1])
                    |> Option.defaultValue 100000

                // Simple rule: Spend 1/20th of remaining time on this move
                let myTime = if board.SideToMove = White then wtime else btime
                let targetTime = myTime / 20
                // Cancel any existing search just in case
                searchCts.Cancel()
                searchCts <- new CancellationTokenSource()
                let token = searchCts.Token

                // UCI Parser for 'depth'
                let depthIdx = rest |> List.tryFindIndex (fun s -> s = "depth")

                let depth =
                    match depthIdx with
                    | Some i when i < rest.Length - 1 ->
                        match Int32.TryParse(rest.[i + 1]) with
                        | true, d -> d
                        | _ -> 4 // Default depth if parsing fails
                    | _ -> 20 // Default depth if 'depth' not specified

                // Start the search in the background
                Async.Start(
                    async {
                        let! result = Search.findBestMove board depth targetTime searchCts.Token

                        match result with
                        | Some m -> printfn "bestmove %s" (Move.toUci m)
                        | None ->
                            // If we have no move (e.g. cancelled at depth 0),
                            // we should still try to find something or print nothing safely
                            ()
                    },
                    token
                )

            | "perft" :: rest ->
                match rest with
                | "suite" :: d :: _ ->
                    match Int32.TryParse d with
                    | true, depth -> Perft.runFullSuite depth
                    | _ -> printfn "Invalid depth: %s" d
                | "suite" :: _ -> Perft.runFullSuite 4
                | d :: _ ->
                    match Int32.TryParse d with
                    | true, depth -> Perft.divide depth board |> ignore
                    | _ -> printfn "Invalid depth: %s" d
                | [] ->
                    // Handles just the word "perft"
                    Perft.divide 1 board |> ignore

            | "print" :: _ -> Board.prettyPrint board

            | "moves" :: _ -> Debug.displayMoves board
            | "verify" :: _ -> Debug.verify board
            | "attacks" :: "white" :: _ -> Debug.displayAttackMap board White
            | "attacks" :: "black" :: _ -> Debug.displayAttackMap board Black
            | "testbb" :: _ ->
                let mutable bb = Bitboard.empty
                bb <- Bitboard.set (Square.fromString "e4") bb
                bb <- Bitboard.set (Square.fromString "d5") bb
                printfn "Bitboard counts: %d" (Bitboard.count bb)
                printfn "%s" (Bitboard.toString bb)

            | "stop" :: _ -> searchCts.Cancel() // This tells the search to die
            | "quit" :: _ ->
                searchCts.Cancel()
                Environment.Exit(0)
            | _ -> ()

            run ()

[<EntryPoint>]
let main _ =
    UciLoop.run ()
    0
