# WMS service catalog

Connect to a WMS service and show the available layers and sublayers.

![Image of WMS service catalog](WmsServiceCatalog.jpg)

## Use case

WMS services often contain many layers and sublayers. Presenting the layers and sublayers in a UI allows you to explore what is available in the service and add individual layers to a map.

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

This sample shows [Weather Radar Base Reflectivity Mosaics](https://nowcoast.noaa.gov/geoserver/observations/weather_radar/wms?SERVICE=WMS&REQUEST=GetCapabilities) produced by the US NOAA National Weather Service. The service provides weather radar data from the NWS & OAR Multi-Radar/Multi-Sensor (MRMS) System.

## Tags

catalog, OGC, web map service, WMS
