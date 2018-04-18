# Table of Contents

## Analysis

* [Line of Sight (GeoElement)](ArcGISRuntime.UWP.Viewer/Samples/Analysis/LineOfSightGeoElement)

This sample demonstrates how to perform a dynamic line of sight analysis between two moving GeoElements.

* [Line of sight from location](ArcGISRuntime.UWP.Viewer/Samples/Analysis/LineOfSightLocation)

This sample demonstrates a `LocationLineOfSight` analysis that shows segments that are visible or obstructed along a line drawn from observer to target.

* [Query feature count and extent](ArcGISRuntime.UWP.Viewer/Samples/Analysis/QueryFeatureCountAndExtent)

This sample demonstrates how to query a feature table, in this case returning a count, for features that are within the visible extent or that meet specified criteria.

* [Viewshed (GeoElement)](ArcGISRuntime.UWP.Viewer/Samples/Analysis/ViewshedGeoElement)

This sample demonstrates how to display a live viewshed analysis for a moving GeoElement. The analysis is offset vertically so that the viewpoint is from the top of the GeoElement (in this case, a model of a tank).

* [Viewshed (Location)](ArcGISRuntime.UWP.Viewer/Samples/Analysis/ViewshedLocation)

This sample demonstrates the configurable properties of viewshed analysis, including frustum color, heading, pitch, distances, angles, and location.

* [Viewshed for camera](ArcGISRuntime.UWP.Viewer/Samples/Analysis/ViewshedCamera)

This sample demonstrates how to create a `LocationViewshed` to display interactive viewshed results in the scene view. The viewshed observer is defined by the scene view camera to evaluate visible and obstructed areas of the scene from that location.

## Data

* [Edit and sync features](ArcGISRuntime.UWP.Viewer/Samples/Data/EditAndSyncFeatures)

This sample demonstrates how to synchronize offline edits with a feature service.

* [Feature layer (GeoPackage)](ArcGISRuntime.UWP.Viewer/Samples/Data/FeatureLayerGeoPackage)

This sample demonstrates how to open a GeoPackage and show a GeoPackage feature table in a feature layer.

* [Feature layer (shapefile)](ArcGISRuntime.UWP.Viewer/Samples/Data/FeatureLayerShapefile)

This sample demonstrates how to open a shapefile stored on the device and display it as a feature layer with default symbology.

* [Feature layer query](ArcGISRuntime.UWP.Viewer/Samples/Data/FeatureLayerQuery)

This sample demonstrates how to return features from a feature layer using an attribute query on the underlying feature table.

* [Generate geodatabase](ArcGISRuntime.UWP.Viewer/Samples/Data/GenerateGeodatabase)

This sample demonstrates how to take a feature service offline by generating a geodatabase.

* [Geodatabase transactions](ArcGISRuntime.UWP.Viewer/Samples/Data/GeodatabaseTransactions)

This sample demonstrates how to manage edits to a local geodatabase inside of transactions.

* [List related features](ArcGISRuntime.UWP.Viewer/Samples/Data/ListRelatedFeatures)

This sample demonstrates how to query features related to an identified feature.

* [Raster layer (GeoPackage)](ArcGISRuntime.UWP.Viewer/Samples/Data/RasterLayerGeoPackage)

This sample demonstrates how to open a GeoPackage and show a GeoPackage raster in a raster layer.

* [Read a GeoPackage](ArcGISRuntime.UWP.Viewer/Samples/Data/ReadGeoPackage)

This sample demonstrates how to open a GeoPackage file from the local file system and list the available GeoPackageRasters and GeoPackageFeatureTables from the GeoPackage. Users can add and remove the selected datasets as RasterLayers or FeatureLayers to the map.

* [Read shapefile metadata](ArcGISRuntime.UWP.Viewer/Samples/Data/ReadShapefileMetadata)

This sample demonstrates how to open a shapefile stored on the device, read metadata that describes the dataset, and display it as a feature layer with default symbology.

* [Service feature table (cache)](ArcGISRuntime.UWP.Viewer/Samples/Data/ServiceFeatureTableCache)

This sample demonstrates how to use a feature service in on interaction cache mode.

* [Service feature table (manual cache)](ArcGISRuntime.UWP.Viewer/Samples/Data/ServiceFeatureTableManualCache)

This sample demonstrates how to use a feature service in manual cache mode.

* [Service feature table (no cache)](ArcGISRuntime.UWP.Viewer/Samples/Data/ServiceFeatureTableNoCache)

