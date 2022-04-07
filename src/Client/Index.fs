module Index

open Elmish
open Fable.Remoting.Client
open Shared

type Status =
    | Initial
    | Getting
    | Done of tableCount:int
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
        { model with Status = Getting }, cmd
    | GotTables result ->
        match result with
        | Result.Ok {Name = name; TableCount = tableCount; Bytes = bytes} ->
            bytes.SaveFileAs(name)
            { model with Status = Done tableCount }, Cmd.none
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
                            prop.onChange (SetUrl >> dispatch)
                            prop.onKeyPress (fun kp -> if kp.key = "Enter" then GetTables |> dispatch)
                        ]
                    ]
                ]
                Bulma.control.p [
                    Bulma.button.a [
                        color.isDark
                        if model.Status = Getting then Bulma.button.isLoading
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
                // Bulma.text.hasTextCentered
                // Bulma.text.hasTextWeightBold
                prop.children [
                    Bulma.content [
                        Bulma.text.hasTextLeft
                        prop.children [
                            Html.h1 [ prop.text "Get the tables from a web page as an Excel spreadsheet" ]
                            Html.li [ prop.text "Enter the url of a website that has html tables" ]
                            Html.li [ prop.text "Click \"Download\"" ]
                            Html.li [ prop.text "Check your browser downloads"]
                            Html.li [ prop.text "Each table is in a tab in the spreadsheet"]
                        ]
                    ]
                ]
            ]
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
                        | Initial
                        | Getting ->
                            ()
                        | Done tc ->
                            let noun = if tc = 1 then "table" else "tables"
                            messagePanel color.isSuccess "Yay!" $"Extracted {tc} {noun} - see your browser downloads."
                        | Error e ->
                            messagePanel color.isDanger "Oopsie!" e
                        Html.img [ prop.src "/push-button-receive-table.png" ]
                    ]
                ]
            ]
        ]
    ]
