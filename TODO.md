# ✅ **TODO.md (Detailed Project Task List)**

# Floundroid — TODO List 

A detailed, structured task list aligned with the project roadmap.

---

# Stage 1 — Functional Core (Correctness) ✅
- [x] 1.1 — Core Types
- [x] 1.2 — Move Generation
- [x] 1.3 — FEN + Utilities
- [x] 1.4 — Perft Validation
- [x] 1.5 — Debugging Tools

---

# Stage 2 — UCI Engine Interface ✅
- [x] 2.1 — UCI Protocol (`uci`, `isready`, `position`, `go`, `stop`, `quit`)
- [x] 2.2 — Search Framework (Alpha-beta, ID, PST, Quiescence)
- [x] 2.3 — Async Architecture (Thread pooling + Cancellation tokens)
- [x] 2.4 — GUI Integration (Verified in Arena/CuteChess)
- [x] 2.5 — **Milestone: v0.2.0 Functional Release**

---

# Stage 3 — Mechanical Brain (Performance) 🏗️
## 3.1 Bitboards
- [x] Implement `Bitboard` type (uint64)
- [x] Map piece placements to 12 piece bitboards
- [x] Remove `Map<Square, Piece>` as source of truth
- [x] Implement bitwise pawn/knight/king attack generation (Lookup tables)
- [x] Port `isSquareAttacked` to bitboard logic
- [x] Implement bitwise sliding attack generation (Magic Bitboards) 🟡

## 3.2 Zobrist Hashing
- [x] Generate random keys for pieces, side, castling, and EP
- [x] Implement incremental hash updates in `applyMove`

## 3.3 Transposition Table
- [x] Define TT entry structure
- [x] Implement TT Probing and Storing logic in Search
- [x] Implement TT Ageing (to prevent opening transpositions from clogging the endgame)

## 3.4 Move Ordering
- [x] TT move
- [ ] Optimize QS MoveGen (Stop generating full legal moves; generate only captures)
- [x] Killer moves
- [x] History heuristic
- [x] MVV‑LVA for captures

## 3.5 Quiescence Search
- [ ] Add Promotions to Quiescence Search
- [x] Stand‑pat evaluation
- [x] Capture search
- [ ] Check extensions (optional)

## 3.6 Time Management
- [x] Soft time limit
- [ ] Update Time Management to support winc / binc parameters
- [x] Hard time limit
- [x] Node counting

---

# Stage 4 — Strength Phase

## 4.1 Evaluation Overhaul
- [ ] Pawn structure evaluation
- [ ] King safety
- [ ] Mobility
- [ ] Passed pawns
- [ ] Space
- [ ] Tapered eval (MG/EG)

## 4.2 Search Enhancements
- [ ] Null‑move pruning
- [ ] Late Move Reductions (LMR)
- [ ] Aspiration windows
- [ ] Singular extensions
- [ ] Futility pruning

## 4.3 Parallel Search (Optional)
- [ ] Lazy SMP
- [ ] Shared TT

## 4.4 Endgame Tablebases (Optional)
- [ ] Syzygy probing

---

# Stage 5 — Innovation & F#‑Specific Optimisation

## 5.1 Hybrid Architecture
- [ ] Functional façade over imperative core
- [ ] Struct‑based hot paths

## 5.2 Performance Experiments
- [ ] SIMD intrinsics for bitboard ops
- [ ] Vectorised attack generation

## 5.3 Neural Evaluation (Optional)
- [ ] NNUE‑style network
- [ ] ONNX/TorchSharp integration

---

# Meta
- [ ] Add CI pipeline
- [ ] Add benchmarks
- [ ] Add documentation site
- [ ] Add logo + branding polish
