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
  <strong>Floundroid</strong> is a chess engine built in F#, evolving from functional abstractions into an optimized zero-allocation architecture.
</p>

---

## 🚀 Latest Stable Release: v0.4.9 (The Velocity Surge)

Release **v0.4.9** represents the most significant architectural overhaul in Floundroid's history. By moving away from standard heap allocations and embracing low-level memory management, the engine has achieved a "Velocity Surge" that has widened the gap between Floundroid and its rivals.

In recent testing, Floundroid didn't just beat Halogen 6.0—it dominated it, scoring an incredible **73.5%** over 100 games and gaining over **170 Elo** in a single update.

### **Key Improvements in v0.4.9**
- **Zero-Allocation Search**
  - Replaced all heap-allocated move lists and score arrays with `stackalloc` and `Span<int>`. 
  - Eliminated Garbage Collector (GC) pressure, resulting in much higher and more consistent Nodes Per Second (NPS).
- **Selection-Pick Move Ordering**
  - Transitioned from `Array.Sort` to an iterative "Selection Pick" system. The search now only orders the best moves as needed, drastically reducing wasted cycles in nodes with early cutoffs.
- **Search Stability & Pressure**
  - Refined Aspiration Windows and Null-Move Pruning (NMP) logic.
  - Increased search efficiency and tactical sharpness, causing opponents to buckle under the tactical complexity (evidenced by Halogen 6.0's move-gen failures under pressure).
- 👉 [**Download Floundroid.exe v0.4.9**](https://github.com/Phil-Brooks/Floundroid/releases/latest)

---

## 📊 Benchmarks (v0.4.9 vs Halogen 6.0)

Floundroid v0.4.9 was tested in a 100-game match against **Halogen 6.0 (2440 Elo)**. The results show a complete shift in the power dynamic of the rivalry.

| Metric | Result vs Halogen 6.0 |
| :--- | :--- |
| **Score** | **73.5%** |
| **Elo Difference** | **+177.2 +/- 67.2** |
| **Estimated Performance** | **~2617 Elo** |
| **Wins** | 63 (43 White / 20 Black) |
| **Draws** | 21 |
| **Losses** | 16 |

*Note: Floundroid v0.4.9 demonstrated "White-side Lethality" with a 91% score as White, while maintaining a very high 56% win rate as Black.*

---

## 🧩 Feature List

### **Architecture**
- **Zero-Allocation Engine:** Uses `stackalloc` and `Span<T>` to ensure the search hot-path never touches the managed heap.
- **Bitboard Engine:** Full bitboard representation for pieces and occupancy.
- **Magic Bitboards:** Optimized sliding piece attack generation.
- **Zobrist Hashing:** Fast incremental position hashing for transposition table lookups.

### **Search**
- **Selection-Pick Ordering:** Iteratively finds the next best move, avoiding unnecessary sorting of the entire move list.
- **Principal Variation Search (PVS):** Efficiently prunes the search tree.
- **Transposition Table (TT):** Large-scale caching with aging and depth-preferred replacement.
- **Late Move Reductions (LMR):** Reduces search depth for non-critical moves.
- **Null-Move Pruning (NMP):** Aggressive pruning with depth-based reduction scaling.
- **Aspiration Windows:** Narrowed search bounds to speed up alpha-beta cutoffs.
- **Optuuna Tuning:** All search constants optimized

### **Evaluation**
- **Comprehensive Texel Tuning:** All evaluation constants optimized against 725k positions.
- **Tapered Evaluation:** Smooth transition between Middlegame (MG) and Endgame (EG).
- **Advanced PSTs:** Separate MG/EG 64-square tables for every piece.

---

## 🛠 Project Roadmap

### **Stage 1 — Functional Core** ✅  
### **Stage 2 — UCI Engine Interface** ✅  
### **Stage 3 — Mechanical Brain** ✅  
### **Stage 4 — Strength Phase** ✅  

### **Stage 5 — Innovation & Optimization** 🏗️ *Planned*
- [ ] **SIMD/BMI2:** Explore intrinsics for magic bitboard lookups.
- [ ] **Lazy SMP:** Implement parallel search.
- [ ] **Syzygy Support:** Probe 3-4-5-6 piece tablebases.
- [ ] **NNUE Neural Evaluation core** (Bootstrap training using self generated data)

