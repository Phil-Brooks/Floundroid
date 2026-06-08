namespace Floundroid.UCI

open System
open CommandParser
open UciProtocol

module UciLoop =
    let rec run () =
        let line = Console.ReadLine()
        match line with
        | null -> ()
        | cmd ->
            match tokenize cmd with
            | "uci"::_ ->
                printId ()
                printUciOk ()
            | "isready"::_ ->
                printfn "readyok"
            | "quit"::_ -> ()
            | _ -> ()
            run ()
