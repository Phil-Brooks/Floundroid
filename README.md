# Floundroid

[![Build Status](https://img.shields.io/badge/.NET-10.0-blueviolet)](https://dotnet.microsoft.com/download)
[![Language](https://img.shields.io/badge/Language-F%23-blue)](https://fsharp.org/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

<p align="center">
  <img src="logo.png" alt="Floundroid Logo" width="300">
  <br />
  <i>"Functional. Robotic. Fishy."</i>
  <br />
  <strong>Floundroid</strong> is a chess engine built in F#, evolving from clean functional abstractions into a high‑performance bitboard machine.
</p>

---

## 🌊 Why Floundroid?

Floundroid sits at the intersection of the natural (Flounder/Stockfish heritage) and the synthetic (Android/functional logic).  
The goal is simple:

> **Make impossible chess states unrepresentable, then make the engine fast enough to matter.**

F#’s type system and functional purity make it ideal for correctness-first development, while .NET’s performance primitives allow a later transition to bitboards and low‑level optimisation.

---

## 🛠 Project Roadmap

Floundroid is built in **five deliberate stages**, moving from correctness → playability → performance → strength → innovation.

---

### **Stage 1 — Functional Core (In Progress) 🟢**

Focus: **Correctness, purity, and domain modelling**

- Strong types for pieces, squares, moves, and board state
- Pure functional move generation (pseudo‑legal + legal)
- FEN parsing and board visualisation
- Full Perft test suite for correctness
- Internal debugging tools

---

### **Stage 2 — UCI Engine Interface 🟡**

Focus: **Playability**

- Full UCI protocol implementation
- Async search loop using F# `async`
- Basic alpha‑beta search
- Simple evaluation (material + PSTs)
- Integration with Arena, CuteChess, Banksia

---

### **Stage 3 — Mechanical Brain 🔴**

Focus: **Performance**

- Bitboard representation (12 piece boards + occupancy)
- Magic bitboards or lookup tables for sliding pieces
- Zobrist hashing
- Transposition table
- Move ordering (TT move, killers, history)
- Quiescence search

---

### **Stage 4 — Strength Phase 🔵**

Focus: **Modern engine heuristics**

- Null‑move pruning
- Late Move Reductions (LMR)
- Aspiration windows
- Tapered evaluation (MG/EG blend)
- King safety, pawn structure, mobility

---

### **Stage 5 — Innovation & F#‑Specific Optimisation 🟣**

Focus: **Identity**

- Hybrid functional/imperative core
- Struct‑based hot paths
- SIMD/HW intrinsics for bitboard ops
- Optional NNUE‑style evaluation
- Experimental search techniques

---

## 🚀 Getting Started

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)

### Build & Run

```bash
git clone https://github.com/Phil-Brooks/Floundroid.git
cd Floundroid
dotnet build
dotnet run
