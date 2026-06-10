namespace TestFloundroid

open System
open Xunit
open Program

module ColourTests =

    [<Fact>]
    let ``Colour opposite works`` () =
        Assert.Equal(Black, Colour.opposite White)
        Assert.Equal(White, Colour.opposite Black)

    [<Fact>]
    let ``Colour char roundtrip`` () =
        let chars = [ 'w'; 'b'; 'W'; 'B' ]
        for c in chars do
            let col = Colour.fromChar c
            Assert.Equal(c.ToString().ToLower()[0], Colour.toChar col)

module FileTests =

    [<Fact>]
    let ``File int roundtrip`` () =
        for i in 0..7 do
            let f = File.fromInt i
            Assert.Equal(i, File.toInt f)

    [<Fact>]
    let ``File char roundtrip`` () =
        for c in ['a'..'h'] do
            let f = File.fromChar c
            Assert.Equal(c, File.toChar f)

module RankTests =

    [<Fact>]
    let ``Rank int roundtrip`` () =
        for i in 0..7 do
            let r = Rank.fromInt i
            Assert.Equal(i, Rank.toInt r)

    [<Fact>]
    let ``Rank char roundtrip`` () =
        for c in ['1'..'8'] do
            let r = Rank.fromChar c
            Assert.Equal(c, Rank.toChar r)

module SquareTests =

    [<Fact>]
    let ``Square file/rank roundtrip`` () =
        for f in 0..7 do
            for r in 0..7 do
                let sq = Square.ofFileRank (File.fromInt f) (Rank.fromInt r)
                Assert.Equal(f, Square.file sq |> File.toInt)
                Assert.Equal(r, Square.rank sq |> Rank.toInt)

    [<Fact>]
    let ``Square string roundtrip`` () =
        let allSquares =
            [ for f in ['a'..'h'] do
                for r in ['1'..'8'] do
                    $"{f}{r}" ]

        for s in allSquares do
            let sq = Square.fromString s
            Assert.Equal(s, Square.toString sq)

module PieceTypeTests =

    [<Fact>]
    let ``PieceType char roundtrip`` () =
        let chars = [ 'p'; 'n'; 'b'; 'r'; 'q'; 'k' ]
        for c in chars do
            let pt = PieceType.fromChar c
            Assert.Equal(c, PieceType.toChar pt)

module PieceTests =
    [<Fact>]
    let ``Piece char roundtrip`` () =
        let chars = [ 'p'; 'n'; 'b'; 'r'; 'q'; 'k'; 'P'; 'N'; 'B'; 'R'; 'Q'; 'K' ]
        for c in chars do
            let p = Piece.fromChar c
            Assert.Equal(c, Piece.toChar p)

module MoveKindTests =
    [<Fact>]
    let ``Quiet move kind is stable and matches itself`` () =
        let a = Quiet
        let b = Quiet
        Assert.Equal(a, b)
    
    [<Fact>]
    let ``MoveKind pattern match on Quiet works`` () =
        let mk : MoveKind = Quiet
        match mk with
        | Quiet -> Assert.True(true)
        | _ -> Assert.True(false, "Expected Quiet move kind")

module MoveTests =
    [<Fact>]
    let ``Move stores fields correctly`` () =
        let fromSq = Square.fromString "e2"
        let toSq = Square.fromString "e4"
        let mv =
            { From = fromSq
              To = toSq
              Kind = Quiet }
        Assert.Equal(fromSq, mv.From)
        Assert.Equal(toSq, mv.To)
        Assert.Equal(Quiet, mv.Kind)

module BoardTests =
    [<Fact>]
    let ``Empty board has no pieces`` () =
        let b = Board.empty
        for sq in 0..63 do
            Assert.True(Board.tryGetPiece b sq |> Option.isNone)
    [<Fact>]
    let ``Set and get piece works`` () =
        let b = Board.empty
        let sq = Square.fromString "e4"
        let p = Piece.fromChar 'Q'
        let b2 = Board.setPiece b sq (Some p)
        Assert.Equal(Some p, Board.tryGetPiece b2 sq)
    [<Fact>]
    let ``Remove piece works`` () =
        let b = Board.empty
        let sq = Square.fromString "e4"
        let p = Piece.fromChar 'Q'
        let b2 = Board.setPiece b sq (Some p)
        let b3 = Board.setPiece b2 sq None
        Assert.True(Board.tryGetPiece b3 sq |> Option.isNone)
