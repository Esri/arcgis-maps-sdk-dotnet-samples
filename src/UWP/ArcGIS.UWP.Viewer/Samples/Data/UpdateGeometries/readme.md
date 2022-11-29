﻿# Update geometries (feature service)

Update a feature's location in an online feature service.

![Image of update geometries feature service](UpdateGeometries.jpg)

## Use case

Sometimes users may want to edit features in an online feature service by moving them.

## How to use the sample

Tap a feature to select it. Tap again to set the updated location for that feature. An alert will be shown confirming success or failure.

## How it works

1. Create a `ServiceFeatureTable` object from a URL.
2. Create a `FeatureLayer` object from the `ServiceFeatureTable`.
3. Select a feature from the feature layer, using `FeatureLayer.SelectFeatures`.
4. Load the selected feature.
5. Change the selected feature's location using `Feature.Geometry = geometry`.
6. After the change, update the table on the server using `ApplyEditsAsync`.

## Relevant API

* Feature
* FeatureLayer
* ServiceFeatureTable

## Tags

editing, feature layer, feature table, moving, service, updating