# ✅ **TODO.md (Detailed Project Task List)**

# Floundroid — TODO List 

A detailed, structured task list aligned with the project roadmap.

---

# Stage 1 — Functional Core (Correctness)

## 1.1 — Core Types
- [x] Define foundational types (Colour, File, Rank, CastlingRights)
- [x] Define Square type + helpers (separate module)
- [x] Define PieceType and Piece
- [x] Define MoveKind and Move
- [x] Define Board (full GameState)
- [x] Remove duplicate Square definitions
- [x] Ensure no circular dependencies
- [x] Add barrel module (`module All = ()`)
- [x] Update all modules to use new types

## 1.2 Move Generation
- [x] Knight moves
- [x] Bishop moves
- [x] Rook moves
- [x] Queen moves
- [x] King moves (basic)
- [x] Pawn moves (quiet, capture, double)
- [x] En passant rules
- [x] Pawn promotions
- [x] Castling rules (pseudo‑legal: rights + path clearance)
- [x] Attack Detection (`isSquareAttacked`)
- [x] Check detection
- [x] Pin detection
- [x] Legal move filtering (The "King Safety" filter)

## 1.3 FEN + Utilities
- [x] FEN parser
- [x] FEN serializer
- [x] Board pretty‑printer
- [x] Move → SAN (Disambiguation, check, and mate support)

## 1.4 Perft
- [ ] Implement perft driver (Recursive counter + Divide utility with SAN/NPS)
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
- [x] Implement `uci`
- [x] Implement `isready`
- [x] Implement `position` (startpos and FEN)
- [ ] Implement `go depth N`
- [ ] Implement `stop`
- [x] Implement `quit`

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