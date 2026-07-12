namespace Floundroid

open System
open System.Text
open System.Runtime.Intrinsics.X86

module Bitboard =
    let empty: Bitboard = 0uL
    let all: Bitboard = 0xFFFFFFFFFFFFFFFFuL

    /// Sets the bit at the given square.
    let set (sq: int) (bb: Bitboard) : Bitboard = bb ||| (1uL <<< sq)

    /// Clears the bit at the given square.
    let clear (sq: int) (bb: Bitboard) : Bitboard = bb &&& ~~~(1uL <<< sq)

    /// Checks if a square is set.
    let contains (sq: int) (bb: Bitboard) : bool = (bb &&& (1uL <<< sq)) <> 0uL

    /// Returns the number of set bits (population count).
    let count (bb: Bitboard) : int =
        System.Numerics.BitOperations.PopCount(bb)

    /// Returns 64 if the bitboard is empty (standard for TrailingZeroCount).
    let lsb (bb: Bitboard) : int =
        System.Numerics.BitOperations.TrailingZeroCount(bb)
    
    /// Returns the index of the least significant bit (0-63) and clears it from the bitboard.
    /// This is a high-performance way to iterate through pieces.
    let popLsb (bb: byref<Bitboard>) : int =
        let lsb = System.Numerics.BitOperations.TrailingZeroCount(bb)
        bb <- bb &&& (bb - 1uL)
        lsb

    let bits (bb: Bitboard) : seq<int> =
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