This sample demonstrates how to use a feature service in on interaction no cache mode.

* [Statistical query](ArcGISRuntime.UWP.Viewer/Samples/Data/StatisticalQuery)

This sample demonstrates how to query a feature table to get statistics for a specified field.

* [Statistical query group and sort results](ArcGISRuntime.UWP.Viewer/Samples/Data/StatsQueryGroupAndSort)

This sample demonstrates how to query a feature table to get statistics for a specified field and to group and sort the results.

* [Symbolize a shapefile](ArcGISRuntime.UWP.Viewer/Samples/Data/SymbolizeShapefile)

This sample demonstrates how to apply a custom renderer to a shapefile displayed by a feature layer.

## Geometry

* [Format coordinates](ArcGISRuntime.UWP.Viewer/Samples/Geometry/FormatCoordinates)

This sample demonstrates how to convert between `MapPoint` and string representations of a point using various coordinate systems.

## GeometryEngine

* [Buffer](ArcGISRuntime.UWP.Viewer/Samples/GeometryEngine/Buffer)

This sample demonstrates how to use the GeometryEngine.Buffer to generate a polygon from an input geometry with a buffer distance.

* [Buffer list](ArcGISRuntime.UWP.Viewer/Samples/GeometryEngine/BufferList)

This sample demonstrates how to use the GeometryEngine.Buffer to generate one or more polygon from a series of input geometries and matching series of buffer distances. The option to union all the resulting buffer(s) is provided.

* [Create geometries](ArcGISRuntime.UWP.Viewer/Samples/GeometryEngine/CreateGeometries)

This sample demonstrates simple ways to create different types of Geometry. The geometries are added as Graphics to a GraphicsOverlay and displayed in a MapView. An Envelope is used to set the Viewpoint of the MapView.

* [Geodesic operations](ArcGISRuntime.UWP.Viewer/Samples/GeometryEngine/GeodesicOperations)

This sample demonstrates how to use the Geometry engine to calculate a geodesic path between two points and measure its distance.

* [List transformations by suitability](ArcGISRuntime.UWP.Viewer/Samples/GeometryEngine/ListTransformations)

This sample demonstrates how to use the TransformationCatalog to get a list of available DatumTransformations that can be used to project a Geometry between two different SpatialReferences, and how to use one of the transformations to perform the GeometryEngine.project operation. The TransformationCatalog is also used to set the location of files upon which grid-based transformations depend, and to find the default transformation used for the two SpatialReferences.

* [Nearest vertex](ArcGISRuntime.UWP.Viewer/Samples/GeometryEngine/NearestVertex)

This sample demonstrates how to use the Geometry engine find the nearest vertex and nearest coordinate of a polygon to a point. The distance for each is shown.

* [Project with specific transformation](ArcGISRuntime.UWP.Viewer/Samples/GeometryEngine/ProjectWithSpecificTransformation)

This sample demonstrates how to use the GeometryEngine with a specified geographic transformation to transform a geometry from one coordinate system to another. 

* [Spatial relationships](ArcGISRuntime.UWP.Viewer/Samples/GeometryEngine/SpatialRelationships)

This sample demonstrates how to use the GeometryEngine to evaluate the spatial relationships (for example, polygon a contains line b) between geometries.

## Geoprocessing

* [Analyze hotspots](ArcGISRuntime.UWP.Viewer/Samples/Geoprocessing/AnalyzeHotspots)

This sample demonstrates how to execute the GeoprocessingTask asynchronously to calculate a hotspot analysis based on the frequency of 911 calls. It calculates the frequency of these calls within a given study area during a specified constrained time period set between 1/1/1998 and 5/31/1998.

* [List geodatabase versions](ArcGISRuntime.UWP.Viewer/Samples/Geoprocessing/ListGeodatabaseVersions)

This sample calls a custom GeoprocessingTask to get a list of available versions for an enterprise geodatabase. The task returns a table of geodatabase version information, which is displayed in the app as a list.

* [Viewshed (Geoprocessing)](ArcGISRuntime.UWP.Viewer/Samples/Geoprocessing/AnalyzeViewshed)

This sample demonstrates how to use GeoprocessingTask to calculate a viewshed using a geoprocessing service. Click any point on the map to see all areas that are visible within a 1 kilometer radius. It may take a few seconds for the model to run and send back the results.

## GraphicsOverlay

* [Add graphics (SimpleRenderer)](ArcGISRuntime.UWP.Viewer/Samples/GraphicsOverlay/AddGraphicsRenderer)

