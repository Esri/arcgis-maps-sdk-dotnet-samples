# Read shapefile metadata

Read a shapefile and display its metadata.

![Image of read shapefile metadata](ReadShapefileMetadata.jpg)

## Use case

You can display information about the shapefile your user is viewing, like tags, credits, and summary.

## How to use the sample

The shapefile's metadata will be displayed when you open the sample.

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

[Aurora Colorado Shapefiles](https://www.arcgis.com/home/item.html?id=d98b3e5293834c5f852f13c569930caa) is available as an item hosted on ArcGIS Online].

## About the data

This sample uses a shapefile showing bike trails in Aurora, CO. The [Aurora Colorado Shapefiles](https://www.arcgis.com/home/item.html?id=d98b3e5293834c5f852f13c569930caa) are available as an item on ArcGIS Online.

## Tags

credits, description, metadata, package, shape file, shapefile, summary, symbology, tags, visualization