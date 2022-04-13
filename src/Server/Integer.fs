module Integer

open System

let tryParse (s : string) =
    match Int32.TryParse s with
    | true, i -> Some i
    | false, _ -> None