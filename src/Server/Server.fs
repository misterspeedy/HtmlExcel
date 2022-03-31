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

let doGetTables (url : string) =
    async {
        try
            let! htmlDoc = HtmlDocument.AsyncLoad url

            let fileName =
                htmlDoc.Descendants ["title"]
                |> Seq.tryHead
                |> Option.map (fun h -> h.InnerText())
                |> Option.defaultValue "Table"
                |> fun s -> $"{s}.xlsx"

            let cells =
                [
                    let tables = htmlDoc.Descendants ["table"]
                    for i, table in tables |> Seq.indexed do
                        let allRows =
                            table.Descendants ["tr"]
                        if allRows |> Seq.length > 1 then
                            Worksheet $"Table{i}"
                            for row in allRows do
                                let thds = row.Descendants ["th"; "td"]
                                // TODO measure maximum rowspan for the row and go down by
                                // an appropriate amount - e.g. https://en.wikipedia.org/wiki/List_of_tallest_trees
                                for thd in thds do
                                    let colSpan = thd.AttributeValue "colspan" |> Integer.tryParse |> Option.defaultValue 1
                                    Style (if thd.Name() = "th" then [ FontEmphasis Bold ] else [])
                                    Cell [
                                        // TODO handle numbers
                                        String (thd.InnerText())
                                        Next (RightBy colSpan)
                                    ]
                                    Style []
                                Go NewRow
                            AutoFit All
                ]
            return Ok ({Name = fileName; Bytes = cells |> Render.AsStreamBytes})
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
