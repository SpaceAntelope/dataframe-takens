(*
 * For VSCODE be sure to set { "FSharp.useSdkScripts": true } if you are going to be using the fsi.
 * This switches to netcore fsi.
 *)
#load @".paket\load\netstandard2.1\main.group.fsx"
#r "../TakensTheorem.Core.dll"
#r "System.Net.Http"

open TakensTheorem.Core
open KaggleClient
open System.Net.Http
open System
open System.IO

let kaggleJsonPath = Path.Combine(__SOURCE_DIRECTORY__, "../kaggle.json")
let destinationFolder = Path.Combine(__SOURCE_DIRECTORY__, "../../Data")
let client = new HttpClient()

if (Directory.Exists >> not) destinationFolder then Directory.CreateDirectory destinationFolder |> ignore


let report (file: string, bytesRead: int64, totalBytes: int64) =
    let status =
        sprintf "Downloading file [%s] -- %dKB of %.02fMB received." (Path.GetFileName(file).Replace("\\", "/"))
            bytesRead (float totalBytes / 1024.0 / 1024.0)

    let percentage = float bytesRead / float totalBytes
    let barTotalWidth = Console.WindowWidth - 11 - status.Length

    let barCompleted =
        percentage * float barTotalWidth
        |> Math.Ceiling
        |> int

    let percentageStr =
        sprintf " %.2f%%"
            ((if percentage > 0.99 then 1.0 else percentage)
             * 100.00)

    printf "[%s] %s\r" ((percentageStr.PadLeft(barCompleted, '|')).PadRight(barTotalWidth, ' ')) status



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
  ReportingCallback = Some(report) }
|> DownloadDatasetAsync
|> Async.AwaitTask
|> Async.RunSynchronously
