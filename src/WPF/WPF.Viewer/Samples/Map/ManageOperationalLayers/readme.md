# Manage operational layers

Add, remove, and reorder operational layers in a map.

![Image of manage operational layers](ManageOperationalLayers.jpg)

## Use case

Operational layers display the primary content of the map and usually provide dynamic content for the user to interact with (as opposed to basemap layers that provide context).

The order of operational layers in a map determines the visual hierarchy of layers in the view. You can bring attention to a specific layer by rendering above other layers.

## How to use the sample

When the app starts, a list displays the operational layers that are currently displayed in the map. Right-click on the list item to remove the layer, or left-click to move it to the top. The map will be updated automatically.

The second list shows layers that have been removed from the map. Click one to add it to the map.

## How it works

1. Get the operational layers from the map using `map.OperationalLayers`.
2. Add or remove layers using `layerList.Add(layer)` and `layerList.Remove(layer)` respectively. The last layer in the list will be rendered on top.

## Relevant API

* ArcGISMapImageLayer
* Map
* MapView
* MapView.OperationalLayers

## Additional information

You cannot add the same layer to the map multiple times or add the same layer to multiple maps. Instead, create a new layer using the `FeatureTable`.

## Tags

add, delete, layer, map, remove
