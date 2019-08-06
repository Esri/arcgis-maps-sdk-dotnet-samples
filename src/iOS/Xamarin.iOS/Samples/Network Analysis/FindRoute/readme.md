# Find route

Display directions for a route between two points.

![screenshot](FindRoute.jpg)

## Use case

Find routes with driving directions between any number of locations. You might use the ArcGIS platform to create a custom network for routing on a private roads.

## How to use the sample

For simplicity, the sample comes loaded with a start and end stop. Click 'Solve route' to display a route between these stops. Once the route is generated, click 'Directions' to see turn-by-turn directions.

## How it works

1. Create a `RouteTask` using a URL to an online route service.
2. Generate default `RouteParameters` using `routeTask.CreateDefaultParametersAsync()`.
3. Set `returnStops` and `returnDirections` on the parameters to true.
4. Add `Stop`s to the parameters `stops` collection for each destination.
5. Solve the route using `routeTask.SolveAsync(routeParameters)` to get a `RouteResult`.
6. Iterate through the result's `Route`s. To display the route, create a graphic using the geometry from `route.RouteGeometry`. To display directions, use `routeDirectionManeuvers`, and for each `DirectionManeuver`, display `DirectionManeuver.DirectionText`.

## Relevant API

* DirectionManeuver
* Route
* RouteParameters
* RouteResult
* RouteTask
* Stop

## Tags

directions, driving, navigation, network, network analysis, route, routing, shortest path, turn-by-turn
