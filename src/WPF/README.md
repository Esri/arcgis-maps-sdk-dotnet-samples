# Table of Contents

## Analysis

* [Distance measurement analysis](ArcGISRuntime.WPF.Viewer/Samples/Analysis/DistanceMeasurement)

This sample demonstrates measuring 3D distances between two points in a scene. The distance measurement analysis allows you to add the same measuring experience found in ArcGIS Pro, City Engine, and the ArcGIS API for JavaScript to your app. You can set the unit system of measurement (metric or imperial) and have the units automatically switch to one appropriate for the current scale. The rendering is handled internally so they do not interfere with other analyses like viewsheds.

* [Line of sight (GeoElement)](ArcGISRuntime.WPF.Viewer/Samples/Analysis/LineOfSightGeoElement)

This sample demonstrates how to perform a dynamic line of sight analysis between two moving GeoElements.

* [Line of sight from location](ArcGISRuntime.WPF.Viewer/Samples/Analysis/LineOfSightLocation)

This sample demonstrates a `LocationLineOfSight` analysis that shows segments that are visible or obstructed along a line drawn from observer to target.

* [Query feature count and extent](ArcGISRuntime.WPF.Viewer/Samples/Analysis/QueryFeatureCountAndExtent)

This sample demonstrates how to query a feature table, in this case returning a count, for features that are within the visible extent or that meet specified criteria.

* [Viewshed (GeoElement)](ArcGISRuntime.WPF.Viewer/Samples/Analysis/ViewshedGeoElement)

This sample demonstrates how to display a live viewshed analysis for a moving GeoElement. The analysis is offset vertically so that the viewpoint is from the top of the GeoElement (in this case, a model of a tank).

* [Viewshed (Location)](ArcGISRuntime.WPF.Viewer/Samples/Analysis/ViewshedLocation)

This sample demonstrates the configurable properties of viewshed analysis, including frustum color, heading, pitch, distances, angles, and location.

* [Viewshed for camera](ArcGISRuntime.WPF.Viewer/Samples/Analysis/ViewshedCamera)

This sample demonstrates how to create a `LocationViewshed` to display interactive viewshed results in the scene view. The viewshed observer is defined by the scene view camera to evaluate visible and obstructed areas of the scene from that location.

## Data

* [Edit and sync features](ArcGISRuntime.WPF.Viewer/Samples/Data/EditAndSyncFeatures)

This sample demonstrates how to synchronize offline edits with a feature service.

* [Feature layer (geodatabase)](ArcGISRuntime.WPF.Viewer/Samples/Data/FeatureLayerGeodatabase)

This sample demonstrates how to consume an Esri .geodatabase file (aka. mobile geodatabase) by using a FeatureLayer and a GeodatabaseFeatureTable.

* [Feature layer (GeoPackage)](ArcGISRuntime.WPF.Viewer/Samples/Data/FeatureLayerGeoPackage)

This sample demonstrates how to open a GeoPackage and show a GeoPackage feature table in a feature layer.

* [Feature layer (shapefile)](ArcGISRuntime.WPF.Viewer/Samples/Data/FeatureLayerShapefile)

This sample demonstrates how to open a shapefile stored on the device and display it as a feature layer with default symbology.

* [Feature layer query](ArcGISRuntime.WPF.Viewer/Samples/Data/FeatureLayerQuery)

This sample demonstrates how to query a feature layer via feature table.

* [Generate geodatabase](ArcGISRuntime.WPF.Viewer/Samples/Data/GenerateGeodatabase)

This sample demonstrates how to take a feature service offline by generating a geodatabase.

* [Geodatabase transactions](ArcGISRuntime.WPF.Viewer/Samples/Data/GeodatabaseTransactions)

This sample demonstrates how to manage edits to a local geodatabase inside of transactions.

* [List related features](ArcGISRuntime.WPF.Viewer/Samples/Data/ListRelatedFeatures)

This sample demonstrates how to query features related to an identified feature.

* [Raster layer (GeoPackage)](ArcGISRuntime.WPF.Viewer/Samples/Data/RasterLayerGeoPackage)

This sample demonstrates how to open a GeoPackage and show a GeoPackage raster in a raster layer.

* [Read a GeoPackage](ArcGISRuntime.WPF.Viewer/Samples/Data/ReadGeoPackage)

