module Floundroid

open System
open System.Text

// --- CORE TYPES ---

type Colour =
    | White
    | Black

module Colour =
    /// Converts a Colour to its character representation ('w' for White, 'b' for Black).
    let toChar =
        function
        | White -> 'w'
        | Black -> 'b'

    /// Converts a character to a Colour ('w' for White, 'b' for Black).
    let fromChar =
        function
        | 'w'
        | 'W' -> White
        | 'b'
        | 'B' -> Black
        | c -> failwithf "Invalid colour char: %c" c

    /// Returns the opposite colour.
    let opposite =
        function
        | White -> Black
        | Black -> White

type File =
    | A
    | B
    | C
    | D
    | E
    | F
    | G
    | H

module File =
    /// Converts a File to its integer representation (0-7).
    let toInt =
        function
        | A -> 0
        | B -> 1
        | C -> 2
        | D -> 3
        | E -> 4
        | F -> 5
        | G -> 6
        | H -> 7

    /// Converts an integer to a File (0-7).
    let fromInt =
        function
        | 0 -> A
        | 1 -> B
        | 2 -> C
        | 3 -> D
        | 4 -> E
        | 5 -> F
        | 6 -> G
        | 7 -> H
        | i -> invalidArg "i" $"File index {i} out of range (0-7)"

    /// Converts a File to its character representation ('a'-'h').
    let toChar f = "abcdefgh".[toInt f]

    /// Converts a character to a File ('a'-'h').
    let fromChar =
        function
        | 'a' -> A
        | 'b' -> B
        | 'c' -> C
        | 'd' -> D
        | 'e' -> E
        | 'f' -> F
        | 'g' -> G
        | 'h' -> H
        | c -> invalidArg "c" $"{c}"

type Rank =
    | R1
    | R2
    | R3
    | R4
    | R5
    | R6
    | R7
    | R8

module Rank =
    /// Converts a Rank to its integer representation (0-7).
    let toInt =
        function
        | R1 -> 0
        | R2 -> 1
        | R3 -> 2
        | R4 -> 3
        | R5 -> 4
        | R6 -> 5
        | R7 -> 6
        | R8 -> 7

    /// Converts an integer to a Rank (0-7).
    let fromInt =
        function
        | 0 -> R1
        | 1 -> R2
        | 2 -> R3
        | 3 -> R4
        | 4 -> R5
        | 5 -> R6
        | 6 -> R7
        | 7 -> R8
        | i -> invalidArg "i" $"Rank index {i} out of range (0-7)"

    /// Converts a Rank to its character representation ('1'-'8').
    let toChar r = "12345678".[toInt r]

    /// Converts a character to a Rank ('1'-'8').
    let fromChar =
        function
        | '1' -> R1
        | '2' -> R2
        | '3' -> R3
        | '4' -> R4
        | '5' -> R5
        | '6' -> R6
        | '7' -> R7
        | '8' -> R8
        | c -> invalidArg "c" $"{c}"

type Square = int

module Square =
    /// Converts a File and Rank to a Square.
    let ofFileRank (f: File) (r: Rank) : Square = Rank.toInt r * 8 + File.toInt f

    /// Gets the File of a Square.
    let file (sq: Square) : File = File.fromInt (sq % 8)

    /// Gets the Rank of a Square.
    let rank (sq: Square) : Rank = Rank.fromInt (sq / 8)

    /// Converts a Square to its string representation.
    let toString (sq: Square) : string =
        $"{File.toChar (file sq)}{Rank.toChar (rank sq)}"

    /// Converts a string representation of a square (e.g., "d4") to a Square.
    let fromString (s: string) : Square =
        ofFileRank (File.fromChar s.[0]) (Rank.fromChar s.[1])

    /// Checks if a square is on the board.
    let isOnBoard (f: int) (r: int) = f >= 0 && f < 8 && r >= 0 && r < 8

type PieceType =
    | Pawn
    | Knight
    | Bishop
    | Rook
    | Queen
    | King

module PieceType =
    /// Converts a PieceType to its character representation ('p', 'n', 'b', 'r', 'q', 'k').
    let toChar =
        function
        | Pawn -> 'p'
        | Knight -> 'n'
        | Bishop -> 'b'
        | Rook -> 'r'
        | Queen -> 'q'
        | King -> 'k'

    /// Converts a character to a PieceType.
    let fromChar =
        function
        | 'p'
        | 'P' -> Pawn
        | 'n'
        | 'N' -> Knight
        | 'b'
        | 'B' -> Bishop
        | 'r'
        | 'R' -> Rook
        | 'q'
        | 'Q' -> Queen
        | 'k'
        | 'K' -> King
        | c -> invalidArg "c" $"{c}"

type Piece = { Colour: Colour; Kind: PieceType }

