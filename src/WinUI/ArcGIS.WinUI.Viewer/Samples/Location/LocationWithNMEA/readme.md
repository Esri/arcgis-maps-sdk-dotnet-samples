# Display device location with NMEA data sources

Parse NMEA sentences and use the results to show device location on the map.

![Image of NMEA location data source](LocationWithNMEA.jpg)

## Use case

NMEA sentences can be retrieved from GPS receivers and parsed into a series of coordinates with additional information. Devices without a built-in GPS receiver can retrieve NMEA sentences by using a separate GPS dongle, commonly connected bluetooth or through a serial port.

The NMEA location data source allows for detailed interrogation of the information coming from the GPS receiver. For example, allowing you to report the number of satellites in view.

## How to use the sample

Tap "Start" to parse the NMEA sentences into a simulated location data source, and initiate the location display. Tap "Recenter" to recenter the location display. Tap "Reset" to reset the location display.

## How it works

1. Load NMEA sentences from a local file.
2. Parse the NMEA sentence strings, and push data into `NmeaLocationDataSource`.
3. Set the `NmeaLocationDataSource` to the location display's data source.
4. Start the location display to begin receiving location and satellite updates.

## Relevant API

* Location
* LocationDisplay
* NmeaLocationDataSource
* NmeaSatelliteInfo

## About the data

A list of NMEA sentences is used to initialize a to simulate a stream of NMEA data. This simulated data source provides NMEA data periodically, and allows the sample to be used on devices without a GPS dongle that produces NMEA data.

The route taken in this sample features a [2-minute driving trip around Redlands, CA](https://arcgis.com/home/item.html?id=d5bad9f4fee9483791e405880fb466da).

## Additional information

Below is a list of protocol strings for commonly used GNSS external accessories. Please refer to the [ArcGIS Field Maps documentation](https://doc.arcgis.com/en/field-maps/ios/help/high-accuracy-data-collection.htm#ESRI_SECTION2_612D328A655644DCAF5CF0210308C821) for model and firmware requirements.

### Supported by this sample

* com.bad-elf.gps
* com.eos-gnss.positioningsource
* com.geneq.sxbluegpssource

### Others

* com.amanenterprises.nmeasource
* com.dualav.xgps150
* com.garmin.pvt
* com.junipersys.geode
* com.leica-geosystems.zeno.gnss
* com.searanllc.serial
* com.trimble.correction, com.trimble.command (1)

(1) Some Trimble models require a proprietary SDK for NMEA output.

## Tags

dongle, GNSS, GPS, history, navigation, NMEA, real-time, RTK, trace
