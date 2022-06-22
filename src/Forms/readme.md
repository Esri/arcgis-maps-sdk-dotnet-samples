# Table of contents

## Analysis

* [Distance measurement analysis](Shared/Samples/Analysis/DistanceMeasurement) - Measure distances between two points in 3D.
* [Line of sight (geoelement)](Shared/Samples/Analysis/LineOfSightGeoElement) - Show a line of sight between two moving objects.
* [Line of sight (location)](Shared/Samples/Analysis/LineOfSightLocation) - Perform a line of sight analysis between two points in real time.
* [Query feature count and extent](Shared/Samples/Analysis/QueryFeatureCountAndExtent) - Zoom to features matching a query and count the features in the current visible extent.
* [Viewshed for camera](Shared/Samples/Analysis/ViewshedCamera) - Analyze the viewshed for a camera. A viewshed shows the visible and obstructed areas from an observer's vantage point. 
* [Viewshed for GeoElement](Shared/Samples/Analysis/ViewshedGeoElement) - Analyze the viewshed for an object (GeoElement) in a scene.
* [Viewshed (location)](Shared/Samples/Analysis/ViewshedLocation) - Perform a viewshed analysis from a defined vantage point.

## Data

* [Add features](Shared/Samples/Data/AddFeatures) - Add features to a feature layer.
* [Add features with contingent values](Shared/Samples/Data/AddFeaturesWithContingentValues) - Create and add features whose attribute values satisfy a predefined set of contingencies.
* [Delete features (feature service)](Shared/Samples/Data/DeleteFeatures) - Delete features from an online feature service.
* [Edit and sync features](Shared/Samples/Data/EditAndSyncFeatures) - Synchronize offline edits with a feature service.
* [Edit with branch versioning](Shared/Samples/Data/EditBranchVersioning) - Create, query and edit a specific server version using service geodatabase.
* [Edit feature attachments](Shared/Samples/Data/EditFeatureAttachments) - Add, delete, and download attachments for features from a service.
* [Edit features with feature-linked annotation](Shared/Samples/Data/EditFeatureLinkedAnnotation) - Edit feature attributes which are linked to annotation through an expression.
* [Feature layer query](Shared/Samples/Data/FeatureLayerQuery) - Find features in a feature table which match an SQL query.
* [Generate geodatabase](Shared/Samples/Data/GenerateGeodatabase) - Generate a local geodatabase from an online feature service.
* [Geodatabase transactions](Shared/Samples/Data/GeodatabaseTransactions) - Use transactions to manage how changes are committed to a geodatabase.
* [List related features](Shared/Samples/Data/ListRelatedFeatures) - List features related to the selected feature.
* [Query features with Arcade expression](Shared/Samples/Data/QueryFeaturesWithArcadeExpression) - Query features on a map using an Arcade expression.
* [Raster layer (GeoPackage)](Shared/Samples/Data/RasterLayerGeoPackage) - Display a raster contained in a GeoPackage.
* [Read GeoPackage](Shared/Samples/Data/ReadGeoPackage) - Add rasters and feature tables from a GeoPackage to a map.
* [Read shapefile metadata](Shared/Samples/Data/ReadShapefileMetadata) - Read a shapefile and display its metadata.
* [Statistical query](Shared/Samples/Data/StatisticalQuery) - Query a table to get aggregated statistics back for a specific field.
* [Statistical query group and sort](Shared/Samples/Data/StatsQueryGroupAndSort) - Query a feature table for statistics, grouping and sorting by different fields.
* [Symbolize shapefile](Shared/Samples/Data/SymbolizeShapefile) - Display a shapefile with custom symbology.
* [Toggle between feature request modes](Shared/Samples/Data/ToggleBetweenFeatureRequestModes) - Use different feature request modes to populate the map from a service feature table.
* [Update attributes (feature service)](Shared/Samples/Data/UpdateAttributes) - Update feature attributes in an online feature service.
* [Update geometries (feature service)](Shared/Samples/Data/UpdateGeometries) - Update a feature's location in an online feature service.
* [View point cloud data offline](Shared/Samples/Data/ViewPointCloudDataOffline) - Display local 3D point cloud data.

## Geometry

