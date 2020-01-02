namespace TakensTheorem.Test

open System
open Xunit
open Microsoft.Data.Analysis
open TakensTheorem.Core
open System.IO

module DataFrameTests =

    [<Fact>]
    let ``Force System.Double when loading from csv``() =
        use fs = File.OpenRead(@"expected\temperature0-25 copy.csv")
        use ms = new MemoryStream()
        fs.CopyTo(ms)
        ms.Position <- 0L

        let frameWithSingle = DataFrame.LoadCsv(ms)

        let indexOfTypeSingle =
            frameWithSingle
            |> DataFrame.Columns
            |> Seq.mapi (fun i col -> i, col.DataType)
            |> Seq.filter (fun (i, colType) -> colType = typeof<single>)
            |> Seq.map fst

        ms.Position <- 0L
        let frameWithDouble = DataFrame.LoadCsvForceFloat ms

        let actualCols =
            frameWithDouble
            |> DataFrame.Columns
            |> Array.ofSeq

        for index in indexOfTypeSingle do
            Assert.Equal(typeof<float>, actualCols.[index].DataType)

        Assert.Equal(frameWithSingle |> DataFrame.Length, frameWithDouble |> DataFrame.Length)

    [<Theory>]
    [<InlineData(10, 20)>]
    [<InlineData(0, 20)>]
    [<InlineData(88, 99)>]
    [<InlineData(0, 99)>]
    [<InlineData(0, 0)>]
    [<InlineData(99, 99)>]
    [<InlineData(55, 55)>]
    let ``Primitive Column Slice`` (start, stop) =
        let source = PrimitiveDataFrameColumn("source", [ 0 .. 99 ])
        let expected = [| 0 .. 99 |].[start..stop] |> Array.map Nullable

        let actual =
            source
            |> DataFrameColumn.Slice(int64 start, int64 stop)
            |> DataFrameColumn.Values

        Assert.Equal(expected.Length, actual.Length)
        Assert.Equal<Nullable<int> []>(expected, actual)

    [<Theory>]
    [<InlineData(10, 20)>]
    [<InlineData(0, 20)>]
    [<InlineData(88, 99)>]
    [<InlineData(0, 99)>]
    [<InlineData(0, 0)>]
    [<InlineData(99, 99)>]
    [<InlineData(55, 55)>]
    let ``String Column Slice`` (start, stop) =
        let sourceValues = [| 0 .. 99 |] |> Array.map string
        let source = StringDataFrameColumn("source", sourceValues)
        let expected = sourceValues.[start..stop]

        let actual =
            source
            |> StringDataFrameColumn.Slice(int64 start, int64 stop)
            |> StringDataFrameColumn.Values

        Assert.Equal(expected.Length, actual.Length)
        Assert.Equal<string []>(expected, actual)
