# Table of contents

## Analysis

* [Distance measurement analysis](Maui.Samples/Samples/Analysis/DistanceMeasurement) - Measure distances between two points in 3D.
* [Line of sight (geoelement)](Maui.Samples/Samples/Analysis/LineOfSightGeoElement) - Show a line of sight between two moving objects.
* [Line of sight (location)](Maui.Samples/Samples/Analysis/LineOfSightLocation) - Perform a line of sight analysis between two points in real time.
* [Query feature count and extent](Maui.Samples/Samples/Analysis/QueryFeatureCountAndExtent) - Zoom to features matching a query and count the features in the current visible extent.
* [Viewshed (location)](Maui.Samples/Samples/Analysis/ViewshedLocation) - Perform a viewshed analysis from a defined vantage point.
* [Viewshed for GeoElement](Maui.Samples/Samples/Analysis/ViewshedGeoElement) - Analyze the viewshed for an object (GeoElement) in a scene.
* [Viewshed for camera](Maui.Samples/Samples/Analysis/ViewshedCamera) - Analyze the viewshed for a camera. A viewshed shows the visible and obstructed areas from an observer's vantage point.

## Data

* [Add features with contingent values](Maui.Samples/Samples/Data/AddFeaturesWithContingentValues) - Create and add features whose attribute values satisfy a predefined set of contingencies.
* [Create mobile geodatabase](Maui.Samples/Samples/Data/CreateMobileGeodatabase) - Create and share a mobile geodatabase.
* [Edit and sync features](Maui.Samples/Samples/Data/EditAndSyncFeatures) - Synchronize offline edits with a feature service.
* [Edit feature attachments](Maui.Samples/Samples/Data/EditFeatureAttachments) - Add, delete, and download attachments for features from a service.
* [Edit features with feature-linked annotation](Maui.Samples/Samples/Data/EditFeatureLinkedAnnotation) - Edit feature attributes which are linked to annotation through an expression.
* [Edit with branch versioning](Maui.Samples/Samples/Data/EditBranchVersioning) - Create, query and edit a specific server version using service geodatabase.
* [Feature layer query](Maui.Samples/Samples/Data/FeatureLayerQuery) - Find features in a feature table which match an SQL query.
* [Generate geodatabase replica from feature service](Maui.Samples/Samples/Data/GenerateGeodatabaseReplica) - Generate a local geodatabase from an online feature service.
* [Geodatabase transactions](Maui.Samples/Samples/Data/GeodatabaseTransactions) - Use transactions to manage how changes are committed to a geodatabase.
* [List related features](Maui.Samples/Samples/Data/ListRelatedFeatures) - List features related to the selected feature.
* [Manage features](Maui.Samples/Samples/Data/ManageFeatures) - Manage a feature layer's features in four distinct ways.
* [Query features with Arcade expression](Maui.Samples/Samples/Data/QueryFeaturesWithArcadeExpression) - Query features on a map using an Arcade expression.
* [Raster layer (GeoPackage)](Maui.Samples/Samples/Data/RasterLayerGeoPackage) - Display a raster contained in a GeoPackage.
* [Read GeoPackage](Maui.Samples/Samples/Data/ReadGeoPackage) - Add rasters and feature tables from a GeoPackage to a map.
* [Read shapefile metadata](Maui.Samples/Samples/Data/ReadShapefileMetadata) - Read a shapefile and display its metadata.
* [Statistical query](Maui.Samples/Samples/Data/StatisticalQuery) - Query a table to get aggregated statistics back for a specific field.
* [Statistical query group and sort](Maui.Samples/Samples/Data/StatsQueryGroupAndSort) - Query a feature table for statistics, grouping and sorting by different fields.
* [Symbolize shapefile](Maui.Samples/Samples/Data/SymbolizeShapefile) - Display a shapefile with custom symbology.
* [Toggle between feature request modes](Maui.Samples/Samples/Data/ToggleBetweenFeatureRequestModes) - Use different feature request modes to populate the map from a service feature table.
* [View point cloud data offline](Maui.Samples/Samples/Data/ViewPointCloudDataOffline) - Display local 3D point cloud data.

