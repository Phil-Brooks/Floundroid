# Floundroid
[![.NET](https://img.shields.io/badge/.NET-10.0-blueviolet)](https://dotnet.microsoft.com/download)  
[![Language](https://img.shields.io/badge/Language-F%23-blue)](https://fsharp.org/)  
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)  
![Status](https://img.shields.io/badge/Status-Stage%204%20In%20Progress-blue)  
[![Latest Release](https://img.shields.io/github/v/release/Phil-Brooks/Floundroid)](https://github.com/Phil-Brooks/Floundroid/releases)

<p align="center">
  <img src="logo.png" alt="Floundroid Logo" width="300">
  <br />
  <i>"Functional. Robotic. Fishy."</i>
  <br />
  <strong>Floundroid</strong> is a chess engine built in F#, evolving from clean functional abstractions into a high‑performance bitboard machine.
</p>

---

## 🚀 Latest Stable Release: v0.4.1 (Pawn Structure Evaluation)
This release introduces the first major components of the Stage 4 Strength Phase, focusing on positional understanding through pawn structure analysis.

### **Key Features in v0.4.1**
- **Pawn Structure Evaluation**: The engine now recognizes and scores structural weaknesses and strengths, including:
  - **Passed Pawns**: Encourages pushing pawns that have no opposition.
  - **Isolated Pawns**: Penalizes pawns with no friendly neighbors.
  - **Doubled Pawns**: Penalizes vertically stacked pawns that hinder mobility.
- **Null-Move Pruning (NMP)**: Skip-search logic for high-beta cutoffs.
- **Adaptive Depth Reduction**: Dynamic $R$ values based on search depth.
- **UCI Increment Handling**: Improved time management for blitz/bullet with increments.
- 👉 [**Download Floundroid.exe v0.4.1**](https://github.com/Phil-Brooks/Floundroid/releases/latest)

---

## 📊 Calibration & Rating

Floundroid is regularly benchmarked against **TSCP 1.81** (est. 1550 Elo). 

### **v0.4.1 Performance Summary**
As of v0.4.1, Floundroid has surpassed its baseline target (TSCP), maintaining a consistent winning record across multi-match sessions.

*   **Final Rating:** `~1630 Elo` (+80 relative to TSCP) — a massive **+172 Elo gain** over v0.4.0a.
*   **Combined Record vs TSCP**: 107 Wins, 31 Draws, 62 Losses (200 games played)
*   **Aggregated Score**: 61.25%
*   **Technical Performance:** 
    *   **Evaluation Impact**: The addition of pawn structure logic has significantly improved the "Black" side performance, reaching as high as a 77% win rate in testing.
    *   **Positional Stability**: Drastically reduced the frequency of tactical blunders caused by poor late-game structure.
*   **Status**: Stage 4 (Strength Phase) is well underway. 

---

## 🛠 Project Roadmap

### **Stage 1 — Functional Core** ✅
- Strong F# domain modelling and Perft validation.

### **Stage 2 — UCI Engine Interface** ✅
- Async search architecture and iterative deepening.

### **Stage 3 — Mechanical Brain** ✅
- [x] **Bitboards / Magic Bitboards**: Optimized sliding piece logic.
- [x] **Zobrist Hashing**: Incremental position fingerprinting.
- [x] **Transposition Table**: Advanced caching and entry ageing.
- [x] **Move Ordering**: Heuristics (Killers, History, MVV-LVA).
- [x] **Draw Detection**: 3-fold repetition, 50-move rule, Insufficient Material.

### **Stage 4 — Strength Phase** 🔵
**Status: In Progress**
- [x] **Evaluation Overhaul**: Pawn Structure (Passed, Isolated, Doubled).
- [ ] **Evaluation Overhaul**: Mobility and King Safety.
- [x] **Search Pruning**: Null-move pruning.
- [ ] **Search Pruning**: LMR (Late Move Reductions) and Aspiration Windows.
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