This sample demonstrates how to open a GeoPackage file from the local file system and list the available GeoPackageRasters and GeoPackageFeatureTables from the GeoPackage. Users can add and remove the selected datasets as RasterLayers or FeatureLayers to the map.

* [Read shapefile metadata](ArcGISRuntime.WPF.Viewer/Samples/Data/ReadShapefileMetadata)

This sample demonstrates how to open a shapefile stored on the device, read metadata that describes the dataset, and display it as a feature layer with default symbology.

* [Service feature table (cache)](ArcGISRuntime.WPF.Viewer/Samples/Data/ServiceFeatureTableCache)

This sample demonstrates how to use a feature service in on interaction cache mode.

* [Service feature table (manual cache)](ArcGISRuntime.WPF.Viewer/Samples/Data/ServiceFeatureTableManualCache)

This sample demonstrates how to use a feature service in manual cache mode.

* [Service feature table (no cache)](ArcGISRuntime.WPF.Viewer/Samples/Data/ServiceFeatureTableNoCache)

This sample demonstrates how to use a feature service in on interaction no cache mode.

* [Statistical query](ArcGISRuntime.WPF.Viewer/Samples/Data/StatisticalQuery)

This sample demonstrates how to query a feature table to get statistics for a specified field.

* [Statistical query group and sort results](ArcGISRuntime.WPF.Viewer/Samples/Data/StatsQueryGroupAndSort)

This sample demonstrates how to query a feature table to get statistics for a specified field and to group and sort the results.

* [Symbolize a shapefile](ArcGISRuntime.WPF.Viewer/Samples/Data/SymbolizeShapefile)

This sample demonstrates how to apply a custom renderer to a shapefile displayed by a feature layer.

## Geometry

* [Buffer](ArcGISRuntime.WPF.Viewer/Samples/Geometry/Buffer)

This sample demonstrates how to use `GeometryEngine.Buffer` to create polygons from a map location and linear distance (radius). For each input location, the sample creates two buffer polygons (using the same distance) and displays them on the map using different symbols. One polygon is calculated using the `planar` (flat) coordinate space of the map's spatial reference. The other is created using a `geodesic` technique that considers the curved shape of the Earth's surface (which is generally a more accurate representation). Distortion in the map increases as you move away from the standard parallels of the spatial reference's projection. This map is in Web Mercator so areas near the equator are the most accurate. As you move the buffer location north or south from that line, you'll see a greater difference in the polygon size and shape. Planar operations are generally faster, but performance improvement may only be noticeable for large operations (buffering a great number or complex geometry).
Creating buffers is a core concept in GIS proximity analysis, allowing you to visualize and locate geographic features contained within a polygon. For example, suppose you wanted to visualize areas of your city where alcohol sales are prohibited because they are within 500 meters of a school. The first step in this proximity analysis would be to generate 500 meter buffer polygons around all schools in the city. Any such businesses you find inside one of the resulting polygons are violating the law. If you are using planar buffers, make sure that the input locations and distance are suited to the spatial reference you're using. Remember that you can also create your buffers using geodesic and then project them to the spatial reference you need for display or analysis. For more information about using buffer analysis, see [How buffer analysis works](https://pro.arcgis.com/en/pro-app/tool-reference/analysis/how-buffer-analysis-works.htm) in the ArcGIS Pro documentation.

* [Buffer list](ArcGISRuntime.WPF.Viewer/Samples/Geometry/BufferList)

This sample demonstrates how to use a planar (Euclidean) buffer operation by calling `GeometryEngine.Buffer` to generate one or more polygons from a collection of input geometries and a corresponding collection of buffer distances. The result polygons can be returned as individual geometries or unioned into a single polygon output.

* [Clip geometry](ArcGISRuntime.WPF.Viewer/Samples/Geometry/ClipGeometry)

This sample demonstrates how to clip a geometry with an envelope using the GeometryEngine

* [Convex hull](ArcGISRuntime.WPF.Viewer/Samples/Geometry/ConvexHull)

This sample demonstrates how to use the GeometryEngine.ConvexHull operation to generate a polygon that encloses a series of user-tapped map points.

* [Convex hull list](ArcGISRuntime.WPF.Viewer/Samples/Geometry/ConvexHullList)

This sample demonstrates how to use the GeometryEngine.ConvexHull to generate convex hull polygon(s) from multiple input geometries.

* [Create geometries](ArcGISRuntime.WPF.Viewer/Samples/Geometry/CreateGeometries)

