namespace TakensTheorem.Core

open System
open XPlot.Plotly
open Microsoft.Data.Analysis
open System.Collections.Generic

module Common =
    let dateRange (start: DateTime) (stop: DateTime) (next: DateTime -> DateTime) =
        Seq.unfold (fun (state: DateTime) ->
            if state <= stop then Some(state, next state) else None) start

    let to2d<'T> (source: 'T [] []) =
        let l1 = source |> Array.length

        let l2 =
            source
            |> Array.head
            |> Array.length
        Array2D.init l1 l2 (fun x y -> source.[x].[y])

    module Dictionary =
        let notIn (dic: IDictionary<'T, _>) (key: 'T) = (dic.ContainsKey >> not) key
        
        let update (key: 'TKey, value: 'TValue) (dic: IDictionary<'TKey, 'TValue>) =
            if key |> notIn dic then dic.Add(key, value) else dic.[key] <- value
                
    // let inline (/!) (key: 'T) (dic: IDictionary<'T, _>): bool = (dic.ContainsKey >> not) key
    // let (!>) (source: DataFrameColumn) = source :?> PrimitiveDataFrameColumn<_>
    // let (!>!) (source: DataFrameColumn) = source :?> StringDataFrameColumn

    let transpose (source: 'a[][]) =
        let length1 = source |> Array.length
        let length2 = source |> Array.head |> Array.length
        Array.init length2 (fun d1 -> Array.init length1 (fun d2 -> source.[d2].[d1]))

    let rec transpose2 = function
        | (_::_)::_ as M -> List.map List.head M :: transpose2 (List.map List.tail M)
        | _ -> []        

    
