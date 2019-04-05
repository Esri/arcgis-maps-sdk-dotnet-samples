# Download a preplanned map area

Take a map offline using an available preplanned map area.

![](DownloadPreplannedMap.jpg)

## Use case

When a map is taken offline, a package containing basemap tiles, feature data, and other resources is created. In the preplanned workflow, the author of the web map has set up the offline packages ahead of time, which enables faster, more resource-efficient downloads compared to an on-demand workflow. Because the resources for the area are packaged once and can be downloaded many times by different users, this approach is more scalable for large organizations. To see the difference for yourself, compare this sample to the Generate an offline map sample.

## How to use the sample

Select a map area to take offline, then use the button to take it offline. If the area is already offline, the offline copy will be opened. Click 'Delete offline areas' to remove any downloaded map areas.

## How it works

1. Open the map from a Portal item and display it.
2. Create an `OfflineMapTask` from the Portal item.
3. Call `_offlineTask.GetPreplannedMapAreasAsync()` to find the preplanned areas, then load each one by calling `_area.LoadAsync()`.
4. Display the areas in the UI.
5. When the user selects a map area, start the download.
    1. Create a `DownloadPreplannedOfflineMapParameters` using `OfflineMapTask.CreateDefaultDownloadPreplannedOfflineMapParametersAsync`.
    2. Create a `DownloadPreplannedOfflineMapJob` using `OfflineMapTask.DownloadPreplannedOfflineMap`, passing in the parameters.
    3. Wait for the job to complete with `job.GetResultAsync()`.
    4. Display any errors to the user.
    5. Show the offline map in the `MapView`.

## Relevant API

* DownloadPreplannedOfflineMapJob
* DownloadPreplannedOfflineMapParameters
* DownloadPreplannedOfflineMapResult
* DownloadPreplannedOfflineMapResult.HasErrors
* DownloadPreplannedOfflineMapResult.LayerErrors
* DownloadPreplannedOfflineMapResult.TableErrors
* OfflineMapTask
* OfflineMapTask.CreateDefaultDownloadPreplannedOfflineMapParametersAsync
* OfflineMapTask.DownloadPreplannedOfflineMap
* OfflineMapTask.GetPreplannedMapAreasAsync
* PreplannedMapArea

## About the data

The [Naperville stormwater network map](https://arcgisruntime.maps.arcgis.com/home/item.html?id=acc027394bc84c2fb04d1ed317aac674) is based on ArcGIS Solutions for Stormwater and provides a realistic depiction of a theoretical stormwater network.

## Additional information

See [Take a map offline - preplanned](https://developers.arcgis.com/net/latest/wpf/guide/take-map-offline-preplanned.htm) to learn about preplanned workflows, including how to define preplanned areas in ArcGIS Online.

## Tags

Offline, preplanned, pre-planned, map area
