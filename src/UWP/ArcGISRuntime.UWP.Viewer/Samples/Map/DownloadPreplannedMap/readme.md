# Download preplanned map area

Take a map offline using a preplanned map area.

![Image of download preplanned map area](DownloadPreplannedMap.jpg)

## Use case

Generating offline maps on demand for a specific area can be time consuming for users and a processing load on the server. If areas of interest are known ahead of time, a web map author can pre-create packages for these areas. This way, the generation only needs to happen once, making the workflow more efficient for users and servers.

An archaeology team could define preplanned map areas for dig sites which can be taken offline for field use.

## How to use the sample

Downloading tiles for offline use requires authentication with the web map's server. An [ArcGIS Online](www.arcgis.com) account is required to use this sample.

Select a map area from the Preplanned Map Areas list. Click the button to download the selected area. The download progress will be shown in the Downloads list. When a download is complete, select it to display the offline map in the map view.

## How it works

1. Open the online `Map` from a `PortalItem` and display it.
2. Create an `OfflineMapTask` using the portal item.
3. Get the `PreplannedMapArea`s from the task, and then load them.
4. To download a selected map area, create the default `DownloadPreplannedOfflineMapParameters` from the task using the selected preplanned map area.
5. Set the update mode of the preplanned map area.
6. Use the parameters and a download path to create a `DownloadPreplannedOfflineMapJob` from the task.
7. Start the job. Once it has completed, get the  `DownloadPreplannedOfflineMapResult`.
8. Get the `Map` from the result and display it in the `MapView`.

## Relevant API

* DownloadPreplannedOfflineMapJob
* DownloadPreplannedOfflineMapParameters
* DownloadPreplannedOfflineMapResult
* OfflineMapTask
* PreplannedMapArea

## About the data

The [Naperville stormwater network map](https://arcgisruntime.maps.arcgis.com/home/item.html?id=acc027394bc84c2fb04d1ed317aac674) is based on ArcGIS Solutions for Stormwater and provides a realistic depiction of a theoretical stormwater network.

## Additional information

`PreplannedUpdateMode` can be used to set the way the preplanned map area receives updates in several ways:

* `NoUpdates` - No updates will be performed.
* `SyncWithFeatureServices` - Changes, including local edits, will be synced directly with the underlying feature services.
* `DownloadScheduledUpdates` - Scheduled, read-only updates will be downloaded from the online map area and applied to the local mobile geodatabases.

See [Take a map offline - preplanned](https://developers.arcgis.com/net/latest/uwp/guide/take-map-offline-preplanned.htm) to learn about preplanned workflows, including how to define preplanned areas in ArcGIS Online. Alternatively, visit [Take a map offline - on demand](https://developers.arcgis.com/net/latest/uwp/guide/take-map-offline-on-demand.htm) or refer to the sample 'Generate Offline Map' to learn about the on-demand workflow and see how the workflows differ.

## Tags

map area, offline, pre-planned, preplanned