This sample demonstrates how you add graphics and set a renderer on a graphic overlays.

* [Add graphics with symbols](ArcGISRuntime.UWP.Viewer/Samples/GraphicsOverlay/AddGraphicsWithSymbols)

This sample demonstrates how to add various types of graphics to a `GraphicsOverlay`.

* [Animate 3D Graphic](ArcGISRuntime.UWP.Viewer/Samples/GraphicsOverlay/Animate3DGraphic)

This sample demonstrates how to animate a graphic's position and follow it using a camera controller.

* [Identify graphics](ArcGISRuntime.UWP.Viewer/Samples/GraphicsOverlay/IdentifyGraphics)

This sample demonstrates how to identify graphics in a graphics overlay. When you tap on a graphic on the map, you will see an alert message displayed.

* [Sketch graphics on the map](ArcGISRuntime.UWP.Viewer/Samples/GraphicsOverlay/SketchOnMap)

This sample demonstrates how to interactively sketch and edit graphics in the map view and display them in a graphics overlay. You can sketch a variety of geometry types and undo or redo operations.

* [Surface placement](ArcGISRuntime.UWP.Viewer/Samples/GraphicsOverlay/SurfacePlacements)

This sample demonstrates how to position graphics using different Surface Placements.

## Hydrography

* [Add ENC Exchange Set](ArcGISRuntime.UWP.Viewer/Samples/Hydrography/AddEncExchangeSet)

This sample demonstrates how to load an ENC Exchange Set, including its component cells.

* [Change ENC display settings](ArcGISRuntime.UWP.Viewer/Samples/Hydrography/ChangeEncDisplaySettings)

This sample demonstrates how to control ENC environment settings. These settings apply to the display of all ENC content in your app.

* [Select ENC features](ArcGISRuntime.UWP.Viewer/Samples/Hydrography/SelectEncFeatures)

This sample demonstrates how to select an ENC feature.

## Layers

* [ArcGIS map image layer (URL)](ArcGISRuntime.UWP.Viewer/Samples/Layers/ArcGISMapImageLayerUrl)

This sample demonstrates how to add an ArcGISMapImageLayer as a base layer in a map. The ArcGISMapImageLayer comes from an ArcGIS Server sample web service.

* [ArcGIS raster function (service)](ArcGISRuntime.UWP.Viewer/Samples/Layers/RasterLayerRasterFunction)

This sample demonstrates how to show a raster layer on a map based on an image service layer that has a raster function applied.

* [ArcGIS raster layer (service)](ArcGISRuntime.UWP.Viewer/Samples/Layers/RasterLayerImageServiceRaster)

This sample demonstrates how to show a raster layer on a map based on an image service layer.

* [ArcGIS scene layer (URL)](ArcGISRuntime.UWP.Viewer/Samples/Layers/SceneLayerUrl)

This sample demonstrates how to add an ArcGISSceneLayer as a layer in a Scene.

* [ArcGIS tiled layer (URL)](ArcGISRuntime.UWP.Viewer/Samples/Layers/ArcGISTiledLayerUrl)

This sample demonstrates how to add an ArcGISTiledLayer as a base layer in a map. The ArcGISTiledLayer comes from an ArcGIS Server sample web service.

* [ArcGIS vector tiled layer (URL)](ArcGISRuntime.UWP.Viewer/Samples/Layers/ArcGISVectorTiledLayerUrl)

This sample demonstrates how to create a ArcGISVectorTiledLayer and bind this to a Basemap which is used in the creation of a map.

* [Blend renderer](ArcGISRuntime.UWP.Viewer/Samples/Layers/ChangeBlendRenderer)

This sample demonstrates how to use blend renderer on a raster layer. You can get a hillshade blended with either a colored raster or color ramp.

* [Change feature layer renderer](ArcGISRuntime.UWP.Viewer/Samples/Layers/ChangeFeatureLayerRenderer)

This sample demonstrates how to change renderer for a feature layer. It also shows how to reset the renderer back to the default.

* [Change sublayer visibility](ArcGISRuntime.UWP.Viewer/Samples/Layers/ChangeSublayerVisibility)

This sample demonstrates how to show or hide sublayers of a map image layer.

* [Create a feature collection layer from a portal item](ArcGISRuntime.UWP.Viewer/Samples/Layers/FeatureCollectionLayerFromPortal)

This sample demonstrates opening a feature collection saved as a portal item.

