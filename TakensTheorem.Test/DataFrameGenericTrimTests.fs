namespace TakensTheorem.Test

open System
open Xunit
open Microsoft.Data.Analysis
open TakensTheorem.Core
open System.IO

[<AbstractClass>]
type DataFrameTrimTests<'T when 'T: equality and 'T :> ValueType and 'T: (new: unit -> 'T) and 'T: struct>() =
    [<Theory>]
    [<MemberData("TrimCases")>]
    member x.``Trim a column's values`` (toTrim: 'T, values: 'T [], expected: 'T []) =
        let actual =
            values
            |> DataFrameColumn<_>.FromValues "Untrimmed"
            |> DataFrameColumn<_>.Trim toTrim
            |> DataFrameColumn<_>.Values

        Assert.Equal<Nullable<'T> []>(expected |> Array.map Nullable, actual)

type DataFrameTrimTestsInt32() =
    inherit DataFrameTrimTests<int>()

    static member TrimCases: TheoryData<int, int [], int []> =
        let result = TheoryData<int, int [], int []>()
        result.Add(0, [| 0; 0; 0; 0; 1; 2; 3; 0; 0; 0 |], [| 1; 2; 3 |])
        result.Add(0, [| 0; 0; 0; 0; 1; 2; 3 |], [| 1; 2; 3 |])
        result.Add(0, [| 1; 2; 3; 0; 0; 0 |], [| 1; 2; 3 |])
        result.Add(0, [| 1; 2; 3 |], [| 1; 2; 3 |])
        result

type DataFrameTrimTestsDecimal() =
    inherit DataFrameTrimTests<Decimal>()

    static member TrimCases =
        let result = TheoryData<Decimal, Decimal [], Decimal []>()
        result.Add(0M, [| 0M; 0M; 0M; 0M; 1M; 2M; 3M; 0M; 0M; 0M |], [| 1M; 2M; 3M |])
        result.Add(0M, [| 0M; 0M; 0M; 0M; 1M; 2M; 3M |], [| 1M; 2M; 3M |])
        result.Add(0M, [| 1M; 2M; 3M; 0M; 0M; 0M |], [| 1M; 2M; 3M |])
        result.Add(0M, [| 1M; 2M; 3M |], [| 1M; 2M; 3M |])
        result


type DataFrameTrimTestsChar() =
    inherit DataFrameTrimTests<char>()

    static member TrimCases =
        let result = TheoryData<char, char [], char []>()

        result.Add('s', [| 's'; 't'; 'r'; 'i'; 'k'; 'e'; 's' |], [| 't'; 'r'; 'i'; 'k'; 'e' |])
        result.Add('s', [| 's'; 't'; 'r'; 'i'; 'k'; 'e' |], [| 't'; 'r'; 'i'; 'k'; 'e' |])
        result.Add('s', [| 't'; 'r'; 'i'; 'k'; 'e'; 's' |], [| 't'; 'r'; 'i'; 'k'; 'e' |])
        result.Add('s', [| 't'; 'r'; 'i'; 'k'; 'e' |], [| 't'; 'r'; 'i'; 'k'; 'e' |])

        result

type DataFrameTrimTestsDateTime() =
    inherit DataFrameTrimTests<DateTime>()

    static member TrimCases =
        let result = TheoryData<DateTime, DateTime [], DateTime []>()
        let daily (dt: DateTime) = dt.AddDays(1.)
        result.Add
            (DateTime(1990, 1, 1),
             Array.ofSeq <| Common.dateRange (DateTime(1990, 1, 1)) (DateTime(1990, 1, 10)) daily,
             Array.ofSeq <| Common.dateRange (DateTime(1990, 1, 2)) (DateTime(1990, 1, 10)) daily)
        
        result

type DataFrameTrimTestString() =

    static member TrimCases =
        let result = TheoryData<string, string [], string []>()
        result.Add(null, [| null; null; "one"; "two"; "three"; null |], [| "one"; "two"; "three" |])
        result.Add(null, [| null; null; "one"; "two"; "three" |], [| "one"; "two"; "three" |])
        result.Add(null, [| "one"; "two"; "three"; null |], [| "one"; "two"; "three" |])
        result.Add(null, [| "one"; "two"; "three" |], [| "one"; "two"; "three" |])
        result

    [<Theory>]
    [<MemberData("TrimCases")>]
    member x.``Trim the nulls off a column's values`` (toTrim: string, values: string [], expected: string []) =
        let actual =
            values
            |> StringDataFrameColumn.FromValues "Untrimmed"
            |> StringDataFrameColumn.Trim toTrim
            |> StringDataFrameColumn.Values

        Assert.Equal<string []>(expected, actual)
