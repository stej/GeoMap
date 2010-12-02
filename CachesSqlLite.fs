module CachesSqlLite

open System
open CachesModel
open System.Data.SQLite

let readCachesFromDb loadHandler fileName = 
    use conn = new System.Data.SQLite.SQLiteConnection()
    conn.ConnectionString <- sprintf "Data Source=\"%s\"" fileName
    conn.Open()
    let caches =
        let count = 
          use cmd = conn.CreateCommand()
          cmd.CommandText <- "Select count(Id) from Cache"
          Convert.ToInt32(cmd.ExecuteScalar())
        use cmd = conn.CreateCommand()
        cmd.CommandText <- "Select * from Cache"
        let rd = cmd.ExecuteReader()

        let rec getCaches (rd:SQLiteDataReader) index =
          seq {
            if rd.Read() then
              yield new Cache(rd)
              loadHandler count index
              yield! getCaches rd (index+1)
          }
        let ret = getCaches rd 0 |> Seq.toList
        rd.Close()
        ret
    conn.Close()
    caches

let newDb fileName = 
    use conn = new System.Data.SQLite.SQLiteConnection()
    conn.ConnectionString <- sprintf "Data Source=\"%s\"" fileName
    conn.Open();
    use cmd = conn.CreateCommand()
    cmd.CommandText <- @"CREATE TABLE [Cache] (
                [Id] varchar(15) PRIMARY KEY NOT NULL,
                [Name] varchar(128) NOT NULL,
                [Url] varchar(70) NOT NULL,
                [File] varchar(15) NOT NULL,
                [Type] varchar(15) NOT NULL,
                [Size] integer NOT NULL,
                [Coords] varchar(30) NOT NULL,
                [CoordsFinal] varchar(30) NOT NULL DEFAULT '',
                [Keywords] varchar(128) NOT NULL DEFAULT '',
                [Notes] varchar(256) NOT NULL DEFAULT '',
                [Difficulty] float NOT NULL,
                [Terrain] float NOT NULL,
                [Hint] varchar(128) NOT NULL DEFAULT '',
                [CreatedAt] varchar(10) NOT NULL,
                [Found] boolean NOT NULL,
                [IsPremium] boolean NOT NULL
                );"
    cmd.ExecuteNonQuery() |> ignore
    conn.Close()

let writeCachesToDb fileName (caches: Cache []) = 
    use conn = new System.Data.SQLite.SQLiteConnection()
    conn.ConnectionString <- sprintf "Data Source=\"%s\"" fileName
    conn.Open()
    let addCache (cache:Cache) = 
      use cmd = conn.CreateCommand()
      cmd.CommandText <- "INSERT INTO Cache(Id, Name, Url, File, Type, Size, Coords, CoordsFinal, Found, Keywords, Notes, Difficulty, Terrain, Hint, CreatedAt, IsPremium) VALUES(@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, @p9, @p10, @p11, @p12, @p13, @p14, @p15, @p16)"
      cmd.Parameters.Add(new SQLiteParameter("@p1", cache.Id)) |> ignore
      cmd.Parameters.Add(new SQLiteParameter("@p2", cache.Name)) |> ignore
      cmd.Parameters.Add(new SQLiteParameter("@p3", cache.Url)) |> ignore
      cmd.Parameters.Add(new SQLiteParameter("@p4", cache.File)) |> ignore
      cmd.Parameters.Add(new SQLiteParameter("@p5", (cacheTypeToString cache.Type))) |> ignore
      cmd.Parameters.Add(new SQLiteParameter("@p6", cache.Size)) |> ignore
      cmd.Parameters.Add(new SQLiteParameter("@p7", cache.CoordsOrig)) |> ignore
      cmd.Parameters.Add(new SQLiteParameter("@p8", cache.CoordsFinalOrig)) |> ignore
      cmd.Parameters.Add(new SQLiteParameter("@p9", cache.Found)) |> ignore
      cmd.Parameters.Add(new SQLiteParameter("@p10", (String.Join(",", cache.Keywords)))) |> ignore
      cmd.Parameters.Add(new SQLiteParameter("@p11", cache.Notes)) |> ignore
      cmd.Parameters.Add(new SQLiteParameter("@p12", cache.Difficulty)) |> ignore
      cmd.Parameters.Add(new SQLiteParameter("@p13", cache.Terrain)) |> ignore
      cmd.Parameters.Add(new SQLiteParameter("@p14", cache.Hint)) |> ignore
      cmd.Parameters.Add(new SQLiteParameter("@p15", cache.CreatedAt)) |> ignore
      cmd.Parameters.Add(new SQLiteParameter("@p16", cache.IsPremium)) |> ignore
      cmd.ExecuteNonQuery(); |> ignore
    caches |> Array.iter addCache
    conn.Close()