module Piece =
    /// Converts a Piece to its character representation (uppercase for White, lowercase for Black).
    let toChar (p: Piece) =
        let c = PieceType.toChar p.Kind in if p.Colour = White then Char.ToUpper c else c

    /// Converts a character to a Piece, determining colour from case (uppercase = White, lowercase = Black).
    let fromChar c =
        { Colour = (if Char.IsUpper c then White else Black)
          Kind = PieceType.fromChar c }

type CastlingRights =
    { WhiteKingSide: bool
      WhiteQueenSide: bool
      BlackKingSide: bool
      BlackQueenSide: bool }

module CastlingRights =
    let none =
        { WhiteKingSide = false
          WhiteQueenSide = false
          BlackKingSide = false
          BlackQueenSide = false }

    /// Converts a string representation of castling rights to a CastlingRights value.
    let fromString (s: string) =
        if s = "-" then
            none
        else
            { WhiteKingSide = s.Contains "K"
              WhiteQueenSide = s.Contains "Q"
              BlackKingSide = s.Contains "k"
              BlackQueenSide = s.Contains "q" }

    /// Converts a CastlingRights value to its string representation.
    let toString cr =
        let sb = StringBuilder()

        if cr.WhiteKingSide then
            sb.Append("K") |> ignore

        if cr.WhiteQueenSide then
            sb.Append("Q") |> ignore

        if cr.BlackKingSide then
            sb.Append("k") |> ignore

        if cr.BlackQueenSide then
            sb.Append("q") |> ignore

        if sb.Length = 0 then "-" else sb.ToString()

type MoveKind =
    | Quiet
    | Capture
    | Promotion of PieceType
    | EnPassant
    | CastleKingSide
    | CastleQueenSide

type Move =
    { From: Square
      To: Square
      Kind: MoveKind }

module Move =
    /// Converts a Move to its UCI string representation.
    let toUci (m: Move) =
        let baseStr = Square.toString m.From + Square.toString m.To

        match m.Kind with
        | Promotion pt -> baseStr + (PieceType.toChar pt |> string)
        | _ -> baseStr

    /// Converts a UCI string representation of a move to a Move value.
    let fromUci (s: string) =
        if s.Length < 4 then
            invalidArg "s" "UCI move string too short"

        let fromSq = Square.fromString (s.Substring(0, 2))
        let toSq = Square.fromString (s.Substring(2, 2))

        let kind =
            if s.Length = 5 then
                Promotion(PieceType.fromChar s.[4])
            else
                Quiet

        { From = fromSq
          To = toSq
          Kind = kind }

type Board =
    { Pieces: Map<Square, Piece>
      SideToMove: Colour
      CastlingRights: CastlingRights
      EnPassantSquare: Square option
      HalfmoveClock: int
      FullmoveNumber: int }

// --- ATTACK DETECTION ---

module Attack =
    /// Checks if a square is attacked by the specified colour.
    let isSquareAttacked (b: Board) (sq: Square) (attacker: Colour) =
        let f, r = Square.file sq |> File.toInt, Square.rank sq |> Rank.toInt
        let them = attacker

        // Pawn attacks
        let pawnDir = if them = White then -1 else 1
        let mutable found = false

        for df in [ -1; 1 ] do
            let nf, nr = f + df, r + pawnDir

            if Square.isOnBoard nf nr then
                let s2 = Square.ofFileRank (File.fromInt nf) (Rank.fromInt nr)

                match b.Pieces |> Map.tryFind s2 with
                | Some p when p.Colour = them && p.Kind = Pawn -> found <- true
                | _ -> ()

        if found then
            true
        else
            // Knight
            let knightOffsets =
                [ (1, 2); (1, -2); (-1, 2); (-1, -2); (2, 1); (2, -1); (-2, 1); (-2, -1) ]

            let knightHit =
                knightOffsets
                |> List.exists (fun (df, dr) ->
                    let nf, nr = f + df, r + dr

                    if Square.isOnBoard nf nr then
                        let s2 = Square.ofFileRank (File.fromInt nf) (Rank.fromInt nr)

                        match b.Pieces |> Map.tryFind s2 with
                        | Some p when p.Colour = them && p.Kind = Knight -> true
                        | _ -> false
                    else
                        false)

            if knightHit then
                true
            else
                // Sliding
                let checkDir dirs =
                    dirs
                    |> List.exists (fun (df, dr) ->
                        let mutable nf, nr = f + df, r + dr
                        let mutable hit, blocked = false, false

                        while Square.isOnBoard nf nr && not blocked do
                            let s2 = Square.ofFileRank (File.fromInt nf) (Rank.fromInt nr)

                            match b.Pieces |> Map.tryFind s2 with
                            | None ->
                                nf <- nf + df
                                nr <- nr + dr
                            | Some p ->
                                blocked <- true

                                if p.Colour = them then
                                    match p.Kind with
                                    | Queen -> hit <- true
                                    | Rook when df = 0 || dr = 0 -> hit <- true
                                    | Bishop when df <> 0 && dr <> 0 -> hit <- true
                                    | _ -> ()

                        hit)

                if checkDir [ (1, 1); (1, -1); (-1, 1); (-1, -1); (1, 0); (-1, 0); (0, 1); (0, -1) ] then
                    true
                else
                    // King
                    let kingOffsets =
                        [ (1, 1); (1, -1); (-1, 1); (-1, -1); (1, 0); (-1, 0); (0, 1); (0, -1) ]

                    kingOffsets
                    |> List.exists (fun (df, dr) ->
                        let nf, nr = f + df, r + dr

                        if Square.isOnBoard nf nr then
                            let s2 = Square.ofFileRank (File.fromInt nf) (Rank.fromInt nr)

                            match b.Pieces |> Map.tryFind s2 with
                            | Some p when p.Colour = them && p.Kind = King -> true
                            | _ -> false
                        else
                            false)

