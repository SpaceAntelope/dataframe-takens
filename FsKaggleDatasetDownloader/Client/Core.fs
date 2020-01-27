namespace FsKaggleDatasetDownloader.Client

open System.Net.Http
open System.Threading
open System.IO
open System
open FsKaggleDatasetDownloader.Types.Core
open FSharp.Control.Tasks.V2
open System.Threading.Tasks

module Core =
    type WriteAsyncCallback = array<byte> * int * int -> Task

    type SampleInterval =
        | ByteCount of int64
        | Time of TimeSpan

    let DownloadFileSimpleAsync (url: string) (destinationPath: string) (client: HttpClient) =
        async {
            use! stream = client.GetStreamAsync(url) |> Async.AwaitTask
            use fstream = new FileStream(destinationPath, FileMode.CreateNew)
            do! stream.CopyToAsync fstream |> Async.AwaitTask
            fstream.Close()
            stream.Close()
        }

    let DownloadStreamAsync (url: string) (writeAsync: WriteAsyncCallback) (client: HttpClient)
        (cancellationToken: CancellationToken) (report: (ReportingData -> unit) option) (notes: string)
        (bufferLength: int) (sampleInterval: SampleInterval) =

        let mutable timeOfLastSample = DateTime.Now
        let mutable dataSinceLastSample = 0L

        let submitReport totalRead total =
            let elapsedSec = (DateTime.Now - timeOfLastSample).TotalSeconds

            let bps = float dataSinceLastSample / elapsedSec

            let msg = sprintf "%d / %f s = %.2f bps" dataSinceLastSample elapsedSec bps

            timeOfLastSample <- DateTime.Now
            dataSinceLastSample <- 0L

            { Notes = msg
              BytesRead = totalRead
              TotalBytes = total
              BytesPerSecond = bps }

        task {
            use! response = client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken)

            response.EnsureSuccessStatusCode() |> ignore

            let total =
                if response.Content.Headers.ContentLength.HasValue
                then response.Content.Headers.ContentLength.Value
                else -1L

            use! contentStream = response.Content.ReadAsStreamAsync()

            let mutable totalRead = 0L
            let mutable isMoreToRead = true

            let buffer = Array.create bufferLength 0uy

            while isMoreToRead && not cancellationToken.IsCancellationRequested do
                let! read = contentStream.ReadAsync(buffer, 0, bufferLength)
                if read > 0 then
                    do! writeAsync (buffer, 0, read)
                    totalRead <- totalRead + int64 read
                    dataSinceLastSample <- dataSinceLastSample + int64 read
                else
                    isMoreToRead <- false

                match report, sampleInterval with
                | Some reportCallback, Time interval when (timeOfLastSample - DateTime.Now) >= interval
                                                          || (not isMoreToRead && dataSinceLastSample > 0L) ->
                    submitReport totalRead total |> reportCallback

                | Some reportCallback, ByteCount interval when dataSinceLastSample >= interval
                                                               || (not isMoreToRead && dataSinceLastSample > 0L) ->
                    submitReport totalRead total |> reportCallback
                | _, _ -> ()
        }

    let DownloadFileAsync2 (url: string) destinationPath (client: HttpClient) (cancellationToken: CancellationToken)
        (report: (ReportingData -> unit) option) =
        let bufferLength = 8192
        let sampleInterval = TimeSpan.FromMilliseconds(500.0)

        use fileStream =
            new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferLength, true)

        DownloadStreamAsync url (fileStream.WriteAsync) client cancellationToken report destinationPath bufferLength
            (Time sampleInterval)


    let DownloadFileAsync (url: string) destinationPath (client: HttpClient) (cancellationToken: CancellationToken)
        (report: (ReportingData -> unit) option) =
        let bufferLength = 8192

        let sampleInterval = TimeSpan.FromMilliseconds(500.0)

        task {
            use! response = client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken)

            response.EnsureSuccessStatusCode() |> ignore

            let total =
                if response.Content.Headers.ContentLength.HasValue
                then response.Content.Headers.ContentLength.Value
                else -1L

            use! contentStream = response.Content.ReadAsStreamAsync()
            use fileStream =
                new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferLength, true)

            let mutable totalRead = 0L
            let mutable totalReads = 0L
            let mutable isMoreToRead = true
            let mutable timeOfLastSample = DateTime.Now
            let mutable dataSinceLastSample = 0L

            let buffer = Array.create bufferLength 0uy

            while isMoreToRead && not cancellationToken.IsCancellationRequested do
                let! read = contentStream.ReadAsync(buffer, 0, bufferLength)
                if read > 0 then
                    do! fileStream.WriteAsync(buffer, 0, read)
                    totalRead <- totalRead + int64 read
                    dataSinceLastSample <- dataSinceLastSample + int64 read
                    totalReads <- totalReads + 1L
                else
                    isMoreToRead <- false

                match report with
                | Some reportCallback when (timeOfLastSample - DateTime.Now) >= sampleInterval || not isMoreToRead ->
                    let bps = float dataSinceLastSample / float (timeOfLastSample - DateTime.Now).Seconds

                    timeOfLastSample <- DateTime.Now
                    dataSinceLastSample <- 0L

                    reportCallback
                        { Notes = destinationPath
                          BytesRead = totalRead
                          TotalBytes = total
                          BytesPerSecond = bps }
                | _ -> ()
        }
