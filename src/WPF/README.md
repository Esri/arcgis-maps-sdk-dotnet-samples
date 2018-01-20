## Sample Table Of Contents
## Maps

* [Change viewpoint](ArcGISRuntime.WPF.Samples/Samples/MapView/ChangeViewpoint)

    This sample demonstrates different ways in which you can change the viewpoint or visible area of the map.

* [Map rotation](ArcGISRuntime.WPF.Samples/Samples/MapView/MapRotation)

    This sample demonstrates how to rotate a map.

* [Display drawing status](ArcGISRuntime.WPF.Samples/Samples/MapView/DisplayDrawingStatus)

    This sample demonstrates how to use the DrawStatus value of the MapView to notify user that the MapView is drawing.

* [Display layer view state](ArcGISRuntime.WPF.Samples/Samples/MapView/DisplayLayerViewState)

    This sample demonstrates how to get view status for layers in a map.

* [Take screenshot](ArcGISRuntime.WPF.Samples/Samples/MapView/TakeScreenshot)

    This sample demonstrates how you can take screenshot of a map. Click 'take screenshot' button to take a screenshot of the visible area of the map. Created image is shown in the sample after creation.

* [Show magnifier](ArcGISRuntime.WPF.Samples/Samples/MapView/ShowMagnifier)

    This sample demonstrates how you can tap and hold on a map to get the magnifier. You can also pan while tapping and holding to move the magnifier across the map.

* [Show callout](ArcGISRuntime.WPF.Samples/Samples/MapView/ShowCallout)

    This sample illustrates how to show callouts on a map in response to user interaction.

* [Feature layer time offset](ArcGISRuntime.WPF.Samples/Samples/MapView/FeatureLayerTimeOffset)

    This sample demonstrates how to show data from the same service side-by-side with a time offset. This allows for the comparison of data over time.

* [GeoView viewpoint synchronization](ArcGISRuntime.WPF.Samples/Samples/MapView/GeoViewSync)

    This sample demonstrates how to keep two geo views (MapView/SceneView) in sync with each other.

* [Display a map](ArcGISRuntime.WPF.Samples/Samples/Map/DisplayMap)

    This sample demonstrates how to display a map with a basemap.

* [Open map (URL)](ArcGISRuntime.WPF.Samples/Samples/Map/OpenMapURL)

    This sample demonstrates loading a webmap in a map from a Uri.

* [Open mobile map (map package)](ArcGISRuntime.WPF.Samples/Samples/Map/OpenMobileMap)

    This sample demonstrates how to open a mobile map from a map package.

* [Search a portal for maps](ArcGISRuntime.WPF.Samples/Samples/Map/SearchPortalMaps)

    This sample demonstrates searching a portal for web maps and loading them in the map view. You can search ArcGIS Online public web maps using tag values or browse the web maps in your account. OAuth is used to authenticate with ArcGIS Online to access items in your account.

* [Set min & max scale](ArcGISRuntime.WPF.Samples/Samples/Map/SetMinMaxScale)

    This sample demonstrates how to set the minimum and maximum scale of a Map. Setting the minimum and maximum scale for the Map can be useful in keeping the user focused at a certain level of detail.

* [Access load status](ArcGISRuntime.WPF.Samples/Samples/Map/AccessLoadStatus)

    This sample demonstrates how to access the Maps' LoadStatus. The LoadStatus will be considered loaded when the following are true: The Map has a valid SpatialReference and the Map has an been set to the MapView.

* [Set initial map location](ArcGISRuntime.WPF.Samples/Samples/Map/SetInitialMapLocation)

    This sample creates a map with a standard ESRI Imagery with Labels basemap that is centered on a latitude and longitude location and zoomed into a specific level of detail.

* [Set initial map area](ArcGISRuntime.WPF.Samples/Samples/Map/SetInitialMapArea)

    This sample demonstrates how to set the initial viewpoint from envelope defined by minimum (x,y) and maximum (x,y) values. The map's InitialViewpoint is set to this viewpoint before the map is loaded into the MapView. Upon loading the map zoom to this initial area.

* [Set map spatial reference](ArcGISRuntime.WPF.Samples/Samples/Map/SetMapSpatialReference)

    This sample demonstrates how you can set the spatial reference on a Map and all the layers would project accordingly.

* [Change basemap](ArcGISRuntime.WPF.Samples/Samples/Map/ChangeBasemap)

    This sample demonstrates how to dynamically change the basemap displayed in a Map.

* [Manage bookmarks](ArcGISRuntime.WPF.Samples/Samples/Map/ManageBookmarks)

    This sample demonstrates how to access and add bookmarks to a map.

