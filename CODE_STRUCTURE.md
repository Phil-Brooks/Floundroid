# Floundroid Technical Reference
Generated on: 13/06/2026 11:10:21

## 📑 Table of Contents
- [Floundroid](#-module-floundroid)
- [Colour](#-module-colour)
- [File](#-module-file)
- [Rank](#-module-rank)
- [Square](#-module-square)
- [Bitboard](#-module-bitboard)
- [PieceType](#-module-piecetype)
- [Piece](#-module-piece)
- [CastlingRights](#-module-castlingrights)
- [Move](#-module-move)
- [Attack](#-module-attack)
- [Board](#-module-board)
- [MoveGen](#-module-movegen)
- [San](#-module-san)
- [Evaluation](#-module-evaluation)
- [Search](#-module-search)
- [Perft](#-module-perft)
- [Debug](#-module-debug)
- [UciLoop](#-module-uciloop)



## 📦 module Floundroid
---

#### 🧩 `type Colour =`
> Colours are represented as a discriminated union with two cases: White and Black.

## 📦 module Colour
---
- **fn** `toChar`
    - *Converts a Colour to its character representation ('w' for White, 'b' for Black).*
- **fn** `fromChar`
    - *Converts a character to a Colour ('w' for White, 'b' for Black).*
- **fn** `opposite`
    - *Returns the opposite colour.*

#### 🧩 `type File =`
> Files are represented as integers from 0 to 7, where 0 = file a and 7 = file h.

## 📦 module File
---
- **fn** `toInt`
    - *Converts a File to its integer representation (0-7).*
- **fn** `fromInt`
    - *Converts an integer to a File (0-7).*
- **fn** `fromChar`
    - *Converts a character to a File ('a'-'h').*

#### 🧩 `type Rank =`
> Ranks are represented as integers from 0 to 7, where 0 = rank 1 and 7 = rank 8.

## 📦 module Rank
---
- **fn** `toInt`
    - *Converts a Rank to its integer representation (0-7).*
- **fn** `fromInt`
    - *Converts an integer to a Rank (0-7).*
- **fn** `fromChar`
    - *Converts a character to a Rank ('1'-'8').*

#### 🧩 `type Square = int`
> Squares are represented as integers from 0 to 63, where 0 = a1, 1 = b1, ..., 63 = h8.

## 📦 module Square
---
- **fn** `toString`
    - *Converts a Square to its string representation.*
- **fn** `fromString`
    - *Converts a string representation of a square (e.g., "d4") to a Square.*

#### 🧩 `type Bitboard = uint64`
> Bitboards are 64-bit unsigned integers where each bit represents a square.
> Bit 0 is a1, Bit 7 is h1, Bit 63 is h8.

## 📦 module Bitboard
---
- **fn** `inline`
    - *Sets the bit at the given square.*
- **fn** `inline`
    - *Clears the bit at the given square.*
- **fn** `inline`
    - *Checks if a square is set.*
- **fn** `inline`
    - *Returns the number of set bits (population count).*
- **fn** `inline`
    - *Returns the index of the least significant bit (0-63) and clears it from the bitboard.*
    - *This is a high-performance way to iterate through pieces.*
- **fn** `toString`
    - *Visualizes the bitboard as an 8x8 grid for debugging.*

#### 🧩 `type PieceType =`

## 📦 module PieceType
---
- **fn** `toChar`
    - *Converts a PieceType to its character representation ('p', 'n', 'b', 'r', 'q', 'k').*
- **fn** `fromChar`
    - *Converts a character to a PieceType.*

#### 🧩 `type Piece = { Colour: Colour; Kind: PieceType }`
> A Piece consists of a Colour and a PieceType.

## 📦 module Piece
---
- **fn** `toChar`
    - *Converts a Piece to its character representation (uppercase for White, lowercase for Black).*
- **fn** `fromChar`
    - *Converts a character to a Piece, determining colour from case (uppercase = White, lowercase = Black).*

#### 🧩 `type CastlingRights =`
> Castling rights are represented as a record with four boolean fields.

## 📦 module CastlingRights
---
- **fn** `none`
- **fn** `fromString`
    - *Converts a string representation of castling rights to a CastlingRights value.*
- **fn** `toString`
    - *Converts a CastlingRights value to its string representation.*

#### 🧩 `type MoveKind =`
> Move kinds represent the different types of moves in chess, including quiet moves, captures, promotions, en passant, and castling.

#### 🧩 `type Move =`
> A Move consists of a source square, a destination square, and a MoveKind indicating the type of move.

## 📦 module Move
---
- **fn** `toUci`
    - *Converts a Move to its UCI string representation.*
- **fn** `fromUci`
    - *Converts a UCI string representation of a move to a Move value.*

#### 🧩 `type Board =`
> The Board type represents the state of a chess game, including piece placement, side to move, castling rights, en passant target square, and move clocks.

## 📦 module Attack
---
- **fn** `isSquareAttacked`
    - *Checks if a square is attacked by the specified colour.*

## 📦 module Board
---
- **fn** `empty`
- **fn** `setPiece`
    - *Sets a piece on a square.*
- **fn** `fromFen`
    - *Parses a FEN string and returns a Board record representing the position.*
- **fn** `toFen`
    - *Converts a Board record to its FEN string representation.*
- **fn** `isInCheckFor`
    - *Checks if a player is in check.*
    - ***Param colour**: The colour of the player to check.*
    - ***Param b**: The current game state.*
    - ***Returns**: True if the player is in check, false otherwise.*
- **fn** `getPins`
    - *Gets a map of pinned pieces and the squares they are pinned to.*
- **fn** `applyMove`
    - *Executes a move on the board and returns a new immutable board state.*
    - *Updates castling rights, en passant targets, and move clocks.*
    - ***Param m**: The validated move to apply.*
    - ***Param b**: The current game state.*
    - ***Returns**: A new Board record reflecting the post-move state.*
- **fn** `prettyPrint`
    - *Prints the board in a human-readable format.*

## 📦 module MoveGen
---
- **fn** `getPseudoLegalMoves`
    - *Gets all pseudo-legal moves for the current position.*
- **fn** `getLegalMoves`
    - *Gets all legal moves for the current position.*

## 📦 module San
---
- **fn** `toSan`
    - *Converts a move to Standard Algebraic Notation (SAN) based on the current board state.*

## 📦 module Evaluation
---
- **fn** `pieceValue`
    - *Assigns a base value to each piece type for evaluation purposes.*
- **fn** `evaluate`
    - *Evaluates the board position from White's perspective. Positive scores favor White, negative scores favor Black.*

## 📦 module Search
---
- **fn** `quiesce`
    - *Quiescence search: plays out all captures until the position is stable.*
- **fn** `negamax`
    - *Negamax search with alpha-beta pruning.*

#### 🧩 `type PerftSuiteItem =`
> Represents a single test case for the perft suite, including the position (FEN), expected node counts at various depths, and a name for identification.

## 📦 module Perft
---
- **fn** `countNodes`
    - *Counts the number of leaf nodes at a given depth from the current board state.*
- **fn** `divide`
    - *Divides the perft calculation for a given depth and board state.*
- **fn** `runFullSuite`
    - *Runs the full perft suite up to a specified maximum depth, comparing results against expected values.*

## 📦 module Debug
---
- **fn** `displayMoves`
    - *1.5.1 - Move list visualisation*
- **fn** `verify`
    - *1.5.2 - Board consistency checker*
- **fn** `displayAttackMap`
    - *1.5.3 - Attack map visualiser*

## 📦 module UciLoop
---
- **fn** `run`
- **fn** `main`
