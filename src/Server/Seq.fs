module Seq

let tryMax s =
    if s |> Seq.length > 0 then
        s |> Seq.max |> Some
    else
        None