// --- BOARD UTILS & FEN ---

module Board =
    let empty =
        { Pieces = Map.empty
          SideToMove = White
          CastlingRights = CastlingRights.none
          EnPassantSquare = None
          HalfmoveClock = 0
          FullmoveNumber = 1 }

    /// Tries to get a piece from a square.
    let tryGetPiece (b: Board) (sq: Square) = b.Pieces |> Map.tryFind sq
    /// Checks if a square is occupied.
    let isOccupied (b: Board) (sq: Square) = b.Pieces |> Map.containsKey sq

    /// Sets a piece on a square.
    let setPiece (b: Board) (sq: Square) (pOpt: Piece option) =
        match pOpt with
        | Some p -> { b with Pieces = b.Pieces.Add(sq, p) }
        | None -> { b with Pieces = b.Pieces.Remove sq }

    /// Parses a FEN string and returns a Board record representing the position.
    let fromFen (fen: string) =
        let parts = fen.Split(' ')
        let rows = parts.[0].Split('/')
        let mutable pieces = Map.empty

        for r in 0..7 do
            let rank, file = 7 - r, ref 0

            for char in rows.[r] do
                if Char.IsDigit char then
                    file.Value <- file.Value + (int char - int '0')
                else
                    let sq = Square.ofFileRank (File.fromInt file.Value) (Rank.fromInt rank)
                    pieces <- pieces.Add(sq, Piece.fromChar char)
                    file.Value <- file.Value + 1

        { Pieces = pieces
          SideToMove = Colour.fromChar parts.[1].[0]
          CastlingRights = CastlingRights.fromString parts.[2]
          EnPassantSquare =
            if parts.[3] = "-" then
                None
            else
                Some(Square.fromString parts.[3])
          HalfmoveClock = int parts.[4]
          FullmoveNumber = int parts.[5] }

    /// Converts a Board record to its FEN string representation.
    let toFen (b: Board) =
        let sb = StringBuilder()

        for r in 7..-1..0 do
            let mutable emptyCount = 0

            for f in 0..7 do
                let sq = Square.ofFileRank (File.fromInt f) (Rank.fromInt r)

                match b.Pieces |> Map.tryFind sq with
                | Some p ->
                    if emptyCount > 0 then
                        sb.Append(emptyCount) |> ignore

                    emptyCount <- 0
                    sb.Append(Piece.toChar p) |> ignore
                | None -> emptyCount <- emptyCount + 1

            if emptyCount > 0 then
                sb.Append(emptyCount) |> ignore

            if r > 0 then
                sb.Append('/') |> ignore

        sprintf
            "%O %c %s %s %d %d"
            sb
            (Colour.toChar b.SideToMove)
            (CastlingRights.toString b.CastlingRights)
            (match b.EnPassantSquare with
             | Some s -> Square.toString s
             | None -> "-")
            b.HalfmoveClock
            b.FullmoveNumber

    /// <summary>
    /// Checks if a player is in check.
    /// </summary>
    /// <param name="colour">The colour of the player to check.</param>
    /// <param name="b">The current game state.</param>
    /// <returns>True if the player is in check, false otherwise.</returns>
    let isInCheckFor (colour: Colour) (b: Board) =
        let kingSq =
            b.Pieces
            |> Seq.tryFind (fun (KeyValue(_, p)) -> p.Colour = colour && p.Kind = King)

        match kingSq with
        | None -> false
        | Some(KeyValue(ks, _)) -> Attack.isSquareAttacked b ks (Colour.opposite colour)

    /// Checks if the side to move is currently in check.
    let isInCheck (b: Board) = isInCheckFor b.SideToMove b

    /// Gets a map of pinned pieces and the squares they are pinned to.
    let getPins (b: Board) =
        let us, them = b.SideToMove, Colour.opposite b.SideToMove

        let kingSq =
            b.Pieces
            |> Seq.tryFind (fun (KeyValue(_, p)) -> p.Colour = us && p.Kind = King)
            |> Option.map (fun (KeyValue(sq, _)) -> sq)
            |> Option.defaultValue -1

        if kingSq = -1 then
            Map.empty
        else
            let kf, kr = Square.file kingSq |> File.toInt, Square.rank kingSq |> Rank.toInt

            let directions =
                [ (1, 0); (-1, 0); (0, 1); (0, -1); (1, 1); (1, -1); (-1, 1); (-1, -1) ]

            let pins = ResizeArray<int * (int list)>()

            for (df, dr) in directions do
                let mutable nf, nr, blocker, doneDir = kf + df, kr + dr, None, false

                while Square.isOnBoard nf nr && not doneDir do
                    let sq = Square.ofFileRank (File.fromInt nf) (Rank.fromInt nr)

                    match b.Pieces |> Map.tryFind sq with
                    | None ->
                        nf <- nf + df
                        nr <- nr + dr
                    | Some p when p.Colour = us ->
                        if blocker.IsNone then
                            blocker <- Some sq
                            nf <- nf + df
                            nr <- nr + dr
                        else
                            doneDir <- true
                    | Some p when p.Colour = them ->
                        let isSlider =
                            match p.Kind with
                            | Rook -> df = 0 || dr = 0
                            | Bishop -> df <> 0 && dr <> 0
                            | Queen -> true
                            | _ -> false

                        if isSlider && blocker.IsSome then
                            let mutable ray, rf, rr, rayDone = [], kf + df, kr + dr, false

                            while Square.isOnBoard rf rr && not rayDone do
                                let rsq = Square.ofFileRank (File.fromInt rf) (Rank.fromInt rr)
                                ray <- rsq :: ray

                                if rsq = sq then
                                    rayDone <- true

                                rf <- rf + df
                                rr <- rr + dr

                            pins.Add(blocker.Value, List.rev ray)

                        doneDir <- true
                    | _ -> doneDir <- true

            pins |> Map.ofSeq

    /// <summary>
    /// Executes a move on the board and returns a new immutable board state.
    /// Updates castling rights, en passant targets, and move clocks.
    /// </summary>
    /// <param name="m">The validated move to apply.</param>
    /// <param name="b">The current game state.</param>
    /// <returns>A new Board record reflecting the post-move state.</returns>
    let applyMove (m: Move) (b: Board) =
        match b.Pieces |> Map.tryFind m.From with
        | None -> b
        | Some piece ->
            let us, them = b.SideToMove, Colour.opposite b.SideToMove
            let mutable newPieces = b.Pieces.Remove(m.From)

            let pieceToPlace =
                match m.Kind with
                | Promotion pt -> { Colour = us; Kind = pt }
                | _ -> piece

            match m.Kind with
            | EnPassant ->
                let capturedSq =
                    if us = White then
                        Square.ofFileRank (Square.file m.To) (Rank.fromInt (Rank.toInt (Square.rank m.To) - 1))
                    else
                        Square.ofFileRank (Square.file m.To) (Rank.fromInt (Rank.toInt (Square.rank m.To) + 1))

                newPieces <- newPieces.Remove(capturedSq)
            | CastleKingSide ->
                let rR = if us = White then Rank.R1 else Rank.R8
                let rF, rT = Square.ofFileRank File.H rR, Square.ofFileRank File.F rR

                if newPieces.ContainsKey rF then
                    let rk = newPieces.[rF] in newPieces <- newPieces.Remove(rF).Add(rT, rk)
            | CastleQueenSide ->
                let rR = if us = White then Rank.R1 else Rank.R8
                let rF, rT = Square.ofFileRank File.A rR, Square.ofFileRank File.D rR

                if newPieces.ContainsKey rF then
                    let rk = newPieces.[rF] in newPieces <- newPieces.Remove(rF).Add(rT, rk)
            | _ -> ()

            newPieces <- newPieces.Add(m.To, pieceToPlace)

            let updateRights (cr: CastlingRights) =
                let mutable r = cr

                // King moved
                if piece.Kind = King then
                    if us = White then
                        r <-
                            { r with
                                WhiteKingSide = false
                                WhiteQueenSide = false }
                    else
                        r <-
                            { r with
                                BlackKingSide = false
                                BlackQueenSide = false }

                // Rook moved from its home square
                let revokeForSquare sq cur =
                    match Square.file sq, Square.rank sq with
                    | File.A, Rank.R1 -> { cur with WhiteQueenSide = false }
                    | File.H, Rank.R1 -> { cur with WhiteKingSide = false }
                    | File.A, Rank.R8 -> { cur with BlackQueenSide = false }
                    | File.H, Rank.R8 -> { cur with BlackKingSide = false }
                    | _ -> cur

                r <- revokeForSquare m.From r

                // Rook captured on its home square
                match b.Pieces.TryFind m.To with
                | Some captured when captured.Kind = Rook -> r <- revokeForSquare m.To r
                | _ -> ()

                r

            let nextEp =
                if
                    piece.Kind = Pawn
                    && Math.Abs(Rank.toInt (Square.rank m.From) - Rank.toInt (Square.rank m.To)) = 2
                then
                    Some(Square.ofFileRank (Square.file m.From) (if us = White then Rank.R3 else Rank.R6))
                else
                    None

            { b with
                Pieces = newPieces
                SideToMove = them
                CastlingRights = updateRights b.CastlingRights
                EnPassantSquare = nextEp
                HalfmoveClock =
                    if piece.Kind = Pawn || b.Pieces.ContainsKey m.To then
                        0
                    else
                        b.HalfmoveClock + 1
                FullmoveNumber =
                    if us = Black then
                        b.FullmoveNumber + 1
                    else
                        b.FullmoveNumber }

    /// Prints the board in a human-readable format.
    let prettyPrint (b: Board) =
        for r in 7..-1..0 do
            printf "%d " (r + 1)

            for f in 0..7 do
                match b.Pieces |> Map.tryFind (Square.ofFileRank (File.fromInt f) (Rank.fromInt r)) with
                | Some p -> printf "%c " (Piece.toChar p)
                | None -> printf ". "

            printfn ""

        printfn "  a b c d e f g h"

