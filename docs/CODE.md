# Code Documentation

# Code File: Program.fs

## 📑 Table of Contents
- [Floundroid](#module-floundroid)
- [Colour](#module-colour)
- [File](#module-file)
- [Rank](#module-rank)
- [Square](#module-square)
- [PieceType](#module-piecetype)
- [Piece](#module-piece)
- [CastlingRights](#module-castlingrights)
- [Move](#module-move)
- [Bitboard](#module-bitboard)
- [BitboardSet](#module-bitboardset)
- [SlidingAttackGen](#module-slidingattackgen)
- [Magic](#module-magic)
- [BitboardGen](#module-bitboardgen)
- [Attack](#module-attack)
- [Zobrist](#module-zobrist)
- [TranspositionTable](#module-transpositiontable)
- [Board](#module-board)
- [MoveGen](#module-movegen)
- [San](#module-san)
- [Evaluation](#module-evaluation)
- [Search](#module-search)
- [Perft](#module-perft)
- [Debug](#module-debug)
- [UciLoop](#module-uciloop)



## 📦 module Floundroid
---

#### 🧩 `type Colour`
> Colour is represented as an integer 0–1, where 0 = White and 1 = Black.

## 📦 module Colour
---
- **fn** `toChar`
    - *Converts a Colour to its character representation ('w' or 'b').*
- **fn** `fromChar`
    - *Converts a character ('w' or 'b') to a Colour.*
- **fn** `opposite`
    - *Returns the opposite colour.*

#### 🧩 `type File`

## 📦 module File
---
- **fn** `firstChar`
- **fn** `toInt`
    - *Converts a File to its integer representation (0–7).*
- **fn** `fromInt`
    - *Converts an integer (0–7) to a File.*
- **fn** `toChar`
    - *Converts a File to its character representation ('a'–'h').*
- **fn** `fromChar`
    - *Converts a character ('a'–'h') to a File.*

#### 🧩 `type Rank`

## 📦 module Rank
---
- **fn** `firstChar`
- **fn** `toInt`
    - *Converts a Rank to its integer representation (0–7).*
- **fn** `fromInt`
    - *Converts an integer (0–7) to a Rank.*
- **fn** `toChar`
    - *Converts a Rank to its character representation ('1'–'8').*
- **fn** `fromChar`
    - *Converts a character ('1'–'8') to a Rank.*

#### 🧩 `type Square = int`
> Squares are represented as integers 0–63, where 0 = a1 and 63 = h8.

## 📦 module Square
---
- **fn** `ofFileRank`
    - *Converts a File and Rank to a Square.*
- **fn** `file`
    - *Gets the File of a Square.*
- **fn** `rank`
    - *Gets the Rank of a Square.*
- **fn** `toString`
    - *Converts a Square to algebraic notation (e.g. "e4").*
- **fn** `fromString`
    - *Converts algebraic notation (e.g. "d4") to a Square.*
- **fn** `isOnBoard`
    - *Checks if a file/rank pair is on the board.*

#### 🧩 `type PieceType`

## 📦 module PieceType
---
- **fn** `chars`
- **fn** `toChar`
    - *Converts a PieceType to its character representation ('p'..'k').*
- **fn** `fromChar`
    - *Converts a character to a PieceType.*

#### 🧩 `type Piece`

## 📦 module Piece
---
- **fn** `colour`
- **fn** `kind`
- **fn** `toChar`
- **fn** `fromChar`

#### 🧩 `type CastlingRights`

## 📦 module CastlingRights
---
- **fn** `fromString`
- **fn** `toString`

#### 🧩 `type Move`

## 📦 module Move
---
- **fn** `fromSq`
- **fn** `toSq`
- **fn** `kind`
- **fn** `promo`
- **fn** `toUci`

#### 🧩 `type Bitboard = uint64`
> Bitboards are 64-bit unsigned integers where each bit represents a square.
> Bit 0 is a1, Bit 7 is h1, Bit 63 is h8.

## 📦 module Bitboard
---
- **fn** `empty`
- **fn** `all`
- **fn** `set`
    - *Sets the bit at the given square.*
- **fn** `clear`
    - *Clears the bit at the given square.*
- **fn** `contains`
    - *Checks if a square is set.*
- **fn** `count`
    - *Returns the number of set bits (population count).*
- **fn** `popLsb`
    - *Returns the index of the least significant bit (0-63) and clears it from the bitboard.*
    - *This is a high-performance way to iterate through pieces.*
- **fn** `toString`
    - *Visualizes the bitboard as an 8x8 grid for debugging.*

#### 🧩 `type BitboardSet`
> A collection of bitboards representing all pieces on the board.

## 📦 module BitboardSet
---
- **fn** `empty`
- **fn** `fromMap`
    - *Converts a Piece Map into a BitboardSet.*
- **fn** `getPieceAt`
    - *Identifies the piece (if any) at a specific square using bitboards.*
- **fn** `togglePiece`
    - *A helper to flip a piece on/off. Essential for incremental updates.*
- **fn** `allPieces`
    - *Returns a sequence of all (Square, Piece) pairs currently on the board.*

## 📦 module SlidingAttackGen
---
- **fn** `bishopAttacks`
    - *Generates a bitboard of all squares a Bishop attacks from a given square,*
    - *accounting for blockers. This is the "slow" version used for table init.*
- **fn** `rookAttacks`
    - *Generates a bitboard of all squares a Rook attacks from a given square,*
    - *accounting for blockers. This is the "slow" version used for table init.*
- **fn** `bishopMask`
    - *The "Mask" for Magic Bitboards: This excludes the very last square*
    - *on the edge of the board for every ray, because a blocker on the*
    - *edge doesn't change the attack bitboard.*
- **fn** `rookMask`

## 📦 module Magic
---

#### 🧩 `type MagicEntry = { Mask: Bitboard; Offset: int }`
> Represents a magic entry for a square, containing the mask and the offset into the attack table.
- **fn** `bishopTable`
- **fn** `rookTable`
- **fn** `bishopEntries`
- **fn** `rookEntries`
- **fn** `getIndex`
    - *This maps an occupancy bitboard to a unique index from 0 to 2^bits-1*
    - *It is essentially a manual "PEXT" instruction.*
- **fn** `private`
    - *Generates every possible blocker pattern for a mask (reverse of getIndex)*
- **fn** `init`
    - *Initializes the sliding attack tables for bishops and rooks.*

## 📦 module BitboardGen
---
- **fn** `knightAttacks`
    - *Pre-calculated knight attacks for every square*
- **fn** `kingAttacks`
    - *Pre-calculated king attacks for every square*
- **fn** `pawnAttacks`
    - *Pawn attacks: [Colour index 0=White, 1=Black, Square 0-63]*
- **fn** `private`

#### 🧩 `type Board`
> The Board type represents the state of a chess game, including piece placement, side to move, castling rights, en passant target square, and move clocks.

## 📦 module Attack
---
- **fn** `isSquareAttacked`

## 📦 module Zobrist
---

#### 🧩 `type ZobristTable = {`
> Storage for all random keys used for hashing.
- **fn** `pieceIdx`
    - *Maps PieceType to an index 0-5*
- **fn** `colourIdx`
    - *Maps Colour to index 0-1*
- **fn** `private`
    - *Pre-calculates the table with a fixed seed for reproducibility.*
- **fn** `Table`
    - *The global lookup table for Zobrist keys.*
- **fn** `getPieceKey`
    - *Gets the key for a specific piece on a square.*
- **fn** `getCastlingKey`
    - *Gets the key for a specific set of castling rights.*
- **fn** `getEnPassantKey`
    - *Gets the key for an En Passant file (0-7).*

## 📦 module TranspositionTable
---

#### 🧩 `type NodeFlag`
> Flags for TT entries: Exact (PV), Alpha (Upper bound), Beta (Lower bound)

#### 🧩 `type TTEntry = {`
- **fn** `emptyEntry`
- **fn** `advanceAge`
    - *Advances the age of the transposition table, allowing for aging out old entries.*
- **fn** `SIZE`
    - *A table size of 2^20 is roughly 32-64MB depending on padding.*
- **fn** `table`
- **fn** `mateToTT`
    - *Adjusts mate scores from the search to be relative to the root.*
    - *This ensures "Mate in 5" found at depth 10 is stored correctly.*
- **fn** `mateFromTT`
    - *Adjusts mate scores from the TT back to the search, reversing the previous adjustment.*
- **fn** `clear`
    - *Clears the transposition table, resetting all entries to empty.*
- **fn** `store`
    - *Stores an entry in the transposition table with the given parameters.*
- **fn** `probe`

## 📦 module Board
---
- **fn** `fromUci`
- **fn** `empty`
- **fn** `tryGetPiece`
    - *Tries to get a piece from a square (Source of truth: Bitboards).*
- **fn** `isOccupied`
    - *Checks if a square is occupied (Source of truth: Bitboards).*
- **fn** `findKing`
    - *Find the king square (Needed for check detection).*
- **fn** `setPiece`
    - *Sets a piece on a square and updates bitboards (Source of truth: Bitboards).*
- **fn** `calculateHash`
    - *Calculates the full Zobrist hash from scratch (Used for FEN initialization)*
- **fn** `fromFen`
    - *Parses a FEN string and returns a Board record representing the position.*
- **fn** `toFen`
    - *Converts a Board record to its FEN string representation.*
- **fn** `isInCheckFor`
    - *Checks if a player is in check.*
    - ***Param colour**: The colour of the player to check.*
    - ***Param b**: The current game state.*
    - ***Returns**: True if the player is in check, false otherwise.*
- **fn** `isInCheck`
    - *Checks if the side to move is currently in check.*
- **fn** `applyMove`
    - *Executes a move on the board and returns a new immutable board state.*
    - *Updates castling rights, en passant targets, and move clocks.*
    - ***Param m**: The validated move to apply.*
    - ***Param b**: The current game state.*
    - ***Returns**: A new Board record reflecting the post-move state.*
    - *Executes a move, updating both Bitboards and the Piece Map.*
    - *This is the final step before the Map is removed entirely.*
- **fn** `applyNullMove`
    - *Executes a null move, updating side to move, en passant, halfmove clock, fullmove number, and hash.*
- **fn** `hasInsufficientMaterial`
    - *Checks if the board has insufficient material for checkmate.*
- **fn** `prettyPrint`
    - *Prints the board in a human-readable format.*

## 📦 module MoveGen
---
- **fn** `getPseudoLegalMoves`
- **fn** `getLegalMoves`
    - *Gets all legal moves for the current position.*
- **fn** `getCaptureMoves`
    - *Optimized generator for Quiescence Search: Only returns Captures, En Passants, and Promotions.*

## 📦 module San
---
- **fn** `toSan`
    - *Converts a move to Standard Algebraic Notation (SAN) based on the current board state.*

## 📦 module Evaluation
---
- **fn** `pieceValue`
    - *Assigns a base value to each piece type for evaluation purposes.*
- **fn** `pawnPst`
    - *The pawn PST is designed for the middle game. In a more complete engine, we would switch to a different PST in the endgame.*
- **fn** `knightPst`
    - *The knight PST is designed for the middle game. In a more complete engine, we would switch to a different PST in the endgame.*
- **fn** `bishopPst`
    - *The bishop PST is designed for the middle game. In a more complete engine, we would switch to a different PST in the endgame.*
- **fn** `rookPst`
    - *The rook PST is designed for the middle game. In a more complete engine, we would switch to a different PST in the endgame.*
- **fn** `queenPst`
    - *The queen PST is designed for the middle game. In a more complete engine, we would switch to a different PST in the endgame.*
- **fn** `kingPst`
    - *The king PST is designed for the middle game. In a more complete engine, we would switch to a different PST in the endgame.*
- **fn** `private`
- **fn** `private`
- **fn** `private`
- **fn** `private`
- **fn** `pawnStructureScore`
    - *Evaluates the pawn structure of the board, returning a score from White's perspective.*
- **fn** `kingSafety`
    - *Very small king-safety heuristic: penalise missing pawn shield for short-castled kings.*
- **fn** `evaluate`
    - *Evaluates the board position from White's perspective. Positive scores favor White, negative scores favor Black.*

## 📦 module Search
---
- **fn** `MATE_VALUE`
- **fn** `INF`
- **fn** `killerMoves`
- **fn** `clearKillers`
- **fn** `historyTable`
- **fn** `clearHistory`
- **fn** `isRepetition`
- **fn** `quiesce`
    - *Quiescence search: plays out tactical moves until the position is stable.*
- **fn** `negamaxInternal`
    - *Internal negamax implementation with Null-Move Pruning (allowNull).*
- **fn** `negamax`
    - *Negamax search with alpha-beta pruning and Transposition Table integration.*
- **fn** `findBestMove`
    - *Iterative Deepening*

#### 🧩 `type PerftSuiteItem`
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
- **fn** `startFen`
- **fn** `tryGetIntArg`
- **fn** `calculateTargetTime`
- **fn** `run`

