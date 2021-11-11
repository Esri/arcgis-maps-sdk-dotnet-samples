# Render Simple symbols

Show simple 2D symbols on a map.

![Image of simple symbol](RenderSimpleSymbols.jpg)

## Use case

Display all pre-defined 2D simple symbols on map.

## How to use the sample

The sample loads with graphics that are symbolized with predefined 2D symbols for points, line , polygons and text.

## How it works

1. Create simple symbols for each predefined style for line, point and polygons.
2. Create graphics passing in a geometry and the associated symbol.
3. Add graphics to the graphics overlay with `graphicsOverlay.Graphics.Add(graphic)`.

## Relevant API

* Graphic
* GraphicsOverlay
* Point
* SimpleMarkerSymbol
* PictureMarkerSymbol
* SimpleLineSymbol
* SimpleFillSymbol

## Tags

symbol