## Geometry

* [Buffer](Maui.Samples/Samples/Geometry/Buffer) - Create a buffer around a map point and display the results as a `Graphic`
* [Buffer list](Maui.Samples/Samples/Geometry/BufferList) - Generate multiple individual buffers or a single unioned buffer around multiple points.
* [Clip geometry](Maui.Samples/Samples/Geometry/ClipGeometry) - Clip a geometry with another geometry.
* [Convex hull](Maui.Samples/Samples/Geometry/ConvexHull) - Create a convex hull for a given set of points. The convex hull is a polygon with shortest perimeter that encloses a set of points. As a visual analogy, consider a set of points as nails in a board. The convex hull of the points would be like a rubber band stretched around the outermost nails.
* [Convex hull list](Maui.Samples/Samples/Geometry/ConvexHullList) - Generate convex hull polygon(s) from multiple input geometries.
* [Create geometries](Maui.Samples/Samples/Geometry/CreateGeometries) - Create simple geometry types.
* [Cut geometry](Maui.Samples/Samples/Geometry/CutGeometry) - Cut a geometry along a polyline.
* [Densify and generalize](Maui.Samples/Samples/Geometry/DensifyAndGeneralize) - A multipart geometry can be densified by adding interpolated points at regular intervals. Generalizing multipart geometry simplifies it while preserving its general shape. Densifying a multipart geometry adds more vertices at regular intervals.
* [Format coordinates](Maui.Samples/Samples/Geometry/FormatCoordinates) - Format coordinates in a variety of common notations.
* [Geodesic operations](Maui.Samples/Samples/Geometry/GeodesicOperations) - Calculate a geodesic path between two points and measure its distance.
* [List transformations by suitability](Maui.Samples/Samples/Geometry/ListTransformations) - Get a list of suitable transformations for projecting a geometry between two spatial references with different horizontal datums.
* [Nearest vertex](Maui.Samples/Samples/Geometry/NearestVertex) - Find the closest vertex and coordinate of a geometry to a point.
* [Perform spatial operations](Maui.Samples/Samples/Geometry/SpatialOperations) - Find the union, intersection, or difference of two geometries.
* [Project](Maui.Samples/Samples/Geometry/Project) - Project a point from one spatial reference to another.
* [Project with specific transformation](Maui.Samples/Samples/Geometry/ProjectWithSpecificTransformation) - Project a point from one coordinate system to another using a specific transformation step.
* [Spatial relationships](Maui.Samples/Samples/Geometry/SpatialRelationships) - Determine spatial relationships between two geometries.

## Geoprocessing

* [Analyze hotspots](Maui.Samples/Samples/Geoprocessing/AnalyzeHotspots) - Use a geoprocessing service and a set of features to identify statistically significant hot spots and cold spots.
* [Analyze viewshed (geoprocessing)](Maui.Samples/Samples/Geoprocessing/AnalyzeViewshed) - Calculate a viewshed using a geoprocessing service, in this case showing what parts of a landscape are visible from points on mountainous terrain.
* [List geodatabase versions](Maui.Samples/Samples/Geoprocessing/ListGeodatabaseVersions) - Connect to a service and list versions of the geodatabase.

## GraphicsOverlay

* [Add graphics with renderer](Maui.Samples/Samples/GraphicsOverlay/AddGraphicsRenderer) - A renderer allows you to change the style of all graphics in a graphics overlay by referencing a single symbol style. A renderer will only affect graphics that do not specify their own symbol style.
* [Add graphics with symbols](Maui.Samples/Samples/GraphicsOverlay/AddGraphicsWithSymbols) - Use a symbol style to display a graphic on a graphics overlay.
* [Animate 3D graphic](Maui.Samples/Samples/GraphicsOverlay/Animate3DGraphic) - An `OrbitGeoElementCameraController` follows a graphic while the graphic's position and rotation are animated.
* [Dictionary renderer with graphics overlay](Maui.Samples/Samples/GraphicsOverlay/DictionaryRendererGraphicsOverlay) - Create graphics from an XML file with key-value pairs for each graphic, and display the military symbols using a MIL-STD-2525D web style in 2D.
* [Identify graphics](Maui.Samples/Samples/GraphicsOverlay/IdentifyGraphics) - Display an alert message when a graphic is clicked.
* [Scene properties expressions](Maui.Samples/Samples/GraphicsOverlay/ScenePropertiesExpressions) - Update the orientation of a graphic using expressions based on its attributes.
* [Sketch on map](Maui.Samples/Samples/GraphicsOverlay/SketchOnMap) - Use the Sketch Editor to edit or sketch a new point, line, or polygon geometry on to a map.
* [Surface placement](Maui.Samples/Samples/GraphicsOverlay/SurfacePlacements) - Position graphics relative to a surface using different surface placement modes.

