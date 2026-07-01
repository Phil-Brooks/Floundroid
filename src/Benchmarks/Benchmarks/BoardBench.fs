namespace Benchmarks

open BenchmarkDotNet.Attributes
open Floundroid

[<MemoryDiagnoser>]
type BoardBench() =

    let mutable board = Unchecked.defaultof<Board>

    [<GlobalSetup>]
    member _.Setup() =
        board <- Board.fromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
    

    [<Benchmark>]
    member _.Evaluate() =
        let mutable count = 0
        for sq in 0..63 do
            if Board.tryGetPiece board sq <> -1 then count <- count + 1
        count