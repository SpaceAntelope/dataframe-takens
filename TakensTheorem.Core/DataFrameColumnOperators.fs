namespace TakensTheorem.Core

open System
open XPlot.Plotly
open Microsoft.Data.Analysis
open System.Collections.Generic
open System.IO

module DataFrameColumnOperators =
    (*
     * Temporary inequality operators, presumably the dataframecolumn will overload them
     * over the elemntwise functions eventually.
    *)


    // type ColOpOverloads2 = ColOpOverloads2
    //     with
    //         static member And(ColOpOverloads2, (left: PrimitiveDataFrameColumn<_>, right: PrimitiveDataFrameColumn<_>)) =
    //             left.And(right) :?> PrimitiveDataFrameColumn<bool>
    //         static member And(ColOpOverloads2, (left: DataFrameColumn, right: PrimitiveDataFrameColumn<'T>)) =
    //             left.And(right) :?> PrimitiveDataFrameColumn<bool>

    // let inline private intermediateAndOperator< ^l when ^l: (static member And: (^il * ^ir)
    //                                                               -> PrimitiveDataFrameColumn<bool>)> (left: ^l) right =
    //     left.And(right) :?> PrimitiveDataFrameColumn<bool> //(left : (member And: DataFrameColumn -> DataFrameColumn)).And(right)


    // type ColOpOverloads<'T when 'T :> ValueType and 'T: struct and 'T: (new: unit -> 'T)> = EmptyUnionCase
    //     with
    //         static member inline ($) (EmptyUnionCase,
    //                                   (left: PrimitiveDataFrameColumn<'T>, right: PrimitiveDataFrameColumn<'T>)) =
    //             left.And(right) :?> PrimitiveDataFrameColumn<bool>
    //         static member inline ($) (EmptyUnionCase, (left: DataFrameColumn, right: PrimitiveDataFrameColumn<'T>)) =
    //             left.And(right) :?> PrimitiveDataFrameColumn<bool>
    // // static member inline Op(EmptyUnionCase, left: PrimitiveDataFrameColumn<'T>, right: DataFrameColumn) =
    // //     left.And(right) :?> PrimitiveDataFrameColumn<bool>


    // type OverOps<'T when 'T :> ValueType and 'T: struct and 'T: (new: unit -> 'T)> =
    //     static member _And (left: PrimitiveDataFrameColumn<'T>, right: PrimitiveDataFrameColumn<'T>) =
    //         left.Add(right) :?> PrimitiveDataFrameColumn<bool>
    //     static member _And (left: DataFrameColumn, right: PrimitiveDataFrameColumn<'T>) =
    //         left.Add(right) :?> PrimitiveDataFrameColumn<bool>
    //     static member _And (left: PrimitiveDataFrameColumn<'T>, right: DataFrameColumn) =
    //         left.Add(right) :?> PrimitiveDataFrameColumn<bool>

    // let inline private interOp< ^l, ^r when (^l or ^r): (static member _And: ^l * ^r -> PrimitiveDataFrameColumn<bool>)> left
    //            right = ((^l or ^r): (static member _And: ^l * ^r -> PrimitiveDataFrameColumn<bool>) (left, right))

    // let inline (/&&/) (left: ^l) (right: ^r) = interOp left right

    // // let inline (/&/) (left: #DataFrameColumn) (right: #DataFrameColumn) =
    //     left.And(right) :?> PrimitiveDataFrameColumn<bool>


    let inline (!>) (source: DataFrameColumn) = source :?> PrimitiveDataFrameColumn<_>

    let inline (!>!) (source: DataFrameColumn) = source :?> StringDataFrameColumn

    let inline (!<) (source: PrimitiveDataFrameColumn<'T>) = source :> DataFrameColumn

    let inline (!<!) (source: StringDataFrameColumn) = source :> DataFrameColumn

    let (/&) (col: PrimitiveDataFrameColumn<'T>) (value: bool) = col.And(value)
    let (&/) (value: bool) (col: PrimitiveDataFrameColumn<'T>) = col.And(value)
    let (/&/) (col1: PrimitiveDataFrameColumn<'T>) (col2: PrimitiveDataFrameColumn<'T>) =
        col1.And(col2) :?> PrimitiveDataFrameColumn<bool>

    let (/>) (col: PrimitiveDataFrameColumn<'T>) (value: 'T) = col.ElementwiseGreaterThan(value = value)
    let (>/) (value: 'T) (col: PrimitiveDataFrameColumn<'T>) = col.ElementwiseLessThan(value = value)
    let (/>/) (left: #DataFrameColumn) (right: #DataFrameColumn) = left.ElementwiseGreaterThan(right)

    let (/>=) (col: PrimitiveDataFrameColumn<'T>) (value: 'T) = col.ElementwiseGreaterThanOrEqual(value = value)
    let (>=/) (value: 'T) (col: PrimitiveDataFrameColumn<'T>) = col.ElementwiseLessThanOrEqual(value = value)
    let (/>=/) (left: #DataFrameColumn) (right: #DataFrameColumn) = left.ElementwiseGreaterThanOrEqual(right)

    let (/<) (col1: PrimitiveDataFrameColumn<'T>) (value: 'T) = col1.ElementwiseLessThan(value = value)
    let (</) (value: 'T) (col: PrimitiveDataFrameColumn<'T>) = col.ElementwiseGreaterThan(value = value)
    let (/</) (left: #DataFrameColumn) (right: #DataFrameColumn) = left.ElementwiseLessThan(right)

    let (/<=) (col1: PrimitiveDataFrameColumn<'T>) (value: 'T) = col1.ElementwiseLessThanOrEqual(value = value)
    let (<=/) (value: 'T) (col: PrimitiveDataFrameColumn<'T>) = col.ElementwiseGreaterThanOrEqual(value = value)
    let (/<=/) (left: #DataFrameColumn) (right: #DataFrameColumn) = left.ElementwiseLessThanOrEqual(right)

    let (/=) (col1: PrimitiveDataFrameColumn<'T>) (value: 'T) = col1.ElementwiseEquals(value = value)
    let (=/) (value: 'T) (col: PrimitiveDataFrameColumn<'T>) = col.ElementwiseEquals(value = value)
    let (/=/) (left: #DataFrameColumn) (right: #DataFrameColumn) = left.ElementwiseEquals(right)
    // type OverloadInequalities<'T when 'T :> ValueType and 'T: struct and 'T: (new: unit -> 'T)> = EmptyUnionCase
    //     with
    //         static member inline (.>) (EmptyUnionCase, (left: PrimitiveDataFrameColumn<'T>, right: 'T)) =
    //             left.ElementwiseGreaterThan(right)
    //         static member inline (.>) (EmptyUnionCase, (left: 'T, right: PrimitiveDataFrameColumn<'T>)) =
    //             right.ElementwiseLessThan(left)
    //         static member inline (.>) (EmptyUnionCase, (left: DataFrameColumn, right: PrimitiveDataFrameColumn<'T>)) =
    //             left.ElementwiseGreaterThan(right)
    //         static member inline (.>) (EmptyUnionCase, (left: PrimitiveDataFrameColumn<'T>, right: DataFrameColumn)) =
    //             right.ElementwiseLessThan(left)
    //         static member inline (.>) (EmptyUnionCase,
    //                                    (left: PrimitiveDataFrameColumn<'T>, right: PrimitiveDataFrameColumn<'T>)) =
    //             left.ElementwiseGreaterThan(right)

    // let inline (.>.) left right = left .> right


//     type WhatWhat<'T when 'T :> ValueType and 'T: struct and 'T: (new: unit -> 'T)> =
//         static member Gt(left: PrimitiveDataFrameColumn<'T>, right: 'T) = left.ElementwiseGreaterThan(value = right)
//         static member Gt(left: 'T, right: PrimitiveDataFrameColumn<'T>) = right.ElementwiseLessThan(value = left)
//         static member Gt(left: DataFrameColumn, right: PrimitiveDataFrameColumn<'T>) =
//             left.ElementwiseGreaterThan(right)
//         static member Gt(left: PrimitiveDataFrameColumn<'T>, right: DataFrameColumn) = right.ElementwiseLessThan(left)
//         static member Gt(left: PrimitiveDataFrameColumn<'T>, right: PrimitiveDataFrameColumn<'T>) =
//             left.ElementwiseGreaterThan(right)
//     // static member Gt(left: #DataFrameColumn, right: 'T) = left.ElementwiseGreaterThan(right)
//     // static member Gt(left: 'T, right: #DataFrameColumn) = right.ElementwiseLessThan(left)
//     // static member Gt(left: #DataFrameColumn, right: #DataFrameColumn) = left.ElementwiseGreaterThan(right)

//     let inline greaterThan< ^l, ^r, ^z when (^l or ^r): (static member ElementwiseGreaterThan: ^l * ^z
//                                                               -> PrimitiveDataFrameColumn<bool>)> left right =
//         ((^l or ^r): (static member ElementwiseGreaterThan: ^l * ^z -> PrimitiveDataFrameColumn<bool>) (left, right))

//     let inline (/>/) (left: ^l) (right: ^r): PrimitiveDataFrameColumn<bool> = greaterThan left right

// // type WhatAgain<'T when 'T :> ValueType and 'T: struct and 'T: (new: unit -> 'T)> = Empty with
//     static member inline ($) (x : 'T, _) =


// type WriteAnyOverloads = WriteAnyOverloads with
//         static member inline ($) (x: char, _) = Console.Write x
//         static member inline ($) (x: string, _) = Console.Write x

// let inline write any = any $ WriteAnyOverloads
