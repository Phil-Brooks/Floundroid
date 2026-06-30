namespace Benchmarks

open BenchmarkDotNet.Attributes
open Floundroid

[<MemoryDiagnoser>]
type EvalBench() =

    let mutable board = Unchecked.defaultof<Board>

    [<GlobalSetup>]
    member _.Setup() =
        board <- Board.fromFen("r1bq1rk1/ppp2ppp/2n1pn2/3p4/3P4/2N1PN2/PPP2PPP/R1BQ1RK1 w - - 0 1")

    [<Benchmark>]
    member _.Evaluate() =
       Evaluation.evaluate board
