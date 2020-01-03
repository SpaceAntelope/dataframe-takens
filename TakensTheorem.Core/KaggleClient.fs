namespace TakensTheorem.Core

open System.Net.Http
open System.Text.Json
open System.Text.Json.Serialization
open System
open System.IO
open System.Net.Http.Headers

module KaggleClient =
    let BaseApiUrl = "https://www.kaggle.com/api/v1/"

    type AuthorizedClient = AuthorizedClient of HttpClient

    type Credentials =
        { Username: string
          Key: string }
        static member LoadFrom(path: string): Credentials =
            use reader = new StreamReader(path)
            let json = reader.ReadToEnd()
            JsonSerializer.Deserialize(json)

    let CreateAuthorizedClient(auth: Credentials) =
        let authToken =
            sprintf "%s:%s" auth.Username auth.Key
            |> Text.ASCIIEncoding.ASCII.GetBytes
            |> Convert.ToBase64String

        let client = new HttpClient()
        client.DefaultRequestHeaders.Authorization <- AuthenticationHeaderValue("Basic", authToken)

        AuthorizedClient client

    let DownLoadFileAsync (urlPath: string[]) destinationFolder (AuthorizedClient client) =
        let url = sprintf "%sdatasets/download/%s" BaseApiUrl <| String.Join("/", urlPath)
        let filename = urlPath |> Array.last

        async {
            use! stream = client.GetStreamAsync(url) |> Async.AwaitTask
            use fstream = new FileStream(Path.Combine(destinationFolder, filename), FileMode.CreateNew)
            let! _ = stream.CopyToAsync fstream |> Async.AwaitTask
            fstream.Close()
            stream.Close()
        }
