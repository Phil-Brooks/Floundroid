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

## 🚀 Latest Stable Release: v0.4.0 (The "Null-Move Pruning" Update)
This release initiates the **Strength Phase (Stage 4)** by introducing Null-Move Pruning (NMP), allowing the engine to search deeper and more efficiently.

### **Key Features in v0.4.0**
- **Null-Move Pruning (NMP)**: The engine now skips searching moves when the evaluation is so favorable that passing the turn still results in a beta cutoff.
- **Adaptive Depth Reduction**: Dynamically reduces search depth (\(R = 3\) for depth > 6, \(R = 2\) otherwise) to maximize search speed while minimizing tactical blind spots.
- **Safety Heuristics**: Restricts NMP when in check or when the side-to-move has no non-pawn material (avoiding zugzwang pitfalls).
- 👉 [**Download Floundroid.exe v0.4.0**](https://github.com/Phil-Brooks/Floundroid/releases/latest)

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
- **Null-Move Pruning (NMP)** with adaptive depth reduction.
- **Bitboard representation** using **Magic Bitboards** for sliding pieces.
- **Transposition Table** with Zobrist hashing and aging logic.
- **Heuristics**: MVV-LVA, Killers, and History moves.
- **Draw Detection**: 3-fold repetition, 50-move rule, and Insufficient Material.

### **UCI Support**
- Full UCI protocol compliance.
- **Async search loop**: Remains responsive to GUI commands (`stop`, `quit`) even during deep calculations.

---

## 📊 Calibration & Rating

Floundroid is regularly benchmarked against **TSCP 1.81** (est. 1550 Elo).

### **v0.4.0 Performance Baseline**
*   **Final Rating:** `~1359 Elo` (-191 relative to TSCP) — a massive **+117 Elo increase** over v0.3.7!
*   **Record vs TSCP**: 19 Wins, 12 Draws, 69 Losses (100 games played)
*   **Technical Performance:** 
    *   **Null-Move Pruning Impact**: Enabled deeper searches and higher tactical accuracy, leading to a much higher win rate (19% vs 7%).
    *   **Asymmetric Performance**: Achieved a 41.0% score as Black (17 wins, 7 draws, 26 losses), significantly outperforming White at 9.0% (2 wins, 5 draws, 43 losses), indicating strong defensive counter-attacking play.
*   **Status**: Stage 4 (Strength Phase) is in progress. Null-Move Pruning is fully implemented and verified.

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
- [x] **Draw Detection**: 3-fold repetition, 50-move rule, Insufficient Material.

### **Stage 4 — Strength Phase** 🔵
**Status: In Progress**
- [ ] **Evaluation Overhaul**: Mobility, King Safety, and Pawn Structure.
- [ ] **Search Pruning**: Null-move pruning [x], LMR [ ], Aspiration Windows [ ].
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
```