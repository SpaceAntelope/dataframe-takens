namespace TakensTheorem.Test

open System
open Xunit
open Microsoft.Data.Analysis
open TakensTheorem.Core
open System.IO
open KaggleClient

module KaggleTests =

    let dataPath = "../../../../Data"
    let filename = "historical-hourly-weather-data.zip"
    let user = "selfishgene"
    let dataUrl = DownloadDatasetUrl user filename
    // [<Fact>]
    let ``Download data`` () =
            let destinationPath = sprintf "%s/%s" dataPath filename
            if (File.Exists>>not) destinationPath
            then 
                

    //     //Assert.Equal(Path.GetFullPath dataPath), "")
    //     Assert.True(Directory.Exists dataPath)
    //     if (List.exists (String.IsNullOrEmpty) [username;key])
    //     then
    //         Assert.True(true)
    //     else
    //         let client =
    //             KaggleClient.CreateAuthorizedClient
    //                 { Username = username
    //                   Key = key }
                
    //         DownLoadFileAsync [|"selfishgene";"historical-hourly-weather-data"|] "/" client
    //         |> Async.StartImmediate

    //         Assert.True(File.Exists("historical-hourly-weather-data.zip"))
    ()