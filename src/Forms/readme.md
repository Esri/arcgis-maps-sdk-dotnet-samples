# Table of contents

## Analysis

* [Distance measurement analysis](Shared/Samples/Analysis/DistanceMeasurement/readme.md) - Measure distances between two points in 3D.
* [Line of sight (geoelement)](Shared/Samples/Analysis/LineOfSightGeoElement/readme.md) - Show a line of sight between two moving objects.
* [Line of sight (location)](Shared/Samples/Analysis/LineOfSightLocation/readme.md) - Perform a line of sight analysis between two points in real time.
* [Query feature count and extent](Shared/Samples/Analysis/QueryFeatureCountAndExtent/readme.md) - Zoom to features matching a query and count the features in the current visible extent.
* [Viewshed for camera](Shared/Samples/Analysis/ViewshedCamera/readme.md) - Analyze the viewshed for a camera. A viewshed shows the visible and obstructed areas from an observer's vantage point. 
* [Viewshed for GeoElement](Shared/Samples/Analysis/ViewshedGeoElement/readme.md) - Analyze the viewshed for an object (GeoElement) in a scene.
* [Viewshed for location](Shared/Samples/Analysis/ViewshedLocation/readme.md) - Perform a viewshed analysis from a defined vantage point. Viewshed analyses have several configuration options that are demonstrated in this sample.

## Data

* [Add features](Shared/Samples/Data/AddFeatures/readme.md) - Add features to a feature layer.
* [Delete features (feature service)](Shared/Samples/Data/DeleteFeatures/readme.md) - Delete features from an online feature service.
* [Edit and sync features](Shared/Samples/Data/EditAndSyncFeatures/readme.md) - Synchronize offline edits with a feature service.
* [Edit feature attachments](Shared/Samples/Data/EditFeatureAttachments/readme.md) - Add, delete, and download attachments for features from a service.
* [Feature layer (geodatabase)](Shared/Samples/Data/FeatureLayerGeodatabase/readme.md) - Display features from a local geodatabase.
* [Feature layer (GeoPackage)](Shared/Samples/Data/FeatureLayerGeoPackage/readme.md) - Display features from a local GeoPackage.
* [Feature layer query](Shared/Samples/Data/FeatureLayerQuery/readme.md) - Find features in a feature table which match an SQL query.
* [Feature layer (shapefile)](Shared/Samples/Data/FeatureLayerShapefile/readme.md) - Open a shapefile stored on the device and display it as a feature layer with default symbology.
* [Generate geodatabase](Shared/Samples/Data/GenerateGeodatabase/readme.md) - Generate a local geodatabase from an online feature service.
* [Geodatabase transactions](Shared/Samples/Data/GeodatabaseTransactions/readme.md) - This sample demonstrates how to manage edits to a local geodatabase inside of transactions.
* [List related features](Shared/Samples/Data/ListRelatedFeatures/readme.md) - List features related to the selected feature.
* [Raster layer (GeoPackage)](Shared/Samples/Data/RasterLayerGeoPackage/readme.md) - Open a `GeoPackage`, obtain a raster from the package, and display the table as a `RasterLayer`.
* [Read a GeoPackage](Shared/Samples/Data/ReadGeoPackage/readme.md) - Add rasters and feature tables from GeoPackages to a map.
* [Read shapefile metadata](Shared/Samples/Data/ReadShapefileMetadata/readme.md) - Read a shapefile and display its metadata.
* [Service feature table (on interaction cache)](Shared/Samples/Data/ServiceFeatureTableCache/readme.md) - Display a feature layer from a service using the **on interaction cache** feature request mode.
* [Service feature table (manual cache)](Shared/Samples/Data/ServiceFeatureTableManualCache/readme.md) - Display a feature layer from a service using the **manual cache** feature request mode.
* [Service feature table (no cache)](Shared/Samples/Data/ServiceFeatureTableNoCache/readme.md) - Display a feature layer from a service using the **no cache** feature request mode.
* [Statistical query](Shared/Samples/Data/StatisticalQuery/readme.md) - Query a table to get aggregated statistics back for a specific field.
* [Statistical query group and sort](Shared/Samples/Data/StatsQueryGroupAndSort/readme.md) - Query a feature table for statistics, grouping and sorting by different fields.
* [Symbolize a shapefile](Shared/Samples/Data/SymbolizeShapefile/readme.md) - Display a shapefile with custom symbology.
* [Update attributes (feature service)](Shared/Samples/Data/UpdateAttributes/readme.md) - Update feature attributes in an online feature service.
* [Update geometries (feature service)](Shared/Samples/Data/UpdateGeometries/readme.md) - Update a feature's location in an online feature service.
* [View point cloud data offline](Shared/Samples/Data/ViewPointCloudDataOffline/readme.md) - Display local 3D point cloud data.

