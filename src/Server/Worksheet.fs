module Worksheet

open System

let maxLen = 31
let illegal = ['\\'; '/'; '*'; '?'; ':'; '['; ']'] |> Set.ofList

let trySafeName (s : string) =
    s
    |> Seq.filter (fun c -> illegal |> Set.contains c |> not)
    |> Seq.truncate 31
    |> Array.ofSeq
    |> fun cs ->
        new String(cs)
    |> fun s ->
        if String.IsNullOrWhiteSpace s then
            None
        else
            Some s