* [Buffer](Shared/Samples/Geometry/Buffer) - Create a buffer around a map point and display the results as a `Graphic`
* [Buffer list](Shared/Samples/Geometry/BufferList) - Generate multiple individual buffers or a single unioned buffer around multiple points.
* [Clip geometry](Shared/Samples/Geometry/ClipGeometry) - Clip a geometry with another geometry.
* [Convex hull](Shared/Samples/Geometry/ConvexHull) - Create a convex hull for a given set of points. The convex hull is a polygon with shortest perimeter that encloses a set of points. As a visual analogy, consider a set of points as nails in a board. The convex hull of the points would be like a rubber band stretched around the outermost nails.
* [Convex hull list](Shared/Samples/Geometry/ConvexHullList) - Generate convex hull polygon(s) from multiple input geometries.
* [Create geometries](Shared/Samples/Geometry/CreateGeometries) - Create simple geometry types.
* [Cut geometry](Shared/Samples/Geometry/CutGeometry) - Cut a geometry along a polyline.
* [Densify and generalize](Shared/Samples/Geometry/DensifyAndGeneralize) - A multipart geometry can be densified by adding interpolated points at regular intervals. Generalizing multipart geometry simplifies it while preserving its general shape. Densifying a multipart geometry adds more vertices at regular intervals.
* [Format coordinates](Shared/Samples/Geometry/FormatCoordinates) - Format coordinates in a variety of common notations.
* [Geodesic operations](Shared/Samples/Geometry/GeodesicOperations) - Calculate a geodesic path between two points and measure its distance.
* [List transformations by suitability](Shared/Samples/Geometry/ListTransformations) - Get a list of suitable transformations for projecting a geometry between two spatial references with different horizontal datums.
* [Nearest vertex](Shared/Samples/Geometry/NearestVertex) - Find the closest vertex and coordinate of a geometry to a point.
* [Project](Shared/Samples/Geometry/Project) - Project a point from one spatial reference to another.
* [Project with specific transformation](Shared/Samples/Geometry/ProjectWithSpecificTransformation) - Project a point from one coordinate system to another using a specific transformation step.
* [Perform spatial operations](Shared/Samples/Geometry/SpatialOperations) - Find the union, intersection, or difference of two geometries.
* [Spatial relationships](Shared/Samples/Geometry/SpatialRelationships) - Determine spatial relationships between two geometries.

## Geoprocessing

* [Analyze hotspots](Shared/Samples/Geoprocessing/AnalyzeHotspots) - Use a geoprocessing service and a set of features to identify statistically significant hot spots and cold spots.
* [Analyze viewshed (geoprocessing)](Shared/Samples/Geoprocessing/AnalyzeViewshed) - Calculate a viewshed using a geoprocessing service, in this case showing what parts of a landscape are visible from points on mountainous terrain.
* [List geodatabase versions](Shared/Samples/Geoprocessing/ListGeodatabaseVersions) - Connect to a service and list versions of the geodatabase.

## GraphicsOverlay

* [Add graphics with renderer](Shared/Samples/GraphicsOverlay/AddGraphicsRenderer) - A renderer allows you to change the style of all graphics in a graphics overlay by referencing a single symbol style. A renderer will only affect graphics that do not specify their own symbol style.
* [Add graphics with symbols](Shared/Samples/GraphicsOverlay/AddGraphicsWithSymbols) - Use a symbol style to display a graphic on a graphics overlay.
* [Animate 3D graphic](Shared/Samples/GraphicsOverlay/Animate3DGraphic) - An `OrbitGeoElementCameraController` follows a graphic while the graphic's position and rotation are animated.
* [Dictionary renderer with graphics overlay](Shared/Samples/GraphicsOverlay/DictionaryRendererGraphicsOverlay) - Create graphics from an XML file with key-value pairs for each graphic, and display the military symbols using a MIL-STD-2525D web style in 2D.
* [Identify graphics](Shared/Samples/GraphicsOverlay/IdentifyGraphics) - Display an alert message when a graphic is clicked.
* [Scene properties expressions](Shared/Samples/GraphicsOverlay/ScenePropertiesExpressions) - Update the orientation of a graphic using expressions based on its attributes.
* [Sketch on map](Shared/Samples/GraphicsOverlay/SketchOnMap) - Use the Sketch Editor to edit or sketch a new point, line, or polygon geometry on to a map.
* [Surface placement](Shared/Samples/GraphicsOverlay/SurfacePlacements) - Position graphics relative to a surface using different surface placement modes.

## Hydrography