## Geometry

* [Buffer](Shared/Samples/Geometry/Buffer/readme.md) - Create a buffer around a map point and display the results as a `Graphic`
* [Buffer list](Shared/Samples/Geometry/BufferList/readme.md) - Generate multiple individual buffers or a single unioned buffer around multiple points.
* [Clip geometry](Shared/Samples/Geometry/ClipGeometry/readme.md) - Clip a geometry with another geometry.
* [Convex hull](Shared/Samples/Geometry/ConvexHull/readme.md) - Create a convex hull for a given set of points. The convex hull is a polygon with shortest perimeter that encloses a set of points. As a visual analogy, consider a set of points as nails in a board. The convex hull of the points would be like a rubber band stretched around the outermost nails.
* [Convex hull list](Shared/Samples/Geometry/ConvexHullList/readme.md) - This sample demonstrates how to use the GeometryEngine.ConvexHull to generate convex hull polygon(s) from multiple input geometries.
* [Create geometries](Shared/Samples/Geometry/CreateGeometries/readme.md) - Create simple geometry types.
* [Cut geometry](Shared/Samples/Geometry/CutGeometry/readme.md) - Cut a geometry along a polyline.
* [Densify and generalize](Shared/Samples/Geometry/DensifyAndGeneralize/readme.md) - A multipart geometry can be densified by adding interpolated points at regular intervals. Generalizing multipart geometry simplifies it while preserving its general shape. Densifying a multipart geometry adds more vertices at regular intervals.
* [Format coordinates](Shared/Samples/Geometry/FormatCoordinates/readme.md) - Format coordinates in a variety of common notations.
* [Geodesic operations](Shared/Samples/Geometry/GeodesicOperations/readme.md) - This sample demonstrates how to perform geodesic operations on geometries using the GeometryEngine. Geodesic calculations take into account the curvature of the Earth, while planar calculations are based on a 2D Cartesian plane.
* [List transformations by suitability](Shared/Samples/Geometry/ListTransformations/readme.md) - Get a list of suitable transformations for projecting a geometry between two spatial references with different horizontal datums.
* [Nearest vertex](Shared/Samples/Geometry/NearestVertex/readme.md) - Shows how to find the nearest vertex on a geometry to a given point.
* [Project](Shared/Samples/Geometry/Project/readme.md) - Project a point from one spatial reference to another.
* [Project with specific transformation](Shared/Samples/Geometry/ProjectWithSpecificTransformation/readme.md) - This sample demonstrates how to use the GeometryEngine with a specified geographic transformation to transform a geometry from one coordinate system to another. 
* [Perform spatial operations](Shared/Samples/Geometry/SpatialOperations/readme.md) - Find the union, intersection, or difference of two geometries.
* [Spatial relationships](Shared/Samples/Geometry/SpatialRelationships/readme.md) - Determine spatial relationships between two geometries.

## Geoprocessing

* [Analyze hotspots](Shared/Samples/Geoprocessing/AnalyzeHotspots/readme.md) - Use a geoprocessing service and a set of features to identify statistically significant hot spots and cold spots.
* [Analyze viewshed (geoprocessing)](Shared/Samples/Geoprocessing/AnalyzeViewshed/readme.md) - Calculate a viewshed using a geoprocessing service, in this case showing what parts of a landscape are visible from points on mountainous terrain.
* [List geodatabase versions](Shared/Samples/Geoprocessing/ListGeodatabaseVersions/readme.md) - This sample calls a custom GeoprocessingTask to get a list of available versions for an enterprise geodatabase. The task returns a table of geodatabase version information, which is displayed in the app as a list.

