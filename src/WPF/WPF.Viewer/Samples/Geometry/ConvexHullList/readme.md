# Convex hull list

Generate convex hull polygon(s) from multiple input geometries.

![Image of convex hull list](ConvexHullList.jpg)

## Use case

Creating a convex hull allows for analysis to define the polygon with the least possible perimeter that encloses a group of geometric shapes. As a visual analogy, consider a set of nails in a board where the convex hull is a rubber band stretched around the outermost nails.

## How to use the sample

Click the 'Create Convex Hull' button to create convex hull(s) from the polygon graphics. If the 'Union' checkbox is checked, the resulting output will be one polygon being the convex hull for the two input polygons. If the 'Union' checkbox is un-checked, the resulting output will have two convex hull polygons - one for each of the two input polygons. Click the 'Reset' button to start over.

## How it works

1. Create an `Map` and display it in a `MapView`.
2. Create two input polygon graphics and add them to a `GraphicsOverlay`.
3. Call `inputGeometries.ConvexHull(boolean)`, specifying a list of geometries for which to generate the convex hull. Set the boolean parameter to `true` to generate a convex hull for the union of the geometries. Set it to `false` to create a convex hull for each individual geometry.
4. Loop through the returned geometries and add them as graphics for display on the map.

## Relevant API

* GeometryEngine.ConvexHull
* Graphic.ZIndex
* GraphicsOverlay

## Tags

analysis, geometry, outline, perimeter, union