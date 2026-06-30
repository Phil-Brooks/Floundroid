namespace Benchmarks

open BenchmarkDotNet.Attributes
open Floundroid

[<MemoryDiagnoser>]
type BitboardBench() =

    let bb = 0x00FF00FF00FF00FFUL

    [<Benchmark>]
    member _.PopCount() =
        Bitboard.count bb

    [<Benchmark>]
    member _.Lsb() =
        Bitboard.lsb bb
