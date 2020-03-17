# Take screenshot

Take a screenshot of the map.

![Image of take screenshot](TakeScreenshot.jpg)

## Use case

GIS users may want to export a screenshot of a map to enable sharing as an image or printing.

## How to use the sample

Pan and zoom to find an interesting location, then use the button to take a screenshot. The screenshot will be displayed. Note that there may be a small delay if the map is still rendering when you push the button.

## How it works

1. Wait for the MapView to finish rendering the Map.
2. Call `mapView.ExportImageAsync()` to get the `RuntimeImage`, which is a bitmap image.
3. Use the `RuntimeImage.ToImageSourceAsync` extension method to get a copy of the image that is compatible with native image display controls.

## Relevant API

* GeoView.DrawStatus
* GeoView.DrawStatusChanged
* GeoView.ExportImageAsync
* RuntimeImage
* RuntimeImage.ToImageSourceAsync

## Tags

capture, export, image, print, screen capture, screenshot, share, shot