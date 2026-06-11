# Floundroid Code Structure

Generated on: 11/06/2026 12:36:51

## 📦 module Floundroid
---
- **Type:** `type Colour =`

## 📦 module Colour =
---
- **Type:** `type File =`

## 📦 module File =
---
  - `fn` toChar
- **Type:** `type Rank =`

## 📦 module Rank =
---
  - `fn` toChar
- **Type:** `type Square = int`

## 📦 module Square =
---
  - `fn` ofFileRank
  - `fn` file
  - `fn` rank
  - `fn` toString
  - `fn` fromString
  - `fn` isOnBoard
- **Type:** `type PieceType =`

## 📦 module PieceType =
---
- **Type:** `type Piece = { Colour: Colour; Kind: PieceType }`

## 📦 module Piece =
---
  - `fn` toChar
  - `fn` c
- **Type:** `type CastlingRights =`

## 📦 module CastlingRights =
---
  - `fn` fromString
  - `fn` sb
- **Type:** `type MoveKind =`
- **Type:** `type Move =`

## 📦 module Move =
---
  - `fn` toUci
  - `fn` baseStr
  - `fn` fromUci
  - `fn` fromSq
  - `fn` toSq
- **Type:** `type Board =`

## 📦 module Attack =
---
  - `fn` isSquareAttacked
  - `fn` f,
  - `fn` pawnDir
  - `fn` mutable
  - `fn` nf,
  - `fn` s2
  - `fn` nf,
  - `fn` s2
  - `fn` mutable
  - `fn` mutable
  - `fn` s2
  - `fn` nf,
  - `fn` s2

## 📦 module Board =
---
  - `fn` tryGetPiece
  - `fn` isOccupied
  - `fn` setPiece
  - `fn` fromFen
  - `fn` parts
  - `fn` rows
  - `fn` mutable
  - `fn` rank,
  - `fn` sq
  - `fn` toFen
  - `fn` sb
  - `fn` mutable
  - `fn` sq
  - `fn` isInCheckFor
  - `fn` isInCheck
  - `fn` getPins
  - `fn` us,
  - `fn` kf,
  - `fn` pins
  - `fn` mutable
  - `fn` sq
  - `fn` mutable
  - `fn` rsq
  - `fn` applyMove
  - `fn` us,
  - `fn` mutable
  - `fn` rR
  - `fn` rF,
  - `fn` rk
  - `fn` rR
  - `fn` rF,
  - `fn` rk
  - `fn` updateRights
  - `fn` revokeForSquare
  - `fn` prettyPrint

## 📦 module MoveGen =
---
  - `fn` getPseudoLegalMoves
  - `fn` moves
  - `fn` us,
  - `fn` f,
  - `fn` d
  - `fn` p1
  - `fn` p2
  - `fn` nf,
  - `fn` cap
  - `fn` nf,
  - `fn` t
  - `fn` rnk,
  - `fn` mutable
  - `fn` t
  - `fn` getLegalMoves
  - `fn` us,
  - `fn` rnk
  - `fn` midFile
  - `fn` destFile
  - `fn` midSquare
  - `fn` destSquare

## 📦 module San =
---
  - `fn` toSan
  - `fn` piece
  - `fn` nextBoard
  - `fn` isCheck
  - `fn` isMate
  - `fn` pChar
  - `fn` altPiece
  - `fn` cap
- **Type:** `type PerftSuiteItem =`

## 📦 module Perft =
---
  - `fn` rec
  - `fn` moves
  - `fn` mutable
  - `fn` divide
  - `fn` sw
  - `fn` moves
  - `fn` mutable
  - `fn` n
  - `fn` ms
  - `fn` nps
  - `fn` runFullSuite
  - `fn` totalSw
  - `fn` b
  - `fn` depthsToTest
  - `fn` expected
  - `fn` sw
  - `fn` actual

## 📦 module Debug =
---
  - `fn` displayMoves
  - `fn` moves
  - `fn` verify
  - `fn` errors
  - `fn` pieces
  - `fn` r
  - `fn` displayAttackMap
  - `fn` sq

## 📦 module UciLoop =
---
  - `fn` startFen
  - `fn` mutable
  - `fn` line
  - `fn` ts
  - `fn` (fen,
  - `fn` movesIdx
  - `fn` f
  - `fn` m
  - `fn` legalMoves
  - `fn` moves
