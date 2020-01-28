module Tests

open System
open Xunit
open System.Net.Http
open System.Net
open System.IO
open System.Threading
open FsKaggleDatasetDownloader.Core
open FsKaggleDatasetDownloader.Kaggle

[<Fact>]
let ``DownloadStreamAsync Test Reporting with 1MB``() =

    let oneMB = 1_048_576

    use message = new HttpResponseMessage(HttpStatusCode.OK)
    message.Content <- new ByteArrayContent(Array.create<byte> oneMB 1uy)

    let mockHandler =
        { new System.Net.Http.HttpMessageHandler() with
            member x.SendAsync(request, cancellationToken) = async { return message } |> Async.StartAsTask }

    use client = new HttpClient(mockHandler)
    use memstr = new MemoryStream()
    let reportResult = ResizeArray<ReportingData>()
    let desiredSamples = 64
    let bufferSize = oneMB / desiredSamples
    let sampleInterval = ByteCount <| int64 bufferSize

    let report (info: ReportingData) =
        //Async.Sleep(200) |> Async.RunSynchronously

        reportResult.Add info

    DownloadStreamAsync
        {
          BufferLength = bufferSize
          Url = "http://localhost"
          Client = client
          Token = Some(CancellationToken())
          WriteAsync = memstr.WriteAsync
          ReportOptions =
              Some
                  { ReportTitle = "Test"
                    SampleInterval = sampleInterval
                    ReportCallback = report } }
    |> Async.AwaitTask
    |> Async.RunSynchronously

    System.IO.File.WriteAllLines
        ("output.txt",
         reportResult
         |> Seq.mapi (fun i info ->
             sprintf "%03d. %.2f%% @ %.2fMB/s\n%A\n" i (100.0 * float info.BytesRead / float info.TotalBytes)
                 (info.BytesPerSecond / 1024.0) info))

    Assert.Equal(desiredSamples, reportResult.Count)
