// Learn more about F# at http://fsharp.org
namespace FsKaggleDatasetDownloader.CLI

open FsKaggleDatasetDownloader.Types

module Program =
    open System
    open System.IO
    open System.Net.Http
    open FsKaggleDatasetDownloader.Types.API
    open FsKaggleDatasetDownloader.Client.Kaggle

    let printHelp() =
        printfn "Usage:"
        printfn "Call executable with two parameters:"
        printfn "\tFirst param: Path to kaggle.json"
        printfn "\tSecond param: Dataset output path"
        printfn ""

    let EnsureKaggleJsonExists path =
        if path |> (File.Exists >> not) then
            printHelp()
            failwithf "Could not locate credential file in path '%s'" path

        printf "%s found..." path
        path

    let CreateOutputFolderIfMissing(path: string) =
        let fullPath =
            // if System.Text.RegularExpressions.Regex.IsMatch(path.Trim(), "^[\\\\//\\.]") then
            //     Path.Combine(Path.GetDirectoryName(Reflection.Assembly.GetExecutingAssembly().Location), path)
            //     |> Path.GetFullPath
            // else
                path

        if fullPath |> (File.Exists >> not) then Directory.CreateDirectory(fullPath) |> ignore

        printf "%s verified..." fullPath
        fullPath

    [<EntryPoint>]
    let main argv =
        if  isNull argv || Array.isEmpty argv then
            printHelp()
            failwith "Error: no parameters found."

        let kaggleJsonPath = EnsureKaggleJsonExists argv.[0]
        let destinationFolder = CreateOutputFolderIfMissing argv.[1]

        use client = new HttpClient()

        { DatasetInfo =
              { Owner = "selfishgene"
                Dataset = "historical-hourly-weather-data"
                Request = CompleteDatasetZipped }
          AuthorizedClient =
              kaggleJsonPath
              |> Credentials.LoadFrom
              |> Credentials.AuthorizeClient client
          DestinationFolder = destinationFolder
          CancellationToken = None
          ReportingCallback = Some Reporter.ProgressBar }
        |> DownloadDatasetAsync
        |> Async.AwaitTask
        |> Async.RunSynchronously

        0 // return an integer exit code