* [Author a map](ArcGISRuntime.WPF.Samples/Samples/Map/AuthorMap)

    This sample demonstrates how to author and save a map as an ArcGIS portal item (web map). Saving a map to arcgis.com requires an ArcGIS Online login.

* [Read a GeoPackage](ArcGISRuntime.WPF.Samples/Samples/Data/ReadGeoPackage)

    This sample demonstrates how to open a GeoPackage file from the local file system and list the available GeoPackageRasters and GeoPackageFeatureTables from the GeoPackage. Users can add and remove the selected datasets as RasterLayers or FeatureLayers to the map.

## Layers

* [ArcGIS tiled layer (URL)](ArcGISRuntime.WPF.Samples/Samples/Layers/ArcGISTiledLayerUrl)

    This sample demonstrates how to add an ArcGISTiledLayer as a base layer in a map. The ArcGISTiledLayer comes from an ArcGIS Server sample web service.

* [ArcGIS vector tiled layer (URL)](ArcGISRuntime.WPF.Samples/Samples/Layers/ArcGISVectorTiledLayerUrl)

    This sample demonstrates how to create a ArcGISVectorTiledLayer and bind this to a Basemap which is used in the creation of a map.

* [Export tiles](ArcGISRuntime.WPF.Samples/Samples/Layers/ExportTiles)

    This sample demonstrates how to export tiles from a map server.

* [ArcGIS map image layer (URL)](ArcGISRuntime.WPF.Samples/Samples/Layers/ArcGISMapImageLayerUrl)

    This sample demonstrates how to add an ArcGISMapImageLayer as a base layer in a map. The ArcGISMapImageLayer comes from an ArcGIS Server sample web service.

* [Change sublayer visibility](ArcGISRuntime.WPF.Samples/Samples/Layers/ChangeSublayerVisibility)

    This sample demonstrates how to show or hide sublayers of a map image layer.

* [WMTS layer](ArcGISRuntime.WPF.Samples/Samples/Layers/WMTSLayer)

    This sample demonstrates how to display a WMTS layer on a map via a Uri and WmtsLayerInfo.

* [WMS layer (URL)](ArcGISRuntime.WPF.Samples/Samples/Layers/WMSLayerUrl)

    This sample demonstrates how to add a layer from a WMS service to a map.

* [WMS service catalog](ArcGISRuntime.WPF.Samples/Samples/Layers/WmsServiceCatalog)

    This sample demonstrates how to enable and disable the display of layers discovered from a WMS service.

* [Identify WMS features](ArcGISRuntime.WPF.Samples/Samples/Layers/WmsIdentify)

    This sample demonstrates how to identify WMS features and display the associated content for an identified WMS feature.


- **Raster Layers**

    * [ArcGIS raster layer (service)](ArcGISRuntime.WPF.Samples/Samples/Layers/RasterLayerImageServiceRaster)

    This sample demonstrates how to show a raster layer on a map based on an image service layer.

    * [ArcGIS raster function (service)](ArcGISRuntime.WPF.Samples/Samples/Layers/RasterLayerRasterFunction)

    This sample demonstrates how to show a raster layer on a map based on an image service layer that has a raster function applied.

    * [Raster layer (file)](ArcGISRuntime.WPF.Samples/Samples/Layers/RasterLayerFile)

    This sample demonstrates how to use a raster layer created from a local raster file.

    * [Raster rendering rule](ArcGISRuntime.WPF.Samples/Samples/Layers/RasterRenderingRule)

    This sample demonstrates how to create an `ImageServiceRaster`, fetch the `RenderingRule`s from the service info, and use a `RenderingRule` to create an `ImageServiceRaster` and add it to a raster layer.

    * [Raster layer (GeoPackage)](ArcGISRuntime.WPF.Samples/Samples/Data/RasterLayerGeoPackage)

    This sample demonstrates how to open a GeoPackage and show a GeoPackage raster in a raster layer.

## Features

* [Feature layer (feature service)](ArcGISRuntime.WPF.Samples/Samples/Layers/FeatureLayerUrl)

    This sample demonstrates how to show a feature layer on a map using the URL to the service.

* [Change feature layer renderer](ArcGISRuntime.WPF.Samples/Samples/Layers/ChangeFeatureLayerRenderer)

    This sample demonstrates how to change renderer for a feature layer. It also shows how to reset the renderer back to the default.

* [Feature layer selection](ArcGISRuntime.WPF.Samples/Samples/Layers/FeatureLayerSelection)

    This sample demonstrates how to select features in a feature layer by tapping a MapView.

