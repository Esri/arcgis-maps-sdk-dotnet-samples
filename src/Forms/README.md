## Sample Table Of Contents
## Maps


- **MapView**

    * [Change viewpoint](Shared/Samples/MapView/ChangeViewpoint)

    This sample demonstrates different ways in which you can change the viewpoint or visible area of the map.

    * [Map rotation](Shared/Samples/MapView/MapRotation)

    This sample illustrates how to rotate a map.

    * [Display drawing status](Shared/Samples/MapView/DisplayDrawingStatus)

    This sample demonstrates how to use the DrawStatus value of the MapView to notify user that the MapView is drawing.

    * [Display layer view state](Shared/Samples/MapView/DisplayLayerViewState)

    This sample demonstrates how to get view status for layers in a map.

    * [Take Screenshot](Shared/Samples/MapView/TakeScreenshot)

    This sample demonstrates how you can take screenshot of a map. Click 'capture' button to take a screenshot of the visible area of the map. Created image is shown in the sample after creation.

    * [Show magnifier](Shared/Samples/MapView/ShowMagnifier)

    This sample demonstrates how you can tap and hold on a map to get the magnifier. You can also pan while tapping and holding to move the magnifier across the map.

    * [Show callout](Shared/Samples/MapView/ShowCallout)

    This sample illustrates how to show callouts on a map in response to user interaction.


- **Map**

    * [Display a map](Shared/Samples/Map/DisplayMap)

    This sample demonstrates how to display a map with a basemap.

    * [Open Map (URL)](Shared/Samples/Map/OpenMapURL)

    This sample demonstrates how to open an existing map from a portal. The sample opens with a map displayed by default. You can change the shown map by selecting a new one from the populated list.

    * [Open mobile map (map package)](Shared/Samples/Map/OpenMobileMap)

    This sample demonstrates how to open a mobile map from a map package.

    * [Search a portal for maps](Shared/Samples/Map/SearchPortalMaps)

    This sample demonstrates searching a portal for web maps and loading them in the map view. You can search ArcGIS Online public web maps using tag values or browse the web maps in your account. OAuth is used to authenticate with ArcGIS Online to access items in your account.

    * [Set Min & Max Scale](Shared/Samples/Map/SetMinMaxScale)

    This sample demonstrates how to set the minimum and maximum scale of a Map. Setting the minimum and maximum scale for the Map can be useful in keeping the user focused at a certain level of detail.

    * [Access load status](Shared/Samples/Map/AccessLoadStatus)

    This sample demonstrates how to access the Maps' LoadStatus. The LoadStatus will be considered loaded when the following are true: The Map has a valid SpatialReference and the Map has an been set to the MapView.

    * [Set initial map location](Shared/Samples/Map/SetInitialMapLocation)

    This sample demonstrates how to create a map with a standard ESRI Imagery with Labels basemap that is centered on a latitude and longitude location and zoomed into a specific level of detail.

    * [Set initial map area](Shared/Samples/Map/SetInitialMapArea)

    This sample demonstrates how to set the initial viewpoint from envelope defined by minimum (x,y) and maximum (x,y) values. The map's InitialViewpoint is set to this viewpoint before the map is loaded into the MapView. Upon loading the map zoom to this initial area.

    * [Set map spatial reference](Shared/Samples/Map/SetMapSpatialReference)

    This sample demonstrates how you can set the spatial reference on a Map and all the operational layers would project accordingly.

    * [Change basemap](Shared/Samples/Map/ChangeBasemap)

    This sample demonstrates how to dynamically change the basemap displayed in a Map.

    * [Manage Bookmarks](Shared/Samples/Map/ManageBookmarks)

    This sample demonstrates how to access and add bookmarks to a map.

    * [Author and save a map](Shared/Samples/Map/AuthorMap)

    This sample demonstrates how to author and save a map as an ArcGIS portal item (web map). Saving a map to arcgis.com requires an ArcGIS Online login.

## Layers


- **Tiled Layers**

    * [ArcGIS tiled layer (URL)](Shared/Samples/Layers/ArcGISTiledLayerUrl)

    This sample demonstrates how to add an ArcGISTiledLayer as a base layer in a map. The ArcGISTiledLayer comes from an ArcGIS Server sample web service.

    * [ArcGIS vector tiled layer (URL)](Shared/Samples/Layers/ArcGISVectorTiledLayerUrl)

    This sample demonstrates how to add an ArcVectorGISTiledLayer as a base layer in a map.

    * [Export tiles](Shared/Samples/Layers/ExportTiles)

    This sample demonstrates how to export tiles from a map server.


- **Map Image Layers**

    * [ArcGIS map image layer (URL)](Shared/Samples/Layers/ArcGISMapImageLayerUrl)

    This sample demonstrates how to add an ArcGISMapImageLayer as a base layer in a map. The ArcGISMapImageLayer comes from an ArcGIS Server sample web service.

    * [Change sublayer visibility](Shared/Samples/Layers/ChangeSublayerVisibility)

    This sample demonstrates how to show or hide sublayers of a map image layer.

    * [WMTS layer](Shared/Samples/Layers/WMTSLayer)

    This sample demonstrates how to display a WMTS layer on a map via a Uri and WmtsLayerInfo.

    * [WMS layer (URL)](Shared/Samples/Layers/WMSLayerUrl)

    This sample demonstrates how to add a layer from a WMS service to a map.

    * [WMS service catalog](Shared/Samples/Layers/WmsServiceCatalog)

    This sample demonstrates how to enable and disable the display of layers discovered from a WMS service.


