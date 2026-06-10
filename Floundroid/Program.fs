open System
open System.Text

// --- CORE TYPES ---

type Colour = White | Black
module Colour =
    let toChar = function White -> 'w' | Black -> 'b'
    let fromChar = function 'w' | 'W' -> White | 'b' | 'B' -> Black | c -> failwithf "Invalid colour char: %c" c
    let opposite = function White -> Black | Black -> White

type File = A | B | C | D | E | F | G | H
module File =
    let toInt = function A -> 0 | B -> 1 | C -> 2 | D -> 3 | E -> 4 | F -> 5 | G -> 6 | H -> 7
    let fromInt = function 0 -> A | 1 -> B | 2 -> C | 3 -> D | 4 -> E | 5 -> F | 6 -> G | 7 -> H | _ -> invalidArg "i" "0-7"
    let toChar f = "abcdefgh".[toInt f]
    let fromChar = function 'a'->A|'b'->B|'c'->C|'d'->D|'e'->E|'f'->F|'g'->G|'h'->H | c -> invalidArg "c" $"{c}"

type Rank = R1 | R2 | R3 | R4 | R5 | R6 | R7 | R8
module Rank =
    let toInt = function R1 -> 0 | R2 -> 1 | R3 -> 2 | R4 -> 3 | R5 -> 4 | R6 -> 5 | R7 -> 6 | R8 -> 7
    let fromInt = function 0 -> R1 | 1 -> R2 | 2 -> R3 | 3 -> R4 | 4 -> R5 | 5 -> R6 | 6 -> R7 | 7 -> R8 | _ -> invalidArg "i" "0-7"
    let toChar r = "12345678".[toInt r]
    let fromChar = function '1'->R1|'2'->R2|'3'->R3|'4'->R4|'5'->R5|'6'->R6|'7'->R7|'8'->R8 | c -> invalidArg "c" $"{c}"

type CastlingRights = { WhiteKingSide: bool; WhiteQueenSide: bool; BlackKingSide: bool; BlackQueenSide: bool }
module CastlingRights =
    let none = { WhiteKingSide = false; WhiteQueenSide = false; BlackKingSide = false; BlackQueenSide = false }
    let fromString (s: string) =
        if s = "-" then none
        else { WhiteKingSide = s.Contains "K"; WhiteQueenSide = s.Contains "Q"; BlackKingSide = s.Contains "k"; BlackQueenSide = s.Contains "q" }
    let toString cr =
        let sb = StringBuilder()
        if cr.WhiteKingSide then sb.Append("K") |> ignore
        if cr.WhiteQueenSide then sb.Append("Q") |> ignore
        if cr.BlackKingSide then sb.Append("k") |> ignore
        if cr.BlackQueenSide then sb.Append("q") |> ignore
        if sb.Length = 0 then "-" else sb.ToString()

type PieceType = Pawn | Knight | Bishop | Rook | Queen | King
module PieceType =
    let toChar = function Pawn -> 'p' | Knight -> 'n' | Bishop -> 'b' | Rook -> 'r' | Queen -> 'q' | King -> 'k'
    let fromChar = function 'p'|'P'->Pawn|'n'|'N'->Knight|'b'|'B'->Bishop|'r'|'R'->Rook|'q'|'Q'->Queen|'k'|'K'->King|c -> invalidArg "c" $"{c}"

type Piece = { Colour : Colour; Kind : PieceType }
module Piece =
    let toChar (p: Piece) =
        let c = PieceType.toChar p.Kind
        if p.Colour = White then Char.ToUpper c else c
    let fromChar c =
        let colour = if Char.IsUpper c then White else Black
        { Colour = colour; Kind = PieceType.fromChar c }