* [Add ENC exchange set](Shared/Samples/Hydrography/AddEncExchangeSet) - Display nautical charts per the ENC specification.
* [Change ENC display settings](Shared/Samples/Hydrography/ChangeEncDisplaySettings) - Configure the display of ENC content.
* [Select ENC features](Shared/Samples/Hydrography/SelectEncFeatures) - Select features in an ENC layer.

## Layers

* [Add an integrated mesh layer](Shared/Samples/Layers/AddAnIntegratedMeshLayer) - View an integrated mesh layer from a scene service.
* [Add a point scene layer](Shared/Samples/Layers/AddPointSceneLayer) - View a point scene layer from a scene service.
* [Apply mosaic rule to rasters](Shared/Samples/Layers/ApplyMosaicRule) - Apply mosaic rule to a mosaic dataset of rasters.
* [ArcGIS map image layer](Shared/Samples/Layers/ArcGISMapImageLayerUrl) - Add an ArcGIS Map Image Layer from a URL to a map.
* [ArcGIS tiled layer](Shared/Samples/Layers/ArcGISTiledLayerUrl) - Load an ArcGIS tiled layer from a URL.
* [ArcGIS vector tiled layer URL](Shared/Samples/Layers/ArcGISVectorTiledLayerUrl) - Load an ArcGIS Vector Tiled Layer from a URL.
* [Browse OGC API feature service](Shared/Samples/Layers/BrowseOAFeatureService) - Browse an OGC API feature service for layers and add them to the map.
* [Browse WFS layers](Shared/Samples/Layers/BrowseWfsLayers) - Browse a WFS service for layers and add them to the map.
* [Blend renderer](Shared/Samples/Layers/ChangeBlendRenderer) - Blend a hillshade with a raster by specifying the elevation data. The resulting raster looks similar to the original raster, but with some terrain shading, giving it a textured look.
* [Change feature layer renderer](Shared/Samples/Layers/ChangeFeatureLayerRenderer) - Change the appearance of a feature layer with a renderer.
* [Stretch renderer](Shared/Samples/Layers/ChangeStretchRenderer) - Use a stretch renderer to enhance the visual contrast of raster data for analysis.
* [Change sublayer renderer](Shared/Samples/Layers/ChangeSublayerRenderer) - Apply a renderer to a sublayer.
* [Map image layer sublayer visibility](Shared/Samples/Layers/ChangeSublayerVisibility) - Change the visibility of sublayers.
* [Control annotation sublayer visibility](Shared/Samples/Layers/ControlAnnotationSublayerVisibility) - Use annotation sublayers to gain finer control of annotation layer subtypes.
* [Create and save KML file](Shared/Samples/Layers/CreateAndSaveKmlFile) - Construct a KML document and save it as a KMZ file.
* [Feature collection layer](Shared/Samples/Layers/CreateFeatureCollectionLayer) - Create a Feature Collection Layer from a Feature Collection Table, and add it to a map.
* [Display annotation](Shared/Samples/Layers/DisplayAnnotation) - Display annotation from a feature service URL.
* [Display dimensions](Shared/Samples/Layers/DisplayDimensions) - Display dimension features from a mobile map package.
* [Display feature layers](Shared/Samples/Layers/DisplayFeatureLayers) - Display feature layers from various data sources.
* [Display KML](Shared/Samples/Layers/DisplayKml) - Display KML from a URL, portal item, or local KML file.
* [Display KML network links](Shared/Samples/Layers/DisplayKmlNetworkLinks) - Display a file with a KML network link, including displaying any network link control messages at launch.
* [Display OGC API collection](Shared/Samples/Layers/DisplayOACollection) - Display an OGC API feature collection and query features while navigating the map view.
* [Display a scene](Shared/Samples/Layers/DisplayScene) - Display a scene with a terrain surface and some imagery.
* [Display subtype feature layer](Shared/Samples/Layers/DisplaySubtypeFeatureLayer) - Displays a composite layer of all the subtype values in a feature class.
* [Display WFS layer](Shared/Samples/Layers/DisplayWfs) - Display a layer from a WFS service, requesting only features for the current extent.
* [Edit KML ground overlay](Shared/Samples/Layers/EditKmlGroundOverlay) - Edit the values of a KML ground overlay.
* [Export tiles](Shared/Samples/Layers/ExportTiles) - Download tiles to a local tile cache file stored on the device.
* [Export vector tiles](Shared/Samples/Layers/ExportVectorTiles) - Export tiles from an online vector tile service.
* [Create feature collection layer (Portal item)](Shared/Samples/Layers/FeatureCollectionLayerFromPortal) - Create a feature collection layer from a portal item.
* [Feature collection layer (query)](Shared/Samples/Layers/FeatureCollectionLayerFromQuery) - Create a feature collection layer to show a query result from a service feature table.
* [Filter by definition expression or display filter](Shared/Samples/Layers/FeatureLayerDefinitionExpression) - Filter features displayed on a map using a definition expression or a display filter.
* [Dictionary renderer with feature layer](Shared/Samples/Layers/FeatureLayerDictionaryRenderer) - Convert features into graphics to show them with mil2525d symbols.
* [Feature layer rendering mode (map)](Shared/Samples/Layers/FeatureLayerRenderingModeMap) - Render features statically or dynamically by setting the feature layer rendering mode.
* [Feature layer rendering mode (scene)](Shared/Samples/Layers/FeatureLayerRenderingModeScene) - Render features in a scene statically or dynamically by setting the feature layer rendering mode.
* [Feature layer selection](Shared/Samples/Layers/FeatureLayerSelection) - Select features in a feature layer.
* [Feature layer (feature service)](Shared/Samples/Layers/FeatureLayerUrl) - Show features from an online feature service.
* [Group layers](Shared/Samples/Layers/GroupLayers) - Group a collection of layers together and toggle their visibility as a group.
* [Identify KML features](Shared/Samples/Layers/IdentifyKmlFeatures) - Show a callout with formatted content for a KML feature.
* [Identify raster cell](Shared/Samples/Layers/IdentifyRasterCell) - Get the cell value of a local raster at the tapped location and display the result in a callout.
* [List KML contents](Shared/Samples/Layers/ListKmlContents) - List the contents of a KML file.
* [Web tiled layer](Shared/Samples/Layers/LoadWebTiledLayer) - Display a tiled web layer.
* [Map image layer tables](Shared/Samples/Layers/MapImageLayerTables) - Find features in a spatial table related to features in a non-spatial table.
* [Query map image sublayer](Shared/Samples/Layers/MapImageSublayerQuery) - Find features in a sublayer based on attributes and location.
* [OpenStreetMap layer](Shared/Samples/Layers/OpenStreetMapLayer) - Add OpenStreetMap as a basemap layer.
* [Play KML Tour](Shared/Samples/Layers/PlayKmlTours) - Play tours in KML files.
* [Query with CQL filters](Shared/Samples/Layers/QueryCQLFilters) - Query data from an OGC API feature service using CQL filters.
* [Colormap renderer](Shared/Samples/Layers/RasterColormapRenderer) - Apply a colormap renderer to a raster.
* [Raster hillshade renderer](Shared/Samples/Layers/RasterHillshade) - Use a hillshade renderer on a raster.
* [Raster layer (file)](Shared/Samples/Layers/RasterLayerFile) - Create and use a raster layer made from a local raster file.
* [Raster layer (service)](Shared/Samples/Layers/RasterLayerImageServiceRaster) - Create a raster layer from a raster image service.
* [Apply raster function to raster from service](Shared/Samples/Layers/RasterLayerRasterFunction) - Load a raster from a service, then apply a function to it.
* [Raster rendering rule](Shared/Samples/Layers/RasterRenderingRule) - Display a raster on a map and apply different rendering rules to that raster.
* [RGB renderer](Shared/Samples/Layers/RasterRgbRenderer) - Apply an RGB renderer to a raster layer to enhance feature visibility.
* [Scene layer selection](Shared/Samples/Layers/SceneLayerSelection) - Identify features in a scene to select.
* [Scene layer (URL)](Shared/Samples/Layers/SceneLayerUrl) - Display an ArcGIS scene layer from a URL.
* [Show labels on layers](Shared/Samples/Layers/ShowLabelsOnLayer) - Display custom labels on a feature layer.
* [Show popup](Shared/Samples/Layers/ShowPopup) - Show predefined popups from a web map.
* [Style WMS layers](Shared/Samples/Layers/StyleWmsLayer) - Change the style of a Web Map Service (WMS) layer.
* [Time-based query](Shared/Samples/Layers/TimeBasedQuery) - Query data using a time extent. 
* [Load WFS with XML query](Shared/Samples/Layers/WfsXmlQuery) - Load a WFS feature table using an XML query.
* [Identify WMS features](Shared/Samples/Layers/WmsIdentify) - Identify features in a WMS layer and display the associated popup content.
* [WMS layer (URL)](Shared/Samples/Layers/WMSLayerUrl) - Display a WMS layer using a WMS service URL.
* [WMS service catalog](Shared/Samples/Layers/WmsServiceCatalog) - Connect to a WMS service and show the available layers and sublayers. 
* [WMTS layer](Shared/Samples/Layers/WMTSLayer) - Display a layer from a Web Map Tile Service.

