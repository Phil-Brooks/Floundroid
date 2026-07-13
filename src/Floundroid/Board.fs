namespace Floundroid

open System
open System.Text
open System.Numerics

module Zobrist =

    /// Storage for all random keys used for hashing.
    type ZobristTable = {
        /// [colour (2)][pieceType (6)][square (64)]
        Pieces: uint64[,,]
        /// Key to XOR if it is Black to move
        SideToMove: uint64
        /// Keys for the 16 possible combinations of castling rights
        Castling: uint64[]
        /// Keys for the 8 possible files for an En Passant target
        EnPassantFile: uint64[]
    }

    /// Pre-calculates the table with a fixed seed for reproducibility.
    let private initializeTable () =
        let seed = 1010101 
        let rng = Random(seed)
        
        let next64 () =
            let buffer = Array.zeroCreate<byte> 8
            rng.NextBytes(buffer)
            BitConverter.ToUInt64(buffer, 0)

        let pieces = Array3D.init 2 6 64 (fun _ _ _ -> next64())
        let side = next64()
        let castling = Array.init 16 (fun _ -> next64())
        let ep = Array.init 8 (fun _ -> next64())

        { Pieces = pieces; SideToMove = side; Castling = castling; EnPassantFile = ep }

    /// The global lookup table for Zobrist keys.
    let Table = initializeTable()

    /// Gets the key for a specific piece on a square.
    let getPieceKey (pc: int) (sq: int) =
        Table.Pieces.[Piece.colour pc, Piece.kind pc, sq]

    /// Gets the key for a specific set of castling rights.
    let getCastlingKey (cr: int) =
        let mutable index = 0
        if (cr &&& CastlingRights.WK) <> 0  then index <- index ||| 1
        if (cr &&& CastlingRights.WQ) <> 0 then index <- index ||| 2
        if (cr &&& CastlingRights.BK) <> 0  then index <- index ||| 4
        if (cr &&& CastlingRights.BQ) <> 0 then index <- index ||| 8
        Table.Castling.[index]

    /// Gets the key for an En Passant file (0-7).
    let getEnPassantKey (sq: int) =
        if sq < 0 || sq > 63 then 0UL else
        Table.EnPassantFile.[sq % 8]

module TranspositionTable =

    /// Flags for TT entries: Exact (PV), Alpha (Upper bound), Beta (Lower bound)
    let [<Literal>] NodeExact = 0
    let [<Literal>] NodeAlpha = 1
    let [<Literal>] NodeBeta = 2

    [<Struct>]
    type TTEntry = {
        Hash: uint64
        Move: int
        Value: int
        Depth: int
        Age : byte
        Flag: int
    }

    // Update empty entry
    let emptyEntry = { Hash = 0UL; Move = 0; Value = 0; Depth = -1; Age = 0uy; Flag = NodeAlpha }

    // Global age counter
    let mutable currentAge = 0uy

    /// Advances the age of the transposition table, allowing for aging out old entries.
    let advanceAge () = currentAge <- currentAge + 1uy

    /// A table size of 2^20 is roughly 32-64MB depending on padding.
    let SIZE = 1 <<< 20 
    let table: TTEntry[] = Array.create SIZE emptyEntry

    /// Adjusts mate scores from the search to be relative to the root.
    /// This ensures "Mate in 5" found at depth 10 is stored correctly.
    let mateToTT (score: int) (ply: int) =
        if score > 20000 then score + ply
        elif score < -20000 then score - ply
        else score

    /// Adjusts mate scores from the TT back to the search, reversing the previous adjustment.
    let mateFromTT (score: int) (ply: int) =
        if score > 20000 then score - ply
        elif score < -20000 then score + ply
        else score

    /// Clears the transposition table, resetting all entries to empty.
    let clear () =
        Array.fill table 0 SIZE emptyEntry

    /// Stores an entry in the transposition table with the given parameters.
    let store (hash: uint64) (depth: int) (ply: int) (flag: int) (value: int) (m: int) =
        let index = int (hash &&& uint64 (SIZE - 1))
        let adjustedValue = mateToTT value ply
        
        let existing = table.[index]

        // REPLACEMENT STRATEGY: 
        // 1. Always replace if the slot is empty or the hash matches (updating existing info)
        // 2. Always replace if the existing entry is from an OLDER age
        // 3. Otherwise, only replace if the new search is deeper
        let isOld = existing.Age <> currentAge
        
        if existing.Hash = 0UL || existing.Hash = hash || isOld || depth >= existing.Depth then
            table.[index] <- { 
                Hash = hash; Move = m; Value = adjustedValue; 
                Depth = depth; Flag = flag; Age = currentAge // <-- Store current age
            }

    let probe (hash: uint64) =
        let index = int (hash &&& uint64 (SIZE - 1))
        let entry = table.[index]
        if entry.Hash = hash then Some entry else None