## Hydrography

* [Add ENC exchange set](Maui.Samples/Samples/Hydrography/AddEncExchangeSet) - Display nautical charts per the ENC specification.
* [Change ENC display settings](Maui.Samples/Samples/Hydrography/ChangeEncDisplaySettings) - Configure the display of ENC content.
* [Select ENC features](Maui.Samples/Samples/Hydrography/SelectEncFeatures) - Select features in an ENC layer.

## Layers

* [Add a point scene layer](Maui.Samples/Samples/Layers/AddPointSceneLayer) - View a point scene layer from a scene service.
* [Add integrated mesh layer](Maui.Samples/Samples/Layers/AddAnIntegratedMeshLayer) - View an integrated mesh layer from a scene service.
* [Add vector tiled layer from custom style](Maui.Samples/Samples/Layers/AddVectorTiledLayerFromCustomStyle) - Load ArcGIS vector tiled layers using custom styles.
* [Apply mosaic rule to rasters](Maui.Samples/Samples/Layers/ApplyMosaicRule) - Apply mosaic rule to a mosaic dataset of rasters.
* [Apply raster function to raster from service](Maui.Samples/Samples/Layers/RasterLayerRasterFunction) - Load a raster from a service, then apply a function to it.
* [ArcGIS map image layer](Maui.Samples/Samples/Layers/ArcGISMapImageLayerUrl) - Add an ArcGIS Map Image Layer from a URL to a map.
* [ArcGIS tiled layer](Maui.Samples/Samples/Layers/ArcGISTiledLayerUrl) - Load an ArcGIS tiled layer from a URL.
* [Blend renderer](Maui.Samples/Samples/Layers/ChangeBlendRenderer) - Blend a hillshade with a raster by specifying the elevation data. The resulting raster looks similar to the original raster, but with some terrain shading, giving it a textured look.
* [Browse OGC API feature service](Maui.Samples/Samples/Layers/BrowseOAFeatureService) - Browse an OGC API feature service for layers and add them to the map.
* [Browse WFS layers](Maui.Samples/Samples/Layers/BrowseWfsLayers) - Browse a WFS service for layers and add them to the map.
* [Change feature layer renderer](Maui.Samples/Samples/Layers/ChangeFeatureLayerRenderer) - Change the appearance of a feature layer with a renderer.
* [Change sublayer renderer](Maui.Samples/Samples/Layers/ChangeSublayerRenderer) - Apply a renderer to a sublayer.
* [Colormap renderer](Maui.Samples/Samples/Layers/RasterColormapRenderer) - Apply a colormap renderer to a raster.
* [Control annotation sublayer visibility](Maui.Samples/Samples/Layers/ControlAnnotationSublayerVisibility) - Use annotation sublayers to gain finer control of annotation layer subtypes.
* [Create and save KML file](Maui.Samples/Samples/Layers/CreateAndSaveKmlFile) - Construct a KML document and save it as a KMZ file.
* [Create feature collection layer (Portal item)](Maui.Samples/Samples/Layers/FeatureCollectionLayerFromPortal) - Create a feature collection layer from a portal item.
* [Dictionary renderer with feature layer](Maui.Samples/Samples/Layers/FeatureLayerDictionaryRenderer) - Convert features into graphics to show them with mil2525d symbols.
* [Display KML](Maui.Samples/Samples/Layers/DisplayKml) - Display KML from a URL, portal item, or local KML file.
* [Display KML network links](Maui.Samples/Samples/Layers/DisplayKmlNetworkLinks) - Display a file with a KML network link, including displaying any network link control messages at launch.
* [Display OGC API collection](Maui.Samples/Samples/Layers/DisplayOACollection) - Display an OGC API feature collection and query features while navigating the map view.
* [Display WFS layer](Maui.Samples/Samples/Layers/DisplayWfs) - Display a layer from a WFS service, requesting only features for the current extent.
* [Display a scene](Maui.Samples/Samples/Layers/DisplayScene) - Display a scene with a terrain surface and some imagery.
* [Display annotation](Maui.Samples/Samples/Layers/DisplayAnnotation) - Display annotation from a feature service URL.
* [Display dimensions](Maui.Samples/Samples/Layers/DisplayDimensions) - Display dimension features from a mobile map package.
* [Display feature layers](Maui.Samples/Samples/Layers/DisplayFeatureLayers) - Display feature layers from various data sources.
* [Display route layer](Maui.Samples/Samples/Layers/DisplayRouteLayer) - Display a route layer and its directions using a feature collection.
* [Display subtype feature layer](Maui.Samples/Samples/Layers/DisplaySubtypeFeatureLayer) - Displays a composite layer of all the subtype values in a feature class.
* [Edit KML ground overlay](Maui.Samples/Samples/Layers/EditKmlGroundOverlay) - Edit the values of a KML ground overlay.
* [Export tiles](Maui.Samples/Samples/Layers/ExportTiles) - Download tiles to a local tile cache file stored on the device.
* [Export vector tiles](Maui.Samples/Samples/Layers/ExportVectorTiles) - Export tiles from an online vector tile service.
* [Feature collection layer](Maui.Samples/Samples/Layers/CreateFeatureCollectionLayer) - Create a Feature Collection Layer from a Feature Collection Table, and add it to a map.
* [Feature collection layer (query)](Maui.Samples/Samples/Layers/FeatureCollectionLayerFromQuery) - Create a feature collection layer to show a query result from a service feature table.
* [Feature layer (feature service)](Maui.Samples/Samples/Layers/FeatureLayerUrl) - Show features from an online feature service.
* [Feature layer rendering mode (map)](Maui.Samples/Samples/Layers/FeatureLayerRenderingModeMap) - Render features statically or dynamically by setting the feature layer rendering mode.
* [Feature layer rendering mode (scene)](Maui.Samples/Samples/Layers/FeatureLayerRenderingModeScene) - Render features in a scene statically or dynamically by setting the feature layer rendering mode.
* [Feature layer selection](Maui.Samples/Samples/Layers/FeatureLayerSelection) - Select features in a feature layer.
* [Filter by definition expression or display filter](Maui.Samples/Samples/Layers/FeatureLayerDefinitionExpression) - Filter features displayed on a map using a definition expression or a display filter.
* [Group layers](Maui.Samples/Samples/Layers/GroupLayers) - Group a collection of layers together and toggle their visibility as a group.
* [Hillshade renderer](Maui.Samples/Samples/Layers/RasterHillshade) - Apply a hillshade renderer to a raster.
* [Identify KML features](Maui.Samples/Samples/Layers/IdentifyKmlFeatures) - Show a callout with formatted content for a KML feature.
* [Identify WMS features](Maui.Samples/Samples/Layers/WmsIdentify) - Identify features in a WMS layer and display the associated popup content.
* [Identify raster cell](Maui.Samples/Samples/Layers/IdentifyRasterCell) - Get the cell value of a local raster at the tapped location and display the result in a callout.
* [List KML contents](Maui.Samples/Samples/Layers/ListKmlContents) - List the contents of a KML file.
* [Load WFS with XML query](Maui.Samples/Samples/Layers/WfsXmlQuery) - Load a WFS feature table using an XML query.
* [Map image layer sublayer visibility](Maui.Samples/Samples/Layers/ChangeSublayerVisibility) - Change the visibility of sublayers.
* [Map image layer tables](Maui.Samples/Samples/Layers/MapImageLayerTables) - Find features in a spatial table related to features in a non-spatial table.
* [OpenStreetMap layer](Maui.Samples/Samples/Layers/OpenStreetMapLayer) - Add OpenStreetMap as a basemap layer.
* [Play KML tour](Maui.Samples/Samples/Layers/PlayKmlTours) - Play tours in KML files.
* [Query map image sublayer](Maui.Samples/Samples/Layers/MapImageSublayerQuery) - Find features in a sublayer based on attributes and location.
* [Query with CQL filters](Maui.Samples/Samples/Layers/QueryCQLFilters) - Query data from an OGC API feature service using CQL filters.
* [RGB renderer](Maui.Samples/Samples/Layers/RasterRgbRenderer) - Apply an RGB renderer to a raster layer to enhance feature visibility.
* [Raster layer (file)](Maui.Samples/Samples/Layers/RasterLayerFile) - Create and use a raster layer made from a local raster file.
* [Raster layer (service)](Maui.Samples/Samples/Layers/RasterLayerImageServiceRaster) - Create a raster layer from a raster image service.
* [Raster rendering rule](Maui.Samples/Samples/Layers/RasterRenderingRule) - Display a raster on a map and apply different rendering rules to that raster.
* [Scene layer (URL)](Maui.Samples/Samples/Layers/SceneLayerUrl) - Display an ArcGIS scene layer from a URL.
* [Scene layer selection](Maui.Samples/Samples/Layers/SceneLayerSelection) - Identify features in a scene to select.
* [Show labels on layers](Maui.Samples/Samples/Layers/ShowLabelsOnLayer) - Display custom labels on a feature layer.
* [Stretch renderer](Maui.Samples/Samples/Layers/ChangeStretchRenderer) - Use a stretch renderer to enhance the visual contrast of raster data for analysis.
* [Style WMS layers](Maui.Samples/Samples/Layers/StyleWmsLayer) - Change the style of a Web Map Service (WMS) layer.
* [Time-based query](Maui.Samples/Samples/Layers/TimeBasedQuery) - Query data using a time extent. 
* [WMS layer (URL)](Maui.Samples/Samples/Layers/WMSLayerUrl) - Display a WMS layer using a WMS service URL.
* [WMS service catalog](Maui.Samples/Samples/Layers/WmsServiceCatalog) - Connect to a WMS service and show the available layers and sublayers. 
* [WMTS layer](Maui.Samples/Samples/Layers/WMTSLayer) - Display a layer from a Web Map Tile Service.
* [Web tiled layer](Maui.Samples/Samples/Layers/LoadWebTiledLayer) - Display a tiled web layer.

