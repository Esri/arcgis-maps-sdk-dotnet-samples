# Reverse geocode

Use an online service to find the address for a tapped point.

![](ReverseGeocode.jpg)

## Use case

Reverse geocoding - finding the address for a point - is a fundamental GIS task. For example, you might use a geocoder to find a customer's delivery address based on the location returned by their device's GPS.

## How to use the sample

Tap the map to see the nearest address displayed in a callout.

## How it works

1. Create a `LocatorTask` object using a URL.
2. Set the `GeocodeParameters` for the `LocatorTask` and specify the geocoder's attributes.
3. Get the matching results from the `GeocodeResult` using  `LocatorTask.reverseGeocodeAsync`.
4. Show the results using a `PictureMarkerSymbol` and add the symbol to a `Graphic` in the `GraphicsOverlay`.

## Relevant API

* GeocodeParameters
* GraphicsOverlay
* LocatorTask
* MapView
* PictureMarkerSymbol
* ReverseGeocodeParameters
* TileCache



