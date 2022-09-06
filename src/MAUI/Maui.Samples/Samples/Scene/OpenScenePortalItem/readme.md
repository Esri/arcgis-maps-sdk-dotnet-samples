# Open scene (portal item)

Open a web scene from a portal item.

![Image of open a scene portal item](OpenScenePortalItem.jpg)

## Use case

A scene is symbolized geospatial content that allows you to visualize and analyze geographic information in an intuitive and interactive 3D environment. Web scenes are an ArcGIS format for storing scenes in ArcGIS Online or portal. Scenes can be used to visualize a complex 3D environment like a city.

## How to use the sample

When the sample opens, it will automatically display the scene from ArcGIS Online. Pan and zoom to explore the scene.

## How it works

To open a web scene from a portal item:

1. Create a `PortalItem` with an item ID pointing to a web scene.
2. Create a `Scene` passing in the portal item.
3. Set the scene to a `SceneView` to display it.

## Relevant API

* PortalItem
* Scene
* SceneView

## About the data

This sample uses a [Berlin, Germany](https://www.arcgis.com/home/item.html?id=31874da8a16d45bfbc1273422f772270) scene hosted on ArcGIS Online.

## Tags

portal, scene, web scene
