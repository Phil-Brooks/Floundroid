# Tests Documentation

## 📑 Table of Contents

- [TypesTests.fs](#code-file-typestests.fs)
- [Tests.fs](#code-file-tests.fs)

# Code File: TypesTests.fs

## 📑 Table of Contents
- [ColourTests](#module-colourtests)
- [FileTests](#module-filetests)
- [RankTests](#module-ranktests)
- [SquareTests](#module-squaretests)



## 📦 module ColourTests
---
- **fn** `opposite works both ways`
    - *Tests that the opposite of a colour is correctly computed.*
- **fn** `opposite is involutive`
    - *Tests that the opposite of a colour is involutive.*
- **fn** `fromChar toChar roundtrip`
    - *Tests that the colour character conversion works correctly.*
- **fn** `toChar fromChar roundtrip`
    - *Tests that the colour character conversion works correctly.*
- **fn** `fromChar rejects invalid characters`
    - *Tests that the colour character conversion rejects invalid characters.*

## 📦 module FileTests
---
- **fn** `File int roundtrip`
- **fn** `File char roundtrip`

## 📦 module RankTests
---
- **fn** `Rank int roundtrip`
- **fn** `Rank char roundtrip`

## 📦 module SquareTests
---
- **fn** `Square file/rank roundtrip`
- **fn** `Square string roundtrip`

# Code File: Tests.fs

## 📑 Table of Contents
- [PieceTypeTests](#module-piecetypetests)
- [PieceTests](#module-piecetests)
- [BoardTests](#module-boardtests)
- [FenTests](#module-fentests)
- [MoveGenTests](#module-movegentests)
- [PromotionTests](#module-promotiontests)
- [AttackTests](#module-attacktests)
- [CheckDetectionTests](#module-checkdetectiontests)
- [LegalMoveFilteringTests](#module-legalmovefilteringtests)
- [PerftTests](#module-perfttests)
- [EvaluationTests](#module-evaluationtests)
- [SearchTests](#module-searchtests)
- [BitboardTests](#module-bitboardtests)
- [SlidingAttackTests](#module-slidingattacktests)
- [ZobristTests](#module-zobristtests)
- [HashTests](#module-hashtests)
- [TTTests](#module-tttests)
- [MoveOrderingTests](#module-moveorderingtests)
- [InsufficientMaterialTests](#module-insufficientmaterialtests)
- [UciParsingTests](#module-uciparsingtests)
- [RepetitionTests](#module-repetitiontests)
- [FiftyMoveRuleTests](#module-fiftymoveruletests)



## 📦 module PieceTypeTests
---
- **fn** `PieceType char roundtrip`

## 📦 module PieceTests
---
- **fn** `Piece char roundtrip`

## 📦 module BoardTests
---
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

## 📦 module FenTests
---

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

## 📦 module PromotionTests
---
- **fn** `White pawn generates 4 quiet promotion moves`
- **fn** `White pawn generates 4 capture promotion moves`
- **fn** `Black pawn generates 4 quiet promotion moves`
- **fn** `Black pawn generates 4 capture promotion moves`

## 📦 module AttackTests
---
- **fn** `Square attacked by knight`
- **fn** `Square attacked by bishop`
- **fn** `Square attacked by pawn`
- **fn** `Square not attacked`
- **fn** `White pawn attacks upwards (should be detected but isSquareAttacked returns false)`
- **fn** `Black pawn attack detection is correct`

## 📦 module CheckDetectionTests
---
- **fn** `White is in check from rook`
- **fn** `White is not in check`
- **fn** `Black is in check from knight`

## 📦 module LegalMoveFilteringTests
---
- **fn** `Pinned knight has no legal moves`
- **fn** `King cannot move into check`
- **fn** `Illegal en passant exposing king is filtered out`
- **fn** `Cannot castle into attacked destination square`

## 📦 module PerftTests
---
- **fn** `Initial position depth 1 is 20`
- **fn** `Initial position depth 2 is 400`
- **fn** `Kiwipete depth 1 is 48`
- **fn** `Perft Position 3 Depth 3 is 2812`

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

## 📦 module SearchTests
---
- **fn** `Search finds a mate in one (Scholars Mate)`
- **fn** `Search finds a simple winning capture`
- **fn** `Search identifies stalemate as draw`
- **fn** `Search returns a fallback move when immediately cancelled`
- **fn** `Board applyNullMove toggles side to move and clears en passant`
- **fn** `Enabling NMP does not change the evaluation at fixed depth`

## 📦 module BitboardTests
---
- **fn** `Bitboard set and contains works`
- **fn** `Bitboard count works`
- **fn** `Bitboard popLsb iterates and clears bits`
- **fn** `Knight attacks on b1 are correct`
- **fn** `Knight on a-file does not wrap to h-file`
- **fn** `King attacks in corner are 3`
- **fn** `White pawn attacks from e2 hit d3 and f3`
- **fn** `Black pawn attacks from d7 hit c6 and e6`
- **fn** `Pawn on a-file does not wrap when attacking`

## 📦 module SlidingAttackTests
---
- **fn** `Rook slow attacks are blocked correctly`
- **fn** `Bishop slow attacks hit diagonals`
- **fn** `Rook mask excludes edges`
- **fn** `Bishop mask excludes edges`
- **fn** `Table initialization matches slow reference for first 4 squares`
- **fn** `Bishop table lookup matches slow reference for center square`

## 📦 module ZobristTests
---
- **fn** `Zobrist Table is deterministic with fixed seed`
- **fn** `Keys are unique for different pieces and squares`
- **fn** `Castling combinations result in different keys`

## 📦 module HashTests
---
- **fn** `Incremental hash matches full hash after quiet move`
- **fn** `Hash handles piece captures correctly`
- **fn** `Hash handles promotion correctly`
- **fn** `Hash remains same after repetition of moves`

## 📦 module TTTests
---
- **fn** `TT can store and retrieve an entry`
- **fn** `Mate scores are adjusted correctly for ply`
- **fn** `TT handles collisions via replacement`
- **fn** `TT reduces node count in transpositions`
- **fn** `Search does not poison TT when cancelled`
- **fn** `TT Ageing replaces shallow new move over deep old move`

## 📦 module MoveOrderingTests
---
- **fn** `MVV-LVA prefers Pawn takes Queen over Queen takes Pawn`

## 📦 module InsufficientMaterialTests
---
- **fn** `KB vs KB same color is a draw`

## 📦 module UciParsingTests
---
- **fn** `Position startpos moves e2e4 results in correct board`
- **fn** `Go time target includes white increment when white is to move`
- **fn** `Go time target includes black increment when black is to move`

## 📦 module RepetitionTests
---
- **fn** `Negamax returns 0 immediately if current position is a repetition`
- **fn** `isRepetition correctly identifies hash in history`

## 📦 module FiftyMoveRuleTests
---
- **fn** `Negamax returns 0 if halfmove clock is 100`
- **fn** `Negamax returns 0 if halfmove clock reaches 100 during search`