This sample demonstrates simple ways to create different types of Geometry. The geometries are added as Graphics to a GraphicsOverlay and displayed in a MapView. An Envelope is used to set the Viewpoint of the MapView.

* [Cut geometry](ArcGISRuntime.WPF.Viewer/Samples/Geometry/CutGeometry)

This sample demonstrates how to cut a geometry with a polyline using the GeometryEngine.

* [Densify and generalize](ArcGISRuntime.WPF.Viewer/Samples/Geometry/DensifyAndGeneralize)

This sample demonstrates how to densify or generalize a polyline geometry. In this example, points representing a ship's location are shown at irregular intervals. You can densify the polyline to interpolate points along the line at regular intervals. Generalizing the polyline can also simplify the geometry while preserving its general shape.

* [Format coordinates](ArcGISRuntime.WPF.Viewer/Samples/Geometry/FormatCoordinates)

This sample demonstrates how to convert between `MapPoint` and string representations of a point using various coordinate systems.

* [Geodesic operations](ArcGISRuntime.WPF.Viewer/Samples/Geometry/GeodesicOperations)

This sample demonstrates how to use the Geometry engine to calculate a geodesic path between two points and measure its distance.

* [List transformations by suitability](ArcGISRuntime.WPF.Viewer/Samples/Geometry/ListTransformations)

This sample demonstrates how to use the TransformationCatalog to get a list of available DatumTransformations that can be used to project a Geometry between two different SpatialReferences, and how to use one of the transformations to perform the GeometryEngine.project operation. The TransformationCatalog is also used to set the location of files upon which grid-based transformations depend, and to find the default transformation used for the two SpatialReferences.

* [Nearest vertex](ArcGISRuntime.WPF.Viewer/Samples/Geometry/NearestVertex)

This sample demonstrates how to use the Geometry engine find the nearest vertex and nearest coordinate of a polygon to a point. The distance for each is shown.

* [Project with specific transformation](ArcGISRuntime.WPF.Viewer/Samples/Geometry/ProjectWithSpecificTransformation)

This sample demonstrates how to use the GeometryEngine with a specified geographic transformation to transform a geometry from one coordinate system to another. 

* [Spatial relationships](ArcGISRuntime.WPF.Viewer/Samples/Geometry/SpatialRelationships)

This sample demonstrates how to use the GeometryEngine to evaluate the spatial relationships (for example, polygon a contains line b) between geometries.

## Geoprocessing

* [Analyze hotspots](ArcGISRuntime.WPF.Viewer/Samples/Geoprocessing/AnalyzeHotspots)

This sample demonstrates how to execute the GeoprocessingTask asynchronously to calculate a hotspot analysis based on the frequency of 911 calls. It calculates the frequency of these calls within a given study area during a specified constrained time period set between 1/1/1998 and 5/31/1998.

* [List geodatabase versions](ArcGISRuntime.WPF.Viewer/Samples/Geoprocessing/ListGeodatabaseVersions)

This sample calls a custom GeoprocessingTask to get a list of available versions for an enterprise geodatabase. The task returns a table of geodatabase version information, which is displayed in the app as a list.

* [Viewshed (Geoprocessing)](ArcGISRuntime.WPF.Viewer/Samples/Geoprocessing/AnalyzeViewshed)

This sample demonstrates how to use GeoprocessingTask to calculate a viewshed using a geoprocessing service. Click any point on the map to see all areas that are visible within a 1 kilometer radius. It may take a few seconds for the model to run and send back the results.

## GraphicsOverlay

* [Add graphics (SimpleRenderer)](ArcGISRuntime.WPF.Viewer/Samples/GraphicsOverlay/AddGraphicsRenderer)

This sample demonstrates how you add graphics and set a renderer on a graphic overlays.

* [Add graphics with symbols](ArcGISRuntime.WPF.Viewer/Samples/GraphicsOverlay/AddGraphicsWithSymbols)

This sample demonstrates how to add various types of graphics to a `GraphicsOverlay`.

* [Animate 3D graphic](ArcGISRuntime.WPF.Viewer/Samples/GraphicsOverlay/Animate3DGraphic)

This sample demonstrates how to animate a graphic's position and follow it using a camera controller.

* [Identify graphics](ArcGISRuntime.WPF.Viewer/Samples/GraphicsOverlay/IdentifyGraphics)

This sample demonstrates how to identify graphics in a graphics overlay. When you tap on a graphic on the map, you will see an alert message displayed.

