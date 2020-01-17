namespace TakensTheorem.Core 

module KNN = 
    open Accord.MachineLearning

    let Distances (points: float[][]) (knn: KNearestNeighbors) = 
        [| for point in points do
            let distance = 
                knn.GetNearestNeighbors point
                |> fst
                |> Array.map (fun nbr -> knn.Distance.Distance(nbr, point))
            
            yield (point, distance) |]
    