* [Feature layer definition expression](ArcGISRuntime.WPF.Samples/Samples/Layers/FeatureLayerDefinitionExpression)

    This sample demonstrates how to apply definition expression to a feature layer for filtering features. It also shows how to reset the definition expression.

* [Create a new feature collection layer](ArcGISRuntime.WPF.Samples/Samples/Layers/CreateFeatureCollectionLayer)

    This sample demonstrates how to create a new feature collection with several feature collection tables. The collection is displayed in the map as a feature collection layer.

* [Create a feature collection layer from a portal item](ArcGISRuntime.WPF.Samples/Samples/Layers/FeatureCollectionLayerFromPortal)

    This sample demonstrates opening a feature collection saved as a portal item.

* [Feature collection layer from query result](ArcGISRuntime.WPF.Samples/Samples/Layers/FeatureCollectionLayerFromQuery)

    This sample demonstrates how to create a feature collection layer to show a query result from a service feature table.

* [Time-based query](ArcGISRuntime.WPF.Samples/Samples/Layers/TimeBasedQuery)

    This sample demonstrates how to apply a time-based parameter to a feature layer query.

* [Feature layer dictionary renderer](ArcGISRuntime.WPF.Samples/Samples/Layers/FeatureLayerDictionaryRenderer)

    Demonstrates how to apply a dictionary renderer to a feature layer and display mil2525d graphics. The dictionary renderer creates these graphics using a mil2525d style file and the attributes attached to each feature within the geodatabase.

* [Feature Layer Rendering Mode (Map)](ArcGISRuntime.WPF.Samples/Samples/Layers/FeatureLayerRenderingModeMap)

    This sample demonstrates how to use load settings to set preferred rendering mode for feature layers, specifically static or dynamic rendering modes.

* [Feature layer rendering mode (Scene)](ArcGISRuntime.WPF.Samples/Samples/Layers/FeatureLayerRenderingModeScene)

    This sample demonstrates how to use load settings to change the preferred rendering mode for a scene. Static rendering mode only redraws features periodically when a sceneview is navigating, while dynamic mode dynamically re-renders as the scene moves.

* [Service feature table (cache)](ArcGISRuntime.WPF.Samples/Samples/Data/ServiceFeatureTableCache)

    This sample demonstrates how to use a feature service in on interaction cache mode.

* [Service feature table (no cache)](ArcGISRuntime.WPF.Samples/Samples/Data/ServiceFeatureTableNoCache)

    This sample demonstrates how to use a feature service in on interaction no cache mode.

* [Service feature table (manual cache)](ArcGISRuntime.WPF.Samples/Samples/Data/ServiceFeatureTableManualCache)

    This sample demonstrates how to use a feature service in manual cache mode.

* [Feature layer query](ArcGISRuntime.WPF.Samples/Samples/Data/FeatureLayerQuery)

    This sample demonstrates how to query a feature layer via feature table.

* [Generate geodatabase](ArcGISRuntime.WPF.Samples/Samples/Data/GenerateGeodatabase)

    This sample demonstrates how to take a feature service offline by generating a geodatabase.

* [Geodatabase transactions](ArcGISRuntime.WPF.Samples/Samples/Data/GeodatabaseTransactions)

    This sample demonstrates how to manage edits to a local geodatabase inside of transactions.

* [Feature layer (shapefile)](ArcGISRuntime.WPF.Samples/Samples/Data/FeatureLayerShapefile)

    This sample demonstrates how to open a shapefile stored on the device and display it as a feature layer with default symbology.

* [Edit and sync features](ArcGISRuntime.WPF.Samples/Samples/Data/EditAndSyncFeatures)

    This sample demonstrates how to synchronize offline edits with a feature service.

* [Feature layer (GeoPackage)](ArcGISRuntime.WPF.Samples/Samples/Data/FeatureLayerGeoPackage)

    This sample demonstrates how to open a GeoPackage and show a GeoPackage feature table in a feature layer.

* [Read shapefile metadata](ArcGISRuntime.WPF.Samples/Samples/Data/ReadShapefileMetadata)

    This sample demonstrates how to open a shapefile stored on the device, read metadata that describes the dataset, and display it as a feature layer with default symbology.

* [Symbolize a shapefile](ArcGISRuntime.WPF.Samples/Samples/Data/SymbolizeShapefile)

    This sample demonstrates how to apply a custom renderer to a shapefile displayed by a feature layer.

* [List related features](ArcGISRuntime.WPF.Samples/Samples/Data/ListRelatedFeatures)

    This sample demonstrates how to query features related to an identified feature.

