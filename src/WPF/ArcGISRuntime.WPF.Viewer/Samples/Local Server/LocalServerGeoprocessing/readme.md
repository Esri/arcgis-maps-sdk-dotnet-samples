# Local server geoprocessing

Create contour lines from local raster data using a local geoprocessing package `.gpk` and the contour geoprocessing tool.

![Image of local server geoprocessing](LocalServerGeoprocessing.jpg)

## Use case

For executing offline geoprocessing tasks in your ArcGIS Runtime apps via an offline (local) server.

## How to use the sample

Contour Line Controls (Top Left):

* Interval - Specifies the spacing between contour lines.
* Generate Contours - Adds contour lines to map using interval.
* Clear Results - Removes contour lines from map.

## How it works

1. Create and run a local server with `LocalServer.Instance`.
2. Start the server asynchronously with `server.StartAsync()`.
3. Start a `LocalGeoprocessingService` and run a `GeoprocessingTask`.
    1. Instantiate `LocalGeoprocessingService(Url, ServiceType)` to create a local geoprocessing service.
    2. Call `LocalGeoprocessingService.StartAsync()` to start the service asynchronously.
    3. Instantiate `GeoprocessingTask(LocalGeoprocessingService.Url + "/Contour")` to create a geoprocessing task that uses the contour lines tool.
4. Create an instance of `GeoprocessingParameters`.
    1. Instantiate `GeoprocessingParameters(ExecutionType)` creates geoprocessing parameters.
    2. Create a parameter using `gpParams.Inputs["ContourInterval"] = new GeoprocessingDoublevalue)` using the desired contour value.
5. Create and start a `GeoprocessingJob` using the previous parameters.
    1. Create a geoprocessing job with `GeoprocessingTask.CreateJob(GeoprocessingParameters)`.
    2. Start the job with `GeoprocessingJob.Start()`.
6. Add contour lines as an `ArcGISMapImageLayer` to the map.
    1. Get url from local geoprocessing service using the `service.Url` property.
    2. Get server job id of geoprocessing job using the `GeoprocessingJob.ServerJobId` property.
    3. Replace `GPServer` from url with `MapServer/jobs/jobId`, to get generate contour lines data.
    4. Create a map image layer from that new url and add that layer to the map.

## Relevant API

* GeoprocessingDouble
* GeoprocessingJob
* GeoprocessingParameter
* GeoprocessingParameters
* GeoprocessingTask
* LocalGeoprocessingService
* LocalGeoprocessingService.ServiceType
* LocalServer
* LocalServerStatus

## Offline data

This sample downloads the following items from ArcGIS Online automatically:

* [Contour.gpk](https://www.arcgis.com/home/item.html?id=da9e565a52ca41c1937cff1a01017068) - A Geoprocessing Package for generating contour lines.

## Additional information

Local Server can be downloaded for Windows and Linux platforms from the [developers website](https://developers.arcgis.com/downloads/apis-and-sdks?product=local-server#arcgis-runtime-local-server). Local Server is not supported on macOS.

## Tags

geoprocessing, local, offline, parameters, processing, service