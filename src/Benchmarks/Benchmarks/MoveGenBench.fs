namespace Benchmarks

open BenchmarkDotNet.Attributes
open Floundroid
open System
open System.Runtime.CompilerServices
open Microsoft.FSharp.NativeInterop

#nowarn "9" // For NativePtr usage

[<MemoryDiagnoser>]
type MoveGenBench() =

    let mutable board = Unchecked.defaultof<Board>
    //let mutable moveBuffer = Unchecked.defaultof<ResizeArray<int>>
    let mutable capsBuffer = Unchecked.defaultof<ResizeArray<int>>

    [<GlobalSetup>]
    member _.Setup() =
        //board <- Board.fromFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
        board <- Board.fromFen("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1")
        //moveBuffer <- ResizeArray<int>()
        capsBuffer <- ResizeArray<int>()

    //[<Benchmark>]
    //member _.PseudoLegal() =
    //    let moves = MoveGen.getPseudoLegalMoves board moveBuffer
    //    moves.Length

    //[<Benchmark>]
    //member _.Legal() =
    //    let moves = MoveGen.getLegalMoves board
    //    moves.Length

    [<Benchmark>]
    member _.Captures() =
        let movePtr = NativePtr.stackalloc<int> 256
        let moveSpan = Span<int>(NativePtr.toVoidPtr movePtr, 256)

        // 2. Generate moves directly into the stack span
        let moveCount = MoveGen.getCaptureMoves board moveSpan
        moveCount
        
        
        
