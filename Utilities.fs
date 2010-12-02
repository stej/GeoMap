module Utilities

let openUrl (url:string) =
    System.Diagnostics.Process.Start(url) |> ignore