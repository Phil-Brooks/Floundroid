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
  <strong>Floundroid</strong> is a chess engine built in F#, evolving from functional abstractions into an optimized bitboard-based architecture.
</p>

---

## 🚀 Latest Stable Release: v0.4.6 (The High-Stakes Calibration)

This release marks a significant milestone in Floundroid's development. After the performance gains of v0.4.5, we have moved away from testing against entry-level engines and begun benchmarking against **Halogen 6.0** (est. 2440 Elo). 

By identifying further bottlenecks in the search and evaluation hot-paths, v0.4.6 hardens the engine for "Master-level" play.

### **Key Improvements in v0.4.6**
- **Deep-Trace Benchmarking**
  - Identified and resolved secondary bottlenecks in the bitboard attack generators.
  - Optimized the Search Stack to better handle the tactical pressure of 2400+ rated opponents.
- **Improved UCI Stability**
  - Enhanced move-validation logic, ensuring robust performance even when opponents encounter internal errors.
- **Engine Hardening**
  - While still trailing a 2440-rated heavyweight, Floundroid has established a "floor" rating in the 2200+ range, providing a high-quality baseline for upcoming evaluation tuning.
- 👉 [**Download Floundroid.exe v0.4.6**](https://github.com/Phil-Brooks/Floundroid/releases/latest)

---

## 📊 Calibration & Rating (v0.4.6 vs Halogen 6.0)

To test the limits of our recent optimizations, Floundroid v0.4.6 was put through a 100-game gauntlet against **Halogen 6.0 (2440 Elo)** at `tc=30`.

### **Performance Comparison**

| Metric | Result vs Halogen 6.0 |
| :--- | :--- |
| **Score** | 24.0% |
| **Elo Difference** | -200.2 +/- 72.2 |
| **Estimated Performance** | **~2240 Elo** |
| **Wins** | 16 |
| **Draws** | 16 |
| **Losses** | 68 |

### **Interpretation**
Testing against a 2440-rated opponent is a "stress test." Floundroid v0.4.6 successfully held its own in nearly 1/3 of the games (32% non-loss rate). These results highlight that while our **search speed** (NPS) is now competitive, the next leap in strength must come from **Evaluation Tuning**—specifically refining how the engine perceives King Safety and Pawn Structures to close the 200-point gap.

---

## 🛠 Project Roadmap

### **Stage 1 — Functional Core** ✅  
### **Stage 2 — UCI Engine Interface** ✅  
### **Stage 3 — Mechanical Brain** ✅  

### **Stage 4 — Strength Phase** 🏗️ *In Progress*  
- [x] Mobility & Tapered Evaluation (v0.4.3)
- [x] Reverse Futility Pruning (v0.4.4)
- [x] Aspiration Windows (v0.4.4)
- [x] BenchmarkDotNet Performance Tuning (v0.4.5)
- [x] **Further Benchmarking Fixes (v0.4.6)** 
- [x] Tapered PSTs (Middlegame vs Endgame tables)
- [x] Passed pawn advancement bonuses  
- [ ] **Strength Tuning (SPSA/Texel)** 🔵 *Next Priority*
- [ ] Pawn shield & King safety refinement 
- [ ] Rook bonuses (Open/Half-open files)

### **Stage 5 — Innovation & Optimization** ⚪ *Planned*
- [ ] SIMD/BMI2 Bitboard Optimizations
- [ ] Singular Extensions
- [ ] NNUE Neural Evaluation core