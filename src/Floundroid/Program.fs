module Floundroid

open System
open System.Text
open System.Threading

/// Colour is represented as an integer 0–1, where 0 = White and 1 = Black.
type Colour =
    | White = 0
    | Black = 1

module Colour =

    /// Converts a Colour to its character representation ('w' or 'b').
    let toChar (c: Colour) =
        if c = Colour.White then 'w' else 'b'

    /// Converts a character ('w' or 'b') to a Colour.
    let fromChar c =
        match c with
        | 'w' | 'W' -> Colour.White
        | 'b' | 'B' -> Colour.Black
        | _ -> invalidArg "c" $"Invalid colour char: %c{c}"

    /// Returns the opposite colour.
    let opposite (c: Colour) =
        if c = Colour.White then Colour.Black else Colour.White

type File =
    | A = 0
    | B = 1
    | C = 2
    | D = 3
    | E = 4
    | F = 5
    | G = 6
    | H = 7

module File =

    let firstChar = int 'a'
    
    /// Converts a File to its integer representation (0–7).
    let toInt (f: File) : int =
        int f

    /// Converts an integer (0–7) to a File.
    let fromInt (i: int) : File =
        if uint i <= 7u then enum<File> i
        else invalidArg "i" $"File index {i} out of range (0–7)"

    /// Converts a File to its character representation ('a'–'h').
    let toChar (f: File) : char =
        char (firstChar + int f)

    /// Converts a character ('a'–'h') to a File.
    let fromChar (c: char) : File =
        let i = int c - firstChar
        if uint i <= 7u then enum<File> i
        else invalidArg "c" $"Invalid file char: {c}"

type Rank =
    | R1 = 0
    | R2 = 1
    | R3 = 2
    | R4 = 3
    | R5 = 4
    | R6 = 5
    | R7 = 6
    | R8 = 7

module Rank =

    // Must NOT be private if inline functions use it
    let firstChar = int '1'

    /// Converts a Rank to its integer representation (0–7).
    let toInt (r: Rank) : int =
        int r

    /// Converts an integer (0–7) to a Rank.
    let fromInt (i: int) : Rank =
        if uint i <= 7u then enum<Rank> i
        else invalidArg "i" $"Rank index {i} out of range (0–7)"

    /// Converts a Rank to its character representation ('1'–'8').
    let toChar (r: Rank) : char =
        char (firstChar + int r)

    /// Converts a character ('1'–'8') to a Rank.
    let fromChar (c: char) : Rank =
        let i = int c - firstChar
        if uint i <= 7u then enum<Rank> i
        else invalidArg "c" $"Invalid rank char: {c}"

/// Squares are represented as integers 0–63, where 0 = a1 and 63 = h8.
type Square = int

module Square =

    /// Converts a File and Rank to a Square.
    let ofFileRank (f: File) (r: Rank) : Square =
        (Rank.toInt r <<< 3) + File.toInt f
        // same as r*8 + f, but uses bit shift

    /// Gets the File of a Square.
    let file (sq: Square) : File =
        File.fromInt (sq &&& 7)
        // sq % 8 → sq & 7

    /// Gets the Rank of a Square.
    let rank (sq: Square) : Rank =
        Rank.fromInt (sq >>> 3)
        // sq / 8 → sq >> 3

    /// Converts a Square to algebraic notation (e.g. "e4").
    let toString (sq: Square) : string =
        let f = file sq |> File.toChar
        let r = rank sq |> Rank.toChar
        string f + string r

    /// Converts algebraic notation (e.g. "d4") to a Square.
    let fromString (s: string) : Square =
        if s.Length <> 2 then
            invalidArg "s" $"Invalid square string: {s}"
        let f = File.fromChar s[0]
        let r = Rank.fromChar s[1]
        ofFileRank f r

    /// Checks if a file/rank pair is on the board.
    let isOnBoard (f: int) (r: int) =
        uint f <= 7u && uint r <= 7u

type PieceType =
    | Pawn   = 0
    | Knight = 1
    | Bishop = 2
    | Rook   = 3
    | Queen  = 4
    | King   = 5

module PieceType =

    // Precomputed char table for speed
    let chars = [| 'p'; 'n'; 'b'; 'r'; 'q'; 'k' |]

    /// Converts a PieceType to its character representation ('p'..'k').
    let toChar (pt: PieceType) : char =
        chars[int pt]

    /// Converts a character to a PieceType.
    let fromChar (c: char) : PieceType =
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

module Piece =

    let colour (p: Piece) : Colour =
        enum<Colour> (int p.data >>> 3)

    let kind (p: Piece) : PieceType =
        enum<PieceType> (int p.data &&& 0b111)

    let toChar (p: Piece) : char =
        let c = PieceType.toChar (kind p)
        if colour p = Colour.White then Char.ToUpper c else c

    let fromChar (c: char) : Piece =
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

module CastlingRights =

    let fromString (s: string) =
        if s = "-" then CastlingRights.None
        else
            (if s.Contains "K" then CastlingRights.WK else CastlingRights.None)
            ||| (if s.Contains "Q" then CastlingRights.WQ else CastlingRights.None)
            ||| (if s.Contains "k" then CastlingRights.BK else CastlingRights.None)
            ||| (if s.Contains "q" then CastlingRights.BQ else CastlingRights.None)

    let toString (cr: CastlingRights) =
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

module Move =

    let fromSq (m: Move) =
        int (m.data &&& 0b111111u)

    let toSq (m: Move) =
        int ((m.data >>> 6) &&& 0b111111u)

    let kind (m: Move) =
        int ((m.data >>> 12) &&& 0b1111u)

    let promo (m: Move) =
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

/// Bitboards are 64-bit unsigned integers where each bit represents a square.
/// Bit 0 is a1, Bit 7 is h1, Bit 63 is h8.
type Bitboard = uint64

module Bitboard =
    let empty: Bitboard = 0uL
    let all: Bitboard = 0xFFFFFFFFFFFFFFFFuL

    /// Sets the bit at the given square.
    let set (sq: Square) (bb: Bitboard) : Bitboard = bb ||| (1uL <<< sq)

    /// Clears the bit at the given square.
    let clear (sq: Square) (bb: Bitboard) : Bitboard = bb &&& ~~~(1uL <<< sq)

    /// Checks if a square is set.
    let contains (sq: Square) (bb: Bitboard) : bool = (bb &&& (1uL <<< sq)) <> 0uL

    /// Returns the number of set bits (population count).
    let count (bb: Bitboard) : int =
        System.Numerics.BitOperations.PopCount(bb)

    /// Returns the index of the least significant bit (0-63) and clears it from the bitboard.
    /// This is a high-performance way to iterate through pieces.
    let popLsb (bb: byref<Bitboard>) : Square =
        let lsb = System.Numerics.BitOperations.TrailingZeroCount(bb)
        bb <- bb &&& (bb - 1uL)
        lsb

    let bits (bb: Bitboard) : seq<Square> =
        seq {
            let mutable x = bb
            while x <> 0UL do
                let sq = popLsb &x
                yield sq
        }
    
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

    /// Identifies the piece (if any) at a specific square using bitboards.
    let getPieceAt (sq: Square) (bbs: BitboardSet) : Piece option =
        let bit = 1uL <<< sq

        if (bbs.Occupancy &&& bit) = 0uL then
            None
        else
            let color = if (bbs.WhiteTotal &&& bit) <> 0uL then Colour.White else Colour.Black

            let kind =
                if (bbs.WhitePawns ||| bbs.BlackPawns) &&& bit <> 0uL then
                    PieceType.Pawn
                elif (bbs.WhiteKnights ||| bbs.BlackKnights) &&& bit <> 0uL then
                    PieceType.Knight
                elif (bbs.WhiteBishops ||| bbs.BlackBishops) &&& bit <> 0uL then
                    PieceType.Bishop
                elif (bbs.WhiteRooks ||| bbs.BlackRooks) &&& bit <> 0uL then
                    PieceType.Rook
                elif (bbs.WhiteQueens ||| bbs.BlackQueens) &&& bit <> 0uL then
                    PieceType.Queen
                else
                    PieceType.King

            Some (Piece(color, kind))

    /// A helper to flip a piece on/off. Essential for incremental updates.
    let togglePiece (p: Piece) (sq: Square) (bbs: BitboardSet) =
        let bit = 1uL <<< sq

        let newBbs =
            match Piece.colour p, Piece.kind p with
            | Colour.White, PieceType.Pawn ->
                { bbs with
                    WhitePawns = bbs.WhitePawns ^^^ bit }
            | Colour.White, PieceType.Knight ->
                { bbs with
                    WhiteKnights = bbs.WhiteKnights ^^^ bit }
            | Colour.White, PieceType.Bishop ->
                { bbs with
                    WhiteBishops = bbs.WhiteBishops ^^^ bit }
            | Colour.White, PieceType.Rook ->
                { bbs with
                    WhiteRooks = bbs.WhiteRooks ^^^ bit }
            | Colour.White, PieceType.Queen ->
                { bbs with
                    WhiteQueens = bbs.WhiteQueens ^^^ bit }
            | Colour.White, PieceType.King ->
                { bbs with
                    WhiteKings = bbs.WhiteKings ^^^ bit }
            | Colour.Black, PieceType.Pawn ->
                { bbs with
                    BlackPawns = bbs.BlackPawns ^^^ bit }
            | Colour.Black, PieceType.Knight ->
                { bbs with
                    BlackKnights = bbs.BlackKnights ^^^ bit }
            | Colour.Black, PieceType.Bishop ->
                { bbs with
                    BlackBishops = bbs.BlackBishops ^^^ bit }
            | Colour.Black, PieceType.Rook ->
                { bbs with
                    BlackRooks = bbs.BlackRooks ^^^ bit }
            | Colour.Black, PieceType.Queen ->
                { bbs with
                    BlackQueens = bbs.BlackQueens ^^^ bit }
            | Colour.Black, PieceType.King ->
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

