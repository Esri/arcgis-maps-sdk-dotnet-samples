# Map initial extent

Display the map at an initial viewpoint representing a bounding geometry.

![Image of map initial extent](SetInitialMapArea.jpg)

## Use case

Setting the initial viewpoint is useful when a user wishes to first load the map at a particular area of interest.

## How to use the sample

As application is loading, initial view point is set and map view opens at the given location.

## How it works

1. Instantiate a `Map` object.
2. Instantiate a `Viewpoint` object using an `Envelope` object.
3. Set the starting location of the map with `map.InitialViewpoint`.
4. Set the map in the `MapView`.

## Relevant API

* Envelope
* Map
* MapView
* Point
* Viewpoint

## Tags

extent, envelope, initial, viewpoint, zoom