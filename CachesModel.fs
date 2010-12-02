module CachesModel

open System
open System.IO
open System.Text.RegularExpressions
open System.Globalization
open System.Collections.Generic

/// converts "N 49° 35.934 E 017° 15.056" to float point
let convertCoordsToFloatingPointFormat coords filePath = 
    let m = Regex.Match(coords, @"^\s*N\s*(?<s1>\d+)°\s*(?<s2>[\d\.]+)\s*E\s*(?<s3>\d+)°\s(?<s4>[\d\.]+)\s*$")
    (*PointLatLng(Double.Parse(m.Groups.["s1"].Value) + Double.Parse(m.Groups.["s2"].Value, CultureInfo.InvariantCulture) / 60.0,
                Double.Parse(m.Groups.["s3"].Value) + Double.Parse(m.Groups.["s4"].Value, CultureInfo.InvariantCulture) / 60.0)*)
    if m.Success = false then
        printfn "Coords in file %s don't match the regex" filePath
        0.0, 0.0
    else
        let toDouble (groupName:string) = 
            try
                Double.Parse(m.Groups.[groupName].Value, CultureInfo.InvariantCulture)
            with
                | :? FormatException as ex -> printfn "Unable to convert value %s in file %s" m.Groups.[groupName].Value filePath
                                              0.0 // default

        ((toDouble "s1") + (toDouble "s2") / 60.0, (toDouble "s3") + (toDouble "s4") / 60.0)

let convertCoordsToFloatingPointFormat2 coords = 
    if coords = "" then
        0.0, 0.0
    else
        let m = Regex.Match(coords, @"^\s*N\s*(?<s1>\d+)°\s*(?<s2>[\d\.]+)\s*E\s*(?<s3>\d+)°\s(?<s4>[\d\.]+)\s*$")
        if m.Success = false then
            printfn "Coords '%s' don't match the regex" coords
            0.0, 0.0
        else
            let toDouble (groupName:string) = 
                try
                    Double.Parse(m.Groups.[groupName].Value, CultureInfo.InvariantCulture)
                with
                    | :? FormatException as ex -> printfn "Unable to convert value %s" m.Groups.[groupName].Value
                                                  0.0 // default
            ((toDouble "s1") + (toDouble "s2") / 60.0, (toDouble "s3") + (toDouble "s4") / 60.0)

/// type of cache
type CacheType = 
    | Traditional
    | MultiCache
    | Mystery
    | Event
    | Letter
    | Earth
    | Whereigo
    | Other

let stringToCacheType value = 
    match value with 
        | "traditional" -> Traditional
        | "multi"       -> MultiCache
        | "mystery"     -> Mystery
        | "event"       -> Event
        | "letterbox"   -> Letter
        | "earth"       -> Earth
        | "whereigo"    -> Whereigo
        |_              -> Other
let cacheTypeToString value = 
    match value with 
        | Traditional -> "traditional"
        | MultiCache  -> "multi"
        | Mystery     -> "mystery"
        | Event       -> "event"
        | Letter      -> "letterbox"
        | Earth       -> "earth"
        | Whereigo    -> "whereigo"
        | Other       -> "other"

/// basic cache info
type Cache(reader:System.Data.SQLite.SQLiteDataReader) =
    let id             = reader.["Id"].ToString()
    let name           = reader.["Name"].ToString()
    let file           = reader.["File"].ToString()
    let url            = reader.["Url"].ToString()
    let size           = Convert.ToInt32(reader.["Size"])
    let ctype           = stringToCacheType (reader.["Type"].ToString())
    let coordsOrig     = reader.["Coords"].ToString()
    let coords         = convertCoordsToFloatingPointFormat2 coordsOrig
    let coordsFinalOrig = reader.["CoordsFinal"].ToString()
    let coordsFinal    = convertCoordsToFloatingPointFormat2 coordsFinalOrig
    let found          = Convert.ToBoolean(reader.["Found"])
    let keywords       = reader.["Keywords"].ToString().Split(',')
    let notes          = reader.["Notes"].ToString()
    let difficulty     = Convert.ToDouble(reader.["Difficulty"])
    let terrain        = Convert.ToDouble(reader.["Terrain"])
    let hint           = reader.["Hint"].ToString()
    let createdAt      = Convert.ToDateTime(reader.["CreatedAt"])
    let isPremium      = Convert.ToBoolean(reader.["IsPremium"])
    member x.Id = id
    member x.Name = name
    member x.File = file
    member x.Url = url
    member x.Size = size
    member x.Type = ctype
    member x.CoordsOrig = coordsOrig
    member x.Coords = coords
    member x.CoordsFinalOrig = coordsFinalOrig
    member x.CoordsFinal = coordsFinal
    member x.Found = found
    member x.Keywords = keywords
    member x.Notes = notes
    member x.Difficulty = difficulty
    member x.Terrain = terrain
    member x.Hint = hint
    member x.CreatedAt = createdAt
    member x.IsPremium = isPremium

    member x.HasKeyword keyword =
        x.Keywords |> Array.exists (fun k -> k = keyword)
//    member x.AddNote note = 
//        if x.Notes = null || x.Notes.Length = 0 then x.Notes <- note
//        else x.Notes <- x.Notes + "\n" + note
    member x.CoordsToShow =
        match x.CoordsFinal with | (0.0,0.0) -> x.Coords | (a,b) -> printfn "final used: %s" x.CoordsFinalOrig; (a,b)
    member x.CoordsToShowOrig =
        match x.CoordsFinal with | (0.0, 0.0) -> x.CoordsOrig | (a,b) -> x.CoordsFinalOrig
    member x.HasValidCoords = 
        (fst x.Coords) <> 0.0 && (snd x.Coords) <> 0.0

