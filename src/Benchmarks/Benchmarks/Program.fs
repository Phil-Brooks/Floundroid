open BenchmarkDotNet.Running
open Benchmarks

[<EntryPoint>]
let main _ =
    
    //BenchmarkRunner.Run<BoardBench>() |> ignore
    //BenchmarkRunner.Run<MoveGenBench>() |> ignore
    //BenchmarkRunner.Run<EvalBench>() |> ignore
    BenchmarkRunner.Run<SearchBench>() |> ignore
    0
