namespace FsKaggleDatasetDownloader.Types

open System.Net.Http
open FsKaggleDatasetDownloader.Types.Core
open System.Threading

module API =

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
            | Filename filename -> sprintf "%sdatasets/download/%s/%s/%s" Constants.BaseApiUrl x.Owner x.Dataset filename
            | CompleteDatasetZipped -> sprintf "%sdatasets/download/%s/%s" Constants.BaseApiUrl x.Owner x.Dataset

    type DownloadOptions =
        { DatasetInfo: DatasetInfo
          AuthorizedClient: AuthorizedClient
          DestinationFolder: string
          Overwrite: bool
          CancellationToken: CancellationToken option
          ReportingCallback: (ReportingData -> unit) option }
    
    