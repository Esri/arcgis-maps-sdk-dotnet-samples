# Edit and sync features

Synchronize offline edits with a feature service.

![screenshot](EditAndSyncFeatures.jpg)

## Use case

A survey worker who works in an area without an internet connection could take a geodatabase of survey features offline at their office, make edits and add new features to the offline geodatabase in the field, and sync the updates with the online feature service after returning to the office.

## How to use the sample

Pan and zoom to position the red rectangle around the area you want to take offline. Click "Generate" to take the area offline. When complete, the offline area will be shown in the red rectangle. To edit features, click to select a feature, and click again anywhere else within the red rectangle to move the selected feature. To sync the edits with the feature service, click "Synchronize".

## How it works

1. Create a `GeodatabaseSyncTask` from a URL to a feature service.
2. Use `createDefaultGenerateGeodatabaseParametersAsync()` on the geodatabase sync task to create `GenerateGeodatabaseParameters`, passing in an `Envelope` extent as the parameter.
3. Create a `GenerateGeodatabaseJob` from the `GeodatabaseSyncTask` using `generateGeodatabaseAsync(...)`, passing in the parameters and a path to where the geodatabase should be downloaded locally.
4. Start the job and get the result `Geodatabase`.
5. Load the geodatabase and get its feature tables. Create feature layers from the feature tables and add them to the map's operational layers collection.
6. Create `SyncGeodatabaseParameters` and set the sync direction.
7. Create a `SyncGeodatabaseJob` from `GeodatabaseSyncTask` using `.syncGeodatabaseAsync(...)` passing in the parameters and geodatabase as arguments.
8. Start the sync job to synchronize the edits.

## Relevant API

* FeatureLayer
* FeatureTable
* GenerateGeodatabaseJob
* GenerateGeodatabaseParameters
* GeodatabaseSyncTask
* SyncGeodatabaseJob
* SyncGeodatabaseParameters
* SyncLayerOption

## Offline data

This sample downloads the following items from ArcGIS Online automatically:

* [SanFrancisco.tpk](https://www.arcgis.com/home/item.html?id=3f1bbf0ec70b409a975f5c91f363fe7d) - Streets Tile Package (tpk) for the City of San Francisco.

## About the data

The basemap uses an offline tile package of San Francisco. The online feature service has features with wildfire information.

## Tags

feature service, geodatabase, offline, synchronize
