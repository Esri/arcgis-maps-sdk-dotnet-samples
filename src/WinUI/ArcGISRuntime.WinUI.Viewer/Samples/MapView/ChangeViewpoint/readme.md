# Change viewpoint

Set the map view to a new viewpoint.

![Image of change viewpoint](ChangeViewpoint.jpg)

## Use case

Programatically navigate to a specified location in the map or scene. Use this to focus on a particular point or area of interest.

## How to use the sample

The map view has several methods for setting its current viewpoint. Select a viewpoint from the UI to see the viewpoint changed using that method.

## How it works

1. Create a new `Map` object and set it to the `MapView` object.
2. Change the map's `Viewpoint` using one of the available methods:
  * Use `MapView.SetViewpointAsync()` to pan to a viewpoint.
  * Use `MapView.SetViewpointCenterAsync()` to center the viewpoint on a `Point`.
  * Use `MyMapView.SetViewpointScaleAsync()` to set a distance from the ground using a scale.
  * Use `MapView.SetViewpointGeometryAsync()` to set the viewpoint to a given `Geometry`.

## Relevant API

* Geometry
* Map
* MapView
* Point
* Viewpoint

## Additional information

Below are some other ways to set a viewpoint:

* SetViewpoint
* SetViewpointAsync
* SetViewpointCenterAsync
* SetViewpointGeometryAsync
* SetViewpointRotationAsync
* SetViewpointScaleAsync

## Tags

animate, extent, pan, rotate, scale, view, zoom