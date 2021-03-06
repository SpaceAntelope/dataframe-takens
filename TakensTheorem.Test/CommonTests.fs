namespace TakensTheorem.Test

open System
open Xunit
open Microsoft.Data.Analysis
open TakensTheorem.Core
open Common.Dictionary
open System.Collections.Generic

// type DateRangeData() =
//     inherit TheoryData<DateTime, DateTime>



type CommonTests() =

    static member DateCases =
        let addDays (dt: DateTime) = dt.AddDays(1.0)
        let addHours (dt: DateTime) = dt.AddHours(1.0)

        let data = TheoryData<DateTime, DateTime, DateTime -> DateTime>()
        data.Add(DateTime(2020, 1, 1), DateTime(2020, 12, 31), addDays)
        data.Add(DateTime(2020, 10, 1), DateTime(2021, 1, 31), addDays)
        data.Add(DateTime(2081, 10, 10), DateTime(2081, 12, 31), addDays)
        data.Add(DateTime(2020, 1, 1, 22, 0, 0), DateTime(2020, 1, 5, 23, 0, 0), addHours)
        data.Add(DateTime(2020, 12, 1), DateTime(2020, 12, 1, 18, 0, 0), addHours)
        data.Add(DateTime(2081, 10, 10), DateTime(2081, 10, 12), addHours)
        data

    static member MatricesToTranspose =
        let data = TheoryData<int [] [], int [] []>()
        data.Add
            ([| [| 1; 2; 3 |]
                [| 1; 2; 3 |] |],
             [| [| 1; 1 |]
                [| 2; 2 |]
                [| 3; 3 |] |])
        data.Add
            ([| [| 1; 2; 3 |]
                [| 4; 5; 6 |]
                [| 7; 8; 9 |] |],
             [| [| 1; 4; 7 |]
                [| 2; 5; 8 |]
                [| 3; 6; 9 |] |])
        data.Add([| [| 1 |] |], [| [| 1 |] |])
        data.Add
            ([| [| 1; 2; 3 |] |],
             [| [| 1 |]
                [| 2 |]
                [| 3 |] |])
        data.Add
            ([| [| 1 |]
                [| 2 |]
                [| 3 |] |], [| [| 1; 2; 3 |] |])
        data.Add
            ([| [||]
                [||] |], [||])
        data

    [<Theory>]
    [<MemberData("DateCases")>]
    member x.``Date range test borders`` (start: DateTime) (stop: DateTime) (next: DateTime -> DateTime) =
        let span = stop - start
        let range = Common.dateRange start stop next |> Array.ofSeq

        Assert.Equal(start, range |> Array.head)
        Assert.Equal(stop, range |> Array.last)
    //Assert.Equal(span.Days + 1, range |> Array.length)


    [<Theory>]
    [<InlineData("a")>]
    [<InlineData("z")>]
    [<InlineData(1)>]
    [<InlineData(null)>]
    member x.``Check dictionary for keys`` (key) =
        let cache =
            [ ("a", 1)
              ("b", 2)
              ("c", 3) ]
            |> dict

        Assert.Equal((cache.ContainsKey >> not) key, key |> notIn cache)

    [<Fact>]
    member x.``Update dictionary``() =
        let cache = Dictionary<string, int>()

        [ ("a", 1)
          ("b", 2)
          ("c", 3) ]
        |> List.iter (cache.Add)

        cache |> update ("a", -1)
        Assert.Equal(cache.["a"], -1)
        cache |> update ("z", -1)
        Assert.Equal(cache.["z"], -1)
        cache |> update ("z", -2)
        Assert.Equal(cache.["z"], -2)

    [<Trait("Category", "Common")>]
    [<Theory>]
    [<MemberData("MatricesToTranspose")>]
    member x.``2d dense array transposition`` (source: int [] [], transposed: int [] []) =
        Assert.Equal<int [] []>(transposed, Common.transpose source)

        let to2dList arr =
            arr
            |> Array.map (List.ofArray)
            |> List.ofArray

        Assert.Equal<int list list>(to2dList transposed, to2dList source |> Common.transpose2)
