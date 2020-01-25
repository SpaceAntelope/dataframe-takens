namespace FsKaggleDatasetDownloader.Client

open System.Threading
open System.IO
open FsKaggleDatasetDownloader.Types.API

module Kaggle =
    let DownloadDatasetAsync(options: DownloadOptions) =
        let url = options.DatasetInfo.ToUrl()
        let fileName =
            options.DatasetInfo.Request.ToOption() |> Option.defaultValue (options.DatasetInfo.Dataset + ".zip")
        let destinationFile = Path.Combine(options.DestinationFolder, fileName)

        if File.Exists destinationFile 
        then
            if options.Overwrite
            then File.Delete destinationFile
            else failwithf "File [%s] already exists." destinationFile

        let (AuthorizedClient client) = options.AuthorizedClient
        let token = options.CancellationToken |> Option.defaultValue (CancellationToken())

        Core.DownloadFileAsync url destinationFile client token options.ReportingCallback
