# Table of contents

## Analysis

* [Distance measurement analysis](WPF.Viewer/Samples/Analysis/DistanceMeasurement) - Measure distances between two points in 3D.
* [Line of sight (geoelement)](WPF.Viewer/Samples/Analysis/LineOfSightGeoElement) - Show a line of sight between two moving objects.
* [Line of sight (location)](WPF.Viewer/Samples/Analysis/LineOfSightLocation) - Perform a line of sight analysis between two points in real time.
* [Query feature count and extent](WPF.Viewer/Samples/Analysis/QueryFeatureCountAndExtent) - Zoom to features matching a query and count the features in the current visible extent.
* [Viewshed (location)](WPF.Viewer/Samples/Analysis/ViewshedLocation) - Perform a viewshed analysis from a defined vantage point.
* [Viewshed for GeoElement](WPF.Viewer/Samples/Analysis/ViewshedGeoElement) - Analyze the viewshed for an object (GeoElement) in a scene.
* [Viewshed for camera](WPF.Viewer/Samples/Analysis/ViewshedCamera) - Analyze the viewshed for a camera. A viewshed shows the visible and obstructed areas from an observer's vantage point.

## Data

* [Add features with contingent values](WPF.Viewer/Samples/Data/AddFeaturesWithContingentValues) - Create and add features whose attribute values satisfy a predefined set of contingencies.
* [Create KML multi-track](WPF.Viewer/Samples/Data/CreateKmlMultiTrack) - Create, save and preview a KML multi-track, captured from a location data source.
* [Create mobile geodatabase](WPF.Viewer/Samples/Data/CreateMobileGeodatabase) - Create and share a mobile geodatabase.
* [Edit and sync features](WPF.Viewer/Samples/Data/EditAndSyncFeatures) - Synchronize offline edits with a feature service.
* [Edit feature attachments](WPF.Viewer/Samples/Data/EditFeatureAttachments) - Add, delete, and download attachments for features from a service.
* [Edit features using feature forms](WPF.Viewer/Samples/Data/EditFeaturesUsingFeatureForms) - Display and edit feature attributes using feature forms.
* [Edit features with feature-linked annotation](WPF.Viewer/Samples/Data/EditFeatureLinkedAnnotation) - Edit feature attributes which are linked to annotation through an expression.
* [Edit with branch versioning](WPF.Viewer/Samples/Data/EditBranchVersioning) - Create, query and edit a specific server version using service geodatabase.
* [Feature layer query](WPF.Viewer/Samples/Data/FeatureLayerQuery) - Find features in a feature table which match an SQL query.
* [Generate geodatabase replica from feature service](WPF.Viewer/Samples/Data/GenerateGeodatabaseReplica) - Generate a local geodatabase from an online feature service.
* [Geodatabase transactions](WPF.Viewer/Samples/Data/GeodatabaseTransactions) - Use transactions to manage how changes are committed to a geodatabase.
* [List related features](WPF.Viewer/Samples/Data/ListRelatedFeatures) - List features related to the selected feature.
* [Manage features](WPF.Viewer/Samples/Data/ManageFeatures) - Create, update, and delete features to manage a feature layer.
* [Query features with Arcade expression](WPF.Viewer/Samples/Data/QueryFeaturesWithArcadeExpression) - Query features on a map using an Arcade expression.
* [Raster layer (GeoPackage)](WPF.Viewer/Samples/Data/RasterLayerGeoPackage) - Display a raster contained in a GeoPackage.
* [Read GeoPackage](WPF.Viewer/Samples/Data/ReadGeoPackage) - Add rasters and feature tables from a GeoPackage to a map.
* [Read shapefile metadata](WPF.Viewer/Samples/Data/ReadShapefileMetadata) - Read a shapefile and display its metadata.
* [Statistical query](WPF.Viewer/Samples/Data/StatisticalQuery) - Query a table to get aggregated statistics back for a specific field.
* [Statistical query group and sort](WPF.Viewer/Samples/Data/StatsQueryGroupAndSort) - Query a feature table for statistics, grouping and sorting by different fields.
* [Symbolize shapefile](WPF.Viewer/Samples/Data/SymbolizeShapefile) - Display a shapefile with custom symbology.
* [Toggle between feature request modes](WPF.Viewer/Samples/Data/ToggleBetweenFeatureRequestModes) - Use different feature request modes to populate the map from a service feature table.
* [View point cloud data offline](WPF.Viewer/Samples/Data/ViewPointCloudDataOffline) - Display local 3D point cloud data.

## Geometry