* [Create a new feature collection layer](ArcGISRuntime.UWP.Viewer/Samples/Layers/CreateFeatureCollectionLayer)

This sample demonstrates how to create a new feature collection with several feature collection tables. The collection is displayed in the map as a feature collection layer.

* [Display scene](ArcGISRuntime.UWP.Viewer/Samples/Layers/DisplayScene)

Demonstrates how to display a scene with an elevation data source. An elevation data source allows objects to be viewed in 3D, like this picture of Mt. Everest.

* [Export tiles](ArcGISRuntime.UWP.Viewer/Samples/Layers/ExportTiles)

This sample demonstrates how to export tiles from a map server.

* [Feature collection layer from query result](ArcGISRuntime.UWP.Viewer/Samples/Layers/FeatureCollectionLayerFromQuery)

This sample demonstrates how to create a feature collection layer to show a query result from a service feature table.

* [Feature layer (feature service)](ArcGISRuntime.UWP.Viewer/Samples/Layers/FeatureLayerUrl)

This sample demonstrates how to show a feature layer on a map using the URL to the service.

* [Feature layer definition expression](ArcGISRuntime.UWP.Viewer/Samples/Layers/FeatureLayerDefinitionExpression)

This sample demonstrates how to apply definition expression to a feature layer for filtering features. It also shows how to reset the definition expression.

* [Feature layer dictionary renderer](ArcGISRuntime.UWP.Viewer/Samples/Layers/FeatureLayerDictionaryRenderer)

Demonstrates how to apply a dictionary renderer to a feature layer and display mil2525d graphics. The dictionary renderer creates these graphics using a mil2525d style file and the attributes attached to each feature within the geodatabase.

* [Feature Layer Rendering Mode (Map)](ArcGISRuntime.UWP.Viewer/Samples/Layers/FeatureLayerRenderingModeMap)

This sample demonstrates how to use load settings to set preferred rendering mode for feature layers, specifically static or dynamic rendering modes.

* [Feature layer rendering mode (Scene)](ArcGISRuntime.UWP.Viewer/Samples/Layers/FeatureLayerRenderingModeScene)

This sample demonstrates how to use load settings to change the preferred rendering mode for a scene. Static rendering mode only redraws features periodically when a sceneview is navigating, while dynamic mode dynamically re-renders as the scene moves.

* [Feature layer selection](ArcGISRuntime.UWP.Viewer/Samples/Layers/FeatureLayerSelection)

This sample demonstrates how to select features in a feature layer by tapping a MapView.

* [Identify WMS features](ArcGISRuntime.UWP.Viewer/Samples/Layers/WmsIdentify)

This sample demonstrates how to identify WMS features and display the associated content for an identified WMS feature.

* [Raster hillshade renderer](ArcGISRuntime.UWP.Viewer/Samples/Layers/RasterHillshade)

This sample demonstrates how to use a hillshade renderer on a raster layer. Hillshade renderers can adjust a grayscale raster (usually of terrain) according to a hypothetical sun position (azimuth and altitude).

* [Raster layer (file)](ArcGISRuntime.UWP.Viewer/Samples/Layers/RasterLayerFile)

This sample demonstrates how to use a raster layer created from a local raster file.

* [Raster rendering rule](ArcGISRuntime.UWP.Viewer/Samples/Layers/RasterRenderingRule)

This sample demonstrates how to create an `ImageServiceRaster`, fetch the `RenderingRule`s from the service info, and use a `RenderingRule` to create an `ImageServiceRaster` and add it to a raster layer.

* [Raster RGB renderer](ArcGISRuntime.UWP.Viewer/Samples/Layers/RasterRgbRenderer)

This sample demonstrates how to use an RGB renderer on a raster layer. An RGB renderer is used to adjust the color bands of a multi-spectral image.

* [Stretch renderer](ArcGISRuntime.UWP.Viewer/Samples/Layers/ChangeStretchRenderer)

This sample demonstrates how to use stretch renderer on a raster layer.

* [Time-based query](ArcGISRuntime.UWP.Viewer/Samples/Layers/TimeBasedQuery)

This sample demonstrates how to apply a time-based parameter to a feature layer query.

* [Web TiledLayer](ArcGISRuntime.UWP.Viewer/Samples/Layers/Web_TiledLayer)

This sample demonstrates how to load a web tiled layer from a non-ArcGIS service, including how to include proper attribution.

* [WMS layer (URL)](ArcGISRuntime.UWP.Viewer/Samples/Layers/WMSLayerUrl)

