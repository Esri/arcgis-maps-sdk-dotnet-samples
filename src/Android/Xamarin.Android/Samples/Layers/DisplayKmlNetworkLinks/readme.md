# Display KML network links

KML files can reference other KML files on the network as well as refresh intervals. This can be used to create a map that will periodically refresh itself with the latest data. This sample demonstrates how to display a file with a network link.

![](DisplayKmlNetworkLinks.jpg)

Note that although the base KML file is loaded locally, all of the content is being downloaded from a website every few seconds.

## How to use the sample

The sample will load the KML file automatically. The data shown should refresh automatically every few seconds.

## Relevant API

* `KmlDataset(Uri)`
* `KmlLayer(KmlDataset)`

## Offline data

This sample uses the radar.kmz file, which can be found on [ArcGIS Online](https://arcgisruntime.maps.arcgis.com/home/item.html?id=600748d4464442288f6db8a4ba27dc95).

## About the map

This map shows the current air traffic in parts of Europe with heading, altitude, and ground speed. Additionally, noise levels from ground monitoring stations are shown.

## Tags

KML, KMZ, Network Link,