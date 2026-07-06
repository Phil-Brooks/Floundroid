# Floundroid — TODO List

---

# Stage 3 — Mechanical Brain (Performance) ✅
- [x] 3.1 Bitboards (Magic Bitboards + Lookup Tables)
- [x] 3.2 Zobrist Hashing (Incremental updates)
- [x] 3.3 Transposition Table (Aging + Probing)
- [x] 3.4 Move Ordering (Killers, History, MVV-LVA)
- [x] 3.5 Quiescence Search (Capture search + Stand-pat)
- [x] 3.6 Time Management (Soft/Hard limits)
- [x] 3.7 Draw Detection (3rd-fold, 50-move, Insufficient material)

---

#### Stage 4 — Strength Phase ✅
*Refocused on data-driven evaluation.*

**4.1 Evaluation Overhaul**
- [x] **Direct Bitboard Iteration:** 12-layer loop for cache-friendly evaluation.
- [x] **Mobility:** Tuned PopCount-based piece activity scoring.
- [x] **Tapered Eval:** Phase-based interpolation between MG and EG.
- [x] **King Safety (Offense):** Mathematically optimized attacker scaling (Weights tuned from 3 $\rightarrow$ 35).
- [x] **Tapered PSTs:** Full 768-entry table optimization via Texel Tuning.
- [x] **Passed Pawns:** Basic detection integrated into tuning.
- [x] **Pawn Shield:** 
- [x] **Rook Bonuses:** Open and half-open file detection.

**4.2 Search Enhancements (The "Big Five") ✅**
- [x] **Null‑move pruning (NMP)**
- [x] **Late Move Reductions (LMR)**
- [x] **Principal Variation Search (PVS)**
- [x] **Reverse Futility Pruning (RFP)**
- [x] **Aspiration Windows**

**4.3 Strength Tuning ✅**
- [x] **Automated Tuning Framework:** Texel Tuning engine implemented with K-factor calibration.
- [x] **Parameter Calibration:** 
- [x] **PST Optimization:** All piece-square tables refined against 725k quiet positions.
- [ ] **SPSA Implementation:** Upgrade tuning loop for faster multi-parameter optimization (Stage 5).

---

#### Stage 5 — Innovation & Optimization 🏗️

**5.1 Performance & Architecture**
- [ ] **SIMD/BMI2:** Explore intrinsics for magic bitboard lookups.

**5.2 Neural Evaluation**
- [ ] **NNUE network architecture.**
- [ ] **Data Generation:** Use tuned classical eval to bootstrap NNUE training data.

