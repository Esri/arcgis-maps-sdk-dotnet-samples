# Navigate route

Use a routing service to navigate between points.

![Navigate route sample](NavigateRoute.jpg)

## Use case

Navigation is often used by field workers while traveling between points to get live directions based on their location.

## How to use the sample

Click 'Navigate' to simulate travelling and to receive directions from a preset starting point to a preset destination. Check 'Voice Directions' to activate announcing maneuvers. Click 'Reset' to start the simulation from the beginning.

## How it works

1. Create a `RouteTask` using a URL to an online route service.
2. Generate default `RouteParameters` using `RouteTask.CreateDefaultParametersAsync()`.
3. Set `ReturnStops` and `ReturnDirections` on the parameters to true.
4. Add `Stop`s to the parameters for each destination using `SetStops(stops)`.
5. Solve the route using `RouteTask.SolveRouteAsync(routeParameters)` to get a `RouteResult`.
6. Create a `RouteTracker` using the route result, and the index of the desired route to take.
7. Use `TrackLocationAsync(LocationDataSource.Location)` to track the location of the device and update the route tracking status.
8. Add a listener to capture `TrackingStatusChangedEvent`s, and then get the `TrackingStatus` and use it to display updated route information. Tracking status includes a variety of information on the route progress, such as the remaining distance, remaining geometry or traversed geometry (represented by a `Polyline`), or the remaining time (`TimeSpan`), amongst others.
9. Add a `NewVoiceGuidanceListener` to get the `VoiceGuidance` whenever new instructions are available. From the voice guidance, get the `string` representing the directions and use a text-to-speech engine to output the maneuver directions.
10. You can also query the tracking status for the current `DirectionManeuver` index, retrieve that maneuver from the `Route` and get it's direction text to display in the GUI.
11. To establish whether the destination has been reached, get the `DestinationStatus` from the tracking status. If the destination status is `Reached`, we have arrived at the destination and can stop routing. If there are several destinations in your route, and the remaining destination count is greater than 1, switch the route tracker to the next destination.

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

## About the data

The route taken in this sample goes from the San Diego Convention Center, site of the annual Esri User Conference, to the Fleet Science Center, San Diego.

## Tags

directions, maneuver, navigation, route, turn-by-turn, voice