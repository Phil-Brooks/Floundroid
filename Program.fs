open System
open Chess.Core // Replace with whatever namespace you used

[<EntryPoint>]
let main argv =
    printfn "--- F# Chess Engine Stage 1 ---"

    // 1. Setup the starting board
    // (For now, let's just use the empty board or manually place pieces)
    let startBoard = Board.empty 
    
    // 2. Display the turn
    printfn "Current Turn: %A" startBoard.Turn

    // 3. Run a Perft Test
    let depth = 1
    printfn "Running Perft at depth %d..." depth
    
    let timer = Diagnostics.Stopwatch.StartNew()
    let moveCount = Perft.countMoves depth startBoard
    timer.Stop()

    printfn "Moves found: %d" moveCount
    printfn "Time taken: %A" timer.Elapsed

    // Prevent the console from closing immediately
    printfn "\nPress any key to exit..."
    Console.ReadKey() |> ignore
    
    0 // Return code 0 (success)