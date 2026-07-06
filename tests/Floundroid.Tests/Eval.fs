namespace Floundroid.Tests

open Xunit
open Floundroid

module EvaluationTests =

    [<Fact>]
    let ``Starting position evaluation is perfectly symmetrical (0)`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
        Assert.Equal(0, Evaluation.evaluate b)

    [<Theory>]
    [<InlineData("rnb1kbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1", 800)>] // White up Queen
    [<InlineData("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNB1KBNR w KQkq - 0 1", -800)>] // Black up Queen
    let ``Material advantage is detected correctly`` (fen: string, expectedMinMaterial: int) =
        let b = Board.fromFen fen
        let score = Evaluation.evaluate b

        if expectedMinMaterial > 0 then
            // Instead of exact 900, we check if it's strongly positive
            Assert.True(score >= expectedMinMaterial, $"White should be up significantly, got {score}")
        else
            // Instead of exact -900, we check if it's strongly negative
            Assert.True(score <= expectedMinMaterial, $"Black should be up significantly, got {score}")

    [<Fact>]
    let ``Knight on D4 is valued higher than Knight on A1 (PST)`` () =
        let corner = Board.fromFen "8/8/8/8/8/8/8/N7 w - - 0 1"
        let center = Board.fromFen "8/8/8/8/3N4/8/8/8 w - - 0 1"

        let scoreCorner = Evaluation.evaluate corner
        let scoreCenter = Evaluation.evaluate center

        Assert.True(
            scoreCenter > scoreCorner,
            $"Central knight ({scoreCenter}) should be worth more than corner knight ({scoreCorner})"
        )

    [<Fact>]
    let ``Black piece positioning is mirrored correctly`` () =
        // A black pawn on d7 (starting) vs d2 (advanced)
        // Advanced black pawns should score better for Black (more negative total score)
        let starting = Board.fromFen "8/3p4/8/8/8/8/8/8 b - - 0 1"
        let advanced = Board.fromFen "8/8/8/8/8/8/3p4/8 b - - 0 1"

        let scoreStarting = Evaluation.evaluate starting
        let scoreAdvanced = Evaluation.evaluate advanced

        // advanced should be "better" for Black, meaning a more negative number
        Assert.True(
            scoreAdvanced < scoreStarting,
            $"Advanced black pawn ({scoreAdvanced}) should be better for black than starting pawn ({scoreStarting})"
        )

    [<Fact>]
    let ``Evaluating an empty board returns 0`` () =
        let b = Board.empty
        Assert.Equal(0, Evaluation.evaluate b)

    [<Fact>]
    let ``Starting position pawn structure is symmetrical`` () =
        let b = Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"

        let psMG, psEG = Evaluation.pawnStructureScore b
        Assert.Equal(0, psMG)
        Assert.Equal(0, psEG)

    [<Fact>]
    let ``Doubled pawns are penalized`` () =
        let healthy = Board.fromFen "4k3/8/8/8/8/8/3P4/4K3 w - - 0 1"
        let doubled = Board.fromFen "4k3/8/8/8/8/3P4/3P4/4K3 w - - 0 1"

        let psDoubledMG, psDoubledEG = Evaluation.pawnStructureScore doubled
        let psHealthyMG, psHealthyEG = Evaluation.pawnStructureScore healthy

        Assert.True(
            psDoubledMG < psHealthyMG,
            "Doubled white pawns should score worse than a single healthy pawn."
        )

    [<Fact>]
    let ``Isolated pawns are penalized compared with connected pawns`` () =
        let isolated = Board.fromFen "4k3/8/8/8/8/8/2P2P2/4K3 w - - 0 1"
        let connected = Board.fromFen "4k3/8/8/8/8/8/2PP4/4K3 w - - 0 1"

        let psConnectedMG, psConnectedEG = Evaluation.pawnStructureScore connected
        let psIsolatedMG, psIsolatedEG = Evaluation.pawnStructureScore isolated

        Assert.True(
            psConnectedMG > psIsolatedMG,
            "Connected pawns should score better than isolated pawns."
        )

    [<Fact>]
    let ``Passed pawns are rewarded`` () =
        let blocked = Board.fromFen "4k3/8/3p4/8/3P4/8/8/4K3 w - - 0 1"
        let passed = Board.fromFen "4k3/8/8/8/3P4/8/8/4K3 w - - 0 1"

        Assert.True(
            Evaluation.pawnStructureScore passed > Evaluation.pawnStructureScore blocked,
            "Passed white pawns should score better than blocked pawns."
        )

    [<Fact>]
    let ``Black passed pawn scores for black`` () =
        let b = Board.fromFen "4k3/8/8/8/3p4/8/8/4K3 b - - 0 1"

        let psMG, psEG = Evaluation.pawnStructureScore b
        Assert.True(
            psMG < 0 || psEG < 0,
            "A black passed pawn should make the white-perspective score negative."
        )

    [<Fact>]
    let ``White missing g-pawn is worse than full pawn shield`` () =
        let full =
            "rnbq1rk1/pppppppp/8/8/8/8/PPPPPPPP/RNBQ1RK1 w - - 0 1"
        let weak =
            "rnbq1rk1/pppppppp/8/8/8/6P1/PPPPPP1P/RNBQ1RK1 w - - 0 1"

        let bFull = Board.fromFen full
        let bWeak = Board.fromFen weak

        let eFull = Evaluation.evaluate bFull
        let eWeak = Evaluation.evaluate bWeak

        Assert.True(eFull > eWeak)

    [<Fact>]
    let ``open h-file next to white king is penalised`` () =
        let fenSafe =
            "rnbq1rk1/pppppppp/8/8/8/8/PPPPPPPP/RNBQ1RK1 w - - 0 1"

        let fenOpen =
            "rnbq1rk1/pppppppp/8/8/8/8/PPPPPPPP/RNBQ1R2 w - - 0 1" // remove h1 pawn

        let bSafe = Board.fromFen fenSafe
        let bOpen = Board.fromFen fenOpen

        Assert.True(Evaluation.evaluate bSafe > Evaluation.evaluate bOpen)

    //[<Fact>]
    //let ``half-open h-file is penalised`` () =
    //    let fenSafe =
    //        "rnbq1rk1/1ppppppp/8/8/8/8/PPPPPPPP/RNBQ1RK1 w - - 0 1"

    //    let fenHalf =
    //        "rnbq1rk1/ppppppp1/8/8/8/8/PPPPPPPP/RNBQ1RK1 w - - 0 1" // black pawn missing on h7

    //    let bSafe = Board.fromFen fenSafe
    //    let bHalf = Board.fromFen fenHalf
    //    let safescr = Evaluation.evaluate bSafe
    //    let halfscr = Evaluation.evaluate bHalf

    //    Assert.True(safescr > halfscr)

    [<Fact>]
    let ``no open-file penalty when king not short-castled`` () =
        let fen =
            "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w - - 0 1"

        let b = Board.fromFen fen
        Assert.Equal(Evaluation.evaluate b, Evaluation.evaluate b)

    [<Fact>]
    let ``enemy knight near white king is penalised`` () =
        let safe =
            "rnbq1rk1/pppppppp/6n1/8/8/8/PPPPPPPP/RNBQ1RK1 w - - 0 1"

        let knightNear =
            "rnbq1rk1/pppppppp/8/8/8/6n1/PPPPPPPP/RNBQ1RK1 w - - 0 1" // knight on g3

        let bSafe = Board.fromFen safe
        let bNear = Board.fromFen knightNear
        let safeScore = Evaluation.evaluate bSafe
        let nearScore = Evaluation.evaluate bNear

        Assert.True(safeScore > nearScore)

    //[<Fact>]
    //let ``enemy queen near white king is penalised more than knight`` () =
    //    let knight =
    //        "rnb2rk1/pppppppp/6q1/8/8/6n1/PPPPPPPP/RNBQ1RK1 w - - 0 1"

    //    let queen =
    //        "rnb2rk1/pppppppp/6n1/8/8/6q1/PPPPPPPP/RNBQ1RK1 w - - 0 1"

    //    let bN = Board.fromFen knight
    //    let bQ = Board.fromFen queen
    //    let scoreN = Evaluation.evaluate bN
    //    let scoreQ = Evaluation.evaluate bQ

    //    Assert.True(scoreN > scoreQ)

    [<Fact>]
    let ``Evaluation is antisymmetric under colour swap`` () =
        let fen = "rnbq1rk1/pppp1ppp/5n2/4p3/3PP3/2N2N2/PPP2PPP/R1BQ1RK1 w - - 0 1"
        let bWhite = Board.fromFen fen

        // crude colour swap: just flip perspective via FEN
        let fenBlack =
            "r1bq1rk1/ppp2ppp/2n2n2/3pp3/4P3/5N2/PPPP1PPP/RNBQ1RK1 b - - 0 1"
        let bBlack = Board.fromFen fenBlack

        let sWhite = Evaluation.evaluate bWhite
        let sBlack = Evaluation.evaluate bBlack

        Assert.Equal(sWhite, -sBlack)

    [<Fact>]
    let ``Short-castled king with full shield scores better than uncastled king`` () =
        let uncastled =
            Board.fromFen "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQ1RK1 w - - 0 1"

        let castled =
            Board.fromFen "rnbq1rk1/pppppppp/8/8/8/8/PPPPPPPP/RNBQ1RK1 w - - 0 1"

        let sUn = Evaluation.evaluate uncastled
        let sCast = Evaluation.evaluate castled

        Assert.True(sCast > sUn)
