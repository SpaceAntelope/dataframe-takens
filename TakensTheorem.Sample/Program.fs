namespace TakensTheorem.Sample

open System
open TakensTheorem.Core
open TakensTheorem.Core.ZipHelper
open TakensTheorem.Core.Common
open Microsoft.Data.Analysis

module Main =

    [<EntryPoint>]
    let main argv =
        printfn "Hello World from F#!"
        let path = @"~\Documents\Datasets\historical-hourly-weather-data.zip"
        use loader = new ZippedCsvLoader(path)
        let cityTable = loader.ToDataFrame "city_attributes.csv"
        let temperatureDF = loader.ToDataFrame "temperature.csv"
        let indexName = temperatureDF.Columns.[0].Name

        temperatureDF.[indexName] <- temperatureDF.[indexName]
                                     |> Seq.cast<string>
                                     |> Seq.map (DateTime.Parse)
                                     |> DataFrameColumn.FromValues indexName

        let start = DateTime(2015, 6, 22)
        let stop = DateTime(2015, 8, 31)

        let dateFilterColumn = 
            !> temperatureDF.[indexName]
            |> DataFrameColumn<DateTime>.CreateFilter 
                (fun (dt : Nullable<DateTime>) -> 
                    if dt.HasValue then 
                        dt.Value >= start && dt.Value <= stop
                    else false)

        let weatherDataMontreal = 
            temperatureDF.Filter(dateFilterColumn).["Montreal"]

        let origSignal = weatherDataMontreal.Clone()

        temperatureDF.[indexName].ElementwiseGreaterThanOrEqual(start)
        |> ignore
        
        0 // return an integer exit code
