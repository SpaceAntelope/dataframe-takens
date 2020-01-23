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

    [<Fact>]
    let ``LeftShift primitive column``() =
        let column = [ 1 .. 10 ] |> DataFrameColumn.FromValues "col"

        let expected = [| 5; 6; 7; 8; 9; 10; -1; -1; -1; -1 |] |> Array.map Nullable

        let actual =
            column
            |> DataFrameColumn.LeftShift 4L -1
            |> (!>)
            |> DataFrameColumn.Values

        Assert.Equal<Nullable<int> []>(expected, actual)

    [<Fact>]
    let ``LeftShift string column``() =
        let column =
            [ 1 .. 10 ]
            |> Seq.map string
            |> StringDataFrameColumn.FromValues "col"

        let expected = [| 5; 6; 7; 8; 9; 10; -1; -1; -1; -1 |] |> Array.map string

        let actual =
            column
            |> DataFrameColumn.LeftShift 4L "-1"
            |> (!>!)
            |> StringDataFrameColumn.Values

        Assert.Equal<string []>(expected, actual)

    [<Fact>]
    let ``RightShift primitive column``() =
        let column = [ 1 .. 10 ] |> DataFrameColumn.FromValues "col"

        let expected = [| -1; -1; -1; -1; 1; 2; 3; 4; 5; 6 |] |> Array.map Nullable

        let actual =
            column
            |> DataFrameColumn.RightShift 4L -1
            |> (!>)
            |> DataFrameColumn.Values

        Assert.Equal<Nullable<int> []>(expected, actual)

    [<Fact>]
    let ``RightShift string column``() =
        let column =
            [ 1 .. 10 ]
            |> Seq.map string
            |> StringDataFrameColumn.FromValues "col"

        let expected = [| -1; -1; -1; -1; 1; 2; 3; 4; 5; 6 |] |> Array.map string

        let actual =
            column
            |> DataFrameColumn.RightShift 4L "-1"
            |> (!>!)
            |> StringDataFrameColumn.Values

        Assert.Equal<string []>(expected, actual)

    [<Fact>]
    let ``Equality comparison for primitive columns is meaningless``() =
        let left = [ 1 .. 10 ] |> DataFrameColumn.FromValues ""
        let right = [ 1 .. 10 ] |> DataFrameColumn.FromValues ""
        let right' = [20 .. -1 .. 10] |> DataFrameColumn.FromValues ""

        Assert.False((left = right))
        Assert.False((left = right'))

    [<AbstractClass>]
    type TestApply() =

        member x.``Apply transformation to column`` (source: 'T [], expected: 'T [],
                                                     transformation: Func<'T Nullable, int64, 'T Nullable>) =

            let expectedColumn = expected |> DataFrameColumn.FromValues ""
            let actualColumn = source |> DataFrameColumn.FromValues ""

            actualColumn |> DataFrameColumn.Apply transformation

            let p = expectedColumn = actualColumn
            ()
//Assert.True(expectedColumn = actualColumn)
