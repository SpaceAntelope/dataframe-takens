namespace TakensTheorem.Test

open System
open Xunit
open Microsoft.Data.Analysis
open TakensTheorem.Core
open System.IO
open System.Collections.Generic


module DataFrameColumnTests =
    open DataFrameColumnOperators

    [<Fact>]
    let ``Filter primitive column by bool column``() =
        let predicate = fun i -> i % 3 = 0
        let values = [| 0 .. 12 |]

        let filter =
            values
            |> Array.map predicate
            |> DataFrameColumn.FromValues ""

        let col = DataFrameColumn.FromValues "" values

        let expected = [| 0; 3; 6; 9; 12 |] |> Array.map Nullable

        //let actual = !> (col :> DataFrameColumn).Clone(filter) |> DataFrameColumn.Values
        let actual =
            col
            |> DataFrameColumn.Filter filter
            |> (!>)
            |> DataFrameColumn.Values

        Assert.NotEmpty(actual)
        Assert.Equal<Nullable<int> []>(expected, actual)

    [<Fact>]
    let ``Filter string column by bool column``() =
        let predicate = fun (str: string) -> str.StartsWith("a") || str.EndsWith("a")
        let values = [| "a"; "ada"; "b"; "beth"; "ares"; "kara" |]

        let filter =
            values
            |> Array.map predicate
            |> DataFrameColumn.FromValues ""

        let col = values |> StringDataFrameColumn.FromValues ""

        let expected = values |> Array.filter predicate

        let actual =
            col
            |> DataFrameColumn.Filter filter
            |> (!>!)
            |> StringDataFrameColumn.Values

        Assert.NotEmpty(actual)
        Assert.Equal<string []>(expected, actual)

    [<Fact>]
    let ``Filter DateTime column by bool column``() =
        let predicate = fun (dt: DateTime) -> dt.DayOfWeek = DayOfWeek.Friday
        let values = Common.dateRange (DateTime(2020, 1, 1)) (DateTime(2020, 1, 31)) (fun dt -> dt.AddDays(1.))

        let filter =
            values
            |> Seq.map predicate
            |> DataFrameColumn.FromValues ""

        let col = values |> DataFrameColumn.FromValues ""

        let expected =
            values
            |> Seq.filter predicate
            |> Seq.map Nullable
            |> Array.ofSeq

        let actual =
            col
            |> DataFrameColumn.Filter filter
            |> (!>)
            |> DataFrameColumn.Values

        Assert.NotEmpty(actual)
        Assert.Equal<Nullable<DateTime> []>(expected, actual)

    [<Fact>]
    let ``Check operator overloading``() =
        let column = [ 10 .. 20 ] |> DataFrameColumn.FromValues ""
        let value = 100

        let actual =
            !>(!<column + value)
            |> DataFrameColumn.Values
            |> Array.map (fun x -> x.Value)
        Assert.Equal<int []>([| 110 .. 120 |], actual)

        let actual =
            !>(value + !<column)
            |> DataFrameColumn.Values
            |> Array.map (fun x -> x.Value)
        Assert.Equal<int []>([| 110 .. 120 |], actual)
