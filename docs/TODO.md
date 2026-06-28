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

# Stage 4 — Strength Phase 🏗️

## 4.1 Evaluation Overhaul
- [x] **Direct Bitboard Iteration:** 12-layer loop for cache-friendly evaluation.
- [x] **Mobility:** PopCount-based piece activity scoring.
- [x] **Tapered Eval:** Phase-based interpolation between MG and EG.
- [x] **King Safety (Offense):** Attacker scaling based on proximity to enemy King.
- [x] **Tapered PSTs:** Distinct MG and EG tables.
- [x] **Passed Pawns:** Basic detection and rank-based bonuses.
- [ ] **Pawn Shield:** (Deferred - Requires tuning).
- [ ] **Rook Bonuses:** Open and half-open file detection. (Likely covered by mobility).

## 4.2 Search Enhancements (The "Big Five") ✅
- [x] **Null‑move pruning (NMP)**
- [x] **Late Move Reductions (LMR)**
- [x] **Principal Variation Search (PVS)**
- [x] **Reverse Futility Pruning (RFP)**
- [x] **Aspiration Windows** (v0.4.4)
- [ ] **Singular extensions** (Advanced - Save for Stage 5)
- [ ] **Static Exchange Evaluation (SEE)** (Deferred - Stability priority)

## 4.3 Strength Tuning 🔵 *In Progress*
- [ ] **Parameter Calibration:** Tune material values (matsMG/EG) against test suites.
- [ ] **PST Optimization:** Refine piece-square tables for better positional play.
- [ ] **Automated Tuning:** Explore Texel Tuning or SPSA framework.

---

# Stage 5 — Innovation & Optimization

## 5.1 Performance Experiments
- [ ] SIMD intrinsics for bitboard operations.
- [ ] BMI2 PEXT Intrinsics for magic indexing.
- [ ] Struct-based Move representation to reduce allocations.

## 5.2 Neural Evaluation
- [ ] NNUE network architecture.
- [ ] ONNX runtime integration for F#.

