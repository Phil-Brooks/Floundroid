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

## 🚀 Latest Stable Release: v0.4.3 (The Bitboard Eval Update)

This release delivers a major increase in playing strength by transitioning the evaluation core to a high-performance bitboard architecture. By eliminating legacy lookups and optimizing for CPU instruction caches, Floundroid now searches more efficiently and demonstrates better positional awareness.

### **Key Improvements in v0.4.3**
- **Refactored Evaluation Core**
  - Implemented 12-layer bitboard iteration to eliminate `getPieceAt` lookups.
  - Optimized for Instruction Cache (I-Cache) efficiency by avoiding over-inlining.
- **Tapered Evaluation Framework**
  - Added Middlegame (MG) and Endgame (EG) scoring accumulators.
  - Smooth interpolation between game phases based on non-pawn material.
- **Mobility Evaluation**
  - Integrated hardware-accelerated `PopCount` for pseudo-legal move counting.
  - Pieces now seek active squares with higher board influence.
- **King Safety (Attacker Counting)**
  - Implemented offensive safety scaling, penalizing the king based on the number and weight of enemy pieces attacking the "King Zone."
- 👉 [**Download Floundroid.exe v0.5.0**](https://github.com/Phil-Brooks/Floundroid/releases/latest)

---

## 📊 Calibration & Rating (v0.4.3 vs Cinnamon 2.0)

Floundroid v0.4.3 was tested against **Cinnamon 2.0** over 100 games at `tc=30`.

### **Performance Comparison**

| Metric | v0.4.2 (Baseline) | **v0.4.3 (Latest)** | Change |
| :--- | :--- | :--- | :--- |
| **Score vs Cinnamon** | 11.5% | **27.0%** | **+15.5%** |
| **Elo Difference** | –354 | **–172** | **+182 Elo** |
| **Wins** | 8 | **20** | **+12 Wins** |
| **Draw Ratio** | 7% | **14%** | **+7%** |

### **Interpretation**
The **+182 Elo increase** is a direct result of improved evaluation speed and the introduction of piece mobility. The engine is now significantly more competitive, doubling both its win and draw rates against a established opponent.

---

## 🛠 Project Roadmap

### **Stage 1 — Functional Core** ✅  
### **Stage 2 — UCI Engine Interface** ✅  
### **Stage 3 — Mechanical Brain** ✅  

### **Stage 4 — Strength Phase** 🔵 *In Progress*  
- [x] Pawn structure evaluation  
- [x] Null-move pruning  
- [x] Types refactor & UCI correctness (v0.4.2)  
- [x] **Mobility evaluation** (v0.4.3)
- [x] **King safety (Attacker weighting)** (v0.4.3)
- [x] **Tapered evaluation framework** (v0.4.3)
- [ ] Late Move Reductions (LMR)  
- [ ] Tapered PSTs (Middlegame vs Endgame tables)
- [ ] Aspiration windows  
- [ ] Passed pawn advancement bonuses  

