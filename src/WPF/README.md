## Sample Table Of Contents
## Maps

* [Change viewpoint](ArcGISRuntime.WPF.Samples/Samples/MapView/ChangeViewpoint)

    This sample demonstrates different ways in which you can change the viewpoint or visible area of the map.

* [Map rotation](ArcGISRuntime.WPF.Samples/Samples/MapView/MapRotation)

    This sample demonstrates how to rotate a map.

* [Display drawing status](ArcGISRuntime.WPF.Samples/Samples/MapView/DisplayDrawingStatus)

    This sample demonstrates how to use the DrawStatus value of the MapView to notify user that the MapView is drawing.

* [Display layer view state](ArcGISRuntime.WPF.Samples/Samples/MapView/DisplayLayerViewState)

    This sample demonstrates how to get view status for layers in a map.

* [Take Screenshot](ArcGISRuntime.WPF.Samples/Samples/MapView/TakeScreenshot)

    This sample demonstrates how you can take screenshot of a map. Click 'take screenshot' button to take a screenshot of the visible area of the map. Created image is shown in the sample after creation.

* [Show magnifier](ArcGISRuntime.WPF.Samples/Samples/MapView/ShowMagnifier)

    This sample demonstrates how you can tap and hold on a map to get the magnifier. You can also pan while tapping and holding to move the magnifier across the map.

* [Show callout](ArcGISRuntime.WPF.Samples/Samples/MapView/ShowCallout)

    This sample illustrates how to show callouts on a map in response to user interaction.

* [Display a map](ArcGISRuntime.WPF.Samples/Samples/Map/DisplayMap)

    This sample demonstrates how to display a map with a basemap.

* [Open map (URL)](ArcGISRuntime.WPF.Samples/Samples/Map/OpenMapURL)

    This sample demonstrates loading a webmap in a map from a Uri.

* [Open mobile map (map package)](ArcGISRuntime.WPF.Samples/Samples/Map/OpenMobileMap)

    This sample demonstrates how to open a mobile map from a map package.

* [Search a portal for maps](ArcGISRuntime.WPF.Samples/Samples/Map/SearchPortalMaps)

    This sample demonstrates searching a portal for web maps and loading them in the map view. You can search ArcGIS Online public web maps using tag values or browse the web maps in your account. OAuth is used to authenticate with ArcGIS Online to access items in your account.

* [Set Min & Max Scale](ArcGISRuntime.WPF.Samples/Samples/Map/SetMinMaxScale)

    This sample demonstrates how to set the minimum and maximum scale of a Map. Setting the minimum and maximum scale for the Map can be useful in keeping the user focused at a certain level of detail.

* [Access load status](ArcGISRuntime.WPF.Samples/Samples/Map/AccessLoadStatus)

    This sample demonstrates how to access the Maps' LoadStatus. The LoadStatus will be considered loaded when the following are true: The Map has a valid SpatialReference and the Map has an been set to the MapView.

* [Set initial map location](ArcGISRuntime.WPF.Samples/Samples/Map/SetInitialMapLocation)

    This sample creates a map with a standard ESRI Imagery with Labels basemap that is centered on a latitude and longitude location and zoomed into a specific level of detail.

* [Set initial map area](ArcGISRuntime.WPF.Samples/Samples/Map/SetInitialMapArea)

    This sample demonstrates how to set the initial viewpoint from envelope defined by minimum (x,y) and maximum (x,y) values. The map's InitialViewpoint is set to this viewpoint before the map is loaded into the MapView. Upon loading the map zoom to this initial area.

* [Set map spatial reference](ArcGISRuntime.WPF.Samples/Samples/Map/SetMapSpatialReference)

    This sample demonstrates how you can set the spatial reference on a Map and all the layers would project accordingly.

* [Change basemap](ArcGISRuntime.WPF.Samples/Samples/Map/ChangeBasemap)

    This sample demonstrates how to dynamically change the basemap displayed in a Map.

* [Manage Bookmarks](ArcGISRuntime.WPF.Samples/Samples/Map/ManageBookmarks)

    This sample demonstrates how to access and add bookmarks to a map.

* [Author a map](ArcGISRuntime.WPF.Samples/Samples/Map/AuthorMap)

    This sample demonstrates how to author and save a map as an ArcGIS portal item (web map). Saving a map to arcgis.com requires an ArcGIS Online login.

## Layers

* [ArcGIS tiled layer (URL)](ArcGISRuntime.WPF.Samples/Samples/Layers/ArcGISTiledLayerUrl)

    This sample demonstrates how to add an ArcGISTiledLayer as a base layer in a map. The ArcGISTiledLayer comes from an ArcGIS Server sample web service.

