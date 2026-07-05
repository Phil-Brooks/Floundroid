namespace Texel

[<AutoOpen>]
module Types =
    type Feature = { Index: int; Value: int16 }

    type TuningEntry = {
        // The result of the game: 1.0 (Win), 0.5 (Draw), 0.0 (Loss)
        Result: float
        // The game phase for this position (0..MaxPhase)
        Phase: int
        // Only the features that were active (to save memory)
        Features: Feature[] 
}