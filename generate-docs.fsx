open System
open System.IO
open System.Text.RegularExpressions

let sourceFile = @"D:\Github\Floundroid\Floundroid\Program.fs" // Change this to your filename
let outputFile = "CODE_STRUCTURE.md"

let generateDocs () =
    if not (File.Exists(sourceFile)) then 
        printfn "Source file not found!"
    else
        let lines = File.ReadAllLines(sourceFile)
        let mutable markdown = ["# Floundroid Code Structure"; ""; "Generated on: " + DateTime.Now.ToString()]
        
        for line in lines do
            let trimmed = line.Trim()
            
            // Match Modules
            if trimmed.StartsWith("module ") then
                markdown <- markdown @ [""; "## 📦 " + trimmed; "---"]
            
            // Match Types
            elif trimmed.StartsWith("type ") then
                markdown <- markdown @ ["- **Type:** `" + trimmed + "`"]
                
            // Match Functions (let)
            elif trimmed.StartsWith("let ") && not (trimmed.Contains(" =") && trimmed.Length < 20) then
                // Filter out simple variable assignments, keep functions
                let funcName = trimmed.Split(' ').[1]
                markdown <- markdown @ ["  - `fn` " + funcName]

        File.WriteAllLines(outputFile, markdown)
        printfn "Documentation generated in %s" outputFile

generateDocs()