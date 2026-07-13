namespace Floundroid

open System.Runtime.InteropServices
open System.Numerics // For BitOperations

[<AutoOpen>]
module Types =

    /// Bitboards are 64-bit unsigned integers where each bit represents a square.
    /// Bit 0 is a1, Bit 7 is h1, Bit 63 is h8.
    type Bitboard = uint64

    /// A collection of bitboards representing all pieces on the board.
    type BitboardSet =
        { WhitePawns: Bitboard
          WhiteKnights: Bitboard
          WhiteBishops: Bitboard
          WhiteRooks: Bitboard
          WhiteQueens: Bitboard
          WhiteKings: Bitboard
          BlackPawns: Bitboard
          BlackKnights: Bitboard
          BlackBishops: Bitboard
          BlackRooks: Bitboard
          BlackQueens: Bitboard
          BlackKings: Bitboard
          // Combined layers
          WhiteTotal: Bitboard
          BlackTotal: Bitboard
          Occupancy: Bitboard }

    /// The Board type represents the state of a chess game, including piece placement, side to move, castling rights, en passant target square, and move clocks.
    type Board =
        { Bitboards: BitboardSet
          SideToMove: int
          CastlingRights: int
          EnPassantSquare: int
          HalfmoveClock: int
          FullmoveNumber: int
          ScoreMG: int
          ScoreEG: int
          Hash: uint64 }

    [<Struct; StructLayout(LayoutKind.Sequential, Pack = 1)>]
    type MarlinRecord =
        [<DefaultValue>] val mutable Occupancy: uint64
        [<DefaultValue>] val mutable WhitePieces: uint64
        [<DefaultValue>] val mutable Score: int16
        [<DefaultValue>] val mutable Result: byte
        [<DefaultValue>] val mutable SideToMove: byte
        [<DefaultValue>] val mutable WhiteKingSq: byte
        [<DefaultValue>] val mutable BlackKingSq: byte
        [<DefaultValue>] val mutable Padding: uint64
        [<DefaultValue>] val mutable FullMove: uint16    
    
    /// Represents a single test case for the perft suite, including the position (FEN), expected node counts at various depths, and a name for identification.
    type PerftSuiteItem =
        { Name: string
          Fen: string
          Expected: uint64 list }
