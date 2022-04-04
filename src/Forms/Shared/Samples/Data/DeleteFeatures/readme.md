# Delete features (feature service)

Delete features from an online feature service.

![Image of delete features feature service](DeleteFeatures.jpg)

## Use case

Sometimes users may want to delete features from an online feature service.

## How to use the sample

To delete a feature, tap it, then tap 'Delete incident'.

## How it works

1. Create a `ServiceGeodatabase` object from a URL.
2. Get a `ServiceFeatureTable` object from the `ServiceGeodatabase` object.
3. Create a `FeatureLayer` object from the service feature table.
4. Select features from the feature layer via `FeatureLayer.SelectFeatures()`.
5. Remove the selected features from the service feature table using `ServiceFeatureTable.DeleteFeatureAsync()`.
6. Update the table on the server using `ServiceFeatureTable.ServiceGeodatabase.ApplyEditsAsync()`.

## Relevant API

* Feature
* FeatureLayer
* ServiceFeatureTable
* ServiceGeodatabase

## Additional Information

When editing feature tables that are subject to database behavior (operations on one table affecting another table), it's now recommended to call these methods (apply edits & undo edits) on the `ServiceGeodatabase` object rather than on the `ServiceFeatureTable` object. Using the `ServiceGeodatabase` object to call these methods will prevent possible data inconsistencies and ensure transactional integrity so that all changes can be commited or rolled back. 

## Tags

deletion, feature, online, Service, table