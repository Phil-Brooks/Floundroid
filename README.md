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

## 🚀 Latest Stable Release: v0.4.7 (The Texel Enlightenment)

This release represents the single largest jump in Floundroid's playing strength to date. By implementing a custom **Texel Tuning** framework, we have replaced "human-guessed" evaluation constants with mathematically optimized values derived from over 725,000 professional game positions.

The result is an engine that no longer just "searches fast," but truly "understands" positional nuances, king safety, and endgame transitions.

### **Key Improvements in v0.4.7**
- **Data-Driven Evaluation (Texel Tuning)**
  - Optimized 402 parameters including Material weights, Piece-Square Tables (PSTs), and Mobility.
  - Calibrated the **K**-factor to 1.39, perfectly grounding the engine's internal scoring scale.
- **Overhauled King Safety**
  - King Attack weights were refined (with some values increasing by 10x), making the engine significantly more lethal in attacking configurations.
- **Tapered Precision**
  - Fine-tuned the interpolation between Middlegame and Endgame, resulting in much stronger conversion of winning advantages.
- 👉 [**Download Floundroid.exe v0.4.7**](https://github.com/Phil-Brooks/Floundroid/releases/latest)

---

## 📊 Benchmarks (v0.4.7 vs Halogen 6.0)

Floundroid v0.4.7 was tested in a 100-game match against **Halogen 6.0 (2440 Elo)**. The tuning has effectively closed the gap, bringing Floundroid within striking distance of a 2450+ rating.

| Metric | Result vs Halogen 6.0 |
| :--- | :--- |
| **Score** | 47.0% |
| **Elo Difference** | **-20.9 +/- 58.3** |
| **Estimated Performance** | **~2420 Elo** |
| **Wins** | 33 |
| **Draws** | 28 |
| **Losses** | 39 |

*Note: Floundroid showed particular strength as White, scoring a 58% win rate, highlighting its newfound aggressive attacking style.*

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
- **Texel Tuned Weights:** All constants optimized against 725k quiet positions.
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
- [x] **Automated Texel Tuning (v0.4.7)**
- [x] Pawn shield & Rook bonuses 

### **Stage 5 — Innovation & Optimization** 🏗️ *Planned*
- [ ] SIMD/BMI2 Bitboard Optimizations
- [ ] **NNUE Neural Evaluation core** (Bootstrap training using v0.4.7 evaluation)
