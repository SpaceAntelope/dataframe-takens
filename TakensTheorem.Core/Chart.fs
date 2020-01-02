namespace TakensTheorem.Core

open System
open XPlot.Plotly
open Microsoft.Data.Analysis

module Chart = ()

    // let plotColumn<'TIndex, 'TColumn 
    //         when 'TIndex  :> ValueType 
    //          and 'TColumn :> ValueType> 
    //         (indexName : string) (columnName: string) (source: DataFrame) =
        
    //     Scatter(
    //         x = DataFrameColumn.AsArray<'TIndex> source.[indexName], 
    //         y = DataFrameColumn.AsArray<'TColumn> source.[columnName] )
    //     |> Chart.Plot  