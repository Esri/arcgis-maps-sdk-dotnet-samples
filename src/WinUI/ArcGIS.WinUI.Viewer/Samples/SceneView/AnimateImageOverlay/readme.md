# Animate images with image overlay

Animate a series of images with an image overlay.

![Image of animate images with image overlay](AnimateImageOverlay.jpg)

## Use case

An image overlay is useful for displaying fast and dynamic images; for example, rendering real-time sensor data captured from a drone. Each frame from the drone becomes a static image which is updated on the fly as the data is made available.

## How to use the sample

The application loads a map of the Southwestern United States. Click the "Start" or "Stop" buttons to start or stop the radar animation. Use the drop down menu to select how quickly the animation plays. Move the slider to change the opacity of the image overlay.

## How it works

1. Create an `ImageOverlay` and add it to the `SceneView`.
2. Set up a timer with an initial interval time of 67ms, which will display approximately 15 image frames per second.
3. Connect an event to the timer.
4. Create a new `ImageFrame` every timeout and set it on the image overlay.

## Relevant API

* ImageFrame
* ImageOverlay
* SceneView

## Offline data

This sample uses [radar images](https://arcgisruntime.maps.arcgis.com/home/item.html?id=9465e8c02b294c69bdb42de056a23ab1) captured by the National Weather Service.

## About the data

These radar images were captured by the US National Weather Service (NWS). They highlight the Pacific Southwest sector which is made up of part the western United States and Mexico. For more information visit the [National Weather Service](https://www.weather.gov/jetstream/gis) website.

## Additional information

The supported image formats are GeoTIFF, TIFF, JPEG, and PNG. `ImageOverlay` does not support the rich processing and rendering capabilities of a `RasterLayer`. Use `Raster` and `RasterLayer` for static image rendering, analysis, and persistence.

## Tags

3d, animation, drone, dynamic, image frame, image overlay, real time, rendering