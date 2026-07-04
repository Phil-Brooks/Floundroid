namespace Floundroid

module Program =
    [<EntryPoint>]
    let main _ =
        UciLoop.run ()
        0
