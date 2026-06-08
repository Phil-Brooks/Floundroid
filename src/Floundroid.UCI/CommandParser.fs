namespace Floundroid.UCI

module CommandParser =
    let tokenize (line: string) =
        line.Split(' ', System.StringSplitOptions.RemoveEmptyEntries)
        |> Array.toList