module Magic =
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
            let bMask = bishopMask sq
            let bBits = Bitboard.count bMask
            bishopEntries.[sq] <- { Mask = bMask; Offset = sq * 4096 }
            for i in 0 .. (1 <<< bBits) - 1 do
                let blockers = getBlockers i bMask
                let tableIdx = (sq * 4096) + i
                bishopTable.[tableIdx] <- bishopAttacks sq blockers

            // Rooks
            let rMask = rookMask sq
            let rBits = Bitboard.count rMask
            rookEntries.[sq] <- { Mask = rMask; Offset = sq * 4096 }
            for i in 0 .. (1 <<< rBits) - 1 do
                let blockers = getBlockers i rMask
                let tableIdx = (sq * 4096) + i
                rookTable.[tableIdx] <- rookAttacks sq blockers
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
      FullmoveNumber: int
      Hash: uint64 }

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
    let pieceIdx (pt: PieceType) = int pt

    /// Maps Colour to index 0-1
    let colourIdx (c: Colour) = int c
    
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
        Table.Pieces.[colourIdx (Piece.colour pc), pieceIdx (Piece.kind pc), sq]

    /// Gets the key for a specific set of castling rights.
    let getCastlingKey (cr: CastlingRights) =
        let mutable index = 0
        if (int cr &&& int CastlingRights.WK) <> 0  then index <- index ||| 1
        if (int cr &&& int CastlingRights.WQ) <> 0 then index <- index ||| 2
        if (int cr &&& int CastlingRights.BK) <> 0  then index <- index ||| 4
        if (int cr &&& int CastlingRights.BQ) <> 0 then index <- index ||| 8
        Table.Castling.[index]

    /// Gets the key for an En Passant file (0-7).
    let getEnPassantKey (sq: Square option) =
        match sq with
        | Some s -> Table.EnPassantFile.[s % 8]
        | None -> 0UL

module TranspositionTable =

    /// Flags for TT entries: Exact (PV), Alpha (Upper bound), Beta (Lower bound)
    type NodeFlag = 
        | Exact = 0 
        | Alpha = 1 
        | Beta = 2

    [<Struct>]
    type TTEntry = {
        Hash: uint64
        Move: Move option
        Value: int
        Depth: int
        Age : byte
        Flag: NodeFlag
    }

    // Update empty entry
    let emptyEntry = { Hash = 0UL; Move = None; Value = 0; Depth = -1; Age = 0uy; Flag = NodeFlag.Alpha }

    // Global age counter
    let mutable currentAge = 0uy

    /// Advances the age of the transposition table, allowing for aging out old entries.
    let advanceAge () = currentAge <- currentAge + 1uy

    /// A table size of 2^20 is roughly 32-64MB depending on padding.
    let SIZE = 1 <<< 20 
    let table: TTEntry[] = Array.create SIZE emptyEntry

    /// Adjusts mate scores from the search to be relative to the root.
    /// This ensures "Mate in 5" found at depth 10 is stored correctly.
    let mateToTT (score: int) (ply: int) =
        if score > 20000 then score + ply
        elif score < -20000 then score - ply
        else score

    /// Adjusts mate scores from the TT back to the search, reversing the previous adjustment.
    let mateFromTT (score: int) (ply: int) =
        if score > 20000 then score - ply
        elif score < -20000 then score + ply
        else score

    /// Clears the transposition table, resetting all entries to empty.
    let clear () =
        Array.fill table 0 SIZE emptyEntry

    /// Stores an entry in the transposition table with the given parameters.
    let store (hash: uint64) (depth: int) (ply: int) (flag: NodeFlag) (value: int) (m: Move option) =
        let index = int (hash % uint64 SIZE)
        let adjustedValue = mateToTT value ply
        
        let existing = table.[index]

        // REPLACEMENT STRATEGY: 
        // 1. Always replace if the slot is empty or the hash matches (updating existing info)
        // 2. Always replace if the existing entry is from an OLDER age
        // 3. Otherwise, only replace if the new search is deeper
        let isOld = existing.Age <> currentAge
        
        if existing.Hash = 0UL || existing.Hash = hash || isOld || depth >= existing.Depth then
            table.[index] <- { 
                Hash = hash; Move = m; Value = adjustedValue; 
                Depth = depth; Flag = flag; Age = currentAge // <-- Store current age
            }

    let probe (hash: uint64) =
        let index = int (hash % uint64 SIZE)
        let entry = table.[index]
        if entry.Hash = hash then Some entry else None

