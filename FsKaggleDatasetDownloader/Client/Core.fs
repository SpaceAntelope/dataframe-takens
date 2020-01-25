namespace FsKaggleDatasetDownloader.Client

open System.Net.Http
open System.Threading
open System.IO
open FsKaggleDatasetDownloader.Types.Core
open FSharp.Control.Tasks.V2

module Core =
    let DownloadFileSimpleAsync (url: string) (destinationPath: string) (client: HttpClient) =
        async {
            use! stream = client.GetStreamAsync(url) |> Async.AwaitTask
            use fstream = new FileStream(destinationPath, FileMode.CreateNew)
            do! stream.CopyToAsync fstream |> Async.AwaitTask
            fstream.Close()
            stream.Close()
        }

    let DownloadFileAsync (url: string) destinationPath (client: HttpClient) (cancellationToken: CancellationToken)
        (report: (ReportingData -> unit) option) =
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
                    rep
                        { DestinationPath = destinationPath
                          BytesRead = totalRead
                          TotalBytes = total }
                | _ -> ()
        }