# Show location history

Display your location history on the map.

![Image of show location history](ShowLocationHistory.jpg)

## Use case

You can track device location history and display it as lines and points on the map. The history can be used to visualize how the user moved through the world, to retrace their steps, or to create new feature geometry. An unmapped trail, for example, could be added to the map using this technique.

## How to use the sample

Click, 'Start tracking' to start tracking your location, which will appear as points on the map. A line will connect the points for easier visualization. Click 'Stop tracking' to stop updating the location history.

## How it works

1. If necessary, request location permission from the operating system.
2. Create a graphics overlay to show each point and another graphics overlay for displaying the route line.
3. Create a `SystemLocationDataSource` and start it to begin receiving location updates.
4. Subscribe to the `LocationChanged` event to handle location updates.
5. Every time the location updates, store that location, display a point on the map, and re-create the route line.

## Relevant API

* Location.Position
* LocationDataSource
* LocationDataSource.LocationChanged
* LocationDataSource.UpdateLocation
* LocationDisplay.AutoPanMode
* LocationDisplay.DataSource
* LocationDisplay.IsEnabled
* MapView.LocationDisplay
* SystemLocationDataSource

## About the data

The sample uses a dark gray basemap with lime and red graphics. A custom location data source that provides fake/random location data is provided to enable easier testing and allow the sample to be useful on desktop/non-mobile devices.

## Tags

bread crumb, breadcrumb, GPS, history, movement, navigation, real-time, trace, track, trail