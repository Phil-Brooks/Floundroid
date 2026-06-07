# Floundroid


[![Build Status](https://img.shields.io/badge/.NET-10.0-blueviolet)](https://dotnet.microsoft.com/download)

[![Language](https://img.shields.io/badge/Language-F%23-blue)](https://fsharp.org/)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

<p align="center">

&#x20; <img src="logo.png" alt="Floundroid Logo" width="300">

&#x20; <i>"Functional. Robotic. Fishy."</i>

&#x20; <strong>Floundroid</strong> is a chess engine built in F#, designed to move from clean functional abstractions to high-performance bit-twiddling.

</p>


## 🌊 Why Floundroid?

The name **Floundroid** represents the intersection of the natural (Flounder/Stockfish heritage) and the synthetic (Android/Functional logic). By leveraging F#’s powerful type system, Floundroid aims to make "impossible chess states unrepresentable" while eventually competing with the strongest engines in the .NET ecosystem.


---



## 🛠 Project Roadmap



Floundroid is being built in three distinct stages to ensure correctness before pursuing raw speed.



### Stage 1: The Core (In Progress) 🟢

Focus: **Correctness and Domain Modeling**.

- [ ] **Types**: Using Discriminated Unions and Records for pieces, squares, and moves.

- [ ] **MoveGen**: Pure functional move generation for all pieces.

- [ ] **Validation**: Implementation of the **Perft** (Performance Test) suite.

- [ ] **FEN**: Ability to load board states via Forsyth-Edwards Notation.



### Stage 2: The Interface 🟡

Focus: **The UCI Protocol**.

- [ ] **UCI Bridge**: A standard-input/output loop to talk to GUIs like Arena or CuteChess.

- [ ] **Async Search**: Using F# `async` workflows to allow the engine to "think" while still listening for commands.

- [ ] **Basic Search**: Alpha-Beta pruning with simple material evaluation.



### Stage 3: The Mechanical Brain 🔴

Focus: **Performance and Strength**.

- [ ] **Bitboards**: Transitioning to `uint64` representations for lightning-fast move generation.

- [ ] **Memory**: Implementation of Transposition Tables using Zobrist Hashing.

- [ ] **Heuristics**: Iterative deepening, Quiescence search, and Move Ordering.



---



## 🚀 Getting Started



### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)



### Build and Run

Clone the repository and run the engine locally:



```bash

git clone https://github.com/Phil-Brooks/Floundroid.git

cd Floundroid

dotnet build

dotnet run