module Pst =
    // Tapered Material: (Middlegame, Endgame)
    let matsMG = [| 103; 412; 403; 514; 1120; 0 |]
    let matsEG = [| 103; 332; 331; 622; 1098; 0 |]

    let MG = [|
        // Pawn
        [|   0;   0;   0;   0;   0;   0;   0;   0; -32; -12; -25; -23; -18;  28;  36; -17; -21; -19;   0;  -1;  15;  22;  33;  -1; -30; -17;   4;  25;  31;  24;   6; -22; -17;   7;   9;  34;  35;  26;  19; -20; -14; -13;  17;  13;  58;  67;  25; -16; 128; 143;  96; 101;  87;  93;  66;  65;   0;   0;   0;   0;   0;   0;   0;   0 |]
        // Knight
        [| -50; -20; -35; -30; -10; -24; -16; -50; -40; -27; -12;   8;  10;  19; -20; -26; -30; -12;  10;  16;  27;  21;  23; -19; -20;   5;  14;   9;  30;  15;  14; -12; -23;  15;   5;  44;  14;  52;   3;   0; -30;  31;  16;  35;  47;  63;  36; -26; -54; -25;  35;   5;   5;  10; -19; -40; -111; -40; -30; -30; -30; -33; -40; -50 |]
        // Bishop
        [| -22;  -7;  -5; -13;  -9;  -8; -10; -20; -10;  22;   6;   1;   9;  17;  44;  -9;  -9;   7;  12;  -4;   8;  29;  13;  -1; -10;   0; -10;  14;  16;  -7;  -3;  -6; -10; -10;  -2;  23;  16;   6;  -6; -15; -27;   6;  11;   6;   9;   9;   4; -14; -18;   0; -22;  -6;   0;   6;   1; -58; -21; -10; -13; -10; -10; -10; -10; -20 |]
        // Rook
        [|   0;   1;   8;  22;  25;  26; -19;   2; -34; -12; -12;   0;   7;  15; -10; -58; -37; -22; -10; -13;   3;  -5; -10; -28; -43; -18;  -9;  -8;   0; -12;  -3; -25; -27; -14;   0;   0;  -1;   3;  -7; -13; -12;   0;   0;  -1;  -6;   2;  11;  -3;   4;   7;  26;  21;  22;  28;   7;   8;   2;   1;  -5;   2;   5;   0;   0;   0 |]
        // Queen
        [|  -2; -10;   6;  23;   1; -16; -10; -20; -29;  -9;  13;  13;  22;  12;  -6;  -8; -19;   0; -10;  -4;  -3;   0;   9;  -3; -15; -33; -17; -21; -12;  -5;  -3;  -2; -26; -32; -25; -31; -11;   4;   0; -13; -15; -14;  -5;  -1;  13;  31;  27;  38; -29; -56;  -2;   1;   1;  20;  12;  25; -20;  -7;  -8;  -5;  25;   0;  -7;  16 |]
        // King
        [|   3;  55;  32; -54;  21; -24;  49;  41;  19;  20;  -7; -64; -48; -25;  23;  30; -10; -19; -26; -36; -38; -32;  -4; -13; -20; -30; -33; -53; -56; -40; -32; -28; -30; -34; -40; -50; -50; -40; -27; -29; -30; -35; -40; -50; -50; -35; -16; -29; -30; -39; -40; -50; -50; -40; -40; -30; -30; -40; -40; -50; -50; -40; -40; -30 |]
    |]

    let EG = [|
        // Pawn
        [|   0;   0;   0;   0;   0;   0;   0;   0;  29;  11;  23;  17;  24;   8;  -2;   0;  17;  14;   2;   6;   5;   2;  -6;  -2;  29;  18;   3;  -7;  -5;  -7;   4;   8;  45;  27;  13;  -8;  -5;   2;  15;  25;  90;  83;  59;  23;   2;  22;  54;  69; 207; 196; 174; 154; 162; 155; 186; 211;   0;   0;   0;   0;   0;   0;   0;   0 |]
        // Knight
        [| -50; -46; -27;  -8; -15; -19; -45; -50; -30; -20;  -4;  -4;   1; -16; -19; -30; -22;   4;   1;  16;  11;  -3; -18; -21; -12;  -6;  19;  33;  17;  18;   3; -20;  -9;   8;  28;  26;  26;  12;   7; -13; -30; -10;  15;  12;  -2;   2; -15; -30; -40; -17; -13;   1; -13; -22; -20; -40; -56; -40; -28; -30; -28; -35; -40; -56 |]
        // Bishop
        [| -20;  -5;  -8;   4;   5;  -3; -10; -19; -10; -18;  -4;   2;   3;  -2; -14; -13;  -6;   0;   6;   9;  14;  -3;   0;  -5;  -3;   3;  12;  11;  -3;   6;  -1;  -5;   3;  13;  10;   3;   4;   4;   2;   2;   8;   1;   3;  -5;  -8;   4;   1;   1;  -9;   0;   0; -10;   0;  -1;  -2; -14; -20; -10; -10; -10; -10; -10; -10; -20 |]
        // Rook
        [|   0;   1;   2;  -8; -11;  -9;   1; -20;   3;  -2;   0;   1; -10; -11;  -8;   3;   3;   3;  -7;  -3; -12; -13;  -7; -10;  12;   5;   6;   0;  -6;  -8;  -9;  -6;   9;   3;   8;  -2;  -2;   3;  -7;   3;   6;   6;   2;   4;  -3;  -1;  -1;  -3;  10;  12;   8;  10;  -3;   4;   8;   7;  14;   9;  14;  12;  11;   9;   7;  10 |]
        // Queen
        [| -17; -14; -10; -17;   4; -10; -10; -20; -10;  -6; -23;  -6;  -3;   5;  -1; -10; -10; -20;   5;  -5;   6;  10;   7;  -1;  -5;   7;   2;  17;   8;   9;  17;  11;   1;   5;  -1;   5;  16;  11;  28;  33; -10;  -3;  -3;  12;  18;  14;   6;  12; -14;  -5;   2;   9;   6;   9;   4;  -1; -19;   3;   3;   5;  13;   4;  -4;  11 |]
        // King
        [| -86; -67; -46; -24; -50; -26; -57; -89; -51; -29;  -4;  10;  13;   3; -19; -44; -31; -10;   7;  17;  21;  12;  -3; -27; -34;  -9;  19;  26;  28;  21;   3; -30; -23;  17;  26;  29;  27;  34;  23; -12;  -6;  15;  23;  19;  20;  44;  42;  -1; -30;   8;   6;   7;   5;  30;  10; -16; -50; -40; -30; -20; -20; -28; -32; -50 |]
    |]

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
    let getPieceAt (sq: int) (bbs: BitboardSet) : int =
        let bit = 1uL <<< sq

        if (bbs.Occupancy &&& bit) = 0uL then
            -1
        else
            let color = if (bbs.WhiteTotal &&& bit) <> 0uL then Colour.White else Colour.Black

            let kind =
                if color = Colour.White then
                    if (bbs.WhitePawns &&& bit) <> 0uL then PieceType.Pawn
                    elif (bbs.WhiteKnights &&& bit) <> 0uL then PieceType.Knight
                    elif (bbs.WhiteBishops &&& bit) <> 0uL then PieceType.Bishop
                    elif (bbs.WhiteRooks &&& bit) <> 0uL then PieceType.Rook
                    elif (bbs.WhiteQueens &&& bit) <> 0uL then PieceType.Queen
                    else PieceType.King
                else
                    if (bbs.BlackPawns &&& bit) <> 0uL then PieceType.Pawn
                    elif (bbs.BlackKnights &&& bit) <> 0uL then PieceType.Knight
                    elif (bbs.BlackBishops &&& bit) <> 0uL then PieceType.Bishop
                    elif (bbs.BlackRooks &&& bit) <> 0uL then PieceType.Rook
                    elif (bbs.BlackQueens &&& bit) <> 0uL then PieceType.Queen
                    else PieceType.King

            (color <<< 3) ||| kind

    /// A helper to flip a piece on/off. Essential for incremental updates.
    let togglePiece (p: int) (sq: int) (bbs: BitboardSet) =
        let bit = 1uL <<< sq
        let color = Piece.colour p
        let kind = Piece.kind p

        if color = Colour.White then
            let newBbs =
                match kind with
                | PieceType.Pawn -> { bbs with WhitePawns = bbs.WhitePawns ^^^ bit }
                | PieceType.Knight -> { bbs with WhiteKnights = bbs.WhiteKnights ^^^ bit }
                | PieceType.Bishop -> { bbs with WhiteBishops = bbs.WhiteBishops ^^^ bit }
                | PieceType.Rook -> { bbs with WhiteRooks = bbs.WhiteRooks ^^^ bit }
                | PieceType.Queen -> { bbs with WhiteQueens = bbs.WhiteQueens ^^^ bit }
                | PieceType.King -> { bbs with WhiteKings = bbs.WhiteKings ^^^ bit }
                | _ -> invalidArg "p" $"Invalid kind: %A{kind}"

            let whiteTotal =
                newBbs.WhitePawns
                ||| newBbs.WhiteKnights
                ||| newBbs.WhiteBishops
                ||| newBbs.WhiteRooks
                ||| newBbs.WhiteQueens
                ||| newBbs.WhiteKings

            { newBbs with
                WhiteTotal = whiteTotal
                Occupancy = whiteTotal ||| bbs.BlackTotal }
        else
            let newBbs =
                match kind with
                | PieceType.Pawn -> { bbs with BlackPawns = bbs.BlackPawns ^^^ bit }
                | PieceType.Knight -> { bbs with BlackKnights = bbs.BlackKnights ^^^ bit }
                | PieceType.Bishop -> { bbs with BlackBishops = bbs.BlackBishops ^^^ bit }
                | PieceType.Rook -> { bbs with BlackRooks = bbs.BlackRooks ^^^ bit }
                | PieceType.Queen -> { bbs with BlackQueens = bbs.BlackQueens ^^^ bit }
                | PieceType.King -> { bbs with BlackKings = bbs.BlackKings ^^^ bit }
                | _ -> invalidArg "p" $"Invalid kind: %A{kind}"

            let blackTotal =
                newBbs.BlackPawns
                ||| newBbs.BlackKnights
                ||| newBbs.BlackBishops
                ||| newBbs.BlackRooks
                ||| newBbs.BlackQueens
                ||| newBbs.BlackKings

            { newBbs with
                BlackTotal = blackTotal
                Occupancy = bbs.WhiteTotal ||| blackTotal }
    
    /// Returns a sequence of all (Square, Piece) pairs currently on the board.
    let allPieces (bbs: BitboardSet) =
        seq {
            let mutable occ = bbs.Occupancy

            while occ <> 0uL do
                let sq = Bitboard.popLsb &occ
                yield (sq, getPieceAt sq bbs)
        }

    /// Evaluates the board using piece-square tables and tapered material.
    let getscr (bbs: BitboardSet) =
        let mutable mg = 0
        let mutable eg = 0

        let evalLayerW (bb: Bitboard) (kIdx: int) =
            let mutable tempBb = bb
            let tableMG = Pst.MG.[kIdx]
            let tableEG = Pst.EG.[kIdx]
            
            while tempBb <> 0uL do
                let sq = Bitboard.popLsb &tempBb
                let pstIdx = sq
                mg <- mg + Pst.matsMG.[kIdx] + tableMG.[pstIdx]
                eg <- eg + Pst.matsEG.[kIdx] + tableEG.[pstIdx]
        let evalLayerB (bb: Bitboard) (kIdx: int) =
            let mutable tempBb = bb
            let tableMG = Pst.MG.[kIdx]
            let tableEG = Pst.EG.[kIdx]
            
            while tempBb <> 0uL do
                let sq = Bitboard.popLsb &tempBb
                let pstIdx = sq ^^^ 56
                mg <- mg - Pst.matsMG.[kIdx] - tableMG.[pstIdx]
                eg <- eg - Pst.matsEG.[kIdx] - tableEG.[pstIdx]

        // --- THE 12 LAYERS (Must include all) ---
        evalLayerW bbs.WhitePawns    PieceType.Pawn   
        evalLayerW bbs.WhiteKnights  PieceType.Knight
        evalLayerW bbs.WhiteBishops  PieceType.Bishop
        evalLayerW bbs.WhiteRooks    PieceType.Rook
        evalLayerW bbs.WhiteQueens   PieceType.Queen
        evalLayerW bbs.WhiteKings    PieceType.King

        evalLayerB bbs.BlackPawns    PieceType.Pawn   
        evalLayerB bbs.BlackKnights  PieceType.Knight
        evalLayerB bbs.BlackBishops  PieceType.Bishop
        evalLayerB bbs.BlackRooks    PieceType.Rook   
        evalLayerB bbs.BlackQueens   PieceType.Queen  
        evalLayerB bbs.BlackKings    PieceType.King   
        mg,eg

