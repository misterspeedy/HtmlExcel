module Html

    module Attribute =

        open FSharp.Data

        let getAsIntOr (dflt : int) name (node : HtmlNode) =
            node.AttributeValue name |> Integer.tryParse |> Option.defaultValue dflt

    module Table =

        open FSharp.Data
        open Shared
        open FsExcel

        let getAsWorksheetAsync (url : string) =
            async {
                try

                    let safeNameGenerator = Worksheet.SafeNameGenerator()

                    let url = url |> Url.forceSchema

                    if url |> Url.isAvailable then

                        let! htmlDoc = HtmlDocument.AsyncLoad (url |> Url.forceSchema)
                        let mutable tableCount = 0

                        let cells =
                            [
                                let tables = htmlDoc.Descendants ["table"]
                                let mutable tableIndex = 1
                                for table in tables do
                                    // TODO render <thead>s and <tbody> separately.
                                    // Doing this will ensure that rowspans end correctly, e.g. https://www.w3schools.com/tags/tryit.asp?filename=tryhtml_th_rowspan_0
                                    let allRows =
                                        table.Descendants ["tr"]
                                    if allRows |> Seq.length > 1 then
                                        tableCount <- tableCount + 1

                                        let tabName =
                                            table.Descendants ["caption"]
                                            |> Seq.tryHead
                                            |> Option.map (fun c -> c.InnerText())
                                            |> Option.defaultValue ""
                                            |> fun s -> safeNameGenerator.Make(s, tableIndex)

                                        Worksheet tabName

                                        tableIndex <- tableIndex + 1

                                        let rowSpanMap = Dictionary2d.Bool()

                                        let rowCount = allRows |> Seq.length

                                        for rowIndex, row in allRows |> Seq.indexed do

                                            let thds = row.Descendants ["th"; "td"] |> Array.ofSeq

                                            // Set up a map of cells where there is no corresponding th/td
                                            // element because there is a th/td with a rowSpan > 1 above.
                                            let mutable colIndex = 0
                                            for thd in thds do

                                                let rowSpan =
                                                    thd
                                                    |> Attribute.getAsIntOr 1 "rowspan"
                                                    |> fun cs ->
                                                        if cs > 0 then cs
                                                        // A rowSpan of 0 means span everything down to the bottom:
                                                        else rowCount - rowIndex

                                                for offset in 1..rowSpan-1 do
                                                    rowSpanMap.[rowIndex+offset, colIndex] <- true
                                                colIndex <- colIndex + (thd |> Attribute.getAsIntOr 1 "colspan")

                                            let mutable thdIndex = 0
                                            let mutable colIndex = 0

                                            while thdIndex < thds.Length do
                                                if rowSpanMap.[rowIndex, colIndex] then
                                                    // This cell has a th/td with rowSpan > 1 above it:
                                                    Cell []
                                                    colIndex <- colIndex + 1
                                                else
                                                    let thd = thds.[thdIndex]
                                                    let colSpan = thd |> Attribute.getAsIntOr 1 "colspan"

                                                    Cell [
                                                        String (thd.InnerText())
                                                        if thd.Name() = "th" then FontEmphasis Bold
                                                        Next (RightBy colSpan)
                                                    ]
                                                    thdIndex <- thdIndex + 1
                                                    colIndex <- colIndex + 1
                                            Go NewRow

                                        AutoFit All
                            ]
                        if tableCount > 0 then
                            return Ok (
                                {
                                    Name = htmlDoc |> Filename.fromHtmlDoc
                                    TableCount = tableCount
                                    Bytes = cells |> Render.AsStreamBytes
                                })
                        else
                            return Error ("Sorry, that web page doesn't seem to contain any HTML tables.")
                    else
                        return Error ("Sorry, we couldn't reach that URL.")
                with
                | e ->
                    return Error e.Message
            }