* [Sketch graphics on the map](ArcGISRuntime.WPF.Viewer/Samples/GraphicsOverlay/SketchOnMap)

This sample demonstrates how to interactively sketch and edit graphics in the map view and display them in a graphics overlay. You can sketch a variety of geometry types and undo or redo operations.

* [Surface placement](ArcGISRuntime.WPF.Viewer/Samples/GraphicsOverlay/SurfacePlacements)

This sample demonstrates how to position graphics using different Surface Placements.

## Hydrography

* [Add ENC Exchange Set](ArcGISRuntime.WPF.Viewer/Samples/Hydrography/AddEncExchangeSet)

This sample demonstrates how to load an ENC Exchange Set, including its component cells.

* [Change ENC display settings](ArcGISRuntime.WPF.Viewer/Samples/Hydrography/ChangeEncDisplaySettings)

This sample demonstrates how to control ENC environment settings. These settings apply to the display of all ENC content in your app.

* [Select ENC features](ArcGISRuntime.WPF.Viewer/Samples/Hydrography/SelectEncFeatures)

This sample demonstrates how to select an ENC feature.

## Layers

* [ArcGIS map image layer (URL)](ArcGISRuntime.WPF.Viewer/Samples/Layers/ArcGISMapImageLayerUrl)

This sample demonstrates how to add an ArcGISMapImageLayer as a base layer in a map. The ArcGISMapImageLayer comes from an ArcGIS Server sample web service.

* [ArcGIS raster function (service)](ArcGISRuntime.WPF.Viewer/Samples/Layers/RasterLayerRasterFunction)

This sample demonstrates how to show a raster layer on a map based on an image service layer that has a raster function applied.

* [ArcGIS raster layer (service)](ArcGISRuntime.WPF.Viewer/Samples/Layers/RasterLayerImageServiceRaster)

This sample demonstrates how to show a raster layer on a map based on an image service layer.

* [ArcGIS scene layer (URL)](ArcGISRuntime.WPF.Viewer/Samples/Layers/SceneLayerUrl)

This sample demonstrates how to add an ArcGISSceneLayer as a layer in a Scene.

* [ArcGIS tiled layer (URL)](ArcGISRuntime.WPF.Viewer/Samples/Layers/ArcGISTiledLayerUrl)

This sample demonstrates how to add an ArcGISTiledLayer as a base layer in a map. The ArcGISTiledLayer comes from an ArcGIS Server sample web service.

* [ArcGIS vector tiled layer (URL)](ArcGISRuntime.WPF.Viewer/Samples/Layers/ArcGISVectorTiledLayerUrl)

This sample demonstrates how to create a ArcGISVectorTiledLayer and bind this to a Basemap which is used in the creation of a map.

* [Blend renderer](ArcGISRuntime.WPF.Viewer/Samples/Layers/ChangeBlendRenderer)

This sample demonstrates how to use blend renderer on a raster layer. You can get a hillshade blended with either a colored raster or color ramp.

* [Change feature layer renderer](ArcGISRuntime.WPF.Viewer/Samples/Layers/ChangeFeatureLayerRenderer)

This sample demonstrates how to change renderer for a feature layer. It also shows how to reset the renderer back to the default.

* [Change sublayer renderer](ArcGISRuntime.WPF.Viewer/Samples/Layers/ChangeSublayerRenderer)

This sample demonstrates how to change the sub-layer renderer of an ArcGIS map image layer. A unique value renderer is applied to see different population ranges in the counties sub-layer data.

* [Change sublayer visibility](ArcGISRuntime.WPF.Viewer/Samples/Layers/ChangeSublayerVisibility)

This sample demonstrates how to show or hide sublayers of a map image layer.

* [Create a feature collection layer from a portal item](ArcGISRuntime.WPF.Viewer/Samples/Layers/FeatureCollectionLayerFromPortal)

This sample demonstrates opening a feature collection saved as a portal item.

* [Create a new feature collection layer](ArcGISRuntime.WPF.Viewer/Samples/Layers/CreateFeatureCollectionLayer)

This sample demonstrates how to create a new feature collection with several feature collection tables. The collection is displayed in the map as a feature collection layer.

* [Display scene](ArcGISRuntime.WPF.Viewer/Samples/Layers/DisplayScene)

Demonstrates how to display a scene with an elevation data source. An elevation data source allows objects to be viewed in 3D, like this picture of Mt. Everest.