module Board =

    let empty =
        { Bitboards = BitboardSet.empty // Placeholder
          SideToMove = Colour.White
          CastlingRights = CastlingRights.None
          EnPassantSquare = None
          HalfmoveClock = 0
          FullmoveNumber = 1
          Hash = 0UL }

    let isSquareAttacked (b: Board) (sq: Square) (attacker: Colour) =
        let bbs = b.Bitboards
        let them = attacker

        // 1. Pawn, Knight, King (Keep existing logic)
        let usIdx = if them = Colour.Black then 0 else 1
        let pawnAttackMask = BitboardGen.pawnAttacks.[usIdx, sq]
        let themPawns = if them = Colour.White then bbs.WhitePawns else bbs.BlackPawns

        if (pawnAttackMask &&& themPawns) <> 0uL then true
        else
            let knightAttackMask = BitboardGen.knightAttacks.[sq]
            let themKnights = if them = Colour.White then bbs.WhiteKnights else bbs.BlackKnights
            if (knightAttackMask &&& themKnights) <> 0uL then true
            else
                let kingAttackMask = BitboardGen.kingAttacks.[sq]
                let themKing = if them = Colour.White then bbs.WhiteKings else bbs.BlackKings
                if (kingAttackMask &&& themKing) <> 0uL then true
                else
                    // Inside Board.isSquareAttacked, replace the sliding logic with:
                    let occ = bbs.Occupancy

                    // Bishop & Queen
                    let bEntry = Magic.bishopEntries.[sq]
                    let bIdx = bEntry.Offset + Magic.getIndex occ bEntry.Mask
                    let bishopAttacks = Magic.bishopTable.[bIdx]
                    let themBishops = if them = Colour.White then (bbs.WhiteBishops ||| bbs.WhiteQueens) else (bbs.BlackBishops ||| bbs.BlackQueens)

                    if (bishopAttacks &&& themBishops) <> 0uL then true
                    else
                        // Rook & Queen
                        let rEntry = Magic.rookEntries.[sq]
                        let rIdx = rEntry.Offset + Magic.getIndex occ rEntry.Mask
                        let rookAttacks = Magic.rookTable.[rIdx]
                        let themRooks = if them = Colour.White then (bbs.WhiteRooks ||| bbs.WhiteQueens) else (bbs.BlackRooks ||| bbs.BlackQueens)
    
                        (rookAttacks &&& themRooks) <> 0uL                    

    let fromUci (b: Board) (s: string) : Move =
        if s.Length < 4 then
            invalidArg "s" "UCI move string too short"

        let fromSq = Square.fromString (s.Substring(0, 2))
        let toSq   = Square.fromString (s.Substring(2, 2))

        let movingPiece =
            BitboardSet.getPieceAt fromSq b.Bitboards
            |> Option.get

        let isPawn = Piece.kind movingPiece = PieceType.Pawn

        // --- 1. Promotion ---
        if s.Length = 5 then
            let promo = int (PieceType.fromChar s.[4])
            Move(fromSq, toSq, 5, promo)

        // --- 2. Castling ---
        elif Piece.kind movingPiece = PieceType.King then
            match (fromSq, toSq) with
            | (4, 6)   -> Move(fromSq, toSq, 3, 0)   // White O-O
            | (4, 2)   -> Move(fromSq, toSq, 4, 0)   // White O-O-O
            | (60, 62) -> Move(fromSq, toSq, 3, 0)   // Black O-O
            | (60, 58) -> Move(fromSq, toSq, 4, 0)   // Black O-O-O
            | _ -> Move(fromSq, toSq, 0, 0)

        // --- 3. En passant ---
        elif isPawn &&
             b.EnPassantSquare.IsSome &&
             toSq = b.EnPassantSquare.Value &&
             Square.file fromSq <> Square.file toSq then
            Move(fromSq, toSq, 2, 0)

        // --- 4. Capture ---
        elif BitboardSet.getPieceAt toSq b.Bitboards |> Option.isSome then
            Move(fromSq, toSq, 1, 0)

        // --- 5. Quiet ---
        else
            Move(fromSq, toSq, 0, 0)
   
    /// Tries to get a piece from a square (Source of truth: Bitboards).
    let tryGetPiece (b: Board) (sq: Square) = BitboardSet.getPieceAt sq b.Bitboards

    /// Checks if a square is occupied (Source of truth: Bitboards).
    let isOccupied (b: Board) (sq: Square) =
        (b.Bitboards.Occupancy &&& (1uL <<< sq)) <> 0uL

    /// Find the king square (Needed for check detection).
    let findKing (colour: Colour) (b: Board) =
        let mutable bb =
            if colour = Colour.White then
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

    /// Calculates the full Zobrist hash from scratch (Used for FEN initialization)
    let calculateHash (b: Board) =
        let mutable h = 0UL
        
        // 1. Pieces
        for (sq, piece) in BitboardSet.allPieces b.Bitboards do
            h <- h ^^^ (Zobrist.getPieceKey piece sq)
            
        // 2. Side to move
        if b.SideToMove = Colour.Black then
            h <- h ^^^ Zobrist.Table.SideToMove
            
        // 3. Castling rights
        h <- h ^^^ (Zobrist.getCastlingKey b.CastlingRights)
        
        // 4. En Passant
        h <- h ^^^ (Zobrist.getEnPassantKey b.EnPassantSquare)
        
        h    
    
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

        let boardWithoutHash = 
            { Bitboards = bbs
              SideToMove = Colour.fromChar parts.[1].[0]
              CastlingRights = CastlingRights.fromString parts.[2]
              EnPassantSquare = if parts.[3] = "-" then None else Some(Square.fromString parts.[3])
              HalfmoveClock = int parts.[4]
              FullmoveNumber = int parts.[5]
              Hash = 0UL } 
        { boardWithoutHash with Hash = calculateHash boardWithoutHash }
 
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
        else isSquareAttacked b kingSq (Colour.opposite colour)
    
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
        // 1. Initialize variables for the new state
        let mutable newBitboards = b.Bitboards
        let mutable newHash = b.Hash
        let movingPiece = BitboardSet.getPieceAt (Move.fromSq m) b.Bitboards |> Option.get
        let isPawn = Piece.kind movingPiece = PieceType.Pawn
        let opponent = Colour.opposite b.SideToMove

        // 2. XOR out old state from Hash (Side, Castling, EP)
        newHash <- newHash ^^^ Zobrist.Table.SideToMove
        newHash <- newHash ^^^ (Zobrist.getCastlingKey b.CastlingRights)
        newHash <- newHash ^^^ (Zobrist.getEnPassantKey b.EnPassantSquare)

        // 3. Remove the moving piece from the source
        // TogglePiece XORs the bitboard and we XOR the hash
        newBitboards <- BitboardSet.togglePiece movingPiece (Move.fromSq m) newBitboards
        newHash <- newHash ^^^ (Zobrist.getPieceKey movingPiece (Move.fromSq m))

        // 4. Handle Captures (including En Passant)
        let capturedPieceAtTo = BitboardSet.getPieceAt (Move.toSq m) b.Bitboards
    
        match Move.kind m with
        | 2 ->   // EnPassant
            let epPawnSq = if b.SideToMove = Colour.White then (Move.toSq m) - 8 else (Move.toSq m) + 8
            let victimPawn = Piece(opponent, PieceType.Pawn)
            newBitboards <- BitboardSet.togglePiece victimPawn epPawnSq newBitboards
            newHash <- newHash ^^^ (Zobrist.getPieceKey victimPawn epPawnSq)
        | _ ->
            // Normal captures (Quiet, Promotion, or Castling can't capture, but we check 'To' occupancy)
            match capturedPieceAtTo with
            | Some victim ->
                newBitboards <- BitboardSet.togglePiece victim (Move.toSq m) newBitboards
                newHash <- newHash ^^^ (Zobrist.getPieceKey victim (Move.toSq m))
            | None -> ()

        // 5. Place the piece at the destination
        match Move.kind m with
        | 5 ->   // Promotion
            let promoType = Move.promo m |> enum<PieceType>
            let promotedPiece = Piece(b.SideToMove, promoType)

            let toSq = Move.toSq m
            newBitboards <- BitboardSet.togglePiece promotedPiece toSq newBitboards
            newHash <- newHash ^^^ Zobrist.getPieceKey promotedPiece toSq

        | _ ->
            let toSq = Move.toSq m
            newBitboards <- BitboardSet.togglePiece movingPiece toSq newBitboards
            newHash <- newHash ^^^ Zobrist.getPieceKey movingPiece toSq

        // 6. Handle Special Rook Moves (Castling)
        match Move.kind m with
        | 3 ->   // CastleKingSide        
            let (rSrc, rDst) = if b.SideToMove = Colour.White then (7, 5) else (63, 61)
            let rook = Piece(b.SideToMove, PieceType.Rook)
            newBitboards <- BitboardSet.togglePiece rook rSrc newBitboards
            newBitboards <- BitboardSet.togglePiece rook rDst newBitboards
            newHash <- newHash ^^^ (Zobrist.getPieceKey rook rSrc) ^^^ (Zobrist.getPieceKey rook rDst)
        | 4 ->   // CastleQueenSide ->
            let (rSrc, rDst) = if b.SideToMove = Colour.White then (0, 3) else (56, 59)
            let rook = Piece(b.SideToMove, PieceType.Rook)
            newBitboards <- BitboardSet.togglePiece rook rSrc newBitboards
            newBitboards <- BitboardSet.togglePiece rook rDst newBitboards
            newHash <- newHash ^^^ (Zobrist.getPieceKey rook rSrc) ^^^ (Zobrist.getPieceKey rook rDst)
        | _ -> ()

        // 7. Update Castling Rights
        // Rights are lost if King moves, or if Rooks move/are captured
        let mutable newCR = b.CastlingRights
        if Piece.kind movingPiece = PieceType.King then
            if b.SideToMove = Colour.White then
                newCR <- newCR &&& ~~~(CastlingRights.WK ||| CastlingRights.WQ)
            else
                newCR <- newCR &&& ~~~(CastlingRights.BK ||| CastlingRights.BQ)

        // If a rook moves from its starting square
        if (Move.fromSq m) = 0 then newCR <- newCR &&& ~~~(CastlingRights.WQ)
        if (Move.fromSq m) = 7 then newCR <- newCR &&& ~~~(CastlingRights.WK)
        if (Move.fromSq m) = 56 then newCR <- newCR &&& ~~~(CastlingRights.BQ)
        if (Move.fromSq m) = 63 then newCR <- newCR &&& ~~~(CastlingRights.BK)

        // If a rook is captured on its starting square
        if (Move.toSq m) = 0 then newCR <- newCR &&& ~~~(CastlingRights.WQ)
        if (Move.toSq m) = 7 then newCR <- newCR &&& ~~~(CastlingRights.WK)
        if (Move.toSq m) = 56 then newCR <- newCR &&& ~~~(CastlingRights.BQ)
        if (Move.toSq m) = 63 then newCR <- newCR &&& ~~~(CastlingRights.BK)

        // 8. Update En Passant Square
        // Only set if a pawn moves two squares
        let newEPSquare =
            if isPawn && abs ((Move.toSq m) - (Move.fromSq m)) = 16 then
                Some (((Move.fromSq m) + (Move.toSq m)) / 2)
            else None

        // 9. Update Clocks
        let newHMClock =
            if isPawn || capturedPieceAtTo.IsSome then 0
            else b.HalfmoveClock + 1
        
        let newFMNumber =
            if b.SideToMove = Colour.Black then b.FullmoveNumber + 1
            else b.FullmoveNumber

        // 10. Finalize Hash (XOR in new Castling and new EP)
        newHash <- newHash ^^^ (Zobrist.getCastlingKey newCR)
        newHash <- newHash ^^^ (Zobrist.getEnPassantKey newEPSquare)

        { Bitboards = newBitboards
          SideToMove = opponent
          CastlingRights = newCR
          EnPassantSquare = newEPSquare
          HalfmoveClock = newHMClock
          FullmoveNumber = newFMNumber
          Hash = newHash }

    /// Executes a null move, updating side to move, en passant, halfmove clock, fullmove number, and hash.
    let applyNullMove (b: Board) =
        let opponent = Colour.opposite b.SideToMove
        let mutable newHash = b.Hash
        
        newHash <- newHash ^^^ Zobrist.Table.SideToMove
        newHash <- newHash ^^^ (Zobrist.getEnPassantKey b.EnPassantSquare)
        
        let newFMNumber =
            if b.SideToMove = Colour.Black then b.FullmoveNumber + 1
            else b.FullmoveNumber

        { Bitboards = b.Bitboards
          SideToMove = opponent
          CastlingRights = b.CastlingRights
          EnPassantSquare = None
          HalfmoveClock = b.HalfmoveClock + 1
          FullmoveNumber = newFMNumber
          Hash = newHash }

    /// Checks if the board has insufficient material for checkmate.
    let hasInsufficientMaterial (b: Board) =
        let bbs = b.Bitboards
        if bbs.WhitePawns <> 0uL || bbs.BlackPawns <> 0uL || 
           bbs.WhiteRooks <> 0uL || bbs.BlackRooks <> 0uL || 
           bbs.WhiteQueens <> 0uL || bbs.BlackQueens <> 0uL then
            false
        else
            let whiteKnights = Bitboard.count bbs.WhiteKnights
            let blackKnights = Bitboard.count bbs.BlackKnights
            let whiteBishops = Bitboard.count bbs.WhiteBishops
            let blackBishops = Bitboard.count bbs.BlackBishops
            
            let whiteMinors = whiteKnights + whiteBishops
            let blackMinors = blackKnights + blackBishops
            
            if whiteMinors = 0 && blackMinors = 0 then true // K vs K
            elif whiteMinors = 1 && blackMinors = 0 then true // KN vs K or KB vs K
            elif whiteMinors = 0 && blackMinors = 1 then true // K vs KN or K vs KB
            elif whiteKnights = 0 && blackKnights = 0 then
                // Only Bishops left. Draw if all bishops are on the same color.
                let whiteSquaresMask = 0xAA55AA55AA55AA55uL
                let allBishops = bbs.WhiteBishops ||| bbs.BlackBishops
                let onWhite = (allBishops &&& whiteSquaresMask) <> 0uL
                let onBlack = (allBishops &&& ~~~whiteSquaresMask) <> 0uL
                not (onWhite && onBlack) 
            else false

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
            [ PieceType.Bishop, [ (1, 1); (1, -1); (-1, 1); (-1, -1) ]
              PieceType.Rook, [ (1, 0); (-1, 0); (0, 1); (0, -1) ]
              PieceType.Queen, [ (1, 1); (1, -1); (-1, 1); (-1, -1); (1, 0); (-1, 0); (0, 1); (0, -1) ]
              PieceType.Knight, [ (1, 2); (1, -2); (-1, 2); (-1, -2); (2, 1); (2, -1); (-2, 1); (-2, -1) ]
              PieceType.King, [ (1, 1); (1, -1); (-1, 1); (-1, -1); (1, 0); (-1, 0); (0, 1); (0, -1) ] ]

    // Gets all pseudo-legal moves for the current position using Bitboards.
    let getPseudoLegalMoves (b: Board) =
        let moves = ResizeArray<Move>()
        let us, them = b.SideToMove, Colour.opposite b.SideToMove

        // Use the bitboard iterator to find all our pieces
        for (sq, p) in BitboardSet.allPieces b.Bitboards do
            if Piece.colour p = us then
                let f, r = Square.file sq |> File.toInt, Square.rank sq |> Rank.toInt

                match Piece.kind p with
                | PieceType.Pawn ->
                    let d = if us = Colour.White then 1 else -1

                    // 1. Single Push
                    let nr1 = r + d
                    if nr1 >= 0 && nr1 <= 7 then
                        let p1 = Square.ofFileRank (File.fromInt f) (Rank.fromInt nr1)
                        if not (Board.isOccupied b p1) then
                            // Promotion push
                            if nr1 = (if us = Colour.White then 7 else 0) then
                                for pt in [ PieceType.Queen; PieceType.Rook; PieceType.Bishop; PieceType.Knight ] do
                                    moves.Add(Move(sq, p1, 5, int pt))
                            else
                                moves.Add(Move(sq, p1, 0, 0))

                            // 2. Double push from starting rank
                            if r = (if us = Colour.White then 1 else 6) then
                                let nr2 = r + 2 * d
                                let p2 = Square.ofFileRank (File.fromInt f) (Rank.fromInt nr2)
                                if not (Board.isOccupied b p2) then
                                    moves.Add(Move(sq, p2, 0, 0))

                    // 3. Captures
                    for df in [ -1; 1 ] do
                        let nf, nr = f + df, r + d
                        if Square.isOnBoard nf nr then
                            let cap = Square.ofFileRank (File.fromInt nf) (Rank.fromInt nr)
                            match Board.tryGetPiece b cap with
                            | Some victim when Piece.colour victim = them ->
                                if nr = (if us = Colour.White then 7 else 0) then
                                    for pt in [ PieceType.Queen; PieceType.Rook; PieceType.Bishop; PieceType.Knight ] do
                                        moves.Add(Move(sq, cap, 5, int pt))
                                else
                                    moves.Add(Move(sq, cap, 1, 0))
                            | _ -> ()

                    // 4. En passant
                    match b.EnPassantSquare with
                    | Some ep ->
                        if abs (File.toInt (Square.file ep) - f) = 1 && Rank.toInt (Square.rank ep) = r + d then
                            moves.Add(Move(sq, ep, 2, 0))
                    | None -> ()

                | PieceType.Knight ->
                    // Use high-speed Bitboard lookup
                    let mutable attacks = BitboardGen.knightAttacks.[sq]
                    while attacks <> 0uL do
                        let t = Bitboard.popLsb &attacks
                        match Board.tryGetPiece b t with
                        | Some target ->
                            if Piece.colour target = them then
                                moves.Add(Move(sq, t, 1, 0))
                        | None -> moves.Add(Move(sq, t, 0, 0))

                | PieceType.King ->
                    // Use high-speed Bitboard lookup
                    let mutable attacks = BitboardGen.kingAttacks.[sq]
                    while attacks <> 0uL do
                        let t = Bitboard.popLsb &attacks
                        match Board.tryGetPiece b t with
                        | Some target ->
                            if Piece.colour target = them then
                                moves.Add(Move(sq, t, 1, 0))
                        | None -> moves.Add(Move(sq, t, 0, 0))  

                    // Castling
                    let rnk, cr = (if us = Colour.White then 0 else 7), b.CastlingRights
                    if (us = Colour.White && (int cr &&& int CastlingRights.WK) <> 0) || (us = Colour.Black && (int cr &&& int CastlingRights.BK) <> 0) then
                        let f1, g1 = Square.ofFileRank File.F (Rank.fromInt rnk), Square.ofFileRank File.G (Rank.fromInt rnk)
                        if not (Board.isOccupied b f1) && not (Board.isOccupied b g1) then
                            moves.Add(Move(sq, g1, 3, 0))

                    if (us = Colour.White && (int cr &&& int CastlingRights.WQ) <> 0) || (us = Colour.Black && (int cr &&& int CastlingRights.BQ) <> 0) then
                        let d1, c1, b1 = Square.ofFileRank File.D (Rank.fromInt rnk), 
                                         Square.ofFileRank File.C (Rank.fromInt rnk),
                                         Square.ofFileRank File.B (Rank.fromInt rnk)
                        if not (Board.isOccupied b d1) && not (Board.isOccupied b c1) && not (Board.isOccupied b b1) then
                            moves.Add(Move(sq, c1, 4, 0))

                | PieceType.Bishop | PieceType.Rook | PieceType.Queen as kind ->
                    let occ = b.Bitboards.Occupancy
                    let mutable combinedAttacks = 0uL

                    if kind = PieceType.Bishop || kind = PieceType.Queen then
                        let e = Magic.bishopEntries.[sq]
                        combinedAttacks <- combinedAttacks ||| Magic.bishopTable.[e.Offset + Magic.getIndex occ e.Mask]

                    if kind = PieceType.Rook || kind = PieceType.Queen then
                        let e = Magic.rookEntries.[sq]
                        combinedAttacks <- combinedAttacks ||| Magic.rookTable.[e.Offset + Magic.getIndex occ e.Mask]

                    let usTotal = if us = Colour.White then b.Bitboards.WhiteTotal else b.Bitboards.BlackTotal
                    let mutable targets = combinedAttacks &&& ~~~usTotal
    
                    while targets <> 0uL do
                        let t = Bitboard.popLsb &targets
                        if Board.isOccupied b t then moves.Add(Move(sq, t, 1, 0))
                        else moves.Add(Move(sq, t, 0, 0))

                | _ -> invalidArg "Piece.kind p" $"Invalid PieceType: %A{Piece.kind p}"

        moves.ToArray()    

    /// Gets all legal moves for the current position.
    let getLegalMoves (b: Board) =
        let us, them = b.SideToMove, Colour.opposite b.SideToMove

        getPseudoLegalMoves b
        |> Array.filter (fun m ->
            let castlingCheck =
                match Move.kind m with
                | 3
                | 4 ->
                    if Board.isInCheckFor us b then
                        false
                    else
                        let rnk = if us = Colour.White then Rank.R1 else Rank.R8

                        let midFile = if Move.kind m = 3 then File.F else File.D

                        let destFile = if Move.kind m = 3 then File.G else File.C

                        let midSquare = Square.ofFileRank midFile rnk

                        let destSquare = Square.ofFileRank destFile rnk

                        not (Board.isSquareAttacked b midSquare them)
                        && not (Board.isSquareAttacked b destSquare them)
                | _ -> true

            castlingCheck && not (Board.isInCheckFor us (Board.applyMove m b)))

    /// Optimized generator for Quiescence Search: Only returns Captures, En Passants, and Promotions.
    let getCaptureMoves (b: Board) =
        let moves = ResizeArray<Move>()
        let us, them = b.SideToMove, Colour.opposite b.SideToMove

        for (sq, p) in BitboardSet.allPieces b.Bitboards do
            if Piece.colour p = us then
                let f, r = Square.file sq |> File.toInt, Square.rank sq |> Rank.toInt

                match Piece.kind p with
                | PieceType.Pawn ->
                    let d = if us = Colour.White then 1 else -1
                    let targetRank = if us = Colour.White then 7 else 0
                    
                    // 1. Captures (Normal + Promotion Captures)
                    for df in [ -1; 1 ] do
                        let nf, nr = f + df, r + d
                        if Square.isOnBoard nf nr then
                            let cap = Square.ofFileRank (File.fromInt nf) (Rank.fromInt nr)
                            match Board.tryGetPiece b cap with
                            | Some victim when Piece.colour victim = them ->
                                if nr = targetRank then
                                    for pt in [ PieceType.Queen; PieceType.Rook; PieceType.Bishop; PieceType.Knight ] do
                                        moves.Add(Move(sq, cap, 5, int pt))
                                else
                                    moves.Add(Move(sq, cap, 1, 0))
                            | _ -> ()

                    // 2. En passant
                    match b.EnPassantSquare with
                    | Some ep ->
                        if abs (File.toInt (Square.file ep) - f) = 1 && Rank.toInt (Square.rank ep) = r + d then
                            moves.Add(Move(sq, ep, 2, 0))
                    | None -> ()
                    
                    // 3. Quiet Promotions (important for QS)
                    let nr1 = r + d
                    if nr1 = targetRank then
                        let p1 = Square.ofFileRank (File.fromInt f) (Rank.fromInt nr1)
                        if not (Board.isOccupied b p1) then
                            for pt in [ PieceType.Queen; PieceType.Rook; PieceType.Bishop; PieceType.Knight ] do
                                moves.Add(Move(sq, p1, 5, int pt))

                | PieceType.Knight ->
                    let mutable attacks = BitboardGen.knightAttacks.[sq] &&& (if us = Colour.White then b.Bitboards.BlackTotal else b.Bitboards.WhiteTotal)
                    while attacks <> 0uL do
                        let t = Bitboard.popLsb &attacks
                        moves.Add(Move(sq, t, 1, 0))

                | PieceType.King ->
                    let mutable attacks = BitboardGen.kingAttacks.[sq] &&& (if us = Colour.White then b.Bitboards.BlackTotal else b.Bitboards.WhiteTotal)
                    while attacks <> 0uL do
                        let t = Bitboard.popLsb &attacks
                        moves.Add(Move(sq, t, 1, 0))

                | PieceType.Bishop | PieceType.Rook | PieceType.Queen as kind ->
                    let occ = b.Bitboards.Occupancy
                    let mutable combinedAttacks = 0uL
                    if kind = PieceType.Bishop || kind = PieceType.Queen then
                        let e = Magic.bishopEntries.[sq]
                        combinedAttacks <- combinedAttacks ||| Magic.bishopTable.[e.Offset + Magic.getIndex occ e.Mask]
                    if kind = PieceType.Rook || kind = PieceType.Queen then
                        let e = Magic.rookEntries.[sq]
                        combinedAttacks <- combinedAttacks ||| Magic.rookTable.[e.Offset + Magic.getIndex occ e.Mask]

                    let themTotal = if us = Colour.White then b.Bitboards.BlackTotal else b.Bitboards.WhiteTotal
                    let mutable targets = combinedAttacks &&& themTotal
    
                    while targets <> 0uL do
                        let t = Bitboard.popLsb &targets
                        moves.Add(Move(sq, t, 1, 0))

                | _ -> invalidArg "Piece.kind p" $"Invalid PieceType: %A{Piece.kind p}"

        moves.ToArray()

