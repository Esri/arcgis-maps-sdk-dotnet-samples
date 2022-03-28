# Nearest vertex

Find the closest vertex and coordinate of a geometry to a point.

![Image of nearest vertex](NearestVertex.jpg)

## Use case

Determine the shortest distance between a location and the boundary of an area. For example, developers can snap imprecise user clicks to a geometry if the tap is within a certain distance of the geometry.

## How to use the sample

Tap anywhere on the map. An orange cross will show at that location. A blue circle will show the polygon's nearest vertex to the point that was clicked. A red diamond will appear at the coordinate on the geometry that is nearest to the point that was clicked. If tapped inside the geometry, the red and orange markers will overlap. The information box showing distance between the tapped point and the nearest vertex/coordinate will be updated with every new location clicked.

## How it works

1. Get a `Geometry` and a `Point` to check the nearest vertex against.
2. Call `GeometryEngine.NearestVertex(inputGeometry, point)`.
3. Use the returned `ProximityResult` to get the `Point` representing the polygon vertex, and to determine the distance between that vertex and the tapped point.
4. Call `GeometryEngine.NearestCoordinate(inputGeometry, point)`.
5. Use the returned `ProximityResult` to get the `Point` representing the coordinate on the polygon, and to determine the distance between that coordinate and the tapped point.

## Relevant API

* GeometryEngine
* ProximityResult

## Additional information

The value of `ProximityResult.distance` is planar (Euclidean) distance. Planar distances are only accurate for geometries that have a defined projected coordinate system, which maintain the desired level of accuracy. The example polygon in this sample is defined in California State Plane Coordinate System - Zone 5 (WKID 2229), which maintains accuracy near Southern California. Accuracy declines outside the state plane zone.

## Tags

analysis, coordinate, geometry, nearest, proximity, vertex