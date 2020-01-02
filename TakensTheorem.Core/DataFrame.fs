namespace TakensTheorem.Core

open System
open XPlot.Plotly
open Microsoft.Data.Analysis
open System.Collections.Generic
open System.IO


type DataFrameColumn<'T when 'T :> ValueType and 'T: struct and 'T: (new: unit -> 'T)> private () =

    static member Values(source: PrimitiveDataFrameColumn<'T>) =
        source
        |> Seq.cast<Nullable<'T>>
        |> Array.ofSeq

    static member Slice(start: int64, stop: int64) =
        fun (source: PrimitiveDataFrameColumn<'T>) ->
            let data = Array.init (int (stop - start) + 1) (fun i -> source.[start + int64 i])
            PrimitiveDataFrameColumn<'T>(source.Name, data)

    static member Trim(trimValue: 'T) =
        fun (source: PrimitiveDataFrameColumn<'T>) ->
            let trimValue = Nullable trimValue
            let mutable start = 0L
            let mutable stop = source.Length - 1L

            while source.[start] = trimValue do
                start <- start + 1L
            while source.[stop] = trimValue do
                stop <- stop - 1L

            let trimmed = (source |> DataFrameColumn.Values).[int start..int stop]

            PrimitiveDataFrameColumn<'T>(source.Name, trimmed)

    static member FromValues (name: string) (values: 'T seq) = PrimitiveDataFrameColumn<'T>(name, values)

    static member CreateFilter(predicate: Nullable<'T> -> bool) =
        fun (source: PrimitiveDataFrameColumn<'T>) ->
            source
            |> Seq.cast<Nullable<'T>>
            |> Seq.map predicate
            |> DataFrameColumn.FromValues(sprintf "%s filter" source.Name)

    static member Plot(source: PrimitiveDataFrameColumn<'T>) = Scatter(y = DataFrameColumn.Values source) |> Chart.Plot

[<RequireQualifiedAccess>]
module StringDataFrameColumn =
    let Values(source: StringDataFrameColumn) =
        source
        |> Seq.cast<string>
        |> Array.ofSeq

    let FromValues (name: string) (values: string seq) = StringDataFrameColumn(name, values)

    let Slice (start: int64, stop: int64) (source: StringDataFrameColumn) =
        let data = Array.init (int (stop - start) + 1) (fun i -> source.[start + int64 i])
        StringDataFrameColumn(source.Name, data)

    let Trim (trimValue: string) (source: StringDataFrameColumn) =
        let mutable start = 0L
        let mutable stop = source.Length - 1L

        while source.[start] = trimValue do
            start <- start + 1L
        while source.[stop] = trimValue do
            stop <- stop - 1L

        //let trimmed = (source |> AsArrayOfString).[int start..int stop]
        let trimmed = Array.init (int (stop - start) + 1) (fun n -> source.[start + int64 n])

        StringDataFrameColumn(source.Name, trimmed)

    let CreateFilter (predicate: string -> bool) (source: StringDataFrameColumn) =
        source
        |> Seq.cast<string>
        |> Seq.map predicate
        |> DataFrameColumn.FromValues(sprintf "%s filter" source.Name)


[<RequireQualifiedAccess>]
module DataFrameColumn =

    let Rolling<'T when 'T :> ValueType> windowSize (groupFunction: 'T [] -> 'T) (source: DataFrameColumn) =
        source
        |> Seq.cast<'T>
        |> Seq.windowed windowSize
        |> Seq.map groupFunction

    let TakensEmbedding<'T when 'T: equality and 'T :> ValueType and 'T: (new: unit -> 'T) and 'T: struct> (delay: int)
        (dimension: int) (data: PrimitiveDataFrameColumn<'T>) =
        (* This function returns the Takens embedding of data with delay into dimension,
         * delay*dimension must be < len(data)
         *)
        data
        |> DataFrameColumn.Values
        |> Takens.Embedding delay dimension


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
