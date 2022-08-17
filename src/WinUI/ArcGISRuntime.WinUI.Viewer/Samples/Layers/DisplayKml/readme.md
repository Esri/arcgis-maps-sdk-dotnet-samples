# Display KML

Display KML from a URL, portal item, or local KML file.

![Image of display KML](DisplayKml.jpg)

## Use case

Keyhole Markup Language (KML) is a data format used by Google Earth. KML is popular as a transmission format for consumer use and for sharing geographic data between apps. You can use Runtime to display KML files, with full support for a variety of features, including network links, 3D models, screen overlays, and tours.

## How to use the sample

Use the UI to select a source. A KML file from that source will be loaded and displayed in the scene.

## How it works

1. To create a KML layer from a URL, create a `KmlDataset` using the URL to the KML file. Then pass the dataset to the `KmlLayer` constructor.
2. To create a KML layer from a portal item, construct a `PortalItem` with a `Portal` and the KML portal item ID. Pass the portal item to the `KmlLayer` constructor.
3. To create a KML layer from a local file, create a `KmlDataset` using the absolute file path to the local KML file. Then pass the dataset to the `KmlLayer` constructor.
4. Add the layer as an operational layer to the scene with `scene.OperationalLayers.Add(kmlLayer)`.

## Relevant API

* KmlDataset
* KmlLayer

## Offline data

This sample uses [US state capitals](https://arcgisruntime.maps.arcgis.com/home/item.html?id=324e4742820e46cfbe5029ff2c32cb1f) KML file. It is downloaded from ArcGIS Online automatically.

## About the data

This sample displays three different KML files:

* From URL - this is a map of the significant weather outlook produced by NOAA/NWS. It uses KML network links to always show the latest data.
* From local file - this is a map of U.S. state capitals. It doesn't define an icon, so the default pushpin is used for the points.
* From portal item - this is a map of U.S. states.

## Tags

keyhole, KML, KMZ, OGC