## Display Information

* [Add graphics (SimpleRenderer)](ArcGISRuntime.WPF.Samples/Samples/GraphicsOverlay/AddGraphicsRenderer)

    This sample demonstrates how you add graphics and set a renderer on a graphic overlays.

* [Identify graphics](ArcGISRuntime.WPF.Samples/Samples/GraphicsOverlay/IdentifyGraphics)

    This sample demonstrates how to identify graphics in a graphics overlay. When you tap on a graphic on the map, you will see an alert message displayed.

* [Sketch graphics on the map](ArcGISRuntime.WPF.Samples/Samples/GraphicsOverlay/SketchOnMap)

    This sample demonstrates how to interactively sketch and edit graphics in the map view and display them in a graphics overlay. You can sketch a variety of geometry types and undo or redo operations.

* [Surface placement](ArcGISRuntime.WPF.Samples/Samples/GraphicsOverlay/SurfacePlacements)

    This sample demonstrates how to position graphics using different Surface Placements.

* [Render simple markers](ArcGISRuntime.WPF.Samples/Samples/Symbology/RenderSimpleMarkers)

    This sample adds a point graphic to a graphics overlay symbolized with a red circle specified via a SimpleMarkerSymbol.

* [Render picture markers](ArcGISRuntime.WPF.Samples/Samples/Symbology/RenderPictureMarkers)

    This sample demonstrates how to create picture marker symbols from a URL and embedded resources.

* [Render unique values](ArcGISRuntime.WPF.Samples/Samples/Symbology/RenderUniqueValues)

    This sample demonstrate how to use a unique value renderer to style different features in a feature layer with different symbols. Features do not have a symbol property for you to set, renderers should be used to define the symbol for features in feature layers. The unique value renderer allows for separate symbols to be used for features that have specific attribute values in a defined field.

* [Simple renderer](ArcGISRuntime.WPF.Samples/Samples/Symbology/SimpleRenderers)

    This sample demonstrates how to create a simple renderer and add it to a graphics overlay. Renderers define the symbology for all graphics in a graphics overlay (unless they are overridden by setting the symbol directly on the graphic). Simple renderers can also be defined on feature layers using the same code.

## Analysis

* [Analyze hotspots](ArcGISRuntime.WPF.Samples/Samples/Geoprocessing/AnalyzeHotspots)

    This sample demonstrates how to execute the GeoprocessingTask asynchronously to calculate a hotspot analysis based on the frequency of 911 calls. It calculates the frequency of these calls within a given study area during a specified constrained time period set between 1/1/1998 and 5/31/1998.

* [Viewshed (Geoprocessing)](ArcGISRuntime.WPF.Samples/Samples/Geoprocessing/AnalyzeViewshed)

    This sample demonstrates how to use GeoprocessingTask to calculate a viewshed using a geoprocessing service. Click any point on the map to see all areas that are visible within a 1 kilometer radius. It may take a few seconds for the model to run and send back the results.

* [Viewshed for camera](ArcGISRuntime.WPF.Samples/Samples/Analysis/ViewshedCamera)

    This sample demonstrates how to create a `LocationViewshed` to display interactive viewshed results in the scene view. The viewshed observer is defined by the scene view camera to evaluate visible and obstructed areas of the scene from that location.

* [Line of sight from location](ArcGISRuntime.WPF.Samples/Samples/Analysis/LineOfSightLocation)

    This sample demonstrates a `LocationLineOfSight` analysis that shows segments that are visible or obstructed along a line drawn from observer to target.

* [Line of Sight (GeoElement)](ArcGISRuntime.WPF.Samples/Samples/Analysis/LineOfSightGeoElement)

    This sample demonstrates how to perform a dynamic line of sight analysis between two moving GeoElements.

* [List geodatabase versions](ArcGISRuntime.WPF.Samples/Samples/Geoprocessing/ListGeodatabaseVersions)

    This sample calls a custom GeoprocessingTask to get a list of available versions for an enterprise geodatabase. The task returns a table of geodatabase version information, which is displayed in the app as a list.

* [Query feature count and extent](ArcGISRuntime.WPF.Samples/Samples/Analysis/QueryFeatureCountAndExtent)

    This sample demonstrates how to query a feature table, in this case returning a count, for features that are within the visible extent or that meet specified criteria.

* [Statistical query](ArcGISRuntime.WPF.Samples/Samples/Data/StatisticalQuery)

    This sample demonstrates how to query a feature table to get statistics for a specified field.

