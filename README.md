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

## 📊 Calibration & Rating (Updated for tc=30)

Floundroid is benchmarked against **TSCP 1.81** (est. ~1550 Elo).  
Earlier results using increment time controls were invalid because TSCP cannot handle increments reliably.

### **v0.4.1 Performance at tc=30 (Valid Test Conditions)**

Using a fixed time control of **tc=30**, the results are:

- **Final Score:** `33 – 60 – 7` (36.5%)  
- **Elo Difference:** `–96 ± 69`  
- **Draw Ratio:** 7%  
- **LOS:** 0.3%

These results reveal a clear pattern:

- **As White:**  
  `32 – 13 – 5` (69%) — strong initiative play  
- **As Black:**  
  `1 – 47 – 2` (4%) — severe defensive weaknesses

> These findings indicate a major problem when playing as Black.

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
