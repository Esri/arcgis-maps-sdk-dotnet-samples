## Sample Table Of Contents
## Maps


- **MapView**

    * [Change viewpoint](Xamarin.Android/Samples/MapView/ChangeViewpoint)

    This sample demonstrates different ways in which you can change the viewpoint or visible area of the map.

    * [Display draw status](Xamarin.Android/Samples/MapView/DisplayDrawStatus)

    This sample demonstrates how to display the draw status of a Map

    * [Map rotation](Xamarin.Android/Samples/MapView/MapRotation)

    This sample illustrates how to rotate a map.


- **Map**

    * [Display a map](Xamarin.Android/Samples/Map/DisplayMap)

    This samples demonstrates how to display a map with a basemap

    * [Open an existing map](Xamarin.Android/Samples/Map/OpenExistingMap)

    This sample demonstrates how to open an existing map from a portal. The sample opens with a map displayed by default. You can change the shown map by selecting a new one from the populated list.

    * [Set Min & Max Scale](Xamarin.Android/Samples/Map/SetMinMaxScale)

    This sample demonstrates how to set the minimum and maximum scale of a Map. Setting the minimum and maximum scale for the Map can be useful in keeping the user focused at a certain level of detail.

    * [Set initial map location](Xamarin.Android/Samples/Map/SetInitialMapLocation)

    This sample demonstrates how to create a map with a standard ESRI Imagery with Labels basemap that is centered on a latitude and longitude location and zoomed into a specific level of detail.

    * [Set initial map area](Xamarin.Android/Samples/Map/SetInitialMapArea)

    This sample demonstrates how to set the initial viewpoint from envelope defined by minimum (x,y) and maximum (x,y) values. The map's InitialViewpoint is set to this viewpoint before the map is loaded into the MapView. Upon loading the map zoom to this initial area.

    * [Set map spatial reference](Xamarin.Android/Samples/Map/SetMapSpatialReference)

    This sample demonstrates how you can set the spatial reference on a Map and all the operational layers would project accordingly.

    * [Access load status](Xamarin.Android/Samples/Map/AccessLoadStatus)

    This sample demonstrates how to access the Maps' LoadStatus. The LoadStatus will be considered loaded when the following are true: The Map has a valid SpatialReference and the Map has an been set to the MapView.

    * [Change basemap](Xamarin.Android/Samples/Map/ChangeBasemap)

    This sample demonstrates how to dynamically change the basemap displayed in a Map.

## Layers


- **Tiled Layers**

    * [ArcGIS tiled layer (URL)](Xamarin.Android/Samples/Layers/ArcGISTiledLayerUrl)

    This sample demonstrates how to add an ArcGISTiledLayer as a base layer in a map. The ArcGISTiledLayer comes from an ArcGIS Server sample web service.


- **Map Image Layers**

    * [ArcGIS map image layer (URL)](Xamarin.Android/Samples/Layers/ArcGISMapImageLayerUrl)

    This sample demonstrates how to add an ArcGISMapImageLayer as a base layer in a map. The ArcGISMapImageLayer comes from an ArcGIS Server sample web service.

    * [Change sublayer visibility](Xamarin.Android/Samples/Layers/ChangeSublayerVisibility)

    This sample demonstrates how to show or hide sublayers of a map image layer.

## Features


- **Feature Layers**

    * [Feature layer (feature service)](Xamarin.Android/Samples/Layers/FeatureLayerUrl)

    This sample demonstrates how to show a feature layer on a map using the URL to the service.

    * [Feature layer query](Xamarin.Android/Samples/Data/FeatureLayerQuery)

    This sample demonstrates how to query a feature layer via feature table.

    * [Change feature layer renderer](Xamarin.Android/Samples/Layers/ChangeFeatureLayerRenderer)

    This sample demonstrates how to change renderer for a feature layer. It also shows how to reset the renderer back to the default.

    * [Feature layer selection](Xamarin.Android/Samples/Layers/FeatureLayerSelection)

    This sample demonstrates how to select features in a feature layer by tapping a MapView.

    * [Feature layer definition expression](Xamarin.Android/Samples/Layers/FeatureLayerDefinitionExpression)

    This sample demonstrates how to apply definition expression to a feature layer for filtering features. It also shows how to reset the definition expression.


- **Feature Tables**

    * [Service feature table (cache)](Xamarin.Android/Samples/Data/ServiceFeatureTableCache)

    This sample demonstrates how to use a feature service in on interaction cache mode.

    * [Service feature table (no cache)](Xamarin.Android/Samples/Data/ServiceFeatureTableNoCache)

    This sample demonstrates how to use a feature service in on interaction no cache mode.

    * [Service feature table (manual cache)](Xamarin.Android/Samples/Data/ServiceFeatureTableManualCache)

    This sample demonstrates how to use a feature service in manual cache mode.

## Display Information


- **Graphics Overlay**

    * [Add graphics (Renderer)](Xamarin.Android/Samples/GraphicsOverlay/AddGraphicsRenderer)

    This sample demonstrates how you add graphics and set a renderer on a graphic overlays.

    * [Identify graphics](Xamarin.Android/Samples/GraphicsOverlay/IdentifyGraphics)

    This sample demonstrates how to identify graphics in a graphics overlay. When you tap on a graphic on the map, you will see an alert message displayed.


- **Symbology**

    * [Render simple markers](Xamarin.Android/Samples/Symbology/RenderSimpleMarkers)

    This sample adds a point graphic to a graphics overlay symbolized with a red circle specified via a SimpleMarkerSymbol.

    * [Render picture markers](Xamarin.Android/Samples/Symbology/RenderPictureMarkers)

    This sample demonstrates how to create picture marker symbols from a URL and embedded resources.

    * [Unique value renderer](Xamarin.Android/Samples/Symbology/RenderUniqueValues)

    This sample demonstrate how to use a unique value renderer to style different features in a feature layer with different symbols. Features do not have a symbol property for you to set, renderers should be used to define the symbol for features in feature layers. The unique value renderer allows for separate symbols to be used for features that have specific attribute values in a defined field.

## Location


- **Display Location**

    * [Display Device Location](Xamarin.Android/Samples/Location/DisplayDeviceLocation)

    This sample demonstrates how you can enable location services and switch between different types of auto pan modes.



[](Esri Tags: ArcGIS Runtime SDK .NET WinRT WinStore WPF WinPhone C# C-Sharp DotNet XAML MVVM)
[](Esri Language: DotNet)
