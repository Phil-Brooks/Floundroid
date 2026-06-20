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
  <strong>Floundroid</strong> is a chess engine built in F#, evolving from clean functional abstractions into a high‑performance bitboard machine.
</p>

---

## 🚀 Latest Stable Release: v0.4.1 (Pawn Structure Evaluation)

This release introduces the first major components of the Stage 4 Strength Phase, focusing on positional understanding through pawn structure analysis.

### **Key Features in v0.4.1**
- **Pawn Structure Evaluation**
  - Passed pawns  
  - Isolated pawns  
  - Doubled pawns  
- **Null-Move Pruning (NMP)**  
- **Adaptive Depth Reduction**  
- **Improved UCI Increment Handling**  
- 👉 [**Download Floundroid.exe v0.4.1**](https://github.com/Phil-Brooks/Floundroid/releases/latest)

---

## 📊 Calibration & Rating (Updated for Cinnamon 2.0)

Floundroid is now benchmarked against **Cinnamon 2.0**, a significantly stronger and more modern engine than TSCP.  
Cinnamon includes full positional evaluation (king safety, mobility, tapered eval, etc.) and is estimated around **2000–2200 Elo**, making it an excellent reference opponent for Stage 4 development.

### **v0.4.1 Performance vs Cinnamon 2.0 (tc=30, 100 games)**

- **Final Score:** `4 – 89 – 7` (7.5%)  
- **Elo Difference:** `–436 ± 127`  
- **Draw Ratio:** 7%  
- **LOS:** 0.0%

These results are expected for an engine at the beginning of Stage 4:

- **As White:**  
  `2 – 43 – 5` (9.0%) — shows initiative and can convert when ahead  
- **As Black:**  
  `2 – 46 – 2` (6.0%) — major defensive vulnerabilities  

### **Interpretation**

Cinnamon exposes the next set of improvements Floundroid needs:

- **King safety evaluation** (highest priority)  
- **Mobility evaluation**  
- **Tapered evaluation**  
- **LMR and aspiration windows**  

Despite the rating gap, Floundroid demonstrated:

- Stable search  
- Correct move generation  
- Strong tactical defence  
- Clean conversions when ahead  
- No illegal moves or engine crashes  

This confirms the engine is **structurally healthy** and ready for the next strength upgrades.

---

## 🛠 Project Roadmap

### **Stage 1 — Functional Core** ✅
- Strong F# domain modelling  
- Perft validation  

### **Stage 2 — UCI Engine Interface** ✅
- Async search  
- Iterative deepening  

### **Stage 3 — Mechanical Brain** ✅
- Bitboards / Magic Bitboards  
- Zobrist hashing  
- Transposition table  
- Move ordering (Killers, History, MVV-LVA)  
- Draw detection  

### **Stage 4 — Strength Phase** 🔵 *In Progress*
- [x] Pawn structure evaluation  
- [x] Null-move pruning  
- [ ] **Mobility evaluation**  
- [ ] **King safety evaluation**  
- [ ] LMR (Late Move Reductions)  
- [ ] Aspiration windows  
- [ ] Tapered evaluation  

### **Stage 5 — Innovation** 🟣
- [ ] SIMD / hardware intrinsics  
- [ ] Optional NNUE evaluation  

---

## 🚀 Getting Started

### **Prerequisites**
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)

### **Installation**
```bash
git clone https://github.com/Phil-Brooks/Floundroid.git
cd Floundroid
dotnet build -c Release
