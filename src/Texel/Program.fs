open System
open System.IO
open Floundroid
open Texel
open Texel.TuningData

// Sigmoid Function
let sigmoid score K = 1.0 / (1.0 + Math.Pow(10.0, -K * (float score) / 400.0))

// Calculate Mean Squared Error (Thread-safe sum)
let calculateError (entries: TuningEntry[]) (weightsMG:int array) (weightsEG:int array) (K: float) =
    let totalError = 
        Array.Parallel.map (fun entry ->
            let mutable mg = 0
            let mutable eg = 0
            for f in entry.Features do
                mg <- mg + (int f.Value * weightsMG.[f.Index])
                eg <- eg + (int f.Value * weightsEG.[f.Index])
            
            let score = (mg * entry.Phase + eg * (Evaluation.MaxPhase - entry.Phase)) / Evaluation.MaxPhase
            let prediction = sigmoid score K
            Math.Pow(entry.Result - prediction, 2.0)
        ) entries
        |> Array.sum
    totalError / float entries.Length

let loadEpd file =
    File.ReadLines(file)
    |> Seq.filter (fun line -> not (String.IsNullOrWhiteSpace(line)))
    |> Seq.map (fun line ->
        try
            // 1. Find the result inside the quotes
            let firstQuote = line.IndexOf('"')
            let lastQuote = line.LastIndexOf('"')
            
            if firstQuote = -1 || lastQuote = -1 then 
                failwith "Could not find result in quotes"

            let resultStr = line.Substring(firstQuote + 1, lastQuote - firstQuote - 1)
            let result = 
                match resultStr with
                | "1-0"     -> 1.0
                | "0-1"     -> 0.0
                | "1/2-1/2" -> 0.5
                | _         -> 0.5

            // 2. Extract the FEN
            // In EPD, the FEN is always the first 4-6 parts. 
            // We can take everything before the first opcode (like 'c9')
            let tokens = line.Split([| ' ' |], StringSplitOptions.RemoveEmptyEntries)
            
            // Reconstruct the FEN (Piece placement, Side, Castling, En Passant)
            // This skips the 'c9' and the quoted result.
            let fen = String.Join(" ", tokens.[0..3])

            Trace.createTuningEntry fen result
        with
        | ex -> 
            printfn "Error parsing line: %s\nException: %s" line ex.Message
            // Return a dummy entry or skip (we use null/None pattern here usually)
            Trace.createTuningEntry "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1" 0.5
    )
    |> Seq.toArray

let tuneK entries weightsMG weightsEG =
    printfn "Tuning K factor..."
    let mutable bestK = 0.75
    let mutable minError = 1.0

    // Test K values from 0.1 to 3.0
    for kInt in 10 .. 300 do
        let testK = float kInt / 100.0
        let err = calculateError entries weightsMG weightsEG testK
        if err < minError then
            minError <- err
            bestK <- testK

    printfn "Optimal K found: %f (Initial Error: %f)" bestK minError
    bestK

[<EntryPoint>]
let main argv =
    if argv.Length = 0 then
        printfn "Usage: Texel.exe <path_to_epd>"
        exit 1

    printfn "Texel Tuner starting..."

    // 1. Load Initial Weights
    let mutable (weightsMG, weightsEG) = getInitialWeights()
    let mutable K = 0.75 

    // 2. Data Loading
    printfn "Loading dataset: %s" argv.[0]
    let entries = loadEpd argv.[0]
    printfn "Loaded %d positions." entries.Length
    // Find optimal K first!
    let K = tuneK entries weightsMG weightsEG

    // 3. Optimization Loop
    let mutable bestError = calculateError entries weightsMG weightsEG K
    printfn "Initial Error: %f" bestError

    let mutable epoch = 1
    let mutable keepTuning = true
    let epsilon = 0.0000001 // Adjust this: smaller = more precise, larger = faster stop

    while epoch <= 1000 && keepTuning do
        let mutable improved = false
        
        // --- Tune MG Weights ---
        for i in 0 .. TotalParams - 1 do
            let original = weightsMG.[i]
            
            weightsMG.[i] <- original + 1
            let errPlus = calculateError entries weightsMG weightsEG K
            
            if errPlus < (bestError - epsilon) then
                bestError <- errPlus
                improved <- true
            else
                weightsMG.[i] <- original - 1
                let errMinus = calculateError entries weightsMG weightsEG K
                if errMinus < (bestError - epsilon) then
                    bestError <- errMinus
                    improved <- true
                else
                    weightsMG.[i] <- original

        // --- Tune EG Weights ---
        for i in 0 .. TotalParams - 1 do
            let original = weightsEG.[i]
            
            weightsEG.[i] <- original + 1
            let errPlus = calculateError entries weightsMG weightsEG K
            
            if errPlus < (bestError - epsilon) then
                bestError <- errPlus
                improved <- true
            else
                weightsEG.[i] <- original - 1
                let errMinus = calculateError entries weightsMG weightsEG K
                if errMinus < (bestError - epsilon) then
                    bestError <- errMinus
                    improved <- true
                else
                    weightsEG.[i] <- original

        printfn "Epoch %d - Best Error: %f" epoch bestError
        
        if not improved then 
            printfn "Convergence reached or local minima found."
            printTunedWeights(weightsMG, weightsEG)
            // You might want to break
            keepTuning <- false
        else 
            epoch <- epoch + 1
        
        if epoch % 100 = 0 then
            printTunedWeights(weightsMG, weightsEG)

    0