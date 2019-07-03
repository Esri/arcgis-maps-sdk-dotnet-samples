# Set map spatial reference

Specify a map's spatial reference.

![screenshot](SetMapSpatialReference.jpg)

## Use case

Choosing the correct spatial reference is important for ensuring accurate projection of data points to a map.  

## How to use the sample

Simply run the app.

## How it works

1. Instantiate an `Map` object using a spatial reference e.g. `ArcGISMap(SpatialReference.Create(54024))`.
2. Instantiate a `Basemap` object using an `ArcGISMapImageLayer` object.
3. Set the base map to the map.
4. Display the map in a map view.

## Relevant API

* Map
* ArcGISMapImageLayer
* Basemap
* MapView
* SpatialReference

## Additional information

Operational layers will automatically project to this spatial reference when possible.

## Tags

SpatialReference, WKID, project
