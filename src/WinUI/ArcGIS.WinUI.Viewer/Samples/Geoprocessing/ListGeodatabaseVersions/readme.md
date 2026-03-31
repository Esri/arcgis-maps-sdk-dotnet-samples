# List geodatabase versions

Connect to a service and list versions of the geodatabase.

![Image of list geodatabase versions](ListGeodatabaseVersions.jpg)

## Use case

As part of a multi-user editing scenario, you can check with the server to see how many versions of the geodatabase are outstanding before syncing.

## How to use the sample

When the sample loads, a list of geodatabase versions and their properties will be displayed.

## How it works

1. Create a geoprocessing task referring to a **GPServer** with a **ListVersions** task.
2. Use the task to create default parameters.
3. Use the created parameters to create a job.
4. Run the job to get a `GeoprocessingResult`.
5. Get a list of geoprocessing features from the **Versions** output parameter of the results.
6. Format the geodatabase versions for display.

## Relevant API

* GeoprocessingFeatures
* GeoprocessingFeatures.Features
* GeoprocessingJob
* GeoprocessingJob.GetResultAsync
* GeoprocessingParameters
* GeoprocessingResult
* GeoprocessingResult.Outputs
* GeoprocessingTask
* GeoprocessingTask.CreateDefaultParametersAsync
* GeoprocessingTask.CreateJob

## About the data

The sample uses a [sample geoprocessing service](https://sampleserver6.arcgisonline.com/arcgis/rest/services/GDBVersions/GPServer/ListVersions) hosted on ArcGIS Online.

## Additional information

ArcGIS Server does not include a geoprocessing service for listing geodatabase versions. Instead you must configure and publish one yourself using ArcGIS Pro. To learn more about geoprocessing services see [Web tools and geoprocessing services](https://enterprise.arcgis.com/en/server/latest/publish-services/linux/what-is-a-web-tool.htm) in the *ArcGIS Server* documentation.

## Tags

conflict resolution, data management, database, multi-user, sync, version
