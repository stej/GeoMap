module Controls

open System.Drawing
open System.Windows.Forms
open GMap.NET
open GMap.NET.WindowsForms
open GMap.NET.WindowsForms.Markers
open CachesModel

let mapGoogleStreetConst = "Google Street"
let mapGoogleSateliteConst = "Google Satelite"
let mapGoogleHybridConst = "Google Hybrid"
let mapBingMapConst = "Bing Map"
let mapBingSateliteConst = "Bing Satelite"
let mapBingHybridConst = "Bing Hybrid"
let mapYahooMapConst = "Yahoo Map"
let mapYahooSateliteConst = "Yahoo Satelite"
let mapYahooHybridConst = "Yahoo Hybrid"
let mapMapyCZStreetConst = "MapyCZ Street"
let mapMapyCZHybridConst = "MapyCZ Hybrid"
let mapMapyCZSateliteConst = "MapyCZ Satelite"
let mapMapyCZTouristConst = "MapyCZ Tourist"
let TraditionalConst = "Traditional"
let MultiConst       = "Multi"
let EventConst       = "Event"
let MysteryConst     = "Mystery"
let LetterConst      = "Letter"
let EarthConst       = "Earth"
let WhereigoConst    = "Whereigo"
let FoundConst       = "Found"
let WaitingConst     = "Waiting"
let SizeUnknownConst = "Unknown"

let getMapControl() =
    let c = new GMap.NET.WindowsForms.GMapControl(
              Anchor = (AnchorStyles.Top ||| AnchorStyles.Bottom ||| AnchorStyles.Left ||| AnchorStyles.Right),
              Bearing = 0.0f,
              CanDragMap = true,
              GrayScaleMode = false,
              LevelsKeepInMemmory = 5,
              Location = System.Drawing.Point(0, 0),
              MapType = GMap.NET.MapType.GoogleMap,
              MarkersEnabled = true,
              MaxZoom = 18,
              MinZoom = 2,
              MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter,
              Name = "gMapControl1",
              PolygonsEnabled = true,
              RetryLoadTile = 0,
              RoutesEnabled = true,
              ShowTileGridLines = false,
              Size = System.Drawing.Size(700, 500),
              TabIndex = 0,
              VirtualSizeEnabled = false,
              Zoom = 12.0,
              Position = PointLatLng(49.6034664, 17.254005)) 
    c.Manager.Mode <- AccessMode.ServerAndCache
    c

let getMapTypeControl() =
    let c = new System.Windows.Forms.ComboBox(
              FormattingEnabled = true,
              Location = System.Drawing.Point(715, 6),
              Name = "mapType",
              Size = System.Drawing.Size(121, 21),
              TabIndex = 2,
              DropDownStyle = ComboBoxStyle.DropDownList)
    c.Anchor <- AnchorStyles.Top ||| AnchorStyles.Right
    c.Items.AddRange([|mapGoogleStreetConst
                       mapGoogleSateliteConst
                       mapGoogleHybridConst
                       mapBingMapConst
                       mapBingSateliteConst
                       mapBingHybridConst
                       mapYahooMapConst
                       mapYahooSateliteConst
                       mapYahooHybridConst
                       mapMapyCZStreetConst
                       mapMapyCZSateliteConst
                       mapMapyCZHybridConst
                       mapMapyCZTouristConst|])
    c.SelectedIndex <- 0
    c
let getActiveCountControl() = 
    new System.Windows.Forms.Label (
        Anchor = (AnchorStyles.Top ||| AnchorStyles.Right),
        Text = "Active: ",
        Location = System.Drawing.Point(715, 34),
        Size = System.Drawing.Size(120, 15),
        Visible = false
    )
