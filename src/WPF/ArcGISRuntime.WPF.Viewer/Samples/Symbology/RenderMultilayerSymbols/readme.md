# Render Multilayer symbols

Shows multilayer symbols on a map.

![Image of multilayer symbols](RenderMultilayerSymbols.jpg)

## Use case

Display how to create multilayer symbol using APIs. Use all pre-defined 2D simple symbol styles and create an equivalent multilayer symbol using API.

## How to use the sample

The sample loads multilayer symbols for point, line and polygons. Note, there is no multilayer respresentation of a simple TextSymbol symbol.

## How it works

1. Create multilayer symbols for each predefined 2D symbol style.
2. Create graphics passing in a geometry and the associated symbol.
3. Add graphics to the graphics overlay with `graphicsOverlay.Graphics.Add(graphic)`
## Relevant API

* Graphic
* GraphicsOverlay
* Point
* SimpleMarkerSymbol

## Tags

symbol
