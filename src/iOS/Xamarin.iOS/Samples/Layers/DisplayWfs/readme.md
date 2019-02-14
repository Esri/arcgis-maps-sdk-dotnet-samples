# Display a WFS layer

Display a layer from a WFS service, requesting only features for the current extent.

![](DisplayWfs.jpg)

## Use case

WFS is an open standard with functionality similar to ArcGIS feature services. Runtime support for WFS allows you to interoperate with open systems, which are often used in inter-agency efforts, like those for disaster relief.

## How to use the sample

Pan and zoom to see features relevant to the current map extent.

## How it works

1. Create a `WfsFeatureTable` with a URL.
2. Create a feature layer from the feature table and add it to the map.
3. Listen for the `MapView.NavigationCompleted` event to detect when the user has stopped navigating the map.
4. When the user is finished navigating, use `PopulateFromServiceAsync` to load the table with data for the current visible extent.

## Relevant API

* WfsFeatureTable
* WfsFeatureTable.PopulateFromServiceAsync
* MapView.NavigationCompleted

## About the data

This service shows building footprints for downtown Seattle. For additional information, see the underlying service on [ArcGIS Online](https://arcgisruntime.maps.arcgis.com/home/item.html?id=1b81d35c5b0942678140efc29bc25391).

## Tags

OGC, WFS, feature, web, service, layers, browse, catalog, interaction cache