## Location

* [Display device location with NMEA data sources](Maui.Samples/Samples/Location/LocationWithNMEA) - Parse NMEA sentences and use the results to show device location on the map.
* [Display device location with autopan modes](Maui.Samples/Samples/Location/DisplayDeviceLocation) - Display your current position on the map, as well as switch between different types of auto pan Modes.
* [Set up location-driven Geotriggers](Maui.Samples/Samples/Location/LocationDrivenGeotriggers) - Create a notification every time a given location data source has entered and/or exited a set of features or graphics.
* [Show device location using indoor positioning](Maui.Samples/Samples/Location/IndoorPositioning) - Show your device's real-time location while inside a building by using signals from indoor positioning beacons.
* [Show location history](Maui.Samples/Samples/Location/ShowLocationHistory) - Display your location history on the map.

## Map

* [Apply scheduled updates to preplanned map area](Maui.Samples/Samples/Map/ApplyScheduledUpdates) - Apply scheduled updates to a downloaded preplanned map area.
* [Browse building floors](Maui.Samples/Samples/Map/BrowseBuildingFloors) - Display and browse through building floors from a floor-aware web map.
* [Change basemap](Maui.Samples/Samples/Map/ChangeBasemap) - Change a map's basemap. A basemap is beneath all layers on a `Map` and is used to provide visual reference for the operational layers.
* [Create and save map](Maui.Samples/Samples/Map/AuthorMap) - Create and save a map as an ArcGIS `PortalItem` (i.e. web map).
* [Display map](Maui.Samples/Samples/Map/DisplayMap) - Display a map with an imagery basemap.
* [Display overview map](Maui.Samples/Samples/Map/DisplayOverviewMap) - Include an overview or inset map as an additional map view to show the wider context of the primary view. 
* [Download preplanned map area](Maui.Samples/Samples/Map/DownloadPreplannedMap) - Take a map offline using a preplanned map area.
* [Generate offline map](Maui.Samples/Samples/Map/GenerateOfflineMap) - Take a web map offline.
* [Generate offline map (overrides)](Maui.Samples/Samples/Map/GenerateOfflineMapWithOverrides) - Take a web map offline with additional options for each layer.
* [Generate offline map with local basemap](Maui.Samples/Samples/Map/OfflineBasemapByReference) - Use the `OfflineMapTask` to take a web map offline, but instead of downloading an online basemap, use one which is already on the device.
* [Honor mobile map package expiration date](Maui.Samples/Samples/Map/HonorMobileMapPackageExpiration) - Access the expiration information of an expired mobile map package.
* [Manage bookmarks](Maui.Samples/Samples/Map/ManageBookmarks) - Access and create bookmarks on a map.
* [Manage operational layers](Maui.Samples/Samples/Map/ManageOperationalLayers) - Add, remove, and reorder operational layers in a map.
* [Map initial extent](Maui.Samples/Samples/Map/SetInitialMapArea) - Display the map at an initial viewpoint representing a bounding geometry.
* [Map load status](Maui.Samples/Samples/Map/AccessLoadStatus) - Determine the map's load status which can be: `NotLoaded`, `FailedToLoad`, `Loading`, `Loaded`.
* [Map reference scale](Maui.Samples/Samples/Map/MapReferenceScale) - Set the map's reference scale and which feature layers should honor the reference scale.
* [Map spatial reference](Maui.Samples/Samples/Map/SetMapSpatialReference) - Specify a map's spatial reference.
* [Mobile map (search and route)](Maui.Samples/Samples/Map/MobileMapSearchAndRoute) - Display maps and use locators to enable search and routing offline using a Mobile Map Package.
* [Open map URL](Maui.Samples/Samples/Map/OpenMapURL) - Display a web map.
* [Open mobile map package](Maui.Samples/Samples/Map/OpenMobileMap) - Display a map from a mobile map package.
* [Search for webmap](Maui.Samples/Samples/Map/SearchPortalMaps) - Find webmap portal items by using a search term.
* [Set initial map location](Maui.Samples/Samples/Map/SetInitialMapLocation) - Display a basemap centered at an initial location and scale.
* [Set max extent](Maui.Samples/Samples/Map/SetMaxExtent) - Limit the view of a map to a particular area.
* [Set min & max scale](Maui.Samples/Samples/Map/SetMinMaxScale) - Restrict zooming between specific scale ranges.