// --- MOVE GENERATION ---

module MoveGen =
    let dirs =
        Map
            [ Bishop, [ (1, 1); (1, -1); (-1, 1); (-1, -1) ]
              Rook, [ (1, 0); (-1, 0); (0, 1); (0, -1) ]
              Queen, [ (1, 1); (1, -1); (-1, 1); (-1, -1); (1, 0); (-1, 0); (0, 1); (0, -1) ]
              Knight, [ (1, 2); (1, -2); (-1, 2); (-1, -2); (2, 1); (2, -1); (-2, 1); (-2, -1) ]
              King, [ (1, 1); (1, -1); (-1, 1); (-1, -1); (1, 0); (-1, 0); (0, 1); (0, -1) ] ]

    /// Gets all pseudo-legal moves for the current position.
    let getPseudoLegalMoves (b: Board) =
        let moves = ResizeArray<Move>()
        let us, them = b.SideToMove, Colour.opposite b.SideToMove

        for (KeyValue(sq, p)) in b.Pieces do
            if p.Colour = us then
                let f, r = Square.file sq |> File.toInt, Square.rank sq |> Rank.toInt

                match p.Kind with
                | Pawn ->
                    let d = if us = White then 1 else -1

                    // Pushes
                    let nr1 = r + d

                    if nr1 >= 0 && nr1 <= 7 then
                        let p1 = Square.ofFileRank (File.fromInt f) (Rank.fromInt nr1)

                        if not (b.Pieces.ContainsKey p1) then

                            // Promotion push
                            if nr1 = (if us = White then 7 else 0) then
                                for pt in [ Queen; Rook; Bishop; Knight ] do
                                    moves.Add(
                                        { From = sq
                                          To = p1
                                          Kind = Promotion pt }
                                    )
                            else
                                moves.Add({ From = sq; To = p1; Kind = Quiet })

                            // Double push from starting rank
                            if r = (if us = White then 1 else 6) then
                                let nr2 = r + 2 * d

                                if nr2 >= 0 && nr2 <= 7 then
                                    let p2 = Square.ofFileRank (File.fromInt f) (Rank.fromInt nr2)

                                    if not (b.Pieces.ContainsKey p2) then
                                        moves.Add({ From = sq; To = p2; Kind = Quiet })

                    // Captures
                    for df in [ -1; 1 ] do
                        let nf, nr = f + df, r + d

                        if Square.isOnBoard nf nr then
                            let cap = Square.ofFileRank (File.fromInt nf) (Rank.fromInt nr)

                            match b.Pieces.TryFind cap with
                            | Some victim when victim.Colour = them ->
                                if nr = (if us = White then 7 else 0) then
                                    for pt in [ Queen; Rook; Bishop; Knight ] do
                                        moves.Add(
                                            { From = sq
                                              To = cap
                                              Kind = Promotion pt }
                                        )
                                else
                                    moves.Add({ From = sq; To = cap; Kind = Capture })
                            | _ -> ()

                    // En passant
                    match b.EnPassantSquare with
                    | Some ep ->
                        if abs (File.toInt (Square.file ep) - f) = 1 && Rank.toInt (Square.rank ep) = r + d then
                            moves.Add({ From = sq; To = ep; Kind = EnPassant })
                    | None -> ()
                | Knight
                | King ->
                    for (df, dr) in dirs.[p.Kind] do
                        let nf, nr = f + df, r + dr

                        if Square.isOnBoard nf nr then
                            let t = Square.ofFileRank (File.fromInt nf) (Rank.fromInt nr)

                            match b.Pieces |> Map.tryFind t with
                            | Some target ->
                                if target.Colour = them then
                                    moves.Add({ From = sq; To = t; Kind = Capture })
                            | None -> moves.Add({ From = sq; To = t; Kind = Quiet })

                    if p.Kind = King then
                        let rnk, cr = (if us = White then 0 else 7), b.CastlingRights

                        if (us = White && cr.WhiteKingSide) || (us = Black && cr.BlackKingSide) then
                            let f1, g1 =
                                Square.ofFileRank File.F (Rank.fromInt rnk), Square.ofFileRank File.G (Rank.fromInt rnk)

                            if not (b.Pieces.ContainsKey f1) && not (b.Pieces.ContainsKey g1) then
                                moves.Add(
                                    { From = sq
                                      To = g1
                                      Kind = CastleKingSide }
                                )

                        if (us = White && cr.WhiteQueenSide) || (us = Black && cr.BlackQueenSide) then
                            let d1, c1, b1 =
                                Square.ofFileRank File.D (Rank.fromInt rnk),
                                Square.ofFileRank File.C (Rank.fromInt rnk),
                                Square.ofFileRank File.B (Rank.fromInt rnk)

                            if
                                not (b.Pieces.ContainsKey d1)
                                && not (b.Pieces.ContainsKey c1)
                                && not (b.Pieces.ContainsKey b1)
                            then
                                moves.Add(
                                    { From = sq
                                      To = c1
                                      Kind = CastleQueenSide }
                                )
                | _ ->
                    for (df, dr) in dirs.[p.Kind] do
                        let mutable nf, nr, blocked = f + df, r + dr, false

                        while Square.isOnBoard nf nr && not blocked do
                            let t = Square.ofFileRank (File.fromInt nf) (Rank.fromInt nr)

                            match b.Pieces |> Map.tryFind t with
                            | Some target ->
                                if target.Colour = them then
                                    moves.Add({ From = sq; To = t; Kind = Capture })
                                    blocked <- true
                                else
                                    blocked <- true
                            | None ->
                                moves.Add({ From = sq; To = t; Kind = Quiet })
                                nf <- nf + df
                                nr <- nr + dr

        moves.ToArray()

    /// Gets all legal moves for the current position.
    let getLegalMoves (b: Board) =
        let us, them = b.SideToMove, Colour.opposite b.SideToMove

        getPseudoLegalMoves b
        |> Array.filter (fun m ->
            let castlingCheck =
                match m.Kind with
                | CastleKingSide
                | CastleQueenSide ->
                    if Board.isInCheckFor us b then
                        false
                    else
                        let rnk = if us = White then Rank.R1 else Rank.R8

                        let midFile = if m.Kind = CastleKingSide then File.F else File.D

                        let destFile = if m.Kind = CastleKingSide then File.G else File.C

                        let midSquare = Square.ofFileRank midFile rnk

                        let destSquare = Square.ofFileRank destFile rnk

                        not (Attack.isSquareAttacked b midSquare them)
                        && not (Attack.isSquareAttacked b destSquare them)
                | _ -> true

            castlingCheck && not (Board.isInCheckFor us (Board.applyMove m b)))

