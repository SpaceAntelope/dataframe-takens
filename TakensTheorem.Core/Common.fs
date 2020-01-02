namespace TakensTheorem.Core

open System
open XPlot.Plotly
open Microsoft.Data.Analysis

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

    // let (!>) (source: DataFrameColumn) = source :?> PrimitiveDataFrameColumn<_>
    // let (!>!) (source: DataFrameColumn) = source :?> StringDataFrameColumn