module San =
    /// Converts a move to Standard Algebraic Notation (SAN) based on the current board state.
    let toSan (b: Board) (m: Move) =
        match Move.kind m with
        | 3 -> "O-O"
        | 4 -> "O-O-O"
        | _ ->
            // Use Board.tryGetPiece instead of b.Pieces.Value
            let piece = (Board.tryGetPiece b (Move.fromSq m)).Value

            let isCapture =
                match Move.kind m with
                | 1
                | 2 -> true
                // Use Board.isOccupied instead of b.Pieces.ContainsKey
                | _ -> Board.isOccupied b (Move.toSq m)

            let nextBoard = Board.applyMove m b
            let isCheck = Board.isInCheck nextBoard
            let isMate = isCheck && (MoveGen.getLegalMoves nextBoard).Length = 0

            let moveStr =
                if Piece.kind piece = PieceType.Pawn then
                    let prefix =
                        if isCapture then
                            sprintf "%cx" (File.toChar (Square.file (Move.fromSq m)))
                        else
                            ""

                    let prom =
                        match Move.kind m with
                        | 5 -> sprintf "=%c" (Char.ToUpper(PieceType.toChar (enum<PieceType>(Move.promo m))))
                        | _ -> ""

                    sprintf "%s%s%s" prefix (Square.toString (Move.toSq m)) prom
                else
                    let pChar = Char.ToUpper(PieceType.toChar (Piece.kind piece))

                    let others =
                        MoveGen.getLegalMoves b
                        |> Array.filter (fun alt ->
                            // Use Board.tryGetPiece to identify the piece on the alternative square
                            match Board.tryGetPiece b (Move.fromSq alt) with
                            | Some altPiece ->
                                (Move.fromSq alt) <> (Move.fromSq m) && (Move.toSq alt) = (Move.toSq m) && Piece.kind altPiece = Piece.kind piece
                            | None -> false)

                    let disambiguator =
                        if others.Length = 0 then
                            ""
                        else
                            let sameFile =
                                others |> Array.exists (fun alt -> Square.file (Move.fromSq alt) = Square.file (Move.fromSq m))

                            let sameRank =
                                others |> Array.exists (fun alt -> Square.rank (Move.fromSq alt) = Square.rank (Move.fromSq m))

                            if not sameFile then
                                sprintf "%c" (File.toChar (Square.file (Move.fromSq m)))
                            elif not sameRank then
                                sprintf "%c" (Rank.toChar (Square.rank (Move.fromSq m)))
                            else
                                Square.toString (Move.fromSq m)

                    let cap = if isCapture then "x" else ""
                    sprintf "%c%s%s%s" pChar disambiguator cap (Square.toString (Move.toSq m))

            let suffix =
                if isMate then "#"
                elif isCheck then "+"
                else ""

            moveStr + suffix