## MapView

* [Change time extent](Maui.Samples/Samples/MapView/ChangeTimeExtent) - Filter data in layers by applying a time extent to a MapView.
* [Change viewpoint](Maui.Samples/Samples/MapView/ChangeViewpoint) - Set the map view to a new viewpoint.
* [Display draw status](Maui.Samples/Samples/MapView/DisplayDrawingStatus) - Get the draw status of your map view or scene view to know when all layers in the map or scene have finished drawing.
* [Display grid](Maui.Samples/Samples/MapView/DisplayGrid) - Display coordinate system grids including Latitude/Longitude, MGRS, UTM and USNG on a map view. Also, toggle label visibility and change the color of grid lines and grid labels.
* [Display layer view state](Maui.Samples/Samples/MapView/DisplayLayerViewState) - Determine if a layer is currently being viewed.
* [Feature layer time offset](Maui.Samples/Samples/MapView/FeatureLayerTimeOffset) - Display a time-enabled feature layer with a time offset.
* [Filter by time extent](Maui.Samples/Samples/MapView/FilterByTimeExtent) - The time slider provides controls that allow you to visualize temporal data by applying a specific time extent to a map view.
* [Identify layers](Maui.Samples/Samples/MapView/IdentifyLayers) - Identify features in all layers in a map.
* [Map rotation](Maui.Samples/Samples/MapView/MapRotation) - Rotate a map.
* [Show callout](Maui.Samples/Samples/MapView/ShowCallout) - Show a callout with the latitude and longitude of user-tapped points.
* [Show magnifier](Maui.Samples/Samples/MapView/ShowMagnifier) - Tap and hold on a map to show a magnifier.
* [Take screenshot](Maui.Samples/Samples/MapView/TakeScreenshot) - Take a screenshot of the map.

