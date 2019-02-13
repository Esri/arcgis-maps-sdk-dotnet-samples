# Manage operational layers

Add, remove, and reorder operational layers in a map.

![Manage Operational Layers App](ManageOperationalLayers.jpg)

## How to use the sample

Drag layers between the lists to add and remove them from the map. Drag layers within the lists to reorder them.

## How it works

A map's `OperationalLayers` collection controls which layers are visualized. `MapView` automatically updates the visualization when the `Map` changes. A separate collection holds layers that have been removed from the operational layers. 

## Relevant API

* Map
* ArcGISMapImageLayer
* MapView
* MapView.OperationalLayers

## Tags

Map, scene, operational, hide, remove, add
