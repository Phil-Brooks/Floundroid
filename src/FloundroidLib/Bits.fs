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
    let matsMG = [| 100; 320; 330; 500; 900; 0 |]
    let matsEG = [| 120; 310; 320; 550; 950; 0 |]
    let MG =
        [|
          [|0; 0; 0; 0; 0; 0; 0; 0; 5; 10; 10; -20; -20; 10; 10; 5; 5; -5; -10; 0; 0;
            -10; -5; 5; 0; 0; 0; 20; 20; 0; 0; 0; 5; 5; 10; 25; 25; 10; 5; 5; 10; 10;
            20; 30; 30; 20; 10; 10; 50; 50; 50; 50; 50; 50; 50; 50; 0; 0; 0; 0; 0; 0;
            0; 0|]

          [|-50; -40; -30; -30; -30; -30; -40; -50; -40; -20; 0; 0; 0; 0; -20; -40;
            -30; 0; 10; 15; 15; 10; 0; -30; -30; 5; 15; 20; 20; 15; 5; -30; -30; 0; 15;
            20; 20; 15; 0; -30; -30; 5; 10; 15; 15; 10; 5; -30; -40; -20; 0; 5; 5; 0;
            -20; -40; -50; -40; -30; -30; -30; -30; -40; -50|]

          [|-20; -10; -10; -10; -10; -10; -10; -20; -10; 5; 0; 0; 0; 0; 5; -10; -10;
            10; 10; 10; 10; 10; 10; -10; -10; 0; 10; 10; 10; 10; 0; -10; -10; 5; 5; 10;
            10; 5; 5; -10; -10; 0; 5; 10; 10; 5; 0; -10; -10; 0; 0; 0; 0; 0; 0; -10;
            -20; -10; -10; -10; -10; -10; -10; -20|]
 
          [|0; 0; 0; 5; 5; 0; 0; 0; -5; 0; 0; 0; 0; 0; 0; -5; -5; 0; 0; 0; 0; 0; 0; -5;
            -5; 0; 0; 0; 0; 0; 0; -5; -5; 0; 0; 0; 0; 0; 0; -5; -5; 0; 0; 0; 0; 0; 0;
            -5; 5; 10; 10; 10; 10; 10; 10; 5; 0; 0; 0; 0; 0; 0; 0; 0|]

          [|-20; -10; -10; -5; -5; -10; -10; -20; -10; 0; 0; 0; 0; 5; 0; -10; -10; 0;
            5; 5; 5; 5; 5; -10; -5; 0; 5; 5; 5; 5; 0; 0; -5; 0; 5; 5; 5; 5; 0; -5; -10;
            0; 5; 5; 5; 5; 0; -10; -10; 0; 0; 0; 0; 0; 0; -10; -20; -10; -10; -5; -5;
            -10; -10; -20|]

          [|20; 30; 10; 0; 0; 10; 30; 20; 20; 20; 0; 0; 0; 0; 20; 20; -10; -20; -20;
            -20; -20; -20; -20; -10; -20; -30; -30; -40; -40; -30; -30; -20; -30; -40;
            -40; -50; -50; -40; -40; -30; -30; -40; -40; -50; -50; -40; -40; -30; -30;
            -40; -40; -50; -50; -40; -40; -30; -30; -40; -40; -50; -50; -40; -40; -30|]
            |]
    let EG =
        [|
          // 0. Pawn: Focus on advancing. High rewards for 6th and 7th ranks.
          [| 0;  0;  0;  0;  0;  0;  0;  0;
             0;  0;  0;  0;  0;  0;  0;  0;
            10; 10; 10; 10; 10; 10; 10; 10;
            20; 20; 20; 20; 20; 20; 20; 20;
            40; 40; 40; 40; 40; 40; 40; 40;
            60; 60; 60; 60; 60; 60; 60; 60;
            90; 90; 90; 90; 90; 90; 90; 90;
             0;  0;  0;  0;  0;  0;  0;  0 |]

          // 1. Knight: Centralization is still key, but less fear of the rim than MG.
          [|-50;-40;-30;-30;-30;-30;-40;-50;
            -30;-20;-10;  0;  0;-10;-20;-30;
            -30;-10; 10; 15; 15; 10;-10;-30;
            -30;-10; 15; 20; 20; 15;-10;-30;
            -30;-10; 15; 20; 20; 15;-10;-30;
            -30;-10; 10; 15; 15; 10;-10;-30;
            -40;-20;-10;  0;  0;-10;-20;-40;
            -50;-40;-30;-30;-30;-30;-40;-50 |]

          // 2. Bishop: Stay active, avoid corners.
          [|-20;-10;-10;-10;-10;-10;-10;-20;
            -10;  0;  0;  0;  0;  0;  0;-10;
            -10;  0;  5; 10; 10;  5;  0;-10;
            -10;  5; 10; 15; 15; 10;  5;-10;
            -10;  5; 10; 15; 15; 10;  5;-10;
            -10;  0;  5; 10; 10;  5;  0;-10;
            -10;  0;  0;  0;  0;  0;  0;-10;
            -20;-10;-10;-10;-10;-10;-10;-20 |]

          // 3. Rook: 7th rank bonus and general activity.
          [| 0;  0;  0;  0;  0;  0;  0;  0;
             5; 10; 10; 10; 10; 10; 10;  5;
            -5;  0;  0;  0;  0;  0;  0; -5;
            -5;  0;  0;  0;  0;  0;  0; -5;
            -5;  0;  0;  0;  0;  0;  0; -5;
            -5;  0;  0;  0;  0;  0;  0; -5;
             0;  0;  0;  5;  5;  0;  0;  0;
             0;  0;  0;  0;  0;  0;  0;  0 |]

          // 4. Queen: Centralization.
          [|-20;-10;-10; -5; -5;-10;-10;-20;
            -10;  0;  5;  5;  5;  5;  0;-10;
            -10;  5;  5;  5;  5;  5;  5;-10;
             -5;  5;  5;  5;  5;  5;  5; -5;
              0;  5;  5;  5;  5;  5;  5; -5;
            -10;  0;  5;  5;  5;  5;  0;-10;
            -10;  0;  0;  0;  0;  0;  0;-10;
            -20;-10;-10; -5; -5;-10;-10;-20 |]

          // 5. King: EXTREMELY IMPORTANT. The King must come to the center in EG.
          [|-50;-40;-30;-20;-20;-30;-40;-50;
            -30;-20;-10;  0;  0;-10;-20;-30;
            -30;-10; 20; 30; 30; 20;-10;-30;
            -30;-10; 30; 40; 40; 30;-10;-30;
            -30;-10; 30; 40; 40; 30;-10;-30;
            -30;-10; 20; 30; 30; 20;-10;-30;
            -30;-30;  0;  0;  0;  0;-30;-30;
            -50;-40;-30;-20;-20;-30;-40;-50 |]
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
