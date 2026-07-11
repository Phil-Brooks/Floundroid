namespace Floundroid

module Evaluation =
    // --- Weights & Phase Constants ---
    let MaxPhase = 24 // 4 knights (1), 4 bishops (1), 4 rooks (2), 2 queens (4)
    
    /// Weights for each square of mobility. Index matches PieceType (Knight=1, Bishop=2, etc.
    let mobWeightsMG = [| 0; 3; 6; 5; 1; 1 |]
    let mobWeightsEG = [| 0; 2; 4; 3; 9; 1 |]
    
    // King Attack weights: How scary is each piece near the king?
    let kingAttackWeightsMG = [| 0; 15; 12; 21; 9; 0 |]
    //let kingAttackWeightsEG = [| 0; -4; 0; -5; 33; 0 |] - manually fixed
    let kingAttackWeightsEG = [| 0; 0; 0; 0; 33; 0 |]

    //Bishop Pair Bonus.
    let mutable BishopPairBonusMG = 38
    let mutable BishopPairBonusEG = 56
    //Rook on Open/Half-Open File.
    let mutable RookOpenFileMG = 55
    let mutable RookOpenFileEG = -3
    let mutable RookHalfOpenFileMG = 14
    let mutable RookHalfOpenFileEG = 14
    //Pawn Shield: (Bonus for pawns in front of the King).
    let mutable PawnShieldBonusMG = 18
    let mutable PawnShieldBonusEG = -8
    //doubled pawns
    let mutable DoubledPawnPenaltyMG = 18
    let mutable DoubledPawnPenaltyEG = 27
    //isolated pawns
    let mutable IsolatedPawnPenaltyMG = 18
    let mutable IsolatedPawnPenaltyEG = 9

    /// Evaluates the pawn structure of the board, returning a score from White's perspective.
    let pawnStructureScore (b: Board) =
        let bbs = b.Bitboards
        //WHITE
        let mutable friendlyPawns = bbs.WhitePawns
        let mutable enemyPawns = bbs.BlackPawns
        let mutable mg = 0
        let mutable eg = 0
        let mutable cIdx = 0

        // --- 1. Doubled Pawns (Set-wise) ---
        // A pawn is doubled if there's another friendly pawn "north" of it.
        let doubled = friendlyPawns &&& (friendlyPawns <<< 8)
        // Your logic: score -6 for every "extra" pawn on a file.
        mg <- mg - (Bitboard.count doubled * DoubledPawnPenaltyMG)
        eg <- eg - (Bitboard.count doubled * DoubledPawnPenaltyEG)

        // --- 2. Isolated Pawns (Set-wise) ---
        // A pawn is isolated if (FriendlyPawns AND AdjacentFileMask) is empty.
        // We identify which files have pawns, then find files with no neighbors.
        let mutable fileMapping = friendlyPawns
        fileMapping <- fileMapping ||| (fileMapping >>> 32)
        fileMapping <- fileMapping ||| (fileMapping >>> 16)
        fileMapping <- fileMapping ||| (fileMapping >>> 8)
        let filesWithPawns = uint32 (fileMapping &&& 0xFFuL)

        let neighborFiles = ((filesWithPawns <<< 1) ||| (filesWithPawns >>> 1)) &&& 0xFFu
        let isolatedFilesMask = filesWithPawns &&& ~~~neighborFiles

        // Count how many pawns are on isolated files
        let mutable isolatedPawnCount = 0
        let mutable tempIsoFiles = isolatedFilesMask
        while tempIsoFiles <> 0u do
            let file = System.Numerics.BitOperations.TrailingZeroCount(tempIsoFiles)
            tempIsoFiles <- tempIsoFiles &&& (tempIsoFiles - 1u)
            isolatedPawnCount <- isolatedPawnCount + Bitboard.count (friendlyPawns &&& BitboardGen.FileMasks.[file])
        let totalPawns = Bitboard.count friendlyPawns
        mg <- mg + (totalPawns * 2) - (isolatedPawnCount * IsolatedPawnPenaltyMG)
        eg <- eg + (totalPawns * 2) - (isolatedPawnCount * IsolatedPawnPenaltyEG)

        // --- 3. Passed Pawns (The only remaining loop) ---
        // We only loop through pawns to check for passers.
        let mutable remaining = friendlyPawns
        while remaining <> 0uL do
            let sq = Bitboard.popLsb &remaining
            if (BitboardGen.passedPawnMasks.[cIdx, sq] &&& enemyPawns) = 0uL then
                mg <- mg + BitboardGen.passedPawnBonusTableMG.[cIdx, sq]
                eg <- eg + BitboardGen.passedPawnBonusTableEG.[cIdx, sq]
        
        //BLACK
        friendlyPawns <- bbs.BlackPawns
        enemyPawns <- bbs.WhitePawns
        cIdx <- 1
        // --- 1. Doubled Pawns (Set-wise) ---
        // A pawn is doubled if there's another friendly pawn "north" of it.
        let doubled = friendlyPawns &&& (friendlyPawns >>> 8)
        // Your logic: score -6 for every "extra" pawn on a file.
        mg <- mg + (Bitboard.count doubled * DoubledPawnPenaltyMG)
        eg <- eg + (Bitboard.count doubled * DoubledPawnPenaltyEG)

        // --- 2. Isolated Pawns (Set-wise) ---
        // A pawn is isolated if (FriendlyPawns AND AdjacentFileMask) is empty.
        // We identify which files have pawns, then find files with no neighbors.
        let mutable fileMapping = friendlyPawns
        fileMapping <- fileMapping ||| (fileMapping >>> 32)
        fileMapping <- fileMapping ||| (fileMapping >>> 16)
        fileMapping <- fileMapping ||| (fileMapping >>> 8)
        let filesWithPawns = uint32 (fileMapping &&& 0xFFuL)

        let neighborFiles = ((filesWithPawns <<< 1) ||| (filesWithPawns >>> 1)) &&& 0xFFu
        let isolatedFilesMask = filesWithPawns &&& ~~~neighborFiles

        // Count how many pawns are on isolated files
        let mutable isolatedPawnCount = 0
        let mutable tempIsoFiles = isolatedFilesMask
        while tempIsoFiles <> 0u do
            let file = System.Numerics.BitOperations.TrailingZeroCount(tempIsoFiles)
            tempIsoFiles <- tempIsoFiles &&& (tempIsoFiles - 1u)
            isolatedPawnCount <- isolatedPawnCount + Bitboard.count (friendlyPawns &&& BitboardGen.FileMasks.[file])
        let totalPawns = Bitboard.count friendlyPawns
        mg <- mg - (totalPawns * 2) + (isolatedPawnCount * IsolatedPawnPenaltyMG)
        eg <- eg - (totalPawns * 2) + (isolatedPawnCount * IsolatedPawnPenaltyEG)

        // --- 3. Passed Pawns (The only remaining loop) ---
        // We only loop through pawns to check for passers.
        let mutable remaining = friendlyPawns
        while remaining <> 0uL do
            let sq = Bitboard.popLsb &remaining
            if (BitboardGen.passedPawnMasks.[cIdx, sq] &&& enemyPawns) = 0uL then
                mg <- mg - BitboardGen.passedPawnBonusTableMG.[cIdx, sq]
                eg <- eg - BitboardGen.passedPawnBonusTableEG.[cIdx, sq]
        mg,eg
    
    let getAttackBitboard (sq: int) (kind: int) (occ: Bitboard) =
        match kind with
        | PieceType.Knight -> 
            BitboardGen.knightAttacks.[sq]
        | PieceType.Bishop ->
            let e = Magic.bishopEntries.[sq]
            Magic.bishopTable.[e.Offset + Magic.getIndex occ e.Mask]
        | PieceType.Rook ->
            let e = Magic.rookEntries.[sq]
            Magic.rookTable.[e.Offset + Magic.getIndex occ e.Mask]
        | PieceType.Queen ->
            let be = Magic.bishopEntries.[sq]
            let re = Magic.rookEntries.[sq]
            Magic.bishopTable.[be.Offset + Magic.getIndex occ be.Mask] ||| 
            Magic.rookTable.[re.Offset + Magic.getIndex occ re.Mask]
        | PieceType.King ->
            BitboardGen.kingAttacks.[sq]
        | _ -> 0uL // Pawns are usually handled via pawn-specific structure logic    
    
    /// Evaluates the board position from White's perspective. Positive scores favor White, negative scores favor Black.
    let evaluate (b: Board) =
        let bbs = b.Bitboards
        let occ = bbs.Occupancy
        
        // 1. Calculate Phase (0 = Endgame, 24 = Opening)
        let phase = 
            (Bitboard.count (bbs.WhiteKnights ||| bbs.BlackKnights) * 1) +
            (Bitboard.count (bbs.WhiteBishops ||| bbs.BlackBishops) * 1) +
            (Bitboard.count (bbs.WhiteRooks ||| bbs.BlackRooks) * 2) +
            (Bitboard.count (bbs.WhiteQueens ||| bbs.BlackQueens) * 4)
        let p = if phase > MaxPhase then MaxPhase else phase

        let mutable mg = b.ScoreMG
        let mutable eg = b.ScoreEG

        // King Safety setup
        let whiteKingSq = Bitboard.lsb bbs.WhiteKings
        let blackKingSq = Bitboard.lsb bbs.BlackKings
        let whiteKingZone = if whiteKingSq < 64 then BitboardGen.kingAttacks.[whiteKingSq] else 0uL
        let blackKingZone = if blackKingSq < 64 then BitboardGen.kingAttacks.[blackKingSq] else 0uL
        
        let mutable whiteKingAttacksCount = 0
        let mutable whiteKingAttackWeightMG = 0
        let mutable whiteKingAttackWeightEG = 0
        let mutable blackKingAttacksCount = 0
        let mutable blackKingAttackWeightMG = 0
        let mutable blackKingAttackWeightEG = 0

        // doing bbs.WhiteKnights  PieceType.Knight
        let mutable kIdx = PieceType.Knight
        let mutable tempBb = bbs.WhiteKnights
        let mutable usTotal = bbs.WhiteTotal
        let mutable enemyKingZone = blackKingZone
            
        while tempBb <> 0uL do
            let sq = Bitboard.popLsb &tempBb

            let attacks = getAttackBitboard sq kIdx occ
            let mob = Bitboard.count (attacks &&& ~~~usTotal)
                    
            mg <- mg + (mob * mobWeightsMG.[kIdx])
            eg <- eg + (mob * mobWeightsEG.[kIdx])

            let attacksOnZone = attacks &&& enemyKingZone
            if attacksOnZone <> 0uL then
                blackKingAttacksCount <- blackKingAttacksCount + 1
                blackKingAttackWeightMG <- blackKingAttackWeightMG + kingAttackWeightsMG.[kIdx]
                blackKingAttackWeightEG <- blackKingAttackWeightEG + kingAttackWeightsEG.[kIdx]

        // doing bbs.WhiteBishops  PieceType.Bishop
        kIdx <- PieceType.Bishop
        tempBb <- bbs.WhiteBishops
        usTotal <- bbs.WhiteTotal
        enemyKingZone <- blackKingZone
            
        while tempBb <> 0uL do
            let sq = Bitboard.popLsb &tempBb

            let attacks = getAttackBitboard sq kIdx occ
            let mob = Bitboard.count (attacks &&& ~~~usTotal)
                    
            mg <- mg + (mob * mobWeightsMG.[kIdx])
            eg <- eg + (mob * mobWeightsEG.[kIdx])

            let attacksOnZone = attacks &&& enemyKingZone
            if attacksOnZone <> 0uL then
                blackKingAttacksCount <- blackKingAttacksCount + 1
                blackKingAttackWeightMG <- blackKingAttackWeightMG + kingAttackWeightsMG.[kIdx]
                blackKingAttackWeightEG <- blackKingAttackWeightEG + kingAttackWeightsEG.[kIdx]

        // doing bbs.WhiteRooks    PieceType.Rook
        kIdx <- PieceType.Rook
        tempBb <- bbs.WhiteRooks
        usTotal <- bbs.WhiteTotal
        enemyKingZone <- blackKingZone
            
        while tempBb <> 0uL do
            let sq = Bitboard.popLsb &tempBb

            let file = sq % 8
            let fileMask = BitboardGen.FileMasks.[file]
    
            // Check if our pawns are on this file
            if (fileMask &&& bbs.WhitePawns) = 0uL then
                // If no enemy pawns either, it's fully open
                if (fileMask &&& bbs.BlackPawns) = 0uL then
                    mg <- mg + RookOpenFileMG; eg <- eg + RookOpenFileEG
                else
                    // Half-open
                    mg <- mg + RookHalfOpenFileMG; eg <- eg + RookHalfOpenFileEG
                
            let attacks = getAttackBitboard sq kIdx occ
            let mob = Bitboard.count (attacks &&& ~~~usTotal)
                    
            mg <- mg + (mob * mobWeightsMG.[kIdx])
            eg <- eg + (mob * mobWeightsEG.[kIdx])

            let attacksOnZone = attacks &&& enemyKingZone
            if attacksOnZone <> 0uL then
                blackKingAttacksCount <- blackKingAttacksCount + 1
                blackKingAttackWeightMG <- blackKingAttackWeightMG + kingAttackWeightsMG.[kIdx]
                blackKingAttackWeightEG <- blackKingAttackWeightEG + kingAttackWeightsEG.[kIdx]

        // doing bbs.WhiteQueens   PieceType.Queen
        kIdx <- PieceType.Queen
        tempBb <- bbs.WhiteQueens
        usTotal <- bbs.WhiteTotal
        enemyKingZone <- blackKingZone
            
        while tempBb <> 0uL do
            let sq = Bitboard.popLsb &tempBb

            let attacks = getAttackBitboard sq kIdx occ
            let mob = Bitboard.count (attacks &&& ~~~usTotal)
                    
            mg <- mg + (mob * mobWeightsMG.[kIdx])
            eg <- eg + (mob * mobWeightsEG.[kIdx])

            let attacksOnZone = attacks &&& enemyKingZone
            if attacksOnZone <> 0uL then
                blackKingAttacksCount <- blackKingAttacksCount + 1
                blackKingAttackWeightMG <- blackKingAttackWeightMG + kingAttackWeightsMG.[kIdx]
                blackKingAttackWeightEG <- blackKingAttackWeightEG + kingAttackWeightsEG.[kIdx]

        // doing bbs.WhiteKings    PieceType.King
        kIdx <- PieceType.King
        tempBb <- bbs.WhiteKings
        usTotal <- bbs.WhiteTotal
        enemyKingZone <- blackKingZone
            
        while tempBb <> 0uL do
            let sq = Bitboard.popLsb &tempBb

            if sq < 16 then  // Only shield if on back two ranks
                // Get the area "in front" of the king
                let shieldMask = BitboardGen.kingAttacks.[sq] &&& 0xFFFFFF00uL // Ranks 2+
                mg <- mg + Bitboard.count (shieldMask &&& bbs.WhitePawns) * PawnShieldBonusMG
                eg <- eg + Bitboard.count (shieldMask &&& bbs.WhitePawns) * PawnShieldBonusEG

        //doing bbs.BlackKnights  PieceType.Knight
        kIdx <- PieceType.Knight
        tempBb <- bbs.BlackKnights
        usTotal <- bbs.BlackTotal
        enemyKingZone <- whiteKingZone

        while tempBb <> 0uL do
            let sq = Bitboard.popLsb &tempBb

            let attacks = getAttackBitboard sq kIdx occ
            let mob = Bitboard.count (attacks &&& ~~~usTotal)
                    
            mg <- mg - (mob * mobWeightsMG.[kIdx])
            eg <- eg - (mob * mobWeightsEG.[kIdx])

            let attacksOnZone = attacks &&& enemyKingZone
            if attacksOnZone <> 0uL then
                whiteKingAttacksCount <- whiteKingAttacksCount + 1
                whiteKingAttackWeightMG <- whiteKingAttackWeightMG + kingAttackWeightsMG.[kIdx]
                whiteKingAttackWeightEG <- whiteKingAttackWeightEG + kingAttackWeightsEG.[kIdx]

        //doing bbs.BlackBishops  PieceType.Bishop
        kIdx <- PieceType.Bishop
        tempBb <- bbs.BlackBishops
        usTotal <- bbs.BlackTotal
        enemyKingZone <- whiteKingZone

        while tempBb <> 0uL do
            let sq = Bitboard.popLsb &tempBb

            let attacks = getAttackBitboard sq kIdx occ
            let mob = Bitboard.count (attacks &&& ~~~usTotal)
                    
            mg <- mg - (mob * mobWeightsMG.[kIdx])
            eg <- eg - (mob * mobWeightsEG.[kIdx])

            let attacksOnZone = attacks &&& enemyKingZone
            if attacksOnZone <> 0uL then
                whiteKingAttacksCount <- whiteKingAttacksCount + 1
                whiteKingAttackWeightMG <- whiteKingAttackWeightMG + kingAttackWeightsMG.[kIdx]
                whiteKingAttackWeightEG <- whiteKingAttackWeightEG + kingAttackWeightsEG.[kIdx]

        //doing bbs.BlackRooks    PieceType.Rook
        kIdx <- PieceType.Rook
        tempBb <- bbs.BlackRooks
        usTotal <- bbs.BlackTotal
        enemyKingZone <- whiteKingZone

        while tempBb <> 0uL do
            let sq = Bitboard.popLsb &tempBb

            let file = sq % 8
            let fileMask = BitboardGen.FileMasks.[file]
    
            // Check if our pawns are on this file
            if (fileMask &&& bbs.BlackPawns) = 0uL then
                // If no enemy pawns either, it's fully open
                if (fileMask &&& bbs.WhitePawns) = 0uL then
                    mg <- mg - RookOpenFileMG; eg <- eg - RookOpenFileEG
                else
                    // Half-open
                    mg <- mg - RookHalfOpenFileMG; eg <- eg - RookHalfOpenFileEG                
                
            let attacks = getAttackBitboard sq kIdx occ
            let mob = Bitboard.count (attacks &&& ~~~usTotal)
                    
            mg <- mg - (mob * mobWeightsMG.[kIdx])
            eg <- eg - (mob * mobWeightsEG.[kIdx])

            let attacksOnZone = attacks &&& enemyKingZone
            if attacksOnZone <> 0uL then
                whiteKingAttacksCount <- whiteKingAttacksCount + 1
                whiteKingAttackWeightMG <- whiteKingAttackWeightMG + kingAttackWeightsMG.[kIdx]
                whiteKingAttackWeightEG <- whiteKingAttackWeightEG + kingAttackWeightsEG.[kIdx]

        //doing bbs.BlackQueens   PieceType.Queen
        kIdx <- PieceType.Queen
        tempBb <- bbs.BlackQueens
        usTotal <- bbs.BlackTotal
        enemyKingZone <- whiteKingZone

        while tempBb <> 0uL do
            let sq = Bitboard.popLsb &tempBb

            let attacks = getAttackBitboard sq kIdx occ
            let mob = Bitboard.count (attacks &&& ~~~usTotal)
                    
            mg <- mg - (mob * mobWeightsMG.[kIdx])
            eg <- eg - (mob * mobWeightsEG.[kIdx])

            let attacksOnZone = attacks &&& enemyKingZone
            if attacksOnZone <> 0uL then
                whiteKingAttacksCount <- whiteKingAttacksCount + 1
                whiteKingAttackWeightMG <- whiteKingAttackWeightMG + kingAttackWeightsMG.[kIdx]
                whiteKingAttackWeightEG <- whiteKingAttackWeightEG + kingAttackWeightsEG.[kIdx]

        //doing bbs.bbs.BlackKings    PieceType.King
        kIdx <- PieceType.King
        tempBb <- bbs.BlackKings
        usTotal <- bbs.BlackTotal
        enemyKingZone <- whiteKingZone

        while tempBb <> 0uL do
            let sq = Bitboard.popLsb &tempBb

            if sq > 47 then  // Only shield if on back two ranks
                // Get the area "in front" of the king
                let shieldMask = BitboardGen.kingAttacks.[sq] &&& 0x00FFFFFFFFFFFF00uL // Ranks 7-
                mg <- mg - Bitboard.count (shieldMask &&& bbs.BlackPawns) * PawnShieldBonusMG
                eg <- eg - Bitboard.count (shieldMask &&& bbs.BlackPawns) * PawnShieldBonusEG

        // King Safety Calculation
        let whiteSafetyPenaltyMG = (whiteKingAttacksCount * whiteKingAttackWeightMG)
        let blackSafetyPenaltyMG = (blackKingAttacksCount * blackKingAttackWeightMG)
        mg <- mg + (blackSafetyPenaltyMG - whiteSafetyPenaltyMG)
        let whiteSafetyPenaltyEG = (whiteKingAttacksCount * whiteKingAttackWeightEG)
        let blackSafetyPenaltyEG = (blackKingAttacksCount * blackKingAttackWeightEG)
        eg <- eg + (blackSafetyPenaltyEG - whiteSafetyPenaltyEG)

        let psMG, psEG = pawnStructureScore b
        mg <- mg + psMG
        eg <- eg + psEG
        // Bishop Pair Bonus
        if Bitboard.count bbs.WhiteBishops >= 2 then
            mg <- mg + BishopPairBonusMG
            eg <- eg + BishopPairBonusEG
        if Bitboard.count bbs.BlackBishops >= 2 then
            mg <- mg - BishopPairBonusMG
            eg <- eg - BishopPairBonusEG

        // Tapered Result (White's perspective)
        let score = ((mg * p) + (eg * (MaxPhase - p))) / MaxPhase
        score
