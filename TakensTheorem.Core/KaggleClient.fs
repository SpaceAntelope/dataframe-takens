namespace TakensTheorem.Core

open System.Net.Http
open System.Text.Json
open System.Text.Json.Serialization
open System
open System.IO
open System.Net.Http.Headers
open FSharp.Control.Tasks.V2


module KaggleClient =
    let BaseApiUrl = "https://www.kaggle.com/api/v1/"

    let DownloadDatasetUrl user filename = sprintf "%sdatasets/download/%s/%s" BaseApiUrl user filename

    type AuthorizedClient = AuthorizedClient of HttpClient

    type Credentials() =
        member val username: string = null with get, set
        member val key: string = null with get, set
        static member LoadFrom(path: string): Credentials =
            use reader = new StreamReader(path)
            let json = reader.ReadToEnd()
            JsonSerializer.Deserialize(json)

    let CreateAuthorizedClient(auth: Credentials) =
        let authToken =
            sprintf "%s:%s" auth.username auth.key
            |> Text.ASCIIEncoding.ASCII.GetBytes
            |> Convert.ToBase64String

        let client = new HttpClient()
        client.DefaultRequestHeaders.Authorization <- AuthenticationHeaderValue("Basic", authToken)

        AuthorizedClient client

    let DownLoadFileAsync (urlPath: string []) destinationFolder (AuthorizedClient client) =
        let url = sprintf "%sdatasets/download/%s" BaseApiUrl <| String.Join("/", urlPath)
        let filename = urlPath |> Array.last

        async {
            use! stream = client.GetStreamAsync(url) |> Async.AwaitTask
            use fstream = new FileStream(Path.Combine(destinationFolder, filename), FileMode.CreateNew)
            let! _ = stream.CopyToAsync fstream |> Async.AwaitTask
            fstream.Close()
            stream.Close()
        }

    let DownloadFileAsync2 (url: string) (destinationFile: string) cancellationToken (report: int64 * float -> unit)
        (client: HttpClient) =
        task {
            let bufferLength = 4092
            use! response = client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken)

            response.EnsureSuccessStatusCode() |> ignore

            let total =
                if response.Content.Headers.ContentLength.HasValue
                then response.Content.Headers.ContentLength.Value
                else -1L
                |> float

            use! contentStream = response.Content.ReadAsStreamAsync()
            use fileStream =
                new FileStream(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferLength, true)

            let mutable totalRead = 0L
            let mutable totalReads = 0L
            let mutable isMoreToRead = true
            let buffer = Array.create bufferLength 0uy

            while isMoreToRead && not cancellationToken.IsCancellationRequested do
                let! read = contentStream.ReadAsync(buffer, 0, bufferLength)
                if read > 0 then
                    do! fileStream.WriteAsync(buffer, 0, read)
                    totalRead <- totalRead + int64 read
                    totalReads <- totalReads + 1L
                else
                    isMoreToRead <- false

                report (totalRead, float totalRead / total)
        }


    let DownloadFileAsync (url: string) (destinationFile: string) cancellationToken (report: int64 * float -> unit)
        (client: HttpClient) =
        async {
            let bufferLength = 4092
            use! response = client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                            |> Async.AwaitTask

            response.EnsureSuccessStatusCode() |> ignore

            let total =
                if response.Content.Headers.ContentLength.HasValue
                then response.Content.Headers.ContentLength.Value
                else -1L
                |> float

            use! contentStream = response.Content.ReadAsStreamAsync() |> Async.AwaitTask
            use fileStream =
                new FileStream(destinationFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferLength, true)

            let mutable totalRead = 0L
            let mutable totalReads = 0L
            let mutable isMoreToRead = true
            let buffer = Array.create bufferLength 0uy

            while isMoreToRead && not cancellationToken.IsCancellationRequested do
                let! read = contentStream.ReadAsync(buffer, 0, bufferLength) |> Async.AwaitTask
                if read > 0 then
                    do! fileStream.WriteAsync(buffer, 0, read) |> Async.AwaitTask
                    totalRead <- totalRead + int64 read
                    totalReads <- totalReads + 1L
                else
                    isMoreToRead <- false

                report (totalRead, float totalRead / total)
        }

    

    let inline (=>) (key: string) value = key, box value

    type Style([<ParamArray>] props: (string * obj) []) =
        let _props = props
        override _.ToString() =
            Array.fold (fun state (key, value) -> sprintf "%s; %s: %A" state key value) "" _props
 