## Location

* [Display device location with autopan modes](Shared/Samples/Location/DisplayDeviceLocation) - Display your current position on the map, as well as switch between different types of auto pan Modes.
* [Set up location-driven Geotriggers](Shared/Samples/Location/LocationDrivenGeotriggers) - Create a notification every time a given location data source has entered and/or exited a set of features or graphics.
* [Display device location with NMEA data sources](Shared/Samples/Location/LocationWithNMEA) - Parse NMEA sentences and use the results to show device location on the map.
* [Show location history](Shared/Samples/Location/ShowLocationHistory) - Display your location history on the map.

## Map

* [Access load status](Shared/Samples/Map/AccessLoadStatus) - Determine the map's load status which can be: `NotLoaded`, `FailedToLoad`, `Loading`, `Loaded`.
* [Apply scheduled updates to preplanned map area](Shared/Samples/Map/ApplyScheduledUpdates) - Apply scheduled updates to a downloaded preplanned map area.
* [Create and save map](Shared/Samples/Map/AuthorMap) - Create and save a map as an ArcGIS `PortalItem` (i.e. web map).
* [Browse building floors](Shared/Samples/Map/BrowseBuildingFloors) - Display and browse through building floors from a floor-aware web map.
* [Change basemap](Shared/Samples/Map/ChangeBasemap) - Change a map's basemap. A basemap is beneath all layers on a `Map` and is used to provide visual reference for the operational layers.
* [Display map](Shared/Samples/Map/DisplayMap) - Display a map with an imagery basemap.
* [Display overview map](Shared/Samples/Map/DisplayOverviewMap) - Include an overview or inset map as an additional map view to show the wider context of the primary view. 
* [Download preplanned map area](Shared/Samples/Map/DownloadPreplannedMap) - Take a map offline using a preplanned map area.
* [Generate offline map](Shared/Samples/Map/GenerateOfflineMap) - Take a web map offline.
* [Generate offline map (overrides)](Shared/Samples/Map/GenerateOfflineMapWithOverrides) - Take a web map offline with additional options for each layer.
* [Honor mobile map package expiration date](Shared/Samples/Map/HonorMobileMapPackageExpiration) - Access the expiration information of an expired mobile map package.
* [Manage bookmarks](Shared/Samples/Map/ManageBookmarks) - Access and create bookmarks on a map.
* [Manage operational layers](Shared/Samples/Map/ManageOperationalLayers) - Add, remove, and reorder operational layers in a map.
* [Map reference scale](Shared/Samples/Map/MapReferenceScale) - Set the map's reference scale and which feature layers should honor the reference scale.
* [Mobile map (search and route)](Shared/Samples/Map/MobileMapSearchAndRoute) - Display maps and use locators to enable search and routing offline using a Mobile Map Package.
* [Generate offline map with local basemap](Shared/Samples/Map/OfflineBasemapByReference) - Use the `OfflineMapTask` to take a web map offline, but instead of downloading an online basemap, use one which is already on the device.
* [Open map URL](Shared/Samples/Map/OpenMapURL) - Display a web map.
* [Open mobile map package](Shared/Samples/Map/OpenMobileMap) - Display a map from a mobile map package.
* [Search for webmap](Shared/Samples/Map/SearchPortalMaps) - Find webmap portal items by using a search term.
* [Map initial extent](Shared/Samples/Map/SetInitialMapArea) - Display the map at an initial viewpoint representing a bounding geometry.
* [Set initial map location](Shared/Samples/Map/SetInitialMapLocation) - Display a basemap centered at an initial location and scale.
* [Map spatial reference](Shared/Samples/Map/SetMapSpatialReference) - Specify a map's spatial reference.
* [Set max extent](Shared/Samples/Map/SetMaxExtent) - Limit the view of a map to a particular area.
* [Set min & max scale](Shared/Samples/Map/SetMinMaxScale) - Restrict zooming between specific scale ranges.

