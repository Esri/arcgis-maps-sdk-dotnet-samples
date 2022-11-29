# Display a scene

Display a scene with a terrain surface and some imagery.

![Image of display scene](DisplayScene.jpg)

## Use case

Scene views are 3D representations of real-world areas and objects. Scene views are helpful for visualizing complex datasets where 3D relationships, topography, and elevation of elements are important factors.

## How to use the sample

When loaded, the sample will display a scene. Pan and zoom to explore the scene.

## How it works

1. Create a `Scene` object with the `BasemapStyle.ArcGISImageryStandard` basemap style.
2. Create an `ArcGISTiledElevationSource` object and add it to the scene's base surface.
3. Create a `SceneView` object to display the map.
4. Set the scene to the scene view.

## Relevant API

* Scene
* ArcGISTiledElevationSource
* SceneView

## Tags

3D, basemap, elevation, scene, surface