# -------------------------------
# Floundroid Project Bootstrapper
# -------------------------------

Write-Host "Creating folder structure..."

$dirs = @(
  "src/Floundroid.Core/Types",
  "src/Floundroid.Core/MoveGen",
  "src/Floundroid.Core/Fen",
  "src/Floundroid.Core/Perft",
  "src/Floundroid.Core/Utils",
  "src/Floundroid.UCI",
  "src/Floundroid.Search",
  "src/Floundroid.Bitboard",
  "src/Floundroid.App",
  "tests/Floundroid.Tests"
)

foreach ($d in $dirs) {
  New-Item -ItemType Directory -Path $d -Force | Out-Null
}

Write-Host "Creating project files..."

# -------------------------------
# Floundroid.Core.fsproj
# -------------------------------
@"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="Types/Piece.fs" />
    <Compile Include="Types/Square.fs" />
    <Compile Include="Types/Move.fs" />
    <Compile Include="Types/Board.fs" />
    <Compile Include="MoveGen/PawnMoves.fs" />
    <Compile Include="MoveGen/KnightMoves.fs" />
    <Compile Include="MoveGen/BishopMoves.fs" />
    <Compile Include="MoveGen/RookMoves.fs" />
    <Compile Include="MoveGen/QueenMoves.fs" />
    <Compile Include="MoveGen/KingMoves.fs" />
    <Compile Include="MoveGen/LegalMoves.fs" />
    <Compile Include="Fen/FenParser.fs" />
    <Compile Include="Fen/FenWriter.fs" />
    <Compile Include="Perft/Perft.fs" />
    <Compile Include="Utils/BoardPrinter.fs" />
    <Compile Include="Utils/AttackMaps.fs" />
  </ItemGroup>
</Project>
"@ | Set-Content "src/Floundroid.Core/Floundroid.Core.fsproj"

# -------------------------------
# Floundroid.UCI.fsproj
# -------------------------------
@"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\Floundroid.Core\Floundroid.Core.fsproj" />
    <ProjectReference Include="..\Floundroid.Search\Floundroid.Search.fsproj" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include="CommandParser.fs" />
    <Compile Include="UciProtocol.fs" />
    <Compile Include="UciLoop.fs" />
  </ItemGroup>
</Project>
"@ | Set-Content "src/Floundroid.UCI/Floundroid.UCI.fsproj"

# -------------------------------
# Floundroid.Search.fsproj
# -------------------------------
@"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\Floundroid.Core\Floundroid.Core.fsproj" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include="Evaluation.fs" />
    <Compile Include="MoveOrdering.fs" />
    <Compile Include="AlphaBeta.fs" />
    <Compile Include="TimeManager.fs" />
    <Compile Include="Search.fs" />
  </ItemGroup>
</Project>
"@ | Set-Content "src/Floundroid.Search/Floundroid.Search.fsproj"

# -------------------------------
# Floundroid.Bitboard.fsproj
# -------------------------------
@"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\Floundroid.Core\Floundroid.Core.fsproj" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include="Bitboards.fs" />
    <Compile Include="Magic.fs" />
    <Compile Include="Zobrist.fs" />
    <Compile Include="TranspositionTable.fs" />
    <Compile Include="BitMoveGen.fs" />
  </ItemGroup>
</Project>
"@ | Set-Content "src/Floundroid.Bitboard/Floundroid.Bitboard.fsproj"

# -------------------------------
# Floundroid.App.fsproj
# -------------------------------
@"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\Floundroid.Core\Floundroid.Core.fsproj" />
    <ProjectReference Include="..\Floundroid.UCI\Floundroid.UCI.fsproj" />
    <ProjectReference Include="..\Floundroid.Search\Floundroid.Search.fsproj" />
    <ProjectReference Include="..\Floundroid.Bitboard\Floundroid.Bitboard.fsproj" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include="Program.fs" />
  </ItemGroup>
</Project>
"@ | Set-Content "src/Floundroid.App/Floundroid.App.fsproj"

# -------------------------------
# Floundroid.Tests.fsproj
# -------------------------------
@"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\Floundroid.Core\Floundroid.Core.fsproj" />
    <ProjectReference Include="..\..\src\Floundroid.Search\Floundroid.Search.fsproj" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="Expecto" Version="9.0.4" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include="PerftTests.fs" />
    <Compile Include="FenTests.fs" />
    <Compile Include="MoveGenTests.fs" />
    <Compile Include="SearchTests.fs" />
  </ItemGroup>
