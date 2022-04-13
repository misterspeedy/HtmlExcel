module Filename

open FSharp.Data

let fromHtmlDoc (htmlDoc : HtmlDocument) =
    // Browser will automatically strip out illegal characters etc.
    htmlDoc.Descendants ["title"]
    |> Seq.tryHead
    |> Option.map (fun h -> h.InnerText())
    |> Option.defaultValue "Table"
    |> fun s -> $"{s}.xlsx"