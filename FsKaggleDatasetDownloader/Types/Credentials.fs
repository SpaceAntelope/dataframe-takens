namespace FsKaggleDatasetDownloader.Types

open API
open System
open System.IO
open System.Net.Http
open System.Net.Http.Headers
open System.Text.Json

module Credentials =
    /// <summary>Deserialization of kaggle.json</summary>
    type Credentials() =
            member val username: string = null with get, set
            member val key: string = null with get, set

    let LoadFrom(path: string): Credentials =
        use reader = new StreamReader(path)
        let json = reader.ReadToEnd()
        JsonSerializer.Deserialize(json)

    let AuthorizeClient (client: HttpClient) (auth: Credentials) =
        let authToken =
            sprintf "%s:%s" auth.username auth.key
            |> Text.ASCIIEncoding.ASCII.GetBytes
            |> Convert.ToBase64String

        client.DefaultRequestHeaders.Authorization <- AuthenticationHeaderValue("Basic", authToken)

        AuthorizedClient client