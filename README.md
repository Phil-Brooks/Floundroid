# Floundroid
[![.NET](https://img.shields.io/badge/.NET-10.0-blueviolet)](https://dotnet.microsoft.com/download)  
[![Language](https://img.shields.io/badge/Language-F%23-blue)](https://fsharp.org/)  
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)  
![Status](https://img.shields.io/badge/Status-Stage%203.3%20Complete-brightgreen)  
[![Latest Release](https://img.shields.io/github/v/release/Phil-Brooks/Floundroid)](https://github.com/Phil-Brooks/Floundroid/releases)

<p align="center">
  <img src="logo.png" alt="Floundroid Logo" width="300">
  <br />
  <i>"Functional. Robotic. Fishy."</i>
  <br />
  <strong>Floundroid</strong> is a chess engine built in F#, evolving from clean functional abstractions into a high‑performance bitboard machine.
</p>

---

## 🚀 Latest Stable Release: v0.3.3a (The Stability Hotfix)
This release resolves critical UCI protocol stalls and Transposition Table "poisoning" found in earlier builds. The engine is now 100% stable over long-duration matches.
The engine now features a fully integrated **Transposition Table** and **Zobrist Hashing**, allowing it to "remember" positions and avoid redundant calculations.

### **Key Improvements in v0.3.3a**
- **UCI Protocol Robustness**: Fixed background thread deadlocks; the engine now guarantees a `bestmove` response even under extreme time pressure.
- **Search Sanity**: Implemented a "Poison Guard" to prevent interrupted searches from corrupting the Transposition Table.
- **Time Management**: More aggressive iterative deepening (utilizing 80% of time budget).
- **Stability**: Verified 100% crash-free/stall-free over a 100-game match against TSCP.
- 👉 [**Download Floundroid.exe v0.3.3a**](https://github.com/Phil-Brooks/Floundroid/releases/latest)

---

## 🌊 Why “Floundroid”?

Floundroid reflects the hybrid nature of the project:  
a traditional handcrafted chess engine built **side‑by‑side with AI assistance**.

The *droid* suffix is a deliberate nod to the fact that **Copilot and other AI tools actively support the development process** — from scaffolding the architecture to refining move generation, debugging perft mismatches, and exploring search optimisations.

This is still a human‑driven engine, but it embraces AI as a collaborative tool rather than a replacement for engineering skill.

---

## 🧠 Philosophy

> **Make impossible chess states unrepresentable, then make the engine fast enough to matter.**

Floundroid begins with strong functional modelling, powered by multiple AI assistance models during development, then transitions into a highly optimised bitboard engine.  
F#'s type system ensures correctness; .NET's performance primitives ensure speed.

---

## ⭐ Features

### **AI Collaboration**
- Multi-model assistance for architecture design and code generation 
- Iterative rapid prototyping with AI collaboration tools 
- Cross-validation of implementation approaches through multi-AI feedback

### **Core Engine**
- Strong F# domain modelling  
- Fully typed board, move, and piece representations  
- FEN parsing and board visualisation  
- Deterministic, testable architecture  

### **Move Generation**
- Pseudo‑legal and legal move generation  
- Sliding and non‑sliding piece logic  
- Perft validation suite  

### **Search**
- Alpha‑beta search  
- Iterative deepening  
- Quiescence search  
- Move ordering (TT move, killers, history)  
- Transposition table  
- Zobrist hashing  

### **UCI Support**
- Full UCI protocol  
- Async search loop  
- Compatible with Arena, CuteChess, Banksia  

### **Performance**
- Bitboard representation  
- Magic bitboards or lookup tables  
- Struct‑based hot paths  
- SIMD/HW intrinsic‑friendly layout  

### **Future / Experimental**
- NNUE‑style evaluation  
- Tapered evaluation  
- LMR, null‑move pruning, aspiration windows  
- Opening book + endgame tablebase probing  

---

## 📊 Calibration & Rating

Floundroid is regularly benchmarked against TSCP 1.81 (1550 Elo) to track development progress.

### **v0.3.3a Stability Baseline**
*   **Final Rating:** `~1087 Elo`
*   **Record:** 0 Wins, 8 Draws, 92 Losses (vs TSCP 1.81)
*   **Technical Performance:** 
    *   **100% Stability**: Zero crashes, zero UCI stalls, and zero illegal moves over 100 consecutive games.
    *   **Search Depth**: Consistently reaches Depth 5-6 in midgame, and Depth 9+ in endgames thanks to TT hits.
    *   **Draw Ratio**: 8.0% (all via 3-fold repetition).
*   **Status**: Technically robust. The next phase (Move Ordering) is expected to solve the current "Time Forfeit" issues and significantly increase tactical strength.

---

## 🛠 Project Roadmap

Floundroid is built in **five deliberate stages**, moving from correctness → playability → performance → strength → innovation.

---

### **Stage 1 — Functional Core** ✅ 🟢
**Status: Complete**
- Strong types for pieces, squares, moves, and board state  
- Pure functional move generation (pseudo‑legal + legal)  
- FEN parsing and full Perft test suite validation 

### **Stage 2 — UCI Engine Interface** ✅ 🟢
**Status: Complete**
- Full UCI protocol implementation (`uci`, `isready`, `position`, `go`, `stop`)
- **Async Search Architecture**: Responsive command listener while thinking.
- **Iterative Deepening**: Progressive search depth.
- **Evaluation**: Material values + Piece-Square Tables (PST).

### **Stage 3 — Mechanical Brain** 🟡 
**Status: In Progress (Next Up)**
- **Bitboards**: Transitioning from Map-based board to 64-bit integer representations.
- **Zobrist Hashing**: Efficient position tracking.
- **Transposition Table**: Caching search results.
- **Move Ordering**: Killer moves and History heuristics.

---

### **Stage 4 — Strength Phase 🔵**

Focus: **Modern heuristics**

- Null‑move pruning  
- LMR  
- Aspiration windows  
- Tapered eval  
- King safety, pawn structure, mobility  

---

### **Stage 5 — Innovation & F#‑Specific Optimisation 🟣**

Focus: **Identity**

- Hybrid functional/imperative core  
- Struct‑based hot paths  
- SIMD intrinsics  
- Optional NNUE  
- Experimental search techniques  

---

## 🚀 Getting Started

### **Prerequisites**
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)

### **Installation**
You can download the standalone binary from the [Releases](https://github.com/Phil-Brooks/Floundroid/releases) page or build it from source:

```bash
git clone https://github.com/Phil-Brooks/Floundroid.git
cd Floundroid
dotnet build -c Release
