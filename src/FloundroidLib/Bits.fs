namespace Floundroid

open System
open System.Text

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
    let matsMG = [| 91; 361; 374; 469; 991; 0 |]
    let matsEG = [| 115; 314; 316; 576; 1015; 0 |]
    
    let MG = [|
        // Pawn
        [|   0;   0;   0;   0;   0;   0;   0;   0; -28;   5; -23; -15;  -8;  30;  42; -17; -21;  -4;  -1;  -6;   9;   8;  32; -10; -30;  -3;   1;  16;  22;   9;   6; -29; -18;  18;   3;  27;  25;  12;  18; -28;   5;  23;  40;  39;  59;  53;  33;  -9; 134; 146;  99;  98;  87;  91;  72;  73;   0;   0;   0;   0;   0;   0;   0;   0 |]
        // Knight
        [| -50; -20; -34; -30; -16; -26; -17; -50; -40; -27; -16;   3;   3;  12; -20; -29; -31; -14;   7;  13;  19;  18;  16; -21; -23;   5;  12;   5;  24;  13;  12; -14; -23;  10;   7;  37;   6;  41;  -4;  -3; -30;  23;  14;  31;  37;  48;  31; -26; -47; -22;  29;   5;   5;   7; -19; -40; -100; -40; -30; -30; -30; -32; -40; -50 |]
        // Bishop
        [| -21;  -7;  -4; -10;  -8;  -8; -10; -20; -10;  21;   7;  -1;   6;  13;  37;  -9;  -8;   8;  12;  -3;   6;  24;   9;  -3; -10;   1;  -9;  13;  14;  -6;  -3;  -6; -10; -11;   1;  15;  18;   7;  -5; -12; -23;   4;   8;   9;  10;   9;   4; -10; -18;   0; -14;  -3;   0;   5;   2; -51; -21; -10; -10; -10; -10; -10; -10; -20 |]
        // Rook
        [|  -8;  -5;  13;  23;  24;  18; -25;  -6; -35; -11;  -9;  -3;   2;   7; -10; -54; -35; -19;  -6;  -9;   0;  -5;  -9; -28; -33; -10;  -3;  -5;   0; -10;  -1; -19; -13; -10;   4;   1;   3;   3;  -6;  -6;  -4;   2;   2;   1;   0;   1;   5;  -2;  10;  16;  26;  22;  21;  24;  11;   8;   3;   2;   0;   7;   5;   0;   0;   0 |]
        // Queen
        [|  -2; -12;   0;  16;  -3; -14; -10; -20; -26;  -9;  10;   7;  15;   8;  -6; -10; -15;  -2;  -8;  -3;  -4;   0;   7;  -5; -14; -28; -12; -14;  -9;  -4;   0;  -1; -22; -26; -19; -23;   1;   5;   0;  -7; -15; -13;  -6;   3;  10;  25;  25;  35; -27; -47;   0;   0;   1;  20;  12;  23; -20;  -7;  -8;  -5;  12;  -1;  -8;   7 |]
        // King
        [|   3;  50;  28; -48;  16; -20;  53;  33;  19;  18;  -3; -49; -39; -13;  28;  30; -10; -19; -28; -34; -38; -34;  -8; -11; -20; -30; -33; -52; -53; -39; -30; -22; -30; -34; -40; -50; -50; -40; -27; -29; -30; -35; -40; -50; -50; -36; -16; -29; -30; -39; -40; -50; -50; -40; -40; -30; -30; -40; -40; -50; -50; -40; -40; -30 |]
    |]

    let EG = [|
        // Pawn
        [|   0;   0;   0;   0;   0;   0;   0;   0;  15;  10;  13;  13;  16;   0;   3; -10;   5;  12;  -7;   3;   0;  -6;  -2; -10;  19;  13;  -3;  -8; -10; -11;   5;   1;  42;  29;  19;   7;   0;   7;  21;  23; 114; 120;  97;  80;  68;  67;  96; 100; 204; 201; 172; 153; 158; 157; 178; 205;   0;   0;   0;   0;   0;   0;   0;   0 |]
        // Knight
        [| -50; -42; -30; -16; -20; -23; -42; -50; -30; -20; -12;  -8;  -3; -16; -20; -30; -26;  -5;  -6;   9;   6; -13; -17; -22; -17; -10;  14;  27;  15;  16;   0; -20; -11;   5;  20;  24;  27;  17;   8; -13; -30; -10;  12;  12;   9;  10; -10; -30; -40; -17; -10;   0;  -5; -15; -20; -40; -56; -40; -28; -30; -28; -32; -40; -54 |]
        // Bishop
        [| -20;  -8;  -8;   0;   1;  -2; -10; -20; -10; -18;  -4;   3;   6;  -2; -11; -11;  -5;   2;   5;   9;  12;  -1;   2;  -3;  -3;   4;  11;  11;   0;   5;  -1;  -6;   1;  14;  10;   7;   5;   9;   3;   1;   4;   0;   3;  -1;  -3;   5;   1;   0; -10;   0;   0; -10;   0;   0;  -2; -14; -20; -10; -10; -10; -10; -10; -10; -20 |]
        // Rook
        [|  -6;   1;  -1;  -5;  -8; -11;  -2; -24;  -4;  -4;   2;   5;  -3;  -2;  -6;  -4;  -3;   0;  -6;  -2;  -7;  -9;  -4; -10;   4;   0;   4;  -1;  -6;  -6;  -4;  -9;   2;  -1;  10;   2;   0;   3;  -5;  -2;   7;   9;   6;   7;   0;  -1;   1;  -2;  13;  13;  15;  15;   5;   7;   7;   8;  16;  11;  14;  14;  13;   9;   6;   6 |]
        // Queen
        [| -17; -12; -10;  -2;   3; -10; -10; -20; -10;  -5; -17;   1;   4;   5;  -1; -10; -10; -14;   3;  -4;   5;   8;   7;  -2;  -5;   4;   1;   8;   3;   7;  15;  10;   0;   3;  -4;  -3;   9;   7;  23;  27; -10;  -3;  -3;   8;  11;  12;   6;  12; -12;  -9;   1;   4;   5;   6;   4;  -1; -19;  -3;  -1;   0;   9;   0;  -7;   6 |]
        // King
        [| -78; -56; -38; -25; -44; -26; -51; -76; -45; -26;  -6;   2;   7;  -3; -20; -41; -30; -10;   6;  13;  17;  11;  -1; -23; -30;  -8;  18;  23;  25;  18;   2; -28; -23;  16;  26;  29;  25;  30;  19; -11;  -6;  15;  23;  19;  20;  42;  39;  -1; -30;   7;   6;   6;   3;  29;   9; -16; -50; -40; -30; -20; -20; -28; -32; -50 |]
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
    let passedPawnBonusTable = Array2D.create 2 64 0

    let FileMasks = Array.init 8 (fun f ->
        let mutable mask = 0uL
        for r in 0..7 do mask <- mask ||| (1uL <<< (r * 8 + f))
        mask)

    let AdjacentFileMasks = Array.init 8 (fun f ->
        let mutable mask = 0uL
        if f > 0 then mask <- mask ||| FileMasks.[f - 1]
        if f < 7 then mask <- mask ||| FileMasks.[f + 1]
        mask)

    let pp_init () =
        let bonuses = [| 0; 0; 6; 12; 20; 35; 55; 0 |]

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
            passedPawnBonusTable.[0, sq] <- bonuses.[rank]
            passedPawnBonusTable.[1, sq] <- bonuses.[7 - rank]

    // Initialize the tables immediately
    do 
        initializeLeapers ()
        pp_init()
        Magic.init ()
