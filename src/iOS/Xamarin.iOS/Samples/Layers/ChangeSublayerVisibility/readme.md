# Map image layer sublayer visibility

Change the visibility of sublayers.

![screenshot](ChangeSublayerVisibility.jpg)

## Use case

A map image layer may contain many sublayers such as different types of roads in a road network or city, county, and state borders in a US map. The user may only be interested in a subset of these sublayers. Or, perhaps showing all of the sublayers would show too much detail. In these cases, you can hide certain sublayers by changing their visibility.

## How to use the sample

Click the button to see a list of layers. Each sublayer has a check box which can be used to toggle the visibility of the sublayer.

## How it works

1. Create an `ArcGISMapImageLayer` object with the URL to a map image service.
2. Get the list of sublayers with `mapImageLayer.Sublayers`.
3. For each layer in the sublayer list, set its visible property to true or false.

## Relevant API

* ArcGISMapImageLayer

## Tags

layers, sublayers, visibility
