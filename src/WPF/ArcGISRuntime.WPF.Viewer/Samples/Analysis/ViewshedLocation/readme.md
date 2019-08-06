# Viewshed for location

Perform a viewshed analysis from a defined vantage point. Viewshed analyses have several configuration options that are demonstrated in this sample.

![screenshot](ViewshedLocation.jpg)

## Use case

A **Viewshed** analysis is a type of visual analysis you can perform on a scene. The viewshed aims to answer the question 'What can I see from a given location?'. The output is an overlay with two different colors - one representing the visible areas (green) and the other representing the obstructed areas (red).

## How to use the sample

Use the sliders to change the properties (heading, pitch, etc.), of the viewshed and see them updated in real time.

## How it works

1. Create a `LocationViewshed` passing in the observer location, heading, pitch, horizontal/vertical angles, and min/max distances.
2. Set the property values on the viewshed instance for location, direction, range, and visibility properties.

## Relevant API

* AnalysisOverlay
* ArcGISSceneLayer
* ArcGISTiledElevationSource
* LocationViewshed
* Viewshed

## About the data

The scene in this sample shows a [buildings layer](https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Buildings_Brest/SceneServer/layers/0) in Brest, France.

## Tags

3D, AnalysisOverlay, LocationViewshed, Scene, frustum, viewshed, visibility analysis