* [Export tiles](ArcGISRuntime.WPF.Viewer/Samples/Layers/ExportTiles)

This sample demonstrates how to export tiles from a map server.

* [Feature collection layer from query result](ArcGISRuntime.WPF.Viewer/Samples/Layers/FeatureCollectionLayerFromQuery)

This sample demonstrates how to create a feature collection layer to show a query result from a service feature table.

* [Feature layer (feature service)](ArcGISRuntime.WPF.Viewer/Samples/Layers/FeatureLayerUrl)

This sample demonstrates how to show a feature layer on a map using the URL to the service.

* [Feature layer definition expression](ArcGISRuntime.WPF.Viewer/Samples/Layers/FeatureLayerDefinitionExpression)

This sample demonstrates how to apply definition expression to a feature layer for filtering features. It also shows how to reset the definition expression.

* [Feature layer dictionary renderer](ArcGISRuntime.WPF.Viewer/Samples/Layers/FeatureLayerDictionaryRenderer)

Demonstrates how to apply a dictionary renderer to a feature layer and display mil2525d graphics. The dictionary renderer creates these graphics using a mil2525d style file and the attributes attached to each feature within the geodatabase.

* [Feature layer rendering mode (Map)](ArcGISRuntime.WPF.Viewer/Samples/Layers/FeatureLayerRenderingModeMap)

This sample demonstrates how to use load settings to set preferred rendering mode for feature layers, specifically static or dynamic rendering modes.

* [Feature layer rendering mode (Scene)](ArcGISRuntime.WPF.Viewer/Samples/Layers/FeatureLayerRenderingModeScene)

This sample demonstrates how to use load settings to change the preferred rendering mode for a scene. Static rendering mode only redraws features periodically when a sceneview is navigating, while dynamic mode dynamically re-renders as the scene moves.

* [Feature layer selection](ArcGISRuntime.WPF.Viewer/Samples/Layers/FeatureLayerSelection)

This sample demonstrates how to select features in a feature layer by tapping a MapView.

* [Identify WMS features](ArcGISRuntime.WPF.Viewer/Samples/Layers/WmsIdentify)

This sample demonstrates how to identify WMS features and display the associated content for an identified WMS feature.

* [Query a map image sublayer](ArcGISRuntime.WPF.Viewer/Samples/Layers/MapImageSublayerQuery)

This sample demonstrates how to execute an attribute and spatial query on the sublayers of an ArcGIS map image layer. Sublayers of an ArcGISMapImageLayer may expose a ServiceFeatureTable through a Table property.This allows you to perform the same queries available when working with a table from a FeatureLayer: attribute query, spatial query, statistics query, query for related features, and so on.

* [Query map image layer tables](ArcGISRuntime.WPF.Viewer/Samples/Layers/MapImageLayerTables)

This sample demonstrates how to get a non-spatial table from an ArcGIS map image layer. It shows how to query such a table, as well as how to find related features in another table. The non-spatial tables contained by a map service may contain additional information about sublayer features. Such information can be accessed by traversing table relationships defined in the service.

* [Raster hillshade renderer](ArcGISRuntime.WPF.Viewer/Samples/Layers/RasterHillshade)

This sample demonstrates how to use a hillshade renderer on a raster layer. Hillshade renderers can adjust a grayscale raster (usually of terrain) according to a hypothetical sun position (azimuth and altitude).

* [Raster layer (file)](ArcGISRuntime.WPF.Viewer/Samples/Layers/RasterLayerFile)

This sample demonstrates how to use a raster layer created from a local raster file.

* [Raster rendering rule](ArcGISRuntime.WPF.Viewer/Samples/Layers/RasterRenderingRule)

This sample demonstrates how to create an `ImageServiceRaster`, fetch the `RenderingRule`s from the service info, and use a `RenderingRule` to create an `ImageServiceRaster` and add it to a raster layer.

* [Raster RGB renderer](ArcGISRuntime.WPF.Viewer/Samples/Layers/RasterRgbRenderer)

This sample demonstrates how to use an RGB renderer on a raster layer. An RGB renderer is used to adjust the color bands of a multi-spectral image.

* [Scene layer selection](ArcGISRuntime.WPF.Viewer/Samples/Layers/SceneLayerSelection)

This sample demonstrates how to identify geoelements in a scene layer.

