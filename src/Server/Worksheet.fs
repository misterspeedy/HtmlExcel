module Worksheet

open System
open System.Collections.Generic

let maxLen = 31
let illegal = ['\\'; '/'; '*'; '?'; ':'; '['; ']'] |> Set.ofList

let private makeUnique (history : string seq) (s : string) =
    let mutable suffix = 1
    let mutable candidate = s
    while history |> Seq.contains candidate do
        candidate <- $"{s}_{suffix}"
        suffix <- suffix + 1
    candidate

type SafeNameGenerator() =

    let history = ResizeArray<string>()

    member _.Make(s : string, tableIndex : int) =
        let result =
            s
            |> Seq.filter (fun c -> illegal |> Set.contains c |> not)
            |> Seq.truncate 31
            |> Array.ofSeq
            |> fun cs ->
                new String(cs)
            |> fun s ->
                if String.IsNullOrWhiteSpace s then
                    $"Table{tableIndex}"
                else
                    s
            |> makeUnique history
        history.Add result
        result