## MapView

* [Change time extent](Shared/Samples/MapView/ChangeTimeExtent) - Filter data in layers by applying a time extent to a MapView.
* [Change viewpoint](Shared/Samples/MapView/ChangeViewpoint) - Set the map view to a new viewpoint.
* [Display draw status](Shared/Samples/MapView/DisplayDrawingStatus) - Get the draw status of your map view or scene view to know when all layers in the map or scene have finished drawing.
* [Display grid](Shared/Samples/MapView/DisplayGrid) - Display coordinate system grids including Latitude/Longitude, MGRS, UTM and USNG on a map view. Also, toggle label visibility and change the color of grid lines and grid labels.
* [Display layer view state](Shared/Samples/MapView/DisplayLayerViewState) - Determine if a layer is currently being viewed.
* [Feature layer time offset](Shared/Samples/MapView/FeatureLayerTimeOffset) - Display a time-enabled feature layer with a time offset.
* [Filter by time extent](Shared/Samples/MapView/FilterByTimeExtent) - The time slider provides controls that allow you to visualize temporal data by applying a specific time extent to a map view.
* [Identify layers](Shared/Samples/MapView/IdentifyLayers) - Identify features in all layers in a map. MapView supports identifying features across multiple layers. Because some layer types have sublayers, the sample recursively counts results for sublayers within each layer.
* [Map rotation](Shared/Samples/MapView/MapRotation) - Rotate a map.
* [Show callout](Shared/Samples/MapView/ShowCallout) - Show a callout with the latitude and longitude of user-tapped points.
* [Show magnifier](Shared/Samples/MapView/ShowMagnifier) - Tap and hold on a map to show a magnifier.
* [Take screenshot](Shared/Samples/MapView/TakeScreenshot) - Take a screenshot of the map.