## Network analysis

* [Find closest facility to an incident (interactive)](Maui.Samples/Samples/NetworkAnalysis/ClosestFacility) - Find a route to the closest facility from a location.
* [Find closest facility to multiple incidents (service)](Maui.Samples/Samples/NetworkAnalysis/ClosestFacilityStatic) - Find routes from several locations to the respective closest facility.
* [Find route](Maui.Samples/Samples/NetworkAnalysis/FindRoute) - Display directions for a route between two points.
* [Find service area](Maui.Samples/Samples/NetworkAnalysis/FindServiceArea) - Find the service area within a network from a given point.
* [Find service areas for multiple facilities](Maui.Samples/Samples/NetworkAnalysis/FindServiceAreasForMultipleFacilities) - Find the service areas of several facilities from a feature service.
* [Navigate route](Maui.Samples/Samples/NetworkAnalysis/NavigateRoute) - Use a routing service to navigate between points.
* [Navigate route with rerouting](Maui.Samples/Samples/NetworkAnalysis/NavigateRouteRerouting) - Navigate between two points and dynamically recalculate an alternate route when the original route is unavailable.
* [Offline routing](Maui.Samples/Samples/NetworkAnalysis/OfflineRouting) - Solve a route on-the-fly using offline data.
* [Route around barriers](Maui.Samples/Samples/NetworkAnalysis/RouteAroundBarriers) - Find a route that reaches all stops without crossing any barriers.

