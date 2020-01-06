namespace TakensTheorem.Core

open System
open XPlot.Plotly
open Microsoft.Data.Analysis
open System.Collections.Generic
open System.IO

module DataFrameColumnOperators =
    (*
     * Temporary inequality operators, presumably the dataframecolumn will overload them
     * over the elemntwise functions eventually.
    *)

    let inline (!>) (source: DataFrameColumn) = source :?> PrimitiveDataFrameColumn<_>

    let inline (!>!) (source: DataFrameColumn) = source :?> StringDataFrameColumn

    let inline (!<) (source: PrimitiveDataFrameColumn<'T>) = source :> DataFrameColumn

    let inline (!<!) (source: StringDataFrameColumn) = source :> DataFrameColumn

    let (/&/) (col1: PrimitiveDataFrameColumn<'T>) (col2: PrimitiveDataFrameColumn<'T>) =
        col1.And(col2) :?> PrimitiveDataFrameColumn<bool>

    let inline (/>) (col: PrimitiveDataFrameColumn<'T>) (value: 'T) = col.ElementwiseGreaterThan(value = value)
    let inline (>/) (value: 'T) (col: PrimitiveDataFrameColumn<'T>) = col.ElementwiseLessThan(value = value)

    let inline (/>=) (col: PrimitiveDataFrameColumn<'T>) (value: 'T) = col.ElementwiseGreaterThanOrEqual(value = value)
    let inline (>=/) (value: 'T) (col: PrimitiveDataFrameColumn<'T>) = col.ElementwiseLessThanOrEqual(value = value)

    let (/<) (col1: PrimitiveDataFrameColumn<_>) (value: 'T) = col1.ElementwiseLessThan(value = value)
    let (</) (value: 'T) (col: PrimitiveDataFrameColumn<_>) = col.ElementwiseGreaterThan(value = value)
    let (/<=) (col1: PrimitiveDataFrameColumn<_>) (value: 'T) = col1.ElementwiseLessThanOrEqual(value = value)
    let (<=/) (value: 'T) (col: PrimitiveDataFrameColumn<_>) = col.ElementwiseGreaterThanOrEqual(value = value)
