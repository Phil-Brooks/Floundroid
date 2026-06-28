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

## 🚀 Latest Stable Release: v0.4.4 (The Search Sophistication Update)

This release marks a major milestone: **Floundroid has officially surpassed Cinnamon 2.0 in playing strength.** By implementing a "Big Five" search architecture, Floundroid now searches significantly deeper and more efficiently, allowing its positional evaluation to shine.

### **Key Improvements in v0.4.4**
- **Aspiration Windows**
  - Implemented stable, wide-window re-searches (±60cp) at higher depths.
  - Reduced search time by narrowing the alpha-beta bounds based on previous depth results.
- **Reverse Futility Pruning (RFP)**
  - Also known as Static Null Move Pruning; allows the engine to "fail high" early if the static evaluation is significantly above beta.
- **Search Stack Refinement**
  - Fine-tuned **Late Move Reductions (LMR)** and **Principal Variation Search (PVS)** for better tactical stability.
  - Improved Move Ordering using a combination of MVV-LVA, Killer Moves, and History Heuristics.
- 👉 [**Download Floundroid.exe v0.4.4**](https://github.com/Phil-Brooks/Floundroid/releases/latest)

---

## 📊 Calibration & Rating (v0.4.4 vs Cinnamon 2.0)

Floundroid v0.4.4 was tested against **Cinnamon 2.0** over 100 games at `tc=30`.

### **Performance Comparison**

| Metric | v0.4.3 (Previous) | **v0.4.4 (Latest)** | Change |
| :--- | :--- | :--- | :--- |
| **Score vs Cinnamon** | 27.0% | **52.0%** | **+25.0%** |
| **Elo Difference** | –172 | **+14** | **+186 Elo** |
| **Wins** | 20 | **45** | **+25 Wins** |
| **Draw Ratio** | 14% | **14%** | **0%** |

### **Interpretation**
The **+186 Elo increase** is a direct result of search efficiency. By reaching higher depths in the same amount of time, the engine identifies tactical threats and positional advantages much earlier. Surpassing Cinnamon 2.0 establishes Floundroid as a competitive amateur-level engine.

---

## 🛠 Project Roadmap

### **Stage 1 — Functional Core** ✅  
### **Stage 2 — UCI Engine Interface** ✅  
### **Stage 3 — Mechanical Brain** ✅  

### **Stage 4 — Strength Phase** 🏗️ *In Progress*  
- [x] Mobility & Tapered Evaluation (v0.4.3)
- [x] **Reverse Futility Pruning** (v0.4.4)
- [x] **Aspiration Windows** (v0.4.4)
- [x] **Tapered PSTs** (Middlegame vs Endgame tables)
- [x] Passed pawn advancement bonuses  
- [ ] **Strength Tuning (SPSA/Texel)** 🔵 *Next Priority*
- [ ] Pawn shield & King safety refinement 
- [ ] Rook bonuses (Open/Half-open files)

### **Stage 5 — Innovation & Optimization** ⚪ *Planned*
- [ ] SIMD/BMI2 Bitboard Optimizations
- [ ] Singular Extensions
- [ ] NNUE Neural Evaluation core