module San =
    /// Converts a move to Standard Algebraic Notation (SAN) based on the current board state.
    let toSan (b: Board) (m: Move) =
        match m.Kind with
        | CastleKingSide -> "O-O"
        | CastleQueenSide -> "O-O-O"
        | _ ->
            let piece = b.Pieces.[m.From]

            let isCapture =
                match m.Kind with
                | Capture
                | EnPassant -> true
                | _ -> b.Pieces.ContainsKey m.To

            let nextBoard = Board.applyMove m b
            let isCheck = Board.isInCheck nextBoard
            let isMate = isCheck && (MoveGen.getLegalMoves nextBoard).Length = 0

            let moveStr =
                if piece.Kind = Pawn then
                    let prefix =
                        if isCapture then
                            sprintf "%cx" (File.toChar (Square.file m.From))
                        else
                            ""

                    let prom =
                        match m.Kind with
                        | Promotion pt -> sprintf "=%c" (Char.ToUpper(PieceType.toChar pt))
                        | _ -> ""

                    sprintf "%s%s%s" prefix (Square.toString m.To) prom
                else
                    let pChar = Char.ToUpper(PieceType.toChar piece.Kind)

                    let others =
                        MoveGen.getLegalMoves b
                        |> Array.filter (fun alt ->
                            let altPiece = b.Pieces.[alt.From]
                            alt.From <> m.From && alt.To = m.To && altPiece.Kind = piece.Kind)

                    let disambiguator =
                        if others.Length = 0 then
                            ""
                        else
                            let sameFile =
                                others |> Array.exists (fun alt -> Square.file alt.From = Square.file m.From)

                            let sameRank =
                                others |> Array.exists (fun alt -> Square.rank alt.From = Square.rank m.From)

                            if not sameFile then
                                sprintf "%c" (File.toChar (Square.file m.From))
                            elif not sameRank then
                                sprintf "%c" (Rank.toChar (Square.rank m.From))
                            else
                                Square.toString m.From

                    let cap = if isCapture then "x" else ""
                    sprintf "%c%s%s%s" pChar disambiguator cap (Square.toString m.To)

            let suffix =
                if isMate then "#"
                elif isCheck then "+"
                else ""

            moveStr + suffix

