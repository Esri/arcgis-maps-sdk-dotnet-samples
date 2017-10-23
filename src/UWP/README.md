## Sample Table Of Contents
## Analysis


- **Analysis**

    * [Analyze hotspots](ArcGISRuntime.UWP.Samples/Samples/Analysis/AnalyzeHotspots)

    This sample demonstrates how to execute the GeoprocessingTask asynchronously to calculate a hotspot analysis based on the frequency of 911 calls. It calculates the frequency of these calls within a given study area during a specified constrained time period set between 1/1/1998 and 5/31/1998.

    * [Viewshed (Geoprocessing)](ArcGISRuntime.UWP.Samples/Samples/Analysis/AnalyzeViewshed)

    This sample demonstrates how to use GeoprocessingTask to calculate a viewshed using a geoprocessing service. Click any point on the map to see all areas that are visible within a 1 kilometer radius. It may take a few seconds for the model to run and send back the results.

## Cloud and Portal


- **Cloud and Portal**

    * [Create a feature collection layer from a portal item](ArcGISRuntime.UWP.Samples/Samples/CloudAndPortal/FeatureCollectionLayerFromPortal)

    This sample demonstrates opening a feature collection saved as a portal item.

## Layers and Data


- **Layers and Data**

    * [ArcGIS map image layer (URL)](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/ArcGISMapImageLayerUrl)

    This sample demonstrates how to add an ArcGISMapImageLayer as a base layer in a map. The ArcGISMapImageLayer comes from an ArcGIS Server sample web service.

    * [ArcGIS tiled layer (URL)](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/ArcGISTiledLayerUrl)

    This sample demonstrates how to add an ArcGISTiledLayer as a base layer in a map. The ArcGISTiledLayer comes from an ArcGIS Server sample web service.

    * [ArcGIS vector tiled layer (URL)](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/ArcGISVectorTiledLayerUrl)

    This sample demonstrates how to create a ArcGISVectorTiledLayer and bind this to a Basemap which is used in the creation of a map.

    * [Change feature layer renderer](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/ChangeFeatureLayerRenderer)

    This sample demonstrates how to change renderer for a feature layer. It also shows how to reset the renderer back to the default.

    * [Change sublayer visibility](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/ChangeSublayerVisibility)

    This sample demonstrates how to show or hide sublayers of a map image layer.

    * [Create a new feature collection layer](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/CreateFeatureCollectionLayer)

    This sample demonstrates how to create a new feature collection with several feature collection tables. The collection is displayed in the map as a feature collection layer.

    * [Display scene](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/DisplayScene)

    Demonstrates how to display a scene with an elevation data source. An elevation data source allows objects to be viewed in 3D, like this picture of Mt. Everest.

    * [Edit and sync features](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/EditAndSyncFeatures)

    This sample demonstrates how to synchronize offline edits with a feature service.

    * [Export tiles](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/ExportTiles)

    This sample demonstrates how to export tiles from a map server.

    * [Feature collection layer from query result](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/FeatureCollectionLayerFromQuery)

    This sample demonstrates how to create a feature collection layer to show a query result from a service feature table.

    * [Feature layer definition expression](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/FeatureLayerDefinitionExpression)

    This sample demonstrates how to apply definition expression to a feature layer for filtering features. It also shows how to reset the definition expression.

    * [Feature layer dictionary renderer](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/FeatureLayerDictionaryRenderer)

    Demonstrates how to apply a dictionary renderer to a feature layer and display mil2525d graphics. The dictionary renderer creates these graphics using a mil2525d style file and the attributes attached to each feature within the geodatabase.

    * [Feature layer (GeoPackage)](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/FeatureLayerGeoPackage)

    This sample demonstrates how to open a GeoPackage and show a GeoPackage feature table in a feature layer.

    * [Feature layer query](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/FeatureLayerQuery)

    This sample demonstrates how to return features from a feature layer using an attribute query on the underlying feature table.

    * [Feature layer selection](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/FeatureLayerSelection)

    This sample demonstrates how to select features in a feature layer by tapping a MapView.

    * [Feature layer (shapefile)](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/FeatureLayerShapefile)

    This sample demonstrates how to open a shapefile stored on the device and display it as a feature layer with default symbology.

    * [Feature layer (feature service)](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/FeatureLayerUrl)

    This sample demonstrates how to show a feature layer on a map using the URL to the service.

    * [Generate geodatabase](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/GenerateGeodatabase)

    This sample demonstrates how to take a feature service offline by generating a geodatabase.

    * [KML layer (file)](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/KmlLayerFile)

    This sample demonstrates how to display a KML file from local storage.

    * [KML layer (URL)](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/KmlLayerUrl)

    This sample demonstrates how to display a KML file from a URL.

    * [List geodatabase versions](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/ListGeodatabaseVersions)

    This sample demonstrates how to use GeoprocessingTask to get available geodatabase versions from the enterprise geodatabase. Geoprocessing task will return the versions as a table that is shown to the user in a list. This is a good example how to use geoprocessing on mapless application.

    * [Raster layer (file)](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/RasterLayerFile)

    This sample demonstrates how to use a raster layer created from a local raster file.

    * [ArcGIS raster layer (service)](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/RasterLayerImageServiceRaster)

    This sample demonstrates how to show a raster layer on a map based on an image service layer.

    * [ArcGIS raster function (service)](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/RasterLayerRasterFunction)

    This sample demonstrates how to show a raster layer on a map based on an image service layer that has a raster function applied.

    * [Raster rendering rule](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/RasterRenderingRule)

    This sample demonstrates how to create an `ImageServiceRaster`, fetch the `RenderingRule`s from the service info, and use a `RenderingRule` to create an `ImageServiceRaster` and add it to a raster layer.

    * [ArcGIS Scene layer (URL)](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/SceneLayerUrl)

    This sample demonstrates how to add an ArcGISSceneLayer as a layer in a Scene.

    * [Service feature table (cache)](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/ServiceFeatureTableCache)

    This sample demonstrates how to use a feature service in on interaction cache mode.

    * [Service feature table (manual cache)](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/ServiceFeatureTableManualCache)

    This sample demonstrates how to use a feature service in manual cache mode.

    * [Service feature table (no cache)](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/ServiceFeatureTableNoCache)

    This sample demonstrates how to use a feature service in on interaction no cache mode.

    * [Statistical query](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/StatisticalQuery)

    This sample demonstrates how to query a feature table to get statistics for a specified field.

    * [Statistical query group and sort results](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/StatsQueryGroupAndSort)

    This sample demonstrates how to query a feature table to get statistics for a specified field and to group and sort the results.

    * [Time-based query](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/TimeBasedQuery)

    This sample demonstrates how to apply a time-based parameter to a feature layer query.

    * [WMS layer (URL)](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/WmsLayerUrl)

    This sample demonstrates how to add a layer from a WMS service to a map.

    * [WMS service catalog](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/WmsServiceCatalog)

    This sample demonstrates how to enable and disable the display of layers discovered from a WMS service.

    * [WMTS layer](ArcGISRuntime.UWP.Samples/Samples/LayersAndData/WMTSLayer)

    This sample demonstrates how to display a WMTS layer on a map via a Uri and WmtsLayerInfo.