- **Raster Layers**

    * [ArcGIS raster layer (service)](Shared/Samples/Layers/RasterLayerImageServiceRaster)

    This sample demonstrates how to show a raster layer on a map based on an image service layer.

    * [ArcGIS raster function (service)](Shared/Samples/Layers/RasterLayerRasterFunction)

    This sample demonstrates how to show a raster layer on a map based on an image service layer that has a raster function applied.

    * [Raster layer (file)](Shared/Samples/Layers/RasterLayerFile)

    This sample demonstrates how to use a raster layer created from a local raster file.

    * [Raster rendering rule](Shared/Samples/Layers/RasterRenderingRule)

    This sample demonstrates how to create an `ImageServiceRaster`, fetch the `RenderingRule`s from the service info, and use a `RenderingRule` to create an `ImageServiceRaster` and add it to a raster layer.

    * [Raster layer (GeoPackage)](Shared/Samples/Data/RasterLayerGeoPackage)

    This sample demonstrates how to open a GeoPackage and show a GeoPackage raster in a raster layer.

## Features


- **Feature Layers**

    * [Feature layer (feature service)](Shared/Samples/Layers/FeatureLayerUrl)

    This sample demonstrates how to show a feature layer on a map using the URL to the service.

    * [Change feature layer renderer](Shared/Samples/Layers/ChangeFeatureLayerRenderer)

    This sample demonstrates how to change renderer for a feature layer. It also shows how to reset the renderer back to the default.

    * [Feature layer selection](Shared/Samples/Layers/FeatureLayerSelection)

    This sample demonstrates how to select features in a feature layer by tapping a MapView.

    * [Feature layer definition expression](Shared/Samples/Layers/FeatureLayerDefinitionExpression)

    This sample demonstrates how to apply definition expression to a feature layer for filtering features. It also shows how to reset the definition expression.

    * [Create a feature collection layer](Shared/Samples/Layers/CreateFeatureCollectionLayer)

    This sample demonstrates how to create a new feature collection with several feature collection tables. The collection is displayed in the map as a feature collection layer.

    * [Create a feature collection layer from a portal item](Shared/Samples/Layers/FeatureCollectionLayerFromPortal)

    This sample demonstrates opening a feature collection saved as a portal item.

    * [Feature collection layer from query result](Shared/Samples/Layers/FeatureCollectionLayerFromQuery)

    This sample demonstrates how to create a feature collection layer to show a query result from a service feature table.

    * [Time-based query](Shared/Samples/Layers/TimeBasedQuery)

    This sample demonstrates how to apply a time-based parameter to a feature layer query.

    * [Feature layer dictionary renderer](Shared/Samples/Layers/FeatureLayerDictionaryRenderer)

    Demonstrates how to apply a dictionary renderer to a feature layer and display mil2525d graphics. The dictionary renderer creates these graphics using a mil2525d style file and the attributes attached to each feature within the geodatabase.


- **Feature Tables**

    * [Service feature table (cache)](Shared/Samples/Data/ServiceFeatureTableCache)

    This sample demonstrates how to use a feature service in on interaction cache mode.

    * [Service feature table (no cache)](Shared/Samples/Data/ServiceFeatureTableNoCache)

    This sample demonstrates how to use a feature service in on interaction no cache mode.

    * [Service feature table (manual cache)](Shared/Samples/Data/ServiceFeatureTableManualCache)

    This sample demonstrates how to use a feature service in manual cache mode.

    * [Feature layer query](Shared/Samples/Data/FeatureLayerQuery)

    This sample demonstrates how to query a feature layer via feature table.

    * [Generate geodatabase](Shared/Samples/Data/GenerateGeodatabase)

    This sample demonstrates how to take a feature service offline by generating a geodatabase.

    * [Feature layer (shapefile)](Shared/Samples/Data/FeatureLayerShapefile)

    This sample demonstrates how to open a shapefile stored on the device and display it as a feature layer with default symbology.

    * [Edit and sync features](Shared/Samples/Data/EditAndSyncFeatures)

    This sample demonstrates how to synchronize offline edits with a feature service.

    * [Feature layer (GeoPackage)](Shared/Samples/Data/FeatureLayerGeoPackage)

    This sample demonstrates how to open a GeoPackage and show a GeoPackage feature table in a feature layer.

    * [Read shapefile metadata](Shared/Samples/Data/ReadShapefileMetadata)

    This sample demonstrates how to open a shapefile stored on the device, read metadata that describes the dataset, and display it as a feature layer with default symbology.

## Display Information


