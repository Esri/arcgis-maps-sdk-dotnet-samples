# Update geometries (feature service)

Update a feature's location in an online feature service.

![Image of update geometries feature service](UpdateGeometries.jpg)

## Use case

Sometimes users may want to edit features in an online feature service by moving them.

## How to use the sample

Tap a feature to select it. Tap again to set the updated location for that feature. An alert will be shown confirming success or failure.

## How it works

1. Create a `ServiceGeodatabase` object from a URL.
2. Get the `ServiceFeatureTable` from the `ServiceGeodatbase` object. 
3. Create a `FeatureLayer` object from the `ServiceFeatureTable`.
4. Select a feature from the feature layer, using `FeatureLayer.SelectFeatures`.
5. Load the selected feature.
6. Change the selected feature's location using `Feature.Geometry = geometry`.
7. After the change, update the table on the server using `ApplyEditsAsync`.

## Relevant API

* Feature
* FeatureLayer
* ServiceFeatureTable
* ServiceGeodatabase

## Additional Information

When editing feature tables that are subject to database behavior (operations on one table affecting another table), it's now recommended to call these methods (apply edits & undo edits) on the `ServiceGeodatabase` object rather than on the `ServiceFeatureTable` object. Using the `ServiceGeodatabase` object to call these methods will prevent possible data inconsistencies and ensure transactional integrity so that all changes can be commited or rolled back. 

## Tags

editing, feature layer, feature table, moving, service, updating