# Floundroid
[![.NET](https://img.shields.io/badge/.NET-10.0-blueviolet)](https://dotnet.microsoft.com/download)  
[![Language](https://img.shields.io/badge/Language-F%23-blue)](https://fsharp.org/)  
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)  
![Status](https://img.shields.io/badge/Status-Stage%203%20Complete-brightgreen)  
[![Latest Release](https://img.shields.io/github/v/release/Phil-Brooks/Floundroid)](https://github.com/Phil-Brooks/Floundroid/releases)

<p align="center">
  <img src="logo.png" alt="Floundroid Logo" width="300">
  <br />
  <i>"Functional. Robotic. Fishy."</i>
  <br />
  <strong>Floundroid</strong> is a chess engine built in F#, evolving from clean functional abstractions into a high‑performance bitboard machine.
</p>

---

## 🚀 Latest Stable Release: v0.3.6 (The "Mechanical Brain" Update)
This release marks the **100% completion of Stage 3**. The engine has transitioned fully to a high-performance bitboard architecture with professional-grade search heuristics.

### **Key Features in v0.3.6**
- **Magic Bitboards**: Implemented high-speed bitwise sliding attack generation, significantly increasing nodes-per-second (NPS).
- **Transposition Table (TT) Maturity**: Added **TT Ageing** and optimized probing, allowing the engine to retain critical search data across turns without clogging memory.
- **Advanced Move Ordering**: Integrated **MVV-LVA**, **Killer Moves**, and **History Heuristics** to prune the search tree more aggressively.
- **Enhanced Tenacity**: The engine is now significantly harder to "topple," evidenced by a nearly 50% increase in draw rate against higher-rated opposition.
- 👉 [**Download Floundroid.exe v0.3.6**](https://github.com/Phil-Brooks/Floundroid/releases/latest)

---

## 🌊 Why “Floundroid”?
Floundroid reflects the hybrid nature of the project: a traditional handcrafted chess engine built side-by-side with AI assistance. The *droid* suffix acknowledges the collaborative role of tools like Copilot in refining move generation and debugging complex bitwise logic.

---

## 🧠 Philosophy
> **Make impossible chess states unrepresentable, then make the engine fast enough to matter.**

---

## ⭐ Features

### **Search & Performance**
- **Alpha-beta search** with Iterative Deepening and Quiescence search.
- **Bitboard representation** using **Magic Bitboards** for sliding pieces.
- **Transposition Table** with Zobrist hashing and aging logic.
- **Heuristics**: MVV-LVA, Killers, and History moves.

### **UCI Support**
- Full UCI protocol compliance.
- **Async search loop**: Remains responsive to GUI commands (`stop`, `quit`) even during deep calculations.

---

## 📊 Calibration & Rating

Floundroid is regularly benchmarked against **TSCP 1.81** (est. 1550 Elo).

### **v0.3.6 Performance Baseline**
*   **Final Rating:** `~1249 Elo` (-301 relative to TSCP)
*   **Record vs TSCP**: 1 Win, 28 Draws, 71 Losses (100 games played)
*   **Technical Performance:** 
    *   **Resilience**: The **Draw Ratio** jumped to **28.0%** (up from 19.0% in v0.3.4), showing massive improvements in defensive stability.
    *   **Playing as Black**: Achieved a 40% draw rate as the second player, proving the search architecture's robustness.
*   **Status**: Stage 3 is officially closed. The engine is mechanically "complete"; future gains will come from evaluation tuning (Stage 4).

---

## 🛠 Project Roadmap

### **Stage 1 — Functional Core** ✅
- Strong F# domain modelling and Perft validation.

### **Stage 2 — UCI Engine Interface** ✅
- Async search architecture and iterative deepening.

### **Stage 3 — Mechanical Brain** ✅
- [x] **Bitboards**: Full 64-bit integer representation.
- [x] **Magic Bitboards**: Optimized sliding piece logic.
- [x] **Zobrist Hashing**: Incremental position fingerprinting.
- [x] **Transposition Table**: Advanced caching and entry ageing.
- [x] **Move Ordering**: Heuristics (Killers, History, MVV-LVA).

### **Stage 4 — Strength Phase** 🔵
**Status: In Progress**
- [ ] **Evaluation Overhaul**: Mobility, King Safety, and Pawn Structure.
- [ ] **Search Pruning**: Null-move pruning, LMR, and Aspiration Windows.
- [ ] **Tapered Eval**: Smooth transitions between Middle-game and End-game.

### **Stage 5 — Innovation** 🟣
- [ ] SIMD/Hardware Intrinsics.
- [ ] Optional NNUE evaluation.

---

## 🚀 Getting Started

### **Prerequisites**
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)

### **Installation**
```bash
git clone https://github.com/Phil-Brooks/Floundroid.git
cd Floundroid
dotnet build -c Release