## Scene

* [Change atmosphere effect](Maui.Samples/Samples/Scene/ChangeAtmosphereEffect) - Changes the appearance of the atmosphere in a scene.
* [Create terrain from local tile package](Maui.Samples/Samples/Scene/CreateTerrainSurfaceTilePackage) - Set the terrain surface with elevation described by a local tile package.
* [Create terrain surface from a local raster](Maui.Samples/Samples/Scene/CreateTerrainSurfaceRaster) - Set the terrain surface with elevation described by a raster file.
* [Get elevation at a point](Maui.Samples/Samples/Scene/GetElevationAtPoint) - Get the elevation for a given point on a surface in a scene.
* [Open mobile scene package](Maui.Samples/Samples/Scene/OpenMobileScenePackage) - Opens and displays a scene from a Mobile Scene Package (.mspk).
* [Open scene (portal item)](Maui.Samples/Samples/Scene/OpenScenePortalItem) - Open a web scene from a portal item.
* [Show labels on layer 3D](Maui.Samples/Samples/Scene/ShowLabelsOnLayer3D) - Display custom labels in a 3D scene.
* [Terrain exaggeration](Maui.Samples/Samples/Scene/TerrainExaggeration) - Vertically exaggerate terrain in a scene.
* [View content beneath terrain surface](Maui.Samples/Samples/Scene/ViewContentBeneathSurface) - See through terrain in a scene and move the camera underground.

## SceneView

* [Animate images with image overlay](Maui.Samples/Samples/SceneView/AnimateImageOverlay) - Animate a series of images with an image overlay.
* [Choose camera controller](Maui.Samples/Samples/SceneView/ChooseCameraController) - Control the behavior of the camera in a scene.
* [GeoView viewpoint synchronization](Maui.Samples/Samples/SceneView/GeoViewSync) - Keep the view points of two views (e.g. MapView and SceneView) synchronized with each other.

## Search

