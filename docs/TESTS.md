# Tests Documentation

# Code File: Tests.fs

## 📑 Table of Contents
- [ColourTests](#module-colourtests)
- [FileTests](#module-filetests)
- [RankTests](#module-ranktests)
- [SquareTests](#module-squaretests)
- [PieceTypeTests](#module-piecetypetests)
- [PieceTests](#module-piecetests)
- [BitboardTests](#module-bitboardtests)
- [BitboardSetTests](#module-bitboardsettests)
- [MagicTests](#module-magictests)
- [BitboardGenTests](#module-bitboardgentests)
- [ZobristTests](#module-zobristtests)
- [TTTests](#module-tttests)
- [BoardTests](#module-boardtests)
- [MoveGenTests](#module-movegentests)
- [EvaluationTests](#module-evaluationtests)
- [SearchTests](#module-searchtests)
- [PerftTests](#module-perfttests)
- [DebugTests](#module-debugtests)
- [UciTests](#module-ucitests)



## 📦 module ColourTests
---
- **fn** `opposite works both ways`
    - *Tests that the opposite of a colour is correctly computed.*
- **fn** `opposite is involutive (example)`
    - *Tests that the opposite of a colour is involutive.*
- **fn** `fromChar toChar roundtrip`
    - *Tests that the colour character conversion works correctly.*
- **fn** `toChar fromChar roundtrip`
    - *Tests that the colour character conversion works correctly.*
- **fn** `fromChar rejects invalid characters`
    - *Tests that the colour character conversion rejects invalid characters.*

## 📦 module FileTests
---
- **fn** `File char roundtrip`
- **fn** `File toChar fromChar roundtrip`

## 📦 module RankTests
---
- **fn** `Rank char roundtrip`
- **fn** `Rank toChar fromChar roundtrip`

## 📦 module SquareTests
---
- **fn** `Square file/rank roundtrip`
- **fn** `Square string roundtrip`
- **fn** `Square.fromString rejects invalid strings`
- **fn** `Square.isOnBoard works for valid and invalid coords`

## 📦 module PieceTypeTests
---
- **fn** `PieceType char roundtrip`

## 📦 module PieceTests
---
- **fn** `Piece char roundtrip`

## 📦 module BitboardTests
---
- **fn** `Bitboard set and contains works`
- **fn** `Bitboard count works`
- **fn** `Bitboard popLsb iterates and clears bits`
- **fn** `Constants are correct`

## 📦 module BitboardSetTests
---
- **fn** `allSquares`
- **fn** `allPieces`
- **fn** `empty has no pieces`
- **fn** `togglePiece adds then removes piece`
- **fn** `togglePiece updates totals and occupancy`
- **fn** `getPieceAt returns correct piece after toggle`
- **fn** `getPieceAt returns -1 for untouched squares`
- **fn** `allPieces returns exactly the toggled pieces`

#### 🧩 `type PieceGen`

#### 🧩 `type SquareGen`

## 📦 module MagicTests
---
- **fn** `Rook slow attacks are blocked correctly`
- **fn** `Bishop slow attacks hit diagonals`
- **fn** `Rook mask excludes edges`
- **fn** `Bishop mask excludes edges`
- **fn** `Table initialization matches slow reference for first 4 squares`
- **fn** `Bishop table lookup matches slow reference for center square`
- **fn** `getIndex and getBlockers are perfect inverses for rook masks`
- **fn** `getIndex and getBlockers are perfect inverses for bishop masks`
- **fn** `rookMask never includes the origin square`
- **fn** `bishopMask never includes the origin square`

#### 🧩 `type BlockerGen`

## 📦 module BitboardGenTests
---
- **fn** `Knight attacks on b1 are correct`
- **fn** `Knight on a-file does not wrap to h-file`
- **fn** `King attacks in corner are 3`
- **fn** `White pawn attacks from e2 hit d3 and f3`
- **fn** `Black pawn attacks from d7 hit c6 and e6`
- **fn** `Pawn on a-file does not wrap when attacking`
- **fn** `Knight on d4 has 8 attacks`
- **fn** `King on d4 has 8 attacks`
- **fn** `White pawn on 7th rank has no forward attacks`
- **fn** `Black pawn on 2nd rank has no forward attacks`
- **fn** `Knight on h8 has 2 attacks`
- **fn** `King on h8 has 3 attacks`
- **fn** `White pawn on a7 only attacks b8`

## 📦 module ZobristTests
---
- **fn** `Zobrist Table is deterministic with fixed seed`
- **fn** `Keys are unique for different pieces and squares`
- **fn** `Castling combinations result in different keys`
- **fn** `All piece-square Zobrist keys are unique`
- **fn** `All 16 castling Zobrist keys are unique`
- **fn** `En passant keys are unique per file`
- **fn** `En passant None returns 0`
- **fn** `Empty board Zobrist hash is deterministic`
- **fn** `Piece key XORing twice cancels out`

## 📦 module TTTests
---
- **fn** `TT can store and retrieve an entry`
- **fn** `Mate scores are adjusted correctly for ply`
- **fn** `TT handles collisions via replacement`
- **fn** `TT reduces node count in transpositions`
- **fn** `Search does not poison TT when cancelled`
- **fn** `TT Ageing replaces shallow new move over deep old move`
- **fn** `TT clear resets all entries`
- **fn** `TT probe returns None on hash mismatch at same index`
- **fn** `TT keeps deeper entry when ages are equal`
- **fn** `TT replaces deep old entry with shallow new entry of newer age`
- **fn** `mateToTT and mateFromTT are identity for non-mate scores`

## 📦 module BoardTests
---
- **fn** `Incremental hash matches full hash after quiet move`
- **fn** `Hash handles piece captures correctly`
- **fn** `Hash handles promotion correctly`
- **fn** `Hash remains same after repetition of moves`
- **fn** `Square attacked by knight`
- **fn** `Square attacked by bishop`
- **fn** `Square attacked by pawn`
- **fn** `Square not attacked`
- **fn** `White pawn attacks upwards (should be detected but isSquareAttacked returns false)`
- **fn** `Black pawn attack detection is correct`
- **fn** `Empty board has no pieces`
- **fn** `Set and get piece works`
- **fn** `applyMove updates piece positions and side to move`
- **fn** `applyMove increments fullmove number after black moves`
- **fn** `Kingside castling moves rook`
- **fn** `En passant removes captured pawn`
- **fn** `Promotion creates promoted piece`
- **fn** `Moving rook removes kingside castling rights`
- **fn** `Moving King removes all castling rights`
- **fn** `Capturing opponent rook removes their castling rights`
- **fn** `Pawn move resets halfmove clock`
- **fn** `Capture resets halfmove clock`
- **fn** `Quiet non-pawn move increments halfmove clock`
- **fn** `fromUci parses promotion correctly`
- **fn** `fromUci parses kingside castling correctly`
- **fn** `fromUci parses en passant correctly`
- **fn** `fromUci parses capture correctly`
- **fn** `fromUci parses quiet move correctly`
- **fn** `isInCheck detects simple check`
- **fn** `isInCheck is false when king not attacked`
- **fn** `applyNullMove flips side and updates hash consistently`
- **fn** `K vs K is insufficient material`
- **fn** `KN vs K is insufficient material`
- **fn** `KB vs KB on opposite colors is not insufficient`
- **fn** `White is in check from rook`
- **fn** `White is not in check`
- **fn** `Black is in check from knight`

## 📦 module MoveGenTests
---
- **fn** `Starting position has 20 pseudo-legal moves`
- **fn** `Knight in center has 8 moves`
- **fn** `Pawn captures correctly`
- **fn** `Pawn En Passant is detected`
- **fn** `Slider logic stops at edge and captures enemies`
- **fn** `Starting position has 20 legal moves`
- **fn** `Black cannot castle out of check after d1d8`
- **fn** `Black cannot castle through check`
- **fn** `getCaptureMoves ignores quiet moves`
- **fn** `Pawn cannot jump over a piece with double push`
- **fn** `UCI move parser identifies e1g1 as a Castling move not a Quiet move`
- **fn** `White pawn generates 4 quiet promotion moves`
- **fn** `White pawn generates 4 capture promotion moves`
- **fn** `Black pawn generates 4 quiet promotion moves`
- **fn** `Black pawn generates 4 capture promotion moves`
- **fn** `Pinned knight has no legal moves`
- **fn** `King cannot move into check`
- **fn** `Illegal en passant exposing king is filtered out`
- **fn** `Cannot castle into attacked destination square`
- **fn** `All legal moves are a subset of pseudo-legal moves`
- **fn** `Checkmated side has no legal moves`
- **fn** `Stalemated side has no legal moves and is not in check`
- **fn** `getCaptureMoves only returns legal captures`
- **fn** `Starting position perft(1) and perft(2) via getLegalMoves`

## 📦 module EvaluationTests
---
- **fn** `Starting position evaluation is perfectly symmetrical (0)`
- **fn** `Knight on D4 is valued higher than Knight on A1 (PST)`
- **fn** `Black piece positioning is mirrored correctly`
- **fn** `Evaluating an empty board returns 0`
- **fn** `Starting position pawn structure is symmetrical`
- **fn** `Doubled pawns are penalized`
- **fn** `Isolated pawns are penalized compared with connected pawns`
- **fn** `Passed pawns are rewarded`
- **fn** `Black passed pawn scores for black`
- **fn** `White missing g-pawn is worse than full pawn shield`
- **fn** `open h-file next to white king is penalised`
- **fn** `no open-file penalty when king not short-castled`
- **fn** `enemy knight near white king is penalised`
- **fn** `Evaluation is antisymmetric under colour swap`
- **fn** `Short-castled king with full shield scores better than uncastled king`

## 📦 module SearchTests
---
- **fn** `Search finds a mate in one (Scholars Mate)`
- **fn** `Search finds a simple winning capture`
- **fn** `Search identifies stalemate as draw`
- **fn** `Search returns a fallback move when immediately cancelled`
- **fn** `Board applyNullMove toggles side to move and clears en passant`
- **fn** `Enabling NMP does not change the evaluation at fixed depth`
- **fn** `MVV-LVA prefers Pawn takes Queen over Queen takes Pawn`
- **fn** `KB vs KB same color is a draw`
- **fn** `Negamax returns 0 immediately if current position is a repetition`
- **fn** `isRepetition correctly identifies hash in history`
- **fn** `Negamax returns 0 if halfmove clock is 100`
- **fn** `Negamax returns 0 if halfmove clock reaches 100 during search`
- **fn** `Killer table is writable`
- **fn** `History table is writable`
- **fn** `Null move pruning is disabled when in check`
- **fn** `Null move pruning disabled in zugzwang-like endgame`
- **fn** `TT Exact entry causes immediate cutoff`
- **fn** `TT move is searched first`
- **fn** `Search returns correct mate distance`

## 📦 module PerftTests
---
- **fn** `Initial position depth 1 is 20`
- **fn** `Initial position depth 2 is 400`
- **fn** `Kiwipete depth 1 is 48`
- **fn** `Perft Position 3 Depth 3 is 2812`
- **fn** `Initial position depth 3 is 8902`
- **fn** `Initial position perft divide depth 2 e2e4 is 20`
- **fn** `SAN for e2e4 is e4`

## 📦 module DebugTests
---
- **fn** `Debug.verify accepts a legal starting position`
- **fn** `Debug verify detects two white kings`
- **fn** `Debug verify detects pawn on illegal rank`
- **fn** `Debug verify detects illegal check on side not to move`
- **fn** `Debug displayMoves prints legal moves with SAN`
- **fn** `Debug displayAttackMap shows rook attack pattern`

## 📦 module UciTests
---
- **fn** `Position startpos moves e2e4 results in correct board`
- **fn** `Go time target includes white increment when white is to move`
- **fn** `Go time target includes black increment when black is to move`
- **fn** `tryGetIntArg extracts integer after key`
- **fn** `tryGetIntArg returns None when key missing`