This sample demonstrates how to add a layer from a WMS service to a map.

* [WMS service catalog](ArcGISRuntime.UWP.Viewer/Samples/Layers/WmsServiceCatalog)

This sample demonstrates how to enable and disable the display of layers discovered from a WMS service.

* [WMTS layer](ArcGISRuntime.UWP.Viewer/Samples/Layers/WMTSLayer)

This sample demonstrates how to display a WMTS layer on a map via a Uri and WmtsLayerInfo.

## Location

* [Display device location](ArcGISRuntime.UWP.Viewer/Samples/Location/DisplayDeviceLocation)

This sample demonstrates how you can enable location services and switch between different types of auto pan modes.

## Map

* [Access load status](ArcGISRuntime.UWP.Viewer/Samples/Map/AccessLoadStatus)

This sample demonstrates how to access the Maps' LoadStatus. The LoadStatus will be considered loaded when the following are true: The Map has a valid SpatialReference and the Map has an been set to the MapView.

* [Author a map](ArcGISRuntime.UWP.Viewer/Samples/Map/AuthorMap)

This sample demonstrates how to author and save a map as an ArcGIS portal item (web map). Saving a map to arcgis.com requires an ArcGIS Online login.

* [Change basemap](ArcGISRuntime.UWP.Viewer/Samples/Map/ChangeBasemap)

This sample demonstrates how to dynamically change the basemap displayed in a Map.

* [Display a map](ArcGISRuntime.UWP.Viewer/Samples/Map/DisplayMap)

This sample demonstrates how to display a map with a basemap.

* [Download preplanned map areas](ArcGISRuntime.UWP.Viewer/Samples/Map/DownloadPreplannedMapAreas)

This sample demonstrates how to download preplanned map areas from a webmap. In the preplanned offline workflow, the author of the online map defines map areas for offline use. When these areas are created, their offline packages are created and stored online for clients to download. This is the biggest difference between on-demand and preplanned workflows since on-demand workflow data packages are generated at request time.

* [Manage bookmarks](ArcGISRuntime.UWP.Viewer/Samples/Map/ManageBookmarks)

This sample demonstrates how to access and add bookmarks to a map.

* [Open map (URL)](ArcGISRuntime.UWP.Viewer/Samples/Map/OpenMapURL)

This sample demonstrates loading a webmap in a map from a Uri.

* [Open mobile map (map package)](ArcGISRuntime.UWP.Viewer/Samples/Map/OpenMobileMap)

This sample demonstrates how to open a mobile map from a map package.

* [Search a portal for maps](ArcGISRuntime.UWP.Viewer/Samples/Map/SearchPortalMaps)

This sample demonstrates searching a portal for web maps and loading them in the map view. You can search ArcGIS Online public web maps using tag values or browse the web maps in your account. OAuth is used to authenticate with ArcGIS Online to access items in your account.

* [Set initial map area](ArcGISRuntime.UWP.Viewer/Samples/Map/SetInitialMapArea)

This sample displays a map at a specific viewpoint. In this sample a viewpoint is constructed from an envelope defined by minimum (x,y) and maximum (x,y) values. The map's initialViewpoint is set to this viewpoint before the map is loaded. Upon loading the map zooms to this initial area.

* [Set initial map location](ArcGISRuntime.UWP.Viewer/Samples/Map/SetInitialMapLocation)

This sample creates a map with a standard ESRI Imagery with Labels basemap that is centered on a latitude and longitude location and zoomed into a specific level of detail.

* [Set map spatial reference](ArcGISRuntime.UWP.Viewer/Samples/Map/SetMapSpatialReference)

This sample demonstrates how you can set the spatial reference on a Map and all the layers would project accordingly.

* [Set min & max scale](ArcGISRuntime.UWP.Viewer/Samples/Map/SetMinMaxScale)

This sample demonstrates how to set the minimum and maximum scale of a Map. Setting the minimum and maximum scale for the Map can be useful in keeping the user focused at a certain level of detail.

## MapView

* [Change time extent](ArcGISRuntime.UWP.Viewer/Samples/MapView/ChangeTimeExtent)

This sample demonstrates how to filter data in layers by applying a time extent to a MapView.

* [Change viewpoint](ArcGISRuntime.UWP.Viewer/Samples/MapView/ChangeViewpoint)

This sample demonstrates different ways in which you can change the viewpoint or visible area of the map.

