# Read shapefile metadata

Read a shapefile and display its metadata.

![screenshot](ReadShapefileMetadata.jpg)

## Use case

You can display information about the shapefile your user is viewing, like tags, credits, and summary.

## How to use the sample

Click 'See metadata' to show the shapefile's metadata.

## How it works

1. Call `ShapefileFeatureTable.OpenAsync("path_to_shapefile")` to create the `ShapefileFeatureTable`.
2. Get the `ShapefileInfo` from the feature table's `Info` property.
3. Get the image from `fileInfo.Thumbnail` and display it.
4. Display the `Summary`, `Credits`, and `Tags` properties from the shapefile info.

## Relevant API

* ShapefileFeatureTable
* ShapefileFeatureTable.Info
* ShapefileFeatureTable.OpenAsync
* ShapefileInfo
* ShapefileInfo.Credits
* ShapefileInfo.Summary
* ShapefileInfo.Tags
* ShapefileInfo.Thumbnail

## Offline data

This sample downloads the following items from ArcGIS Online automatically:

* [Aurora_CO_shp.zip](https://www.arcgis.com/home/item.html?id=d98b3e5293834c5f852f13c569930caa) - Shapefiles that cover Aurora Colorado: Public art (points), Bike trails (lines), and Subdivisions (polygons).

## About the data

This sample uses a shapefile showing trail bike paths in Aurora, CO. The shapefile is available as an item on [ArcGIS Online](https://www.arcgis.com/home/item.html?id=d98b3e5293834c5f852f13c569930caa).

## Tags

credits, description, metadata, package, shape file, shapefile, summary, symbology, tags, visualization
