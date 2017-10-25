## Sample Table Of Contents
## Analysis


- **Analysis**

    * [Analyze hotspots](Xamarin.Android/Samples/Analysis/AnalyzeHotspots)

    This sample demonstrates how to execute the GeoprocessingTask asynchronously to calculate a hotspot analysis based on the frequency of 911 calls. It calculates the frequency of these calls within a given study area during a specified constrained time period set between 1/1/1998 and 5/31/1998.

    * [Viewshed (Geoprocessing)](Xamarin.Android/Samples/Analysis/AnalyzeViewshed)

    This sample demonstrates how to use GeoprocessingTask to calculate a viewshed using a geoprocessing service. Click any point on the map to see all areas that are visible within a 1 kilometer radius. It may take a few seconds for the model to run and send back the results.

## Cloud and Portal


- **Cloud and Portal**

    * [Create a feature collection layer from a portal item](Xamarin.Android/Samples/CloudAndPortal/FeatureCollectionLayerFromPortal)

    This sample demonstrates opening a feature collection saved as a portal item.

## Layers and Data


- **Layers and Data**

    * [ArcGIS map image layer (URL)](Xamarin.Android/Samples/LayersAndData/ArcGISMapImageLayerUrl)

    This sample demonstrates how to add an ArcGISMapImageLayer as a base layer in a map. The ArcGISMapImageLayer comes from an ArcGIS Server sample web service.

    * [ArcGIS tiled layer (URL)](Xamarin.Android/Samples/LayersAndData/ArcGISTiledLayerUrl)

    This sample demonstrates how to add an ArcGISTiledLayer as a base layer in a map. The ArcGISTiledLayer comes from an ArcGIS Server sample web service.

    * [ArcGIS vector tiled layer (URL)](Xamarin.Android/Samples/LayersAndData/ArcGISVectorTiledLayerUrl)

    This sample demonstrates how to create a ArcGISVectorTiledLayer and bind this to a Basemap which is used in the creation of a map.

    * [Change feature layer renderer](Xamarin.Android/Samples/LayersAndData/ChangeFeatureLayerRenderer)

    This sample demonstrates how to change renderer for a feature layer. It also shows how to reset the renderer back to the default.

    * [Change sublayer visibility](Xamarin.Android/Samples/LayersAndData/ChangeSublayerVisibility)

    This sample demonstrates how to show or hide sublayers of a map image layer.

    * [Create feature collection layer](Xamarin.Android/Samples/LayersAndData/CreateFeatureCollectionLayer)

    This sample demonstrates how to create a new feature collection with several feature collection tables. The collection is displayed in the map as a feature collection layer.

    * [Display scene](Xamarin.Android/Samples/LayersAndData/DisplayScene)

    Demonstrates how to display a scene with an elevation data source. An elevation data source allows objects to be viewed in 3D, like this picture of Mt. Everest.

    * [Edit and sync features](Xamarin.Android/Samples/LayersAndData/EditAndSyncFeatures)

    This sample demonstrates how to synchronize offline edits with a feature service.

    * [Export tiles](Xamarin.Android/Samples/LayersAndData/ExportTiles)

    This sample demonstrates how to export tiles from a map server.

    * [Feature collection layer from query result](Xamarin.Android/Samples/LayersAndData/FeatureCollectionLayerFromQuery)

    This sample demonstrates how to create a feature collection layer to show a query result from a service feature table.

    * [Feature layer definition expression](Xamarin.Android/Samples/LayersAndData/FeatureLayerDefinitionExpression)

    This sample demonstrates how to apply definition expression to a feature layer for filtering features. It also shows how to reset the definition expression.

    * [Feature layer dictionary renderer](Xamarin.Android/Samples/LayersAndData/FeatureLayerDictionaryRenderer)

    Demonstrates how to apply a dictionary renderer to a feature layer and display mil2525d graphics. The dictionary renderer creates these graphics using a mil2525d style file and the attributes attached to each feature within the geodatabase.

    * [Feature layer (GeoPackage)](Xamarin.Android/Samples/LayersAndData/FeatureLayerGeoPackage)

    This sample demonstrates how to open a GeoPackage and show a GeoPackage feature table in a feature layer.

    * [Feature layer query](Xamarin.Android/Samples/LayersAndData/FeatureLayerQuery)

    This sample demonstrates how to query a feature layer via feature table.

    * [Feature layer selection](Xamarin.Android/Samples/LayersAndData/FeatureLayerSelection)

    This sample demonstrates how to select features in a feature layer by tapping a MapView.

    * [Feature layer (shapefile)](Xamarin.Android/Samples/LayersAndData/FeatureLayerShapefile)

    This sample demonstrates how to open a shapefile stored on the device and display it as a feature layer with default symbology.

    * [Feature layer (feature service)](Xamarin.Android/Samples/LayersAndData/FeatureLayerUrl)

    This sample demonstrates how to show a feature layer on a map using the URL to the service.

    * [Generate geodatabase](Xamarin.Android/Samples/LayersAndData/GenerateGeodatabase)

    This sample demonstrates how to take a feature service offline by generating a geodatabase.

    * [KML layer (file)](Xamarin.Android/Samples/LayersAndData/KmlLayerFile)

    This sample demonstrates how to display a KML file from local storage.

    * [KML layer (URL)](Xamarin.Android/Samples/LayersAndData/KmlLayerUrl)

    This sample demonstrates how to display a KML file from a URL.

    * [List geodatabase versions](Xamarin.Android/Samples/LayersAndData/ListGeodatabaseVersions)

    This sample demonstrates how to use GeoprocessingTask to get available geodatabase versions from the enterprise geodatabase. Geoprocessing task will return the versions as a table that is shown to the user in a list. This is a good example how to use geoprocessing on mapless application.

    * [Raster layer (file)](Xamarin.Android/Samples/LayersAndData/RasterLayerFile)

    This sample demonstrates how to use a raster layer created from a local raster file.

    * [ArcGIS raster layer (service)](Xamarin.Android/Samples/LayersAndData/RasterLayerImageServiceRaster)

    This sample demonstrates how to show a raster layer on a map based on an image service layer.

    * [ArcGIS raster function (service)](Xamarin.Android/Samples/LayersAndData/RasterLayerRasterFunction)

    This sample demonstrates how to show a raster layer on a map based on an image service layer that has a raster function applied.

    * [Raster rendering rule](Xamarin.Android/Samples/LayersAndData/RasterRenderingRule)

    This sample demonstrates how to create an `ImageServiceRaster`, fetch the `RenderingRule`s from the service info, and use a `RenderingRule` to create an `ImageServiceRaster` and add it to a raster layer.

    * [Read shapefile metadata](Xamarin.Android/Samples/LayersAndData/ReadShapefileMetadata)

    This sample demonstrates how to open a shapefile stored on the device, read metadata that describes the dataset, and display it as a feature layer with default symbology.

    * [ArcGIS Scene layer (URL)](Xamarin.Android/Samples/LayersAndData/SceneLayerUrl)

    This sample demonstrates how to add an ArcGISSceneLayer as a layer in a Scene.

    * [Service feature table (cache)](Xamarin.Android/Samples/LayersAndData/ServiceFeatureTableCache)

    This sample demonstrates how to use a feature service in on interaction cache mode.

    * [Service feature table (manual cache)](Xamarin.Android/Samples/LayersAndData/ServiceFeatureTableManualCache)

    This sample demonstrates how to use a feature service in manual cache mode.

    * [Service feature table (no cache)](Xamarin.Android/Samples/LayersAndData/ServiceFeatureTableNoCache)

    This sample demonstrates how to use a feature service in on interaction no cache mode.

    * [Statistical query](Xamarin.Android/Samples/LayersAndData/StatisticalQuery)

    This sample demonstrates how to query a feature table to get statistics for a specified field.

    * [Statistical query group and sort results](Xamarin.Android/Samples/LayersAndData/StatsQueryGroupAndSort)

    This sample demonstrates how to query a feature table to get statistics for a specified field and to group and sort the results.

    * [Time-based query](Xamarin.Android/Samples/LayersAndData/TimeBasedQuery)

    This sample demonstrates how to apply a time-based parameter to a feature layer query.

    * [WMS layer (URL)](Xamarin.Android/Samples/LayersAndData/WmsLayerUrl)

    This sample demonstrates how to add a layer from a WMS service to a map.

    * [WMS service catalog](Xamarin.Android/Samples/LayersAndData/WmsServiceCatalog)

    This sample demonstrates how to enable and disable the display of layers discovered from a WMS service.

    * [WMTS layer](Xamarin.Android/Samples/LayersAndData/WMTSLayer)

    This sample demonstrates how to display a WMTS layer on a map via a Uri and WmtsLayerInfo.

