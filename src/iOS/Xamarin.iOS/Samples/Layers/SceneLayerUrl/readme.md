# Scene layer (URL)

Display an ArcGIS scene layer from a URL.

![screenshot](SceneLayerUrl.jpg)

## How to use the sample

Pan and zoom to explore the scene.

## How it works

1. Create an `ArcGISSceneLayer` passing in the URL to a scene layer service.
2. Wait for the user to tap with the `sceneView.GeoViewTapped` event and get the tapped screen point.
3. Call `sceneView.IdentifyLayersAsync(sceneLayer, screenPoint, tolerance, false, 1)` to identify features in the scene.
4. From the resulting `IdentifyLayerResult`, get the list of identified `GeoElements` with `result.GeoElements`.
5. Get the first element in the list, checking that it is a feature, and call `sceneLayer.SelectFeature(feature)` to select it.

## Relevant API

* Scene
* SceneLayer
* SceneView

## About the data

This sample shows a [Portland building layer](https://www.arcgis.com/home/item.html?id=2b721b9e7bef45e2b7ff78a398a33acc) from ArcGIS Online.

## Tags

3D, Buildings, model, scene
