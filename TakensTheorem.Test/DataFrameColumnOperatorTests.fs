namespace TakensTheorem.Test

open System
open Xunit
open Microsoft.Data.Analysis
open TakensTheorem.Core
open System.IO


module DataFrameColumnOperatorTests =

    open DataFrameColumnOperators

    [<Theory>]
    [<InlineData(10)>]
    let ``Column Greater Than Or Equal`` (value: int) =
        let values = [| 10 .. 20 |]

        let expected =
            values
            |> Array.map (fun i -> i >= value)
            |> DataFrameColumn.FromValues "filter"

        let col = DataFrameColumn.FromValues "filter" values
        let actual = (/>=) col value

        Assert.Equal<Nullable<bool> []>(expected |> DataFrameColumn.Values, actual |> DataFrameColumn.Values)


    [<Theory>]
    [<InlineData(10)>]
    let ``Column Greater Than`` (value: float) =
        let values = [| 10 .. 20 |] |> Array.map float

        let expected =
            values
            |> Array.map (fun i -> i > value)
            |> DataFrameColumn.FromValues "filter"

        let col = DataFrameColumn.FromValues "filter" values
        let actual = col /> value

        Assert.Equal<Nullable<bool> []>(expected |> DataFrameColumn.Values, actual |> DataFrameColumn.Values)