module Board =

    let empty =
        { Bitboards = BitboardSet.empty // Placeholder
          SideToMove = Colour.White
          CastlingRights = CastlingRights.None
          EnPassantSquare = -1
          HalfmoveClock = 0
          FullmoveNumber = 1
          ScoreMG = 0
          ScoreEG = 0
          Hash = 0UL }

    let isSquareAttacked (b: Board) (sq: int) (attacker: int) =
        let bbs = b.Bitboards
        let them = attacker

        // 1. Pawn, Knight, King (Keep existing logic)
        let usIdx = if them = Colour.Black then 0 else 1
        let pawnAttackMask = BitboardGen.pawnAttacks.[usIdx, sq]
        let themPawns = if them = Colour.White then bbs.WhitePawns else bbs.BlackPawns

        if (pawnAttackMask &&& themPawns) <> 0uL then true
        else
            let knightAttackMask = BitboardGen.knightAttacks.[sq]
            let themKnights = if them = Colour.White then bbs.WhiteKnights else bbs.BlackKnights
            if (knightAttackMask &&& themKnights) <> 0uL then true
            else
                let kingAttackMask = BitboardGen.kingAttacks.[sq]
                let themKing = if them = Colour.White then bbs.WhiteKings else bbs.BlackKings
                if (kingAttackMask &&& themKing) <> 0uL then true
                else
                    // Inside Board.isSquareAttacked, replace the sliding logic with:
                    let occ = bbs.Occupancy

                    // Bishop & Queen
                    let bEntry = Magic.bishopEntries.[sq]
                    let bIdx = bEntry.Offset + Magic.getIndex occ bEntry.Mask
                    let bishopAttacks = Magic.bishopTable.[bIdx]
                    let themBishops = if them = Colour.White then (bbs.WhiteBishops ||| bbs.WhiteQueens) else (bbs.BlackBishops ||| bbs.BlackQueens)

                    if (bishopAttacks &&& themBishops) <> 0uL then true
                    else
                        // Rook & Queen
                        let rEntry = Magic.rookEntries.[sq]
                        let rIdx = rEntry.Offset + Magic.getIndex occ rEntry.Mask
                        let rookAttacks = Magic.rookTable.[rIdx]
                        let themRooks = if them = Colour.White then (bbs.WhiteRooks ||| bbs.WhiteQueens) else (bbs.BlackRooks ||| bbs.BlackQueens)
    
                        (rookAttacks &&& themRooks) <> 0uL                    

    let fromUci (b: Board) (s: string) : int =
        if s.Length < 4 then
            invalidArg "s" "UCI move string too short"

        let fromSq = Square.fromString (s.Substring(0, 2))
        let toSq   = Square.fromString (s.Substring(2, 2))

        let movingPiece =
            BitboardSet.getPieceAt fromSq b.Bitboards

        let isPawn = Piece.kind movingPiece = PieceType.Pawn

        // --- 1. Promotion ---
        if s.Length = 5 then
            let promo = int (PieceType.fromChar s.[4])
            Move.create(fromSq, toSq, 5, promo)

        // --- 2. Castling ---
        elif Piece.kind movingPiece = PieceType.King then
            match (fromSq, toSq) with
            | (4, 6)   -> Move.create(fromSq, toSq, 3, 0)   // White O-O
            | (4, 2)   -> Move.create(fromSq, toSq, 4, 0)   // White O-O-O
            | (60, 62) -> Move.create(fromSq, toSq, 3, 0)   // Black O-O
            | (60, 58) -> Move.create(fromSq, toSq, 4, 0)   // Black O-O-O
            | _ -> Move.create(fromSq, toSq, 0, 0)

        // --- 3. En passant ---
        elif isPawn &&
             b.EnPassantSquare <> -1 &&
             toSq = b.EnPassantSquare &&
             Square.file fromSq <> Square.file toSq then
            Move.create(fromSq, toSq, 2, 0)

        // --- 4. Capture ---
        elif BitboardSet.getPieceAt toSq b.Bitboards <> -1 then
            Move.create(fromSq, toSq, 1, 0)

        // --- 5. Quiet ---
        else
            Move.create(fromSq, toSq, 0, 0)
   
    /// Tries to get a piece from a square (Source of truth: Bitboards).
    let tryGetPiece (b: Board) (sq: int) = 
        BitboardSet.getPieceAt sq b.Bitboards

    /// Checks if a square is occupied (Source of truth: Bitboards).
    let isOccupied (b: Board) (sq: int) =
        (b.Bitboards.Occupancy &&& (1uL <<< sq)) <> 0uL
    
    /// Checks if a square is occupied by White
    let isOccupiedW (b: Board) (sq: int) =
        (b.Bitboards.WhiteTotal &&& (1uL <<< sq)) <> 0uL

    /// Checks if a square is occupied by Black
    let isOccupiedB (b: Board) (sq: int) =
        (b.Bitboards.BlackTotal &&& (1uL <<< sq)) <> 0uL

    /// Find the king square (Needed for check detection).
    let findKing (colour: int) (b: Board) =
        let mutable bb =
            if colour = Colour.White then
                b.Bitboards.WhiteKings
            else
                b.Bitboards.BlackKings

        if bb = 0uL then -1 else Bitboard.popLsb &bb

    /// Sets a piece on a square and updates bitboards (Source of truth: Bitboards).
    let setPiece (b: Board) (sq: int) (pOpt: int option) =
        let mutable newBbs = b.Bitboards

        // 1. If there's already a piece at this square, we must toggle it OFF first
        let oldPiece = BitboardSet.getPieceAt sq b.Bitboards 
        if oldPiece <> -1 then
            newBbs <- BitboardSet.togglePiece oldPiece sq newBbs

        // 2. If we are setting a new piece, toggle it ON
        match pOpt with
        | Some p -> newBbs <- BitboardSet.togglePiece p sq newBbs
        | None -> ()

        { b with
            Bitboards = newBbs }

    /// Calculates the full Zobrist hash from scratch (Used for FEN initialization)
    let calculateHash (b: Board) =
        let mutable h = 0UL
        
        // 1. Pieces
        for (sq, piece) in BitboardSet.allPieces b.Bitboards do
            h <- h ^^^ (Zobrist.getPieceKey piece sq)
            
        // 2. Side to move
        if b.SideToMove = Colour.Black then
            h <- h ^^^ Zobrist.Table.SideToMove
            
        // 3. Castling rights
        h <- h ^^^ (Zobrist.getCastlingKey b.CastlingRights)
        
        // 4. En Passant
        h <- h ^^^ (Zobrist.getEnPassantKey b.EnPassantSquare)
        
        h    
    
    /// Parses a FEN string and returns a Board record representing the position.
    let fromFen (fen: string) =
        let parts = fen.Split(' ')
        let rows = parts.[0].Split('/')
        let mutable bbs = BitboardSet.empty // Start with empty bitboards

        for r in 0..7 do
            let rank, file = 7 - r, ref 0
            for char in rows.[r] do
                if Char.IsDigit char then
                    file.Value <- file.Value + (int char - int '0')
                else
                    let sq = Square.ofFileRank (file.Value) rank
                    // Directly toggle the piece on the bitboard
                    bbs <- BitboardSet.togglePiece (Piece.fromChar char) sq bbs
                    file.Value <- file.Value + 1

        let scmg, sceg = BitboardSet.getscr bbs
        
        let boardWithoutHash = 
            { Bitboards = bbs
              SideToMove = Colour.fromChar parts.[1].[0]
              CastlingRights = CastlingRights.fromString parts.[2]
              EnPassantSquare = if parts.[3] = "-" then -1 else Square.fromString parts.[3]
              HalfmoveClock = if parts.Length > 4 then int parts.[4] else 0
              FullmoveNumber = if parts.Length > 5 then int parts.[5] else 1
              ScoreMG = scmg
              ScoreEG = sceg
              Hash = 0UL }
        { boardWithoutHash with Hash = calculateHash boardWithoutHash }
 
    /// The standard starting position in FEN notation.
    let start = fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"

    /// Converts a Board record to its FEN string representation.
    let toFen (b: Board) =
        let sb = StringBuilder()
        for r in 7..-1..0 do
            let mutable emptyCount = 0
            for f in 0..7 do
                let sq = Square.ofFileRank f r
                let p = tryGetPiece b sq // This now uses Bitboards!
                if p <> -1 then
                    if emptyCount > 0 then sb.Append(emptyCount) |> ignore
                    emptyCount <- 0
                    sb.Append(Piece.toChar p) |> ignore
                else emptyCount <- emptyCount + 1
            if emptyCount > 0 then sb.Append(emptyCount) |> ignore
            if r > 0 then sb.Append('/') |> ignore

        sprintf "%O %c %s %s %d %d"
            sb
            (Colour.toChar b.SideToMove)
            (CastlingRights.toString b.CastlingRights)
            (match b.EnPassantSquare with | s when s <> -1 -> Square.toString s | _ -> "-")
            b.HalfmoveClock
            b.FullmoveNumber

    /// <summary>
    /// Checks if a player is in check.
    /// </summary>
    /// <param name="colour">The colour of the player to check.</param>
    /// <param name="b">The current game state.</param>
    /// <returns>True if the player is in check, false otherwise.</returns>
    let isInCheckFor (colour: int) (b: Board) =
        let kingSq = findKing colour b
        if kingSq = -1 then false
        else isSquareAttacked b kingSq (Colour.opposite colour)
    
    /// Checks if the side to move is currently in check.
    let isInCheck (b: Board) = isInCheckFor b.SideToMove b

    /// <summary>
    /// Executes a move on the board and returns a new immutable board state.
    /// Updates castling rights, en passant targets, and move clocks.
    /// </summary>
    /// <param name="m">The validated move to apply.</param>
    /// <param name="b">The current game state.</param>
    /// <returns>A new Board record reflecting the post-move state.</returns>
    /// <summary>
    /// Executes a move, updating both Bitboards and the Piece Map.
    /// This is the final step before the Map is removed entirely.
    /// </summary>
    let applyMove (m: int) (b: Board) =
        // 1. Initialize variables for the new state
        let mutable newBitboards = b.Bitboards
        let mutable newScoreMG = b.ScoreMG
        let mutable newScoreEG = b.ScoreEG
        let mutable newHash = b.Hash
        let movingPiece = BitboardSet.getPieceAt (Move.fromSq m) b.Bitboards
        let isPawn = Piece.kind movingPiece = PieceType.Pawn
        let opponent = Colour.opposite b.SideToMove
        let toSq = Move.toSq m
        let fromSq = Move.fromSq m

        // 2. XOR out old state from Hash (Side, Castling, EP)
        newHash <- newHash ^^^ Zobrist.Table.SideToMove
        newHash <- newHash ^^^ (Zobrist.getCastlingKey b.CastlingRights)
        newHash <- newHash ^^^ (Zobrist.getEnPassantKey b.EnPassantSquare)

        // 3. Remove the moving piece from the source
        // TogglePiece XORs the bitboard and we XOR the hash
        newBitboards <- BitboardSet.togglePiece movingPiece fromSq newBitboards
        newHash <- newHash ^^^ (Zobrist.getPieceKey movingPiece fromSq) 
        if b.SideToMove = Colour.White then
            newScoreMG <- newScoreMG - Pst.MG[Piece.kind movingPiece].[fromSq]
            newScoreEG <- newScoreEG - Pst.EG[Piece.kind movingPiece].[fromSq]
        else
            let idx = fromSq ^^^ 56
            newScoreMG <- newScoreMG + Pst.MG[Piece.kind movingPiece].[idx]
            newScoreEG <- newScoreEG + Pst.EG[Piece.kind movingPiece].[idx]

        // 4. Handle Captures (including En Passant)
        let capturedPieceAtTo = BitboardSet.getPieceAt toSq b.Bitboards
    
        match Move.kind m with
        | 2 ->   // EnPassant
            let epPawnSq = if b.SideToMove = Colour.White then toSq - 8 else toSq + 8
            let victimPawn = (opponent <<< 3) ||| PieceType.Pawn
            newBitboards <- BitboardSet.togglePiece victimPawn epPawnSq newBitboards
            newHash <- newHash ^^^ (Zobrist.getPieceKey victimPawn epPawnSq)
            if b.SideToMove = Colour.White then
                let idx = epPawnSq ^^^ 56
                newScoreMG <- newScoreMG + Pst.MG[PieceType.Pawn].[idx] + Pst.matsMG[PieceType.Pawn]
                newScoreEG <- newScoreEG + Pst.EG[PieceType.Pawn].[idx] + Pst.matsEG[PieceType.Pawn]
            else
                newScoreMG <- newScoreMG - Pst.MG[PieceType.Pawn].[epPawnSq] - Pst.matsMG[PieceType.Pawn]
                newScoreEG <- newScoreEG - Pst.EG[PieceType.Pawn].[epPawnSq] - Pst.matsEG[PieceType.Pawn]

        | _ ->
            // Normal captures (Quiet, Promotion, or Castling can't capture, but we check 'To' occupancy)
            if capturedPieceAtTo <> -1 then
                newBitboards <- BitboardSet.togglePiece capturedPieceAtTo toSq newBitboards
                newHash <- newHash ^^^ (Zobrist.getPieceKey capturedPieceAtTo toSq)
                if b.SideToMove = Colour.White then
                    let idx = toSq ^^^ 56
                    newScoreMG <- newScoreMG + Pst.MG[Piece.kind capturedPieceAtTo].[idx] + Pst.matsMG[Piece.kind capturedPieceAtTo]
                    newScoreEG <- newScoreEG + Pst.EG[Piece.kind capturedPieceAtTo].[idx] + Pst.matsEG[Piece.kind capturedPieceAtTo]
                else
                    newScoreMG <- newScoreMG - Pst.MG[Piece.kind capturedPieceAtTo].[toSq] - Pst.matsMG[Piece.kind capturedPieceAtTo]
                    newScoreEG <- newScoreEG - Pst.EG[Piece.kind capturedPieceAtTo].[toSq] - Pst.matsEG[Piece.kind capturedPieceAtTo]

        // 5. Place the piece at the destination
        match Move.kind m with
        | 5 ->   // Promotion
            let promoType = Move.promo m
            let promotedPiece = (b.SideToMove <<< 3) ||| promoType

            newBitboards <- BitboardSet.togglePiece promotedPiece toSq newBitboards
            newHash <- newHash ^^^ Zobrist.getPieceKey promotedPiece toSq
            if b.SideToMove = Colour.White then
                newScoreMG <- newScoreMG + Pst.MG[promoType].[toSq] + Pst.matsMG[promoType] - Pst.matsMG[Piece.kind movingPiece]
                newScoreEG <- newScoreEG + Pst.EG[promoType].[toSq] + Pst.matsEG[promoType] - Pst.matsEG[Piece.kind movingPiece]
            else
                let idx = toSq ^^^ 56
                newScoreMG <- newScoreMG - Pst.MG[promoType].[idx] - Pst.matsMG[promoType] + Pst.matsMG[Piece.kind movingPiece]
                newScoreEG <- newScoreEG - Pst.EG[promoType].[idx] - Pst.matsEG[promoType] + Pst.matsEG[Piece.kind movingPiece]

        | _ ->
            newBitboards <- BitboardSet.togglePiece movingPiece toSq newBitboards
            newHash <- newHash ^^^ Zobrist.getPieceKey movingPiece toSq
            if b.SideToMove = Colour.White then
                newScoreMG <- newScoreMG + Pst.MG[Piece.kind movingPiece].[toSq]
                newScoreEG <- newScoreEG + Pst.EG[Piece.kind movingPiece].[toSq]
            else
                let idx = toSq ^^^ 56
                newScoreMG <- newScoreMG - Pst.MG[Piece.kind movingPiece].[idx]
                newScoreEG <- newScoreEG - Pst.EG[Piece.kind movingPiece].[idx]

        // 6. Handle Special Rook Moves (Castling)
        match Move.kind m with
        | 3 ->   // CastleKingSide        
            let (rSrc, rDst) = if b.SideToMove = Colour.White then (7, 5) else (63, 61)
            let rook = (b.SideToMove <<< 3) ||| PieceType.Rook
            newBitboards <- BitboardSet.togglePiece rook rSrc newBitboards
            newBitboards <- BitboardSet.togglePiece rook rDst newBitboards
            newHash <- newHash ^^^ (Zobrist.getPieceKey rook rSrc) ^^^ (Zobrist.getPieceKey rook rDst)
            if b.SideToMove = Colour.White then
                newScoreMG <- newScoreMG - Pst.MG[PieceType.Rook].[rSrc]
                newScoreEG <- newScoreEG - Pst.EG[PieceType.Rook].[rSrc]
                newScoreMG <- newScoreMG + Pst.MG[PieceType.Rook].[rDst]
                newScoreEG <- newScoreEG + Pst.EG[PieceType.Rook].[rDst]
            else
                let sidx = rSrc ^^^ 56
                newScoreMG <- newScoreMG + Pst.MG[PieceType.Rook].[sidx]
                newScoreEG <- newScoreEG + Pst.EG[PieceType.Rook].[sidx]
                let didx = rDst ^^^ 56
                newScoreMG <- newScoreMG - Pst.MG[PieceType.Rook].[didx]
                newScoreEG <- newScoreEG - Pst.EG[PieceType.Rook].[didx]

        | 4 ->   // CastleQueenSide ->
            let (rSrc, rDst) = if b.SideToMove = Colour.White then (0, 3) else (56, 59)
            let rook = (b.SideToMove <<< 3) ||| PieceType.Rook
            newBitboards <- BitboardSet.togglePiece rook rSrc newBitboards
            newBitboards <- BitboardSet.togglePiece rook rDst newBitboards
            newHash <- newHash ^^^ (Zobrist.getPieceKey rook rSrc) ^^^ (Zobrist.getPieceKey rook rDst)
            if b.SideToMove = Colour.White then
                newScoreMG <- newScoreMG - Pst.MG[PieceType.Rook].[rSrc]
                newScoreEG <- newScoreEG - Pst.EG[PieceType.Rook].[rSrc]
                newScoreMG <- newScoreMG + Pst.MG[PieceType.Rook].[rDst]
                newScoreEG <- newScoreEG + Pst.EG[PieceType.Rook].[rDst]
            else
                let sidx = rSrc ^^^ 56
                newScoreMG <- newScoreMG + Pst.MG[PieceType.Rook].[sidx]
                newScoreEG <- newScoreEG + Pst.EG[PieceType.Rook].[sidx]
                let didx = rDst ^^^ 56
                newScoreMG <- newScoreMG - Pst.MG[PieceType.Rook].[didx]
                newScoreEG <- newScoreEG - Pst.EG[PieceType.Rook].[didx]

        | _ -> ()

        // 7. Update Castling Rights
        // Rights are lost if King moves, or if Rooks move/are captured
        let mutable newCR = b.CastlingRights
        if Piece.kind movingPiece = PieceType.King then
            if b.SideToMove = Colour.White then
                newCR <- newCR &&& ~~~(CastlingRights.WK ||| CastlingRights.WQ)
            else
                newCR <- newCR &&& ~~~(CastlingRights.BK ||| CastlingRights.BQ)

        // If a rook moves from its starting square
        if fromSq = 0 then newCR <- newCR &&& ~~~(CastlingRights.WQ)
        if fromSq = 7 then newCR <- newCR &&& ~~~(CastlingRights.WK)
        if fromSq = 56 then newCR <- newCR &&& ~~~(CastlingRights.BQ)
        if fromSq = 63 then newCR <- newCR &&& ~~~(CastlingRights.BK)

        // If a rook is captured on its starting square
        if toSq = 0 then newCR <- newCR &&& ~~~(CastlingRights.WQ)
        if toSq = 7 then newCR <- newCR &&& ~~~(CastlingRights.WK)
        if toSq = 56 then newCR <- newCR &&& ~~~(CastlingRights.BQ)
        if toSq = 63 then newCR <- newCR &&& ~~~(CastlingRights.BK)

        // 8. Update En Passant Square
        // Only set if a pawn moves two squares
        let newEPSquare =
            if isPawn && abs (toSq - fromSq) = 16 then
                (fromSq + toSq) / 2
            else -1

        // 9. Update Clocks
        let newHMClock =
            if isPawn || capturedPieceAtTo <> -1 then 0
            else b.HalfmoveClock + 1
        
        let newFMNumber =
            if b.SideToMove = Colour.Black then b.FullmoveNumber + 1
            else b.FullmoveNumber

        // 10. Finalize Hash (XOR in new Castling and new EP)
        newHash <- newHash ^^^ (Zobrist.getCastlingKey newCR)
        newHash <- newHash ^^^ (Zobrist.getEnPassantKey newEPSquare)

        { Bitboards = newBitboards
          SideToMove = opponent
          CastlingRights = newCR
          EnPassantSquare = newEPSquare
          HalfmoveClock = newHMClock
          FullmoveNumber = newFMNumber
          ScoreMG = newScoreMG
          ScoreEG = newScoreEG
          Hash = newHash }

    /// Executes a null move, updating side to move, en passant, halfmove clock, fullmove number, and hash.
    let applyNullMove (b: Board) =
        let opponent = Colour.opposite b.SideToMove
        let mutable newHash = b.Hash
        
        newHash <- newHash ^^^ Zobrist.Table.SideToMove
        newHash <- newHash ^^^ (Zobrist.getEnPassantKey b.EnPassantSquare)
        
        let newFMNumber =
            if b.SideToMove = Colour.Black then b.FullmoveNumber + 1
            else b.FullmoveNumber

        { Bitboards = b.Bitboards
          SideToMove = opponent
          CastlingRights = b.CastlingRights
          EnPassantSquare = -1
          HalfmoveClock = b.HalfmoveClock + 1
          FullmoveNumber = newFMNumber
          ScoreMG = b.ScoreMG
          ScoreEG = b.ScoreEG
          Hash = newHash }

    /// Checks if the board has insufficient material for checkmate.
    let hasInsufficientMaterial (b: Board) =
        let bbs = b.Bitboards
        if bbs.WhitePawns <> 0uL || bbs.BlackPawns <> 0uL || 
           bbs.WhiteRooks <> 0uL || bbs.BlackRooks <> 0uL || 
           bbs.WhiteQueens <> 0uL || bbs.BlackQueens <> 0uL then
            false
        else
            let whiteKnights = Bitboard.count bbs.WhiteKnights
            let blackKnights = Bitboard.count bbs.BlackKnights
            let whiteBishops = Bitboard.count bbs.WhiteBishops
            let blackBishops = Bitboard.count bbs.BlackBishops
            
            let whiteMinors = whiteKnights + whiteBishops
            let blackMinors = blackKnights + blackBishops
            
            if whiteMinors = 0 && blackMinors = 0 then true // K vs K
            elif whiteMinors = 1 && blackMinors = 0 then true // KN vs K or KB vs K
            elif whiteMinors = 0 && blackMinors = 1 then true // K vs KN or K vs KB
            elif whiteKnights = 0 && blackKnights = 0 then
                // Only Bishops left. Draw if all bishops are on the same color.
                let whiteSquaresMask = 0xAA55AA55AA55AA55uL
                let allBishops = bbs.WhiteBishops ||| bbs.BlackBishops
                let onWhite = (allBishops &&& whiteSquaresMask) <> 0uL
                let onBlack = (allBishops &&& ~~~whiteSquaresMask) <> 0uL
                not (onWhite && onBlack) 
            else false

    /// Prints the board in a human-readable format.
    let prettyPrint (b: Board) =
        for r in 7..-1..0 do
            printf "%d " (r + 1)
            for f in 0..7 do
                // Use the bitboard-powered tryGetPiece
                let p = tryGetPiece b (Square.ofFileRank f r)
                if p <> -1 then
                    printf "%c " (Piece.toChar p)
                else
                    printf ". "
            printfn ""
        printfn "  a b c d e f g h"

    /// Packs a position and its metadata into a 32-byte MarlinRecord for Bullet
    let toMarlinRecord (b: Board) (searchScore: int) (finalResult: float) =
        let mutable r = MarlinRecord()
        
        // 1. Bitboards
        r.Occupancy <- b.Bitboards.Occupancy
        r.WhitePieces <- b.Bitboards.WhiteTotal
        
        // 2. Score: Bullet expects the score relative to WHITE.
        // Most engines return scores relative to SideToMove (Internal Perspective).
        let whiteRelativeScore = 
            if b.SideToMove = Colour.White then searchScore else -searchScore
        
        // Clamp score to int16 range to avoid overflow
        r.Score <- int16 (Math.Clamp(whiteRelativeScore, -32767, 32767))
        
        // 3. Side to Move (0=W, 1=B)
        r.SideToMove <- if b.SideToMove = Colour.White then 0uy else 1uy
        
        // 4. Result (0=B Win, 1=Draw, 2=W Win)
        r.Result <- 
            if finalResult = 1.0 then 2uy      // White Win
            elif finalResult = 0.0 then 0uy    // Black Win
            else 1uy                           // Draw
            
        // 5. King Squares (Using .NET BitOperations for speed)
        r.WhiteKingSq <- byte (BitOperations.TrailingZeroCount b.Bitboards.WhiteKings)
        r.BlackKingSq <- byte (BitOperations.TrailingZeroCount b.Bitboards.BlackKings)
        
        // 6. Metadata
        r.FullMove <- uint16 b.FullmoveNumber
        r
