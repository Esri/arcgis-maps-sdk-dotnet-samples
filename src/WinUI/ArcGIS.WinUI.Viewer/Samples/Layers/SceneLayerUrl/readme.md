# Scene layer (URL)

Display an ArcGIS scene layer from a URL.

![Image of scene layer URL](SceneLayerUrl.jpg)

## Use case

Adding a scene layer from a URL allows you to author the scene layer elsewhere in the platform, say with ArcGIS Pro or CityEngine, and then add that scene layer to a scene in ArcGIS Maps SDK. Loading a scene layer from a URL also permits the layer source to change dynamically without updating the code. Each scene layer added to a scene can assist in performing helpful visual analysis. For example, if presenting the results of a shadow analysis of a major metropolitan downtown area in 3D, adding a scene layer of 3D buildings to the scene that could be toggled on/off would help to better contextualize the source of the shadows.

## How to use the sample

Pan and zoom to explore the scene.

## How it works

1. Create an `ArcGISSceneLayer` passing in the URL to a scene layer service.
2. Create a `Scene` and add the scene layer to its operational layers.
3. Create a `Surface` object and add an `ArcGISTiledElevationSource` to it.
4. Set the surface to the scene's base surface.
5. Create a `SceneView` with the scene.

## Relevant API

* ArcGISScene
* ArcGISTiledElevationSource
* Scene
* SceneView
* Surface

## About the data

This sample shows a [Portland, Oregon USA Scene](https://www.arcgis.com/home/item.html?id=2b721b9e7bef45e2b7ff78a398a33acc) hosted on ArcGIS Online.

## Tags

3D, buildings, model, Portland, scene, service, URL