namespace TakensTheorem.Core

open System.IO
open System.IO.Compression
open System.Text
open Microsoft.Data.Analysis
open System

module ZipHelper =

    type ZipLoader(path: string) =
        let file = File.OpenRead path :> Stream
        let zip = new ZipArchive(file, ZipArchiveMode.Read)

        abstract Close: unit -> unit

        default x.Close() =
            printfn "Base is closed"
            zip.Dispose()
            file.Dispose()

        member x.Entries = zip.Entries

        member x.GetEntry(file: string) = zip.Entries |> Seq.find (fun entry -> entry.Name = file)

        member x.ReadLines(file: string) =
            let entry = x.GetEntry file
            use reader = new StreamReader(entry.Open())
            seq {
                while reader.Peek() >= 0 do
                    yield reader.ReadLine()
            }

        member x.ReadText(file: string) =
            let entry = x.GetEntry file
            use stream = new StreamReader(entry.Open())
            stream.ReadToEnd()

        interface IDisposable with
            member x.Dispose() =
                printfn "Base is disposed"
                x.Close()

    type ZippedCsvLoader(path: string) =
        inherit ZipLoader(path)

        member x.ToDataFrame(file: string, ?forceFloat: bool) =
            let forceFloat = defaultArg forceFloat true

            let entry = x.GetEntry file
            use stream = entry.Open()
            use ms = new MemoryStream()
            stream.CopyTo(ms)
            ms.Position <- 0L          

            if forceFloat then
                DataFrame.LoadCsvForceFloat ms
            else 
                DataFrame.LoadCsv(ms, addIndexColumn = true)                

        member x.ToDataFrames() =
            seq {
                for entry in x.Entries do
                    printfn "%s" entry.Name
                    use stream = entry.Open()
                    use ms = new MemoryStream()
                    stream.CopyTo(ms)
                    ms.Position <- 0L
                    yield entry.Name, DataFrame.LoadCsv(ms)
            }

        override x.Close() =
            printfn "Derived is closed"
            base.Close()

        interface IDisposable with
            member x.Dispose() =
                printfn "Derived is disposed"
                x.Close()

    // let openZippedFile path filename =
    //     let file = File.OpenRead path :> Stream
    //     let zip = new ZipArchive(file, ZipArchiveMode.Read)
    //     let entry = zip.Entries |> Seq.find (fun entry -> entry.Name = filename)
    //     file, zip, entry

    // let unzipLines path fileName =
    //     let file, zip, entry = openZippedFile path fileName

    //     let reader = new StreamReader(entry.Open())
    //     seq {
    //         while reader.Peek() >= 0 do
    //             yield reader.ReadLine()

    //         reader.Dispose()
    //         zip.Dispose()
    //         file.Dispose()
    //     }

    // let unzipText path filename =
    //     let file, zip, entry = openZippedFile path filename
    //     use stream = new StreamReader(entry.Open())
    //     let result = stream.ReadToEnd()
    //     zip.Dispose()
    //     file.Dispose()
    //     result

    // let loadZippedCsvToDataFrame path filename =
    //     let file, zip, entry = openZippedFile path filename
    //     use ms = new MemoryStream()
    //     entry.Open().CopyTo(ms)
    //     ms.Position <- 0L
    //     let result = DataFrame.LoadCsv(ms)
    //     file.Dispose()
    //     zip.Dispose()
    //     result