* [Statistical query group and sort results](ArcGISRuntime.WPF.Samples/Samples/Data/StatsQueryGroupAndSort)

    This sample demonstrates how to query a feature table to get statistics for a specified field and to group and sort the results.

## Network Analysis

* [Find a route](ArcGISRuntime.WPF.Samples/Samples/NetworkAnalysis/FindRoute)

    This sample demonstrates how to solve for the best route between two locations on the map and display driving directions between them.

## Scenes

* [Distance composite symbol](ArcGISRuntime.WPF.Samples/Samples/Symbology/UseDistanceCompositeSym)

    This sample demonstrates how to create a `DistanceCompositeSceneSymbol` with unique marker symbols to display at various distances from the camera.

* [Feature layer extrusion](ArcGISRuntime.WPF.Samples/Samples/Symbology/FeatureLayerExtrusion)

    This sample demonstrates how to apply extrusion to a renderer on a feature layer.

* [ArcGIS scene layer (URL)](ArcGISRuntime.WPF.Samples/Samples/Layers/SceneLayerUrl)

    This sample demonstrates how to add an ArcGISSceneLayer as a layer in a Scene.

* [Display scene](ArcGISRuntime.WPF.Samples/Samples/Layers/DisplayScene)

    Demonstrates how to display a scene with an elevation data source. An elevation data source allows objects to be viewed in 3D, like this picture of Mt. Everest.

## Local Server

* [Dynamic workspace Raster](ArcGISRuntime.WPF.Samples/Samples/LocalServer/DynamicWorkspaceRaster)

    This sample demonstrates how to dynamically add a local Raster to a map using Local Server.

* [Local Server Map Image Layer](ArcGISRuntime.WPF.Samples/Samples/LocalServer/LocalServerMapImageLayer)

    This sample demonstrates how to display a Map Image Layer from a local map service

* [Local Server Services](ArcGISRuntime.WPF.Samples/Samples/LocalServer/LocalServerServices)

    This sample demonstrates how to control local server and manage running services.

* [Local Server Geoprocessing](ArcGISRuntime.WPF.Samples/Samples/LocalServer/LocalServerGeoprocessing)

    This sample demonstrates how to perform geoprocessing tasks using Local Server.

* [Dynamic workspace shapefile](ArcGISRuntime.WPF.Samples/Samples/LocalServer/DynamicWorkspaceShapefile)

    This sample demonstrates how to dynamically add a local shapefile to a map using Local Server.

* [Local Server Feature Layer](ArcGISRuntime.WPF.Samples/Samples/LocalServer/LocalServerFeatureLayer)

    This sample demonstrates how to display a Feature Layer service by a Local Server feature service.

## Hydrography

* [Select ENC features](ArcGISRuntime.WPF.Samples/Samples/Hydrography/SelectEncFeatures)

    This sample demonstrates how to select an ENC feature.

## Location

* [Display device location](ArcGISRuntime.WPF.Samples/Samples/Location/DisplayDeviceLocation)

    This sample demonstrates how you can enable location services and switch between different types of auto pan modes.

## Search


- **Working with Places**

    * [Find address](ArcGISRuntime.WPF.Samples/Samples/Search/FindAddress)

    This sample demonstrates how you can use the LocatorTask API to geocode an address and display it with a pin on the map. Tapping the pin displays the reverse-geocoded address in a callout.

    * [Find place](ArcGISRuntime.WPF.Samples/Samples/Search/FindPlace)

    This sample demonstrates how to use geocode functionality to search for points of interest, around a location or within an extent.

## GeometryEngine


- **Projection**

    * [Project with specific transformation](ArcGISRuntime.WPF.Samples/Samples/GeometryEngine/ProjectWithSpecificTransformation)

    This sample demonstrates how to use the GeometryEngine with a specified geographic transformation to transform a geometry from one coordinate system to another. 

## Geometry


- **Coordinates**

    * [Format coordinates](ArcGISRuntime.WPF.Samples/Samples/Geometry/FormatCoordinates)

    This sample demonstrates how to convert between `MapPoint` and string representations of a point using various coordinate systems.

## Tutorial

* [Author, edit, and save a map](ArcGISRuntime.WPF.Samples/Samples/Tutorial/AuthorEditSaveMap)

    This sample demonstrates how to author and save a map as an ArcGIS portal item (web map). It is also the solution to the [Author, edit, and save maps to your portal tutorial](https://developers.arcgis.com/net/latest/wpf/guide/author-edit-and-save-maps-to-your-portal.htm). Saving a map to arcgis.com requires an ArcGIS Online login.


