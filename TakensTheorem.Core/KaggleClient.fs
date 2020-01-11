namespace TakensTheorem.Core

open System.Net.Http
open System.Text.Json
open System
open System.IO
open System.Net.Http.Headers
open FSharp.Control.Tasks.V2
open System.Threading

module Client =
    let DownloadFileSimpleAsync (url: string) (destinationPath: string) (client: HttpClient) =
        async {
            use! stream = client.GetStreamAsync(url) |> Async.AwaitTask
            use fstream = new FileStream(destinationPath, FileMode.CreateNew)
            do! stream.CopyToAsync fstream |> Async.AwaitTask
            fstream.Close()
            stream.Close()
        }


    let DownloadFileAsync (url: string) destinationPath (client: HttpClient) (cancellationToken: CancellationToken)
        (report: (string * int64 * int64 -> unit) option) =
        let bufferLength = 8192
        let desiredSampleCount = 15L

        task {
            use! response = client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken)

            response.EnsureSuccessStatusCode() |> ignore

            let total =
                if response.Content.Headers.ContentLength.HasValue
                then response.Content.Headers.ContentLength.Value
                else -1L

            let reportStep =
                if total >= 0L
                then total / int64 bufferLength / desiredSampleCount
                else int64 bufferLength * 100L

            use! contentStream = response.Content.ReadAsStreamAsync()
            use fileStream =
                new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferLength, true)

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

                match report with
                | Some rep when totalReads % reportStep = 0L || not isMoreToRead ->
                    rep (destinationPath, totalRead, total)
                | _ -> ()
        }

module KaggleClient =
    let BaseApiUrl = "https://www.kaggle.com/api/v1/"

    type AuthorizedClient = AuthorizedClient of HttpClient

    type DatasetFile =
        | Filename of string
        | CompleteDatasetZipped
        member x.ToOption() =
            match x with
            | Filename filename -> Some filename
            | _ -> None

    type DatasetInfo =
        { Owner: string
          Dataset: string
          Request: DatasetFile }
        member x.ToUrl() =
            match x.Request with
            | Filename filename -> sprintf "%sdatasets/download/%s/%s/%s" BaseApiUrl x.Owner x.Dataset filename
            | CompleteDatasetZipped -> sprintf "%sdatasets/download/%s/%s" BaseApiUrl x.Owner x.Dataset

    type Credentials() =
        member val username: string = null with get, set
        member val key: string = null with get, set

    module Credentials =

        let LoadFrom(path: string): Credentials =
            use reader = new StreamReader(path)
            let json = reader.ReadToEnd()
            JsonSerializer.Deserialize(json)

        let AuthorizeClient (client: HttpClient) (auth: Credentials) =
            let authToken =
                sprintf "%s:%s" auth.username auth.key
                |> Text.ASCIIEncoding.ASCII.GetBytes
                |> Convert.ToBase64String

            client.DefaultRequestHeaders.Authorization <- AuthenticationHeaderValue("Basic", authToken)

            AuthorizedClient client

    type DownloadOptions =
        { DatasetInfo: DatasetInfo
          AuthorizedClient: AuthorizedClient
          DestinationFolder: string
          CancellationToken: CancellationToken option
          ReportingCallback: (string * int64 * int64 -> unit) option }

    let DownloadDatasetAsync(options: DownloadOptions) =
        let url = options.DatasetInfo.ToUrl()
        let fileName =
            options.DatasetInfo.Request.ToOption() |> Option.defaultValue (options.DatasetInfo.Dataset + ".zip")
        let destinationFile = Path.Combine(options.DestinationFolder, fileName)

        if File.Exists destinationFile then failwithf "File [%s] already exists." destinationFile

        let (AuthorizedClient client) = options.AuthorizedClient
        let token = options.CancellationToken |> Option.defaultValue (CancellationToken())

        Client.DownloadFileAsync url destinationFile client token options.ReportingCallback

    let example kaggleJsonPath =
        use client = new HttpClient()

        { DatasetInfo =
              { Owner = "selfishgene"
                Dataset = "historical-hourly-weather-data"
                Request = CompleteDatasetZipped }
          AuthorizedClient =
              kaggleJsonPath
              |> Credentials.LoadFrom
              |> Credentials.AuthorizeClient client
          DestinationFolder = "../Data"
          CancellationToken = None
          ReportingCallback = None }
        |> DownloadDatasetAsync
        |> Async.AwaitTask
        |> Async.RunSynchronously