</Project>
"@ | Set-Content "tests/Floundroid.Tests/Floundroid.Tests.fsproj"

Write-Host "Creating source files..."

# -------------------------------
# Helper function to write files
# -------------------------------
function Write-File($path, $content) {
  $content | Set-Content $path
}

# -------------------------------
# Core Types
# -------------------------------
Write-File "src/Floundroid.Core/Types/Piece.fs" @"
namespace Floundroid.Core.Types

type Color = White | Black

type PieceType =
    | Pawn | Knight | Bishop | Rook | Queen | King

type Piece =
    { Color: Color
      Kind: PieceType }
"@

Write-File "src/Floundroid.Core/Types/Square.fs" @"
namespace Floundroid.Core.Types

type Square = int

module Square =
    let file sq = sq % 8
    let rank sq = sq / 8
"@

Write-File "src/Floundroid.Core/Types/Move.fs" @"
namespace Floundroid.Core.Types

open Floundroid.Core.Types

type Move =
    | Quiet of from:Square * dest:Square
    | Capture of from:Square * dest:Square
    | Promotion of from:Square * dest:Square * promoteTo:PieceType
    | CastleKingSide
    | CastleQueenSide
    | EnPassant of from:Square * dest:Square
"@

Write-File "src/Floundroid.Core/Types/Board.fs" @"
namespace Floundroid.Core.Types

open Floundroid.Core.Types

type Board =
    { Pieces: Map<Square, Piece>
      SideToMove: Color
      CastlingRights: string
      EnPassantSquare: Square option
      HalfmoveClock: int
      FullmoveNumber: int }
"@

# -------------------------------
# MoveGen stubs
# -------------------------------
$moveGenModules = @(
  "PawnMoves", "KnightMoves", "BishopMoves",
  "RookMoves", "QueenMoves", "KingMoves"
)

foreach ($m in $moveGenModules) {
  Write-File "src/Floundroid.Core/MoveGen/$m.fs" @"
namespace Floundroid.Core.MoveGen

open Floundroid.Core.Types

module $m =
    let generate (board: Board) : Move list = []
"@
}

Write-File "src/Floundroid.Core/MoveGen/LegalMoves.fs" @"
namespace Floundroid.Core.MoveGen

open Floundroid.Core.Types

module LegalMoves =
    let generateLegal (board: Board) : Move list =
        [] // TODO
"@

# -------------------------------
# FEN
# -------------------------------
Write-File "src/Floundroid.Core/Fen/FenParser.fs" @"
namespace Floundroid.Core.Fen

open Floundroid.Core.Types

module FenParser =
    let parse (fen: string) : Board =
        failwith "FEN parsing not implemented"
"@

Write-File "src/Floundroid.Core/Fen/FenWriter.fs" @"
namespace Floundroid.Core.Fen

open Floundroid.Core.Types

module FenWriter =
    let toFen (board: Board) : string = "TODO"
"@

# -------------------------------
# Perft
# -------------------------------
Write-File "src/Floundroid.Core/Perft/Perft.fs" @"
namespace Floundroid.Core.Perft

open Floundroid.Core.Types
open Floundroid.Core.MoveGen

module Perft =
    let rec perft depth (board: Board) : int64 =
        if depth = 0 then 1L
        else
            LegalMoves.generateLegal board
            |> List.sumBy (fun _ -> 0L)
"@

# -------------------------------
# Utils
# -------------------------------
Write-File "src/Floundroid.Core/Utils/BoardPrinter.fs" @"
namespace Floundroid.Core.Utils

open Floundroid.Core.Types

module BoardPrinter =
    let print (board: Board) =
        printfn "Board printing not implemented"
"@

Write-File "src/Floundroid.Core/Utils/AttackMaps.fs" @"
namespace Floundroid.Core.Utils

open Floundroid.Core.Types

module AttackMaps =
    let attacksFor (board: Board) (color: Color) : Square list = []
"@

# -------------------------------
# UCI
# -------------------------------
Write-File "src/Floundroid.UCI/CommandParser.fs" @"
namespace Floundroid.UCI

module CommandParser =
    let tokenize (line: string) =
        line.Split(' ', System.StringSplitOptions.RemoveEmptyEntries)
        |> Array.toList
"@

Write-File "src/Floundroid.UCI/UciProtocol.fs" @"
namespace Floundroid.UCI

module UciProtocol =
    let printId () =
        printfn "id name Floundroid"
        printfn "id author Phil Brooks"

    let printUciOk () =
        printfn "uciok"
"@

