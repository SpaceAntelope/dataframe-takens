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
    let ``Nearest neighbors distances for filtered temperature data sample``() =
        
        let frame =
            DataFrame.LoadCsv (
                "data/montreal_high_pass_filtered.csv", 
                dataTypes = [| typeof<string>; typeof<float> |],
                columnNames = [| "datetime"; "Montreal" |], header = false)
        
        let column = 
            !> frame.["Montreal"]  
            |> DataFrameColumn<float>.Slice(0L, 99L)
        
        let expectedDistances = [|
            1.2433812777003848; 1.4124712005421782; 1.0217017963985024; 1.0217017963985024; 1.6319048483376664; 1.4845287516863876; 1.0573812252710937; 1.164218931509905; 0.7113005315480785;
            0.698837899460396; 0.8550502097017815; 0.992095216399437; 1.163184503522989; 1.720655550239235; 1.6663037787228314; 1.767004597200551; 1.5143441512197708; 0.4275111475154998; 0.4275111475154998;
            0.6664694643229622; 0.5344873501126649; 0.5344873501126649; 2.040828392960491; 0.9887748802880497; 1.4635510830062926; 1.4676361402873157; 1.2561608177300156; 1.3871023654447523; 1.164218931509905;
            0.32368928002018066; 0.32368928002018066; 1.4792988303633063; 1.554187183175688; 1.9415338925454664; 1.949243352577961; 1.9705137716609509; 1.9705137716609509; 1.6606203633941534; 1.6663037787228314;
            1.767004597200551; 1.807592532113561; 2.06666545656893; 1.7185687581207987; 2.6075844305601663; 1.939776768012792; 1.9375267614639071; 2.040828392960491; 0.9887748802880497; 1.2433812777003848;
            1.4124712005421782; 1.4060996586300896; 1.2561608177300156; 1.3871023654447523; 0.5810447487069454; 0.5844084630943096; 1.2839257277217189; 0.7113005315480785; 0.698837899460396; 0.8550502097017815;
            0.992095216399437; 1.163184503522989; 1.720655550239235; 2.1110009609878304; 2.0274046032667266; 2.738142099987145; 2.743832528814515; 2.731196848262349; 3.3050755113032193; 3.189598542038574;
            2.5506749995100706; 2.125951340968723; 2.508903390372347; 1.6158043544332377; 1.8007972617086312; 1.3939386444333068; 1.0573812252710937; 0.9780892378979241; 1.0031869330137297; 0.6989770315432103;
            0.6989770315432103; 1.2457756501418507; 1.1496311772694623; 1.6502295515826972; 1.6196476549057968; 1.674256855252845; 1.6606203633941534; 1.7358819164268187; 1.807592532113561; 1.7185687581207987;
            1.8617847246959534; 1.8617847246959534; 2.188939169438026; 3.3050755113032193; 2.9259276318707106; 2.125951340968723;
        |]

            let expectedIndices = [|
                [| 0; 48|];[| 1; 49|];[| 2;  3|];[| 3;  2|];[| 4;  5|];[| 5;  6|];[| 6; 75|];[| 7; 28|];[| 8; 56|];[| 9; 57|];[|10; 58|];[|11; 59|];[|12; 60|];[|13; 61|];[|14; 38|];[|15; 39|];[|16; 21|];[|17; 18|];
                [|18; 17|];[|19; 18|];[|20; 21|];[|21; 20|];[|22; 46|];[|23; 47|];[|24;  0|];[|25; 49|];[|26; 51|];[|27; 52|];[|28;  7|];[|29; 30|];[|30; 29|];[|31; 55|];[|32; 81|];[|33; 82|];[|34; 83|];[|35; 36|];
                [|36; 35|];[|37; 85|];[|38; 14|];[|39; 15|];[|40; 87|];[|41; 87|];[|42; 88|];[|43; 16|];[|44; 45|];[|45; 18|];[|46; 22|];[|47; 23|];[|48;  0|];[|49;  1|];[|50; 26|];[|51; 26|];[|52; 27|];[|53; 30|];
                [|54; 30|];[|55;  7|];[|56;  8|];[|57;  9|];[|58; 10|];[|59; 11|];[|60; 12|];[|61; 13|];[|62; 86|];[|63; 87|];[|64; 88|];[|65; 91|];[|66; 91|];[|67; 92|];[|68; 93|];[|69; 44|];[|70; 94|];[|71; 24|];
                [|72;  1|];[|73; 26|];[|74; 27|];[|75;  6|];[|76; 54|];[|77; 78|];[|78; 79|];[|79; 78|];[|80;  8|];[|81;  8|];[|82; 58|];[|83; 59|];[|84; 60|];[|85; 37|];[|86; 14|];[|87; 40|];[|88; 42|];[|89; 90|];
                [|90; 89|];[|91; 90|];[|92; 67|];[|93; 69|];[|94; 70|]
            |]

        let embeddedData = 
            column
            |> DataFrameColumn.Values 
            |> Takens.Embedding 1 5 
            |> Seq.map (Array.map (fun nullable -> float nullable.Value))
            |> Array.ofSeq
            |> transpose

        let actualDistances = 
            Accord.MachineLearning.KNearestNeighbors(2).Learn(embeddedData, Array.zeroCreate<int> (embeddedData.Length))
            |> KNN.Distances embeddedData 
            |> Array.map (snd>>Array.last)

        Assert.Equal<float[]>(expectedDistances, actualDistances)
            
        let (distances, indices) = 
            KNN.DistancesWithIndices 2 embeddedData
            |> Array.unzip
                
        Assert.Equal<float[]>(expectedDistances, distances |> Array.map Array.last)            
        Assert.Equal<int[][]>(expectedIndices, indices)            

    [<Fact>]
    let ``False nearest neighbours example ``() =
        let frame =
            DataFrame.LoadCsv (
                "data/montreal_high_pass_filtered.csv", 
                dataTypes = [| typeof<string>; typeof<float> |],
                columnNames = [| "datetime"; "Montreal" |], header = false)
    
        let column = frame.["Montreal"] :?> PrimitiveDataFrameColumn<float>

        let expected = [|0; 1515; 497; 82; 16; 2; 0|]
        //[ false_nearest_neighours(filteredWeatherData,1,i) for i in range(0,7)]
        let actual = Array.init 7 (fun n -> DataFrameColumn.FalseNearestNeighbours 1 n column)
        
        Assert.Equal<int[]>(expected, actual)