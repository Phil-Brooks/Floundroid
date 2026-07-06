namespace Texel

open Floundroid

module TuningData =
    // --- OFFSETS ---
    let MaterialOffset = 0
    let PstOffset      = 6
    let MobilityOffset = PstOffset + (6 * 64)   // Starts at 390
    let KingAtkOffset  = MobilityOffset + 6     // Starts at 396
    let BishopPairOffset       = KingAtkOffset + 6
    let RookOpenFileOffset     = BishopPairOffset + 1
    let RookHalfOpenFileOffset = RookOpenFileOffset + 1
    let PawnShieldOffset       = RookHalfOpenFileOffset + 1
    let DoubledPawnOffset      = PawnShieldOffset + 1
    let IsolatedPawnOffset     = DoubledPawnOffset + 1
    let PassedPawnOffset       = IsolatedPawnOffset + 1 // New
    let TotalParams            = PassedPawnOffset + 8   // 8 ranks

    let getInitialWeights () =
        let weightsMG = Array.zeroCreate<int> TotalParams
        let weightsEG = Array.zeroCreate<int> TotalParams

        // 1. Material
        for i in 0 .. 5 do
            weightsMG.[MaterialOffset + i] <- Pst.matsMG.[i]
            weightsEG.[MaterialOffset + i] <- Pst.matsEG.[i]

        // 2. PSTs
        for p in 0 .. 5 do
            for sq in 0 .. 63 do
                let idx = PstOffset + (p * 64) + sq
                weightsMG.[idx] <- Pst.MG.[p].[sq]
                weightsEG.[idx] <- Pst.EG.[p].[sq]

        // 3. Mobility (Initialize MG and EG with same start values)
        for i in 0 .. 5 do
            let idx = MobilityOffset + i
            // Your array had 7 elements, we take the first 6 (Pawn to King)
            weightsMG.[idx] <- Evaluation.mobWeightsMG.[i]
            weightsEG.[idx] <- Evaluation.mobWeightsEG.[i]

        // 4. King Attacks
        for i in 0 .. 5 do
            let idx = KingAtkOffset + i
            weightsMG.[idx] <- Evaluation.kingAttackWeightsMG.[i]
            weightsEG.[idx] <- Evaluation.kingAttackWeightsEG.[i] // Usually tuned mostly in MG

        // New Feature Initialization
        weightsMG.[BishopPairOffset] <- Evaluation.BishopPairBonusMG
        weightsEG.[BishopPairOffset] <- Evaluation.BishopPairBonusEG

        weightsMG.[RookOpenFileOffset] <- Evaluation.RookOpenFileMG
        weightsEG.[RookOpenFileOffset] <- Evaluation.RookOpenFileEG

        weightsMG.[RookHalfOpenFileOffset] <- Evaluation.RookHalfOpenFileMG
        weightsEG.[RookHalfOpenFileOffset] <- Evaluation.RookHalfOpenFileEG

        weightsMG.[PawnShieldOffset] <- Evaluation.PawnShieldBonusMG
        weightsEG.[PawnShieldOffset] <- Evaluation.PawnShieldBonusEG

        weightsMG.[DoubledPawnOffset] <- Evaluation.DoubledPawnPenaltyMG
        weightsEG.[DoubledPawnOffset] <- Evaluation.DoubledPawnPenaltyEG

        weightsMG.[IsolatedPawnOffset] <- Evaluation.IsolatedPawnPenaltyMG
        weightsEG.[IsolatedPawnOffset] <- Evaluation.IsolatedPawnPenaltyEG

        // New: Passed Pawns
        for rank in 0 .. 7 do
            weightsMG.[PassedPawnOffset + rank] <- BitboardGen.ppbonusesMG.[rank]
            weightsEG.[PassedPawnOffset + rank] <- BitboardGen.ppbonusesEG.[rank]
        weightsMG, weightsEG

    let printTunedWeights (mg: int[], eg: int[]) =
        let openBrk = "[|"
        let closeBrk = "|]"

        printfn "\n// --- COPY PASTE THESE INTO YOUR ENGINE ---"
    
        // Print Material
        printfn "let matsMG = %s %s %s" openBrk (String.concat "; " (mg.[0..5] |> Array.map string)) closeBrk
        printfn "let matsEG = %s %s %s" openBrk (String.concat "; " (eg.[0..5] |> Array.map string)) closeBrk

        // Print PSTs
        let pieceNames = [|"Pawn"; "Knight"; "Bishop"; "Rook"; "Queen"; "King"|]
    
        printfn "\nlet MG = [|"
        for p in 0 .. 5 do
            printfn "    // %s" pieceNames.[p]
            printf "    %s " openBrk
            for rank in 0 .. 7 do
                let start = PstOffset + (p * 64) + (rank * 8)
                let row = mg.[start .. start + 7] |> Array.map (fun v -> sprintf "%3d" v)
                printf "%s" (String.concat "; " row)
                if rank < 7 then printf "; "
            printfn " %s" closeBrk
        printfn "|]"

        printfn "\nlet EG = [|"
        for p in 0 .. 5 do
            printfn "    // %s" pieceNames.[p]
            printf "    %s " openBrk
            for rank in 0 .. 7 do
                let start = PstOffset + (p * 64) + (rank * 8)
                let row = eg.[start .. start + 7] |> Array.map (fun v -> sprintf "%3d" v)
                printf "%s" (String.concat "; " row)
                if rank < 7 then printf "; "
            printfn " %s" closeBrk
        printfn "|]"

        // Print Mobility & King Safety
        printfn "\nlet mobWeightsMG = %s %s %s" openBrk (String.concat "; " (mg.[MobilityOffset .. MobilityOffset + 5] |> Array.map string)) closeBrk
        printfn "let mobWeightsEG = %s %s %s" openBrk (String.concat "; " (eg.[MobilityOffset .. MobilityOffset + 5] |> Array.map string)) closeBrk
        printfn "let kingAttackWeightsMG = %s %s %s" openBrk (String.concat "; " (mg.[KingAtkOffset .. KingAtkOffset + 5] |> Array.map string)) closeBrk
        printfn "let kingAttackWeightsEG = %s %s %s" openBrk (String.concat "; " (eg.[KingAtkOffset .. KingAtkOffset + 5] |> Array.map string)) closeBrk
        printfn "let mutable BishopPairBonusMG = %d" mg.[BishopPairOffset]
        printfn "let mutable BishopPairBonusEG = %d" eg.[BishopPairOffset]
        printfn "let mutable RookOpenFileMG = %d" mg.[RookOpenFileOffset]
        printfn "let mutable RookOpenFileEG = %d" eg.[RookOpenFileOffset]
        printfn "let mutable RookHalfOpenFileMG = %d" mg.[RookHalfOpenFileOffset]
        printfn "let mutable RookHalfOpenFileEG = %d" eg.[RookHalfOpenFileOffset]
        printfn "let mutable PawnShieldBonusMG = %d" mg.[PawnShieldOffset]
        printfn "let mutable PawnShieldBonusEG = %d" eg.[PawnShieldOffset]
        printfn "let mutable DoubledPawnPenaltyMG = %d" mg.[DoubledPawnOffset]
        printfn "let mutable DoubledPawnPenaltyEG = %d" eg.[DoubledPawnOffset]
        printfn "let mutable IsolatedPawnPenaltyMG = %d" mg.[IsolatedPawnOffset]
        printfn "let mutable IsolatedPawnPenaltyEG = %d" eg.[IsolatedPawnOffset]
        printfn "\nlet ppbonusesMG = %s %s %s" "[|" (String.concat "; " (mg.[PassedPawnOffset .. PassedPawnOffset + 7] |> Array.map string)) "|]"
        printfn "let ppbonusesEG = %s %s %s" "[|" (String.concat "; " (eg.[PassedPawnOffset .. PassedPawnOffset + 7] |> Array.map string)) "|]"