* [Show labels on layer](ArcGISRuntime.WPF.Viewer/Samples/Layers/ShowLabelsOnLayer)

This sample demonstrates how to show labels on a feature layer

* [Stretch renderer](ArcGISRuntime.WPF.Viewer/Samples/Layers/ChangeStretchRenderer)

This sample demonstrates how to use stretch renderer on a raster layer.

* [Style WMS layers](ArcGISRuntime.WPF.Viewer/Samples/Layers/StyleWmsLayer)

This sample demonstrates how to select from the available styles on WMS sublayers. 

* [Time-based query](ArcGISRuntime.WPF.Viewer/Samples/Layers/TimeBasedQuery)

This sample demonstrates how to apply a time-based parameter to a feature layer query.

* [Web tiled layer](ArcGISRuntime.WPF.Viewer/Samples/Layers/LoadWebTiledLayer)

This sample demonstrates how to load a web tiled layer from a non-ArcGIS service, including how to include proper attribution.

* [WMS layer (URL)](ArcGISRuntime.WPF.Viewer/Samples/Layers/WMSLayerUrl)

This sample demonstrates how to add a layer from a WMS service to a map.

* [WMS service catalog](ArcGISRuntime.WPF.Viewer/Samples/Layers/WmsServiceCatalog)

This sample demonstrates how to enable and disable the display of layers discovered from a WMS service.

* [WMTS layer](ArcGISRuntime.WPF.Viewer/Samples/Layers/WMTSLayer)

This sample demonstrates how to display a WMTS layer on a map via a Uri and WmtsLayerInfo.

## Local Server

* [Dynamic workspace raster](ArcGISRuntime.WPF.Viewer/Samples/Local%20Server/DynamicWorkspaceRaster)

This sample demonstrates how to dynamically add a local Raster to a map using Local Server.

* [Dynamic workspace shapefile](ArcGISRuntime.WPF.Viewer/Samples/Local%20Server/DynamicWorkspaceShapefile)

This sample demonstrates how to dynamically add a local shapefile to a map using Local Server.

* [Local Server feature layer](ArcGISRuntime.WPF.Viewer/Samples/Local%20Server/LocalServerFeatureLayer)

This sample demonstrates how to display a Feature Layer service by a Local Server feature service.

* [Local Server geoprocessing](ArcGISRuntime.WPF.Viewer/Samples/Local%20Server/LocalServerGeoprocessing)

This sample demonstrates how to perform geoprocessing tasks using Local Server.

* [Local Server map image layer](ArcGISRuntime.WPF.Viewer/Samples/Local%20Server/LocalServerMapImageLayer)

This sample demonstrates how to display a Map Image Layer from a local map service

* [Local Server services](ArcGISRuntime.WPF.Viewer/Samples/Local%20Server/LocalServerServices)

This sample demonstrates how to control local server and manage running services.

## Location

* [Display device location](ArcGISRuntime.WPF.Viewer/Samples/Location/DisplayDeviceLocation)

This sample demonstrates how you can enable location services and switch between different types of auto pan modes.

## Map

* [Access load status](ArcGISRuntime.WPF.Viewer/Samples/Map/AccessLoadStatus)

This sample demonstrates how to access the Maps' LoadStatus. The LoadStatus will be considered loaded when the following are true: The Map has a valid SpatialReference and the Map has an been set to the MapView.

* [Author a map](ArcGISRuntime.WPF.Viewer/Samples/Map/AuthorMap)

This sample demonstrates how to author and save a map as an ArcGIS portal item (web map). Saving a map to arcgis.com requires an ArcGIS Online login.

* [Change basemap](ArcGISRuntime.WPF.Viewer/Samples/Map/ChangeBasemap)

This sample demonstrates how to dynamically change the basemap displayed in a Map.

* [Display a map](ArcGISRuntime.WPF.Viewer/Samples/Map/DisplayMap)

This sample demonstrates how to display a map with a basemap.

* [Download preplanned map areas](ArcGISRuntime.WPF.Viewer/Samples/Map/DownloadPreplannedMapAreas)

This sample demonstrates how to download preplanned map areas from a webmap. In the preplanned offline workflow, the author of the online map defines map areas for offline use. When these areas are created, their offline packages are created and stored online for clients to download. This is the biggest difference between on-demand and preplanned workflows since on-demand workflow data packages are generated at request time.

* [Manage bookmarks](ArcGISRuntime.WPF.Viewer/Samples/Map/ManageBookmarks)

