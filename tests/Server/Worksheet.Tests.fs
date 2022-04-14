module Server.Tests

open Expecto

open Shared
open Server

let server = testList "Server" [
    testCase "trySafeName produces a safe tab name" <| fun _ ->
        [
            [], ("", 1), "Table1"
            [], ("", 2), "Table2"
            [], ("Tab", 1), "Tab"
            [], ("Tab\\/*?:[]", 1), "Tab"
            ["Tab"], ("Tab\\/*?:[]", 1), "Tab_1"
            ["Table1"], ("", 1), "Table1_1"
            ["Table3"; "Table2"], ("", 3), "Table3_1"
        ]
        |> List.iter (fun (history, (s, i), expected) ->
            let g = Worksheet.SafeNameGenerator()
            history |> Seq.iteri (fun i h -> g.Make(h, i) |> ignore)
            let actual = g.Make(s, i)
            Expect.equal actual expected $"{s} -> {expected}")
]

let all =
    testList "All"
        [
            //Shared.Tests.shared
            server
        ]

[<EntryPoint>]
let main _ = runTestsWithCLIArgs [] [||] all