## GraphicsOverlay

* [Add graphics with renderer](Shared/Samples/GraphicsOverlay/AddGraphicsRenderer/readme.md) - Change the style of all graphics in a graphics overlay by referencing a single symbol style.
* [Add graphics with symbols](Shared/Samples/GraphicsOverlay/AddGraphicsWithSymbols/readme.md) - Use a symbol style to display a graphic on a graphics overlay.
* [Animate 3D graphic](Shared/Samples/GraphicsOverlay/Animate3DGraphic/readme.md) - An `OrbitGeoElementCameraController` follows a graphic while the graphic's position and rotation are animated.
* [Dictionary renderer with graphics overlay](Shared/Samples/GraphicsOverlay/DictionaryRendererGraphicsOverlay/readme.md) - Render graphics with mil2525d symbols.
* [Identify graphics](Shared/Samples/GraphicsOverlay/IdentifyGraphics/readme.md) - Display an alert message when a graphic is clicked.
* [Scene properties expressions](Shared/Samples/GraphicsOverlay/ScenePropertiesExpressions/readme.md) - Update the orientation of a graphic using scene property rotation expressions.
* [Sketch on map](Shared/Samples/GraphicsOverlay/SketchOnMap/readme.md) - Use the Sketch Editor to edit or sketch a new point, line, or polygon geometry on to a map.
* [Surface placement](Shared/Samples/GraphicsOverlay/SurfacePlacements/readme.md) - Position graphics relative to a surface using different surface placement modes.

## Hydrography

* [Add ENC exchange set](Shared/Samples/Hydrography/AddEncExchangeSet/readme.md) - Display nautical charts per the ENC specification.
* [Change ENC display settings](Shared/Samples/Hydrography/ChangeEncDisplaySettings/readme.md) - Configure the display of ENC content.
* [Select ENC features](Shared/Samples/Hydrography/SelectEncFeatures/readme.md) - Select features in an ENC layer.

## Layers

