# Display device location with autopan modes

Display your current position on the map, as well as switch between different types of auto pan modes.

![Image of display device location with autopan modes](DisplayDeviceLocation.jpg)

## Use case

When using a map within a GIS, it may be helpful for a user to know their own location within a map, whether that's to aid the user's navigation or to provide an easy means of identifying/collecting geospatial information at their location.

## How to use the sample

Select an autopan mode, then use the button to start and stop location display.

## How it works

1. Create a `MapView`.
2. Set the `LocationDisplay.AutoPanMode` that corresponds with the selected element of the combo box.
3. Set the `LocationDisplay.IsEnabled` bool from the `MapView` to true.
4. Set the `LocationDisplay.IsEnabled` bool from the `MapView` to false when the stop button is pressed.

## Relevant API

* LocationDisplay
* LocationDisplay.AutoPanMode
* Map
* MapView

## Additional information

Location permissions are required for this sample.

## Tags

compass, GPS, location, map, mobile, navigation
