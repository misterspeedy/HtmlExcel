module Url

open System
open System.Net

let forceSchema (url : string) =
    let uriBuilder = new UriBuilder(url)
    uriBuilder.Scheme <- "https"
    uriBuilder.Port <- -1
    uriBuilder.ToString()

let isAvailable (url : string) =
    try
        let request = WebRequest.Create(url) :?> HttpWebRequest
        request.Method <- "HEAD"
        use response = request.GetResponse() :?> HttpWebResponse
        response.StatusCode = HttpStatusCode.OK
    with
    | _ ->
        false