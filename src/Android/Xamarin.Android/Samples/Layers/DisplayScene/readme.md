# Display a scene

Display a scene with a terrain surface and some imagery.

![screenshot](DisplayScene.jpg)

## How it works

1. Create an `Scene` object with the `Basemap.CreateImagery()` basemap.
2. Create an `ArcGISTiledElevationSource` object and add it to the scene's base surface.
3. Create a `SceneView` object to display the map.
4. Set the scene to the scene view.

## Relevant API

* Scene
* ArcGISTiledElevationSource
* SceneView

## Tags

3D, basemap, scene, sceneview, surface