## Network analysis

* [Find closest facility to an incident (interactive)](Shared/Samples/NetworkAnalysis/ClosestFacility) - Find a route to the closest facility from a location.
* [Find closest facility to multiple incidents (service)](Shared/Samples/NetworkAnalysis/ClosestFacilityStatic) - Find routes from several locations to the respective closest facility.
* [Find route](Shared/Samples/NetworkAnalysis/FindRoute) - Display directions for a route between two points.
* [Find service area](Shared/Samples/NetworkAnalysis/FindServiceArea) - Find the service area within a network from a given point.
* [Find service areas for multiple facilities](Shared/Samples/NetworkAnalysis/FindServiceAreasForMultipleFacilities) - Find the service areas of several facilities from a feature service.
* [Navigate route](Shared/Samples/NetworkAnalysis/NavigateRoute) - Use a routing service to navigate between points.
* [Navigate route with rerouting](Shared/Samples/NetworkAnalysis/NavigateRouteRerouting) - Navigate between two points and dynamically recalculate an alternate route when the original route is unavailable.
* [Offline routing](Shared/Samples/NetworkAnalysis/OfflineRouting) - Solve a route on-the-fly using offline data.
* [Route around barriers](Shared/Samples/NetworkAnalysis/RouteAroundBarriers) - Find a route that reaches all stops without crossing any barriers.

## Scene

* [Change atmosphere effect](Shared/Samples/Scene/ChangeAtmosphereEffect) - Changes the appearance of the atmosphere in a scene.
* [Create terrain surface from a local raster](Shared/Samples/Scene/CreateTerrainSurfaceRaster) - Set the terrain surface with elevation described by a raster file.
* [Create terrain from local tile package](Shared/Samples/Scene/CreateTerrainSurfaceTilePackage) - Set the terrain surface with elevation described by a local tile package.
* [Get elevation at a point](Shared/Samples/Scene/GetElevationAtPoint) - Get the elevation for a given point on a surface in a scene.
* [Open mobile scene package](Shared/Samples/Scene/OpenMobileScenePackage) - Opens and displays a scene from a Mobile Scene Package (.mspk).
* [Open scene (portal item)](Shared/Samples/Scene/OpenScenePortalItem) - Open a web scene from a portal item.
* [Terrain exaggeration](Shared/Samples/Scene/TerrainExaggeration) - Vertically exaggerate terrain in a scene.
* [View content beneath terrain surface](Shared/Samples/Scene/ViewContentBeneathSurface) - See through terrain in a scene and move the camera underground.

