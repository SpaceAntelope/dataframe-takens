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

    [<Fact>]
    let ``Column to value gt`` () =
            let values = [| -5 .. 5 |]
            let col = DataFrameColumn.FromValues "filter" values
            let outerLimit = 0
            let expected =
                values
                |> Array.map (fun i -> i > outerLimit)
                |> DataFrameColumn.FromValues "filter"
    
            let actual = col /> outerLimit
            Assert.Equal<Nullable<bool> []>(expected |> DataFrameColumn.Values, actual |> DataFrameColumn.Values)
    
            let actual = outerLimit </ col
            Assert.Equal<Nullable<bool> []>(expected |> DataFrameColumn.Values, actual |> DataFrameColumn.Values)

            let col' = Array.create<int> 11 0 |> DataFrameColumn.FromValues ""
            let actual = col />/ col'
            Assert.Equal<Nullable<bool>[]>(expected |> DataFrameColumn.Values, actual |> DataFrameColumn.Values)
    


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

    
    type GlobalAndTests() =

        static member AndCases =
            let result = TheoryData<bool[], bool [], bool []>()
            result.Add(
                [|true;true;false;true;false|],
                [|true;true;true;false;false|],
                [|true;true;false;false;false|])
            result

        [<Theory>]
        [<MemberData("AndCases")>]
        member x. ``Bitwise global AND operator: primitive to primitive``(leftData:bool[],rightData:bool[],expected:bool[]) =
            let left = leftData |> DataFrameColumn.FromValues ""
            let right = rightData |> DataFrameColumn.FromValues ""

            let actual = (left /&/ right) |> DataFrameColumn.Values |> Array.map (fun i -> i.Value)        
            Assert.Equal<bool[]>(expected, actual)
        
            let actual = (right /&/ left) |> DataFrameColumn.Values |> Array.map (fun i -> i.Value)        
            Assert.Equal<bool[]>(expected, actual)
        
        // [<Fact>]
        // [<MemberData("AndCases")>]
        // member x. ``Bitwise global AND operator: primitive to dataframecolumn both ways``(leftData:bool[],rightData:bool[],expected:bool[]) =
        //     let left = leftData |> DataFrameColumn.FromValues ""
        //     let right = rightData |> DataFrameColumn.FromValues "" |> (!<)

        //     let actual = (left /&/ right) |> DataFrameColumn.Values |> Array.map (fun i -> i.Value)        
        //     Assert.Equal<bool[]>(expected, actual)
               
        //     let actual = (right /&/ left) |> DataFrameColumn.Values |> Array.map (fun i -> i.Value)        
        //     Assert.Equal<bool[]>(expected, actual)

    
    
    [<Fact>]
    let ``Bitwise global AND operator``()=
        let left =     [1;1;0;1;0] |> List.map((=)1) |> DataFrameColumn.FromValues ""
        let right =    [1;1;1;0;0] |> List.map((=)1) |> DataFrameColumn.FromValues ""
        let expected = [1;1;0;0;0] |> List.map((=)1) |> Array.ofList //|> DataFrameColumn.FromValues ""
        
        let actual = (left /&/ right) |> DataFrameColumn.Values |> Array.map (fun i -> i.Value)        
        Assert.Equal<bool[]>(expected, actual)
        
        let actual = (right /&/ left) |> DataFrameColumn.Values |> Array.map (fun i -> i.Value)        
        Assert.Equal<bool[]>(expected, actual)