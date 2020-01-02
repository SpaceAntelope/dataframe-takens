namespace TakensTheorem.Core

open System
open Microsoft.Data.Analysis
open System.Collections.Generic
open Common



module Takens =
    let Embedding<'T> (delay: int) (dimension: int) (data: 'T []) =
        (* This function returns the Takens embedding of data with delay into dimension,
         * delay*dimension must be < len(data)
         *)
        let length = data |> Array.length
        if (delay * dimension) > length then failwith "Delay times dimension exceed length of data!"

        seq {
            yield data.[0..(length - delay * dimension - 1)]

            for i in 1 .. dimension - 1 do
                let start = i * delay
                let stop = length - delay * (dimension - i) - 1
                yield data.[start..stop]
        }


