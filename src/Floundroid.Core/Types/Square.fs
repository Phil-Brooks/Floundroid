namespace Floundroid.Core.Types

type Square = int

module Square =
    let file sq = sq % 8
    let rank sq = sq / 8