// --- PERFT, DEBUG& UCI ---
type PerftSuiteItem =
    { Name: string
      Fen: string
      Expected: uint64 list }

module Perft =
    /// Counts the number of leaf nodes at a given depth from the current board state.
    let rec countNodes depth b =
        if depth = 0 then
            1uL
        else
            let moves = MoveGen.getLegalMoves b

            if depth = 1 then
                uint64 moves.Length
            else
                let mutable total = 0uL

                for i in 0 .. moves.Length - 1 do
                    total <- total + countNodes (depth - 1) (Board.applyMove moves.[i] b)

                total

    /// Divides the perft calculation for a given depth and board state.
    let divide depth b =
        let sw = Diagnostics.Stopwatch.StartNew()
        let moves = MoveGen.getLegalMoves b |> Array.sortBy Move.toUci
        let mutable total = 0uL

        printfn "Perft results for depth %d:" depth

        for m in moves do
            let n = countNodes (depth - 1) (Board.applyMove m b)
            // Use the San module we just built!
            printfn "%s (%s): %d" (Move.toUci m) (San.toSan b m) n
            total <- total + n

        sw.Stop()
        let ms = sw.ElapsedMilliseconds
        let nps = if ms > 0L then (total * 1000uL) / uint64 ms else 0uL

        printfn "\nTotal: %d | Time: %d ms | NPS: %d" total ms nps
        total

    let suites =
        [ { Name = "Initial Position"
            Fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
            Expected = [ 1uL; 20uL; 400uL; 8902uL; 197281uL; 4865609uL; 119060324uL ] }

          { Name = "Kiwipete"
            Fen = "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1"
            Expected = [ 1uL; 48uL; 2039uL; 97862uL; 4085603uL; 193690690uL ] }

          { Name = "Endgame/EP"
            Fen = "8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1"
            Expected = [ 1uL; 14uL; 191uL; 2812uL; 43238uL; 674624uL ] }

          { Name = "Promotion Stress Test"
            Fen = "rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8"
            Expected = [ 1uL; 44uL; 1486uL; 62379uL; 2103487uL; 89941194uL ] } ]

    /// Runs the full perft suite up to a specified maximum depth, comparing results against expected values.
    let runFullSuite (maxDepth: int) =
        printfn "Starting Perft Regression Suite (Max Depth: %d)" maxDepth
        printfn "------------------------------------------------"
        let totalSw = Diagnostics.Stopwatch.StartNew()

        for suite in suites do
            printfn "Testing: %s" suite.Name
            let b = Board.fromFen suite.Fen
            let depthsToTest = Math.Min(maxDepth, suite.Expected.Length - 1)

            for d in 1..depthsToTest do
                let expected = suite.Expected.[d]
                let sw = Diagnostics.Stopwatch.StartNew()
                let actual = countNodes d b
                sw.Stop()

                if actual = expected then
                    printfn "  Depth %d: PASS (%d nodes) in %dms" d actual sw.ElapsedMilliseconds
                else
                    printfn "  Depth %d: FAILED! Expected %d, got %d" d expected actual

            printfn ""

        totalSw.Stop()
        printfn "Full Suite Finished in %d ms" totalSw.ElapsedMilliseconds

