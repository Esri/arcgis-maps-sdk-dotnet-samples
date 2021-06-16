# Mobile map (search and route)

Display maps and use locators to enable search and routing offline using a Mobile Map Package.

![Image of mobile map search and route](MobileMapSearchAndRoute.jpg)

## Use case

Mobile map packages make it easy to transmit and store the necessary components for an offline map experience including: transportation networks (for routing/navigation), locators (address search, forward and reverse geocoding), and maps. 

A field worker might download a mobile map package to support their operations while working offline.

## How to use the sample

A list of maps from a mobile map package will be displayed. If the map contains transportation networks, the list item will have a navigation icon. Click on a map in the list to open it. If a locator task is available, click on the map to reverse geocode the location's address. If transportation networks are available, a route will be calculated between geocode locations.

## How it works

1. Create a `MobileMapPackage` using `MobileMapPackage.OpenAsync(path)`.
2. Get a list of maps using the `Maps` property.
3. If the package has a locator, access it using the `LocatorTask` property.
4. To see if a map contains transportation networks, check each map's `TransportationNetworks` property.

## Relevant API

* GeocodeResult
* MobileMapPackage
* ReverseGeocodeParameters
* Route
* RouteParameters
* RouteResult
* RouteTask
* TransportationNetworkDataset

## Offline data

This sample uses the [San Francisco](https://www.arcgis.com/home/item.html?id=260eb6535c824209964cf281766ebe43) mobile map package.

## Tags

disconnected, field mobility, geocode, network, network analysis, offline, routing, search, transportation