* [Buffer](WPF.Viewer/Samples/Geometry/Buffer) - Create a buffer around a map point and display the results as a `Graphic`
* [Buffer list](WPF.Viewer/Samples/Geometry/BufferList) - Generate multiple individual buffers or a single unioned buffer around multiple points.
* [Clip geometry](WPF.Viewer/Samples/Geometry/ClipGeometry) - Clip a geometry with another geometry.
* [Convex hull](WPF.Viewer/Samples/Geometry/ConvexHull) - Create a convex hull for a given set of points. The convex hull is a polygon with shortest perimeter that encloses a set of points. As a visual analogy, consider a set of points as nails in a board. The convex hull of the points would be like a rubber band stretched around the outermost nails.
* [Convex hull list](WPF.Viewer/Samples/Geometry/ConvexHullList) - Generate convex hull polygon(s) from multiple input geometries.
* [Create and edit geometries](WPF.Viewer/Samples/Geometry/CreateAndEditGeometries) - Use the Geometry Editor to create new point, multipoint, polyline, or polygon geometries or to edit existing geometries by interacting with a map view.
* [Create geometries](WPF.Viewer/Samples/Geometry/CreateGeometries) - Create simple geometry types.
* [Cut geometry](WPF.Viewer/Samples/Geometry/CutGeometry) - Cut a geometry along a polyline.
* [Densify and generalize](WPF.Viewer/Samples/Geometry/DensifyAndGeneralize) - A multipart geometry can be densified by adding interpolated points at regular intervals. Generalizing multipart geometry simplifies it while preserving its general shape. Densifying a multipart geometry adds more vertices at regular intervals.
* [Format coordinates](WPF.Viewer/Samples/Geometry/FormatCoordinates) - Format coordinates in a variety of common notations.
* [Geodesic operations](WPF.Viewer/Samples/Geometry/GeodesicOperations) - Calculate a geodesic path between two points and measure its distance.
* [List transformations by suitability](WPF.Viewer/Samples/Geometry/ListTransformations) - Get a list of suitable transformations for projecting a geometry between two spatial references with different horizontal datums.
* [Nearest vertex](WPF.Viewer/Samples/Geometry/NearestVertex) - Find the closest vertex and coordinate of a geometry to a point.
* [Perform spatial operations](WPF.Viewer/Samples/Geometry/SpatialOperations) - Find the union, intersection, or difference of two geometries.
* [Project](WPF.Viewer/Samples/Geometry/Project) - Project a point from one spatial reference to another.
* [Project with specific transformation](WPF.Viewer/Samples/Geometry/ProjectWithSpecificTransformation) - Project a point from one coordinate system to another using a specific transformation step.
* [Snap geometry edits](WPF.Viewer/Samples/Geometry/SnapGeometryEdits) - Use the Geometry Editor to edit a geometry and align it to existing geometries on a map.
* [Spatial relationships](WPF.Viewer/Samples/Geometry/SpatialRelationships) - Determine spatial relationships between two geometries.

## Geoprocessing

* [Analyze hotspots](WPF.Viewer/Samples/Geoprocessing/AnalyzeHotspots) - Use a geoprocessing service and a set of features to identify statistically significant hot spots and cold spots.
* [Analyze viewshed (geoprocessing)](WPF.Viewer/Samples/Geoprocessing/AnalyzeViewshed) - Calculate a viewshed using a geoprocessing service, in this case showing what parts of a landscape are visible from points on mountainous terrain.
* [List geodatabase versions](WPF.Viewer/Samples/Geoprocessing/ListGeodatabaseVersions) - Connect to a service and list versions of the geodatabase.

## GraphicsOverlay

* [Add graphics with renderer](WPF.Viewer/Samples/GraphicsOverlay/AddGraphicsRenderer) - A renderer allows you to change the style of all graphics in a graphics overlay by referencing a single symbol style. A renderer will only affect graphics that do not specify their own symbol style.
* [Add graphics with symbols](WPF.Viewer/Samples/GraphicsOverlay/AddGraphicsWithSymbols) - Use a symbol style to display a graphic on a graphics overlay.
* [Animate 3D graphic](WPF.Viewer/Samples/GraphicsOverlay/Animate3DGraphic) - An `OrbitGeoElementCameraController` follows a graphic while the graphic's position and rotation are animated.
* [Dictionary renderer with graphics overlay](WPF.Viewer/Samples/GraphicsOverlay/DictionaryRendererGraphicsOverlay) - Create graphics from an XML file with key-value pairs for each graphic, and display the military symbols using a MIL-STD-2525D web style in 2D.
* [Identify graphics](WPF.Viewer/Samples/GraphicsOverlay/IdentifyGraphics) - Display an alert message when a graphic is clicked.
* [Scene properties expressions](WPF.Viewer/Samples/GraphicsOverlay/ScenePropertiesExpressions) - Update the orientation of a graphic using expressions based on its attributes.
* [Surface placement](WPF.Viewer/Samples/GraphicsOverlay/SurfacePlacements) - Position graphics relative to a surface using different surface placement modes.