## Maps and Visualization


- **Maps and Visualization**

    * [Access load status](Xamarin.Android/Samples/MapsAndVisualization/AccessLoadStatus)

    This sample demonstrates how to access the Maps' LoadStatus. The LoadStatus will be considered loaded when the following are true: The Map has a valid SpatialReference and the Map has an been set to the MapView.

    * [Add graphics (SimpleRenderer)](Xamarin.Android/Samples/MapsAndVisualization/AddGraphicsRenderer)

    This sample demonstrates how you add graphics and set a renderer on a graphic overlays.

    * [Author, edit, and save a map](Xamarin.Android/Samples/MapsAndVisualization/AuthorEditSaveMap)

    This sample demonstrates how to author and save a map as an ArcGIS portal item (web map). It is also the solution to the [Author, edit, and save maps to your portal tutorial](https://developers.arcgis.com/net/latest/android/guide/author-edit-and-save-maps-to-your-portal.htm). Saving a map to arcgis.com requires an ArcGIS Online login.

    * [Author and save a map](Xamarin.Android/Samples/MapsAndVisualization/AuthorMap)

    This sample demonstrates how to author and save a map as an ArcGIS portal item (web map). Saving a map to arcgis.com requires an ArcGIS Online login.

    * [Change basemap](Xamarin.Android/Samples/MapsAndVisualization/ChangeBasemap)

    This sample demonstrates how to dynamically change the basemap displayed in a Map.

    * [Change viewpoint](Xamarin.Android/Samples/MapsAndVisualization/ChangeViewpoint)

    This sample demonstrates different ways in which you can change the viewpoint or visible area of the map.

    * [Display drawing status](Xamarin.Android/Samples/MapsAndVisualization/DisplayDrawingStatus)

    This sample demonstrates how to use the DrawStatus value of the MapView to notify user that the MapView is drawing.

    * [Display layer view state](Xamarin.Android/Samples/MapsAndVisualization/DisplayLayerViewState)

    This sample demonstrates how to get view status for layers in a map.

    * [Display a map](Xamarin.Android/Samples/MapsAndVisualization/DisplayMap)

    This sample demonstrates how to display a map with a basemap.

    * [Identify graphics](Xamarin.Android/Samples/MapsAndVisualization/IdentifyGraphics)

    This sample demonstrates how to identify graphics in a graphics overlay. When you tap on a graphic on the map, you will see an alert message displayed.

    * [Manage bookmarks](Xamarin.Android/Samples/MapsAndVisualization/ManageBookmarks)

    This sample demonstrates how to access and add bookmarks to a map.

    * [Map rotation](Xamarin.Android/Samples/MapsAndVisualization/MapRotation)

    This sample illustrates how to rotate a map.

    * [Open Map (URL)](Xamarin.Android/Samples/MapsAndVisualization/OpenMapUrl)

    This sample demonstrates how to open an existing map from a portal. The sample opens with a map displayed by default. You can change the shown map by selecting a new one from the populated list.

    * [Open mobile map (map package)](Xamarin.Android/Samples/MapsAndVisualization/OpenMobileMap)

    This sample demonstrates how to open a map from a mobile map package.

    * [Render picture markers](Xamarin.Android/Samples/MapsAndVisualization/RenderPictureMarkers)

    This sample demonstrates how to create picture marker symbols from a URL and embedded resources.

    * [Render simple markers](Xamarin.Android/Samples/MapsAndVisualization/RenderSimpleMarkers)

    This sample adds a point graphic to a graphics overlay symbolized with a red circle specified via a SimpleMarkerSymbol.

    * [Unique value renderer](Xamarin.Android/Samples/MapsAndVisualization/RenderUniqueValues)

    This sample demonstrate how to use a unique value renderer to style different features in a feature layer with different symbols. Features do not have a symbol property for you to set, renderers should be used to define the symbol for features in feature layers. The unique value renderer allows for separate symbols to be used for features that have specific attribute values in a defined field.

    * [Search a portal for maps](Xamarin.Android/Samples/MapsAndVisualization/SearchPortalMaps)

    This sample demonstrates searching a portal for web maps and loading them in the map view. You can search ArcGIS Online public web maps using tag values or browse the web maps in your account. OAuth is used to authenticate with ArcGIS Online to access items in your account.

    * [Set initial map area](Xamarin.Android/Samples/MapsAndVisualization/SetInitialMapArea)

    This sample demonstrates how to set the initial viewpoint from envelope defined by minimum (x,y) and maximum (x,y) values. The map's InitialViewpoint is set to this viewpoint before the map is loaded into the MapView. Upon loading the map zoom to this initial area.

    * [Set initial map location](Xamarin.Android/Samples/MapsAndVisualization/SetInitialMapLocation)

    This sample demonstrates how to create a map with a standard ESRI Imagery with Labels basemap that is centered on a latitude and longitude location and zoomed into a specific level of detail.

    * [Set map spatial reference](Xamarin.Android/Samples/MapsAndVisualization/SetMapSpatialReference)

    This sample demonstrates how you can set the spatial reference on a Map and all the operational layers would project accordingly.

    * [Set Min & Max Scale](Xamarin.Android/Samples/MapsAndVisualization/SetMinMaxScale)

    This sample demonstrates how to set the minimum and maximum scale of a Map. Setting the minimum and maximum scale for the Map can be useful in keeping the user focused at a certain level of detail.

    * [Show callout](Xamarin.Android/Samples/MapsAndVisualization/ShowCallout)

    This sample illustrates how to show callouts on a map in response to user interaction.

    * [Show magnifier](Xamarin.Android/Samples/MapsAndVisualization/ShowMagnifier)

    This sample demonstrates how you can tap and hold on a map to get the magnifier. You can also pan while tapping and holding to move the magnifier across the map.

    * [Simple renderer](Xamarin.Android/Samples/MapsAndVisualization/SimpleRenderers)

    This sample demonstrates how to create a simple renderer and add it to a graphics overlay. Renderers define the symbology for all graphics in a graphics overlay (unless they are overridden by setting the symbol directly on the graphic). Simple renderers can also be defined on feature layers using the same code.

    * [Sketch graphics on the map](Xamarin.Android/Samples/MapsAndVisualization/SketchOnMap)

    This sample demonstrates how to interactively sketch and edit graphics in the map view and display them in a graphics overlay. You can sketch a variety of geometry types and undo or redo operations.

    * [Surface placement](Xamarin.Android/Samples/MapsAndVisualization/SurfacePlacements)

    This sample demonstrates how to position graphics using different Surface Placements.

    * [Take Screenshot](Xamarin.Android/Samples/MapsAndVisualization/TakeScreenshot)

    This sample demonstrates how you can take screenshot of a map. Click 'capture' button to take a screenshot of the visible area of the map. Created image is shown in the sample after creation.

    * [Distance composite symbol](Xamarin.Android/Samples/MapsAndVisualization/UseDistanceCompositeSym)

    This sample demonstrates how to create a `DistanceCompositeSceneSymbol` with unique marker symbols to display at various distances from the camera.

## Routing and Location


- **Routing and Location**

    * [Display Device Location](Xamarin.Android/Samples/RoutingAndLocation/DisplayDeviceLocation)

    This sample demonstrates how you can enable location services and switch between different types of auto pan modes.

    * [Find Address](Xamarin.Android/Samples/RoutingAndLocation/FindAddress)

    This sample demonstrates how you can use the LocatorTask API to geocode an address and display it with a pin on the map. Tapping the pin displays the reverse-geocoded address in a callout.

    * [Find Place](Xamarin.Android/Samples/RoutingAndLocation/FindPlace)

    This sample demonstrates how to use geocode functionality to search for points of interest, around a location or within an extent.

    * [Find a route](Xamarin.Android/Samples/RoutingAndLocation/FindRoute)

    This sample demonstrates how to solve for the best route between two locations on the map and display driving directions between them.


