namespace TakensTheorem.Test

open System
open Xunit
open Microsoft.Data.Analysis
open TakensTheorem.Core


// type DateRangeData() =
//     inherit TheoryData<DateTime, DateTime>



type CommonTests() =

    static member DateCases with get() =
        let addDays (dt:DateTime) = dt.AddDays(1.0)
        let addHours (dt:DateTime) = dt.AddHours(1.0)

        let data = TheoryData<DateTime,DateTime, DateTime->DateTime>()
        data.Add(DateTime(2020, 1, 1), DateTime(2020, 12, 31), addDays)
        data.Add(DateTime(2020, 10, 1), DateTime(2021, 1, 31), addDays)
        data.Add(DateTime(2081, 10, 10), DateTime(2081, 12, 31), addDays)
        data.Add(DateTime(2020, 1, 1, 22, 0, 0), DateTime(2020, 1, 5, 23,0,0), addHours)
        data.Add(DateTime(2020, 12, 1), DateTime(2020, 12, 1, 18,0,0), addHours)
        data.Add(DateTime(2081, 10, 10), DateTime(2081, 10, 12), addHours)
        data
    
        
    [<Theory>]
    [<MemberData("DateCases")>]
    member x.``Date range test borders`` (start: DateTime) (stop: DateTime) (next: DateTime->DateTime) =
        let span = stop - start
        let range = Common.dateRange start stop next |> Array.ofSeq        

        Assert.Equal(start, range |> Array.head)
        Assert.Equal(stop, range |> Array.last)
        //Assert.Equal(span.Days + 1, range |> Array.length)
    
     
    