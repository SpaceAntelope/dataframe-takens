namespace TakensTheorem.Core

open System
open XPlot.Plotly
open Microsoft.Data.Analysis
open System.Collections.Generic
open System.IO

module DataFrameColumnOperators =
    let inline (!>) (source: DataFrameColumn) = source :?> PrimitiveDataFrameColumn<_>

    let inline (!>!) (source: DataFrameColumn) = source :?> StringDataFrameColumn

    let inline (!<) (source: PrimitiveDataFrameColumn<'T>) = source :> DataFrameColumn

    let inline (!<!) (source: StringDataFrameColumn) = source :> DataFrameColumn

    let (/&) (col1: PrimitiveDataFrameColumn<'T>) (col2: PrimitiveDataFrameColumn<'T>) =
        col1.And(col2) :?> PrimitiveDataFrameColumn<bool>

    let inline (/>) (col1: PrimitiveDataFrameColumn<'T>) (value: 'T) = col1.ElementwiseGreaterThan(value = value)

    let inline (/>=) (col1: PrimitiveDataFrameColumn<'T>) (value: 'T) =
        col1.ElementwiseGreaterThanOrEqual(value = value)

    let (/<) (col1: PrimitiveDataFrameColumn<_>) (value: 'T) = col1.ElementwiseLessThan(value = value)
