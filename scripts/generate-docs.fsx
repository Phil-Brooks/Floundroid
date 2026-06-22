open System
open System.IO

let cleanXml (line: string) =
    line
        .Replace("///", "")
        .Replace("<summary>", "")
        .Replace("</summary>", "")
        .Replace("<param name=\"", "**Param ")
        .Replace("\">", "**: ")
        .Replace("</param>", "")
        .Replace("<returns>", "**Returns**: ")
        .Replace("</returns>", "")
        .Trim()

let generateDocs (sourceFile: string, outfile: string, append: bool) =
    if not (File.Exists(sourceFile)) then
        printfn "Source file not found!"
    else
        let lines = File.ReadAllLines(sourceFile)
        let mutable toc = [ "## 📑 Table of Contents" ]
        let mutable content = [ "" ]
        let mutable lastComments = []
        let mutable lastWasFact = false

        for line in lines do
            let trimmed = line.Trim()

            // XML comments
            if trimmed.StartsWith("///") then
                let cleaned = cleanXml trimmed
                if not (String.IsNullOrWhiteSpace cleaned) then
                    lastComments <- lastComments @ [ cleaned ]

            // Detect [<Fact>]
            elif trimmed.StartsWith("[<Fact>]") then
                lastWasFact <- true

            // Module detection
            elif trimmed.StartsWith("module ") then
                let modName =
                    trimmed.Replace("module ", "").Replace("=", "").Trim()

                toc <-
                    toc
                    @ [ sprintf "- [%s](#module-%s)" modName (modName.ToLower().Replace(".", "")) ]

                content <- content @ [ ""; sprintf "## 📦 module %s" modName; "---" ]
                lastComments <- []
                lastWasFact <- false

            // Backtick test functions (after [<Fact>])
            elif lastWasFact && trimmed.StartsWith("let ``") then
                let startIdx = trimmed.IndexOf("``") + 2
                let endIdx = trimmed.IndexOf("``", startIdx)
                let testName = trimmed.Substring(startIdx, endIdx - startIdx)

                content <- content @ [ sprintf "- **fn** `%s`" testName ]

                if not (List.isEmpty lastComments) then
                    content <- content @ (lastComments |> List.map (fun c -> "    - *" + c + "*"))

                lastComments <- []
                lastWasFact <- false

            // Type definitions
            elif trimmed.StartsWith("type ") then
                // Extract the type name (before '=')
                let typeLine = trimmed
                let typeName =
                    if typeLine.Contains("=") then
                        typeLine.Split([|'='|]).[0].Trim()
                    else typeLine

                // If this is a simple one-line type (e.g., "type Square = int")
                if trimmed.Contains("=") && not (trimmed.EndsWith("=")) then
                    content <- content @ [ ""; sprintf "#### 🧩 `%s`" trimmed ]
                else
                    // Multi-line DU: print only "type Name"
                    content <- content @ [ ""; sprintf "#### 🧩 `%s`" typeName ]

                // Attach XML comments if present
                if not (List.isEmpty lastComments) then
                    content <- content @ (lastComments |> List.map (fun c -> "> " + c))

                lastComments <- []
            
            // Normal function detection (ignore backtick tests and inner lets)
            elif
                // top‑level only: exactly 4 spaces before let
                (line.StartsWith("    let ") || line.StartsWith("    let rec "))
                && trimmed.Contains("=")
                && not (trimmed.Contains("``"))
            then
                let parts =
                    trimmed.Split([| ' '; '('; ':' |], StringSplitOptions.RemoveEmptyEntries)

                let name =
                    match parts with
                    | [| "let"; "inline"; name; _ |] -> name
                    | [| "let"; "rec"; name; _ |] -> name
                    | [| "let"; "inline"; "rec"; name; _ |] -> name
                    | [| "let"; name; _ |] -> name
                    | _ ->
                        // fallback: last non-symbol token
                        parts |> Array.filter (fun p -> p <> "let" && p <> "inline" && p <> "rec") |> Array.head

                let noise =
                    [ "sb"; "mutable"; "kf"; "nf"; "rf"; "sq"; "rsq"; "parts"; "rows"; "dirs"; "suites" ]

                if not (List.contains name noise) then
                    content <- content @ [ sprintf "- **fn** `%s`" name ]

                    if not (List.isEmpty lastComments) then
                        content <- content @ (lastComments |> List.map (fun c -> "    - *" + c + "*"))

                lastComments <- []

            // Blank line resets comment buffer
            elif String.IsNullOrWhiteSpace trimmed then
                lastComments <- []

        let md =
            ["# Code File: " + Path.GetFileName(sourceFile)]
            @ [ "" ]
            @ toc
            @ [ "" ]
            @ content
            @ [ "" ]

        if append then
            File.AppendAllLines(outfile, md)
        else
            File.WriteAllLines(outfile, md)

let src1 = @"D:\Github\Floundroid\Floundroid\Types.fs" 
do generateDocs (src1, "../docs/CODE.md", false)
let src2 = @"D:\Github\Floundroid\Floundroid\Program.fs" 
do generateDocs (src2, "../docs/CODE.md", true)
let test1 = @"D:\Github\Floundroid\TestFloundroid\TypesTests.fs" 
do generateDocs (test1, "../docs/TESTS.md", false)
let test2 = @"D:\Github\Floundroid\TestFloundroid\Tests.fs" 
do generateDocs (test2, "../docs/TESTS.md", true)
printfn "Complete! Documentation updated."
