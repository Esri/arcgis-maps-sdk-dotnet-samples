# Add graphics with symbols

Use a symbol style to display a graphic on a graphics overlay.

![Image of Add graphics with symbols](AddGraphicsWithSymbols.jpg)

## Use case

Allows you to customize a graphic by assigning a unique symbol. For example, you may wish to display individual graphics for different landmarks across a region, and to style each one with a unique symbol.  

## How to use the sample

Observe the graphics on the map.

## How it works

1. Create a `GraphicsOverlay` and add it to the `MapView`.
2. Create a `Symbol` such as `SimpleMarkerSymbol`, `SimpleLineSymbol` or `SimpleFillSymbol`.
3. Create a `Graphic`, specifying a `Geometry` and a `Symbol`.
4. Add the `Graphic` to the `GraphicsOverlay`.

## Relevant API

* Geometry
* Graphic
* GraphicsOverlay
* SimpleFillSymbol
* SimpleLineSymbol
* SimpleMarkerSymbol

## Additional information

To set a symbol style across a number of graphics (e.g. showing trees as graphics sharing a symbol in a park), see the "Add graphics with renderer" sample. 

## Tags

SimpleFillSymbol, SimpleLineSymbol, SimpleMarkerSymbol