module Debug =
    /// 1.5.1 - Move list visualisation
    let displayMoves (b: Board) =
        let moves = MoveGen.getLegalMoves b
        printfn "Legal Moves (%d):" moves.Length

        let formatted =
            moves
            |> Array.map (fun m -> sprintf "%s (%s)" (Move.toUci m) (San.toSan b m))
            |> String.concat ", "

        printfn "%s" formatted

    /// 1.5.2 - Board consistency checker
    let verify (b: Board) =
        let errors = ResizeArray<string>()
        let pieces = b.Pieces |> Map.toList |> List.map snd

        // 1. Check Kings
        let whiteKings =
            pieces
            |> List.filter (fun p -> p.Colour = White && p.Kind = King)
            |> List.length

        let blackKings =
            pieces
            |> List.filter (fun p -> p.Colour = Black && p.Kind = King)
            |> List.length

        if whiteKings <> 1 then
            errors.Add(sprintf "Invalid White King count: %d" whiteKings)

        if blackKings <> 1 then
            errors.Add(sprintf "Invalid Black King count: %d" blackKings)

        // 2. Check Pawns
        for (KeyValue(sq, p)) in b.Pieces do
            if p.Kind = Pawn then
                let r = Square.rank sq |> Rank.toInt

                if r = 0 || r = 7 then
                    errors.Add(sprintf "Pawn on illegal rank %d at %s" (r + 1) (Square.toString sq))

        // 3. Side not to move cannot be in check
        if Board.isInCheckFor (Colour.opposite b.SideToMove) b then
            errors.Add("Illegal state: Side NOT to move is in check.")

        if errors.Count = 0 then
            printfn "Board state is consistent."
        else
            printfn "CONSISTENCY ERRORS FOUND:"

            for err in errors do
                printfn " - %s" err

    /// 1.5.3 - Attack map visualiser
    let displayAttackMap (b: Board) (attacker: Colour) =
        printfn "Attack Map for %A:" attacker

        for r in 7..-1..0 do
            printf "%d " (r + 1)

            for f in 0..7 do
                let sq = Square.ofFileRank (File.fromInt f) (Rank.fromInt r)

                if Attack.isSquareAttacked b sq attacker then
                    printf "x "
                else
                    printf ". "

            printfn ""

        printfn "  a b c d e f g h"