* [Display drawing status](ArcGISRuntime.UWP.Viewer/Samples/MapView/DisplayDrawingStatus)

This sample demonstrates how to use the DrawStatus value of the MapView to notify user that the MapView is drawing.

* [Display layer view state](ArcGISRuntime.UWP.Viewer/Samples/MapView/DisplayLayerViewState)

This sample demonstrates how to get view status for layers in a map.

* [Feature layer time offset](ArcGISRuntime.UWP.Viewer/Samples/MapView/FeatureLayerTimeOffset)

This sample demonstrates how to show data from the same service side-by-side with a time offset. This allows for the comparison of data over time.

* [GeoView viewpoint synchronization](ArcGISRuntime.UWP.Viewer/Samples/MapView/GeoViewSync)

This sample demonstrates how to keep two geo views (MapView/SceneView) in sync with each other.

* [Map rotation](ArcGISRuntime.UWP.Viewer/Samples/MapView/MapRotation)

This sample demonstrates how to rotate a map.

* [Show callout](ArcGISRuntime.UWP.Viewer/Samples/MapView/ShowCallout)

This sample illustrates how to show callouts on a map in response to user interaction.

* [Show magnifier](ArcGISRuntime.UWP.Viewer/Samples/MapView/ShowMagnifier)

This sample demonstrates how you can tap and hold on a map to get the magnifier. You can also pan while tapping and holding to move the magnifier across the map.

* [Take screenshot](ArcGISRuntime.UWP.Viewer/Samples/MapView/TakeScreenshot)

This sample demonstrates how you can take screenshot of a map. Click 'capture' button to take a screenshot of the visible area of the map. Created image is shown in the sample after creation.

## Network Analysis

* [Find a route](ArcGISRuntime.UWP.Viewer/Samples/Network%20Analysis/FindRoute)

This sample demonstrates how to solve for the best route between two locations on the map and display driving directions between them.

## Search

* [Find address](ArcGISRuntime.UWP.Viewer/Samples/Search/FindAddress)

This sample demonstrates how you can use the LocatorTask API to geocode an address and display it with a pin on the map. Tapping the pin displays the reverse-geocoded address in a callout.

* [Find place](ArcGISRuntime.UWP.Viewer/Samples/Search/FindPlace)

This sample demonstrates how to use geocode functionality to search for points of interest, around a location or within an extent.

## Symbology

* [Distance composite symbol](ArcGISRuntime.UWP.Viewer/Samples/Symbology/UseDistanceCompositeSym)

This sample demonstrates how to create a `DistanceCompositeSceneSymbol` with unique marker symbols to display at various distances from the camera.

* [Feature Layer Extrusion](ArcGISRuntime.UWP.Viewer/Samples/Symbology/FeatureLayerExtrusion)

This sample demonstrates how to apply extrusion to a renderer on a feature layer.

* [Render picture markers](ArcGISRuntime.UWP.Viewer/Samples/Symbology/RenderPictureMarkers)

This sample demonstrates how to create picture marker symbols from a URL and embedded resources.

* [Render simple markers](ArcGISRuntime.UWP.Viewer/Samples/Symbology/RenderSimpleMarkers)

This sample adds a point graphic to a graphics overlay symbolized with a red circle specified via a SimpleMarkerSymbol.

* [Render unique values](ArcGISRuntime.UWP.Viewer/Samples/Symbology/RenderUniqueValues)

This sample demonstrate how to use a unique value renderer to style different features in a feature layer with different symbols. Features do not have a symbol property for you to set, renderers should be used to define the symbol for features in feature layers. The unique value renderer allows for separate symbols to be used for features that have specific attribute values in a defined field.

* [Simple renderer](ArcGISRuntime.UWP.Viewer/Samples/Symbology/SimpleRenderers)

This sample demonstrates how to create a simple renderer and add it to a graphics overlay. Renderers define the symbology for all graphics in a graphics overlay (unless they are overridden by setting the symbol directly on the graphic). Simple renderers can also be defined on feature layers using the same code.

## Tutorial

* [Author, edit, and save a map](ArcGISRuntime.UWP.Viewer/Samples/Tutorial/AuthorEditSaveMap)

This sample demonstrates how to author and save a map as an ArcGIS portal item (web map). It is also the solution to the [Author, edit, and save maps to your portal tutorial](https://developers.arcgis.com/net/latest/uwp/guide/author-edit-and-save-maps-to-your-portal.htm). Saving a map to arcgis.com requires an ArcGIS Online login.