module Evaluation =

    /// Assigns a base value to each piece type for evaluation purposes.
    let pieceValue =
        function
        | PieceType.Pawn -> 100
        | PieceType.Knight -> 320
        | PieceType.Bishop -> 330
        | PieceType.Rook -> 500
        | PieceType.Queen -> 900
        | PieceType.King -> 20000
        | _ -> invalidArg "PieceType" "Invalid PieceType value"

    // All PSTs are written as they appear on a board (Rank 8 top, Rank 1 bottom)
    // and then reversed so that index 0 = a1.

    /// The pawn PST is designed for the middle game. In a more complete engine, we would switch to a different PST in the endgame.
    let pawnPst =
        Array.rev
            [| 0; 0; 0; 0; 0; 0; 0; 0
               50; 50; 50; 50; 50; 50; 50; 50
               10; 10; 20; 30; 30; 20; 10; 10
               5; 5; 10; 25; 25; 10; 5; 5
               0; 0; 0; 20; 20; 0; 0; 0
               5; -5; -10; 0; 0; -10; -5; 5
               5; 10; 10; -20; -20; 10; 10; 5
               0; 0; 0; 0; 0; 0; 0; 0 |]

    /// The knight PST is designed for the middle game. In a more complete engine, we would switch to a different PST in the endgame.
    let knightPst =
        Array.rev
            [| -50; -40; -30; -30; -30; -30; -40; -50
               -40; -20; 0; 5; 5; 0; -20; -40
               -30; 5; 10; 15; 15; 10; 5; -30
               -30; 0; 15; 20; 20; 15; 0; -30
               -30; 5; 15; 20; 20; 15; 5; -30
               -30; 0; 10; 15; 15; 10; 0; -30
               -40; -20; 0; 0; 0; 0; -20; -40
               -50; -40; -30; -30; -30; -30; -40; -50 |]

    /// The bishop PST is designed for the middle game. In a more complete engine, we would switch to a different PST in the endgame.
    let bishopPst =
        Array.rev
            [| -20; -10; -10; -10; -10; -10; -10; -20
               -10; 0; 0; 0; 0; 0; 0; -10
               -10; 0; 5; 10; 10; 5; 0; -10
               -10; 5; 5; 10; 10; 5; 5; -10
               -10; 0; 10; 10; 10; 10; 0; -10
               -10; 10; 10; 10; 10; 10; 10; -10
               -10; 5; 0; 0; 0; 0; 5; -10
               -20; -10; -10; -10; -10; -10; -10; -20 |]

    /// The rook PST is designed for the middle game. In a more complete engine, we would switch to a different PST in the endgame.
    let rookPst =
        Array.rev
            [| 0; 0; 0; 0; 0; 0; 0; 0
               5; 10; 10; 10; 10; 10; 10; 5
               -5; 0; 0; 0; 0; 0; 0; -5
               -5; 0; 0; 0; 0; 0; 0; -5
               -5; 0; 0; 0; 0; 0; 0; -5
               -5; 0; 0; 0; 0; 0; 0; -5
               -5; 0; 0; 0; 0; 0; 0; -5
               0; 0; 0; 5; 5; 0; 0; 0 |]

    /// The queen PST is designed for the middle game. In a more complete engine, we would switch to a different PST in the endgame.
    let queenPst =
        Array.rev
            [| -20; -10; -10; -5; -5; -10; -10; -20
               -10; 0; 0; 0; 0; 0; 0; -10
               -10; 0; 5; 5; 5; 5; 0; -10
               -5; 0; 5; 5; 5; 5; 0; -5
               0; 0; 5; 5; 5; 5; 0; -5
               -10; 5; 5; 5; 5; 5; 0; -10
               -10; 0; 5; 0; 0; 0; 0; -10
               -20; -10; -10; -5; -5; -10; -10; -20 |]

    /// The king PST is designed for the middle game. In a more complete engine, we would switch to a different PST in the endgame.
    let kingPst =
        Array.rev
            [| -30; -40; -40; -50; -50; -40; -40; -30
               -30; -40; -40; -50; -50; -40; -40; -30
               -30; -40; -40; -50; -50; -40; -40; -30
               -30; -40; -40; -50; -50; -40; -40; -30
               -20; -30; -30; -40; -40; -30; -30; -20
               -10; -20; -20; -20; -20; -20; -20; -10
               20; 20; 0; 0; 0; 0; 20; 20
               20; 30; 10; 0; 0; 10; 30; 20 |]

    let private pawnFileCounts (pawns: Bitboard) =
        let counts = Array.zeroCreate 8
        let mutable remaining = pawns

        while remaining <> 0uL do
            let sq = Bitboard.popLsb &remaining
            counts.[sq % 8] <- counts.[sq % 8] + 1

        counts

    let private isPassedPawn (colour: Colour) (sq: Square) (enemyPawns: Bitboard) =
        let file = sq % 8
        let rank = sq / 8
        let mutable blocked = false

        for targetFile in Math.Max(0, file - 1) .. Math.Min(7, file + 1) do
            if colour = Colour.White then
                for targetRank in rank + 1 .. 7 do
                    if Bitboard.contains (targetRank * 8 + targetFile) enemyPawns then
                        blocked <- true
            else
                for targetRank in rank - 1 .. -1 .. 0 do
                    if Bitboard.contains (targetRank * 8 + targetFile) enemyPawns then
                        blocked <- true

        not blocked

    let private passedPawnBonus (colour: Colour) (sq: Square) =
        let rank = sq / 8
        let progress = if colour = Colour.White then rank else 7 - rank
        [| 0; 0; 6; 12; 20; 35; 55; 0 |].[progress]

    let private evaluatePawnSide (colour: Colour) (friendlyPawns: Bitboard) (enemyPawns: Bitboard) =
        let fileCounts = pawnFileCounts friendlyPawns
        let mutable score = 0
        let mutable remaining = friendlyPawns

        for file in 0..7 do
            if fileCounts.[file] > 1 then
                score <- score - ((fileCounts.[file] - 1) * 6)

        while remaining <> 0uL do
            let sq = Bitboard.popLsb &remaining
            let file = sq % 8

            let hasLeftPawn = file > 0 && fileCounts.[file - 1] > 0
            let hasRightPawn = file < 7 && fileCounts.[file + 1] > 0

            if not hasLeftPawn && not hasRightPawn then
                score <- score - 5
            else
                score <- score + 2

            if isPassedPawn colour sq enemyPawns then
                score <- score + passedPawnBonus colour sq

        score

    /// Evaluates the pawn structure of the board, returning a score from White's perspective.
    let pawnStructureScore (b: Board) =
        let bbs = b.Bitboards
        let whiteScore = evaluatePawnSide Colour.White bbs.WhitePawns bbs.BlackPawns
        let blackScore = evaluatePawnSide Colour.Black bbs.BlackPawns bbs.WhitePawns
        whiteScore - blackScore
    
    /// Very small king-safety heuristic: penalise missing pawn shield for short-castled kings.
    let kingSafety (b: Board) (side: Colour) =
        let kingSq = Board.findKing side b
        if kingSq = -1 then
            0 // No king found, probably an illegal position
        else
        
            let file = Square.file kingSq
            let rank = Square.rank kingSq

            // Only handle short castling (king on f/g1 or f/g8)
            let isShortCastled =
                match side, file, rank with
                | Colour.White, File.F, Rank.R1
                | Colour.White, File.G, Rank.R1
                | Colour.Black, File.F, Rank.R8
                | Colour.Black, File.G, Rank.R8 -> true
                | _ -> false

            if not isShortCastled then
                0
            else
                // f, g, h files
                let files = [ File.F; File.G; File.H ]
                let homeRank = if side = Colour.White then Rank.R2 else Rank.R7

                let mutable penalty = 0

                for f in files do
                    let sq = Square.ofFileRank f homeRank
                    match Board.tryGetPiece b sq with
                    | Some p when Piece.kind p = PieceType.Pawn && Piece.colour p = side -> ()
                    | _ -> penalty <- penalty + 10

                // --- Step 2: open / half-open file danger near king ---
                let openFilePenalty =
                    let mutable p = 0

                    // Files adjacent to king: g and h
                    let dangerFiles =
                        match file with
                        | File.F -> [ File.G; File.H ]   // king on f-file
                        | File.G -> [ File.G; File.H ]   // king on g-file
                        | _ -> []         // shouldn't happen for short castling

                    for df in dangerFiles do
                        let mutable whitePawn = false
                        let mutable blackPawn = false

                        // Scan the whole file for pawns
                        for r in [ Rank.R1; Rank.R2; Rank.R3; Rank.R4; Rank.R5; Rank.R6; Rank.R7; Rank.R8 ] do
                            let sq = Square.ofFileRank df r
                            match Board.tryGetPiece b sq with
                            | Some p when Piece.kind p = PieceType.Pawn && Piece.colour p = Colour.White -> whitePawn <- true
                            | Some p when Piece.kind p = PieceType.Pawn && Piece.colour p = Colour.Black -> blackPawn <- true
                            | _ -> ()

                        match whitePawn, blackPawn with
                        | false, false -> p <- p + 20   // fully open file
                        | true, false when side = Colour.Black -> p <- p + 10  // half-open against Black
                        | false, true when side = Colour.White -> p <- p + 10  // half-open against White
                        | _ -> ()

                    p
                
                // --- Step 3: enemy piece proximity ---
                let proximityPenalty =
                    let mutable p = 0

                    // squares within Chebyshev distance <= 2
                    let kingFile = file
                    let kingRank = rank

                    for df in [ -2 .. 2 ] do
                        for dr in [ -2 .. 2 ] do
                            if not (df = 0 && dr = 0) then
                                let fInt = File.toInt kingFile + df
                                let rInt = Rank.toInt kingRank + dr

                                if Square.isOnBoard fInt rInt then
                                    let f = File.fromInt fInt
                                    let r = Rank.fromInt rInt
                                    let sq = Square.ofFileRank f r

                                    match Board.tryGetPiece b sq with
                                    | Some pc when Piece.colour pc = Colour.opposite side ->
                                        match Piece.kind pc with
                                        | PieceType.Knight -> p <- p + 5
                                        | PieceType.Bishop -> p <- p + 5
                                        | PieceType.Rook   -> p <- p + 8
                                        | PieceType.Queen  -> p <- p + 12
                                        | _ -> ()
                                    | _ -> ()

                    p
                
                let totalPenalty = penalty + openFilePenalty + proximityPenalty
                if side = Colour.White then -totalPenalty else totalPenalty
                
    /// Evaluates the board position from White's perspective. Positive scores favor White, negative scores favor Black.
    let evaluate (b: Board) =
        let mutable score = pawnStructureScore b
        let mutable occ = b.Bitboards.Occupancy
        
        // NEW: tiny king-safety term
        //score <- score + kingSafety b Colour.White
        //score <- score - kingSafety b Colour.Black
        
        while occ <> 0uL do
            let sq = Bitboard.popLsb &occ
            // We know a piece exists here, so we call getPieceAt
            let p = (BitboardSet.getPieceAt sq b.Bitboards).Value
            
            let baseVal = pieceValue (Piece.kind p)
            let pstIndex = if Piece.colour p = Colour.White then sq else sq ^^^ 56
            let pstBonus = 
                match Piece.kind p with
                | PieceType.Pawn -> pawnPst.[pstIndex]
                | PieceType.Knight -> knightPst.[pstIndex]
                | PieceType.Bishop -> bishopPst.[pstIndex]
                | PieceType.Rook -> rookPst.[pstIndex]
                | PieceType.Queen -> queenPst.[pstIndex]
                | PieceType.King -> kingPst.[pstIndex]
                | _ -> invalidArg "Piece.kind p" $"Invalid PieceType: %A{Piece.kind p}"

            if Piece.colour p = Colour.White then score <- score + baseVal + pstBonus
            else score <- score - (baseVal + pstBonus)
        score    

