# Find address

Find the location for an address.

![screenshot](FindAddress.jpg)

## Use case

A user can input a raw address into your app's search bar and zoom to the address location.

## How to use the sample

For simplicity, the sample comes loaded with a set of suggested addresses. Click the arrow and select an address or submit your own address to show its location on the map.

## How it works

1. Create a `LocatorTask` using the URL to a locator service.
2. Set the `GeocodeParameters` for the locator task and specify the geocode's attributes.
3. Get the matching results from the `GeocodeResult` using `locatorTask.GeocodeAsync(addressString, geocodeParameters)`.
4. Create a `Graphic` with the geocode result's location and store the geocode result's attributes in the graphic's attributes.
5. Show the graphic in a `GraphicsOverlay`.

## Relevant API

* GeocodeParameters
* GeocodeResult
* LocatorTask

## Tags

address, geocode, locator, search