* [Add an integrated mesh layer](Shared/Samples/Layers/AddAnIntegratedMeshLayer/readme.md) - View an integrated mesh layer from a scene service.
* [Add a point scene layer](Shared/Samples/Layers/AddPointSceneLayer/readme.md) - View a point scene layer from a scene service.
* [ArcGIS map image layer](Shared/Samples/Layers/ArcGISMapImageLayerUrl/readme.md) - Add an ArcGIS Map Image Layer from a URL to a map.
* [ArcGIS tiled layer](Shared/Samples/Layers/ArcGISTiledLayerUrl/readme.md) - Load an ArcGIS tiled layer from a URL.
* [ArcGIS vector tiled layer URL](Shared/Samples/Layers/ArcGISVectorTiledLayerUrl/readme.md) - Load an ArcGIS Vector Tiled Layer from a URL.
* [Browse WFS layers](Shared/Samples/Layers/BrowseWfsLayers/readme.md) - Browse a WFS service for layers.
* [Blend renderer](Shared/Samples/Layers/ChangeBlendRenderer/readme.md) - Blend a hillshade with a raster by specifying the elevation data. The resulting raster looks similar to the original raster, but with some terrain shading, giving it a textured look.
* [Change feature layer renderer](Shared/Samples/Layers/ChangeFeatureLayerRenderer/readme.md) - Change the appearance of a feature layer with a renderer.
* [Stretch renderer](Shared/Samples/Layers/ChangeStretchRenderer/readme.md) - Use a stretch renderer to enhance the visual contrast of raster data for analysis.
* [Change sublayer renderer](Shared/Samples/Layers/ChangeSublayerRenderer/readme.md) - Apply a renderer to a sublayer.
* [Map image layer sublayer visibility](Shared/Samples/Layers/ChangeSublayerVisibility/readme.md) - Change the visibility of sublayers.
* [Feature collection layer](Shared/Samples/Layers/CreateFeatureCollectionLayer/readme.md) - Create a Feature Collection Layer from a Feature Collection Table, and add it to a map.
* [Display KML](Shared/Samples/Layers/DisplayKml/readme.md) - Load and display KML from a URL, a local file, or a portal item.
* [Display KML network links](Shared/Samples/Layers/DisplayKmlNetworkLinks/readme.md) -  Display a file with a KML network link.
* [Display a scene](Shared/Samples/Layers/DisplayScene/readme.md) - Display a scene with a terrain surface and some imagery.
* [Display a WFS layer](Shared/Samples/Layers/DisplayWfs/readme.md) - Display a layer from a WFS service, requesting only features for the current extent.
* [Export tiles](Shared/Samples/Layers/ExportTiles/readme.md) - Download tiles to a local tile cache file stored on the device.
* [Create a feature collection layer from a portal item](Shared/Samples/Layers/FeatureCollectionLayerFromPortal/readme.md) - This sample demonstrates opening a feature collection saved as a portal item.
* [Feature collection layer (Query)](Shared/Samples/Layers/FeatureCollectionLayerFromQuery/readme.md) - Create a feature collection layer to show a query result from a service feature table. The feature collection is then displayed on a map with a feature collection layer.
* [Feature layer definition expression](Shared/Samples/Layers/FeatureLayerDefinitionExpression/readme.md) - Limit the features to display on the map using a definition expression.
* [Dictionary renderer with feature layer](Shared/Samples/Layers/FeatureLayerDictionaryRenderer/readme.md) - Convert features into graphics to show them with mil2525d symbols.
* [Feature layer rendering mode (map)](Shared/Samples/Layers/FeatureLayerRenderingModeMap/readme.md) - Render features statically or dynamically by setting the feature layer rendering mode.
* [Feature layer rendering mode (scene)](Shared/Samples/Layers/FeatureLayerRenderingModeScene/readme.md) - Render features in a scene statically or dynamically by setting the feature layer rendering mode.
* [Feature layer selection](Shared/Samples/Layers/FeatureLayerSelection/readme.md) - Select features in a feature layer.
* [Feature layer (feature service)](Shared/Samples/Layers/FeatureLayerUrl/readme.md) - Show a feature layer on a map using the URL to the service.
* [Group layers](Shared/Samples/Layers/GroupLayers/readme.md) - Group a collection of layers together and toggle their visibility as a group.
* [Identify KML features](Shared/Samples/Layers/IdentifyKmlFeatures/readme.md) - This sample demonstrates how to identify features in a KML layer. Identified feature attributes are displayed in a callout to simulate a popup.
* [List KML contents](Shared/Samples/Layers/ListKmlContents/readme.md) - List the contents of a KML file. KML files can contain a hierarchy of features, including network links to other KML content.
* [Web tiled layer](Shared/Samples/Layers/LoadWebTiledLayer/readme.md) - Display a tiled web layer.
* [Map image layer tables](Shared/Samples/Layers/MapImageLayerTables/readme.md) - Find features in a spatial table related to features in a non-spatial table.
* [Query map image sublayer](Shared/Samples/Layers/MapImageSublayerQuery/readme.md) - Find features in a sublayer based on attributes and location.
* [OpenStreetMap layer](Shared/Samples/Layers/OpenStreetMapLayer/readme.md) - Add OpenStreetMap as a basemap layer.
* [Play a KML tour](Shared/Samples/Layers/PlayKmlTours/readme.md) - Play tours in KML files.
* [Raster hillshade renderer](Shared/Samples/Layers/RasterHillshade/readme.md) - Use a hillshade renderer on a raster.
* [Raster layer (file)](Shared/Samples/Layers/RasterLayerFile/readme.md) - Create and use a raster layer made from a local raster file.
* [Raster layer (service)](Shared/Samples/Layers/RasterLayerImageServiceRaster/readme.md) - Create a raster layer from a raster image service.
* [ArcGIS raster function (service)](Shared/Samples/Layers/RasterLayerRasterFunction/readme.md) - Show a raster layer from an image service with a raster function applied.
* [Raster rendering rule](Shared/Samples/Layers/RasterRenderingRule/readme.md) - Display a raster on a map and apply different rendering rules to that raster.
* [RGB Renderer](Shared/Samples/Layers/RasterRgbRenderer/readme.md) - Use an `RGBRenderer` on a `RasterLayer`. An `RGBRenderer` is used to adjust the color bands of a multispectral image.
* [Scene layer selection](Shared/Samples/Layers/SceneLayerSelection/readme.md) - Identify GeoElements in a scene layer.
* [Scene layer (URL)](Shared/Samples/Layers/SceneLayerUrl/readme.md) - Display an ArcGIS scene layer from a URL.
* [Show labels on layers](Shared/Samples/Layers/ShowLabelsOnLayer/readme.md) - Display custom labels on a feature layer.
* [Style WMS layers](Shared/Samples/Layers/StyleWmsLayer/readme.md) - Discover available styles and apply them to WMS sublayers.
* [Time-based query](Shared/Samples/Layers/TimeBasedQuery/readme.md) - This sample demonstrates how to query data using a time extent. This workflow can be used to return records that are between a specified start and end date. For example, you could specify to only show records that are before September 16, 2000.
* [Load WFS with XML query](Shared/Samples/Layers/WfsXmlQuery/readme.md) - Load a WFS feature table using an XML query.
* [Identify WMS features](Shared/Samples/Layers/WmsIdentify/readme.md) - Identify features in a WMS layer and display the associated popup content.
* [WMS Layer URL](Shared/Samples/Layers/WMSLayerUrl/readme.md) - Display a WMS layer using a WMS service URL.
* [WMS service catalog](Shared/Samples/Layers/WmsServiceCatalog/readme.md) - Connect to a WMS service and show the available layers and sublayers. Layers are shown in a hierarchy. Selecting a group layer will recursively select all sublayers for display.
* [WMTS layer](Shared/Samples/Layers/WMTSLayer/readme.md) - Display a layer from a Web Map Tile Service.

