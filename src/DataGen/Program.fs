open Floundroid
open System
open System.IO
open System.Threading
open System.Threading.Tasks
open System.Runtime.InteropServices

module Generator =

    // Use the 32-byte struct Bullet expects
    [<Struct; StructLayout(LayoutKind.Sequential, Pack = 1)>]
    type MarlinRecord =
        [<DefaultValue>] val mutable Occupancy: uint64
        [<DefaultValue>] val mutable WhitePieces: uint64
        [<DefaultValue>] val mutable Score: int16
        [<DefaultValue>] val mutable Result: byte      // 0=B Win, 1=Draw, 2=W Win
        [<DefaultValue>] val mutable SideToMove: byte  // 0=W, 1=B
        [<DefaultValue>] val mutable WhiteKingSq: byte
        [<DefaultValue>] val mutable BlackKingSq: byte
        [<DefaultValue>] val mutable Padding: uint64
        [<DefaultValue>] val mutable FullMove: uint16

    // Helper to create a record from a Board state
    let createRecord (b: Board) (eval: int) (finalRes: float) =
        let mutable r = MarlinRecord()
        r.Occupancy <- b.Bitboards.Occupancy
        r.WhitePieces <- b.Bitboards.WhiteTotal
        // Convert to White POV for the trainer
        let whiteEval = if b.SideToMove = Colour.White then eval else -eval
        r.Score <- int16 (Math.Clamp(whiteEval, -32000, 32000))
        r.SideToMove <- if b.SideToMove = Colour.White then 0uy else 1uy
        r.Result <- if finalRes = 1.0 then 2uy elif finalRes = 0.0 then 0uy else 1uy
        r.WhiteKingSq <- byte (Board.findKing Colour.White b)
        r.BlackKingSq <- byte (Board.findKing Colour.Black b)
        r.FullMove <- uint16 b.FullmoveNumber
        r

    let playGame () : MarlinRecord[] =
        let rnd = Random()
        let mutable board = Board.start
        let history = ResizeArray<Board * int>()

        // 1. Random opening 8-10 plies
        for _ in 1 .. (8 + rnd.Next(3)) do
            let moves = MoveGen.getLegalMoves board
            if moves.Length > 0 then
                board <- Board.applyMove moves.[rnd.Next(moves.Length)] board

        // 2. Play until end
        let mutable ply = 0
        while not (MoveGen.getLegalMoves board |> Array.isEmpty) && ply < 200 do
            let eval = Evaluation.evaluate board
            if not (Board.isInCheck board) && Math.Abs(eval) < 2000 then
                history.Add (board, eval)
            
            let moves = MoveGen.getLegalMoves board
            board <- Board.applyMove moves.[rnd.Next(moves.Length)] board
            ply <- ply + 1

        // 3. Determine final result
        let res =
            let moves = MoveGen.getLegalMoves board
            if moves.Length = 0 && Board.isInCheck board then
                if board.SideToMove = Colour.White then 0.0 else 1.0
            else 0.5

        // 4. Convert history to Bullet-ready records
        history 
        |> Seq.map (fun (b, e) -> createRecord b e res)
        |> Seq.toArray

    let run (numThreads: int) (totalPositions: int64) (outPath: string) =
        let mutable count = 0L
        
        Parallel.For(0, numThreads, fun i ->
            let partPath = outPath + sprintf ".part%d" i
            use fs = new FileStream(partPath, FileMode.Create)
            
            while Interlocked.Read(&count) < totalPositions do
                let records = playGame()
                if records.Length > 0 then
                    // Write records as raw bytes (No string formatting!)
                    let span = ReadOnlySpan<MarlinRecord>(records)
                    let bytes = MemoryMarshal.Cast<MarlinRecord, byte>(span)
                    fs.Write(bytes)
                    Interlocked.Add(&count, int64 records.Length) |> ignore
        ) |> ignore

        // Merge parts into one .bin file
        use finalOut = new FileStream(outPath, FileMode.Create)
        for i in 0 .. numThreads - 1 do
            let part = outPath + sprintf ".part%d" i
            if File.Exists(part) then
                use src = File.OpenRead(part)
                src.CopyTo(finalOut)
                src.Close()
                File.Delete(part)

[<EntryPoint>]
let main argv =
    let mutable threads = Environment.ProcessorCount
    let mutable positions = 100000L
    let mutable outPath = "data.bin" // Binary extension

    // (Keep your existing arg parsing here)

    printfn "DataGen (Binary): threads=%d positions=%d out=%s" threads positions outPath
    Generator.run threads positions outPath
    printfn "Done."
    0