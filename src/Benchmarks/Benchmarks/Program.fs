open BenchmarkDotNet.Running
open Benchmarks

[<EntryPoint>]
let main _ =
    
    BenchmarkRunner.Run<MoveGenBench>() |> ignore
    0
