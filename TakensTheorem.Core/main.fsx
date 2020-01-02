#load @"C:\Users\cernu\Source\Repos\DS-AI\TakensTheorem\TakensTheorem.Core\.paket\load\netstandard2.0\main.group.fsx"
#r @"C:\Users\cernu\Source\Repos\DS-AI\TakensTheorem\TakensTheorem.Core\bin\Debug\netstandard2.0\TakensTheorem.Core.dll"

open System
open System.IO
open System.IO.Compression
open NumSharp
open TakensTheorem.Core
open ZipHelper
open Microsoft.Data.Analysis
open System.Collections.Generic

let path = @"~\Documents\Datasets\historical-hourly-weather-data.zip"

let loader = new ZippedCsvLoader(path)
let cityTable: DataFrame = loader.ToDataFrame "city_attributes.csv"
let temperatureDF: DataFrame = loader.ToDataFrame "temperature.csv"

temperatureDF.["datetime"] <- temperatureDF.["datetime"]
                              |> Seq.cast<string>
                              |> Seq.map (DateTime.Parse)
                              |> DataFrame.AsColumn "datetime"


// let t = Array.scan (fun state current -> if current <> DateTime(2015,8,31)  ) DateTime(2015,6,22)

//  seq {
//     while
// }

let start = DateTime(2015, 6, 22)
let stop = DateTime(2015, 8, 22)

stop - start


Common.dateRange start stop (fun dt -> dt.AddHours(1.)) |> Seq.take 10 |> Array.ofSeq

start.AddHours(1.)
start.Ticks

Seq.unfold (fun (state:DateTime) ->
    if state.Ticks <= stop.Ticks 
    then Some(state, state.AddHours(1.)) 
    else None) start
|> Seq.mapi (sprintf "%d %O")
|> Array.ofSeq