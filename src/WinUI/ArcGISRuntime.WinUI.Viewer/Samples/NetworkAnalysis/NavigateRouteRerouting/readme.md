# Navigate route with rerouting

Navigate between two points and dynamically recalculate an alternate route when the original route is unavailable.

![Image of navigate route with rerouting](NavigateRouteRerouting.jpg)

## Use case

While traveling between destinations, field workers use navigation to get live directions based on their locations. In cases where a field worker makes a wrong turn, or if the route suggested is blocked due to a road closure, it is necessary to calculate an alternate route to the original destination.

## How to use the sample

Click 'Navigate' to simulate traveling and to receive directions from a preset starting point to a preset destination. Observe how the route is recalculated when the simulation does not follow the suggested route. Click 'Recenter' to refocus on the location display.

## How it works

1. Create a `RouteTask` using a URL to an online route service.
2. Generate default `RouteParameters` using `RouteTask.CreateDefaultParametersAsync()`.
3. Set `ReturnStops` and `ReturnDirections` on the parameters to true.
4. Add `Stop`s to the parameters `stops` collection for each destination.
5. Solve the route using `RouteTask.SolveRouteAsync(routeParameters)` to get a `RouteResult`.
6. Create a `RouteTracker` using the route result, and the index of the desired route to take.
7. Enable rerouting in the route tracker with `.EnableReroutingAsync(RouteTask, RouteParameters, ReroutingStrategy, false)`. The Boolean specifies `visitFirstStopOnStart` and is false by default. Use `ReroutingStrategy.ToNextWaypoint` to specify that in the case of a reroute the new route goes from present location to next waypoint or stop.
8. Use `.TrackLocationAsync(LocationDataSource.Location)` to track the location of the device and update the route tracking status.
9. Add a listener to capture `TrackingStatusChangedEvent`, and then get the `TrackingStatus` and use it to display updated route information. Tracking status includes a variety of information on the route progress, such as the remaining distance, remaining geometry or traversed geometry (represented by a `Polyline`), or the remaining time (`Double`), amongst others.
10. Add a `NewVoiceGuidanceListener` to get the `VoiceGuidance` whenever new instructions are available. From the voice guidance, get the `string` representing the directions and use a text-to-speech engine to output the maneuver directions.
11. You can also query the tracking status for the current `DirectionManeuver` index, retrieve that maneuver from the `Route` and get its direction text to display in the GUI.
12. To establish whether the destination has been reached, get the `DestinationStatus` from the tracking status. If the destination status is `Reached`, and the `RemainingDestinationCount` is 1, you have arrived at the destination and can stop routing. If there are several destinations in your route, and the remaining destination count is greater than 1, switch the route tracker to the next destination.

## Relevant API

* DestinationStatus
* DirectionManeuver
* Location
* LocationDataSource
* ReroutingStrategy
* Route
* RouteParameters
* RouteTask
* RouteTracker
* Stop
* VoiceGuidance

## Offline data

A geodatabase contains a road network for San Diego. [San Diego Geodatabase](https://arcgisruntime.maps.arcgis.com/home/item.html?id=567e14f3420d40c5a206e5c0284cf8fc)

## About the data

The route taken in this sample goes from the San Diego Convention Center, site of the annual Esri User Conference, to the Fleet Science Center, San Diego.

## Additional information

The route tracker will start a rerouting calculation automatically as necessary when the device's location indicates that it is off-route. The route tracker also validates that the device is "on" the transportation network, if it is not (e.g. in a parking lot) rerouting will not occur until the device location indicates that it is back "on" the transportation network.

## Tags

directions, maneuver, navigation, route, turn-by-turn, voice
