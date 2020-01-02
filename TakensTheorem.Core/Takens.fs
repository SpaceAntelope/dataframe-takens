namespace TakensTheorem.Core

open System
open Microsoft.Data.Analysis
open System.Collections.Generic
open Common

module ColumnOps = 
    let (/&) (col1 : PrimitiveDataFrameColumn<'T>) (col2 : PrimitiveDataFrameColumn<'T>) =
        col1.And(col2) :?> PrimitiveDataFrameColumn<'T>

    let inline (/>) (col1 : PrimitiveDataFrameColumn<_>) value =
        col1.ElementwiseGreaterThan(value)
 
    let inline (/>=) (col1 : PrimitiveDataFrameColumn<_>) value =
        col1.ElementwiseGreaterThanOrEqual(value)

module Takens =
    let Embedding<'T> (delay: int) (dimension: int) (data: 'T []) =
        (* This function returns the Takens embedding of data with delay into dimension,
         * delay*dimension must be < len(data)
         *)
        let length = data |> Array.length
        if (delay * dimension) > length then failwith "Delay times dimension exceed length of data!"

        seq {
            yield data.[0..(length - delay * dimension - 1)]

            for i in 1 .. dimension - 1 do
                let start = i * delay
                let stop = length - delay * (dimension - i) - 1
                yield data.[start..stop]
        }

    // let mutualInformation delay (nBins: int) (data: PrimitiveDataFrameColumn<float>) =
    //     let I = 0

    //     let xmax = data.Max() :?> float
    //     let xmin = data.Min() :?> float

    //     let length = data.Length
    //     let delayData = data |> DataFrameColumn.Slice(delay, length - 1L)
    //     let shortData = data |> DataFrameColumn.Slice(0L, length - delay - 1L)
    //     let sizeBin = Math.Abs(xmax - xmin) / float nBins
    //     let probInBin = Dictionary<float, PrimitiveDataFrameColumn<bool>>()
    //     let conditionBin = Dictionary<float, PrimitiveDataFrameColumn<bool>>()

    //     let range = [ 0 .. nBins - 1 ] |> List.map float
    //     for h in range do
    //         if (not << probInBin.ContainsKey) h then
    //             let sub1 = shortData.ElementwiseGreaterThanOrEqual(xmin + h * sizeBin)
    //             let sub2 = shortData.ElementwiseLessThan(xmin + (h + 1.) * sizeBin)
    //             conditionBin.Add(h, sub1.And(sub2) :?> PrimitiveDataFrameColumn<bool>)


    //         ()
