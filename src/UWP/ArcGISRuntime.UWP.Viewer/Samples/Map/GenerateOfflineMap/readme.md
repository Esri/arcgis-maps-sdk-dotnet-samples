# Generate Offline Map

Demonstrates how to take a web map offline.

![](GenerateOfflineMap.jpg)

## How to use the sample

When the app starts, a web map is loaded from ArcGIS Online. The red border shows the extent of the data that will be downloaded for use offline. Click the `Take map offline` button to start the offline map job (you will be prompted for your ArcGIS Online login). The progress bar will show the job's progress. When complete, the offline map will replace the online map in the map view.

## How it works

To take a web map offline:
1. Create a `Map` with a portal item for an online map (web map).
2. Create `GenerateOfflineMapParameters` that specifies the area of interest, min/max scale, and so on.
3. Create an `OfflineMapTask` that uses the map.
4. Create the `GenerateOfflineMapJob` with `OfflineMapTask.GenerateOfflineMap(parameters, outputPath)` and execute it with `Job.Start()`.
5. When the job is done, get the result (`GenerateOfflineMapResult`) using `Job.GetResultAsync()`.
6. Get the offline map with `GenerateOfflineMapResult.OfflineMap`.


## Relevant API

- GenerateOfflineMapJob
- GenerateOfflineMapParameters
- GenerateOfflineMapResult
- OfflineMapTask