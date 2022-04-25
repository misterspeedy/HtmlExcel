module Worksheet

open System
open System.Collections.Generic

let maxLen = 31
let illegal = ['\\'; '/'; '*'; '?'; ':'; '['; ']'] |> Set.ofList

let private makeUnique (history : string seq) (s : string) =
    let mutable suffixCounter = 1
    let mutable candidate = s
    while history |> Seq.contains candidate do
        let suffix = $"_{suffixCounter}"
        let root =
            s
            |> Seq.truncate (maxLen - suffix.Length)
            |> Array.ofSeq
            |> fun cs -> new String(cs)
        candidate <- root + suffix
        suffixCounter <- suffixCounter + 1
    candidate

type SafeNameGenerator() =

    let history = ResizeArray<string>()

    member _.Make(s : string, tableIndex : int) =
        let result =
            s
            |> Seq.filter (fun c -> illegal |> Set.contains c |> not)
            |> Seq.truncate maxLen
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

