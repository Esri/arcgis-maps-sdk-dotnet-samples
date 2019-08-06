# Render simple markers

Show a simple marker symbol on a map.

![screenshot](RenderSimpleMarkers.jpg)

## Use case

Customize the appearance of a point suitable for the data. For example, a point on the map styled with a circle could represent a drilled borehole location, whereas a cross could represent the location of an old coal mine shaft.

## How to use the sample

The sample loads with a predefined simple marker symbol, set as a red circle.

## How it works

1. Create a `SimpleMarkerSymbol(SimpleMarkerSymbol.Style, color, size)`.
2. Create a `Graphic` passing in a `Point` and the simple marker symbol as parameters.
3. Add the graphic to the graphics overlay with `graphicsOverlay.Graphics.Add(graphic)`.

## Relevant API

* Graphic
* GraphicsOverlay
* Point
* SimpleMarkerSymbol

## Tags

SimpleMarkerSymbol, symbol
