module Csv

open CsvHelper
open System
open System.IO
open System.Globalization

let readCsv<'T> (filename: string) : 'T seq =
    seq {
        use reader = new StreamReader(filename)
        use csv = new CsvReader(reader, CultureInfo.InvariantCulture)
        let records = csv.GetRecords<'T>()

        yield! records
    }

let readCsvWithMap<'T, 'Map when 'Map :> Configuration.ClassMap> (filePath: string) =
    seq {
        use reader = new StreamReader(filePath)
        use csv = new CsvReader(reader, CultureInfo.InvariantCulture)

        csv.Context.RegisterClassMap<'Map>() |> ignore

        let records = csv.GetRecords<'T>()

        yield! records
    }

let saveCsv (filename: string) records =
    use writer = new StreamWriter(filename)
    use csv = new CsvWriter(writer, CultureInfo.InvariantCulture)

    csv.WriteRecords(records)

let saveCsvWithMap<'T, 'Map when 'Map :> Configuration.ClassMap> (filename: string) (records: 'T seq) =
    use writer = new StreamWriter(filename)
    use csv = new CsvWriter(writer, CultureInfo.InvariantCulture)

    csv.Context.RegisterClassMap<'Map>() |> ignore
    csv.WriteRecords(records)
