open System
open System.IO

let outputFile = @"D:\Github\Floundroid\CODE_STRUCTURE.md"

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

        for line in lines do
            let trimmed = line.Trim()

            if trimmed.StartsWith("///") then
                let cleaned = cleanXml trimmed

                if not (String.IsNullOrWhiteSpace cleaned) then
                    lastComments <- lastComments @ [ cleaned ]

            // 1. Better Module Detection (Captures name properly)
            elif (trimmed.StartsWith("module ") || trimmed.StartsWith("namespace ")) then
                let modName =
                    trimmed.Replace("module ", "").Replace("namespace ", "").Replace("=", "").Trim()

                toc <-
                    toc
                    @ [ sprintf "- [%s](#-module-%s)" modName (modName.ToLower().Replace(".", "")) ]

                content <- content @ [ ""; sprintf "## 📦 module %s" modName; "---" ]
                lastComments <- []

            // 2. Full Type Definitions
            elif trimmed.StartsWith("type ") then
                content <- content @ [ ""; "#### 🧩 `" + trimmed + "`" ]

                if not (List.isEmpty lastComments) then
                    content <- content @ (lastComments |> List.map (fun c -> "> " + c))

                lastComments <- []

            // 3. Cleaner Function List
            elif
                (line.StartsWith("let ") || line.StartsWith("    let "))
                && not (trimmed.Contains(" = "))
            then
                let parts =
                    trimmed.Split([| ' '; '('; ':' |], StringSplitOptions.RemoveEmptyEntries)

                let nameIndex = if parts.[1] = "rec" then 2 else 1

                if parts.Length > nameIndex then
                    let funcName = parts.[nameIndex]

                    let noise =
                        [ "sb"
                          "mutable"
                          "kf"
                          "nf"
                          "rf"
                          "sq"
                          "rsq"
                          "parts"
                          "rows"
                          "dirs"
                          "suites" ]

                    if not (List.contains funcName noise) then
                        content <- content @ [ sprintf "- **fn** `%s`" funcName ]

                        if not (List.isEmpty lastComments) then
                            content <- content @ (lastComments |> List.map (fun c -> "    - *" + c + "*"))

                lastComments <- []

            elif String.IsNullOrWhiteSpace trimmed then
                lastComments <- []

        let md =
            ["# Code File: " + Path.GetFileName(sourceFile)]
            @ toc
            @ [ "" ]
            @ content
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