type Square = int
module Square =
    let ofFileRank (file: File) (rank: Rank) : Square = Rank.toInt rank * 8 + File.toInt file
    let file (sq: Square) : File = File.fromInt (sq % 8)
    let rank (sq: Square) : Rank = Rank.fromInt (sq / 8)
    let toString (sq: Square) : string = $"{File.toChar (file sq)}{Rank.toChar (rank sq)}"
    let fromString (s: string) : Square = 
        if s.Length <> 2 then invalidArg "s" "Square must be 2 chars"
        ofFileRank (File.fromChar s.[0]) (Rank.fromChar s.[1])
    let isValid (sq: Square) = sq >= 0 && sq < 64
    let isOnBoard (f: int) (r: int) = f >= 0 && f < 8 && r >= 0 && r < 8

type MoveKind = Quiet | Capture | Promotion of PieceType | EnPassant | CastleKingSide | CastleQueenSide
type Move = { From : Square; To : Square; Kind : MoveKind }
module Move =
    let toUci (m: Move) =
        let baseStr = Square.toString m.From + Square.toString m.To
        match m.Kind with
        | Promotion pt -> baseStr + (PieceType.toChar pt |> string)
        | _ -> baseStr

    let fromUci (s: string) =
        if s.Length < 4 then invalidArg "s" "UCI move must be at least 4 characters"
        let fromSq = Square.fromString (s.Substring(0, 2))
        let toSq   = Square.fromString (s.Substring(2, 2))
        let kind = if s.Length = 5 then Promotion (PieceType.fromChar s[4]) else Quiet
        { From = fromSq; To = toSq; Kind = kind }

type Board =
    { Pieces : Map<Square, Piece>; SideToMove : Colour; CastlingRights : CastlingRights
      EnPassantSquare : Square option; HalfmoveClock : int; FullmoveNumber : int }

module Attack =

    /// Returns true if `sq` is attacked by `attacker` colour.
    let isSquareAttacked (b: Board) (sq: Square) (attacker: Colour) =
        let f = Square.file sq |> File.toInt
        let r = Square.rank sq |> Rank.toInt

        let them = attacker
        let us = Colour.opposite them

        // --- Pawn attacks ---
        let pawnDir = if them = White then -1 else 1
        for df in [-1; 1] do
            let nf, nr = f + df, r + pawnDir
            if Square.isOnBoard nf nr then
                let s2 = Square.ofFileRank (File.fromInt nf) (Rank.fromInt nr)
                match b.Pieces |> Map.tryFind s2 with
                | Some p when p.Colour = them && p.Kind = Pawn -> 
                    // Pawn attacks this square
                    true |> ignore
                | _ -> ()

        // If we found a pawn attack, return immediately
        let pawnHit =
            [-1; 1]
            |> List.exists (fun df ->
                let nf, nr = f + df, r + pawnDir
                if Square.isOnBoard nf nr then
                    let s2 = Square.ofFileRank (File.fromInt nf) (Rank.fromInt nr)
                    match b.Pieces |> Map.tryFind s2 with
                    | Some p when p.Colour = them && p.Kind = Pawn -> true
                    | _ -> false
                else false
            )
        if pawnHit then true else

        // --- Knight attacks ---
        let knightOffsets = [ (1,2); (1,-2); (-1,2); (-1,-2); (2,1); (2,-1); (-2,1); (-2,-1) ]
        let knightHit =
            knightOffsets
            |> List.exists (fun (df, dr) ->
                let nf, nr = f + df, r + dr
                if Square.isOnBoard nf nr then
                    let s2 = Square.ofFileRank (File.fromInt nf) (Rank.fromInt nr)
                    match b.Pieces |> Map.tryFind s2 with
                    | Some p when p.Colour = them && p.Kind = Knight -> true
                    | _ -> false
                else false
            )
        if knightHit then true else

        // --- Sliding attacks: bishop/rook/queen ---
        let bishopDirs = [ (1,1); (1,-1); (-1,1); (-1,-1) ]
        let rookDirs   = [ (1,0); (-1,0); (0,1); (0,-1) ]

        let slidingHit =
            let checkDir (df, dr) =
                let mutable nf, nr = f + df, r + dr
                let mutable hit = false
                let mutable blocked = false
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
                            | Bishop when List.contains (df,dr) bishopDirs -> hit <- true
                            | Rook   when List.contains (df,dr) rookDirs   -> hit <- true
                            | Queen -> hit <- true
                            | _ -> ()
                hit

            let bishopHit = bishopDirs |> List.exists checkDir
            let rookHit   = rookDirs   |> List.exists checkDir
            bishopHit || rookHit

        if slidingHit then true else

        // --- King attacks (adjacent squares) ---
        let kingOffsets = [ (1,1); (1,-1); (-1,1); (-1,-1); (1,0); (-1,0); (0,1); (0,-1) ]
        let kingHit =
            kingOffsets
            |> List.exists (fun (df, dr) ->
                let nf, nr = f + df, r + dr
                if Square.isOnBoard nf nr then
                    let s2 = Square.ofFileRank (File.fromInt nf) (Rank.fromInt nr)
                    match b.Pieces |> Map.tryFind s2 with
                    | Some p when p.Colour = them && p.Kind = King -> true
                    | _ -> false
                else false
            )

        kingHit

