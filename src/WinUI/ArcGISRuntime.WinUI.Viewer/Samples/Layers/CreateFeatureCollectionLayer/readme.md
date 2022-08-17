# Feature collection layer

Create a Feature Collection Layer from a Feature Collection Table, and add it to a map.

![screenshot](CreateFeatureCollectionLayer.jpg)

## Use case

A Feature Collection allows easily importing external data (such as CSV files), as well as creating custom schema for data that is in non-standardized format. This data can then be used to populate a Feature Collection Table, and displayed in a Feature Collection Layer using the attributes and geometries provided in the external data source. For example, an electricity supplier could use this functionality to visualize existing location data of coverage areas (polygons), power stations (points), transmission lines (polylines), and others.

## How to use the sample

When launched, this sample displays a `FeatureCollectionLayer` with a `Point`, `Polyline` and `Polygon` geometry.

## How it works

1. Create a `FeatureCollectionLayer` using a new feature collection, `FeatureCollectionLayer(featureCollection)`
2. Add the feature collection layer to the map, `Map.OperationalLayers.Add(featureCollectionLayer)`.
3. Create a `FeatureCollectionTable` for the `GeometryType`s `Point`, `Polyline`, and `Polygon`, `FeatureCollectionTable(fields, geometryType, spatialReference)`
    * Additionally, pass in a list of `Field` objects to represent the table's schema. In this case a field of type String named `name` is added.
4. Assign a `SimpleRenderer` to each table to render any `Feature`s from that table using the `Symbol` that was set.
5. Add the feature collection table to the feature collection, `FeatureCollection.Tables.Add(featureCollectionTable)`.
6. Use the `createFeature` method to create a feature from the feature collection table, passing an attribute and geometry for that feature, `FeatureCollectionTable.CreateFeature(attributes, geometry)`.
7. Add new features to the table, `FeatureCollectionTable.AddFeatureAsync(feature)`.

## Relevant API

* Feature
* FeatureCollection
* FeatureCollectionLayer
* FeatureCollectionTable
* Field
* SimpleRenderer

## Tags

feature collection, layers
