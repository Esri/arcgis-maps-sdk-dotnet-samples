# Load WFS with XML query

Load a WFS feature table using an XML query.

## Use case

Runtime `QueryParameters` objects can't represent all possible queries that can be made against a WFS feature service. You can provide queries as raw XML strings, allowing you to access query functionality that can't be exposed through Runtime `QueryParameters`.

## How it works

1. Create a `WfsFeatureTable` and a `FeatureLayer` to visualize the table.
2. Set the feature request mode to `ManualCache`. 
3. Call `PopulateFromServiceWithXmlAsync` to populate the table with features.

## Relevant API

* WfsFeatureTable
* FeatureLayer
* WfsFeatureTable.AxisOrder
* WfsFeatureTable.PopulateFromServiceWithXmlAsync

## Tags

OGC, WFS, feature, web, service, XML, query