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
            tempDF.["Montreal"] :?> PrimitiveDataFrameColumn<single>
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
                      .DropNulls(DropNullOptions.Any)).["Montreal"]
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
                      .DropNulls(DropNullOptions.Any)).["Montreal"]
            |> (!>)
            |> DataFrameColumn<float>.Values
            |> Array.ofSeq

        let actual =
            DataFrame.LoadCsv(@"expected\MontrealSlice.csv",
                              dataTypes =
                                 [| typeof<string>
                                    typeof<float> |], 
                              columnNames = [| "datetime"; "Montreal" |])
                      .["Montreal"]
            |> DataFrameColumn.Rolling<float> 24 (Seq.average)
            |> Seq.map Nullable
            |> Array.ofSeq

        for i in 0 .. expected.Length - 1 do
            if expected.[i].HasValue
            then Assert.Equal(expected.[i].Value, actual.[i].Value, 6)
            else Assert.False(actual.[i].HasValue)


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

    [<Fact>]
    let ``Mutual Information Calculation for montreal high pass filtered data``() =
        let frame =
            DataFrame.LoadCsv (
                "data/montreal_high_pass_filtered.csv", 
                dataTypes = [| typeof<string>; typeof<float> |],
                columnNames = [| "datetime"; "Montreal" |], header = false)
        
        let column = frame.["Montreal"] :?> PrimitiveDataFrameColumn<float>

        let expected = [|
            -2.6123319429357847
            -2.6105622604978813
            -2.6084167859085237
            -2.6062719339399316
            -2.604121512212586
            -2.602125548325199
            -2.600248996403993
            -2.598372588327039
            -2.596551096868427
            -2.5946066618393084
            -2.5926609733607022
            -2.5906467769276964
            -2.5883990428231463
            -2.586143083176258
            -2.5840835409427254
            -2.582044793030632
            -2.580004287731667
            -2.577910562566282
            -2.5758162830222617
            -2.573721462584162
        |]

        let actual = [|for i in 1L..20L do DataFrameColumn.MutualInformation i 16 column|]

        Assert.Equal<float[]>(expected, actual)
        
    [<Fact>]
    let ``Nearest neighbors result on filtered temperature data``() =
        
        let frame =
            DataFrame.LoadCsv (
                "data/montreal_high_pass_filtered.csv", 
                dataTypes = [| typeof<string>; typeof<float> |],
                columnNames = [| "datetime"; "Montreal" |], header = false)
        
        let column = 
            !> frame.["Montreal"]  
            |> DataFrameColumn<float>.Slice(0L, 100L)
        
        let expectedDistances = [|
            1.1961186825636652; 1.1570174532619988; 0.924713365874955; 0.8416010875854391; 0.8216006872174546; 0.5530012259505771; 0.5426822482535232; 0.6724128428653727; 0.38928696837920523; 
            0.5687269699244578; 0.8550502097017815; 0.992095216399437; 1.163184503522989; 1.6779094719858654; 0.9352754273474005; 0.8289324376038884; 1.0131224766917089; 0.20849272652496934; 0.4270836524389538; 
            0.43229543572649237; 0.3469692764776467; 0.3974803541616503; 1.6095473979938872; 0.9887748802880497; 1.4635510830062926; 1.4676361402873157; 0.5134590298626094; 0.596523232585226; 0.5975882968812912;
            0.32368928002018066; 0.32368928002018066; 0.8341356356613852; 0.8369587108574873; 1.3354967741183157; 1.3302934076162407; 0.928003692970363; 1.0238870109104385; 1.1090832010605822; 0.7829904069848711;
            0.8318188007977452; 1.100240913485785; 1.0900206730644235; 1.0998700522990594; 1.3364984334072176; 0.42436442058488166; 1.0621702078042041; 1.6909302117186376; 0.9887748802880497; 1.2433812777003848;
            1.4124712005421782; 1.0037014249003011; 1.1039671934595245; 0.7688038384489686; 0.5278644689403048; 0.47574074551663265; 0.34743119822488; 0.5980228752941309; 0.5806215285149936; 0.8550502097017815;
            0.992095216399437; 1.163184503522989; 1.720655550239235; 1.7481300192594764; 1.5170875268868633; 2.435662563862199; 1.912558087807024; 2.1250262273709906; 2.1269591222304234; 2.045241726663645;
            0.8312465998257177; 0.9238644155572081; 1.174643454326043; 0.5302557378943132; 0.6172019368912848; 0.30462284570457987; 0.36569303735471936; 0.5344975655416704; 0.4591987587090555; 0.31871490819863696;
            0.4932505385196668; 0.5489389212253852; 0.5864776861819431; 0.530745839205082; 0.34754870761758744; 0.3742840387548993; 0.37031775957351454; 0.25759841700637487; 0.47522290091640834; 0.4827618748178141;
            0.60676624640614; 0.6203436267729358; 0.6682426684138815; 0.9553589712191689; 1.092853124344269; 1.1078800764925167; 1.216486787502082; 0.6319017537470571; 0.637849577355944; 0.5055640071026929;
            0.22586907773703152;
        |]

        let embeddedData = 
            column
            |> DataFrameColumn.Values 
            |> Takens.Embedding 1 5 
            |> Seq.map (Array.map (fun nullable -> float nullable.Value))
            |> Array.ofSeq
            //|> transpose

        let actualDistances = 
            Accord.MachineLearning.KNearestNeighbors(k=2)
                .Learn(embeddedData, Array.zeroCreate<int> (embeddedData.Length))
            |> KNN.Distances embeddedData 
            
        

        Assert.Equal<float[]>(expectedDistances, actualDistances |> (Array.map snd>>Array.last));
        