## SceneView

* [Animate images with image overlay](Shared/Samples/SceneView/AnimateImageOverlay) - Animate a series of images with an image overlay.
* [Choose camera controller](Shared/Samples/SceneView/ChooseCameraController) - Control the behavior of the camera in a scene.
* [GeoView viewpoint synchronization](Shared/Samples/SceneView/GeoViewSync) - Keep the view points of two views (e.g. MapView and SceneView) synchronized with each other.

## Search

* [Find address](Shared/Samples/Search/FindAddress) - Find the location for an address.
* [Find place](Shared/Samples/Search/FindPlace) - Find places of interest near a location or within a specific area.
* [Offline geocode](Shared/Samples/Search/OfflineGeocode) - Geocode addresses to locations and reverse geocode locations to addresses offline.
* [Reverse geocode](Shared/Samples/Search/ReverseGeocode) - Use an online service to find the address for a tapped point.

## Security

* [Authenticate with OAuth](Shared/Samples/Security/OAuth) - Authenticate with ArcGIS Online (or your own portal) using OAuth2 to access secured resources (such as private web maps or layers).
* [ArcGIS token challenge](Shared/Samples/Security/TokenSecuredChallenge) - This sample demonstrates how to prompt the user for a username and password to authenticate with ArcGIS Server to access an ArcGIS token-secured service. Accessing secured services requires a login that's been defined on the server.

## Symbology

* [Custom dictionary style](Shared/Samples/Symbology/CustomDictionaryStyle) - Use a custom dictionary created from a web style or style file (.stylx) to symbolize features using a variety of attribute values.
* [Feature layer extrusion](Shared/Samples/Symbology/FeatureLayerExtrusion) - Extrude features based on their attributes.
* [Render multilayer symbols](Shared/Samples/Symbology/RenderMultilayerSymbols) - Show different kinds of multilayer symbols on a map similar to some pre-defined 2D simple symbol styles.
* [Picture marker symbol](Shared/Samples/Symbology/RenderPictureMarkers) - Use pictures for markers.
* [Simple marker symbol](Shared/Samples/Symbology/RenderSimpleMarkers) - Show a simple marker symbol on a map.
* [Unique value renderer](Shared/Samples/Symbology/RenderUniqueValues) - Render features in a layer using a distinct symbol for each unique attribute value.
* [Scene symbols](Shared/Samples/Symbology/SceneSymbols) - Show various kinds of 3D symbols in a scene.
* [Simple renderer](Shared/Samples/Symbology/SimpleRenderers) - Display common symbols for all graphics in a graphics overlay with a renderer.
* [Read symbols from mobile style](Shared/Samples/Symbology/SymbolsFromMobileStyle) - Combine multiple symbols from a mobile style file into a single symbol.
* [Create symbol styles from web styles](Shared/Samples/Symbology/SymbolStylesFromWebStyles) - Create symbol styles from a style file hosted on a portal.
* [Apply unique values with alternate symbols](Shared/Samples/Symbology/UniqueValuesAlternateSymbols) - Apply a unique value with alternate symbols at different scales.
* [Distance composite scene symbol](Shared/Samples/Symbology/UseDistanceCompositeSym) - Change a graphic's symbol based on the camera's proximity to it.

## Utility network

* [Configure subnetwork trace](Shared/Samples/UtilityNetwork/ConfigureSubnetworkTrace) - Get a server-defined trace configuration for a given tier and modify its traversability scope, add new condition barriers and control what is included in the subnetwork trace result.
* [Display utility associations](Shared/Samples/UtilityNetwork/DisplayUtilityAssociations) - Create graphics for utility associations in a utility network.
* [Display content of utility network container](Shared/Samples/UtilityNetwork/DisplayUtilityNetworkContainer) - A utility network container allows a dense collection of features to be represented by a single feature, which can be used to reduce map clutter.
* [Perform valve isolation trace](Shared/Samples/UtilityNetwork/PerformValveIsolationTrace) - Run a filtered trace to locate operable features that will isolate an area from the flow of network resources.
* [Trace utility network](Shared/Samples/UtilityNetwork/TraceUtilityNetwork) - Discover connected features in a utility network using connected, subnetwork, upstream, and downstream traces.

