## Sample Table Of Contents
## Maps


- **MapView**

    * [Change viewpoint](ArcGISRuntime.Windows.Samples/Samples/MapView/ChangeViewpoint)

    This sample demonstrates how the current viewpoint can be changed. This Sample opens with a default service viewpoint. Each subsequent button click will change the viewpoint using different methods (one to a provided extent, another a point and scale, and another to rotate the viewpoint by 90 degrees.

    * [Map rotation](ArcGISRuntime.Windows.Samples/Samples/MapView/MapRotation)

    This sample demonstrates how to rotate a map.


- **Map**

    * [Access load status](ArcGISRuntime.Windows.Samples/Samples/Map/AccessLoadStatus)

    This sample demonstrates how to access the Maps' LoadStatus. The LoadStatus will be considered loaded when the following are true: The Map has a valid SpatialReference and the Map has an been set to the MapView.

    * [Set initial map location](ArcGISRuntime.Windows.Samples/Samples/Map/SetInitialMapLocation)

    This sample creates a map with a standard ESRI Imagery with Labels basemap that is centered on a latitude and longitude location and zoomed into a specific level of detail.

    * [Set initial map area](ArcGISRuntime.Windows.Samples/Samples/Map/SetInitialMapArea)

    This sample displays a map at a specific viewpoint. In this sample a viewpoint is constructed from an envelope defined by minimum (x,y) and maximum (x,y) values. The map's initialViewpoint is set to this viewpoint before the map is loaded. Upon loading the map zooms to this initial area.

    * [Set map spatial reference](ArcGISRuntime.Windows.Samples/Samples/Map/SetMapSpatialReference)

    This sample demonstrates how you can set the spatial reference on a Map and all the layers would project accordingly.

## Layers


- **Tiled Layers**

    * [ArcGIS tiled layer (URL)](ArcGISRuntime.Windows.Samples/Samples/Layers/ArcGISTiledLayerUrl)

    The Tiled Layer from URL sample is one of the most basic .Net SDK samples. This example covers using ArcGISTiledLayer as a Basemap, as well as adding the required map and MapView elements. By default, this map supports basic zooming and panning operations.


- **Map Image Layers**

    * [ArcGIS map image layer (URL)](ArcGISRuntime.Windows.Samples/Samples/Layers/ArcGISMapImageLayerUrl)

    The image layer from URL sample is one of the most basic .Net SDK samples. This example covers using ArcGISMapImageLayer as a Basemap, as well as adding the required map and MapView elements. By default, this map supports basic zooming and panning operations.

## Features


- **Feature Layers**

    * [Feature layer definition expression](ArcGISRuntime.Windows.Samples/Samples/Layers/FeatureLayerDefinitionExpression)

    This sample demonstrates how to apply definition expression to a feature layer for filtering features. It also shows how to reset the definition expression.


- **Feature Tables**

    * [Service feature table (cache)](ArcGISRuntime.Windows.Samples/Samples/Data/ServiceFeatureTableCache)

    This sample demonstrates how to use a feature service in on interaction cache mode.

    * [Service feature table (no cache)](ArcGISRuntime.Windows.Samples/Samples/Data/ServiceFeatureTableNoCache)

    This sample demonstrates how to use a feature service in on interaction no cache mode.

    * [Service feature table (manual cache)](ArcGISRuntime.Windows.Samples/Samples/Data/ServiceFeatureTableManualCache)

    This sample demonstrates how to use a feature service in manual cache mode.

    * [Feature layer query](ArcGISRuntime.Windows.Samples/Samples/Data/FeatureLayerQuery)

    This sample demonstrates how to return features from a feature layer using an attribute query on the underlying feature table.

## Display Information


- **Graphics Overlays**

    * [Add graphics (Renderer)](ArcGISRuntime.Windows.Samples/Samples/GraphicsOverlay/AddGraphicsRenderer)

    This sample demonstrates how you add graphics and set a renderer on a graphic overlays.


- **Symbology**

    * [Render simple markers](ArcGISRuntime.Windows.Samples/Samples/Symbology/RenderSimpleMarkers)

    This sample adds a point graphic to a graphics overlay symbolized with a red circle specified via a SimpleMarkerSymbol.

    * [Render picture markers](ArcGISRuntime.Windows.Samples/Samples/Symbology/RenderPictureMarkers)

    This sample demonstrates how to create picture marker symbols from a URL and embedded resources.

    * [Render unique values](ArcGISRuntime.Windows.Samples/Samples/Symbology/RenderUniqueValues)

    This sample demonstrate how to use a unique value renderer to style different features in a feature layer with different symbols. Features do not have a symbol property for you to set, renderers should be used to define the symbol for features in feature layers. The unique value renderer allows for separate symbols to be used for features that have specific attribute values in a defined field.



[](Esri Tags: ArcGIS Runtime SDK .NET WinRT WinStore WPF WinPhone C# C-Sharp DotNet XAML MVVM)
[](Esri Language: DotNet)