Write-File "src/Floundroid.UCI/UciLoop.fs" @"
namespace Floundroid.UCI

open System
open CommandParser
open UciProtocol

module UciLoop =
    let rec run () =
        let line = Console.ReadLine()
        match line with
        | null -> ()
        | cmd ->
            match tokenize cmd with
            | "uci"::_ ->
                printId ()
                printUciOk ()
            | "isready"::_ ->
                printfn "readyok"
            | "quit"::_ -> ()
            | _ -> ()
            run ()
"@

# -------------------------------
# Search
# -------------------------------
Write-File "src/Floundroid.Search/Evaluation.fs" @"
namespace Floundroid.Search

open Floundroid.Core.Types

module Evaluation =
    let evaluate (board: Board) : int = 0
"@

Write-File "src/Floundroid.Search/MoveOrdering.fs" @"
namespace Floundroid.Search

open Floundroid.Core.Types

module MoveOrdering =
    let order (moves: Move list) = moves
"@

Write-File "src/Floundroid.Search/AlphaBeta.fs" @"
namespace Floundroid.Search

open Floundroid.Core.Types
open Evaluation
open MoveOrdering
open Floundroid.Core.MoveGen

module AlphaBeta =
    let rec search depth alpha beta (board: Board) =
        if depth = 0 then evaluate board
        else evaluate board
"@

Write-File "src/Floundroid.Search/TimeManager.fs" @"
namespace Floundroid.Search

module TimeManager =
    let shouldStop () = false
"@

Write-File "src/Floundroid.Search/Search.fs" @"
namespace Floundroid.Search

open Floundroid.Core.Types
open AlphaBeta

module Search =
    let searchBestMove depth (board: Board) = None
"@

# -------------------------------
# Bitboard
# -------------------------------
Write-File "src/Floundroid.Bitboard/Bitboards.fs" @"
namespace Floundroid.Bitboard

type Bitboard = uint64

module Bitboards =
    let empty = 0UL
"@

Write-File "src/Floundroid.Bitboard/Magic.fs" @"
namespace Floundroid.Bitboard

module Magic =
    let init () = ()
"@

Write-File "src/Floundroid.Bitboard/Zobrist.fs" @"
namespace Floundroid.Bitboard

module Zobrist =
    let init () = ()
"@

Write-File "src/Floundroid.Bitboard/TranspositionTable.fs" @"
namespace Floundroid.Bitboard

module TranspositionTable =
    let clear () = ()
"@

Write-File "src/Floundroid.Bitboard/BitMoveGen.fs" @"
namespace Floundroid.Bitboard

module BitMoveGen =
    let generate () = ()
"@

# -------------------------------
# App
# -------------------------------
Write-File "src/Floundroid.App/Program.fs" @"
open Floundroid.UCI

[<EntryPoint>]
let main _ =
    UciLoop.run()
    0
"@

# -------------------------------
# Tests
# -------------------------------
Write-File "tests/Floundroid.Tests/PerftTests.fs" @"
module Floundroid.Tests.PerftTests

open Expecto
open Floundroid.Core.Types
open Floundroid.Core.Perft

[<Tests>]
let perftSuite =
    testList "Perft" [
        test "Dummy perft" {
            let dummyBoard =
                { Pieces = Map.empty
                  SideToMove = Color.White
                  CastlingRights = ""
                  EnPassantSquare = None
                  HalfmoveClock = 0
                  FullmoveNumber = 1 }
            let result = Perft.perft 0 dummyBoard
            Expect.equal result 1L "Perft(0) should be 1"
        }
    ]
"@

Write-File "tests/Floundroid.Tests/FenTests.fs" @"
module Floundroid.Tests.FenTests

open Expecto

[<Tests>]
let fenSuite =
    testList "FEN" [
        test "Placeholder" {
            Expect.isTrue true "TODO"
        }
    ]
"@

Write-File "tests/Floundroid.Tests/MoveGenTests.fs" @"
module Floundroid.Tests.MoveGenTests

open Expecto

[<Tests>]
let moveGenSuite =
    testList "MoveGen" [
        test "Placeholder" {
            Expect.isTrue true "TODO"
        }
    ]
"@

Write-File "tests/Floundroid.Tests/SearchTests.fs" @"
module Floundroid.Tests.SearchTests

open Expecto

[<Tests>]
let searchSuite =
    testList "Search" [
        test "Placeholder" {
            Expect.isTrue true "TODO"
        }
    ]
"@

Write-Host "Floundroid project scaffold created successfully!"