## Location

* [Display device location](Shared/Samples/Location/DisplayDeviceLocation/readme.md) - Display your current position on the map, as well as switch between different types of auto pan Modes.

## Map

* [Access load status](Shared/Samples/Map/AccessLoadStatus/readme.md) - Determine the map's load status which can be: `NotLoaded`, `FailedToLoad`, `Loading`, `Loaded`.
* [Create and save a map](Shared/Samples/Map/AuthorMap/readme.md) - Create and save a map as an ArcGIS `PortalItem` (i.e. web map).
* [Change atmosphere effect](Shared/Samples/Map/ChangeAtmosphereEffect/readme.md) - Changes the appearance of the atmosphere in a scene.
* [Change basemap](Shared/Samples/Map/ChangeBasemap/readme.md) - Change a map's basemap. A basemap is beneath all layers on an `Map` and is used to provide visual reference for the operational layers.
* [Create terrain surface from a local raster](Shared/Samples/Map/CreateTerrainSurfaceFromRaster/readme.md) - Use a terrain surface with elevation described by a raster file.
* [Create terrain surface from a tile package](Shared/Samples/Map/CreateTerrainSurfaceFromTilePackage/readme.md) - Set the terrain surface with elevation described by a local tile package.
* [Display a map](Shared/Samples/Map/DisplayMap/readme.md) - Display a map with an imagery basemap.
* [Download a preplanned map area](Shared/Samples/Map/DownloadPreplannedMap/readme.md) - Take a map offline using an available preplanned map area.
* [Generate offline map](Shared/Samples/Map/GenerateOfflineMap/readme.md) - Take a web map offline.
* [Generate Offline Map (Overrides)](Shared/Samples/Map/GenerateOfflineMapWithOverrides/readme.md) - Use the `OfflineMapTask` with overrides to take a webmap offline. The overrides workflow allows you to adjust the settings used for taking each layer in the map offline. For a simple example of how you take a map offline, please consult the "Generate Offline Map" sample.
* [Get elevation at a point](Shared/Samples/Map/GetElevationAtPoint/readme.md) - Get the elevation for a given point on a surface in a scene. 
* [Manage bookmarks](Shared/Samples/Map/ManageBookmarks/readme.md) - Access and create bookmarks on a map.
* [Manage operational layers](Shared/Samples/Map/ManageOperationalLayers/readme.md) - Add, remove, and reorder operational layers in a map.
* [Map reference scale](Shared/Samples/Map/MapReferenceScale/readme.md) - Set a map's reference scale and control which feature layers should honor that scale.
* [Mobile map (search and route)](Shared/Samples/Map/MobileMapSearchAndRoute/readme.md) - Display maps and use locators to enable search and routing offline using a Mobile Map Package.
* [Generate offline map with local basemap](Shared/Samples/Map/OfflineBasemapByReference/readme.md) - Use the `OfflineMapTask` to take a web map offline, but instead of downloading an online basemap, use one which is already on the device.
* [Open map URL](Shared/Samples/Map/OpenMapURL/readme.md) - Display a web map.
* [Open mobile map package](Shared/Samples/Map/OpenMobileMap/readme.md) - Display a map from a mobile map package.
* [Open Mobile Scene Package](Shared/Samples/Map/OpenMobileScenePackage/readme.md) - Open and display a scene from a Mobile Scene Package (.mspk).
* [Open a scene (Portal item)](Shared/Samples/Map/OpenScene/readme.md) - Open a scene from a Portal item. Just like Web Maps are the ArcGIS format for maps, Web Scenes are the ArcGIS format for scenes. These scenes can be stored in ArcGIS Online or Portal. 
* [Search for webmap](Shared/Samples/Map/SearchPortalMaps/readme.md) - Find webmap portal items by using a search term.
* [Map initial extent](Shared/Samples/Map/SetInitialMapArea/readme.md) - Display the map at an initial viewpoint representing a bounding geometry.
* [Set initial map location](Shared/Samples/Map/SetInitialMapLocation/readme.md) - Display a map centered on an initial point with a specified level of detail (zoom level).
* [Set map spatial reference](Shared/Samples/Map/SetMapSpatialReference/readme.md) - Specify a map's spatial reference.
* [Set min & max scale](Shared/Samples/Map/SetMinMaxScale/readme.md) - Restrict zooming between specific scale ranges.
* [Terrain exaggeration](Shared/Samples/Map/TerrainExaggeration/readme.md) - Configure the vertical exaggeration of terrain (the ground surface) in a scene.
* [View content beneath terrain surface](Shared/Samples/Map/ViewContentBeneathSurface/readme.md) - See through terrain in a scene and move the camera underground.