module UciLoop =
    let startFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
    let mutable board = Board.fromFen startFen

    let rec run () =
        let line = Console.ReadLine()

        if line <> null then
            let ts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries) |> Array.toList

            match ts with
            | "uci" :: _ ->
                printfn "id name Floundroid"
                printfn "id author Phil Brooks"
                printfn "uciok"
            | "isready" :: _ -> printfn "readyok"
            | "position" :: rest ->
                let (fen, moveParts) =
                    match rest with
                    | "startpos" :: "moves" :: m -> (startFen, m)
                    | "startpos" :: _ -> (startFen, [])
                    | "fen" :: fParts ->
                        // Find where "moves" starts, if it exists
                        let movesIdx = fParts |> List.tryFindIndex (fun s -> s = "moves")

                        match movesIdx with
                        | Some i ->
                            let f = fParts |> List.take i |> String.concat " "
                            let m = fParts |> List.skip (i + 1)
                            (f, m)
                        | None -> (String.concat " " fParts, [])
                    | _ -> (startFen, [])

                board <- Board.fromFen fen

                for mStr in moveParts do
                    let legalMoves = MoveGen.getLegalMoves board

                    match legalMoves |> Array.tryFind (fun m -> Move.toUci m = mStr) with
                    | Some m -> board <- Board.applyMove m board
                    | None -> ()

            | "go" :: _ ->
                // Stage 2 logic: Pick the first move available
                let moves = MoveGen.getLegalMoves board

                if moves.Length > 0 then
                    printfn "bestmove %s" (Move.toUci moves.[0])
                else
                    // This case handles checkmate/stalemate
                    printfn "bestmove (none)"

            | "perft" :: rest ->
                match rest with
                | "suite" :: d :: _ ->
                    match Int32.TryParse d with
                    | true, depth -> Perft.runFullSuite depth
                    | _ -> printfn "Invalid depth: %s" d
                | "suite" :: _ -> Perft.runFullSuite 4
                | d :: _ ->
                    match Int32.TryParse d with
                    | true, depth -> Perft.divide depth board |> ignore
                    | _ -> printfn "Invalid depth: %s" d
                | [] ->
                    // Handles just the word "perft"
                    Perft.divide 1 board |> ignore

            | "print" :: _ -> Board.prettyPrint board

            | "moves" :: _ -> Debug.displayMoves board
            | "verify" :: _ -> Debug.verify board
            | "attacks" :: "white" :: _ -> Debug.displayAttackMap board White
            | "attacks" :: "black" :: _ -> Debug.displayAttackMap board Black
            | "quit" :: _ -> Environment.Exit(0)
            | _ -> ()

            run ()

[<EntryPoint>]
let main _ =
    UciLoop.run ()
    0