This sample demonstrates how to access and add bookmarks to a map.

* [Open map (URL)](ArcGISRuntime.WPF.Viewer/Samples/Map/OpenMapURL)

This sample demonstrates loading a webmap in a map from a Uri.

* [Open mobile map (map package)](ArcGISRuntime.WPF.Viewer/Samples/Map/OpenMobileMap)

This sample demonstrates how to open a mobile map from a map package.

* [Open scene (Portal item)](ArcGISRuntime.WPF.Viewer/Samples/Map/OpenScene)

This sample demonstrates how to open a scene from a Portal item. Just like Web Maps are the ArcGIS format for maps, Web Scenes are the ArcGIS format for scenes. These scenes can be stored in ArcGIS Online or Portal.

* [Search a portal for maps](ArcGISRuntime.WPF.Viewer/Samples/Map/SearchPortalMaps)

This sample demonstrates searching a portal for web maps and loading them in the map view. You can search ArcGIS Online public web maps using tag values or browse the web maps in your account. OAuth is used to authenticate with ArcGIS Online to access items in your account.

* [Set initial map area](ArcGISRuntime.WPF.Viewer/Samples/Map/SetInitialMapArea)

This sample demonstrates how to set the initial viewpoint from envelope defined by minimum (x,y) and maximum (x,y) values. The map's InitialViewpoint is set to this viewpoint before the map is loaded into the MapView. Upon loading the map zoom to this initial area.

* [Set initial map location](ArcGISRuntime.WPF.Viewer/Samples/Map/SetInitialMapLocation)

This sample creates a map with a standard ESRI Imagery with Labels basemap that is centered on a latitude and longitude location and zoomed into a specific level of detail.

* [Set map spatial reference](ArcGISRuntime.WPF.Viewer/Samples/Map/SetMapSpatialReference)

This sample demonstrates how you can set the spatial reference on a Map and all the layers would project accordingly.

* [Set min & max scale](ArcGISRuntime.WPF.Viewer/Samples/Map/SetMinMaxScale)

This sample demonstrates how to set the minimum and maximum scale of a Map. Setting the minimum and maximum scale for the Map can be useful in keeping the user focused at a certain level of detail.

## MapView

* [Change time extent](ArcGISRuntime.WPF.Viewer/Samples/MapView/ChangeTimeExtent)

This sample demonstrates how to filter data in layers by applying a time extent to a MapView.

* [Change viewpoint](ArcGISRuntime.WPF.Viewer/Samples/MapView/ChangeViewpoint)

This sample demonstrates different ways in which you can change the viewpoint of the MapView.

* [Display a grid](ArcGISRuntime.WPF.Viewer/Samples/MapView/DisplayGrid)

This sample demonstrates how to display and work with coordinate grid systems such as Latitude/Longitude, MGRS, UTM and USNG on a map view. This includes toggling labels visibility, changing the color of the grid lines, and changing the color of the grid labels.

* [Display drawing status](ArcGISRuntime.WPF.Viewer/Samples/MapView/DisplayDrawingStatus)

This sample demonstrates how to use the DrawStatus value of the MapView to notify user that the MapView is drawing.

* [Display layer view state](ArcGISRuntime.WPF.Viewer/Samples/MapView/DisplayLayerViewState)

This sample demonstrates how to get view status for layers in a map.

* [Feature layer time offset](ArcGISRuntime.WPF.Viewer/Samples/MapView/FeatureLayerTimeOffset)

This sample demonstrates how to show data from the same service side-by-side with a time offset. This allows for the comparison of data over time.

* [GeoView viewpoint synchronization](ArcGISRuntime.WPF.Viewer/Samples/MapView/GeoViewSync)

This sample demonstrates how to keep two geo views (MapView/SceneView) in sync with each other.

* [Map rotation](ArcGISRuntime.WPF.Viewer/Samples/MapView/MapRotation)

This sample demonstrates how to rotate a map.

* [Show callout](ArcGISRuntime.WPF.Viewer/Samples/MapView/ShowCallout)

This sample illustrates how to show callouts on a map in response to user interaction.

* [Show magnifier](ArcGISRuntime.WPF.Viewer/Samples/MapView/ShowMagnifier)

This sample demonstrates how you can tap and hold on a map to get the magnifier. You can also pan while tapping and holding to move the magnifier across the map.

