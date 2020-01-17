namespace TakensTheorem.Core

module KNN =
    open Accord.MachineLearning

    let Distances (points: float [] []) (knn: KNearestNeighbors) =
        [| for point in points do
            let distances =
                knn.GetNearestNeighbors point
                |> fst // skip label info
                |> Array.map (fun nbr -> knn.Distance.Distance(nbr, point))

            yield (point, distances) |]

    let DistancesWithIndices (k: int) (points: float [] []) =
        let knn = KNearestNeighbors(k).Learn(points, [| 0 .. points.Length - 1 |])
        [| for point in points do
            let (neighbors, indices) = knn.GetNearestNeighbors point

            let distances = neighbors |> Array.map (fun nbr -> knn.Distance.Distance(nbr, point))

            yield (distances, indices) |]