## Layers

* [Add 3d tiles layer](WPF.Viewer/Samples/Layers/Add3dTilesLayer) - Add a layer to visualize 3D tiles data that conforms to the OGC 3D Tiles specification.
* [Add a point scene layer](WPF.Viewer/Samples/Layers/AddPointSceneLayer) - View a point scene layer from a scene service.
* [Add custom dynamic entity data source](WPF.Viewer/Samples/Layers/AddCustomDynamicEntityDataSource) - Create a custom dynamic entity data source and display it using a dynamic entity layer.
* [Add dynamic entity layer](WPF.Viewer/Samples/Layers/AddDynamicEntityLayer) - Display data from an ArcGIS stream service using a dynamic entity layer.
* [Add integrated mesh layer](WPF.Viewer/Samples/Layers/AddAnIntegratedMeshLayer) - View an integrated mesh layer from a scene service.
* [Add vector tiled layer from custom style](WPF.Viewer/Samples/Layers/AddVectorTiledLayerFromCustomStyle) - Load ArcGIS vector tiled layers using custom styles.
* [Apply mosaic rule to rasters](WPF.Viewer/Samples/Layers/ApplyMosaicRule) - Apply mosaic rule to a mosaic dataset of rasters.
* [Apply raster function to raster from service](WPF.Viewer/Samples/Layers/RasterLayerRasterFunction) - Load a raster from a service, then apply a function to it.
* [ArcGIS map image layer](WPF.Viewer/Samples/Layers/ArcGISMapImageLayerUrl) - Add an ArcGIS Map Image Layer from a URL to a map.
* [ArcGIS tiled layer](WPF.Viewer/Samples/Layers/ArcGISTiledLayerUrl) - Load an ArcGIS tiled layer from a URL.
* [Blend renderer](WPF.Viewer/Samples/Layers/ChangeBlendRenderer) - Blend a hillshade with a raster by specifying the elevation data. The resulting raster looks similar to the original raster, but with some terrain shading, giving it a textured look.
* [Browse OGC API feature service](WPF.Viewer/Samples/Layers/BrowseOAFeatureService) - Browse an OGC API feature service for layers and add them to the map.
* [Browse WFS layers](WPF.Viewer/Samples/Layers/BrowseWfsLayers) - Browse a WFS service for layers and add them to the map.
* [Change feature layer renderer](WPF.Viewer/Samples/Layers/ChangeFeatureLayerRenderer) - Change the appearance of a feature layer with a renderer.
* [Change sublayer renderer](WPF.Viewer/Samples/Layers/ChangeSublayerRenderer) - Apply a renderer to a sublayer.
* [Colormap renderer](WPF.Viewer/Samples/Layers/RasterColormapRenderer) - Apply a colormap renderer to a raster.
* [Configure clusters](WPF.Viewer/Samples/Layers/ConfigureClusters) - Add client side feature reduction on a point feature layer that is not pre-configured with clustering.
* [Configure electronic navigational charts](WPF.Viewer/Samples/Layers/ConfigureElectronicNavigationalCharts) - Display and configure electronic navigational charts per ENC specification.
* [Control annotation sublayer visibility](WPF.Viewer/Samples/Layers/ControlAnnotationSublayerVisibility) - Use annotation sublayers to gain finer control of annotation layer subtypes.
* [Create and save KML file](WPF.Viewer/Samples/Layers/CreateAndSaveKmlFile) - Construct a KML document and save it as a KMZ file.
* [Create feature collection layer (Portal item)](WPF.Viewer/Samples/Layers/FeatureCollectionLayerFromPortal) - Create a feature collection layer from a portal item.
* [Dictionary renderer with feature layer](WPF.Viewer/Samples/Layers/FeatureLayerDictionaryRenderer) - Convert features into graphics to show them with mil2525d symbols.
* [Display KML](WPF.Viewer/Samples/Layers/DisplayKml) - Display KML from a URL, portal item, or local KML file.
* [Display KML network links](WPF.Viewer/Samples/Layers/DisplayKmlNetworkLinks) - Display a file with a KML network link, including displaying any network link control messages at launch.
* [Display OGC API collection](WPF.Viewer/Samples/Layers/DisplayOACollection) - Display an OGC API feature collection and query features while navigating the map view.
* [Display WFS layer](WPF.Viewer/Samples/Layers/DisplayWfs) - Display a layer from a WFS service, requesting only features for the current extent.
* [Display a scene](WPF.Viewer/Samples/Layers/DisplayScene) - Display a scene with a terrain surface and some imagery.
* [Display annotation](WPF.Viewer/Samples/Layers/DisplayAnnotation) - Display annotation from a feature service URL.
* [Display clusters](WPF.Viewer/Samples/Layers/DisplayClusters) - Display a web map with a point feature layer that has feature reduction enabled to aggregate points into clusters.
* [Display dimensions](WPF.Viewer/Samples/Layers/DisplayDimensions) - Display dimension features from a mobile map package.
* [Display feature layers](WPF.Viewer/Samples/Layers/DisplayFeatureLayers) - Display feature layers from various data sources.
* [Display route layer](WPF.Viewer/Samples/Layers/DisplayRouteLayer) - Display a route layer and its directions using a feature collection.
* [Display subtype feature layer](WPF.Viewer/Samples/Layers/DisplaySubtypeFeatureLayer) - Displays a composite layer of all the subtype values in a feature class.
* [Edit KML ground overlay](WPF.Viewer/Samples/Layers/EditKmlGroundOverlay) - Edit the values of a KML ground overlay.
* [Export tiles](WPF.Viewer/Samples/Layers/ExportTiles) - Download tiles to a local tile cache file stored on the device.
* [Export vector tiles](WPF.Viewer/Samples/Layers/ExportVectorTiles) - Export tiles from an online vector tile service.
* [Feature collection layer](WPF.Viewer/Samples/Layers/CreateFeatureCollectionLayer) - Create a Feature Collection Layer from a Feature Collection Table, and add it to a map.
* [Feature collection layer (query)](WPF.Viewer/Samples/Layers/FeatureCollectionLayerFromQuery) - Create a feature collection layer to show a query result from a service feature table.
* [Feature layer (feature service)](WPF.Viewer/Samples/Layers/FeatureLayerUrl) - Show features from an online feature service.
* [Feature layer rendering mode (map)](WPF.Viewer/Samples/Layers/FeatureLayerRenderingModeMap) - Render features statically or dynamically by setting the feature layer rendering mode.
* [Feature layer rendering mode (scene)](WPF.Viewer/Samples/Layers/FeatureLayerRenderingModeScene) - Render features in a scene statically or dynamically by setting the feature layer rendering mode.
* [Feature layer selection](WPF.Viewer/Samples/Layers/FeatureLayerSelection) - Select features in a feature layer.
* [Filter by definition expression or display filter](WPF.Viewer/Samples/Layers/FeatureLayerDefinitionExpression) - Filter features displayed on a map using a definition expression or a display filter.
* [Group layers](WPF.Viewer/Samples/Layers/GroupLayers) - Group a collection of layers together and toggle their visibility as a group.
* [Hillshade renderer](WPF.Viewer/Samples/Layers/RasterHillshade) - Apply a hillshade renderer to a raster.
* [Identify KML features](WPF.Viewer/Samples/Layers/IdentifyKmlFeatures) - Show a callout with formatted content for a KML feature.
* [Identify WMS features](WPF.Viewer/Samples/Layers/WmsIdentify) - Identify features in a WMS layer and display the associated popup content.
* [Identify raster cell](WPF.Viewer/Samples/Layers/IdentifyRasterCell) - Get the cell value of a local raster at the tapped location and display the result in a callout.
* [List KML contents](WPF.Viewer/Samples/Layers/ListKmlContents) - List the contents of a KML file.
* [Load WFS with XML query](WPF.Viewer/Samples/Layers/WfsXmlQuery) - Load a WFS feature table using an XML query.
* [Map image layer sublayer visibility](WPF.Viewer/Samples/Layers/ChangeSublayerVisibility) - Change the visibility of sublayers.
* [Map image layer tables](WPF.Viewer/Samples/Layers/MapImageLayerTables) - Find features in a spatial table related to features in a non-spatial table.
* [OpenStreetMap layer](WPF.Viewer/Samples/Layers/OpenStreetMapLayer) - Add OpenStreetMap as a basemap layer.
* [Play KML tour](WPF.Viewer/Samples/Layers/PlayKmlTours) - Play tours in KML files.
* [Query map image sublayer](WPF.Viewer/Samples/Layers/MapImageSublayerQuery) - Find features in a sublayer based on attributes and location.
* [Query with CQL filters](WPF.Viewer/Samples/Layers/QueryCQLFilters) - Query data from an OGC API feature service using CQL filters.
* [RGB renderer](WPF.Viewer/Samples/Layers/RasterRgbRenderer) - Apply an RGB renderer to a raster layer to enhance feature visibility.
* [Raster layer (file)](WPF.Viewer/Samples/Layers/RasterLayerFile) - Create and use a raster layer made from a local raster file.
* [Raster layer (service)](WPF.Viewer/Samples/Layers/RasterLayerImageServiceRaster) - Create a raster layer from a raster image service.
* [Raster rendering rule](WPF.Viewer/Samples/Layers/RasterRenderingRule) - Display a raster on a map and apply different rendering rules to that raster.
* [Scene layer (URL)](WPF.Viewer/Samples/Layers/SceneLayerUrl) - Display an ArcGIS scene layer from a URL.
* [Scene layer selection](WPF.Viewer/Samples/Layers/SceneLayerSelection) - Identify features in a scene to select.
* [Show labels on layers](WPF.Viewer/Samples/Layers/ShowLabelsOnLayer) - Display custom labels on a feature layer.
* [Stretch renderer](WPF.Viewer/Samples/Layers/ChangeStretchRenderer) - Use a stretch renderer to enhance the visual contrast of raster data for analysis.
* [Style WMS layers](WPF.Viewer/Samples/Layers/StyleWmsLayer) - Change the style of a Web Map Service (WMS) layer.
* [Time-based query](WPF.Viewer/Samples/Layers/TimeBasedQuery) - Query data using a time extent.
* [WMS layer (URL)](WPF.Viewer/Samples/Layers/WMSLayerUrl) - Display a WMS layer using a WMS service URL.
* [WMS service catalog](WPF.Viewer/Samples/Layers/WmsServiceCatalog) - Connect to a WMS service and show the available layers and sublayers.
* [WMTS layer](WPF.Viewer/Samples/Layers/WMTSLayer) - Display a layer from a Web Map Tile Service.
* [Web tiled layer](WPF.Viewer/Samples/Layers/LoadWebTiledLayer) - Display a tiled web layer.

