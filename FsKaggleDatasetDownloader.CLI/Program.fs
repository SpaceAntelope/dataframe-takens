namespace FsKaggleDatasetDownloader.CLI

open FsKaggleDatasetDownloader.Types

module Program =
    open System
    open System.IO
    open System.Net.Http
    open FsKaggleDatasetDownloader.Types.API
    open FsKaggleDatasetDownloader.Client.Kaggle
    open Argu

    let EnsureKaggleJsonExists path =
        if path |> (File.Exists >> not) then failwithf "Could not locate credential file in path '%s'" path

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
        let errorHandler =
            ProcessExiter
                (colorizer =
                    function
                    | ErrorCode.HelpText ->
                        CLI.PrintExamples()
                        None
                    | _ -> Some ConsoleColor.Red)

        let parser =
            ArgumentParser.Create<CLI.Args>(programName = "FsKaggleDatasetDownloader.CLI", errorHandler = errorHandler)

        let results = parser.ParseCommandLine argv

        //printfn "Got parse results %A" <| results.GetAllResults()

        let kaggleJsonPath =
            results.GetResult
                (CLI.Args.Credentials,
                 defaultValue =
                     Path.Combine
                         (Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".kaggle/kaggle.json"))
            |> EnsureKaggleJsonExists

        let destinationFolder = results.GetResult(CLI.Args.Output, defaultValue = ".")
        let (owner, dataset) = results.GetResult(CLI.Args.Dataset)

        let downloadMode =
            match results.TryGetResult CLI.Args.File with
            | Some file -> DatasetFile.Filename file
            | None -> DatasetFile.CompleteDatasetZipped

        if results.TryGetResult CLI.Args.WhatIf |> Option.isSome then
            0
        else
            use client = new HttpClient()

            { DatasetInfo =
                  { Owner = owner
                    Dataset = dataset
                    Request = downloadMode }
              AuthorizedClient =
                  kaggleJsonPath
                  |> Credentials.LoadFrom
                  |> Credentials.AuthorizeClient client
              DestinationFolder = destinationFolder
              Overwrite = results.TryGetResult CLI.Args.Overwrite |> Option.isSome
              CancellationToken = None
              ReportingCallback = Some Reporter.ProgressBar }
            |> DownloadDatasetAsync
            |> Async.AwaitTask
            |> Async.RunSynchronously

            0 // return an integer exit code