// --- BOARD UTILS & FEN ---

module Board =
    let empty = { Pieces = Map.empty; SideToMove = White; CastlingRights = CastlingRights.none; EnPassantSquare = None; HalfmoveClock = 0; FullmoveNumber = 1 }

    let tryGetPiece (b: Board) (sq: Square) = b.Pieces |> Map.tryFind sq
    let isOccupied (b: Board) (sq: Square) = b.Pieces |> Map.containsKey sq

    let setPiece (b: Board) (sq: Square) (pieceOpt: Piece option) =
        match pieceOpt with
        | Some piece -> { b with Pieces = b.Pieces.Add(sq, piece) }
        | None -> { b with Pieces = b.Pieces.Remove sq }

    let removePiece (b: Board) (sq: Square) = { b with Pieces = b.Pieces.Remove sq }

    let applyMove (m: Move) (b: Board) =
        match tryGetPiece b m.From with
        | None -> invalidArg "m" $"No piece on {Square.toString m.From}"
        | Some piece ->
            let b1 = removePiece b m.From
            let b2 =
                match m.Kind with
                | Promotion pt ->
                    let promoted = { Colour = piece.Colour; Kind = pt }
                    setPiece b1 m.To (Some promoted)
                | _ ->
                    setPiece b1 m.To (Some piece)
            let nextSide = Colour.opposite b.SideToMove
            { b2 with
                SideToMove = nextSide
                EnPassantSquare = None
                HalfmoveClock =
                    match piece.Kind, m.Kind with
                    | Pawn, _ -> 0
                    | _, Capture -> 0
                    | _ -> b.HalfmoveClock + 1
                FullmoveNumber =
                    if b.SideToMove = Black then b.FullmoveNumber + 1 else b.FullmoveNumber }

    /// Returns true if the side to move is currently in check.
    let isInCheck (b: Board) =
        // Find the king of the side to move
        let kingSq =
            b.Pieces
            |> Seq.tryFind (fun (KeyValue(_, p)) -> p.Colour = b.SideToMove && p.Kind = King)
            |> Option.map (fun (KeyValue(sq, _)) -> sq)

        match kingSq with
        | None -> false  // should never happen in a legal position
        | Some ks ->
            Attack.isSquareAttacked b ks (Colour.opposite b.SideToMove)
    
    /// Returns a map of pinned piece squares to the line of allowed movement.
    /// Key = pinned piece square
    /// Value = list of squares along the pin ray (including the king direction)
    let getPins (b: Board) =
        let us = b.SideToMove
        let them = Colour.opposite us

        // Find our king
        let kingSq =
            b.Pieces
            |> Seq.tryFind (fun (KeyValue(_, p)) -> p.Colour = us && p.Kind = King)
            |> Option.map (fun (KeyValue(sq, _)) -> sq)
            |> Option.defaultValue -1

        let kf = Square.file kingSq |> File.toInt
        let kr = Square.rank kingSq |> Rank.toInt

        // Directions a slider can pin along
        let directions =
            [ (1,0); (-1,0); (0,1); (0,-1)      // rook directions
              (1,1); (1,-1); (-1,1); (-1,-1) ] // bishop directions

        let pins = ResizeArray<_>()

        for (df, dr) in directions do
            let mutable nf = kf + df
            let mutable nr = kr + dr
            let mutable blockerSq = None
            let mutable doneDir = false

            while Square.isOnBoard nf nr && not doneDir do
                let sq = Square.ofFileRank (File.fromInt nf) (Rank.fromInt nr)
                match b.Pieces |> Map.tryFind sq with
                | None ->
                    nf <- nf + df
                    nr <- nr + dr

                | Some p when p.Colour = us ->
                    match blockerSq with
                    | None ->
                        blockerSq <- Some sq
                        nf <- nf + df
                        nr <- nr + dr
                    | Some _ ->
                        doneDir <- true

                | Some p when p.Colour = them ->
                    let isSlider =
                        match p.Kind with
                        | Rook when df = 0 || dr = 0 -> true
                        | Bishop when df <> 0 && dr <> 0 -> true
                        | Queen -> true
                        | _ -> false

                    match blockerSq, isSlider with
                    | Some bSq, true ->
                        pins.Add(bSq, (df, dr))
                    | _ -> ()

                    doneDir <- true

                | _ ->
                    doneDir <- true

        // Convert to map: pinnedPiece → allowed ray squares
        pins
        |> Seq.map (fun (sq, (df, dr)) ->
            let mutable ray = []
            let mutable nf = kf + df
            let mutable nr = kr + dr
            while Square.isOnBoard nf nr do
                ray <- Square.ofFileRank (File.fromInt nf) (Rank.fromInt nr) :: ray
                nf <- nf + df
                nr <- nr + dr
            sq, List.rev ray)
        |> Map.ofSeq
    
    
    let fromFen (fen: string) =
        let parts = fen.Split(' ')
        let rows = parts.[0].Split('/')
        let mutable pieces = Map.empty
        for r in 0 .. 7 do
            let rank = 7 - r
            let mutable file = 0
            for char in rows.[r] do
                if Char.IsDigit char then
                    file <- file + (int char - int '0')
                else
                    let sq = Square.ofFileRank (File.fromInt file) (Rank.fromInt rank)
                    pieces <- pieces.Add(sq, Piece.fromChar char)
                    file <- file + 1
        { Pieces = pieces
          SideToMove = Colour.fromChar parts.[1].[0]
          CastlingRights = CastlingRights.fromString parts.[2]
          EnPassantSquare = if parts.[3] = "-" then None else Some (Square.fromString parts.[3])
          HalfmoveClock = int parts.[4]
          FullmoveNumber = int parts.[5] }

    let toFen (b: Board) =
        let sb = StringBuilder()
        for r in 7 .. -1 .. 0 do
            let mutable emptyCount = 0
            for f in 0 .. 7 do
                match b.Pieces |> Map.tryFind (Square.ofFileRank (File.fromInt f) (Rank.fromInt r)) with
                | Some p -> 
                    if emptyCount > 0 then sb.Append(emptyCount) |> ignore
                    emptyCount <- 0
                    sb.Append(Piece.toChar p) |> ignore
                | None -> emptyCount <- emptyCount + 1
            if emptyCount > 0 then sb.Append(emptyCount) |> ignore
            if r > 0 then sb.Append('/') |> ignore
        sprintf "%O %c %s %s %d %d" sb (Colour.toChar b.SideToMove) (CastlingRights.toString b.CastlingRights) 
            (match b.EnPassantSquare with Some sq -> Square.toString sq | None -> "-") b.HalfmoveClock b.FullmoveNumber

    let prettyPrint (b: Board) =
        for r in 7 .. -1 .. 0 do
            printf "%d " (r + 1)
            for f in 0 .. 7 do
                let sq = Square.ofFileRank (File.fromInt f) (Rank.fromInt r)
                match b.Pieces |> Map.tryFind sq with
                | Some p -> printf "%c " (Piece.toChar p)
                | None -> printf ". "
            printfn ""
        printfn "  a b c d e f g h"

