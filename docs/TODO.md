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
- [ ] **Pawn Shield:** (Deferred - Requires tuning as v0.4.3 implementation was unsuccessful).
- [x] **Tapered PSTs:** Implement distinct MG and EG tables (High Priority).
- [ ] **Rook Bonuses:** Open and half-open file detection. (not sure as probably already covered by mobility)

## 4.2 Search Enhancements
- [x] Null‑move pruning
- [x] Late Move Reductions (LMR)
- [x] Principal Variation Search (PVS): Null-window searches for non-PV nodes.
- [ ] Aspiration windows
- [x] Reverse Futility Pruning
- [ ] Singular extensions

---

# Stage 5 — Innovation & Optimization

## 5.1 Performance Experiments
- [ ] SIMD intrinsics for bitboard operations.
- [ ] BMI2 PEXT Intrinsics for magic indexing.
- [ ] Struct-based Move representation to reduce allocations.

## 5.2 Neural Evaluation
- [ ] NNUE network architecture.
- [ ] ONNX runtime integration for F#.