* [ArcGIS vector tiled layer (URL)](ArcGISRuntime.WPF.Samples/Samples/Layers/ArcGISVectorTiledLayerUrl)

    This sample demonstrates how to create a ArcGISVectorTiledLayer and bind this to a Basemap which is used in the creation of a map.

* [ArcGIS map image layer (URL)](ArcGISRuntime.WPF.Samples/Samples/Layers/ArcGISMapImageLayerUrl)

    This sample demonstrates how to add an ArcGISMapImageLayer as a base layer in a map. The ArcGISMapImageLayer comes from an ArcGIS Server sample web service.

* [Change sublayer visibility](ArcGISRuntime.WPF.Samples/Samples/Layers/ChangeSublayerVisibility)

    This sample demonstrates how to show or hide sublayers of a map image layer.

* [WMTS layer](ArcGISRuntime.WPF.Samples/Samples/Layers/WMTSLayer)

    This sample demonstrates how to display a WMTS layer on a map via a Uri and WmtsLayerInfo.

* [ArcGIS raster layer (service)](ArcGISRuntime.WPF.Samples/Samples/Layers/RasterLayerImageServiceRaster)

    This sample demonstrates how to show a raster layer on a map based on an image service layer.

* [ArcGIS raster function (service)](ArcGISRuntime.WPF.Samples/Samples/Layers/RasterLayerRasterFunction)

    This sample demonstrates how to show a raster layer on a map based on an image service layer that has a raster function applied.

## Features

* [Feature layer (feature service)](ArcGISRuntime.WPF.Samples/Samples/Layers/FeatureLayerUrl)

    This sample demonstrates how to show a feature layer on a map using the URL to the service.

* [Change feature layer renderer](ArcGISRuntime.WPF.Samples/Samples/Layers/ChangeFeatureLayerRenderer)

    This sample demonstrates how to change renderer for a feature layer. It also shows how to reset the renderer back to the default.

* [Feature layer selection](ArcGISRuntime.WPF.Samples/Samples/Layers/FeatureLayerSelection)

    This sample demonstrates how to select features in a feature layer by tapping a MapView.

* [Feature layer definition expression](ArcGISRuntime.WPF.Samples/Samples/Layers/FeatureLayerDefinitionExpression)

    This sample demonstrates how to apply definition expression to a feature layer for filtering features. It also shows how to reset the definition expression.

* [Create a new feature collection layer](ArcGISRuntime.WPF.Samples/Samples/Layers/CreateFeatureCollectionLayer)

    This sample demonstrates how to create a new feature collection with several feature collection tables. The collection is displayed in the map as a feature collection layer.

* [Create a feature collection layer from a portal item](ArcGISRuntime.WPF.Samples/Samples/Layers/FeatureCollectionLayerFromPortal)

    This sample demonstrates opening a feature collection saved as a portal item.

* [Feature collection layer from query result](ArcGISRuntime.WPF.Samples/Samples/Layers/FeatureCollectionLayerFromQuery)

    This sample demonstrates how to create a feature collection layer to show a query result from a service feature table.

* [Feature layer dictionary renderer](ArcGISRuntime.WPF.Samples/Samples/Layers/FeatureLayerDictionaryRenderer)

    Demonstrates how to apply a dictionary renderer to a feature layer and display mil2525d graphics. The dictionary renderer creates these graphics using a mil2525d style file and the attributes attached to each feature within the geodatabase.

* [Service feature table (cache)](ArcGISRuntime.WPF.Samples/Samples/Data/ServiceFeatureTableCache)

    This sample demonstrates how to use a feature service in on interaction cache mode.

* [Service feature table (no cache)](ArcGISRuntime.WPF.Samples/Samples/Data/ServiceFeatureTableNoCache)

    This sample demonstrates how to use a feature service in on interaction no cache mode.

* [Service feature table (manual cache)](ArcGISRuntime.WPF.Samples/Samples/Data/ServiceFeatureTableManualCache)

    This sample demonstrates how to use a feature service in manual cache mode.

* [Feature layer query](ArcGISRuntime.WPF.Samples/Samples/Data/FeatureLayerQuery)

    This sample demonstrates how to query a feature layer via feature table.

* [Generate geodatabase](ArcGISRuntime.WPF.Samples/Samples/Data/GenerateGeodatabase)

    This sample demonstrates how to take a feature service offline by generating a geodatabase.

* [Edit and sync features](ArcGISRuntime.WPF.Samples/Samples/Data/EditAndSyncFeatures)

    This sample demonstrates how to synchronize offline edits with a feature service.

## Display Information