module Search =
    let mutable nodes = 0uL
    let MATE_VALUE = 30000
    let INF = 1000000

    // Stores two quiet moves per ply that caused a beta cutoff
    let killerMoves: Move option[,] = Array2D.create 2 256 None

    // Add this to clear killers at the start of every search
    let clearKillers () =
        for i in 0 .. 1 do
            for j in 0 .. 255 do
                killerMoves.[i, j] <- None
    
    // History table: [Side][From][To]
    let historyTable = Array3D.create 2 64 64 0

    let clearHistory () =
        for c in 0..1 do
            for f in 0..63 do
                for t in 0..63 do
                    historyTable.[c, f, t] <- 0    
    
    let isRepetition (hash: uint64) (history: uint64 list) =
        history |> List.contains hash
    
    /// Quiescence search: plays out tactical moves until the position is stable.
    let rec quiesce (b: Board) (ply: int) (alpha: int) (beta: int) (ct: CancellationToken) : int =
        nodes <- nodes + 1uL
        if ct.IsCancellationRequested then alpha
        else
            let sideMult = if b.SideToMove = Colour.White then 1 else -1
            let standPat = Evaluation.evaluate b * sideMult

            if standPat >= beta then beta
            else
                let mutable currentAlpha = Math.Max(alpha, standPat)

                // 1. Generate only tactical moves
                let moves = MoveGen.getCaptureMoves b
                
                // 2. Simple MVV-LVA Scoring for QS
                let scores = Array.zeroCreate moves.Length
                for i in 0 .. moves.Length - 1 do
                    let m = moves.[i]
                    let victimVal = 
                        match Board.tryGetPiece b (Move.toSq m) with 
                        | Some p -> Evaluation.pieceValue (Piece.kind p) 
                        | None -> 100 // En Passant
                    let attackerVal = 
                        match Board.tryGetPiece b (Move.fromSq m) with 
                        | Some p -> Evaluation.pieceValue (Piece.kind p) 
                        | None -> 0
                    scores.[i] <- -(10000 + (victimVal * 10) - attackerVal)

                System.Array.Sort(scores, moves)

                let mutable i = 0
                let mutable exitLoop = false

                while i < moves.Length && not exitLoop do
                    let m = moves.[i]
                    let nextB = Board.applyMove m b
                    
                    // 3. Legality Check
                    if Board.isInCheckFor b.SideToMove nextB then
                        i <- i + 1
                    else
                        let score = -quiesce nextB (ply + 1) (-beta) (-currentAlpha) ct

                        if score >= beta then
                            currentAlpha <- beta
                            exitLoop <- true
                        else
                            if score > currentAlpha then
                                currentAlpha <- score
                            i <- i + 1

                currentAlpha    
    
    /// Internal negamax implementation with Null-Move Pruning (allowNull).
    let rec negamaxInternal (b: Board) (depth: int) (ply: int) (alpha: int) (beta: int) (allowNull: bool) (history: uint64 list) (ct: CancellationToken) : int * Move option =
        nodes <- nodes + 1uL
        
        if ct.IsCancellationRequested then
            (0, None)
        elif ply > 0 && (isRepetition b.Hash history || b.HalfmoveClock >= 100 || Board.hasInsufficientMaterial b) then
            (0, None)
        else
            // --- 1. TT PROBE ---
            let ttEntry = TranspositionTable.probe b.Hash
            let mutable ttMove = None
            let mutable ttValue = 0
            let mutable ttCutoff = false

            match ttEntry with
            | Some entry ->
                ttMove <- entry.Move
                if entry.Depth >= depth then
                    let value = TranspositionTable.mateFromTT entry.Value ply
                    match entry.Flag with
                    | TranspositionTable.NodeFlag.Exact -> (ttValue <- value; ttCutoff <- true)
                    | TranspositionTable.NodeFlag.Alpha when value <= alpha -> (ttValue <- value; ttCutoff <- true)
                    | TranspositionTable.NodeFlag.Beta when value >= beta -> (ttValue <- value; ttCutoff <- true)
                    | _ -> ()
            | None -> ()

            if ttCutoff then (ttValue, ttMove)
            elif depth <= 0 then (quiesce b ply alpha beta ct, None)
            else
                // --- Null-Move Pruning (NMP) ---
                let mutable nmpCutoff = false
                let mutable nmpScore = -INF

                let hasNonPawnMaterial =
                    let bbs = b.Bitboards
                    if b.SideToMove = Colour.White then
                        bbs.WhiteKnights <> 0uL || bbs.WhiteBishops <> 0uL || bbs.WhiteRooks <> 0uL || bbs.WhiteQueens <> 0uL
                    else
                        bbs.BlackKnights <> 0uL || bbs.BlackBishops <> 0uL || bbs.BlackRooks <> 0uL || bbs.BlackQueens <> 0uL

                if allowNull && depth >= 3 && not (Board.isInCheck b) && hasNonPawnMaterial then
                    let R = if depth > 6 then 3 else 2
                    let nullBoard = Board.applyNullMove b
                    let score, _ = negamaxInternal nullBoard (depth - 1 - R) (ply + 1) (-beta) (-beta + 1) false history ct
                    let nullScore = -score
                    
                    if not ct.IsCancellationRequested && nullScore >= beta then
                        if nullScore < MATE_VALUE - 100 then
                            nmpScore <- beta
                            nmpCutoff <- true

                if nmpCutoff then
                    if not ct.IsCancellationRequested then
                        TranspositionTable.store b.Hash depth ply TranspositionTable.NodeFlag.Beta beta None
                    (nmpScore, None)
                else
                    // 1. Generate Pseudo-Legal moves
                    let moves = MoveGen.getPseudoLegalMoves b
                    let mutable bestScore = -INF
                    let mutable bestMove = None
                    let mutable currentAlpha = alpha
                    let originalAlpha = alpha
                    let mutable legalMovesFound = 0
                    
                    // Calculate side index once for the scoring loop
                    let sideIdx = if b.SideToMove = Colour.White then 0 else 1

                    // 2. High-Performance Primitive Sorting
                    let scores = Array.zeroCreate moves.Length
                    for i in 0 .. moves.Length - 1 do
                        let m = moves.[i]
                        let score = 
                            if Some m = ttMove then 1000000 
                            else
                                match Move.kind m with
                                | 1 | 2 ->
                                    let victimVal = 
                                        if Board.isOccupied b (Move.toSq m) then 
                                            (match Board.tryGetPiece b (Move.toSq m) with Some p -> Evaluation.pieceValue (Piece.kind p) | None -> 0) 
                                        else 100 // En Passant
                                    let attackerVal = 
                                        match Board.tryGetPiece b (Move.fromSq m) with Some p -> Evaluation.pieceValue (Piece.kind p) | None -> 0
                                    10000 + (victimVal * 10) - attackerVal
                                | 5 -> 9000 + Evaluation.pieceValue (enum<PieceType>(Move.promo m))
                                | _ -> 
                                    if Some m = killerMoves.[0, ply] then 8000
                                    elif Some m = killerMoves.[1, ply] then 7000
                                    else 
                                        // FIXED: Use the pre-calculated sideIdx here
                                        Math.Min(historyTable.[sideIdx, (Move.fromSq m), (Move.toSq m)], 6000)
                        scores.[i] <- -score 

                    System.Array.Sort(scores, moves)

                    let mutable i = 0
                    let mutable exitLoop = false
                    
                    while i < moves.Length && not exitLoop do
                        let m = moves.[i]
        
                        // --- FIX: Add specific validation for Castling ---
                        let isIllegalCastle = 
                            match Move.kind m with
                            | 3 | 4 ->
                                let us = b.SideToMove
                                let them = Colour.opposite us
                                let rnk = if us = Colour.White then Rank.R1 else Rank.R8
                
                                // 1. Cannot castle OUT OF check
                                if Board.isInCheck b then true
                                else
                                    // 2. Cannot castle THROUGH check
                                    let midFile = if Move.kind m = 3 then File.F else File.D
                                    let midSquare = Square.ofFileRank midFile rnk
                                    Board.isSquareAttacked b midSquare them
                            | _ -> false

                        if isIllegalCastle then
                            i <- i + 1
                        else
                            let nextB = Board.applyMove m b
            
                            // 3. Legality Check (handles normal moves and "into check" for castling)
                            if Board.isInCheckFor b.SideToMove nextB then
                                i <- i + 1
                            else
                                legalMovesFound <- legalMovesFound + 1
                            
                                let score, _ = negamaxInternal nextB (depth - 1) (ply + 1) (-beta) (-currentAlpha) true (b.Hash :: history) ct
                                let actualScore = -score

                                if actualScore > bestScore then
                                    bestScore <- actualScore
                                    bestMove <- Some m

                                currentAlpha <- Math.Max(currentAlpha, bestScore)

                                if currentAlpha >= beta then
                                    // Beta Cutoff: Store Killers and History
                                    match Move.kind m with
                                    | 0 | 3 | 4 ->
                                        if killerMoves.[0, ply] <> Some m then
                                            killerMoves.[1, ply] <- killerMoves.[0, ply]
                                            killerMoves.[0, ply] <- Some m
                                        // Use pre-calculated sideIdx
                                        historyTable.[sideIdx, (Move.fromSq m), (Move.toSq m)] <- historyTable.[sideIdx, (Move.fromSq m), (Move.toSq m)] + (depth * depth)
                                    | _ -> ()
                                    exitLoop <- true
                                else
                                    i <- i + 1           

                    if legalMovesFound = 0 then
                        if Board.isInCheck b then (-MATE_VALUE + ply, None) else (0, None)
                    else
                        if not ct.IsCancellationRequested then
                            let flag = if bestScore <= originalAlpha then TranspositionTable.NodeFlag.Alpha elif bestScore >= beta then TranspositionTable.NodeFlag.Beta else TranspositionTable.NodeFlag.Exact
                            TranspositionTable.store b.Hash depth ply flag bestScore bestMove
                        (bestScore, bestMove)

    /// Negamax search with alpha-beta pruning and Transposition Table integration.
    let negamax (b: Board) (depth: int) (ply: int) (alpha: int) (beta: int) (history: uint64 list) (ct: CancellationToken) : int * Move option =
        negamaxInternal b depth ply alpha beta true history ct

    /// Iterative Deepening
    let findBestMove (b: Board) (maxDepth: int) (targetTimeMs: int) (history: uint64 list) (ct: CancellationToken) =
        async {
            do! Async.SwitchToThreadPool()

            nodes <- 0uL
            clearKillers()
            clearHistory() // --- NEW: Added for Step 3.4 ---
            
            let sw = Diagnostics.Stopwatch.StartNew()
            // CHANGE: Initialize with a fallback immediately instead of None
            // This guarantees a valid move is returned even if cancelled at Depth 1
            let legalMoves = MoveGen.getLegalMoves b
            let mutable absoluteBestMove = if legalMoves.Length > 0 then Some legalMoves.[0] else None

            let mutable d = 1

            while d <= maxDepth && not ct.IsCancellationRequested do
                let score, moveOpt = negamax b d 0 -INF INF history ct

                if not ct.IsCancellationRequested then
                    let elapsed = sw.Elapsed.TotalSeconds
                    let nps = if elapsed > 0.001 then uint64 (float nodes / elapsed) else 0uL
                    match moveOpt with
                    | Some m ->
                        absoluteBestMove <- Some m
                        printfn "info depth %d score cp %d nodes %d nps %d pv %s" d score nodes nps (Move.toUci m)
                    | None -> ()

                // --- SIMPLIFIED TIMER ---
                let totalElapsed = sw.ElapsedMilliseconds
                
                // Only stop if we've used 80% of our target. 
                // We remove the 50ms buffer to ensure we don't bail out at Depth 1.
                if totalElapsed > int64 (targetTimeMs * 6 / 10) then
                    d <- maxDepth + 1 
                else
                    d <- d + 1

            // --- STEP 2 FIX: FALLBACK ---
            if absoluteBestMove.IsNone then
                let legalMoves = MoveGen.getLegalMoves b
                if legalMoves.Length > 0 then
                    absoluteBestMove <- Some legalMoves.[0]

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
            |> List.filter (fun p -> Piece.colour p = Colour.White && Piece.kind p = PieceType.King)
            |> List.length

        let blackKings =
            pieces
            |> List.filter (fun p -> Piece.colour p = Colour.Black && Piece.kind p = PieceType.King)
            |> List.length

        if whiteKings <> 1 then
            errors.Add(sprintf "Invalid White King count: %d" whiteKings)

        if blackKings <> 1 then
            errors.Add(sprintf "Invalid Black King count: %d" blackKings)

        // 2. Check Pawns
        // Replaced b.Pieces loop with the allPiecesList
        for (sq, p) in allPiecesList do
            if Piece.kind p = PieceType.Pawn then
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

                if Board.isSquareAttacked b sq attacker then
                    printf "x "
                else
                    printf ". "

            printfn ""

        printfn "  a b c d e f g h"

module UciLoop =
    let startFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
    let mutable board = Board.fromFen startFen
    let mutable positionHistory: uint64 list = []
    // Track the current search task and its cancellation token
    let mutable searchCts = new CancellationTokenSource()
    let mutable currentSearchId = 0

    let tryGetIntArg (key: string) (args: string list) =
        args
        |> List.tryFindIndex ((=) key)
        |> Option.bind (fun i ->
            if i < args.Length - 1 then
                match Int32.TryParse(args.[i + 1]) with
                | true, value -> Some value
                | _ -> None
            else
                None)

    let calculateTargetTime (sideToMove: Colour) (args: string list) =
        let wtime = tryGetIntArg "wtime" args |> Option.defaultValue 100000
        let btime = tryGetIntArg "btime" args |> Option.defaultValue 100000
        let winc = tryGetIntArg "winc" args |> Option.defaultValue 0
        let binc = tryGetIntArg "binc" args |> Option.defaultValue 0

        let myTime, myIncrement =
            if sideToMove = Colour.White then
                wtime, winc
            else
                btime, binc

        Math.Max(1, (myTime / 20) + (myIncrement / 2))

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
            | "ucinewgame" :: _ -> 
                searchCts.Cancel() // Stop any running search immediately
                TranspositionTable.clear()
                Search.clearKillers() 
                Search.clearHistory() 
                board <- Board.fromFen startFen            
                positionHistory <- []
            
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
                positionHistory <- [ board.Hash ]

                for mStr in moveParts do
                    let m = Board.fromUci board mStr
                    board <- Board.applyMove m board
                    positionHistory <- board.Hash :: positionHistory

                TranspositionTable.advanceAge()
            
            | "go" :: rest ->
                // 1. Calculate time from remaining clock plus side-to-move increment.
                let targetTime = calculateTargetTime board.SideToMove rest

                // 3. Prepare Cancellation
                searchCts.Cancel()
                searchCts <- new CancellationTokenSource()
                currentSearchId <- currentSearchId + 1
                // 2. CAPTURE CURRENT STATE FOR THE ASYNC THREAD
                let mySearchId = currentSearchId
                let currentCts = searchCts 
                let searchBoard = board 
                let currentHistory = positionHistory

                // --- THE ALARM CLOCK ---
                // This tells the token to automatically trigger 'Cancel' after targetTime ms
                let depthIdx = rest |> List.tryFindIndex (fun s -> s = "depth")
                if depthIdx.IsNone then
                    searchCts.CancelAfter(targetTime)

                let depth =
                    match depthIdx with
                    | Some i when i < rest.Length - 1 ->
                        match Int32.TryParse(rest.[i + 1]) with
                        | true, d -> d | _ -> 4
                    | _ -> 20

                Async.Start(
                    async {
                        try                        
                            // Use the token for the search itself
                            let! result = Search.findBestMove searchBoard depth targetTime currentHistory currentCts.Token
        
                            if mySearchId = currentSearchId then
                                match result with
                                | Some m -> printfn "bestmove %s" (Move.toUci m)
                                | None -> 
                                    // Fallback: If search failed, try to find any legal move
                                    let legals = MoveGen.getLegalMoves searchBoard
                                    if legals.Length > 0 then
                                        printfn "bestmove %s" (Move.toUci legals.[0])
                                    else
                                        printfn "bestmove (none)" // UCI standard for no legal moves
                        with
                        | ex -> printfn "info string Error in search: %s" ex.Message
                    }
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
            | "attacks" :: "white" :: _ -> Debug.displayAttackMap board Colour.White
            | "attacks" :: "black" :: _ -> Debug.displayAttackMap board Colour.Black
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
