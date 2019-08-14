# Scene layer selection

Identify GeoElements in a scene layer.

![screenshot](SceneLayerSelection.jpg)

## How to use the sample

Click on a building in the scene layer to select it. Deselect buildings by clicking away from the buildings.

## How it works

1. Create an `ArcGISSceneLayer` passing in the URL to a scene layer service.
2. Wait for the user to tap with the `sceneView.GeoViewTapped` event and get the tapped screen point.
3. Call `sceneView.IdentifyLayersAsync(sceneLayer, screenPoint, tolerance, false, 1)` to identify features in the scene.
4. From the resulting `IdentifyLayerResult`, get the list of identified `GeoElements` with `result.GeoElements`.
5. Get the first element in the list, checking that it is a feature, and call `sceneLayer.SelectFeature(feature)` to select it.

## Relevant API

* ArcGISSceneLayer
* Scene
* SceneView

## About the data

This sample shows a [Berlin building layer](https://www.arcgis.com/home/item.html?id=31874da8a16d45bfbc1273422f772270) from ArcGIS Online.

## Tags

3D, Buildings, Search and Query, model, scene