## Local Server

* [Generate elevation profile with Local Server](WPF.Viewer/Samples/LocalServer/LocalServerGenerateElevationProfile) - Create an elevation profile using a geoprocessing package executed with Local Server.
* [Local Server map image layer](WPF.Viewer/Samples/LocalServer/LocalServerMapImageLayer) - Start the Local Server and Local Map Service, create an ArcGIS Map Image Layer from the Local Map Service, and add it to a map.
* [Local server feature layer](WPF.Viewer/Samples/LocalServer/LocalServerFeatureLayer) - Start a local feature service and display its features in a map.
* [Local server geoprocessing](WPF.Viewer/Samples/LocalServer/LocalServerGeoprocessing) - Create contour lines from local raster data using a local geoprocessing package `.gpk` and the contour geoprocessing tool.
* [Local server services](WPF.Viewer/Samples/LocalServer/LocalServerServices) - Demonstrates how to start and stop the Local Server and start and stop a local map, feature, and geoprocessing service running on the Local Server.

## Location

* [Display device location with NMEA data sources](WPF.Viewer/Samples/Location/LocationWithNMEA) - Parse NMEA sentences and use the results to show device location on the map.
* [Display device location with autopan modes](WPF.Viewer/Samples/Location/DisplayDeviceLocation) - Display your current position on the map, as well as switch between different types of auto pan modes.
* [Set up location-driven Geotriggers](WPF.Viewer/Samples/Location/LocationDrivenGeotriggers) - Create a notification every time a given location data source has entered and/or exited a set of features or graphics.
* [Show location history](WPF.Viewer/Samples/Location/ShowLocationHistory) - Display your location history on the map.