// --- MOVE GENERATION ---

module MoveGen =
    let directions = Map [
        Bishop, [ (1,1); (1,-1); (-1,1); (-1,-1) ]
        Rook,   [ (1,0); (-1,0); (0,1); (0,-1) ]
        Queen,  [ (1,1); (1,-1); (-1,1); (-1,-1); (1,0); (-1,0); (0,1); (0,-1) ]
        Knight, [ (1,2); (1,-2); (-1,2); (-1,-2); (2,1); (2,-1); (-2,1); (-2,-1) ]
        King,   [ (1,1); (1,-1); (-1,1); (-1,-1); (1,0); (-1,0); (0,1); (0,-1) ]
    ]

    let getPseudoLegalMoves (b: Board) =
        let moves = ResizeArray<Move>()
        let us = b.SideToMove
        let them = Colour.opposite us

        for (KeyValue(sq, p)) in b.Pieces do
            if p.Colour = us then
                let f, r = Square.file sq |> File.toInt, Square.rank sq |> Rank.toInt
                
                match p.Kind with
                | Knight ->
                    for (df, dr) in directions.[Knight] do
                        let nf, nr = f + df, r + dr
                        if Square.isOnBoard nf nr then
                            let targetSq = Square.ofFileRank (File.fromInt nf) (Rank.fromInt nr)
                            match b.Pieces |> Map.tryFind targetSq with
                            | Some target when target.Colour = us -> ()
                            | Some _ -> moves.Add({ From = sq; To = targetSq; Kind = Capture })
                            | None -> moves.Add({ From = sq; To = targetSq; Kind = Quiet })

                | King ->
                    // Normal king moves
                    for (df, dr) in directions.[King] do
                        let nf, nr = f + df, r + dr
                        if Square.isOnBoard nf nr then
                            let targetSq = Square.ofFileRank (File.fromInt nf) (Rank.fromInt nr)
                            match b.Pieces |> Map.tryFind targetSq with
                            | Some target when target.Colour = us -> ()
                            | Some _ -> moves.Add({ From = sq; To = targetSq; Kind = Capture })
                            | None -> moves.Add({ From = sq; To = targetSq; Kind = Quiet })

                    // --- Castling (pseudo-legal only) ---
                    let rank = if us = White then 0 else 7
                    let kingStart = Square.ofFileRank File.E (Rank.fromInt rank)

                    if sq = kingStart then
                        let cr = b.CastlingRights

                        // King-side castling
                        let canCastleKs =
                            (us = White && cr.WhiteKingSide) ||
                            (us = Black && cr.BlackKingSide)

                        if canCastleKs then
                            let f1 = Square.ofFileRank File.F (Rank.fromInt rank)
                            let g1 = Square.ofFileRank File.G (Rank.fromInt rank)
                            if not (Board.isOccupied b f1) && not (Board.isOccupied b g1) then
                                moves.Add({ From = sq; To = g1; Kind = CastleKingSide })

                        // Queen-side castling
                        let canCastleQs =
                            (us = White && cr.WhiteQueenSide) ||
                            (us = Black && cr.BlackQueenSide)

                        if canCastleQs then
                            let d1 = Square.ofFileRank File.D (Rank.fromInt rank)
                            let c1 = Square.ofFileRank File.C (Rank.fromInt rank)
                            let b1 = Square.ofFileRank File.B (Rank.fromInt rank)
                            if not (Board.isOccupied b d1)
                               && not (Board.isOccupied b c1)
                               && not (Board.isOccupied b b1) then
                                moves.Add({ From = sq; To = c1; Kind = CastleQueenSide })
 
                | Bishop | Rook | Queen ->
                    for (df, dr) in directions.[p.Kind] do
                        let mutable nf, nr = f + df, r + dr
                        let mutable blocked = false
                        while Square.isOnBoard nf nr && not blocked do
                            let targetSq = Square.ofFileRank (File.fromInt nf) (Rank.fromInt nr)
                            match b.Pieces |> Map.tryFind targetSq with
                            | Some target ->
                                if target.Colour = them then moves.Add({ From = sq; To = targetSq; Kind = Capture })
                                blocked <- true
                            | None -> 
                                moves.Add({ From = sq; To = targetSq; Kind = Quiet })
                            nf <- nf + df
                            nr <- nr + dr

                | Pawn ->
                    let dir = if us = White then 1 else -1
                    let startRank = if us = White then 1 else 6
                    
                    // Push
                    let push1Rank = r + dir
                    let push1 = Square.ofFileRank (File.fromInt f) (Rank.fromInt push1Rank)

                    if not (Map.containsKey push1 b.Pieces) then
                        // Promotion?
                        let promotionRank = if us = White then 7 else 0
                        if push1Rank = promotionRank then
                            for pt in [ Queen; Rook; Bishop; Knight ] do
                                moves.Add({ From = sq; To = push1; Kind = Promotion pt })
                        else
                            moves.Add({ From = sq; To = push1; Kind = Quiet })

                            // Double push
                            if r = startRank then
                                let push2 = Square.ofFileRank (File.fromInt f) (Rank.fromInt (r + 2 * dir))
                                if not (Map.containsKey push2 b.Pieces) then
                                    moves.Add({ From = sq; To = push2; Kind = Quiet })
                    
                    // Captures (including promotion captures)
                    for df in [-1; 1] do
                        let nf = f + df
                        if nf >= 0 && nf <= 7 then
                            let capRank = r + dir
                            let capSq = Square.ofFileRank (File.fromInt nf) (Rank.fromInt capRank)

                            let promotionRank = if us = White then 7 else 0

                            match b.Pieces |> Map.tryFind capSq with
                            | Some target when target.Colour = them ->
                                if capRank = promotionRank then
                                    for pt in [ Queen; Rook; Bishop; Knight ] do
                                        moves.Add({ From = sq; To = capSq; Kind = Promotion pt })
                                else
                                    moves.Add({ From = sq; To = capSq; Kind = Capture })

                            | _ ->
                                // En passant (never a promotion)
                                if Some capSq = b.EnPassantSquare then
                                    moves.Add({ From = sq; To = capSq; Kind = EnPassant })

        moves.ToArray()

// --- UCI LOGIC ---

module UciLoop =
    let mutable currentBoard = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"

    let rec run () =
        let line = Console.ReadLine()
        if line = null then () else
        let tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries) |> Array.toList
        match tokens with
        | "uci"::_ ->
            printfn "id name Floundroid"
            printfn "id author Phil Brooks"
            printfn "uciok"
        | "isready"::_ -> printfn "readyok"
        | "position" :: "startpos" :: _ ->
            currentBoard <- Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        | "position" :: "fen" :: f1 :: f2 :: f3 :: f4 :: f5 :: f6 :: _ ->
            currentBoard <- Board.fromFen $"{f1} {f2} {f3} {f4} {f5} {f6}"
        | "print"::_ -> Board.prettyPrint currentBoard
        | "go"::_ ->
            let moves = MoveGen.getPseudoLegalMoves currentBoard
            if moves.Length > 0 then
                printfn "bestmove %s" (Move.toUci moves.[0])
        | "quit"::_ -> exit 0
        | _ -> ()
        run ()

[<EntryPoint>]
let main _ =
    UciLoop.run()
    0