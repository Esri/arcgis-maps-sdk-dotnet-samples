# Delete features (feature service)

Delete features from an online feature service.

![Image of delete features feature service](DeleteFeatures.jpg)

## Use case

Sometimes users may want to delete features from an online feature service.

## How to use the sample

To delete a feature, tap it, then tap 'Delete incident'.

## How it works

1. Create a `ServiceFeatureTable` object from a URL.
2. Create a `FeatureLayer` object from the service feature table.
3. Select features from the feature layer via `FeatureLayer.SelectFeatures()`.
4. Remove the selected features from the service feature table using `ServiceFeatureTable.DeleteFeatureAsync()`.
5. Update the table on the server using `ServiceFeatureTable.ApplyEditsAsync()`.

## Relevant API

* Feature
* FeatureLayer
* ServiceFeatureTable

## Tags

deletion, feature, online, Service, table