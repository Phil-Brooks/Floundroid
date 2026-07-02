namespace Benchmarks

open BenchmarkDotNet.Attributes
open Floundroid

[<MemoryDiagnoser>]
type MoveGenBench() =

    let mutable board = Unchecked.defaultof<Board>

    [<GlobalSetup>]
    member _.Setup() =
        board <- Board.fromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")

    [<Benchmark>]
    member _.PseudoLegal() =
        let moveBuffer = ResizeArray<int>()
        let moves = MoveGen.getPseudoLegalMoves board moveBuffer
        moves.Length

    //[<Benchmark>]
    //member _.Legal() =
    //    let moves = MoveGen.getLegalMoves board
    //    moves.Length