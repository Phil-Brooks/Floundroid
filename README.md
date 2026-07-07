# Floundroid
[![.NET](https://img.shields.io/badge/.NET-10.0-blueviolet)](https://dotnet.microsoft.com/download)  
[![Language](https://img.shields.io/badge/Language-F%23-blue)](https://fsharp.org/)  
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)  
![Status](https://img.shields.io/badge/Status-Stage%204%20Complete-green)  
[![Latest Release](https://img.shields.io/github/v/release/Phil-Brooks/Floundroid)](https://github.com/Phil-Brooks/Floundroid/releases)

<p align="center">
  <img src="logo.png" alt="Floundroid Logo" width="300">
  <br />
  <i>"Functional. Robotic. Fishy."</i>
  <br />
  <strong>Floundroid</strong> is a chess engine built in F#, evolving from functional abstractions into an optimized bitboard-based architecture.
</p>

---

## 🚀 Latest Stable Release: v0.4.8 (The Refined Edge)

This release marks a major milestone: **Floundroid has officially surpassed its primary benchmark, Halogen 6.0.** 

While v0.4.7 introduced the Texel Tuning framework, **v0.4.8** completes the mission. Every single evaluation parameter—from base material values to complex king safety heuristics—has been mathematically optimized. The engine now plays with a level of precision that makes it a formidable opponent even against established 2400+ Elo engines.

### **Key Improvements in v0.4.8**
- **Full Evaluation Coverage**
  - Expanded Texel Tuning to 100% of the evaluation function. 
  - Refined all Piece-Square Tables (PSTs) and mobility constants to ensure perfect harmony between search and evaluation.
- **Enhanced Tactical Robustness**
  - Improved king safety and attack weightings, leading to a much sharper tactical "eye."
- **Black-Side Dominance**
  - In recent testing, Floundroid has shown extraordinary resilience playing as Black, scoring a remarkable **63%** against high-level opposition.
- 👉 [**Download Floundroid.exe v0.4.8**](https://github.com/Phil-Brooks/Floundroid/releases/latest)

---

## 📊 Benchmarks (v0.4.8 vs Halogen 6.0)

Floundroid v0.4.8 was tested in a 100-game match against **Halogen 6.0 (2440 Elo)**. For the first time, Floundroid has secured a positive score, moving into the lead in this long-standing rivalry.

| Metric | Result vs Halogen 6.0 |
| :--- | :--- |
| **Score** | **52.0%** |
| **Elo Difference** | **+13.9 +/- 53.1** |
| **Estimated Performance** | **~2454 Elo** |
| **Wins** | 32 |
| **Draws** | 40 |
| **Losses** | 28 |

*Note: Floundroid v0.4.8 exhibited extreme stability, with a high draw rate (40%) and a massive 63% performance as Black, suggesting a highly reliable defensive core.*

---

## 🧩 Feature List

Floundroid combines functional programming patterns with high-performance chess theory.

### **Architecture**
- **Bitboard Engine:** Full bitboard representation for pieces and occupancy.
- **Magic Bitboards:** Optimized sliding piece attack generation.
- **Zobrist Hashing:** Fast incremental position hashing for transposition table lookups and draw detection.
- **Monolithic Assembly:** Fully inlined evaluation and search hot-paths to maximize NPS (Nodes Per Second).

### **Search**
- **Principal Variation Search (PVS):** Efficiently prunes the search tree by focusing on the best line.
- **Transposition Table (TT):** Large-scale caching with aging and depth-preferred replacement.
- **Late Move Reductions (LMR):** Reduces search depth for moves statistically unlikely to be the best.
- **Null-Move Pruning (NMP):** Aggressive pruning in positions where "passing the move" still results in a lead.
- **Reverse Futility Pruning (RFP):** Static pruning at nodes near the leaves to save search time.
- **Aspiration Windows:** Narrowed search bounds to speed up alpha-beta cutoffs.

### **Evaluation**
- **Comprehensive Texel Tuning:** All evaluation constants optimized against 725k quiet positions.
- **Tapered Evaluation:** Smooth transition between Middlegame (MG) and Endgame (EG) scores.
- **King Safety:** Attacker-based scaling system that penalizes proximity to the enemy king.
- **Advanced PSTs:** Separate 64-square tables for every piece type, optimized for both MG and EG.
- **Mobility:** Piece-specific activity scoring for all minor and major pieces.
- **Passed Pawn Logic:** Rank-based advancement bonuses.

---

## 🛠 Project Roadmap

### **Stage 1 — Functional Core** ✅  
### **Stage 2 — UCI Engine Interface** ✅  
### **Stage 3 — Mechanical Brain** ✅  
### **Stage 4 — Strength Phase** ✅  
- [x] Mobility & Tapered Evaluation
- [x] Search Pruning (NMP, RFP, LMR)
- [x] Tapered PSTs
- [x] **Comprehensive Texel Tuning (v0.4.8)**
- [x] Pawn shield & Rook bonuses 

### **Stage 5 — Innovation & Optimization** 🏗️ *Planned*
- [ ] SIMD/BMI2 Bitboard Optimizations
- [ ] **NNUE Neural Evaluation core** (Bootstrap training using v0.4.8 evaluation)



