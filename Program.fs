module Program

open System
open System.Globalization
open System.Text.RegularExpressions
open System.Drawing
open System.Windows.Forms
open GMap.NET
open GMap.NET.WindowsForms
open GMap.NET.WindowsForms.Markers

open System
open System.Threading

open CachesModel
open CacheMapLayers
open Utilities
open CachesSqlLite
open Controls

(* --------------------------------------------------- *)
let args = System.Environment.GetCommandLineArgs() // Sys.argv
if args.Length < 2 then
  failwith "No argument specified. Specify path to file with caches."
if args.Length > 2 then
  failwith "Too many arguments specified. Specify only path to file with caches."

let loadCaches loadHandler = 
    let caches = readCachesFromDb loadHandler args.[1] |> List.toArray
    printfn "%A Count of caches: %d" System.DateTime.Now caches.Length
    let keywords = extractKeywords caches
    printfn "%A Got keywords.." System.DateTime.Now 
    caches, keywords

let mainForm = new Form(Text = "Caches buddy", ClientSize = System.Drawing.Size(845, 520))
let mapControl = getMapControl()
let activeCountControl = getActiveCountControl()
let changeMapTypeControl = getMapTypeControl()
let cacheTypeControl = getCachesTypesControl()
let displayedTypesControl = getWhatToDisplayControl()
let keywordsControl = getKeywordsControl()
let sizesControl = getSizesControl()
let cachesCountControl = getCachesDisplayedControl()
let stateControl = getStateControl()
let exportCachesButton = getExportCachesButton()

mainForm.Controls.Add(mapControl)
mainForm.Controls.Add(changeMapTypeControl)
mainForm.Controls.Add(activeCountControl)
mainForm.Controls.Add(cacheTypeControl)
mainForm.Controls.Add(displayedTypesControl)
mainForm.Controls.Add(keywordsControl)
mainForm.Controls.Add(sizesControl)
mainForm.Controls.Add(cachesCountControl)
mainForm.Controls.Add(stateControl)
mainForm.Controls.Add(exportCachesButton)

let mutable markers = [||]

let getSyncContext() = 
    let syncContext = SynchronizationContext.Current
    do if syncContext = null then failwith "no synchronization context found"
    syncContext
let triggerEvent fce (syncContext:SynchronizationContext) =
    syncContext.Post(SendOrPostCallback(fce), state=null)

[<STAThread>]

