module Index

open Elmish
open Fable.Remoting.Client
open Shared

type Status =
    | Initial
    | Done
    | Error of string

type Model =
    {
        Url: string
        Status : Status
    }

type Msg =
    | SetUrl of string
    | GetTables
    | GotTables of Result<NamedWorkBook, string>

let api =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.buildProxy<IApi>

let init () : Model * Cmd<Msg> =
    let model =
        {
            Url = "https://www.w3schools.com/html/html_tables.asp"
            Status = Initial
        }

    let cmd = Cmd.none

    model, cmd

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | SetUrl value ->
        { model with Url = value; Status = Initial }, Cmd.none
    | GetTables ->
        let cmd = Cmd.OfAsync.perform api.getTables model.Url GotTables
        model, cmd
    | GotTables result ->
        match result with
        | Result.Ok {Name = name; Bytes = bytes} ->
            bytes.SaveFileAs(name)
            { model with Status = Done }, Cmd.none
        | Result.Error m ->
            { model with Status = Error m }, Cmd.none

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

let messagePanel color (heading : string) (message : string)  =
    Bulma.message [
        color
        prop.children [
            Bulma.messageHeader [ prop.text heading ]
            Bulma.messageBody [ message |> prop.text ]
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
                        match model.Status with
                        | Initial -> ()
                        | Done -> messagePanel color.isSuccess "Thanks!" "Extracted tables - see your browser downloads."
                        | Error e -> messagePanel color.isDanger "Oopsie!" e
                        Html.img [ prop.src "/push-button-receive-table.png" ]
                    ]
                ]
            ]
        ]
    ]