## Map

* [Apply scheduled updates to preplanned map area](WPF.Viewer/Samples/Map/ApplyScheduledUpdates) - Apply scheduled updates to a downloaded preplanned map area.
* [Browse building floors](WPF.Viewer/Samples/Map/BrowseBuildingFloors) - Display and browse through building floors from a floor-aware web map.
* [Change basemap](WPF.Viewer/Samples/Map/ChangeBasemap) - Change a map's basemap. A basemap is beneath all layers on a `Map` and is used to provide visual reference for the operational layers.
* [Configure basemap style parameters](WPF.Viewer/Samples/Map/ConfigureBasemapStyleParameters) - Apply basemap style parameters customization for a basemap, such as displaying all labels in a specific language or displaying every label in their corresponding local language.
* [Create and save map](WPF.Viewer/Samples/Map/AuthorMap) - Create and save a map as an ArcGIS `PortalItem` (i.e. web map).
* [Create dynamic basemap gallery](WPF.Viewer/Samples/Map/CreateDynamicBasemapGallery) - Implement a basemap gallery that automatically retrieves the latest customization options from the basemap styles service.
* [Display map](WPF.Viewer/Samples/Map/DisplayMap) - Display a map with an imagery basemap.
* [Display overview map](WPF.Viewer/Samples/Map/DisplayOverviewMap) - Include an overview or inset map as an additional map view to show the wider context of the primary view.
* [Download preplanned map area](WPF.Viewer/Samples/Map/DownloadPreplannedMap) - Take a map offline using a preplanned map area.
* [Generate offline map](WPF.Viewer/Samples/Map/GenerateOfflineMap) - Take a web map offline.
* [Generate offline map (overrides)](WPF.Viewer/Samples/Map/GenerateOfflineMapWithOverrides) - Take a web map offline with additional options for each layer.
* [Generate offline map with local basemap](WPF.Viewer/Samples/Map/OfflineBasemapByReference) - Use the `OfflineMapTask` to take a web map offline, but instead of downloading an online basemap, use one which is already on the device.
* [Honor mobile map package expiration date](WPF.Viewer/Samples/Map/HonorMobileMapPackageExpiration) - Access the expiration information of an expired mobile map package.
* [Manage bookmarks](WPF.Viewer/Samples/Map/ManageBookmarks) - Access and create bookmarks on a map.
* [Manage operational layers](WPF.Viewer/Samples/Map/ManageOperationalLayers) - Add, remove, and reorder operational layers in a map.
* [Map initial extent](WPF.Viewer/Samples/Map/SetInitialMapArea) - Display the map at an initial viewpoint representing a bounding geometry.
* [Map load status](WPF.Viewer/Samples/Map/AccessLoadStatus) - Determine the map's load status which can be: `NotLoaded`, `FailedToLoad`, `Loading`, `Loaded`.
* [Map reference scale](WPF.Viewer/Samples/Map/MapReferenceScale) - Set the map's reference scale and which feature layers should honor the reference scale.
* [Map spatial reference](WPF.Viewer/Samples/Map/SetMapSpatialReference) - Specify a map's spatial reference.
* [Mobile map (search and route)](WPF.Viewer/Samples/Map/MobileMapSearchAndRoute) - Display maps and use locators to enable search and routing offline using a Mobile Map Package.
* [Open map URL](WPF.Viewer/Samples/Map/OpenMapURL) - Display a web map.
* [Open mobile map package](WPF.Viewer/Samples/Map/OpenMobileMap) - Display a map from a mobile map package.
* [Search for webmap](WPF.Viewer/Samples/Map/SearchPortalMaps) - Find webmap portal items by using a search term.
* [Set initial map location](WPF.Viewer/Samples/Map/SetInitialMapLocation) - Display a basemap centered at an initial location and scale.
* [Set max extent](WPF.Viewer/Samples/Map/SetMaxExtent) - Limit the view of a map to a particular area.
* [Set min & max scale](WPF.Viewer/Samples/Map/SetMinMaxScale) - Restrict zooming between specific scale ranges.

