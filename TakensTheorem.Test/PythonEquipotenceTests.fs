namespace TakensTheorem.Test

module PythonEquipotenceTests =

    open System
    open Xunit
    open Microsoft.Data.Analysis
    open TakensTheorem.Core
    open TakensTheorem.Core.Common    
    open TakensTheorem.Core.DataFrameColumnOperators

    [<Fact>]
    let ``Embeddings same results as in python``() =
        let dimension = 2
        let delay = 5
        let dfExpected = DataFrame.LoadCsv("expected/montreal0-25.csv")

        Assert.Equal(2L, dfExpected.Rows.Count)

        let expected =
            dfExpected.Rows
            |> Seq.map (Seq.cast<Nullable<single>> >> Array.ofSeq)
            |> Array.ofSeq

        let tempDF = DataFrame.LoadCsv("expected/temperature0-25.csv")
        let indexName = tempDF.Columns.[0].Name
        tempDF.[indexName] <- tempDF.[indexName]
                              |> Seq.cast<string>
                              |> Seq.map (DateTime.Parse)
                              |> fun values -> PrimitiveDataFrameColumn<DateTime>(indexName, values)

        let actual =
            tempDF.["Montreal"]  :?> PrimitiveDataFrameColumn<single>
            |> DataFrameColumn.TakensEmbedding<single> delay dimension //data
            |> Seq.map Array.ofSeq
            |> Array.ofSeq

        let expectedRowCount = expected |> Seq.length

        let expectedColCount =
            expected
            |> Seq.head
            |> Seq.length

        let actualRowCount = actual |> Seq.length

        let actualColCount =
            actual
            |> Seq.head
            |> Seq.length

        Assert.Equal(expectedRowCount, actualRowCount)
        Assert.Equal(expectedColCount, actualColCount)
        Assert.Equal<Nullable<single> [] []>(expected, actual)

    [<Fact>]
    let ``Common.dateRange to pandas.date_range``() =
        let start = DateTime(2015, 6, 22)
        let stop = DateTime(2015, 8, 31)
        let actual = Common.dateRange start stop (fun dt -> dt.AddHours(1.)) |> List.ofSeq

        Assert.Equal(1681, actual |> List.length)
        Assert.Equal(DateTime(2015, 6, 22, 0, 0, 0), actual |> List.head)
        Assert.Equal(DateTime(2015, 8, 31, 0, 0, 0), actual |> List.last)

    [<Fact>]
    let ``Load csv numeric fidelity``() =
        (*
            LoadCsv misses double for float
        *)

        let expected =
            @"expected\MontrealRollingWindow24Mean.csv"
            |> System.IO.File.ReadAllLines
            |> Seq.skip 1
            |> Seq.map (fun line -> line.Split(',') |> Seq.last)
            |> Seq.filter (String.IsNullOrWhiteSpace >> not)
            |> Array.ofSeq

        let actual =
            (DataFrame.LoadCsv(@"expected\MontrealRollingWindow24Mean.csv",
                               dataTypes =
                                   [| typeof<string>
                                      typeof<float> |], columnNames = [| "datetime"; "Montreal" |], header = true)
                      .DropNulls(DropNullOptions.Any))
                      .["Montreal"] 
            |> (!>)
            |> DataFrameColumn<float>.Values
            |> Seq.map string
            |> Array.ofSeq

        for i in 0 .. expected.Length - 1 do
            Assert.Equal(expected.[i], actual.[i])

    [<Fact>]
    let ``Pandas centered rolling window with montreal temps``() =
        let expected =
            (DataFrame.LoadCsv(@"expected\MontrealRollingWindow24Mean.csv",
                               dataTypes =
                                   [| typeof<string>
                                      typeof<float> |], columnNames = [| "datetime"; "Montreal" |], header = true)
                      .DropNulls(DropNullOptions.Any))
                      .["Montreal"]
            |> (!>)
            |> DataFrameColumn<float>.Values
            |> Array.ofSeq

        let actual =
            DataFrame.LoadCsv(@"expected\MontrealSlice.csv",
                dataTypes =
                     [| typeof<string>
                        typeof<float> |], columnNames = [| "datetime"; "Montreal" |])
                .["Montreal"]
            |> DataFrameColumn.Rolling<float> 24 (Seq.average)
            |> Seq.map Nullable
            |> Array.ofSeq

        for i in 0 .. expected.Length - 1 do
            if expected.[i].HasValue 
            then 
                Assert.Equal(expected.[i].Value, actual.[i].Value, 6)
            else 
                Assert.False(actual.[i].HasValue)


    [<Fact>]
    let CheckDataTypes() =
        let frame =
            DataFrame.LoadCsv
                (@"expected\MontrealRollingWindow24Mean.csv",
                 dataTypes =
                     [| typeof<string>
                        typeof<float> |], columnNames = [| "datetime"; "Montreal" |], header = true)

        let types =
            frame.Columns
            |> Seq.cast<DataFrameColumn>
            |> Seq.map (fun col -> col.DataType)
            |> Array.ofSeq

        Assert.Equal<Type []>
            ([| typeof<string>
                typeof<float> |], types)
