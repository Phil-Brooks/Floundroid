# Floundroid
[![.NET](https://img.shields.io/badge/.NET-10.0-blueviolet)](https://dotnet.microsoft.com/download)  
[![Language](https://img.shields.io/badge/Language-F%23-blue)](https://fsharp.org/)  
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)  
![Status](https://img.shields.io/badge/Status-Stage%205%20In%20Progress-orange)  
[![Latest Release](https://img.shields.io/github/v/release/Phil-Brooks/Floundroid)](https://github.com/Phil-Brooks/Floundroid/releases)

<p align="center">
  <img src="logo.png" alt="Floundroid Logo" width="300">
  <br />
  <i>"Functional. Robotic. Fishy."</i>
  <br />
  <strong>Floundroid</strong> is a chess engine built in F#, evolving from functional abstractions into an optimized, multi-threaded, zero-allocation architecture.
</p>

---

## 🚀 Latest Stable Release: v0.5.0 (The Parallel Pulse)

Release **v0.5.0** marks the beginning of **Stage 5: Innovation & Optimization**. This update transitions Floundroid from a single-core speedster into a multi-threaded powerhouse, leveraging hardware-level optimizations to squeeze every ounce of performance out of modern CPUs.

### **Key Improvements in v0.5.0**
- **Lazy SMP (Symmetric Multi-Processing)**
  - Floundroid now searches in parallel. By implementing a "Lazy SMP" architecture, the engine shares information across multiple threads via the Transposition Table, significantly increasing search depth in the same time control.
- **Hardware Acceleration (SIMD/BMI2)**
  - Integrated CPU intrinsics for magic bitboard lookups and bit manipulation. By utilizing **BMI2** (Bit Manipulation Instruction Set 2), the engine reduces the instruction count for move generation, resulting in a cleaner, faster hot-path.
- **Search & Evaluation Refinements**
  - Further stabilization of Aspiration Windows and improved thread synchronization to minimize overhead during parallel searches.

---

## 📊 Benchmarks (v0.5.0 vs Halogen 6.0)

Floundroid v0.5.0 was tested in a 100-game match against **Halogen 6.0 (2440 Elo)** to verify the stability of the new multi-threaded search.

| Metric | Result vs Halogen 6.0 |
| :--- | :--- |
| **Score** | **62.5%** |
| **Elo Difference** | **+88.7 +/- 59.0** |
| **Estimated Performance** | **~2528 Elo** |
| **Wins** | 48 (33 White / 15 Black) |
| **Draws** | 29 |
| **Losses** | 23 |

*Note: The engine continues to demonstrate massive strength as White (76% win rate), with the new SMP implementation providing a significant boost in defensive resilience as Black.*

---

## 🧩 Feature List

### **Architecture**
- **Lazy SMP:** Multi-threaded search utilizing shared Transposition Tables.
- **SIMD/BMI2:** Hardware-accelerated bitboard operations.
- **Zero-Allocation Engine:** Uses `stackalloc` and `Span<T>` to ensure the search hot-path never touches the managed heap.
- **Magic Bitboards:** Optimized sliding piece attack generation.
- **Zobrist Hashing:** Fast incremental position hashing for TT lookups.

### **Search**
- **Selection-Pick Ordering:** Iteratively finds the next best move, avoiding unnecessary sorting.
- **Principal Variation Search (PVS):** Efficiently prunes the search tree.
- **Transposition Table (TT):** Large-scale caching with aging and depth-preferred replacement.
- **Late Move Reductions (LMR):** Reduces search depth for non-critical moves.
- **Null-Move Pruning (NMP):** Aggressive pruning with depth-based reduction scaling.
- **Aspiration Windows:** Narrowed search bounds to speed up alpha-beta cutoffs.

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

### **Stage 5 — Innovation & Optimization** 🏗️ *In Progress*
- [x] **SIMD/BMI2:** Explore intrinsics for magic bitboard lookups. ✅
- [x] **Lazy SMP:** Implement parallel search. ✅
- [ ] **Syzygy Support:** Probe 3-4-5-6 piece tablebases.
- [ ] **NNUE Neural Evaluation core** (Bootstrap training using self-generated data)

---
*👉 [**Download Floundroid.exe v0.5.0**](https://github.com/Phil-Brooks/Floundroid/releases/latest)*

