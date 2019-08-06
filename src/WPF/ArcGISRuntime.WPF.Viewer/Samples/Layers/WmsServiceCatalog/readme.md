# WMS service catalog

Connect to a WMS service and show the available layers and sublayers. Layers are shown in a hierarchy. Selecting a group layer will recursively select all sublayers for display.

![screenshot](WmsServiceCatalog.jpg)

## How to use the sample

1. Open the sample. A hierarchical list of layers and sublayers will appear.
2. Select a layer to enable it for display. If the layer has any children, the children will also be selected.

## How it works

1. A `WmsService` is created and loaded.
2. `WmsService` has a `ServiceInfo` property, which is a `WmsServiceInfo`. `WmsServiceInfo` has a `WmsLayerInfo` object for each layer (excluding sublayers) in the `LayerInfos` collection.
3. A method is called to recursively discover sublayers for each layer. Layers are wrapped in a view model and added to a list.
    * The view model has a `Select` method which recursively selects or deselects itself and sublayers.
    * The view model tracks the children and parent of each layer.
4. Once the layer selection has been updated, another method is called to create a new `WmsLayer` from a list of selected `WmsLayerInfo`.

## Relevant API

* WmsLayer(List<WmsLayerInfo>)
* WmsLayerInfo
* WmsService
* WmsServiceInfo

## About the data

The map in this sample shows a weather forecast produced by the U.S. National Weather Service. The map shows fronts, highs, and lows, as well as areas of forecast precipitation. For more information, see the [service page](https://idpgis.ncep.noaa.gov/arcgis/rest/services/NWS_Forecasts_Guidance_Warnings/natl_fcst_wx_chart/MapServer).

## Tags

OGC, WmsLayer, WmsLayerInfo, WmsService, WmsServiceInfo, web map service
