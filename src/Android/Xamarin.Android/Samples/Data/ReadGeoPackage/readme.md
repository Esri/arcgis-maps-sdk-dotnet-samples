# Read a GeoPackage

Add rasters and feature tables from GeoPackages to a map.

![screenshot](ReadGeoPackage.jpg)

## Use case

GeoPackage is an open standard for sharing raster and vector data. You may want to use GeoPackage to support file-based sharing of geographic data.

## How to use the sample

When the sample loads, the feature tables and rasters from the GeoPackage will be shown on the map.

## How it works

1. Open the GeoPackage using `GeoPackage.OpenAsync(path)`.
2. Iterate through available rasters exposed by `geopackage.GeoPackageRasters`.
    * For each raster, create a raster layer using `new Rasterlayer(geopackageRaster)`, then add it to the map.
3. Iterate through available feature tables, exposed by `geopackage.GeoPackageFeatureTables`.
    * For each feature table, create a feature layer using `new FeatureLayer(geopackageFeatureTable)`, then add it to the map.

## Relevant API

* GeoPackage
* GeoPackageRaster
* GeoPackage.GeoPackageRasters
* GeoPackageFeatureTable
* GeoPackage.GeoPackageFeatureTables

## Offline data

Find this item on [ArcGIS Online](https://arcgisruntime.maps.arcgis.com/home/item.html?id=68ec42517cdd439e81b036210483e8e7).

## About the data

This sample features a GeoPackage with datasets that cover Aurora Colorado: Public art (points), Bike trails (lines), Subdivisions (polygons), Airport noise (raster), and Buildings (raster).

## Tags

GeoPackage, Maps, Rasters, Layers, Tables, OGC, package, container
