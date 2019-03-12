# Display KML Tours

Display tours in KML files.

![](DisplayKmlTours.jpg)

## Use case

KML, the file format used by Google Earth, supports creating tours, which can control the viewpoint of the scene, hide and show content, and play audio. Tours allow you to easily share tours of geographic locations, which can be augmented with rich multimedia. Runtime allows you to consume these tours using a simple API.

## How to use the sample

The sample will load the KMZ file automatically. When a tour is found, the _Play_ button will be enabled. Use _Play_ and _Pause_ to control the tour. When you're ready to show the tour, use the reset button to return the tour to the unplayed state.

## How it works

1. The KML file is opened and added to a layer.
2. The KML tour controller is created. Buttons are wired up to the `Play`, `Pause`, and `Reset` methods.
3. An iterative method explores the tree of KML content and finds the first KML tour. Once a tour is found, it is provided to the KML tour controller.
4. The buttons are enabled to allow the user to play, pause, and reset the tour. 

## Relevant API

* KmlTourController
* KmlTourController.Tour
* KmlTourController.Play()
* KmlTourController.Pause()
* KmlTourController.Reset()
* KmlTour

## Offline data

Data will be downloaded by the sample viewer automatically.

* [Esri_tour.kmz](https://arcgisruntime.maps.arcgis.com/home/item.html?id=f10b1d37fdd645c9bc9b189fb546307c)

## About the data

This sample uses a custom tour created by a member of the ArcGIS Runtime SDK samples team. When you play the tour, you'll see a narrated journey through some of Esri's offices.

## Additional information

See [Google's documentation](https://developers.google.com/kml/documentation/touring) for information about authoring KML tours.

## Tags

KML, tour, story, interactive, narration, play, pause, animation