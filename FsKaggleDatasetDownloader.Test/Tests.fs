module Tests

open System
open Xunit
open System.Net.Http
open System.Net
open System.IO
open System.Threading
open FsKaggleDatasetDownloader.Core
open FsKaggleDatasetDownloader.Kaggle
open Xunit.Abstractions

[<Fact>]
let ``DownloadStreamAsync Test Progress Reporting with 10MB``() =

    let payloadSize = 10_485_760

    use message = new HttpResponseMessage(HttpStatusCode.OK)
    message.Content <- new ByteArrayContent(Array.create<byte> payloadSize 1uy)

    let mockHandler =
        { new System.Net.Http.HttpMessageHandler() with
            member x.SendAsync(request, cancellationToken) = async { return message } |> Async.StartAsTask }

    use client = new HttpClient(mockHandler)
    use memstr = new MemoryStream()
    let reportResult = ResizeArray<ReportingData>()
    let desiredSamples = 64
    let bufferSize = payloadSize / desiredSamples
    let sampleInterval = ByteCount <| int64 bufferSize

    let report (info: ReportingData) =
        //Async.Sleep(200) |> Async.RunSynchronously
        reportResult.Add info

    DownloadStreamAsync
        { BufferLength = bufferSize
          Url = "http://0.0.0.0"
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

    Assert.Equal(int64 payloadSize, memstr.Length)
    Assert.Equal(desiredSamples, reportResult.Count)

type DownloadFileTest(outputHelper: ITestOutputHelper) =
    let output = outputHelper
    let tempPath = Path.GetTempFileName()

    [<Fact>]
    member x.``Download data to temporary file``() =
        output.WriteLine("[[" + tempPath + "]]")
        let payloadSize = 1_048_576

        use message = new HttpResponseMessage(HttpStatusCode.OK)
        message.Content <- new ByteArrayContent(Array.create<byte> payloadSize 1uy)

        let mockHandler =
            { new System.Net.Http.HttpMessageHandler() with
                member x.SendAsync(request, cancellationToken) = async { return message } |> Async.StartAsTask }

        use client = new HttpClient(mockHandler)
        let reportResult = ResizeArray<ReportingData>()

        DownloadFileAsync "http://0.0.0.0" tempPath client None None
        |> Async.AwaitTask
        |> Async.RunSynchronously

        let fileInfo = FileInfo tempPath
        Assert.True(fileInfo.Exists)
        Assert.Equal(fileInfo.Length, int64 payloadSize)

    interface IDisposable with
        member x.Dispose() =
            if File.Exists tempPath then File.Delete tempPath
