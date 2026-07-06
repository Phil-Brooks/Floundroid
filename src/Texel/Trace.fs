namespace Texel

open Floundroid
open TuningData

module Trace =
    // Helper to count isolated pawns (mimics Evaluation logic)
    let getIsolatedPawnCount (friendlyPawns: Bitboard) =
        if friendlyPawns = 0uL then 0
        else
            let mutable fileMapping = friendlyPawns
            fileMapping <- fileMapping ||| (fileMapping >>> 32)
            fileMapping <- fileMapping ||| (fileMapping >>> 16)
            fileMapping <- fileMapping ||| (fileMapping >>> 8)
            let filesWithPawns = uint32 (fileMapping &&& 0xFFuL)

            let neighborFiles = ((filesWithPawns <<< 1) ||| (filesWithPawns >>> 1)) &&& 0xFFu
            let isolatedFilesMask = filesWithPawns &&& ~~~neighborFiles

            let mutable isolatedPawnCount = 0
            let mutable tempIsoFiles = isolatedFilesMask
            while tempIsoFiles <> 0u do
                let file = System.Numerics.BitOperations.TrailingZeroCount(tempIsoFiles)
                tempIsoFiles <- tempIsoFiles &&& (tempIsoFiles - 1u)
                isolatedPawnCount <- isolatedPawnCount + Bitboard.count (friendlyPawns &&& BitboardGen.FileMasks.[file])
            isolatedPawnCount    
   
    
    let getTrace (b: Board) =
        let traceMG = Array.zeroCreate<int16> TotalParams
        let traceEG = Array.zeroCreate<int16> TotalParams
        let bbs = b.Bitboards
        let occ = bbs.Occupancy

        // Helper to add material and PST to trace
        let addPiece pType sq color =
            let sideMult = if color = Colour.White then 1s else -1s
            let relSq = if color = Colour.White then sq else sq ^^^ 56
            
            traceMG.[MaterialOffset + pType] <- traceMG.[MaterialOffset + pType] + sideMult
            traceEG.[MaterialOffset + pType] <- traceEG.[MaterialOffset + pType] + sideMult
            
            traceMG.[PstOffset + (pType * 64) + relSq] <- traceMG.[PstOffset + (pType * 64) + relSq] + sideMult
            traceEG.[PstOffset + (pType * 64) + relSq] <- traceEG.[PstOffset + (pType * 64) + relSq] + sideMult

        // Helper for Mobility and King Safety
        let processComplex (color: int) =
            let sideMult = if color = Colour.White then 1s else -1s
            let usTotal = if color = Colour.White then bbs.WhiteTotal else bbs.BlackTotal
            let usPawns = if color = Colour.White then bbs.WhitePawns else bbs.BlackPawns
            let enemyPawns = if color = Colour.White then bbs.BlackPawns else bbs.WhitePawns
            
            let enemyKingZone = if color = Colour.White then 
                                    (let ksq = Bitboard.lsb bbs.BlackKings in if ksq < 64 then BitboardGen.kingAttacks.[ksq] else 0uL)
                                 else 
                                    (let ksq = Bitboard.lsb bbs.WhiteKings in if ksq < 64 then BitboardGen.kingAttacks.[ksq] else 0uL)
            
            let myPieces = if color = Colour.White then 
                            [| bbs.WhitePawns; bbs.WhiteKnights; bbs.WhiteBishops; bbs.WhiteRooks; bbs.WhiteQueens; bbs.WhiteKings |]
                           else 
                            [| bbs.BlackPawns; bbs.BlackKnights; bbs.BlackBishops; bbs.BlackRooks; bbs.BlackQueens; bbs.BlackKings |]

            // --- Pass 1: Count total attackers for the King Safety multiplier ---
            let mutable attackerCount = 0
            for pType in 1 .. 4 do // Knights, Bishops, Rooks, Queens
                let mutable tempBb = myPieces.[pType]
                while tempBb <> 0uL do
                    let sq = Bitboard.popLsb &tempBb
                    let attacks = Evaluation.getAttackBitboard sq pType occ
                    if (attacks &&& enemyKingZone) <> 0uL then
                        attackerCount <- attackerCount + 1

            // --- Pass 2: Process all features ---
            for pType in 0 .. 5 do
                let mutable tempBb = myPieces.[pType]
                while tempBb <> 0uL do
                    let sq = Bitboard.popLsb &tempBb
                    addPiece pType sq color 

                    // 1. Pawn Shield Logic
                    if pType = PieceType.King then
                        if (if color = Colour.White then sq < 16 else sq > 47) then
                            let shieldMask = if color = Colour.White then 
                                                BitboardGen.kingAttacks.[sq] &&& 0xFFFFFF00uL 
                                             else 
                                                BitboardGen.kingAttacks.[sq] &&& 0x00FFFFFFFFFFFF00uL
                            let count = Bitboard.count (shieldMask &&& usPawns)
                            traceMG.[PawnShieldOffset] <- traceMG.[PawnShieldOffset] + (int16 count * sideMult)
                            traceEG.[PawnShieldOffset] <- traceEG.[PawnShieldOffset] + (int16 count * sideMult)

                    // 2. Rook File Logic
                    if pType = PieceType.Rook then
                        let fileMask = BitboardGen.FileMasks.[sq % 8]
                        if (fileMask &&& usPawns) = 0uL then
                            if (fileMask &&& enemyPawns) = 0uL then
                                traceMG.[RookOpenFileOffset] <- traceMG.[RookOpenFileOffset] + sideMult
                                traceEG.[RookOpenFileOffset] <- traceEG.[RookOpenFileOffset] + sideMult
                            else
                                traceMG.[RookHalfOpenFileOffset] <- traceMG.[RookHalfOpenFileOffset] + sideMult
                                traceEG.[RookHalfOpenFileOffset] <- traceEG.[RookHalfOpenFileOffset] + sideMult

                    // 3. Mobility & King Attacks
                    if pType <> PieceType.Pawn && pType <> PieceType.King then
                        let attacks = Evaluation.getAttackBitboard sq pType occ
                        
                        // Mobility
                        let mob = Bitboard.count (attacks &&& ~~~usTotal)
                        traceMG.[MobilityOffset + pType] <- traceMG.[MobilityOffset + pType] + (int16 mob * sideMult)
                        traceEG.[MobilityOffset + pType] <- traceEG.[MobilityOffset + pType] + (int16 mob * sideMult)

                        // King Safety: Multiply by total attackers to match Eval logic (Count * Sum of Weights)
                        if (attacks &&& enemyKingZone) <> 0uL then
                            let traceVal = int16 attackerCount * sideMult
                            traceMG.[KingAtkOffset + pType] <- traceMG.[KingAtkOffset + pType] + traceVal
                            traceEG.[KingAtkOffset + pType] <- traceEG.[KingAtkOffset + pType] + traceVal        
        
        processComplex Colour.White
        processComplex Colour.Black
        
        // 4. Bishop Pair Logic
        if Bitboard.count bbs.WhiteBishops >= 2 then
            traceMG.[BishopPairOffset] <- traceMG.[BishopPairOffset] + 1s
            traceEG.[BishopPairOffset] <- traceEG.[BishopPairOffset] + 1s
        if Bitboard.count bbs.BlackBishops >= 2 then
            traceMG.[BishopPairOffset] <- traceMG.[BishopPairOffset] - 1s
            traceEG.[BishopPairOffset] <- traceEG.[BishopPairOffset] - 1s

        // --- NEW: Doubled Pawn Logic ---
        // Evaluation uses: mg <- mg - (count * Penalty)
        // So Trace = (BlackCount - WhiteCount)
        let whiteDoubled = Bitboard.count (bbs.WhitePawns &&& (bbs.WhitePawns <<< 8))
        let blackDoubled = Bitboard.count (bbs.BlackPawns &&& (bbs.BlackPawns >>> 8))
        let doubledDiff = int16 (blackDoubled - whiteDoubled)
        traceMG.[DoubledPawnOffset] <- doubledDiff
        traceEG.[DoubledPawnOffset] <- doubledDiff

        // --- NEW: Isolated Pawn Logic ---
        // Evaluation uses: mg <- mg - (count * Penalty)
        let whiteIsolated = getIsolatedPawnCount bbs.WhitePawns
        let blackIsolated = getIsolatedPawnCount bbs.BlackPawns
        let isolatedDiff = int16 (blackIsolated - whiteIsolated)
        traceMG.[IsolatedPawnOffset] <- isolatedDiff
        traceEG.[IsolatedPawnOffset] <- isolatedDiff
        
        // White Passed Pawns
        let mutable whitePawns = bbs.WhitePawns
        while whitePawns <> 0uL do
            let sq = Bitboard.popLsb &whitePawns
            // A pawn is a passer if no enemy pawns are in its passed pawn mask
            if (BitboardGen.passedPawnMasks.[0, sq] &&& bbs.BlackPawns) = 0uL then
                let rank = sq / 8
                traceMG.[PassedPawnOffset + rank] <- traceMG.[PassedPawnOffset + rank] + 1s
                traceEG.[PassedPawnOffset + rank] <- traceEG.[PassedPawnOffset + rank] + 1s

        // Black Passed Pawns
        let mutable blackPawns = bbs.BlackPawns
        while blackPawns <> 0uL do
            let sq = Bitboard.popLsb &blackPawns
            if (BitboardGen.passedPawnMasks.[1, sq] &&& bbs.WhitePawns) = 0uL then
                let rank = 7 - (sq / 8) // Relative rank (0-7)
                traceMG.[PassedPawnOffset + rank] <- traceMG.[PassedPawnOffset + rank] - 1s
                traceEG.[PassedPawnOffset + rank] <- traceEG.[PassedPawnOffset + rank] - 1s
        
        // Return phase and combined traces
        let phase = 
            (Bitboard.count (bbs.WhiteKnights ||| bbs.BlackKnights) * 1) +
            (Bitboard.count (bbs.WhiteBishops ||| bbs.BlackBishops) * 1) +
            (Bitboard.count (bbs.WhiteRooks ||| bbs.BlackRooks) * 2) +
            (Bitboard.count (bbs.WhiteQueens ||| bbs.BlackQueens) * 4)
        phase, traceMG, traceEG

    let createTuningEntry fen result =
        let b = Board.fromFen(fen)
        let phase, traceMG, _ = getTrace b // We only need one trace because indices are identical
    
        // Cap phase
        let p = if phase > Evaluation.MaxPhase then Evaluation.MaxPhase else phase

        // Convert dense array to sparse features to save RAM
        let features = 
            traceMG 
            |> Array.mapi (fun i v -> { Index = i; Value = v }) 
            |> Array.filter (fun f -> f.Value <> 0s)

        { Result = result; Phase = p; Features = features }
    
