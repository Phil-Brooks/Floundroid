namespace Benchmarks

open BenchmarkDotNet.Attributes
open Floundroid

[<MemoryDiagnoser>]
type SearchBench() =

    let mutable board = Unchecked.defaultof<Board>
    //let mutable moveBuffer = Unchecked.defaultof<ResizeArray<int>>
    let mutable capsBuffer = Unchecked.defaultof<ResizeArray<int>>
    let mutable scoreBuffer = Unchecked.defaultof<ResizeArray<int>>

    [<GlobalSetup>]
    member _.Setup() =
        //board <- Board.fromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
        board <- Board.fromFen("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1")
        //moveBuffer <- ResizeArray<int>()
        capsBuffer <- ResizeArray<int>()
        scoreBuffer <- ResizeArray<int>()

    [<Benchmark>]
    member _.Captures() =
        let ans = Search.quiesce board 4 -32000 32000 System.Threading.CancellationToken.None
        ans
