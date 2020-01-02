namespace TakensTheorem.Core

open System
open XPlot.Plotly
open Microsoft.Data.Analysis
open System.Collections.Generic
open System.IO

[<RequireQualifiedAccess>]
module DataFrame =
    let Columns(source: DataFrame) = source.Columns |> Seq.cast<DataFrameColumn>

    let PlotColumn<'TIndex, 'TColumn when 'TIndex :> ValueType and 'TColumn :> ValueType> (indexName: string)
        (columnName: string) (source: DataFrame) =

        Scatter(x = Seq.cast<'TIndex> source.[indexName], y = Seq.cast<'TColumn> source.[columnName]) |> Chart.Plot

    let Length(source: DataFrame) = source.Rows.Count

    let LoadCsvForceFloat(ms: MemoryStream) =

        let frame = DataFrame.LoadCsv(ms, addIndexColumn = true)
        let columns = frame |> Columns

        let dataTypes =
            columns
            |> Seq.map (fun col ->
                if col.DataType = typeof<single> then typeof<float> else col.DataType)
            |> Array.ofSeq

        let names =
            columns
            |> Seq.map (fun col -> col.Name)
            |> Array.ofSeq

        ms.Position <- 0L
        DataFrame.LoadCsv(ms, addIndexColumn = true, header = true, dataTypes = dataTypes, columnNames = names)