## MapView

* [Change time extent](WPF.Viewer/Samples/MapView/ChangeTimeExtent) - Filter data in layers by applying a time extent to a MapView.
* [Change viewpoint](WPF.Viewer/Samples/MapView/ChangeViewpoint) - Set the map view to a new viewpoint.
* [Display draw status](WPF.Viewer/Samples/MapView/DisplayDrawingStatus) - Get the draw status of your map view or scene view to know when all layers in the map or scene have finished drawing.
* [Display grid](WPF.Viewer/Samples/MapView/DisplayGrid) - Display and customize coordinate system grids including Latitude/Longitude, MGRS, UTM and USNG on a map view or scene view.
* [Display layer view state](WPF.Viewer/Samples/MapView/DisplayLayerViewState) - Determine if a layer is currently being viewed.
* [Feature layer time offset](WPF.Viewer/Samples/MapView/FeatureLayerTimeOffset) - Display a time-enabled feature layer with a time offset.
* [Filter by time extent](WPF.Viewer/Samples/MapView/FilterByTimeExtent) - The time slider provides controls that allow you to visualize temporal data by applying a specific time extent to a map view.
* [Identify layers](WPF.Viewer/Samples/MapView/IdentifyLayers) - Identify features in all layers in a map.
* [Map rotation](WPF.Viewer/Samples/MapView/MapRotation) - Rotate a map.
* [Show callout](WPF.Viewer/Samples/MapView/ShowCallout) - Show a callout with the latitude and longitude of user-tapped points.
* [Show magnifier](WPF.Viewer/Samples/MapView/ShowMagnifier) - Tap and hold on a map to show a magnifier.
* [Take screenshot](WPF.Viewer/Samples/MapView/TakeScreenshot) - Take a screenshot of the map.

