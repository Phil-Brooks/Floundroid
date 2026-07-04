namespace Floundroid

module Evaluation =
    // --- Weights & Phase Constants ---
    let MaxPhase = 24 // 4 knights (1), 4 bishops (1), 4 rooks (2), 2 queens (4)
    
    /// Weights for each square of mobility (tuning values)
    let mobWeights = [| 0; 4; 4; 3; 2; 1; 0 |] // Index matches PieceType (Knight=1, Bishop=2, etc.)
    // King Attack weights: How scary is each piece near the king?
    let kingAttackWeights = [| 0; 2; 2; 3; 5; 0 |] 

    /// Evaluates the pawn structure of the board, returning a score from White's perspective.
    let pawnStructureScore (b: Board) =
        let evaluatePawnSide (colour: int) (friendlyPawns: Bitboard) (enemyPawns: Bitboard) =
            let mutable score = 0
            let cIdx = if colour = Colour.White then 0 else 1

            // --- 1. Doubled Pawns (Set-wise) ---
            // A pawn is doubled if there's another friendly pawn "north" of it.
            let doubled = 
                if colour = Colour.White then friendlyPawns &&& (friendlyPawns <<< 8)
                else friendlyPawns &&& (friendlyPawns >>> 8)
            // Your logic: score -6 for every "extra" pawn on a file.
            score <- score - (Bitboard.count doubled * 6)

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
            score <- score + (totalPawns * 2) - (isolatedPawnCount * 7)

            // --- 3. Passed Pawns (The only remaining loop) ---
            // We only loop through pawns to check for passers.
            let mutable remaining = friendlyPawns
            while remaining <> 0uL do
                let sq = Bitboard.popLsb &remaining
                if (BitboardGen.passedPawnMasks.[cIdx, sq] &&& enemyPawns) = 0uL then
                    score <- score + BitboardGen.passedPawnBonusTable.[cIdx, sq]

            score

        let bbs = b.Bitboards
        let whiteScore = evaluatePawnSide Colour.White bbs.WhitePawns bbs.BlackPawns
        let blackScore = evaluatePawnSide Colour.Black bbs.BlackPawns bbs.WhitePawns
        whiteScore - blackScore
    
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
        let mutable whiteKingAttackWeight = 0
        let mutable blackKingAttacksCount = 0
        let mutable blackKingAttackWeight = 0

        let evalLayerW (bb: Bitboard) (kIdx: int) =
            let mutable tempBb = bb
            let usTotal = bbs.WhiteTotal
            let enemyKingZone = blackKingZone
            
            while tempBb <> 0uL do
                let sq = Bitboard.popLsb &tempBb

                if kIdx <> PieceType.Pawn && kIdx <> PieceType.King then
                    let attacks = getAttackBitboard sq kIdx occ
                    let mob = Bitboard.count (attacks &&& ~~~usTotal)
                    
                    mg <- mg + (mob * mobWeights.[kIdx])
                    eg <- eg + (mob * mobWeights.[kIdx])

                    let attacksOnZone = attacks &&& enemyKingZone
                    if attacksOnZone <> 0uL then
                        blackKingAttacksCount <- blackKingAttacksCount + 1
                        blackKingAttackWeight <- blackKingAttackWeight + kingAttackWeights.[kIdx]
        let evalLayerB (bb: Bitboard) (kIdx: int) =
            let mutable tempBb = bb
            let usTotal = bbs.BlackTotal
            let enemyKingZone = whiteKingZone
            
            while tempBb <> 0uL do
                let sq = Bitboard.popLsb &tempBb

                if kIdx <> PieceType.Pawn && kIdx <> PieceType.King then
                    let attacks = getAttackBitboard sq kIdx occ
                    let mob = Bitboard.count (attacks &&& ~~~usTotal)
                    
                    mg <- mg - (mob * mobWeights.[kIdx])
                    eg <- eg - (mob * mobWeights.[kIdx])

                    let attacksOnZone = attacks &&& enemyKingZone
                    if attacksOnZone <> 0uL then
                        whiteKingAttacksCount <- whiteKingAttacksCount + 1
                        whiteKingAttackWeight <- whiteKingAttackWeight + kingAttackWeights.[kIdx]

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

        // King Safety Calculation
        let whiteSafetyPenalty = (whiteKingAttacksCount * whiteKingAttackWeight)
        let blackSafetyPenalty = (blackKingAttacksCount * blackKingAttackWeight)
        mg <- mg + (blackSafetyPenalty - whiteSafetyPenalty)

        // --- King Pawn Shield Bonus (Defenders) ---
        //if whiteKingSq < 64 then
        //    let shield = BitboardGen.kingAttacks.[whiteKingSq] &&& bbs.WhitePawns
        //    mg <- mg + (System.Numerics.BitOperations.PopCount(shield) * 15)

        //if blackKingSq < 64 then
        //    let shield = BitboardGen.kingAttacks.[blackKingSq] &&& bbs.BlackPawns
        //    mg <- mg - (System.Numerics.BitOperations.PopCount(shield) * 15)

        let ps = pawnStructureScore b
        mg <- mg + ps
        eg <- eg + ps

        // Tapered Result (White's perspective)
        let score = ((mg * p) + (eg * (MaxPhase - p))) / MaxPhase
        score