- **Graphics Overlays**

    * [Add graphics (SimpleRenderer)](Shared/Samples/GraphicsOverlay/AddGraphicsRenderer)

    This sample demonstrates how you add graphics and set a renderer on a graphic overlays.

    * [Identify graphics](Shared/Samples/GraphicsOverlay/IdentifyGraphics)

    This sample demonstrates how to identify graphics in a graphics overlay. When you tap on a graphic on the map, you will see an alert message displayed.

    * [Surface placement](Shared/Samples/GraphicsOverlay/SurfacePlacements)

    This sample demonstrates how to position graphics using different Surface Placements.

    * [Sketch graphics on the map](Shared/Samples/GraphicsOverlay/SketchOnMap)

    This sample demonstrates how to interactively sketch and edit graphics in the map view and display them in a graphics overlay. You can sketch a variety of geometry types and undo or redo operations.


- **Symbology**

    * [Render simple markers](Shared/Samples/Symbology/RenderSimpleMarkers)

    This sample adds a point graphic to a graphics overlay symbolized with a red circle specified via a SimpleMarkerSymbol.

    * [Render picture markers](Shared/Samples/Symbology/RenderPictureMarkers)

    This sample demonstrates how to create picture marker symbols from a URL and embedded resources.

    * [Render unique values](Shared/Samples/Symbology/RenderUniqueValues)

    This sample demonstrate how to use a unique value renderer to style different features in a feature layer with different symbols. Features do not have a symbol property for you to set, renderers should be used to define the symbol for features in feature layers. The unique value renderer allows for separate symbols to be used for features that have specific attribute values in a defined field.

    * [Simple renderer](Shared/Samples/Symbology/SimpleRenderers)

    This sample demonstrates how to create a simple renderer and add it to a graphics overlay. Renderers define the symbology for all graphics in a graphics overlay (unless they are overridden by setting the symbol directly on the graphic). Simple renderers can also be defined on feature layers using the same code.

## Analysis


- **Geoprocessing**

    * [Analyze hotspots](Shared/Samples/Geoprocessing/AnalyzeHotspots)

    This sample demonstrates how to execute the GeoprocessingTask asynchronously to calculate a hotspot analysis based on the frequency of 911 calls. It calculates the frequency of these calls within a given study area during a specified constrained time period set between 1/1/1998 and 5/31/1998.

    * [Viewshed (Geoprocessing)](Shared/Samples/Geoprocessing/AnalyzeViewshed)

    This sample demonstrates how to use GeoprocessingTask to calculate a viewshed using a geoprocessing service. Click any point on the map to see all areas that are visible within a 1 kilometer radius. It may take a few seconds for the model to run and send back the results.

    * [List geodatabase versions](Shared/Samples/Geoprocessing/ListGeodatabaseVersions)

    This sample demonstrates how to use GeoprocessingTask to get available geodatabase versions from the enterprise geodatabase. Geoprocessing task will return the versions as a table that is shown to the user in a list. This is a good example how to use geoprocessing on mapless application.


- **Statistics**

    * [Statistical query](Shared/Samples/Data/StatisticalQuery)

    This sample demonstrates how to query a feature table to get statistics for a specified field.

    * [Statistical query group and sort results](Shared/Samples/Data/StatsQueryGroupAndSort)

    This sample demonstrates how to query a feature table to get statistics for a specified field and to group and sort the results.

## Scenes


- **Scene symbols**

    * [Distance composite symbol](Shared/Samples/Symbology/UseDistanceCompositeSym)

    This sample demonstrates how to create a `DistanceCompositeSceneSymbol` with unique marker symbols to display at various distances from the camera.


- **Scene Layers**

    * [ArcGIS Scene layer (URL)](Shared/Samples/Layers/SceneLayerUrl)

    This sample demonstrates how to add an ArcGISSceneLayer as a layer in a Scene.

    * [Display scene](Shared/Samples/Layers/DisplayScene)

    Demonstrates how to display a scene with an elevation data source. An elevation data source allows objects to be viewed in 3D, like this picture of Mt. Everest.

## Network Analysis


- **Route**

    * [Find a route](Shared/Samples/NetworkAnalysis/FindRoute)

    This sample demonstrates how to solve for the best route between two locations on the map and display driving directions between them.

## Location


- **Display Location**

    * [Display device location](Shared/Samples/Location/DisplayDeviceLocation)

    This sample demonstrates how you can enable location services and switch between different types of auto pan modes.

## Search


- **Working with Places**

    * [Find Address](Shared/Samples/Search/FindAddress)

    This sample demonstrates how you can use the LocatorTask API to geocode an address and display it with a pin on the map. Tapping the pin displays the reverse-geocoded address in a callout.

    * [Find Place](Shared/Samples/Search/FindPlace)

    This sample demonstrates how to use geocode functionality to search for points of interest, around a location or within an extent.

## Tutorial


- **Author, edit, and save a map**

    * [Author, edit, and save a map](Shared/Samples/Tutorial/AuthorEditSaveMap)

    This sample demonstrates how to author and save a map as an ArcGIS portal item (web map). It is also the solution to the [Author, edit, and save maps to your portal tutorial](https://developers.arcgis.com/net/latest/forms/guide/author-edit-and-save-maps-to-your-portal.htm). Saving a map to arcgis.com requires an ArcGIS Online login.


