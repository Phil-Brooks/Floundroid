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

## 🚀 Latest Stable Release: v0.3.7 (The "Draw Detection" Update)
This release focuses on **Search Correctness** and rules compliance, ensuring the engine recognizes draws precisely as defined in FIDE rules.

### **Key Features in v0.3.7**
- **50-Move Rule**: The engine now correctly identifies draws when the half-move clock reaches 100, preventing infinite loops in quiet endgames.
- **Insufficient Material Detection**: Implemented logic to recognize dead-draw positions (K vs K, KN vs K, KB vs K, and same-colored bishop endgames), saving search time in non-winnable scenarios.
- **Improved Tenacity**: The engine's defensive profile is significantly enhanced by its ability to actively seek draws in losing positions using the new rules.
- 👉 [**Download Floundroid.exe v0.3.7**](https://github.com/Phil-Brooks/Floundroid/releases/latest)

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
- **Draw Detection**: 3-fold repetition, 50-move rule, and Insufficient Material.

### **UCI Support**
- Full UCI protocol compliance.
- **Async search loop**: Remains responsive to GUI commands (`stop`, `quit`) even during deep calculations.

---

## 📊 Calibration & Rating

Floundroid is regularly benchmarked against **TSCP 1.81** (est. 1550 Elo).

### **v0.3.7 Performance Baseline**
*   **Final Rating:** `~1242 Elo` (-308 relative to TSCP)
*   **Record vs TSCP**: 7 Wins, 15 Draws, 78 Losses (100 games played)
*   **Technical Performance:** 
    *   **Draw Detection in Action**: Of the 15 draws achieved, **7 were via the 50-move rule** and **5 were via insufficient material**, directly validating the new implementation.
    *   **Asymmetric Performance**: Achieved a 25.0% score as Black, significantly outperforming White (4.0%), likely due to more robust defensive responses to aggressive opening play.
*   **Status**: Stage 3 (Performance & Correctness) is officially closed. The engine is mechanically "complete"; future gains will come from evaluation tuning (Stage 4).

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
```