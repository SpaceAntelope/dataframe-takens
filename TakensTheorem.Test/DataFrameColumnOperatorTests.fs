namespace TakensTheorem.Test

open System
open Xunit
open Microsoft.Data.Analysis
open TakensTheorem.Core
open System.IO


module DataFrameColumnOperatorTests =

    open DataFrameColumnOperators

    [<Theory>]
    [<InlineData(10, 10, 20)>]
    [<InlineData(0, 10, 20)>]
    [<InlineData(10, 0, 0)>]
    [<InlineData(1, 0, 1)>]
    [<InlineData(0, 0, 0)>]
    let ``Column to value inclusive inequality`` (value: int, start: int, stop : int) =
        let values = [| start .. stop |]

        let expected =
            values
            |> Array.map (fun i -> i >= value)
            |> DataFrameColumn.FromValues "filter"

        let col = DataFrameColumn.FromValues "filter" values
        let actual = col />= value
        Assert.Equal<Nullable<bool> []>(expected |> DataFrameColumn.Values, actual |> DataFrameColumn.Values)

        let actual = value <=/ col
        Assert.Equal<Nullable<bool> []>(expected |> DataFrameColumn.Values, actual |> DataFrameColumn.Values)


    [<Theory>]
    [<InlineData(10, 10, 20)>]
    [<InlineData(0, 10, 20)>]
    [<InlineData(10, 0, 0)>]
    [<InlineData(1, 0, 1)>]
    [<InlineData(0, 0, 0)>]
    let ``Column to value inequality`` (value: float, start: int, stop: int) =
        let values = [| start .. stop |] |> Array.map float

        let expected =
            values
            |> Array.map (fun i -> i > value)
            |> DataFrameColumn.FromValues "filter"

        let col = DataFrameColumn.FromValues "filter" values
        
        let actual = col /> value
        Assert.Equal<Nullable<bool> []>(expected |> DataFrameColumn.Values, actual |> DataFrameColumn.Values)

        let actual = value </ col
        Assert.Equal<Nullable<bool> []>(expected |> DataFrameColumn.Values, actual |> DataFrameColumn.Values)

    [<Fact>]
    let ``Bitwise global AND operator``()=
        let left =     [1;1;0;1;0] |> List.map((=)1) |> DataFrameColumn.FromValues ""
        let right =    [1;1;1;0;0] |> List.map((=)1) |> DataFrameColumn.FromValues ""
        let expected = [1;1;0;0;0] |> List.map((=)1) |> Array.ofList //|> DataFrameColumn.FromValues ""
        
        let actual = (left /&/ right) |> DataFrameColumn.Values |> Array.map (fun i -> i.Value)        
        Assert.Equal<bool[]>(expected, actual)
        let actual = (right /&/ left) |> DataFrameColumn.Values |> Array.map (fun i -> i.Value)        
        Assert.Equal<bool[]>(expected, actual)