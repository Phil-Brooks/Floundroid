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

## 🚀 Latest Stable Release: v0.4.5 (The Performance Refinement Update)

This release focuses on "under-the-hood" efficiency. By utilizing **BenchmarkDotNet** to identify bottlenecks in the move generator and search hot-paths, Floundroid v0.4.5 achieves significantly higher nodes-per-second (NPS). This increased throughput allows the engine to reach deeper plies within the same time constraints, resulting in a dominant performance increase over Cinnamon 2.0.

### **Key Improvements in v0.4.5**
- **Micro-Optimizations via BenchmarkDotNet**
  - Refactored bitboard lookups and move-generation loops for better CPU cache locality.
  - Reduced allocations in the search stack, lowering GC pressure during high-depth calculations.
- **Improved Time Management**
  - Refined the "move-overhead" logic to prevent losses on time (though still a work in progress).
- **Search Stability**
  - The performance gains have allowed for more stable results in the "Big Five" search architecture (Aspiration Windows, RFP, LMR, PVS, and Move Ordering).
- 👉 [**Download Floundroid.exe v0.4.5**](https://github.com/Phil-Brooks/Floundroid/releases/latest)

---

## 📊 Calibration & Rating (v0.4.5 vs Cinnamon 2.0)

Floundroid v0.4.5 was tested against **Cinnamon 2.0** over 100 games at `tc=30`.

### **Performance Comparison**

| Metric | v0.4.4 (Previous) | **v0.4.5 (Latest)** | Change |
| :--- | :--- | :--- | :--- |
| **Score vs Cinnamon** | 52.0% | **66.5%** | **+14.5%** |
| **Elo Difference** | +14 | **+119** | **+105 Elo** |
| **Wins** | 45 | **58** | **+13 Wins** |
| **Draw Ratio** | 14% | **17%** | **+3%** |

### **Interpretation**
The **+105 Elo increase** over the previous version demonstrates the power of code optimization. Without changing the underlying evaluation terms, Floundroid is now significantly stronger simply by seeing further. With an 80% win rate as White, the engine is becoming a formidable tactical opponent.

---

## 🛠 Project Roadmap

### **Stage 1 — Functional Core** ✅  
### **Stage 2 — UCI Engine Interface** ✅  
### **Stage 3 — Mechanical Brain** ✅  

### **Stage 4 — Strength Phase** 🏗️ *In Progress*  
- [x] Mobility & Tapered Evaluation (v0.4.3)
- [x] Reverse Futility Pruning (v0.4.4)
- [x] Aspiration Windows (v0.4.4)
- [x] **BenchmarkDotNet Performance Tuning** (v0.4.5)
- [x] Tapered PSTs (Middlegame vs Endgame tables)
- [x] Passed pawn advancement bonuses  
- [ ] **Strength Tuning (SPSA/Texel)** 🔵 *Next Priority*
- [ ] Pawn shield & King safety refinement 
- [ ] Rook bonuses (Open/Half-open files)

### **Stage 5 — Innovation & Optimization** ⚪ *Planned*
- [ ] SIMD/BMI2 Bitboard Optimizations
- [ ] Singular Extensions
- [ ] NNUE Neural Evaluation core