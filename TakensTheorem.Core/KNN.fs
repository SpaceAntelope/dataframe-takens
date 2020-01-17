namespace TakensTheorem.Core 

module KNN = 
    open Accord.MachineLearning

    let Distances (points: float[][]) (knn: KNearestNeighbors) = 
        [| for point in points do
            let distances = 
                knn.GetNearestNeighbors point
                |> fst // skip label info
                |> Array.map (fun nbr -> knn.Distance.Distance(nbr, point))
            
            yield (point, distances) |]
    