* [Take screenshot](ArcGISRuntime.WPF.Viewer/Samples/MapView/TakeScreenshot)

This sample demonstrates how you can take screenshot of a map. Click 'take screenshot' button to take a screenshot of the visible area of the map. Created image is shown in the sample after creation.

## Network Analysis

* [Closest facility (interactive)](ArcGISRuntime.WPF.Viewer/Samples/Network%20Analysis/ClosestFacility)

Demonstrates how to solve a Closest Facility Task to find the closest route between a facility (hospital) and a incident (black cross).

* [Closest facility (static)](ArcGISRuntime.WPF.Viewer/Samples/Network%20Analysis/ClosestFacilityStatic)

Demonstrates how to solve a Closest Facility Task to find the closest route between facilities and incidents.

* [Find a route](ArcGISRuntime.WPF.Viewer/Samples/Network%20Analysis/FindRoute)

This sample demonstrates how to solve for the best route between two locations on the map and display driving directions between them.

* [Find service area (interactive)](ArcGISRuntime.WPF.Viewer/Samples/Network%20Analysis/FindServiceArea)

Demonstrates how to find services areas around a point using the ServiceAreaTask. A service area shows locations that can be reached from a facility based off a certain impedance [travel time in this case]. Service areas for a two and five minute travel time are used. Barriers can also be added which can effect the service area by not letting traffic through and adding to the time to get to locations.

## Search

* [Find address](ArcGISRuntime.WPF.Viewer/Samples/Search/FindAddress)

This sample demonstrates how you can use the LocatorTask API to geocode an address and display it with a pin on the map. Tapping the pin displays the reverse-geocoded address in a callout.

* [Find place](ArcGISRuntime.WPF.Viewer/Samples/Search/FindPlace)

This sample demonstrates how to use geocode functionality to search for points of interest, around a location or within an extent.

## Security

* [ArcGIS token challenge](ArcGISRuntime.WPF.Viewer/Samples/Security/TokenSecuredChallenge)

This sample demonstrates how to authenticate with ArcGIS Server using ArcGIS Tokens to access a secure service. Accessing secured services requires a login that's been defined on the server.

* [ArcGIS token with a known user](ArcGISRuntime.WPF.Viewer/Samples/Security/TokenSecuredKnownUser)

This sample demonstrates how to authenticate with ArcGIS Server using ArcGIS Tokens to access a secure service. Accessing secured services requires a login that's been defined on the server.

* [Authenticate with OAuth](ArcGISRuntime.WPF.Viewer/Samples/Security/OAuth)

This sample demonstrates how to authenticate with ArcGIS Online (or your own portal) using OAuth2 to access a secure web map (or the secured layers it contains). Accessing secured items requires a login on the portal (an ArcGIS Online account, for example).

## Symbology

* [Distance composite symbol](ArcGISRuntime.WPF.Viewer/Samples/Symbology/UseDistanceCompositeSym)

This sample demonstrates how to create a `DistanceCompositeSceneSymbol` with unique marker symbols to display at various distances from the camera.

* [Feature layer extrusion](ArcGISRuntime.WPF.Viewer/Samples/Symbology/FeatureLayerExtrusion)

This sample demonstrates how to apply extrusion to a renderer on a feature layer.

* [Render picture markers](ArcGISRuntime.WPF.Viewer/Samples/Symbology/RenderPictureMarkers)

This sample demonstrates how to create picture marker symbols from a URL and embedded resources.

* [Render simple markers](ArcGISRuntime.WPF.Viewer/Samples/Symbology/RenderSimpleMarkers)

This sample adds a point graphic to a graphics overlay symbolized with a red circle specified via a SimpleMarkerSymbol.

* [Render unique values](ArcGISRuntime.WPF.Viewer/Samples/Symbology/RenderUniqueValues)

This sample demonstrate how to use a unique value renderer to style different features in a feature layer with different symbols. Features do not have a symbol property for you to set, renderers should be used to define the symbol for features in feature layers. The unique value renderer allows for separate symbols to be used for features that have specific attribute values in a defined field.

* [Simple renderer](ArcGISRuntime.WPF.Viewer/Samples/Symbology/SimpleRenderers)

This sample demonstrates how to create a simple renderer and add it to a graphics overlay. Renderers define the symbology for all graphics in a graphics overlay (unless they are overridden by setting the symbol directly on the graphic). Simple renderers can also be defined on feature layers using the same code.

