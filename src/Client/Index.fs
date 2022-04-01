module Index

open Elmish
open Fable.Remoting.Client
open Shared

type Model = { Url: string }

type Msg =
    | SetUrl of string
    | GetTables
    | GotTables of Result<NamedWorkBook, string>

let api =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IApi>

let init () : Model * Cmd<Msg> =
    let model = { Url = "https://www.w3schools.com/html/html_tables.asp" }

    let cmd = Cmd.none

    model, cmd

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | SetUrl value ->
        { model with Url = value }, Cmd.none
    | GetTables ->
        let cmd = Cmd.OfAsync.perform api.getTables model.Url GotTables
        model, cmd
    | GotTables result ->
        match result with
        | Ok {Name = name; Bytes = bytes} -> bytes.SaveFileAs(name)
        | Error e -> () // TODO
        model, Cmd.none

open Feliz
open Feliz.Bulma

let navBrand =
    Bulma.navbarBrand.div [
        Bulma.navbarItem.a [
            prop.href "https://safe-stack.github.io/"
            navbarItem.isActive
            prop.children [
                Html.img [
                    prop.src "/favicon.png"
                    prop.alt "Logo"
                ]
            ]
        ]
    ]

let containerBox (model: Model) (dispatch: Msg -> unit) =
    Bulma.box [
        Bulma.field.div [
            field.isGrouped
            prop.children [
                Bulma.control.p [
                    control.isExpanded
                    prop.children [
                        Bulma.input.text [
                            prop.value model.Url
                            prop.placeholder "E.g. https://en.wikipedia.org/wiki/Asteroid"
                            prop.onChange (fun x -> SetUrl x |> dispatch)
                        ]
                    ]
                ]
                Bulma.control.p [
                    Bulma.button.a [
                        color.isPrimary
                        prop.disabled (Model.isValid model.Url |> not)
                        prop.onClick (fun _ -> dispatch GetTables)
                        prop.text "Download"
                    ]
                ]
            ]
        ]
    ]

let view (model: Model) (dispatch: Msg -> unit) =
    Bulma.hero [
        hero.isFullHeight
        color.isWhite
        prop.children [
            Bulma.column [
                column.is10
                column.isOffset1
                prop.children [
                    containerBox model dispatch
                ]
            ]
            Bulma.container [
                Bulma.column [
                    column.is8
                    column.isOffset2
                    prop.children [
                        Html.img [ prop.src "/push-button-receive-table.png" ]
                    ]
                ]
            ]
        ]
    ]