## MapView

* [Change time extent](Shared/Samples/MapView/ChangeTimeExtent/readme.md) - This sample demonstrates how to filter data in layers by applying a time extent to a MapView.
* [Change viewpoint](Shared/Samples/MapView/ChangeViewpoint/readme.md) - Set the map view to a new viewpoint.
* [Display draw status](Shared/Samples/MapView/DisplayDrawingStatus/readme.md) - Get the draw status of your map view or scene view to know when all layers in the map or scene have finished drawing.
* [Display grid](Shared/Samples/MapView/DisplayGrid/readme.md) - Display coordinate system grids including Latitude/Longitude, MGRS, UTM and USNG on a map view. Also, toggle label visibility and change the color of grid lines and grid labels.
* [Display layer view state](Shared/Samples/MapView/DisplayLayerViewState/readme.md) - View the status of the layers on the map.
* [Feature layer time offset](Shared/Samples/MapView/FeatureLayerTimeOffset/readme.md) - Show data from the same service side-by-side with a time offset. This allows for the comparison of data over time.
* [GeoView viewpoint synchronization](Shared/Samples/MapView/GeoViewSync/readme.md) - Keep the view points of two views (a MapView and a SceneView in this case) synchronized with each other.
* [Identify layers](Shared/Samples/MapView/IdentifyLayers/readme.md) - Identify features in all layers in a map. MapView supports identifying features across multiple layers. Because some layer types have sublayers, the sample recursively counts results for sublayers within each layer.
* [Map rotation](Shared/Samples/MapView/MapRotation/readme.md) - Rotate a map.
* [Show callout](Shared/Samples/MapView/ShowCallout/readme.md) - Show a callout with the latitude and longitude of user-tapped points.
* [Show magnifier](Shared/Samples/MapView/ShowMagnifier/readme.md) - Tap and hold on a map to show a magnifier.
* [Take a screenshot](Shared/Samples/MapView/TakeScreenshot/readme.md) - Take a screenshot of the map.