## Network analysis

* [Find closest facility to an incident (interactive)](WPF.Viewer/Samples/NetworkAnalysis/ClosestFacility) - Find a route to the closest facility from a location.
* [Find closest facility to multiple incidents (service)](WPF.Viewer/Samples/NetworkAnalysis/ClosestFacilityStatic) - Find routes from several locations to the respective closest facility.
* [Find route](WPF.Viewer/Samples/NetworkAnalysis/FindRoute) - Display directions for a route between two points.
* [Find service area](WPF.Viewer/Samples/NetworkAnalysis/FindServiceArea) - Find the service area within a network from a given point.
* [Find service areas for multiple facilities](WPF.Viewer/Samples/NetworkAnalysis/FindServiceAreasForMultipleFacilities) - Find the service areas of several facilities from a feature service.
* [Navigate route](WPF.Viewer/Samples/NetworkAnalysis/NavigateRoute) - Use a routing service to navigate between points.
* [Navigate route with rerouting](WPF.Viewer/Samples/NetworkAnalysis/NavigateRouteRerouting) - Navigate between two points and dynamically recalculate an alternate route when the original route is unavailable.
* [Offline routing](WPF.Viewer/Samples/NetworkAnalysis/OfflineRouting) - Solve a route on-the-fly using offline data.
* [Route around barriers](WPF.Viewer/Samples/NetworkAnalysis/RouteAroundBarriers) - Find a route that reaches all stops without crossing any barriers.

## Scene

* [Change atmosphere effect](WPF.Viewer/Samples/Scene/ChangeAtmosphereEffect) - Changes the appearance of the atmosphere in a scene.
* [Create terrain from local tile package](WPF.Viewer/Samples/Scene/CreateTerrainSurfaceTilePackage) - Set the terrain surface with elevation described by a local tile package.
* [Create terrain surface from a local raster](WPF.Viewer/Samples/Scene/CreateTerrainSurfaceRaster) - Set the terrain surface with elevation described by a raster file.
* [Filter features in scene](WPF.Viewer/Samples/Scene/FilterFeaturesInScene) - Filter 3D scene features out of a given geometry with a polygon filter.
* [Get elevation at a point](WPF.Viewer/Samples/Scene/GetElevationAtPoint) - Get the elevation for a given point on a surface in a scene.
* [Open mobile scene package](WPF.Viewer/Samples/Scene/OpenMobileScenePackage) - Opens and displays a scene from a Mobile Scene Package (.mspk).
* [Open scene (portal item)](WPF.Viewer/Samples/Scene/OpenScenePortalItem) - Open a web scene from a portal item.
* [Show labels on layer 3D](WPF.Viewer/Samples/Scene/ShowLabelsOnLayer3D) - Display custom labels in a 3D scene.
* [Terrain exaggeration](WPF.Viewer/Samples/Scene/TerrainExaggeration) - Vertically exaggerate terrain in a scene.
* [View content beneath terrain surface](WPF.Viewer/Samples/Scene/ViewContentBeneathSurface) - See through terrain in a scene and move the camera underground.

## SceneView

* [Animate images with image overlay](WPF.Viewer/Samples/SceneView/AnimateImageOverlay) - Animate a series of images with an image overlay.
* [Choose camera controller](WPF.Viewer/Samples/SceneView/ChooseCameraController) - Control the behavior of the camera in a scene.
* [GeoView viewpoint synchronization](WPF.Viewer/Samples/SceneView/GeoViewSync) - Keep the view points of two views (e.g. MapView and SceneView) synchronized with each other.

## Search

* [Find address](WPF.Viewer/Samples/Search/FindAddress) - Find the location for an address.
* [Find place](WPF.Viewer/Samples/Search/FindPlace) - Find places of interest near a location or within a specific area.
* [Offline geocode](WPF.Viewer/Samples/Search/OfflineGeocode) - Geocode addresses to locations and reverse geocode locations to addresses offline.
* [Reverse geocode](WPF.Viewer/Samples/Search/ReverseGeocode) - Use an online service to find the address for a tapped point.