* [Add graphics (SimpleRenderer)](ArcGISRuntime.WPF.Samples/Samples/GraphicsOverlay/AddGraphicsRenderer)

    This sample demonstrates how you add graphics and set a renderer on a graphic overlays.

* [Identify graphics](ArcGISRuntime.WPF.Samples/Samples/GraphicsOverlay/IdentifyGraphics)

    This sample demonstrates how to identify graphics in a graphics overlay. When you tap on a graphic on the map, you will see an alert message displayed.

* [Sketch graphics on the map](ArcGISRuntime.WPF.Samples/Samples/GraphicsOverlay/SketchOnMap)

    This sample demonstrates how to interactively sketch and edit graphics in the map view and display them in a graphics overlay. You can sketch a variety of geometry types and undo or redo operations.

* [Render simple markers](ArcGISRuntime.WPF.Samples/Samples/Symbology/RenderSimpleMarkers)

    This sample adds a point graphic to a graphics overlay symbolized with a red circle specified via a SimpleMarkerSymbol.

* [Render picture markers](ArcGISRuntime.WPF.Samples/Samples/Symbology/RenderPictureMarkers)

    This sample demonstrates how to create picture marker symbols from a URL and embedded resources.

* [Render unique values](ArcGISRuntime.WPF.Samples/Samples/Symbology/RenderUniqueValues)

    This sample demonstrate how to use a unique value renderer to style different features in a feature layer with different symbols. Features do not have a symbol property for you to set, renderers should be used to define the symbol for features in feature layers. The unique value renderer allows for separate symbols to be used for features that have specific attribute values in a defined field.

## Analysis

* [Analyze hotspots](ArcGISRuntime.WPF.Samples/Samples/Geoprocessing/AnalyzeHotspots)

    This sample demonstrates how to execute the GeoprocessingTask asynchronously to calculate a hotspot analysis based on the frequency of 911 calls. It calculates the frequency of these calls within a given study area during a specified constrained time period set between 1/1/1998 and 5/31/1998.

* [Analyze viewshed](ArcGISRuntime.WPF.Samples/Samples/Geoprocessing/AnalyzeViewshed)

    This sample demonstrates how to use GeoprocessingTask to calculate a viewshed using a geoprocessing service. Click any point on the map to see all areas that are visible within a 1 kilometer radius. It may take a few seconds for the model to run and send back the results.

* [List geodatabase versions](ArcGISRuntime.WPF.Samples/Samples/Geoprocessing/ListGeodatabaseVersions)

    This sample demonstrates how to use GeoprocessingTask to get available geodatabase versions from the enterprise geodatabase. Geoprocessing task will return the versions as a table that is shown to the user in a list. This is a good example how to use geoprocessing on mapless application.

## Network Analysis

* [Find a route](ArcGISRuntime.WPF.Samples/Samples/NetworkAnalysis/FindRoute)

    This sample demonstrates how to solve for the best route between two locations on the map and display driving directions between them.

## Scenes

* [Distance composite symbol](ArcGISRuntime.WPF.Samples/Samples/Symbology/UseDistanceCompositeSym)

    This sample demonstrates how to create a `DistanceCompositeSceneSymbol` with unique marker symbols to display at various distances from the camera.

* [ArcGIS Scene layer (URL)](ArcGISRuntime.WPF.Samples/Samples/Layers/SceneLayerUrl)

    This sample demonstrates how to add an ArcGISSceneLayer as a layer in a Scene.

## Location

* [Display Device Location](ArcGISRuntime.WPF.Samples/Samples/Location/DisplayDeviceLocation)

    This sample demonstrates how you can enable location services and switch between different types of auto pan modes.

## Search


- **Working with Places**

    * [Find Address](ArcGISRuntime.WPF.Samples/Samples/Search/FindAddress)

    This sample demonstrates how you can use the LocatorTask API to geocode an address and display it with a pin on the map. Tapping the pin displays the reverse-geocoded address in a callout.

    * [Find Place](ArcGISRuntime.WPF.Samples/Samples/Search/FindPlace)

    This sample demonstrates how to use geocode functionality to search for points of interest, around a location or within an extent.

## Tutorial

* [Author, edit, and save a map](ArcGISRuntime.WPF.Samples/Samples/Tutorial/AuthorEditSaveMap)

    This sample demonstrates how to author and save a map as an ArcGIS portal item (web map). It is also the solution to the [Author, edit, and save maps to your portal tutorial](https://developers.arcgis.com/net/latest/wpf/guide/author-edit-and-save-maps-to-your-portal.htm). Saving a map to arcgis.com requires an ArcGIS Online login.


