namespace Shared

open System

type NamedWorkBook =
    {
        Name : string
        TableCount : int
        Bytes : byte[]
    }

type Todo = { Id: Guid; Description: string }

module Model =
    let isValid (url: string) =
        String.IsNullOrWhiteSpace url |> not

module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

type IApi =
    { getTables: string -> Async<Result<NamedWorkBook, string>> }