module Magic =
    /// Generates a bitboard of all squares a Bishop attacks from a given square, 
    /// accounting for blockers. This is the "slow" version used for table init.
    let bishopAttacks (sq: int) (blockers: Bitboard) =
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
    let rookAttacks (sq: int) (blockers: Bitboard) =
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
    let bishopMask (sq: int) =
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

    let rookMask (sq: int) =
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

    /// Portable PEXT: uses BMI2.Pext when available, falls back to software implementation.
    let pext (src: uint64) (mask: uint64) : uint64 =
        if mask = 0uL then 0uL
        elif Bmi2.X64.IsSupported then
            Bmi2.X64.ParallelBitExtract(src, mask)
        else
            let mutable result = 0uL
            let mutable bit = 1uL
            let mutable m = mask
            while m <> 0uL do
                // lowest set bit of m
                let lsb = m &&& (uint64 (-(int64 m)))
                if (src &&& lsb) <> 0uL then result <- result ||| bit
                bit <- bit <<< 1
                m <- m &&& (m - 1uL)
            result

    /// Portable PDEP: uses BMI2.Pdep when available, falls back to software implementation.
    let pdep (src: uint64) (mask: uint64) : uint64 =
        if mask = 0uL then 0uL
        elif Bmi2.X64.IsSupported then
            Bmi2.X64.ParallelBitDeposit(src, mask)
        else
            let mutable result = 0uL
            let mutable s = src
            let mutable m = mask
            while m <> 0uL do
                let lsb = m &&& (uint64 (-(int64 m)))
                if (s &&& 1uL) <> 0uL then result <- result ||| lsb
                s <- s >>> 1
                m <- m &&& (m - 1uL)
            result
    
    /// This maps an occupancy bitboard to a unique index from 0 to 2^bits-1
    /// Prefer hardware PEXT via Intrinsics when available; fallback to software if not.
    let getIndex (occ: Bitboard) (mask: Bitboard) : int =
        if mask = 0uL then 0
        else
            let idx = pext occ mask
            int idx

    /// Generates every possible blocker pattern for a mask (reverse of getIndex)
    /// Prefer hardware PDEP via Intrinsics when available; fallback to software when not.
    let private getBlockers (index: int) (mask: Bitboard) : Bitboard =
        if mask = 0uL then 0uL
        else
            let src = uint64 index
            pdep src mask

    /// Initializes the sliding attack tables for bishops and rooks.
    let init () =
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

    let passedPawnMasks = Array2D.create 2 64 0uL
    let passedPawnBonusTableMG = Array2D.create 2 64 0
    let passedPawnBonusTableEG = Array2D.create 2 64 0

    let FileMasks = Array.init 8 (fun f ->
        let mutable mask = 0uL
        for r in 0..7 do mask <- mask ||| (1uL <<< (r * 8 + f))
        mask)

    let AdjacentFileMasks = Array.init 8 (fun f ->
        let mutable mask = 0uL
        if f > 0 then mask <- mask ||| FileMasks.[f - 1]
        if f < 7 then mask <- mask ||| FileMasks.[f + 1]
        mask)

    let ppbonusesMG = [| 0; 5; 10; 20; 35; 50; 70; 0 |]
    let ppbonusesEG = [| 0; 10; 20; 40; 60; 100; 140; 0 |] 

    let pp_init () =

        for sq in 0..63 do
            let file = sq % 8
            let rank = sq / 8
            
            // 1. Calculate Masks
            let mutable whiteMask = 0uL
            let mutable blackMask = 0uL
            
            for f in Math.Max(0, file - 1) .. Math.Min(7, file + 1) do
                for r in 0..7 do
                    let targetSq = r * 8 + f
                    if r > rank then whiteMask <- whiteMask ||| (1uL <<< targetSq)
                    if r < rank then blackMask <- blackMask ||| (1uL <<< targetSq)
            
            passedPawnMasks.[0, sq] <- whiteMask // White (0)
            passedPawnMasks.[1, sq] <- blackMask // Black (1)

            // 2. Calculate Bonus Table (pre-indexed by square for speed)
            passedPawnBonusTableMG.[0, sq] <- ppbonusesMG.[rank]
            passedPawnBonusTableMG.[1, sq] <- ppbonusesMG.[7 - rank]
            passedPawnBonusTableEG.[0, sq] <- ppbonusesEG.[rank]
            passedPawnBonusTableEG.[1, sq] <- ppbonusesEG.[7 - rank]

    // Initialize the tables immediately
    do 
        initializeLeapers ()
        pp_init()
        Magic.init ()
