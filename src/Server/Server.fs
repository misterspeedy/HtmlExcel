module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn
open System
open Shared
open FSharp.Data
open FsExcel

module Integer =

    let tryParse (s : string) =
        match Int32.TryParse s with
        | true, i -> Some i
        | false, _ -> None

module Filename =

    let fromHtmlDoc (htmlDoc :HtmlDocument) =
        // Browser will automatically strip out illegal characters etc.
        htmlDoc.Descendants ["title"]
        |> Seq.tryHead
        |> Option.map (fun h -> h.InnerText())
        |> Option.defaultValue "Table"
        |> fun s -> $"{s}.xlsx"

module Attribute =

    let getAsIntOr (dflt : int) name (node : HtmlNode) =
        node.AttributeValue name |> Integer.tryParse |> Option.defaultValue dflt

module Seq =

    let tryMax s =
        if s |> Seq.length > 0 then
            s |> Seq.max |> Some
        else
            None

type BoolMap() =
    let d = Collections.Generic.Dictionary<int * int, bool>()
    member _.Item
        with get(x, y) =
            match d.TryGetValue((x, y)) with
            | true, b -> b
            | false, _ -> false
        and set (x, y) value =
            d.[(x, y)] <- value

let doGetTables (url : string) =
    async {
        try
            let! htmlDoc = HtmlDocument.AsyncLoad url
            let mutable tablesFound = false

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
                            // TODO use <caption> to name (ensuring unique)
                            tablesFound <- true
                            Worksheet $"Table{tableIndex}"
                            tableIndex <- tableIndex + 1

                            let boolMap = BoolMap()

                            let rowCount = allRows |> Seq.length

                            for rowIndex, row in allRows |> Seq.indexed do

                                let thds = row.Descendants ["th"; "td"] |> Array.ofSeq

                                // Set up a map of cells where there is no corresponding th/td
                                // element because there is a th/td with a colSpan > 1 above.
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
                                        boolMap.[rowIndex+offset, colIndex] <- true
                                    colIndex <- colIndex + (thd |> Attribute.getAsIntOr 1 "colspan")

                                let mutable thdIndex = 0
                                let mutable colIndex = 0

                                while thdIndex < thds.Length do
                                    if boolMap.[rowIndex, colIndex] then
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
            if tablesFound then
                return Ok (
                    {
                        Name = htmlDoc |> Filename.fromHtmlDoc
                        Bytes = cells |> Render.AsStreamBytes
                    })
            else
                return Error ("Sorry, that web page doesn't seem to contain any HTML tables.")
        with
        | e ->
            return Error e.Message
    }

let api =
    { getTables = doGetTables }

let webApp =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue api
    |> Remoting.buildHttpHandler

let app =
    application {
        url "http://0.0.0.0:8085"
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
    }

run app
