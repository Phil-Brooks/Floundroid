namespace Chess.Core

type CastlingRights = {
    WhiteKingside: bool; WhiteQueenside: bool
    BlackKingside: bool; BlackQueenside: bool
}

type BoardState = {
    Squares: Piece option[]        // Array of 64
    Turn: Color
    Castling: CastlingRights
    EnPassant: Square option       // Square behind a pawn that just moved two spaces
    HalfMoveClock: int             // For 50-move rule
    FullMoveNumber: int
}

module Board =
    let empty = {
        Squares = Array.create 64 None
        Turn = White
        Castling = { WhiteKingside = true; WhiteQueenside = true; BlackKingside = true; BlackQueenside = true }
        EnPassant = None
        HalfMoveClock = 0
        FullMoveNumber = 1
    }
    
    // Helper to get piece at square
    let pieceAt (sq: Square) (state: BoardState) = state.Squares.[sq.Value]


    let applyMove (move: Move) (state: BoardState) : BoardState =
        // 1. Copy the squares array so we don't mutate the old state
        let nextSquares = Array.copy state.Squares
        
        // 2. Get the piece that is moving
        let movingPiece = nextSquares.[move.From.Value]
        
        // 3. Update the squares
        nextSquares.[move.To.Value] <- 
            match move.Promotion with
            | Some pType -> Some { Type = pType; Color = state.Turn } // Promote pawn
            | None -> movingPiece
            
        nextSquares.[move.From.Value] <- None

        // 4. Handle special cases (Simplistic versions for now)
        
        // En Passant: If a pawn moves to the EnPassant square, remove the pawn behind it
        // (You will need to expand this logic as you implement full rules)

        // 5. Return the new record with updated metadata
        { state with
            Squares = nextSquares
            Turn = state.Turn.Opposite
            EnPassant = None // Reset EP square (needs logic to set it on double-push)
            HalfMoveClock = if movingPiece.Value.Type = Pawn then 0 else state.HalfMoveClock + 1
            FullMoveNumber = if state.Turn = Black then state.FullMoveNumber + 1 else state.FullMoveNumber
        }