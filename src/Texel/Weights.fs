namespace Texel

open Floundroid

module TuningData =
    // --- OFFSETS ---
    let MaterialOffset = 0
    let PstOffset      = 6
    let MobilityOffset = PstOffset + (6 * 64)   // Starts at 390
    let KingAtkOffset  = MobilityOffset + 6     // Starts at 396
    let TotalParams    = KingAtkOffset + 6      // Total 402

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
            weightsMG.[idx] <- Evaluation.mobWeights.[i]
            weightsEG.[idx] <- Evaluation.mobWeights.[i]

        // 4. King Attacks
        for i in 0 .. 5 do
            let idx = KingAtkOffset + i
            weightsMG.[idx] <- Evaluation.kingAttackWeights.[i]
            weightsEG.[idx] <- Evaluation.kingAttackWeights.[i] // Usually tuned mostly in MG

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
        printfn "\nlet mobWeights = %s %s %s" openBrk (String.concat "; " (mg.[MobilityOffset .. MobilityOffset + 5] |> Array.map string)) closeBrk
        printfn "let kingAttackWeights = %s %s %s" openBrk (String.concat "; " (mg.[KingAtkOffset .. KingAtkOffset + 5] |> Array.map string)) closeBrk

