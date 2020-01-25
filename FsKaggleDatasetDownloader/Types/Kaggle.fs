namespace FsKaggleDatasetDownloader

open System.Net.Http
open Core
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
          CancellationToken: CancellationToken option
          ReportingCallback: (ReportingInfo -> unit) option }
    
    /// <summary>Deserialization of kaggle.json</summary>
    type Credentials() =
        member val username: string = null with get, set
        member val key: string = null with get, set