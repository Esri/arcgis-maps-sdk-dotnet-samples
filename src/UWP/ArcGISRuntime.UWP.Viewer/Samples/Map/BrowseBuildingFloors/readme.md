# Browse building floors

Display and browse through building floors from a floor-aware web map.

![BrowseBuildingFloorsApp](browse-building-floors.png)

## Use case

Having map data to aid indoor navigation in buildings with multiple floors such as airports, museums, or offices can be incredibly useful. For example, you may wish to browse through all available floor maps for an office in order to find the location of an upcoming meeting in advance.

## How to use the sample

Use the spinner to browse different floor levels in the facility. Only the selected floor will be displayed.

## How it works

1. Create a `PortalItem` using the identifier of a floor-aware web map.
2. Create a map using the portal item.
3. Create a map view and assign the map to it.
4. Wait for the map to load and retrieve the map's `FloorManager` property.
5. Wait for the floor manager to load and retrieve the floor-aware data.6 
6. Set all floors to not visible.
7. Set only the selected `FloorLevel` to visible using the `isVisible` property of the floor level.
* **Note:** Manually set the default floor level to the first floor.

## Relevant API

* FloorManager
* FloorLevel
* Map
* PortalItem

## About the data

This sample uses a [floor-aware web map](https://www.arcgis.com/home/item.html?id=f133a698536f44c8884ad81f80b6cfc7) that displays the floors of Building L on the Esri Redlands campus.

## Additional information

The `FloorManager` API also supports browsing different sites and facilities in addition to building floors.

## Tags

building, facility, floor, floor-aware, floors, ground floor, indoor, level, site, story