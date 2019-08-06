# Format coordinates

Format coordinates in a variety of common notations.

![screenshot](FormatCoordinates.jpg)

## Use case

The coordinate formatter can format a map location in WGS84 in a number of common coordinate notations. Parsing one of these formats to a location is also supported. Formats include decimal degrees; degrees, minutes, seconds; Universal Transverse Mercator (UTM), and United States National Grid (USNG).

## How to use the sample

Click on the map to see the clicked location's coordinate formatted in 4 different ways. You can also put a coordinate string in any of these formats in the text field. Hit Enter and the coordinate string will be parsed and the map point and coordinate values will be updated.

## How it works

1. Get or create a map point with a spatial reference.
2. Use one of the static "to" methods on `CoordinateFormatter` such as `CoordinateFormatter.ToLatitudeLongitude(point, CoordinateFormatter.LatitudeLongitudeFormat.DecimalDegrees, 4)` to get the formatted string.
3. To go from a formatted string to a `Point`, use one of the "from" static methods like `CoordinateFormatter.FromUtm(coordinateString, map.SpatialReference, CoordinateFormatter.UtmConversionMode.NorthSouthIndicators)`.

## Relevant API

* CoordinateFormatter
* CoordinateFormatter.LatitudeLongitudeFormat
* CoordinateFormatter.UtmConversionMode

## Tags

USNG, UTM, convert, coordinate, decimal degrees, degree minutes seconds, format, latitude, longitude