do
    let updateCountOfCaches() =
        printf "Counting visible caches.."
        let count = getVisibleObjectsCount markers mapControl
        printfn "..done. Count: %d" count
        cachesCountControl.Text <- "Count of displayed caches: " + count.ToString()

    let updateMarkersVisibility() =
        let selectedKeywords = keywordsControl.SelectedItems
        let foundSelected = displayedTypesControl.SelectedItems.Contains(FoundConst)
        let waitingSelected = displayedTypesControl.SelectedItems.Contains(WaitingConst)
        let selectedSizes = sizesControl.SelectedItems
        let mapTypes = getMappedSelectedTypes cacheTypeControl

        let visibleByKeyword marker =
            selectedKeywords.Count = 0 || Array.exists (fun k->selectedKeywords.Contains(k)) marker.cache.Keywords
        let visibleByFound marker =
            (not foundSelected && not waitingSelected) ||
            (match marker.cache.Found with
                | true -> foundSelected
                | false -> waitingSelected)            
        let visibleBySize marker =
            selectedSizes.Count = 0 || selectedSizes.Contains(match marker.cache.Size with | 5 -> SizeUnknownConst | s -> s.ToString())
        let visibleByType marker =
            mapTypes.[marker.cache.Type]

        let count = (markers |> Array.map (fun m -> m.marker.IsVisible <- (visibleByKeyword m) && 
                                                                          (visibleByFound m) && 
                                                                          (visibleBySize m) &&
                                                                          (visibleByType m)
                                                    m
                                            )
                    ).Length
        activeCountControl.Text <- "" + count.ToString()

    mainForm.Shown.Add(
        fun e ->
            printfn "%A starting async" System.DateTime.Now 
            let syncContext = getSyncContext()
            let progress = new Event<_>()
            let stateChanged = new Event<_>()
            let actOnPercent totalCount currentIndex = 
                triggerEvent (fun _ -> progress.Trigger((totalCount, currentIndex))) syncContext
            let asynccomp = async {
                let caches, keywords = loadCaches actOnPercent
                addKeywords keywords keywordsControl
                
                triggerEvent (fun _ -> stateChanged.Trigger("Adding caches to layers.")) syncContext
                let layer, m = addCachesToLayer mapControl caches
                printfn "%A Added to layers.." System.DateTime.Now 
                markers <- m
                triggerEvent (fun _ -> stateChanged.Trigger("Caches processing finished.")) syncContext
                 
                updateMarkersVisibility()
                updateCountOfCaches()
                triggerEvent (fun _ -> stateChanged.Trigger("Caches imported.")) syncContext
            }
            progress.Publish.Add(fun (total,index) -> let steps = 200
                                                      if total > steps && index % (total / steps) = 0 then
                                                         stateControl.Text <- sprintf "Importing caches - %d from %d." (index+1) total)
            stateChanged.Publish.Add(fun message -> stateControl.Text <- message
                                                    printf "%A " System.DateTime.Now
                                                    printfn "%s" message)
            Async.Start asynccomp
            triggerEvent (fun _ -> stateChanged.Trigger("Caches reading started.")) syncContext
    )
    changeMapTypeControl.SelectedValueChanged.Add( 
        fun e ->
            let item = changeMapTypeControl.SelectedItem.ToString()
            if item = mapGoogleStreetConst then mapControl.MapType <- MapType.GoogleMap
            elif item = mapGoogleSateliteConst then mapControl.MapType <- MapType.GoogleSatellite
            elif item = mapGoogleHybridConst then mapControl.MapType <- MapType.GoogleHybrid
            elif item = mapBingMapConst then mapControl.MapType <- MapType.BingMap
            elif item = mapBingSateliteConst then mapControl.MapType <- MapType.BingSatellite
            elif item = mapBingHybridConst then mapControl.MapType <- MapType.BingHybrid
            elif item = mapYahooMapConst then mapControl.MapType <- MapType.YahooMap
            elif item = mapYahooSateliteConst then mapControl.MapType <- MapType.YahooSatellite
            elif item = mapYahooHybridConst then mapControl.MapType <- MapType.YahooHybrid
            elif item = mapMapyCZStreetConst then mapControl.MapType <- MapType.MapyCZ_Map
            elif item = mapMapyCZTouristConst then mapControl.MapType <- MapType.MapyCZ_MapTurist
            elif item = mapMapyCZSateliteConst then mapControl.MapType <- MapType.MapyCZ_Satellite
            elif item = mapMapyCZHybridConst then mapControl.MapType <- MapType.MapyCZ_Hybrid
            else failwith "unknown"

            updateCountOfCaches()
        )
    mapControl.add_OnMarkerClick(
        fun (m:GMapMarker) _ ->
            let mg = m :?> CachesImages.GeocachingImageMarker
            markers 
                |> Array.filter (fun m -> m.cache.Id = mg.Id) 
                |> Array.iter (fun m -> openUrl m.cache.Url)
    )
    cacheTypeControl.SelectedIndexChanged.Add(
        fun _ -> updateMarkersVisibility()
                 updateCountOfCaches()
    )
    displayedTypesControl.SelectedIndexChanged.Add(
        fun _ -> updateMarkersVisibility()
                 updateCountOfCaches()
    )
    keywordsControl.SelectedIndexChanged.Add(
        fun _ -> updateMarkersVisibility()
                 updateCountOfCaches()
    )
    sizesControl.SelectedIndexChanged.Add(
        fun _ -> updateMarkersVisibility()
                 updateCountOfCaches()
    )
    mapControl.add_OnCurrentPositionChanged(
        fun f -> updateCountOfCaches()
    )
    mapControl.add_OnMapZoomChanged(
        fun f -> updateCountOfCaches()
    )
    exportCachesButton.Click.Add(
        fun _ -> 
          stateControl.Text <- "Export started."
          let syncContext = getSyncContext()
          let completed = new Event<_>()
          let fileName = IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "exported.db")
          let caches = getVisibleObjects markers mapControl
          let task = async {
            newDb fileName
            caches
              |> Array.map (fun m -> m.cache)
              |> writeCachesToDb fileName
            triggerEvent (fun _ -> completed.Trigger(caches.Length)) syncContext
          }
          completed.Publish.Add(fun count -> stateControl.Text <- sprintf "Caches exported. Count: %d." count)
          Async.Start task
    )
    Application.EnableVisualStyles()
    Application.Run(mainForm)
    
(*
(*let joinWithBaseDir dir = 
    IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dir)
let storageDirectory = (if IO.Directory.Exists(joinWithBaseDir "store") then (joinWithBaseDir "store")
                        elif IO.Directory.Exists(joinWithBaseDir "..\\..\\store") then (joinWithBaseDir "..\\..\\store")
                        else failwith "Store directory not found")*)
                        
//let caches = CachesModel.readCaches storageDirectory |> Array.map CachesModel.readCache 
//caches |> Array.filter (fun (cache:Cache) -> cache.HasValidCoords)
//       |> Array.iter CachesModel.printCache
*)


(*mapControl.Click.Add(
        fun e ->
            let a = e :?> MouseEventArgs
            let xportion, yportion = (float a.X / float mapControl.Width, float a.Y / float mapControl.Height)
            let c = mapControl
            c.CurrentPosition <- 
                PointLatLng(c.CurrentViewArea.Top + c.CurrentViewArea.HeightLat * yportion * (if c.CurrentViewArea.Top > 0.0 then -1.0 else 1.0),
                            c.CurrentViewArea.Left + c.CurrentViewArea.WidthLng * xportion * (if c.CurrentViewArea.Left > 0.0 then 1.0 else -1.0))
        )
    *)