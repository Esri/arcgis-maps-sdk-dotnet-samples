# Create and save KML file

Construct a KML document and save it as a KMZ file.

![Image of create and save KML file](CreateAndSaveKmlFile.jpg)

## Use case

If you need to create and save data on the fly, you can use KML to create points, lines, and polygons by sketching on the map, customizing the style, and serializing them as KML nodes in a KML Document. Once complete, you can share the KML data with others that are using a KML reading application, such as ArcGIS Earth.

## How to use the sample

Click on one of the buttons in the middle row to start adding a geometry. Click on the map view to place vertices. Click the "Complete Sketch" button to add the geometry to the KML document as a new KML placemark. Use the style interface to edit the style of the placemark. If you do not wish to set a style, click the "Don't Apply Style" button. When you are finished adding KML nodes, click on the "Save KMZ file" button to save the active KML document as a .kmz file on your system. Use the "Reset" button to clear the current KML document and start a new one.

## How it works

1. Create a `KmlDocument`
2. Create a `KmlDataset` using the `KmlDocument`.
3. Create a `KmlLayer` using the `KmlDataset` and add it to `Map.OperationalLayers`.
4. Create `Geometry` using `GeometryEditor`.
5. Project that `Geometry` to WGS84 using `GeometryEngine.Project`.
6. Create a `KmlGeometry` object using that projected `Geometry`.
7. Create a `KmlPlacemark` using the `KmlGeometry`.
8. Add the `KmlPlacemark` to the `KmlDocument`.
9. Set the `KmlStyle` for the `KmlPlacemark`.
10. When finished with adding `KmlPlacemark` nodes to the `KmlDocument`, save the `KmlDocument` to a file using the `SaveAsAsync` method.

## Relevant API

* GeometryEditor
* GeometryEngine.Project
* KmlDataset
* KmlDocument
* KmlGeometry
* KmlLayer
* KmlNode.SaveAsASync
* KmlPlacemark
* KmlStyle

## Tags

geometry editor, Keyhole, KML, KMZ, OGC