## Security

* [ArcGIS token challenge](WPF.Viewer/Samples/Security/TokenSecuredChallenge) - This sample demonstrates how to prompt the user for a username and password to authenticate with ArcGIS Server to access an ArcGIS token-secured service. Accessing secured services requires a login that's been defined on the server.
* [Authenticate with OAuth](WPF.Viewer/Samples/Security/OAuth) - Authenticate with ArcGIS Online (or your own portal) using OAuth2 to access secured resources (such as private web maps or layers).
* [Certificate authentication with PKI](WPF.Viewer/Samples/Security/CertificateAuthenticationWithPki) - Access secured portals using a certificate.
* [Integrated Windows Authentication](WPF.Viewer/Samples/Security/IntegratedWindowsAuth) - Connect to an IWA secured Portal and search for maps.

## Symbology

* [Apply unique values with alternate symbols](WPF.Viewer/Samples/Symbology/UniqueValuesAlternateSymbols) - Apply a unique value with alternate symbols at different scales.
* [Create symbol styles from web styles](WPF.Viewer/Samples/Symbology/SymbolStylesFromWebStyles) - Create symbol styles from a style file hosted on a portal.
* [Custom dictionary style](WPF.Viewer/Samples/Symbology/CustomDictionaryStyle) - Use a custom dictionary created from a web style or style file (.stylx) to symbolize features using a variety of attribute values.
* [Distance composite scene symbol](WPF.Viewer/Samples/Symbology/UseDistanceCompositeSym) - Change a graphic's symbol based on the camera's proximity to it.
* [Feature layer extrusion](WPF.Viewer/Samples/Symbology/FeatureLayerExtrusion) - Extrude features based on their attributes.
* [Read symbols from mobile style](WPF.Viewer/Samples/Symbology/SymbolsFromMobileStyle) - Combine multiple symbols from a mobile style file into a single symbol.
* [Render multilayer symbols](WPF.Viewer/Samples/Symbology/RenderMultilayerSymbols) - Show different kinds of multilayer symbols on a map similar to some pre-defined 2D simple symbol styles.
* [Scene symbols](WPF.Viewer/Samples/Symbology/SceneSymbols) - Show various kinds of 3D symbols in a scene.
* [Simple renderer](WPF.Viewer/Samples/Symbology/SimpleRenderers) - Display common symbols for all graphics in a graphics overlay with a renderer.
* [Style geometry types with symbols](WPF.Viewer/Samples/Symbology/StyleGeometryTypesWithSymbols) - Use a symbol to display a geometry on a map.
* [Unique value renderer](WPF.Viewer/Samples/Symbology/RenderUniqueValues) - Render features in a layer using a distinct symbol for each unique attribute value.

## Utility network

* [Configure subnetwork trace](WPF.Viewer/Samples/UtilityNetwork/ConfigureSubnetworkTrace) - Get a server-defined trace configuration for a given tier and modify its traversability scope, add new condition barriers and control what is included in the subnetwork trace result.
* [Create load report](WPF.Viewer/Samples/UtilityNetwork/CreateLoadReport) - Demonstrates the creation of a simple electric distribution report. It traces downstream from a given point and adds up the count of customers and total load per phase.
* [Display content of utility network container](WPF.Viewer/Samples/UtilityNetwork/DisplayUtilityNetworkContainer) - A utility network container allows a dense collection of features to be represented by a single feature, which can be used to reduce map clutter.
* [Display utility associations](WPF.Viewer/Samples/UtilityNetwork/DisplayUtilityAssociations) - Create graphics for utility associations in a utility network.
* [Perform valve isolation trace](WPF.Viewer/Samples/UtilityNetwork/PerformValveIsolationTrace) - Run a filtered trace to locate operable features that will isolate an area from the flow of network resources.
* [Snap geometry edits with utility network rules](WPF.Viewer/Samples/UtilityNetwork/SnapGeometryEditsWithUtilityNetworkRules) - Use the Geometry Editor to edit geometries using utility network connectivity rules.
* [Trace utility network](WPF.Viewer/Samples/UtilityNetwork/TraceUtilityNetwork) - Discover connected features in a utility network using connected, subnetwork, upstream, and downstream traces.
* [Validate utility network topology](WPF.Viewer/Samples/UtilityNetwork/ValidateUtilityNetworkTopology) - Demonstrates the workflow of getting the network state and validating the topology of a utility network.
