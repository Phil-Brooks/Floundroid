namespace Floundroid

open System
open FSharp.NativeInterop

module MoveGen =
    let dirs =
        Map
            [ PieceType.Bishop, [ (1, 1); (1, -1); (-1, 1); (-1, -1) ]
              PieceType.Rook, [ (1, 0); (-1, 0); (0, 1); (0, -1) ]
              PieceType.Queen, [ (1, 1); (1, -1); (-1, 1); (-1, -1); (1, 0); (-1, 0); (0, 1); (0, -1) ]
              PieceType.Knight, [ (1, 2); (1, -2); (-1, 2); (-1, -2); (2, 1); (2, -1); (-2, 1); (-2, -1) ]
              PieceType.King, [ (1, 1); (1, -1); (-1, 1); (-1, -1); (1, 0); (-1, 0); (0, 1); (0, -1) ] ]

    /// Generates pseudo-legal moves for White pieces, including pawns, knights, bishops, rooks, and queens.
    let getpseudoW (b:Board) (span: Span<int>) = 
        #nowarn "9"
        let mutable ct = 0
        
        //let getpseudoWP () = 
        let mutable bb = b.Bitboards.WhitePawns
        while bb <> 0uL do
            let sq = Bitboard.popLsb &bb
            let f, r = Square.file sq, Square.rank sq
            // 1. Single Push
            let nr1 = r + 1
            if nr1 >= 0 && nr1 <= 7 then
                let p1 = Square.ofFileRank f nr1
                if not (Board.isOccupied b p1) then
                    // Promotion push
                    if nr1 = 7 then
                        for pt = 4 downto 1 do
                            span[ct] <- Move.create(sq, p1, 5, pt)
                            ct <- ct + 1
                    else
                        span[ct] <- Move.create(sq, p1, 0, 0)
                        ct <- ct + 1
                    // 2. Double push from starting rank
                    if r = 1 then
                        let nr2 = r + 2
                        let p2 = Square.ofFileRank f nr2
                        if not (Board.isOccupied b p2) then
                            span[ct] <- Move.create(sq, p2, 0, 0)
                            ct <- ct + 1
                // 3. Captures
                for df in [| -1; 1 |] do
                    let nf, nr = f + df, r + 1
                    if Square.isOnBoard nf nr then
                        let cap = Square.ofFileRank nf nr
                        if Board.isOccupiedB b cap then
                            if nr = 7 then
                                for pt = 4 downto 1 do
                                    span[ct] <- Move.create(sq, cap, 5, pt)
                                    ct <- ct + 1
                            else
                                span[ct] <- Move.create(sq, cap, 1, 0)
                                ct <- ct + 1
                // 4. En passant
                let ep = b.EnPassantSquare
                if ep <> -1 && abs ((Square.file ep) - f) = 1 && Square.rank ep = r + 1 then
                    span[ct] <- Move.create(sq, ep, 2, 0)
                    ct <- ct + 1
        //let getpseudoWN () = 
        let mutable bb = b.Bitboards.WhiteKnights
        while bb <> 0uL do
            let sq = Bitboard.popLsb &bb
            // Use high-speed Bitboard lookup
            let mutable attacks = BitboardGen.knightAttacks.[sq]
            while attacks <> 0uL do
                let t = Bitboard.popLsb &attacks
                if Board.isOccupiedB b t then
                    span[ct] <- Move.create(sq, t, 1, 0)
                    ct <- ct + 1
                elif not (Board.isOccupied b t) then
                    span[ct] <- Move.create(sq, t, 0, 0)
                    ct <- ct + 1
        //let getpseudoWB () = 
        let mutable bb = b.Bitboards.WhiteBishops
        while bb <> 0uL do
            let sq = Bitboard.popLsb &bb
            let occ = b.Bitboards.Occupancy
            let e = Magic.bishopEntries.[sq]
            let combinedAttacks = Magic.bishopTable.[e.Offset + Magic.getIndex occ e.Mask]
            let mutable targets = combinedAttacks &&& ~~~b.Bitboards.WhiteTotal
            while targets <> 0uL do
                let t = Bitboard.popLsb &targets
                if Board.isOccupied b t then 
                    span[ct] <- Move.create(sq, t, 1, 0)
                    ct <- ct + 1
                else 
                    span[ct] <- Move.create(sq, t, 0, 0)
                    ct <- ct + 1
        //let getpseudoWR () = 
        let mutable bb = b.Bitboards.WhiteRooks
        while bb <> 0uL do
            let sq = Bitboard.popLsb &bb
            let occ = b.Bitboards.Occupancy
            let e = Magic.rookEntries.[sq]
            let combinedAttacks = Magic.rookTable.[e.Offset + Magic.getIndex occ e.Mask]
            let mutable targets = combinedAttacks &&& ~~~b.Bitboards.WhiteTotal
            while targets <> 0uL do
                let t = Bitboard.popLsb &targets
                if Board.isOccupied b t then 
                    span[ct] <- Move.create(sq, t, 1, 0)
                    ct <- ct + 1
                else 
                    span[ct] <- Move.create(sq, t, 0, 0)
                    ct <- ct + 1
        //let getpseudoWQ () = 
        let mutable bb = b.Bitboards.WhiteQueens
        while bb <> 0uL do
            let sq = Bitboard.popLsb &bb
            let occ = b.Bitboards.Occupancy
            let e = Magic.bishopEntries.[sq]
            let mutable combinedAttacks = Magic.bishopTable.[e.Offset + Magic.getIndex occ e.Mask]
            let er = Magic.rookEntries.[sq]
            combinedAttacks <- combinedAttacks ||| Magic.rookTable.[er.Offset + Magic.getIndex occ er.Mask]
            let mutable targets = combinedAttacks &&& ~~~b.Bitboards.WhiteTotal
            while targets <> 0uL do
                let t = Bitboard.popLsb &targets
                if Board.isOccupied b t then 
                    span[ct] <- Move.create(sq, t, 1, 0)
                    ct <- ct + 1
                else 
                    span[ct] <- Move.create(sq, t, 0, 0)
                    ct <- ct + 1
        //let getpseudoWK () = 
        let mutable bb = b.Bitboards.WhiteKings
        while bb <> 0uL do
            let sq = Bitboard.popLsb &bb
            // Use high-speed Bitboard lookup
            let mutable attacks = BitboardGen.kingAttacks.[sq]
            while attacks <> 0uL do
                let t = Bitboard.popLsb &attacks
                if Board.isOccupiedB b t then
                    span[ct] <- Move.create(sq, t, 1, 0)
                    ct <- ct + 1
                elif not (Board.isOccupied b t) then
                    span[ct] <- Move.create(sq, t, 0, 0)
                    ct <- ct + 1

            // Castling
            let rnk, cr = 0, b.CastlingRights
            if (cr &&& CastlingRights.WK) <> 0 then
                let f1, g1 = Square.ofFileRank File.F rnk, Square.ofFileRank File.G rnk
                if not (Board.isOccupied b f1) && not (Board.isOccupied b g1) then
                    span[ct] <- Move.create(sq, g1, 3, 0)
                    ct <- ct + 1

            if (cr &&& CastlingRights.WQ) <> 0 then
                let d1, c1, b1 = Square.ofFileRank File.D rnk,Square.ofFileRank File.C rnk, Square.ofFileRank File.B rnk
                if not (Board.isOccupied b d1) && not (Board.isOccupied b c1) && not (Board.isOccupied b b1) then
                    span[ct] <- Move.create(sq, c1, 4, 0)
                    ct <- ct + 1
        ct

    /// Generates all pseudo-legal moves for Black pieces on the board.
    let getpseudoB (b:Board) (span: Span<int>) = 
        #nowarn "9"
        let mutable ct = 0

        //let getpseudoBP () = 
        let mutable bb = b.Bitboards.BlackPawns
        while bb <> 0uL do
            let sq = Bitboard.popLsb &bb
            let f, r = Square.file sq, Square.rank sq
            // 1. Single Push
            let nr1 = r - 1
            if nr1 >= 0 && nr1 <= 7 then
                let p1 = Square.ofFileRank f nr1
                if not (Board.isOccupied b p1) then
                    // Promotion push
                    if nr1 = 0 then
                        for pt = 4 downto 1 do
                            span[ct] <- Move.create(sq, p1, 5, pt)
                            ct <- ct + 1
                    else
                        span[ct] <- Move.create(sq, p1, 0, 0)
                        ct <- ct + 1
                    // 2. Double push from starting rank
                    if r = 6 then
                        let nr2 = r - 2
                        let p2 = Square.ofFileRank f nr2
                        if not (Board.isOccupied b p2) then
                            span[ct] <- Move.create(sq, p2, 0, 0)
                            ct <- ct + 1
                // 3. Captures
                for df in [| -1; 1 |] do
                    let nf, nr = f + df, r - 1
                    if Square.isOnBoard nf nr then
                        let cap = Square.ofFileRank nf nr
                        if Board.isOccupiedW b cap then
                            if nr = 0 then
                                for pt = 4 downto 1 do
                                    span[ct] <- Move.create(sq, cap, 5, pt)
                                    ct <- ct + 1
                            else
                                span[ct] <- Move.create(sq, cap, 1, 0)
                                ct <- ct + 1
                // 4. En passant
                let ep = b.EnPassantSquare
                if ep <> -1 && abs ((Square.file ep) - f) = 1 && Square.rank ep = r - 1 then
                    span[ct] <- Move.create(sq, ep, 2, 0)
                    ct <- ct + 1
        //let getpseudoBN () = 
        let mutable bb = b.Bitboards.BlackKnights
        while bb <> 0uL do
            let sq = Bitboard.popLsb &bb
            // Use high-speed Bitboard lookup
            let mutable attacks = BitboardGen.knightAttacks.[sq]
            while attacks <> 0uL do
                let t = Bitboard.popLsb &attacks
                if Board.isOccupiedW b t then
                    span[ct] <- Move.create(sq, t, 1, 0)
                    ct <- ct + 1
                elif not (Board.isOccupied b t) then
                    span[ct] <- Move.create(sq, t, 0, 0)
                    ct <- ct + 1
        //let getpseudoBB () = 
        let mutable bb = b.Bitboards.BlackBishops
        while bb <> 0uL do
            let sq = Bitboard.popLsb &bb
            let occ = b.Bitboards.Occupancy
            let e = Magic.bishopEntries.[sq]
            let combinedAttacks = Magic.bishopTable.[e.Offset + Magic.getIndex occ e.Mask]
            let mutable targets = combinedAttacks &&& ~~~b.Bitboards.BlackTotal
            while targets <> 0uL do
                let t = Bitboard.popLsb &targets
                if Board.isOccupied b t then 
                    span[ct] <- Move.create(sq, t, 1, 0)
                    ct <- ct + 1
                else 
                    span[ct] <- Move.create(sq, t, 0, 0)
                    ct <- ct + 1
        //let getpseudoBR () = 
        let mutable bb = b.Bitboards.BlackRooks
        while bb <> 0uL do
            let sq = Bitboard.popLsb &bb
            let occ = b.Bitboards.Occupancy
            let e = Magic.rookEntries.[sq]
            let combinedAttacks = Magic.rookTable.[e.Offset + Magic.getIndex occ e.Mask]
            let mutable targets = combinedAttacks &&& ~~~b.Bitboards.BlackTotal
            while targets <> 0uL do
                let t = Bitboard.popLsb &targets
                if Board.isOccupied b t then 
                    span[ct] <- Move.create(sq, t, 1, 0)
                    ct <- ct + 1
                else 
                    span[ct] <- Move.create(sq, t, 0, 0)
                    ct <- ct + 1
        //let getpseudoBQ () = 
        let mutable bb = b.Bitboards.BlackQueens
        while bb <> 0uL do
            let sq = Bitboard.popLsb &bb
            let occ = b.Bitboards.Occupancy
            let e = Magic.bishopEntries.[sq]
            let mutable combinedAttacks = Magic.bishopTable.[e.Offset + Magic.getIndex occ e.Mask]
            let er = Magic.rookEntries.[sq]
            combinedAttacks <- combinedAttacks ||| Magic.rookTable.[er.Offset + Magic.getIndex occ er.Mask]
            let mutable targets = combinedAttacks &&& ~~~b.Bitboards.BlackTotal
            while targets <> 0uL do
                let t = Bitboard.popLsb &targets
                if Board.isOccupied b t then 
                    span[ct] <- Move.create(sq, t, 1, 0)
                    ct <- ct + 1
                else 
                    span[ct] <- Move.create(sq, t, 0, 0)
                    ct <- ct + 1
        //let getpseudoBK () = 
        let mutable bb = b.Bitboards.BlackKings
        while bb <> 0uL do
            let sq = Bitboard.popLsb &bb
            // Use high-speed Bitboard lookup
            let mutable attacks = BitboardGen.kingAttacks.[sq]
            while attacks <> 0uL do
                let t = Bitboard.popLsb &attacks
                if Board.isOccupiedW b t then
                    span[ct] <- Move.create(sq, t, 1, 0)
                    ct <- ct + 1
                elif not (Board.isOccupied b t) then
                    span[ct] <- Move.create(sq, t, 0, 0)
                    ct <- ct + 1

            // Castling
            let rnk, cr = 7, b.CastlingRights
            if (cr &&& CastlingRights.BK) <> 0 then
                let f8, g8 = Square.ofFileRank File.F rnk, Square.ofFileRank File.G rnk
                if not (Board.isOccupied b f8) && not (Board.isOccupied b g8) then
                    span[ct] <- Move.create(sq, g8, 3, 0)
                    ct <- ct + 1

            if (cr &&& CastlingRights.BQ) <> 0 then
                let d8, c8, b8 = Square.ofFileRank File.D rnk,Square.ofFileRank File.C rnk, Square.ofFileRank File.B rnk
                if not (Board.isOccupied b d8) && not (Board.isOccupied b c8) && not (Board.isOccupied b b8) then
                    span[ct] <- Move.create(sq, c8, 4, 0)
                    ct <- ct + 1
        ct
    
    // Gets all pseudo-legal moves for the current position using Bitboards.
    let getPseudoLegalMoves (b: Board) (span: Span<int>) =
        if b.SideToMove = Colour.White then
            getpseudoW b span
        else 
            getpseudoB b span

    /// Gets all legal moves for the current position.
    let getLegalMoves (b: Board) =
        let us, them = b.SideToMove, Colour.opposite b.SideToMove
        let movePtr = NativePtr.stackalloc<int> 256
        let moveSpan = Span<int>(NativePtr.toVoidPtr movePtr, 256)
        let moveCount = getPseudoLegalMoves b moveSpan
        let psmoves = moveSpan.Slice(0, moveCount).ToArray()
        psmoves
        |> Array.filter (fun m ->
            let castlingCheck =
                match Move.kind m with
                | 3
                | 4 ->
                    if Board.isInCheckFor us b then
                        false
                    else
                        let rnk = if us = Colour.White then Rank.R1 else Rank.R8

                        let midFile = if Move.kind m = 3 then File.F else File.D

                        let destFile = if Move.kind m = 3 then File.G else File.C

                        let midSquare = Square.ofFileRank midFile rnk

                        let destSquare = Square.ofFileRank destFile rnk

                        not (Board.isSquareAttacked b midSquare them)
                        && not (Board.isSquareAttacked b destSquare them)
                | _ -> true

            castlingCheck && not (Board.isInCheckFor us (Board.applyMove m b)))

    /// Generates all capture moves for White pieces, including pawns, knights, bishops, rooks, and queens.
    let getcapW (b:Board) (span: Span<int>) = 
        #nowarn "9"
        let mutable ct = 0

        // getcapWP  
        let mutable bb = b.Bitboards.WhitePawns
        while bb <> 0uL do
            let sq = Bitboard.popLsb &bb
            let f, r = Square.file sq, Square.rank sq
            // 1. Single Push
            let nr1 = r + 1
            if nr1 >= 0 && nr1 <= 7 then
                let p1 = Square.ofFileRank f nr1
                if not (Board.isOccupied b p1) then
                    // Promotion push
                    if nr1 = 7 then
                        for pt = 4 downto 1 do
                            span[ct] <- Move.create(sq, p1, 5, pt)
                            ct <- ct + 1
                // 3. Captures
                for df in [| -1; 1 |] do
                    let nf, nr = f + df, r + 1
                    if Square.isOnBoard nf nr then
                        let cap = Square.ofFileRank nf nr
                        if Board.isOccupiedB b cap then
                            if nr = 7 then
                                for pt = 4 downto 1 do
                                    span[ct] <- Move.create(sq, cap, 5, pt)
                                    ct <- ct + 1
                            else
                                span[ct] <- Move.create(sq, cap, 1, 0)
                                ct <- ct + 1
                // 4. En passant
                let ep = b.EnPassantSquare
                if ep <> -1 && abs ((Square.file ep) - f) = 1 && Square.rank ep = r + 1 then
                    span[ct] <- Move.create(sq, ep, 2, 0)
                    ct <- ct + 1
        //let getcapWN () = 
        let mutable bb = b.Bitboards.WhiteKnights
        while bb <> 0uL do
            let sq = Bitboard.popLsb &bb
            // Use high-speed Bitboard lookup
            let mutable attacks = BitboardGen.knightAttacks.[sq]
            while attacks <> 0uL do
                let t = Bitboard.popLsb &attacks
                if Board.isOccupiedB b t then
                    span[ct] <- Move.create(sq, t, 1, 0)
                    ct <- ct + 1
        //let getcapWB () = 
        let mutable bb = b.Bitboards.WhiteBishops
        while bb <> 0uL do
            let sq = Bitboard.popLsb &bb
            let occ = b.Bitboards.Occupancy
            let e = Magic.bishopEntries.[sq]
            let combinedAttacks = Magic.bishopTable.[e.Offset + Magic.getIndex occ e.Mask]
            let mutable targets = combinedAttacks &&& ~~~b.Bitboards.WhiteTotal
            while targets <> 0uL do
                let t = Bitboard.popLsb &targets
                if Board.isOccupied b t then
                    span[ct] <- Move.create(sq, t, 1, 0)
                    ct <- ct + 1
        //let getcapWR () = 
        let mutable bb = b.Bitboards.WhiteRooks
        while bb <> 0uL do
            let sq = Bitboard.popLsb &bb
            let occ = b.Bitboards.Occupancy
            let e = Magic.rookEntries.[sq]
            let combinedAttacks = Magic.rookTable.[e.Offset + Magic.getIndex occ e.Mask]
            let mutable targets = combinedAttacks &&& ~~~b.Bitboards.WhiteTotal
            while targets <> 0uL do
                let t = Bitboard.popLsb &targets
                if Board.isOccupied b t then 
                    span[ct] <- Move.create(sq, t, 1, 0)
                    ct <- ct + 1
        //let getcapWQ () = 
        let mutable bb = b.Bitboards.WhiteQueens
        while bb <> 0uL do
            let sq = Bitboard.popLsb &bb
            let occ = b.Bitboards.Occupancy
            let e = Magic.bishopEntries.[sq]
            let mutable combinedAttacks = Magic.bishopTable.[e.Offset + Magic.getIndex occ e.Mask]
            let er = Magic.rookEntries.[sq]
            combinedAttacks <- combinedAttacks ||| Magic.rookTable.[er.Offset + Magic.getIndex occ er.Mask]
            let mutable targets = combinedAttacks &&& ~~~b.Bitboards.WhiteTotal
            while targets <> 0uL do
                let t = Bitboard.popLsb &targets
                if Board.isOccupied b t then 
                    span[ct] <- Move.create(sq, t, 1, 0)
                    ct <- ct + 1
        //let getcapWK () = 
        let mutable bb = b.Bitboards.WhiteKings
        while bb <> 0uL do
            let sq = Bitboard.popLsb &bb
            // Use high-speed Bitboard lookup
            let mutable attacks = BitboardGen.kingAttacks.[sq]
            while attacks <> 0uL do
                let t = Bitboard.popLsb &attacks
                if Board.isOccupiedB b t then
                    span[ct] <- Move.create(sq, t, 1, 0)
                    ct <- ct + 1
        ct
    
    /// Generates all capture moves for Black pieces, including pawns, knights, bishops, rooks, and queens.
    let getcapB (b:Board) (span: Span<int>) = 
        #nowarn "9"
        let mutable ct = 0

        //let getcapBP () = 
        let mutable bb = b.Bitboards.BlackPawns
        while bb <> 0uL do
            let sq = Bitboard.popLsb &bb
            let f, r = Square.file sq, Square.rank sq
            // 1. Single Push
            let nr1 = r - 1
            if nr1 >= 0 && nr1 <= 7 then
                let p1 = Square.ofFileRank f nr1
                if not (Board.isOccupied b p1) then
                    // Promotion push
                    if nr1 = 0 then
                        for pt = 4 downto 1 do
                            span[ct] <- Move.create(sq, p1, 5, pt)
                            ct <- ct + 1
                // 3. Captures
                for df in [| -1; 1 |] do
                    let nf, nr = f + df, r - 1
                    if Square.isOnBoard nf nr then
                        let cap = Square.ofFileRank nf nr
                        if Board.isOccupiedW b cap then
                            if nr = 0 then
                                for pt = 4 downto 1 do
                                    span[ct] <- Move.create(sq, cap, 5, pt)
                                    ct <- ct + 1
                            else
                                span[ct] <- Move.create(sq, cap, 1, 0)
                                ct <- ct + 1
                // 4. En passant
                let ep = b.EnPassantSquare
                if ep <> -1 && abs ((Square.file ep) - f) = 1 && Square.rank ep = r - 1 then
                    span[ct] <- Move.create(sq, ep, 2, 0)
                    ct <- ct + 1
        //let getcapBN () = 
        let mutable bb = b.Bitboards.BlackKnights
        while bb <> 0uL do
            let sq = Bitboard.popLsb &bb
            // Use high-speed Bitboard lookup
            let mutable attacks = BitboardGen.knightAttacks.[sq]
            while attacks <> 0uL do
                let t = Bitboard.popLsb &attacks
                if Board.isOccupiedW b t then
                    span[ct] <- Move.create(sq, t, 1, 0)
                    ct <- ct + 1
        //let getcapBB () = 
        let mutable bb = b.Bitboards.BlackBishops
        while bb <> 0uL do
            let sq = Bitboard.popLsb &bb
            let occ = b.Bitboards.Occupancy
            let e = Magic.bishopEntries.[sq]
            let combinedAttacks = Magic.bishopTable.[e.Offset + Magic.getIndex occ e.Mask]
            let mutable targets = combinedAttacks &&& ~~~b.Bitboards.BlackTotal
            while targets <> 0uL do
                let t = Bitboard.popLsb &targets
                if Board.isOccupied b t then 
                    span[ct] <- Move.create(sq, t, 1, 0)
                    ct <- ct + 1
        //let getcapBR () = 
        let mutable bb = b.Bitboards.BlackRooks
        while bb <> 0uL do
            let sq = Bitboard.popLsb &bb
            let occ = b.Bitboards.Occupancy
            let e = Magic.rookEntries.[sq]
            let combinedAttacks = Magic.rookTable.[e.Offset + Magic.getIndex occ e.Mask]
            let mutable targets = combinedAttacks &&& ~~~b.Bitboards.BlackTotal
            while targets <> 0uL do
                let t = Bitboard.popLsb &targets
                if Board.isOccupied b t then 
                    span[ct] <- Move.create(sq, t, 1, 0)
                    ct <- ct + 1
        //let getcapBQ () = 
        let mutable bb = b.Bitboards.BlackQueens
        while bb <> 0uL do
            let sq = Bitboard.popLsb &bb
            let occ = b.Bitboards.Occupancy
            let e = Magic.bishopEntries.[sq]
            let mutable combinedAttacks = Magic.bishopTable.[e.Offset + Magic.getIndex occ e.Mask]
            let er = Magic.rookEntries.[sq]
            combinedAttacks <- combinedAttacks ||| Magic.rookTable.[er.Offset + Magic.getIndex occ er.Mask]
            let mutable targets = combinedAttacks &&& ~~~b.Bitboards.BlackTotal
            while targets <> 0uL do
                let t = Bitboard.popLsb &targets
                if Board.isOccupied b t then 
                    span[ct] <- Move.create(sq, t, 1, 0)
                    ct <- ct + 1
        //let getcapBK () = 
        let mutable bb = b.Bitboards.BlackKings
        while bb <> 0uL do
            let sq = Bitboard.popLsb &bb
            // Use high-speed Bitboard lookup
            let mutable attacks = BitboardGen.kingAttacks.[sq]
            while attacks <> 0uL do
                let t = Bitboard.popLsb &attacks
                if Board.isOccupiedW b t then
                    span[ct] <- Move.create(sq, t, 1, 0)
                    ct <- ct + 1
        ct
    
    /// Optimized generator for Quiescence Search: Only returns Captures, En Passants, and Promotions.
    let getCaptureMoves (b: Board) (span: Span<int>) =
        if b.SideToMove = Colour.White then
            getcapW b span
        else
            getcapB b span
