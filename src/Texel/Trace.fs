namespace Texel

open Floundroid
open TuningData

module Trace =
    
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
            let enemyKingZone = if color = Colour.White then 
                                    (let ksq = Bitboard.lsb bbs.BlackKings in if ksq < 64 then BitboardGen.kingAttacks.[ksq] else 0uL)
                                 else 
                                    (let ksq = Bitboard.lsb bbs.WhiteKings in if ksq < 64 then BitboardGen.kingAttacks.[ksq] else 0uL)
            
            let myPieces = if color = Colour.White then 
                            [| bbs.WhitePawns; bbs.WhiteKnights; bbs.WhiteBishops; bbs.WhiteRooks; bbs.WhiteQueens; bbs.WhiteKings |]
                           else 
                            [| bbs.BlackPawns; bbs.BlackKnights; bbs.BlackBishops; bbs.BlackRooks; bbs.BlackQueens; bbs.BlackKings |]

            for pType in 0 .. 5 do
                let mutable tempBb = myPieces.[pType]
                while tempBb <> 0uL do
                    let sq = Bitboard.popLsb &tempBb
                    addPiece pType sq color // Handle Material/PST

                    // Mobility & King Safety (skip pawns/kings usually)
                    if pType <> PieceType.Pawn && pType <> PieceType.King then
                        let attacks = Evaluation.getAttackBitboard sq pType occ
                        let mob = Bitboard.count (attacks &&& ~~~usTotal)
                        traceMG.[MobilityOffset + pType] <- traceMG.[MobilityOffset + pType] + (int16 mob * sideMult)
                        traceEG.[MobilityOffset + pType] <- traceEG.[MobilityOffset + pType] + (int16 mob * sideMult)

                        if (attacks &&& enemyKingZone) <> 0uL then
                            traceMG.[KingAtkOffset + pType] <- traceMG.[KingAtkOffset + pType] + sideMult

        processComplex Colour.White
        processComplex Colour.Black
        
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
    
