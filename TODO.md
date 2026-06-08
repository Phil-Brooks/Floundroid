
---

# ✅ **TODO.md (Detailed Project Task List)**

# Floundroid — TODO List 

A detailed, structured task list aligned with the project roadmap.

---

# Stage 1 — Functional Core (Correctness)

## 1.1 Domain Model
- [ ] Define `Piece`, `Color`, `Square`, `File`, `Rank`
- [ ] Define `Move` DU (normal, capture, promotion, castle, en passant)
- [ ] Define immutable `Board` record
- [ ] Add helper functions (flip color, square indexing, etc.)

## 1.2 Move Generation
- [ ] Pawn moves (quiet, capture, double, promotion)
- [ ] Knight moves
- [ ] Bishop moves
- [ ] Rook moves
- [ ] Queen moves
- [ ] King moves
- [ ] Castling rules
- [ ] En passant rules
- [ ] Check detection
- [ ] Pin detection
- [ ] Legal move filtering

## 1.3 FEN + Utilities
- [ ] FEN parser
- [ ] FEN serializer
- [ ] Board pretty‑printer
- [ ] Move → SAN (optional)

## 1.4 Perft
- [ ] Implement perft driver
- [ ] Add standard test positions
- [ ] Validate perft(1)–perft(6)
- [ ] Add regression tests

## 1.5 Debugging Tools
- [ ] Move list visualisation
- [ ] Board consistency checker
- [ ] Attack map visualiser

---

# Stage 2 — UCI Engine Interface

## 2.1 UCI Protocol
- [ ] Implement `uci`
- [ ] Implement `isready`
- [ ] Implement `position`
- [ ] Implement `go depth N`
- [ ] Implement `stop`
- [ ] Implement `quit`

## 2.2 Search Framework
- [ ] Iterative deepening loop
- [ ] Alpha‑beta search
- [ ] Basic move ordering (captures first)
- [ ] Simple evaluation (material)
- [ ] Add piece‑square tables

## 2.3 Async Architecture
- [ ] Async search loop
- [ ] Async command listener
- [ ] Cancellation token for `stop`

## 2.4 GUI Integration
- [ ] Arena config
- [ ] CuteChess config
- [ ] Banksia config

---

# Stage 3 — Mechanical Brain (Performance)

## 3.1 Bitboards
- [ ] Replace board model with bitboards
- [ ] 12 piece bitboards
- [ ] Occupancy bitboards
- [ ] Precomputed attack tables
- [ ] Magic bitboards (optional)

## 3.2 Zobrist Hashing
- [ ] Generate random keys
- [ ] Implement hash update logic
- [ ] Integrate into board state

## 3.3 Transposition Table
- [ ] Define TT entry struct
- [ ] Implement store/probe logic
- [ ] Integrate into search

## 3.4 Move Ordering
- [ ] TT move
- [ ] Killer moves
- [ ] History heuristic
- [ ] MVV‑LVA for captures

## 3.5 Quiescence Search
- [ ] Stand‑pat evaluation
- [ ] Capture search
- [ ] Check extensions (optional)

## 3.6 Time Management
- [ ] Soft time limit
- [ ] Hard time limit
- [ ] Node counting

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