## Maps and Visualization


- **Maps and Visualization**

    * [Access load status](ArcGISRuntime.UWP.Samples/Samples/MapsAndVisualization/AccessLoadStatus)

    This sample demonstrates how to access the Maps' LoadStatus. The LoadStatus will be considered loaded when the following are true: The Map has a valid SpatialReference and the Map has an been set to the MapView.

    * [Add graphics (SimpleRenderer)](ArcGISRuntime.UWP.Samples/Samples/MapsAndVisualization/AddGraphicsRenderer)

    This sample demonstrates how you add graphics and set a renderer on a graphic overlays.

    * [Author, edit, and save a map](ArcGISRuntime.UWP.Samples/Samples/MapsAndVisualization/AuthorEditSaveMap)

    This sample demonstrates how to author and save a map as an ArcGIS portal item (web map). It is also the solution to the [Author, edit, and save maps to your portal tutorial](https://developers.arcgis.com/net/latest/uwp/guide/author-edit-and-save-maps-to-your-portal.htm). Saving a map to arcgis.com requires an ArcGIS Online login.

    * [Author a map](ArcGISRuntime.UWP.Samples/Samples/MapsAndVisualization/AuthorMap)

    This sample demonstrates how to author and save a map as an ArcGIS portal item (web map). Saving a map to arcgis.com requires an ArcGIS Online login.

    * [Change basemap](ArcGISRuntime.UWP.Samples/Samples/MapsAndVisualization/ChangeBasemap)

    This sample demonstrates how to dynamically change the basemap displayed in a Map.

    * [Change viewpoint](ArcGISRuntime.UWP.Samples/Samples/MapsAndVisualization/ChangeViewpoint)

    This sample demonstrates different ways in which you can change the viewpoint or visible area of the map.

    * [Display drawing status](ArcGISRuntime.UWP.Samples/Samples/MapsAndVisualization/DisplayDrawingStatus)

    This sample demonstrates how to use the DrawStatus value of the Map to notify user that the Map is drawing.

    * [Display layer view state](ArcGISRuntime.UWP.Samples/Samples/MapsAndVisualization/DisplayLayerViewState)

    This sample demonstrates how to get view status for layers in a map.

    * [Display a map](ArcGISRuntime.UWP.Samples/Samples/MapsAndVisualization/DisplayMap)

    This sample demonstrates how to display a map with a basemap.

    * [Identify graphics](ArcGISRuntime.UWP.Samples/Samples/MapsAndVisualization/IdentifyGraphics)

    This sample demonstrates how to identify graphics in a graphics overlay. When you tap on a graphic on the map, you will see an alert message displayed.

    * [Manage Bookmarks](ArcGISRuntime.UWP.Samples/Samples/MapsAndVisualization/ManageBookmarks)

    This sample demonstrates how to access and add bookmarks to a map.

    * [Map rotation](ArcGISRuntime.UWP.Samples/Samples/MapsAndVisualization/MapRotation)

    This sample demonstrates how to rotate a map.

    * [Open mobile map (map package)](ArcGISRuntime.UWP.Samples/Samples/MapsAndVisualization/OpenMobileMap)

    This sample demonstrates how to open a mobile map from a map package.

    * [Render picture markers](ArcGISRuntime.UWP.Samples/Samples/MapsAndVisualization/RenderPictureMarkers)

    This sample demonstrates how to create picture marker symbols from a URL and embedded resources.

    * [Render simple markers](ArcGISRuntime.UWP.Samples/Samples/MapsAndVisualization/RenderSimpleMarkers)

    This sample adds a point graphic to a graphics overlay symbolized with a red circle specified via a SimpleMarkerSymbol.

    * [Render unique values](ArcGISRuntime.UWP.Samples/Samples/MapsAndVisualization/RenderUniqueValues)

    This sample demonstrate how to use a unique value renderer to style different features in a feature layer with different symbols. Features do not have a symbol property for you to set, renderers should be used to define the symbol for features in feature layers. The unique value renderer allows for separate symbols to be used for features that have specific attribute values in a defined field.

    * [Search a portal for maps](ArcGISRuntime.UWP.Samples/Samples/MapsAndVisualization/SearchPortalMaps)

    This sample demonstrates searching a portal for web maps and loading them in the map view. You can search ArcGIS Online public web maps using tag values or browse the web maps in your account. OAuth is used to authenticate with ArcGIS Online to access items in your account.

    * [Set initial map area](ArcGISRuntime.UWP.Samples/Samples/MapsAndVisualization/SetInitialMapArea)

    This sample displays a map at a specific viewpoint. In this sample a viewpoint is constructed from an envelope defined by minimum (x,y) and maximum (x,y) values. The map's initialViewpoint is set to this viewpoint before the map is loaded. Upon loading the map zooms to this initial area.

    * [Set initial map location](ArcGISRuntime.UWP.Samples/Samples/MapsAndVisualization/SetInitialMapLocation)

    This sample creates a map with a standard ESRI Imagery with Labels basemap that is centered on a latitude and longitude location and zoomed into a specific level of detail.

    * [Set map spatial reference](ArcGISRuntime.UWP.Samples/Samples/MapsAndVisualization/SetMapSpatialReference)

    This sample demonstrates how you can set the spatial reference on a Map and all the layers would project accordingly.

    * [Set Min & Max Scale](ArcGISRuntime.UWP.Samples/Samples/MapsAndVisualization/SetMinMaxScale)

    This sample demonstrates how to set the minimum and maximum scale of a Map. Setting the minimum and maximum scale for the Map can be useful in keeping the user focused at a certain level of detail.

    * [Show callout](ArcGISRuntime.UWP.Samples/Samples/MapsAndVisualization/ShowCallout)

    This sample illustrates how to show callouts on a map in response to user interaction.

    * [Show magnifier](ArcGISRuntime.UWP.Samples/Samples/MapsAndVisualization/ShowMagnifier)

    This sample demonstrates how you can tap and hold on a map to get the magnifier. You can also pan while tapping and holding to move the magnifier across the map.

    * [Simple renderer](ArcGISRuntime.UWP.Samples/Samples/MapsAndVisualization/SimpleRenderers)

    This sample demonstrates how to create a simple renderer and add it to a graphics overlay. Renderers define the symbology for all graphics in a graphics overlay (unless they are overridden by setting the symbol directly on the graphic). Simple renderers can also be defined on feature layers using the same code.

    * [Sketch graphics on the map](ArcGISRuntime.UWP.Samples/Samples/MapsAndVisualization/SketchOnMap)

    This sample demonstrates how to interactively sketch and edit graphics in the map view and display them in a graphics overlay. You can sketch a variety of geometry types and undo or redo operations.

    * [Surface placement](ArcGISRuntime.UWP.Samples/Samples/MapsAndVisualization/SurfacePlacements)

    This sample demonstrates how to position graphics using different Surface Placements.

    * [Take Screenshot](ArcGISRuntime.UWP.Samples/Samples/MapsAndVisualization/TakeScreenshot)

    This sample demonstrates how you can take screenshot of a map. Click 'capture' button to take a screenshot of the visible area of the map. Created image is shown in the sample after creation.

    * [Distance composite symbol](ArcGISRuntime.UWP.Samples/Samples/MapsAndVisualization/UseDistanceCompositeSym)

    This sample demonstrates how to create a `DistanceCompositeSceneSymbol` with unique marker symbols to display at various distances from the camera.

## Routing and Location


- **Routing and Location**

    * [Display Device Location](ArcGISRuntime.UWP.Samples/Samples/RoutingAndLocation/DisplayDeviceLocation)

    This sample demonstrates how you can enable location services and switch between different types of auto pan modes.

    * [Find Address](ArcGISRuntime.UWP.Samples/Samples/RoutingAndLocation/FindAddress)

    This sample demonstrates how you can use the LocatorTask API to geocode an address and display it with a pin on the map. Tapping the pin displays the reverse-geocoded address in a callout.

    * [Find Place](ArcGISRuntime.UWP.Samples/Samples/RoutingAndLocation/FindPlace)

    This sample demonstrates how to use geocode functionality to search for points of interest, around a location or within an extent.

    * [Find a route](ArcGISRuntime.UWP.Samples/Samples/RoutingAndLocation/FindRoute)

    This sample demonstrates how to solve for the best route between two locations on the map and display driving directions between them.


