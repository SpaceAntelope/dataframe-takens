namespace TakensTheorem.Core

open System
open XPlot.Plotly
open Microsoft.Data.Analysis
open System.Collections.Generic
open System.IO
open DataFrameColumnOperators

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

    static member CreateFilter (predicate: Nullable<'T> -> bool) (source: PrimitiveDataFrameColumn<'T>) =
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
    open DataFrameColumnOperators
    open Common.Dictionary

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

    let Filter (filter: PrimitiveDataFrameColumn<bool>) (source: DataFrameColumn) = source.Clone(filter)

    let Length(source: DataFrameColumn) = source.Length
    
    let RightShift shiftLength fillValue (source: DataFrameColumn) =
        let result = source.Clone()
        
        for n in 0L .. (source.Length - 1L) do
            if n < shiftLength then result.[n] <- fillValue
            else result.[n] <- source.[n - shiftLength] 
        
        result

    let LeftShift shiftLength fillValue (source: DataFrameColumn) =
        let result = source.Clone()
        
        for n in 0L .. (source.Length - 1L) do
            if n >= source.Length - shiftLength then result.[n] <- fillValue
            else result.[n] <- source.[n + shiftLength] 
        
        result
        

    let MutualInformation delay (nBins: int) (data: PrimitiveDataFrameColumn<float>) =
        let mutable I = 0.0

        let xmax = data.Max() :?> float
        let xmin = data.Min() :?> float

        let length = data.Length
        let delayData = data |> DataFrameColumn.Slice(delay, length - 1L)
        let shortData = data |> DataFrameColumn.Slice(0L, length - delay - 1L)
        let sizeBin = Math.Abs(xmax - xmin) / float nBins
        let probInBin = Dictionary<float, float>()
        let conditionBin = Dictionary<float, PrimitiveDataFrameColumn<bool>>()
        let conditionDelayBin = Dictionary<float, PrimitiveDataFrameColumn<bool>>()

        let range = [ 0 .. nBins - 1 ] |> List.map float
        for h in range do
            if h |> notIn probInBin then
                //let sub1 = shortData.ElementwiseGreaterThanOrEqual(xmin + h * sizeBin)
                //let sub2 = shortData.ElementwiseLessThan(xmin + (h + 1.) * sizeBin)
                //conditionBin.Add(h, sub1.And(sub2) :?> PrimitiveDataFrameColumn<bool>)

                let filter = (shortData />= (xmin + h * sizeBin)) /&/ (shortData /< (xmin + (h + 1.) * sizeBin))
                conditionBin |> update (h, filter)

                //let pib = float ((!< shortData).Clone(filter).Length) / float shortData.Length
                let probability =
                    (shortData
                     |> Filter filter
                     |> Length
                     |> float) / float shortData.Length

                probInBin |> update (h, probability)

            for k in range do
                if k |> notIn probInBin then
                    let filter = (shortData />= (xmin + k * sizeBin)) /&/ (shortData /< (xmin + (k + 1.) * sizeBin))
                    let pib = float ((!<shortData).Clone(filter).Length) / float shortData.Length
                    conditionBin |> update (k, filter)
                    probInBin |> update (k, pib)

                if k |> notIn conditionDelayBin then
                    let filter = (delayData />= (xmin + k * sizeBin)) /&/ (delayData /< (xmin + (k + 1.) * sizeBin))
                    conditionDelayBin |> update (k, filter)
                
                let Phk =
                    (* With Pandas the right shifting of the right operand happens implicitly,
                       as the prexisting indices are matched 
                     *)
                    (shortData
                     |> Filter(conditionBin.[h] /&/ !>(RightShift delay false conditionDelayBin.[k]))
                     |> Length
                     |> float) / float shortData.Length

                if Phk <> 0. && probInBin.[h] <> 0. && probInBin.[k] <> 0. then
                    I <- I - (Phk * Math.Log(Phk / (probInBin.[h] * probInBin.[k])))
        I