## Network Analysis

* [Find closest facility to an incident (interactive)](Shared/Samples/Network Analysis/ClosestFacility/readme.md) - Find a route to the closest facility from a location.
* [Find closest facility to multiple incidents (service)](Shared/Samples/Network Analysis/ClosestFacilityStatic/readme.md) - Find routes from several locations to the respective closest facility.
* [Find route](Shared/Samples/Network Analysis/FindRoute/readme.md) - Display directions for a route between two points.
* [Find service area](Shared/Samples/Network Analysis/FindServiceArea/readme.md) - Find the service area within a network from a given point.
* [Find service areas for multiple facilities](Shared/Samples/Network Analysis/FindServiceAreasForMultipleFacilities/readme.md) - Find the service areas of several facilities from a feature service.
* [Offline routing](Shared/Samples/Network Analysis/OfflineRouting/readme.md) - Solve a route on-the-fly using offline data.
* [Route around barriers](Shared/Samples/Network Analysis/RouteAroundBarriers/readme.md) - Find a route that reaches all stops without crossing any barriers.

## Search

* [Find address](Shared/Samples/Search/FindAddress/readme.md) - Find the location for an address.
* [Find place](Shared/Samples/Search/FindPlace/readme.md) - Find places of interest near a location or within a specific area.
* [Offline geocode](Shared/Samples/Search/OfflineGeocode/readme.md) - Geocode addresses to locations and reverse geocode locations to addresses offline.
* [Reverse geocode](Shared/Samples/Search/ReverseGeocode/readme.md) - Use an online service to find the address for a tapped point.

## Security

* [Integrated windows authentication](Shared/Samples/Security/IntegratedWindowsAuth/readme.md) - Use Windows credentials to access services hosted on a portal secured with Integrated Windows Authentication (IWA).
* [Authenticate with OAuth](Shared/Samples/Security/OAuth/readme.md) - This sample demonstrates how to authenticate with ArcGIS Online (or your own portal) using OAuth2 to access secured resources (such as private web maps or layers). Accessing secured items requires a login on the portal that hosts them (an ArcGIS Online account, for example).
* [ArcGIS token challenge](Shared/Samples/Security/TokenSecuredChallenge/readme.md) - This sample demonstrates how to prompt the user for a username and password to authenticate with ArcGIS Server to access an ArcGIS token-secured service. Accessing secured services requires a login that's been defined on the server.

## Symbology

* [Feature layer extrusion](Shared/Samples/Symbology/FeatureLayerExtrusion/readme.md) - Extrude features based on their attributes.
* [Render picture markers](Shared/Samples/Symbology/RenderPictureMarkers/readme.md) - Use pictures for markers.
* [Render simple markers](Shared/Samples/Symbology/RenderSimpleMarkers/readme.md) - Show a simple marker symbol on a map.
* [Unique value renderer](Shared/Samples/Symbology/RenderUniqueValues/readme.md) - Render features in a layer using a distinct symbol for each unique attribute value.
* [Scene symbols](Shared/Samples/Symbology/SceneSymbols/readme.md) - Show various kinds of 3D symbols in a scene.
* [Simple renderer](Shared/Samples/Symbology/SimpleRenderers/readme.md) - Display common symbols for all graphics in a graphics overlay with a renderer.
* [Read symbols from a mobile style](Shared/Samples/Symbology/SymbolsFromMobileStyle/readme.md) - Open a mobile style (.stylx) and read its contents. Combine several symbols from the style into a single multilayer point symbol, then use it to display graphics in the map view.
* [Distance composite scene symbol](Shared/Samples/Symbology/UseDistanceCompositeSym/readme.md) - Change a graphic's symbol based on the camera's proximity to it.

