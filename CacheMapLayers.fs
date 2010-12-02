module CacheMapLayers

open GMap.NET
open GMap.NET.WindowsForms
open GMap.NET.WindowsForms.Markers
open CachesModel
open CachesImages

type Marker = {
    cache: Cache
    position: PointLatLng
    marker: GMapMarker}
    
let getPositionFromCoords (cache:Cache) = 
    match cache.CoordsToShow with | (a,b) -> PointLatLng(a,b)
    
let addLayer mapControl name =
    let l = new GMapOverlay(mapControl, name)
    mapControl.Overlays.Add(l)
    l
    
let addToLayer point (cache:Cache) (layer:GMapOverlay) =
    let cacheType = cache.Type
    let found = cache.Found
    let m = match cacheType with
                | CacheType.Traditional -> new TraditionalCacheMarker(point) :> GeocachingImageMarker
                | CacheType.MultiCache -> new MultiCacheMarker(point) :> GeocachingImageMarker
                | CacheType.Mystery -> new MysteryCacheMarker(point) :> GeocachingImageMarker
                | CacheType.Event -> new EventCacheMarker(point) :> GeocachingImageMarker
                | CacheType.Whereigo -> new WhereigoCacheMarker(point) :> GeocachingImageMarker
                | CacheType.Letter -> new LetterCacheMarker(point) :> GeocachingImageMarker
                | CacheType.Earth -> new EarthCacheMarker(point) :> GeocachingImageMarker
                | CacheType.Other -> new UnknownCacheMarker(point) :> GeocachingImageMarker
    m.Found <- found
    m.Id <- cache.Id
    let coords = cache.CoordsToShow
    let coordsOrig = cache.CoordsToShowOrig
    m.ToolTipText <- cache.Id + " " + cache.Name + "\n" + 
                    (cacheSizeToXString cache.Size) + " " + (cacheTypeToString cache.Type) + "\n" +
                    (fst coords).ToString("f6") + " " + (snd coords).ToString("f4") + "\n" +
                    coordsOrig + 
                    (if cache.Notes <> null && cache.Notes.Length > 0 then "\n" + cache.Notes else "") +
                    (if not (System.String.IsNullOrEmpty(cache.Hint)) then "\nHint: " + cache.Hint else "")
    m.ToolTip <- new CacheTooltip(m)
    layer.Markers.Add(m)
    { marker = m; position = point; cache = cache }

let addCachesToLayer mapControl caches = 
    let layer = new GMapOverlay(mapControl, "caches")
    let markers = caches 
                    |> Array.filter (fun (cache:Cache) -> cache.HasValidCoords)
                    |> Array.map (fun (cache:Cache) -> (cache, getPositionFromCoords cache))
                    |> Array.map (fun cacheInfo -> match cacheInfo with | (cache, point) -> (addToLayer point cache layer))

    mapControl.Overlays.Add(layer)
    (layer, markers)

type ViewPort = {
    left: float
    right: float
    top: float
    bottom: float
}
let getMapControlSizes (mapControl:GMapControl) = 
    let left = mapControl.CurrentViewArea.Left
    let right = left + mapControl.CurrentViewArea.WidthLng
    let top = mapControl.CurrentViewArea.Top
    let bottom = top - mapControl.CurrentViewArea.HeightLat
    { left = left; right = right; top = top; bottom = bottom }

let cacheIsInside viewPort (point:PointLatLng) =
    viewPort.left <= point.Lng && point.Lng <= viewPort.right && 
    viewPort.bottom <= point.Lat && point.Lat <= viewPort.top

let getVisibleObjects markers mapControl = 
    let viewPort = getMapControlSizes mapControl
    markers |> Array.filter (fun m -> m.marker.IsVisible && (cacheIsInside viewPort m.position))

let getVisibleObjectsCount markers mapControl = 
    let viewPort = getMapControlSizes mapControl
    markers 
        |> Array.fold (fun count m -> 
                          match (cacheIsInside viewPort m.position) && m.marker.IsVisible with 
                            | true -> count + 1 
                            | _    -> count
                       )
                       0

(*let addCachesToLayers mapControl caches = 
    let getPositionFromCoords (cache:Cache) = 
        match cache.CoordsToShow with | (a,b) -> PointLatLng(a,b)
        
    let addLayer mapControl name =
        let l = new GMapOverlay(mapControl, name)
        mapControl.Overlays.Add(l)
        l

    let layers = dict[
                    CacheType.Other, addLayer mapControl "oth"
                    CacheType.Whereigo, addLayer mapControl "whereigo"
                    CacheType.Letter, addLayer mapControl "letter"
                    CacheType.Earth, addLayer mapControl "earth"
                    CacheType.Event, addLayer mapControl "event"
                    CacheType.Mystery, addLayer mapControl "mystery"
                    CacheType.MultiCache, addLayer mapControl "multi"
                    CacheType.Traditional, addLayer mapControl "traditional"]

    /// temporary
    let addToLayer point (cache:Cache) =
        let cacheType = cache.Type
        let found = cache.Found
        let m = match cacheType with
                    | CacheType.Traditional -> new TraditionalCacheMarker(point) :> GeocachingImageMarker
                    | CacheType.MultiCache -> new MultiCacheMarker(point) :> GeocachingImageMarker
                    | CacheType.Mystery -> new MysteryCacheMarker(point) :> GeocachingImageMarker
                    | CacheType.Event -> new EventCacheMarker(point) :> GeocachingImageMarker
                    | CacheType.Whereigo -> new WhereigoCacheMarker(point) :> GeocachingImageMarker
                    | CacheType.Letter -> new LetterCacheMarker(point) :> GeocachingImageMarker
                    | CacheType.Earth -> new EarthCacheMarker(point) :> GeocachingImageMarker
                    | CacheType.Other -> new UnknownCacheMarker(point) :> GeocachingImageMarker
        m.Found <- found
        m.Id <- cache.Id
        let coords = cache.CoordsToShow
        let coordsOrig = cache.CoordsToShowOrig
        m.ToolTipText <- cache.Id + " " + cache.Name + "\n" + 
                        (cacheSizeToXString cache.Size) + " " + (cacheTypeToString cache.Type) + "\n" +
                        (fst coords).ToString("f6") + " " + (snd coords).ToString("f4") + "\n" +
                        coordsOrig + 
                        (if cache.Notes <> null && cache.Notes.Length > 0 then "\n" + cache.Notes else "") +
                        (if not (System.String.IsNullOrEmpty(cache.Hint)) then "\nHint: " + cache.Hint else "")
        m.ToolTip <- new CacheTooltip(m)
        layers.[cacheType].Markers.Add(m)
        { marker = m; cache = cache }

    //let mutable markers = []
    let markers = caches 
                    |> Array.filter (fun (cache:Cache) -> cache.HasValidCoords)
                    |> Array.map (fun (cache:Cache) -> (cache, getPositionFromCoords cache))
                    |> Array.map (fun cacheInfo -> match cacheInfo with | (cache, point) -> (addToLayer point cache))
                                        
    (layers, markers)
*)