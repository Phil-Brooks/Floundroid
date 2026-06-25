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
- [x] Implement TT Ageing

## 3.4 Move Ordering
- [x] TT move
- [x] QS MoveGen optimisation
- [x] Killer moves
- [x] History heuristic
- [x] MVV‑LVA

## 3.5 Quiescence Search
- [x] Add Promotions to QS
- [x] Stand‑pat evaluation
- [x] Capture search
- [ ] Check extensions (optional)

## 3.6 Time Management
- [x] Soft time limit
- [x] winc / binc support
- [x] Hard time limit
- [x] Node counting

## 3.7 Draw Detection & Correctness (v0.3.7)
- [x] 3-fold repetition
- [x] 50-move rule
- [x] Insufficient material
- [x] Draw scoring in search

---

# Stage 4 — Strength Phase

## 4.1 Evaluation Overhaul
### King Safety (Phase 1 — Basic) 🟡 *in progress but only structure - no effect yet*
- [x] Pawn shield
- [x] Open-file danger
- [x] Enemy piece proximity
- [ ] Attack maps (basic)
- [ ] Tapered eval integration (MG/EG scaling)
- [ ] Queenside castling support
- [ ] Danger scaling (queen > rook > minor)
- [ ] King tropism

### Other Eval Terms
- [ ] Mobility
- [ ] Passed pawns
- [ ] Space
- [ ] Threats
- [ ] Piece activity
- [ ] Rook on open/half-open files
- [ ] Bishop pair bonus

### Overall and Evaluation Refactor (Pre‑Step 4) 🟡 *next*
- [ ] Refactor `evaluate` into sub‑evaluators for each term
- [ ] Extract king safety into `KingSafety` module
- [ ] Extract PST evaluation into standalone function
- [ ] Extract pawn structure into standalone function
- [ ] Introduce `Weights` module
- [ ] Replace magic numbers with weight constants
- [ ] Add unit tests for each eval component
- [ ] Clean, pure `evaluate` composed of sub‑evaluators

## 4.2 Search Enhancements
- [x] Null‑move pruning
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

## 5.3 Hot-Path GC & CPU Optimisations
- [ ] BMI2 PEXT Intrinsics for magic indexing
- [ ] Direct bitboard scanning in MoveGen
- [ ] Direct bitboard iteration in Evaluation
- [ ] Struct Options in TT

## 5.4 Neural Evaluation (Optional)
- [ ] NNUE‑style network
- [ ] ONNX/TorchSharp integration

---

# Meta
- [ ] CI pipeline
- [ ] Benchmarks
- [ ] Documentation site
- [ ] Logo + branding polish
