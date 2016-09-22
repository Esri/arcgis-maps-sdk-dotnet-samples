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


- **Map**

    * [Display a map](Shared/Samples/Map/DisplayMap)

    This samples demonstrates how to display a map with a basemap

    * [Open an existing map](Shared/Samples/Map/OpenExistingMap)

    This sample demonstrates how to open an existing map from a portal. The sample opens with a map displayed by default. You can change the shown map by selecting a new one from the populated list.

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

## Layers


- **Tiled Layers**

    * [ArcGIS tiled layer (URL)](Shared/Samples/Layers/ArcGISTiledLayerUrl)

    This sample demonstrates how to add an ArcGISTiledLayer as a base layer in a map. The ArcGISTiledLayer comes from an ArcGIS Server sample web service.


- **Map Image Layers**

    * [ArcGIS map image layer (URL)](Shared/Samples/Layers/ArcGISMapImageLayerUrl)

    This sample demonstrates how to add an ArcGISMapImageLayer as a base layer in a map. The ArcGISMapImageLayer comes from an ArcGIS Server sample web service.

    * [Change sublayer visibility](Shared/Samples/Layers/ChangeSublayerVisibility)

    This sample demonstrates how to show or hide sublayers of a map image layer.

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


- **Feature Tables**

    * [Service feature table (cache)](Shared/Samples/Data/ServiceFeatureTableCache)

    This sample demonstrates how to use a feature service in on interaction cache mode.

    * [Service feature table (no cache)](Shared/Samples/Data/ServiceFeatureTableNoCache)

    This sample demonstrates how to use a feature service in on interaction no cache mode.

    * [Service feature table (manual cache)](Shared/Samples/Data/ServiceFeatureTableManualCache)

    This sample demonstrates how to use a feature service in manual cache mode.

    * [Feature layer query](Shared/Samples/Data/FeatureLayerQuery)

    This sample demonstrates how to query a feature layer via feature table.

## Display Information


- **Graphics Overlays**

    * [Add graphics (Renderer)](Shared/Samples/GraphicsOverlay/AddGraphicsRenderer)

    This sample demonstrates how you add graphics and set a renderer on a graphic overlays.

    * [Identify graphics](Shared/Samples/GraphicsOverlay/IdentifyGraphics)

    This sample demonstrates how to identify graphics in a graphics overlay. When you tap on a graphic on the map, you will see an alert message displayed.


- **Symbology**

    * [Render simple markers](Shared/Samples/Symbology/RenderSimpleMarkers)

    This sample adds a point graphic to a graphics overlay symbolized with a red circle specified via a SimpleMarkerSymbol.

    * [Render picture markers](Shared/Samples/Symbology/RenderPictureMarkers)

    This sample demonstrates how to create picture marker symbols from a URL and embedded resources.

    * [Render unique values](Shared/Samples/Symbology/RenderUniqueValues)

    This sample demonstrate how to use a unique value renderer to style different features in a feature layer with different symbols. Features do not have a symbol property for you to set, renderers should be used to define the symbol for features in feature layers. The unique value renderer allows for separate symbols to be used for features that have specific attribute values in a defined field.

## Location


- **Display Location**

    * [Display device location](Shared/Samples/Location/DisplayDeviceLocation)

    This sample demonstrates how you can enable location services and switch between different types of auto pan modes.



[](Esri Tags: ArcGIS Runtime SDK .NET WinRT WinStore WPF WinPhone C# C-Sharp DotNet XAML MVVM)
[](Esri Language: DotNet)
