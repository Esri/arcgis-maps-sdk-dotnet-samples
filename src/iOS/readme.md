# Table of contents

## Analysis

* [Distance measurement analysis](../..//Analysis/DistanceMeasurement) - Measure distances between two points in 3D.
* [Line of sight (geoelement)](../..//Analysis/LineOfSightGeoElement) - Show a line of sight between two moving objects.
* [Line of sight (location)](../..//Analysis/LineOfSightLocation) - Perform a line of sight analysis between two points in real time.
* [Query feature count and extent](../..//Analysis/QueryFeatureCountAndExtent) - Zoom to features matching a query and count the features in the current visible extent.
* [Viewshed for camera](../..//Analysis/ViewshedCamera) - Analyze the viewshed for a camera. A viewshed shows the visible and obstructed areas from an observer's vantage point. 
* [Viewshed for GeoElement](../..//Analysis/ViewshedGeoElement) - Analyze the viewshed for an object (GeoElement) in a scene.
* [Viewshed for location](../..//Analysis/ViewshedLocation) - Perform a viewshed analysis from a defined vantage point. Viewshed analyses have several configuration options that are demonstrated in this sample.

## Data

* [Add features](../..//Data/AddFeatures) - Add features to a feature layer.
* [Delete features (feature service)](../..//Data/DeleteFeatures) - Delete features from an online feature service.
* [Edit and sync features](../..//Data/EditAndSyncFeatures) - Synchronize offline edits with a feature service.
* [Edit feature attachments](../..//Data/EditFeatureAttachments) - Add, delete, and download attachments for features from a service.
* [Feature layer (geodatabase)](../..//Data/FeatureLayerGeodatabase) - Display features from a local geodatabase.
* [Feature layer (GeoPackage)](../..//Data/FeatureLayerGeoPackage) - Display features from a local GeoPackage.
* [Feature layer query](../..//Data/FeatureLayerQuery) - Find features in a feature table which match an SQL query.
* [Feature layer (shapefile)](../..//Data/FeatureLayerShapefile) - Open a shapefile stored on the device and display it as a feature layer with default symbology.
* [Generate geodatabase](../..//Data/GenerateGeodatabase) - Generate a local geodatabase from an online feature service.
* [Geodatabase transactions](../..//Data/GeodatabaseTransactions) - This sample demonstrates how to manage edits to a local geodatabase inside of transactions.
* [List related features](../..//Data/ListRelatedFeatures) - List features related to the selected feature.
* [Raster layer (GeoPackage)](../..//Data/RasterLayerGeoPackage) - Open a `GeoPackage`, obtain a raster from the package, and display the table as a `RasterLayer`.
* [Read a GeoPackage](../..//Data/ReadGeoPackage) - Add rasters and feature tables from GeoPackages to a map.
* [Read shapefile metadata](../..//Data/ReadShapefileMetadata) - Read a shapefile and display its metadata.
* [Service feature table (on interaction cache)](../..//Data/ServiceFeatureTableCache) - Display a feature layer from a service using the **on interaction cache** feature request mode.
* [Service feature table (manual cache)](../..//Data/ServiceFeatureTableManualCache) - Display a feature layer from a service using the **manual cache** feature request mode.
* [Service feature table (no cache)](../..//Data/ServiceFeatureTableNoCache) - Display a feature layer from a service using the **no cache** feature request mode.
* [Statistical query](../..//Data/StatisticalQuery) - Query a table to get aggregated statistics back for a specific field.
* [Statistical query group and sort](../..//Data/StatsQueryGroupAndSort) - Query a feature table for statistics, grouping and sorting by different fields.
* [Symbolize a shapefile](../..//Data/SymbolizeShapefile) - Display a shapefile with custom symbology.
* [Update attributes (feature service)](../..//Data/UpdateAttributes) - Update feature attributes in an online feature service.
* [Update geometries (feature service)](../..//Data/UpdateGeometries) - Update a feature's location in an online feature service.
* [View point cloud data offline](../..//Data/ViewPointCloudDataOffline) - Display local 3D point cloud data.

## Geometry

* [Buffer](../..//Geometry/Buffer) - Create a buffer around a map point and display the results as a `Graphic`
* [Buffer list](../..//Geometry/BufferList) - Generate multiple individual buffers or a single unioned buffer around multiple points.
* [Clip geometry](../..//Geometry/ClipGeometry) - Clip a geometry with another geometry.
* [Convex hull](../..//Geometry/ConvexHull) - Create a convex hull for a given set of points. The convex hull is a polygon with shortest perimeter that encloses a set of points. As a visual analogy, consider a set of points as nails in a board. The convex hull of the points would be like a rubber band stretched around the outermost nails.
* [Convex hull list](../..//Geometry/ConvexHullList) - This sample demonstrates how to use the GeometryEngine.ConvexHull to generate convex hull polygon(s) from multiple input geometries.
* [Create geometries](../..//Geometry/CreateGeometries) - Create simple geometry types.
* [Cut geometry](../..//Geometry/CutGeometry) - Cut a geometry along a polyline.
* [Densify and generalize](../..//Geometry/DensifyAndGeneralize) - A multipart geometry can be densified by adding interpolated points at regular intervals. Generalizing multipart geometry simplifies it while preserving its general shape. Densifying a multipart geometry adds more vertices at regular intervals.
* [Format coordinates](../..//Geometry/FormatCoordinates) - Format coordinates in a variety of common notations.
* [Geodesic operations](../..//Geometry/GeodesicOperations) - This sample demonstrates how to perform geodesic operations on geometries using the GeometryEngine. Geodesic calculations take into account the curvature of the Earth, while planar calculations are based on a 2D Cartesian plane.
* [List transformations by suitability](../..//Geometry/ListTransformations) - Get a list of suitable transformations for projecting a geometry between two spatial references with different horizontal datums.
* [Nearest vertex](../..//Geometry/NearestVertex) - Shows how to find the nearest vertex on a geometry to a given point.
* [Project](../..//Geometry/Project) - Project a point from one spatial reference to another.
* [Project with specific transformation](../..//Geometry/ProjectWithSpecificTransformation) - This sample demonstrates how to use the GeometryEngine with a specified geographic transformation to transform a geometry from one coordinate system to another. 
* [Perform spatial operations](../..//Geometry/SpatialOperations) - Find the union, intersection, or difference of two geometries.
* [Spatial relationships](../..//Geometry/SpatialRelationships) - Determine spatial relationships between two geometries.

## Geoprocessing

* [Analyze hotspots](../..//Geoprocessing/AnalyzeHotspots) - Use a geoprocessing service and a set of features to identify statistically significant hot spots and cold spots.
* [Analyze viewshed (geoprocessing)](../..//Geoprocessing/AnalyzeViewshed) - Calculate a viewshed using a geoprocessing service, in this case showing what parts of a landscape are visible from points on mountainous terrain.
* [List geodatabase versions](../..//Geoprocessing/ListGeodatabaseVersions) - This sample calls a custom GeoprocessingTask to get a list of available versions for an enterprise geodatabase. The task returns a table of geodatabase version information, which is displayed in the app as a list.

## GraphicsOverlay

* [Add graphics with renderer](../..//GraphicsOverlay/AddGraphicsRenderer) - Change the style of all graphics in a graphics overlay by referencing a single symbol style.
* [Add graphics with symbols](../..//GraphicsOverlay/AddGraphicsWithSymbols) - Use a symbol style to display a graphic on a graphics overlay.
* [Animate 3D graphic](../..//GraphicsOverlay/Animate3DGraphic) - An `OrbitGeoElementCameraController` follows a graphic while the graphic's position and rotation are animated.
* [Dictionary renderer with graphics overlay](../..//GraphicsOverlay/DictionaryRendererGraphicsOverlay) - Render graphics with mil2525d symbols.
* [Identify graphics](../..//GraphicsOverlay/IdentifyGraphics) - Display an alert message when a graphic is clicked.
* [Scene properties expressions](../..//GraphicsOverlay/ScenePropertiesExpressions) - Update the orientation of a graphic using scene property rotation expressions.
* [Sketch on map](../..//GraphicsOverlay/SketchOnMap) - Use the Sketch Editor to edit or sketch a new point, line, or polygon geometry on to a map.
* [Surface placement](../..//GraphicsOverlay/SurfacePlacements) - Position graphics relative to a surface using different surface placement modes.

## Hydrography

* [Add ENC exchange set](../..//Hydrography/AddEncExchangeSet) - Display nautical charts per the ENC specification.
* [Change ENC display settings](../..//Hydrography/ChangeEncDisplaySettings) - Configure the display of ENC content.
* [Select ENC features](../..//Hydrography/SelectEncFeatures) - Select features in an ENC layer.

## Layers

* [Add an integrated mesh layer](../..//Layers/AddAnIntegratedMeshLayer) - View an integrated mesh layer from a scene service.
* [Add a point scene layer](../..//Layers/AddPointSceneLayer) - View a point scene layer from a scene service.
* [ArcGIS map image layer](../..//Layers/ArcGISMapImageLayerUrl) - Add an ArcGIS Map Image Layer from a URL to a map.
* [ArcGIS tiled layer](../..//Layers/ArcGISTiledLayerUrl) - Load an ArcGIS tiled layer from a URL.
* [ArcGIS vector tiled layer URL](../..//Layers/ArcGISVectorTiledLayerUrl) - Load an ArcGIS Vector Tiled Layer from a URL.
* [Browse WFS layers](../..//Layers/BrowseWfsLayers) - Browse a WFS service for layers.
* [Blend renderer](../..//Layers/ChangeBlendRenderer) - Blend a hillshade with a raster by specifying the elevation data. The resulting raster looks similar to the original raster, but with some terrain shading, giving it a textured look.
* [Change feature layer renderer](../..//Layers/ChangeFeatureLayerRenderer) - Change the appearance of a feature layer with a renderer.
* [Stretch renderer](../..//Layers/ChangeStretchRenderer) - Use a stretch renderer to enhance the visual contrast of raster data for analysis.
* [Change sublayer renderer](../..//Layers/ChangeSublayerRenderer) - Apply a renderer to a sublayer.
* [Map image layer sublayer visibility](../..//Layers/ChangeSublayerVisibility) - Change the visibility of sublayers.
* [Feature collection layer](../..//Layers/CreateFeatureCollectionLayer) - Create a Feature Collection Layer from a Feature Collection Table, and add it to a map.
* [Display KML](../..//Layers/DisplayKml) - This sample demonstrates how to load and display KML files from:
* [Display KML network links](../..//Layers/DisplayKmlNetworkLinks) -  Display a file with a KML network link.
* [Display a scene](../..//Layers/DisplayScene) - Display a scene with a terrain surface and some imagery.
* [Display a WFS layer](../..//Layers/DisplayWfs) - Display a layer from a WFS service, requesting only features for the current extent.
* [Export tiles](../..//Layers/ExportTiles) - Download tiles to a local tile cache file stored on the device.
* [Feature collection layer from portal item](../..//Layers/FeatureCollectionLayerFromPortal) - This sample demonstrates opening a feature collection saved as a portal item.
* [Feature collection layer (Query)](../..//Layers/FeatureCollectionLayerFromQuery) - Create a feature collection layer to show a query result from a service feature table. The feature collection is then displayed on a map with a feature collection layer.
* [Feature layer definition expression](../..//Layers/FeatureLayerDefinitionExpression) - Limit the features to display on the map using a definition expression.
* [Dictionary renderer with feature layer](../..//Layers/FeatureLayerDictionaryRenderer) - Convert features into graphics to show them with mil2525d symbols.
* [Feature layer rendering mode (map)](../..//Layers/FeatureLayerRenderingModeMap) - Render features statically or dynamically by setting the feature layer rendering mode.
* [Feature layer rendering mode (scene)](../..//Layers/FeatureLayerRenderingModeScene) - Render features in a scene statically or dynamically by setting the feature layer rendering mode.
* [Feature layer selection](../..//Layers/FeatureLayerSelection) - Select features in a feature layer.
* [Feature layer (feature service)](../..//Layers/FeatureLayerUrl) - Show a feature layer on a map using the URL to the service.
* [Group layers](../..//Layers/GroupLayers) - Group a collection of layers together and toggle their visibility as a group.
* [Identify KML features](../..//Layers/IdentifyKmlFeatures) - This sample demonstrates how to identify features in a KML layer. Identified feature attributes are displayed in a callout to simulate a popup.
* [List KML contents](../..//Layers/ListKmlContents) - List the contents of a KML file. KML files can contain a hierarchy of features, including network links to other KML content.
* [Web tiled layer](../..//Layers/LoadWebTiledLayer) - Display a tiled web layer.
* [Map image layer tables](../..//Layers/MapImageLayerTables) - Find features in a spatial table related to features in a non-spatial table.
* [Query map image sublayer](../..//Layers/MapImageSublayerQuery) - Find features in a sublayer based on attributes and location.
* [OpenStreetMap layer](../..//Layers/OpenStreetMapLayer) - Add OpenStreetMap as a basemap layer.
* [Play a KML tour](../..//Layers/PlayKmlTours) - Play tours in KML files.
* [Raster hillshade renderer](../..//Layers/RasterHillshade) - Use a hillshade renderer on a raster.
* [Raster layer (file)](../..//Layers/RasterLayerFile) - Create and use a raster layer made from a local raster file.
* [Raster layer (service)](../..//Layers/RasterLayerImageServiceRaster) - Create a raster layer from a raster image service.
* [ArcGIS raster function (service)](../..//Layers/RasterLayerRasterFunction) - Show a raster layer from an image service with a raster function applied.
* [Raster rendering rule](../..//Layers/RasterRenderingRule) - Display a raster on a map and apply different rendering rules to that raster.
* [RGB Renderer](../..//Layers/RasterRgbRenderer) - Use an `RGBRenderer` on a `RasterLayer`. An `RGBRenderer` is used to adjust the color bands of a multispectral image.
* [Scene layer selection](../..//Layers/SceneLayerSelection) - Identify GeoElements in a scene layer.
* [Scene layer (URL)](../..//Layers/SceneLayerUrl) - Display an ArcGIS scene layer from a URL.
* [Show labels on layers](../..//Layers/ShowLabelsOnLayer) - Display custom labels on a feature layer.
* [Style WMS layers](../..//Layers/StyleWmsLayer) - Discover available styles and apply them to WMS sublayers.
* [Time-based query](../..//Layers/TimeBasedQuery) - This sample demonstrates how to query data using a time extent. This workflow can be used to return records that are between a specified start and end date. For example, you could specify to only show records that are before September 16, 2000.
* [Load WFS with XML query](../..//Layers/WfsXmlQuery) - Load a WFS feature table using an XML query.
* [Identify WMS features](../..//Layers/WmsIdentify) - Identify features in a WMS layer and display the associated popup content.
* [WMS Layer URL](../..//Layers/WMSLayerUrl) - Display a WMS layer using a WMS service URL.
* [WMS service catalog](../..//Layers/WmsServiceCatalog) - Connect to a WMS service and show the available layers and sublayers. Layers are shown in a hierarchy. Selecting a group layer will recursively select all sublayers for display.
* [WMTS layer](../..//Layers/WMTSLayer) - Display a layer from a Web Map Tile Service.

## Location

* [Display device location](../..//Location/DisplayDeviceLocation) - Display your current position on the map, as well as switch between different types of auto pan Modes.

## Map

* [Access load status](../..//Map/AccessLoadStatus) - Determine the map's load status which can be: `NotLoaded`, `FailedToLoad`, `Loading`, `Loaded`.
* [Create and save a map](../..//Map/AuthorMap) - Create and save a map as an ArcGIS `PortalItem` (i.e. web map).
* [Change atmosphere effect](../..//Map/ChangeAtmosphereEffect) - Changes the appearance of the atmosphere in a scene.
* [Change basemap](../..//Map/ChangeBasemap) - Change a map's basemap. A basemap is beneath all layers on an `Map` and is used to provide visual reference for the operational layers.
* [Create terrain surface from a local raster](../..//Map/CreateTerrainSurfaceFromRaster) - Use a terrain surface with elevation described by a raster file.
* [Create terrain surface from a tile package](../..//Map/CreateTerrainSurfaceFromTilePackage) - Set the terrain surface with elevation described by a local tile package.
* [Display a map](../..//Map/DisplayMap) - Display a map with an imagery basemap.
* [Download a preplanned map area](../..//Map/DownloadPreplannedMap) - Take a map offline using an available preplanned map area.
* [Generate offline map](../..//Map/GenerateOfflineMap) - Take a web map offline.
* [Generate Offline Map (Overrides)](../..//Map/GenerateOfflineMapWithOverrides) - Use the `OfflineMapTask` with overrides to take a webmap offline. The overrides workflow allows you to adjust the settings used for taking each layer in the map offline. For a simple example of how you take a map offline, please consult the "Generate Offline Map" sample.
* [Get elevation at a point](../..//Map/GetElevationAtPoint) - Get the elevation for a given point on a surface in a scene. 
* [Manage bookmarks](../..//Map/ManageBookmarks) - Access and create bookmarks on a map.
* [Manage operational layers](../..//Map/ManageOperationalLayers) - Add, remove, and reorder operational layers in a map.
* [Map reference scale](../..//Map/MapReferenceScale) - Set a map's reference scale and control which feature layers should honor that scale.
* [Mobile map (search and route)](../..//Map/MobileMapSearchAndRoute) - Display maps and use locators to enable search and routing offline using a Mobile Map Package.
* [Generate offline map with local basemap](../..//Map/OfflineBasemapByReference) - Use the `OfflineMapTask` to take a web map offline, but instead of downloading an online basemap, use one which is already on the device.
* [Open map URL](../..//Map/OpenMapURL) - Display a web map.
* [Open mobile map package](../..//Map/OpenMobileMap) - Display a map from a mobile map package.
* [Open Mobile Scene Package](../..//Map/OpenMobileScenePackage) - Open and display a scene from a Mobile Scene Package (.mspk).
* [Open a scene (Portal item)](../..//Map/OpenScene) - Open a scene from a Portal item. Just like Web Maps are the ArcGIS format for maps, Web Scenes are the ArcGIS format for scenes. These scenes can be stored in ArcGIS Online or Portal. 
* [Search for webmap](../..//Map/SearchPortalMaps) - Find webmap portal items by using a search term.
* [Map initial extent](../..//Map/SetInitialMapArea) - Display the map at an initial viewpoint representing a bounding geometry.
* [Set initial map location](../..//Map/SetInitialMapLocation) - Display a map centered on an initial point with a specified level of detail (zoom level).
* [Set map spatial reference](../..//Map/SetMapSpatialReference) - Specify a map's spatial reference.
* [Set min & max scale](../..//Map/SetMinMaxScale) - Restrict zooming between specific scale ranges.
* [Terrain exaggeration](../..//Map/TerrainExaggeration) - Configure the vertical exaggeration of terrain (the ground surface) in a scene.
* [View content beneath terrain surface](../..//Map/ViewContentBeneathSurface) - See through terrain in a scene and move the camera underground.

## MapView

* [Change time extent](../..//MapView/ChangeTimeExtent) - This sample demonstrates how to filter data in layers by applying a time extent to a MapView.
* [Change viewpoint](../..//MapView/ChangeViewpoint) - Set the map view to a new viewpoint.
* [Display draw status](../..//MapView/DisplayDrawingStatus) - Get the draw status of your map view or scene view to know when all layers in the map or scene have finished drawing.
* [Display grid](../..//MapView/DisplayGrid) - Display coordinate system grids including Latitude/Longitude, MGRS, UTM and USNG on a map view. Also, toggle label visibility and change the color of grid lines and grid labels.
* [Display layer view state](../..//MapView/DisplayLayerViewState) - View the status of the layers on the map.
* [Feature layer time offset](../..//MapView/FeatureLayerTimeOffset) - Show data from the same service side-by-side with a time offset. This allows for the comparison of data over time.
* [GeoView viewpoint synchronization](../..//MapView/GeoViewSync) - Keep the view points of two views (a MapView and a SceneView in this case) synchronized with each other.
* [Identify layers](../..//MapView/IdentifyLayers) - Identify features in all layers in a map. MapView supports identifying features across multiple layers. Because some layer types have sublayers, the sample recursively counts results for sublayers within each layer.
* [Map rotation](../..//MapView/MapRotation) - Rotate a map.
* [Show callout](../..//MapView/ShowCallout) - Show a callout with the latitude and longitude of user-tapped points.
* [Show magnifier](../..//MapView/ShowMagnifier) - Tap and hold on a map to show a magnifier.
* [Take a screenshot](../..//MapView/TakeScreenshot) - Take a screenshot of the map.

## Network Analysis

* [Find closest facility to an incident (interactive)](../..//Network Analysis/ClosestFacility) - Find a route to the closest facility from a location.
* [Find closest facility to multiple incidents (service)](../..//Network Analysis/ClosestFacilityStatic) - Find routes from several locations to the respective closest facility.
* [Find route](../..//Network Analysis/FindRoute) - Display directions for a route between two points.
* [Find service area](../..//Network Analysis/FindServiceArea) - Find the service area within a network from a given point.
* [Find service areas for multiple facilities](../..//Network Analysis/FindServiceAreasForMultipleFacilities) - Find the service areas of several facilities from a feature service.
* [Offline routing](../..//Network Analysis/OfflineRouting) - Solve a route on-the-fly using offline data.
* [Route around barriers](../..//Network Analysis/RouteAroundBarriers) - Find a route that reaches all stops without crossing any barriers.

## Search

* [Find address](../..//Search/FindAddress) - Find the location for an address.
* [Find place](../..//Search/FindPlace) - Find places of interest near a location or within a specific area.
* [Offline geocode](../..//Search/OfflineGeocode) - Geocode addresses to locations and reverse geocode locations to addresses offline.
* [Reverse geocode](../..//Search/ReverseGeocode) - Use an online service to find the address for a tapped point.

## Security

* [Integrated Windows Authentication](../..//Security/IntegratedWindowsAuth) - This sample illustrates the use of Windows credentials to access services hosted on a portal secured with Integrated Windows Authentication (IWA).
When accessing an item secured with IWA from a WPF app, default credentials (the current user's login) are sent to the portal. 
Platforms such as Android, iOS, and Universal Windows Platform (UWP) require credentials to be entered explicitly.
* [Authenticate with OAuth](../..//Security/OAuth) - This sample demonstrates how to authenticate with ArcGIS Online (or your own portal) using OAuth2 to access secured resources (such as private web maps or layers). Accessing secured items requires a login on the portal that hosts them (an ArcGIS Online account, for example).
* [ArcGIS token challenge](../..//Security/TokenSecuredChallenge) - This sample demonstrates how to prompt the user for a username and password to authenticate with ArcGIS Server to access an ArcGIS token-secured service. Accessing secured services requires a login that's been defined on the server.

## Symbology

* [Feature layer extrusion](../..//Symbology/FeatureLayerExtrusion) - Extrude features based on their attributes.
* [Render picture markers](../..//Symbology/RenderPictureMarkers) - Use pictures for markers.
* [Render simple markers](../..//Symbology/RenderSimpleMarkers) - Show a simple marker symbol on a map.
* [Unique value renderer](../..//Symbology/RenderUniqueValues) - Render features in a layer using a distinct symbol for each unique attribute value.
* [Scene symbols](../..//Symbology/SceneSymbols) - Show various kinds of 3D symbols in a scene.
* [Simple renderer](../..//Symbology/SimpleRenderers) - Display common symbols for all graphics in a graphics overlay with a renderer.
* [Read symbols from a mobile style](../..//Symbology/SymbolsFromMobileStyle) - Open a mobile style (.stylx) and read its contents. Combine several symbols from the style into a single multilayer point symbol, then use it to display graphics in the map view.
* [Distance composite scene symbol](../..//Symbology/UseDistanceCompositeSym) - Change a graphic's symbol based on the camera's proximity to it.

