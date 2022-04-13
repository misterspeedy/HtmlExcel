module Dictionary2d

open System.Collections.Generic

type Bool() =
    let d = Dictionary<int * int, bool>()
    member _.Item
        with get(x, y) =
            match d.TryGetValue((x, y)) with
            | true, b -> b
            | false, _ -> false
        and set (x, y) value =
            d.[(x, y)] <- value