(*type Cache = {
    mutable Id : string
    mutable Name : string
    mutable Url : string
    mutable Size : int
    mutable Type : CacheType
    mutable Coords : float*float
    mutable CoordsOrig: string
    mutable CoordsFinal: (float * float) option
    mutable CoordsFinalOrig: string option
    mutable Found : bool
    mutable Keywords: list<string>
    mutable Notes: string
    mutable Difficulty: float
    mutable Terrain: float
    mutable Hint: string
    mutable CreatedAt: DateTime
    FilePath: string }

    with
    member x.HasKeyword keyword =
        x.Keywords |> List.exists (fun k -> k = keyword)
    member x.AddNote note = 
        if x.Notes = null || x.Notes.Length = 0 then x.Notes <- note
        else x.Notes <- x.Notes + "\n" + note
    member x.CoordsToShow =
        match x.CoordsFinal with | Some(a,b) -> printfn "final used: %s" x.CoordsFinalOrig.Value; (a,b) | _ -> x.Coords
    member x.CoordsToShowOrig =
        match x.CoordsFinal with | Some(a,b) -> x.CoordsFinalOrig.Value | _ -> x.CoordsOrig
    member x.HasValidCoords = 
        (fst x.Coords) <> 0.0 && (snd x.Coords) <> 0.0
*)
      
(*let emptyCache = { Id = ""
                   Name = ""
                   Url = ""
                   Size = 0
                   Coords = (0.0, 0.0)
                   CoordsOrig = ""
                   CoordsFinal = None
                   CoordsFinalOrig = None
                   Found = false
                   Keywords = []
                   Type = Other
                   Notes = ""
                   Difficulty = 0.0
                   Terrain = 0.0
                   Hint = ""
                   CreatedAt = DateTime.MinValue
                   FilePath = "" }*)

let cacheSizeToString s = 
    match s with
        | 1 -> "Micro"
        | 2 -> "Small"
        | 3 -> "Regular"
        | 4 -> "Large"
        | _ -> "Unknown"
let cacheSizeToXString s = 
    "(" + (match s with | 1 -> "x" | 2 -> "xx" | 3 -> "xxx" | 4 -> "XXXX" | _ -> "size?") + ")"

let printCache (cache:Cache) =
    printfn "Cache -- %s -------------" cache.Name
    printfn "Id = %s" cache.Id
    printfn "Name = %s" cache.Name
    printfn "Url = %s" cache.Url
    printfn "Size = %i" cache.Size
    printfn "Coords = %f, %f" (fst cache.Coords) (snd cache.Coords)
    printfn "Coords valid = %s" (cache.HasValidCoords.ToString())
    printfn "Found = %s" (if cache.Found then "Yes" else "No")
    printfn "Type = %s" (cacheTypeToString cache.Type)

let extractKeywords (caches:Cache[]) =
    let m = Dictionary<string, string>()
    caches 
        |> Array.map (fun c -> c.Keywords)
        |> Array.iter (fun kwrds -> for k in kwrds do m.[k] <- k)
    m.Values |> Seq.toArray |> Array.filter (fun k -> k.Length > 0 )

(*let cache = { 
    emptyCache with 
      Name = "cacha"
      Url = "url"
      Size = 5
      Type = Traditional
}

let readCaches dir = 
    let dinfo = System.IO.DirectoryInfo(dir)
    dinfo.GetFiles("*.txt") |> Array.map (fun (f:System.IO.FileInfo) -> f.FullName)

let readCache filePath =
    //printfn "%s" filePath
    let mutable newCache = { emptyCache with FilePath = filePath; Id = Path.GetFileNameWithoutExtension(filePath) }
    let splitLine l =
      let what = Regex.Match(l, "^\\w+?:")
      match what.Success with
        | true -> Some(l, what.Value, l.Replace(what.Value, ""))
        | false -> printfn "Unknown row %s" l; None
    let emptyOp() =
        let mutable x = 'a';
        x <- 'a'
    try
        let lines = System.IO.File.ReadAllLines(filePath) |> Array.map splitLine
        for l in lines do
            match l with
                | None -> printfn "unknown row in %s" filePath
                | Some(l, what, value) -> 
                    match what with
                    | "name:" -> newCache.Name <- value 
                    | "url:" -> newCache.Url <- value
                    | "type:" -> newCache.Type <- stringToCacheType value
                    | "size:" -> newCache.Size <- Int32.Parse(value)
                    | "coords:"      -> newCache.Coords <- convertCoordsToFloatingPointFormat value filePath
                                        newCache.CoordsOrig <- value
                    | "coordsfinal:" -> newCache.CoordsFinal <- Some(convertCoordsToFloatingPointFormat value filePath)
                                        newCache.CoordsFinalOrig <- Some(value)
                    | "found:" -> newCache.Found <- bool.Parse(value)
                    | "keywords:" -> newCache.Keywords <- value.Split(',') |> Array.toList
                    | "note:"    -> newCache.AddNote(value)
                    | "difficulty:" -> newCache.Difficulty <- Double.Parse(value, System.Globalization.CultureInfo.InvariantCulture)
                    | "terrain:" -> newCache.Terrain <- Double.Parse(value, System.Globalization.CultureInfo.InvariantCulture)
                    | "hint:" -> newCache.Hint <- value
                    | "filecreatedat:" -> emptyOp() // how to return unit?
                    | "cachecreated:" -> newCache.CreatedAt <- DateTime.ParseExact(value, "yyyy-MM-dd", null)
                    //| _ -> failwith ("unknown name of item. item: " + what + ", file: " + filePath)
                    | _ -> printfn "unknown name of item. item: %s, file: %s" what filePath
    with
        | :? FormatException as ex -> printfn "Problem when reading %s : %s" filePath ex.Message
    newCache
*)