* [Find address](Maui.Samples/Samples/Search/FindAddress) - Find the location for an address.
* [Find place](Maui.Samples/Samples/Search/FindPlace) - Find places of interest near a location or within a specific area.
* [Offline geocode](Maui.Samples/Samples/Search/OfflineGeocode) - Geocode addresses to locations and reverse geocode locations to addresses offline.
* [Reverse geocode](Maui.Samples/Samples/Search/ReverseGeocode) - Use an online service to find the address for a tapped point.

## Security

* [ArcGIS token challenge](Maui.Samples/Samples/Security/TokenSecuredChallenge) - This sample demonstrates how to prompt the user for a username and password to authenticate with ArcGIS Server to access an ArcGIS token-secured service. Accessing secured services requires a login that's been defined on the server.
* [Authenticate with OAuth](Maui.Samples/Samples/Security/OAuth) - Authenticate with ArcGIS Online (or your own portal) using OAuth2 to access secured resources (such as private web maps or layers).

## Symbology

* [Apply unique values with alternate symbols](Maui.Samples/Samples/Symbology/UniqueValuesAlternateSymbols) - Apply a unique value with alternate symbols at different scales.
* [Create symbol styles from web styles](Maui.Samples/Samples/Symbology/SymbolStylesFromWebStyles) - Create symbol styles from a style file hosted on a portal.
* [Custom dictionary style](Maui.Samples/Samples/Symbology/CustomDictionaryStyle) - Use a custom dictionary created from a web style or style file (.stylx) to symbolize features using a variety of attribute values.
* [Distance composite scene symbol](Maui.Samples/Samples/Symbology/UseDistanceCompositeSym) - Change a graphic's symbol based on the camera's proximity to it.
* [Feature layer extrusion](Maui.Samples/Samples/Symbology/FeatureLayerExtrusion) - Extrude features based on their attributes.
* [Picture marker symbol](Maui.Samples/Samples/Symbology/RenderPictureMarkers) - Use pictures for markers.
* [Read symbols from mobile style](Maui.Samples/Samples/Symbology/SymbolsFromMobileStyle) - Combine multiple symbols from a mobile style file into a single symbol.
* [Render multilayer symbols](Maui.Samples/Samples/Symbology/RenderMultilayerSymbols) - Show different kinds of multilayer symbols on a map similar to some pre-defined 2D simple symbol styles.
* [Scene symbols](Maui.Samples/Samples/Symbology/SceneSymbols) - Show various kinds of 3D symbols in a scene.
* [Simple marker symbol](Maui.Samples/Samples/Symbology/RenderSimpleMarkers) - Show a simple marker symbol on a map.
* [Simple renderer](Maui.Samples/Samples/Symbology/SimpleRenderers) - Display common symbols for all graphics in a graphics overlay with a renderer.
* [Unique value renderer](Maui.Samples/Samples/Symbology/RenderUniqueValues) - Render features in a layer using a distinct symbol for each unique attribute value.

## Utility network

* [Configure subnetwork trace](Maui.Samples/Samples/UtilityNetwork/ConfigureSubnetworkTrace) - Get a server-defined trace configuration for a given tier and modify its traversability scope, add new condition barriers and control what is included in the subnetwork trace result.
* [Create load report](Maui.Samples/Samples/UtilityNetwork/CreateLoadReport) - Demonstrates the creation of a simple electric distribution report. It traces downstream from a given point and adds up the count of customers and total load per phase.
* [Display content of utility network container](Maui.Samples/Samples/UtilityNetwork/DisplayUtilityNetworkContainer) - A utility network container allows a dense collection of features to be represented by a single feature, which can be used to reduce map clutter.
* [Display utility associations](Maui.Samples/Samples/UtilityNetwork/DisplayUtilityAssociations) - Create graphics for utility associations in a utility network.
* [Perform valve isolation trace](Maui.Samples/Samples/UtilityNetwork/PerformValveIsolationTrace) - Run a filtered trace to locate operable features that will isolate an area from the flow of network resources.
* [Trace utility network](Maui.Samples/Samples/UtilityNetwork/TraceUtilityNetwork) - Discover connected features in a utility network using connected, subnetwork, upstream, and downstream traces.