let getCachesTypesControl() = 
    let c = new System.Windows.Forms.ListBox(
                FormattingEnabled = true,
                Anchor = (AnchorStyles.Top ||| AnchorStyles.Right),
                Location = System.Drawing.Point(715, 49),
                Name = "cachesTypes",
                Size = System.Drawing.Size(120, 105),
                TabIndex = 5,
                SelectionMode = SelectionMode.MultiExtended)
    c.Items.AddRange([|TraditionalConst :> System.Object
                       MultiConst :> System.Object
                       MysteryConst :> System.Object
                       EventConst :> System.Object
                       WhereigoConst :> System.Object
                       LetterConst :> System.Object
                       EarthConst :> System.Object|])
    for a in 0..6 do c.SelectedIndices.Add(a)
    c
let getWhatToDisplayControl() = 
    let c = new System.Windows.Forms.ListBox(
                FormattingEnabled = true,
                Anchor = (AnchorStyles.Top ||| AnchorStyles.Right),
                Location = System.Drawing.Point(715, 160),
                Name = "whatToDisplay",
                Size = System.Drawing.Size(120, 30),
                TabIndex = 6,
                SelectionMode = SelectionMode.MultiExtended)
    c.Items.AddRange([|FoundConst :> System.Object
                       WaitingConst :> System.Object|])
    for a in 0..1 do c.SelectedIndices.Add(a)
    c
let getKeywordsControl() = 
    let c = new System.Windows.Forms.ListBox(
                FormattingEnabled = true,
                Anchor = (AnchorStyles.Top ||| AnchorStyles.Right),
                Location = System.Drawing.Point(715, 195),
                Name = "keywords",
                Size = System.Drawing.Size(120, 75),
                TabIndex = 7,
                SelectionMode = SelectionMode.MultiExtended)
    //c.Items.AddRange(keywords |> Array.map (fun k -> k :> System.Object))
    c
let addKeywords keyw (keywctl:System.Windows.Forms.ListBox) = 
    keywctl.Items.AddRange(keyw |> Array.map (fun k -> k :> System.Object))
let getSizesControl() = 
    let c = new System.Windows.Forms.ListBox(
                FormattingEnabled = true,
                Anchor = (AnchorStyles.Top ||| AnchorStyles.Right),
                Location = System.Drawing.Point(715, 275),
                Name = "sizes",
                Size = System.Drawing.Size(120, 75),
                TabIndex = 8,
                SelectionMode = SelectionMode.MultiExtended)
    c.Items.AddRange([|"1" :> System.Object
                       "2" :> System.Object
                       "3" :> System.Object
                       "4" :> System.Object
                       SizeUnknownConst :> System.Object|])
    c
let getCachesDisplayedControl() =
    new System.Windows.Forms.Label(
               Anchor = (AnchorStyles.Bottom ||| AnchorStyles.Left),
               Location = System.Drawing.Point(0, 505),
               Size = System.Drawing.Size(200, 15),
               Text = "Count of displayed caches: ?")
let getStateControl() =
    new System.Windows.Forms.Label(
               Anchor = (AnchorStyles.Bottom ||| AnchorStyles.Left),
               Location = System.Drawing.Point(210, 505),
               Size = System.Drawing.Size(250, 15))
let getExportCachesButton() =
    new System.Windows.Forms.Button(
            Anchor = (AnchorStyles.Bottom ||| AnchorStyles.Right),
            Location = new System.Drawing.Point(800, 495),
            Size = new System.Drawing.Size(45, 25),
            TabIndex = 9,
            Text = "Export",
            UseVisualStyleBackColor = true)


let getMappedSelectedTypes (getCachesTypesControl:System.Windows.Forms.ListBox) =
    let s = getCachesTypesControl.SelectedItems
    dict[CacheType.Other, true
         CacheType.Whereigo, s.Contains(WhereigoConst)
         CacheType.Letter, s.Contains(LetterConst)
         CacheType.Earth, s.Contains(EarthConst)
         CacheType.Event, s.Contains(EventConst)
         CacheType.Mystery, s.Contains(MysteryConst)
         CacheType.MultiCache, s.Contains(MultiConst)
         CacheType.Traditional, s.Contains(TraditionalConst)]