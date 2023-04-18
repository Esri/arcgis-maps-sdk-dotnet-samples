# ArcGIS map image layer

Add an ArcGIS Map Image Layer from a URL to a map.

![screenshot](arcgismapimagelayerurl.jpg)

## Use case

Map image layers are also referred to as dynamic map services. These services generate images dynamically on the server and send the resulting image to the requesting client. This is useful when rendering complex cartography or large amounts of data in a connected environment, as the client simply displays the image that the server returns. This can result in more accurate and faster-performing layers.

## How to use the sample

Run the sample and view the map image layer. As you navigate the map, the layer issues image requests for the new map extent. The resulting images are rendered on the screen.

## How it works

1. Create a `Map`.
2. Create an `ArcGISMapImageLayer` with the URL to a map image service.
3. Add it to the map's operational layers collection.

## Relevant API

* ArcGISMapImageLayer

## Tags

display, image, layer, map
