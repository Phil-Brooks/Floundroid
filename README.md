# Floundroid
[![.NET](https://img.shields.io/badge/.NET-10.0-blueviolet)](https://dotnet.microsoft.com/download)  
[![Language](https://img.shields.io/badge/Language-F%23-blue)](https://fsharp.org/)  
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)  
![Status](https://img.shields.io/badge/Status-Stage%203.4%20Complete-brightgreen)  
[![Latest Release](https://img.shields.io/github/v/release/Phil-Brooks/Floundroid)](https://github.com/Phil-Brooks/Floundroid/releases)

<p align="center">
  <img src="logo.png" alt="Floundroid Logo" width="300">
  <br />
  <i>"Functional. Robotic. Fishy."</i>
  <br />
  <strong>Floundroid</strong> is a chess engine built in F#, evolving from clean functional abstractions into a high‑performance bitboard machine.
</p>

---

## 🚀 Latest Stable Release: v0.3.4 (The Performance Update)
This release marks a major breakthrough in search efficiency and tactical awareness. 
By transitioning to a pseudo-legal search and implementing a professional move-ordering suite, Floundroid has gained over **300 Elo points** and achieved its first competitive wins.

### **Key Improvements in v0.3.4**
- **Move Ordering Suite**: Implemented **MVV-LVA**, **Killer Moves**, and **History Heuristics**, allowing the search to prune bad branches millions of times faster.
- **Pseudo-Legal Search**: Optimized the search core by deferring legality checks; boosted **NPS (Nodes Per Second)** by ~300% to a consistent **75k+**.
- **Bulletproof Time Management**: Implemented an "Alarm Clock" (`CancelAfter`) system that eliminates time forfeits, even in fast time controls.
- **TT Efficiency**: Reaches **Depth 7-9** in the opening and **Depth 12+** in endgames thanks to high Transposition Table hit rates.
- 👉 [**Download Floundroid.exe v0.3.4**](https://github.com/Phil-Brooks/Floundroid/releases/latest)

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

Floundroid is regularly benchmarked against TSCP 1.81 (1550 Elo).

### **v0.3.4 Performance Baseline**
*   **Final Rating:** `~1180 Elo` (+100 gain over v0.3.1)
*   **Record vs TSCP**: 1 Win, 18 Draws, 76 Losses (95 games played)
*   **Technical Performance:** 
    *   **Tactical Maturity**: Draw ratio improved to **18.9%**; recorded the engine's first victory against a 1500+ rated opponent.
    *   **Zero Stalls**: 100% UCI compliance confirmed; search is fully thread-safe and responsive.
*   **Status**: Tactically sharp at low depths. The next phase (Evaluation Overhaul) will focus on positional understanding and King safety to close the 300-point gap with TSCP.

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
**Status: 95% Complete**
- [x] **Bitboards**: 64-bit integer board representation.
- [x] **Zobrist Hashing**: Incremental position fingerprinting.
- [x] **Transposition Table**: Search result caching.
- [x] **Move Ordering**: MVV-LVA, Killers, and History heuristics.
- [ ] **Quiescence Polish**: (Next Up) Implementing Delta Pruning for even faster capture searches.

### **Stage 4 — Strength Phase** 🔵
**Status: Up Next**
- [ ] **Evaluation Overhaul**: Mobility, King Safety, and Pawn Structure.

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
