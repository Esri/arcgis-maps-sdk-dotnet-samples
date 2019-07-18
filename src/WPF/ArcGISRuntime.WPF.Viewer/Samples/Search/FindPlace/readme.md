# Find place

Find places of interest near a location or within a specific area.

![screenshot](FindPlace.jpg)

## Use case

When getting directions or looking for nearby places, users may only know what the place has ("food"), the type of place ("gym"), or the generic place name ("Starbucks"), rather than the specific address. You can get suggestions and locations for these places of interest (POIs) using a natural language query. Additionally, you can filter the results to a specific area.

## How to use the sample

Choose a type of place in the first field and an area to search within in the second field. Click the Search button to show the results of the query on the map. Click on a result pin to show its name and address.

## How it works

1. Create a `LocatorTask` using a URL to a locator service.
2. Find the location for an address (or city name) to build an envelope to search within:
    * Create `GeocodeParameters`.
    * Add return fields to the parameters' `ResultAttributeNames` collection. Only add a single "\*" option to return all fields.
    * Call `locatorTask.GeocodeAsync(locationQueryString, geocodeParameters)` to get a list of `GeocodeResult`s.
    * Use the `DisplayLocation` from one of the results to build an `Envelope` to search within.
3. Get place of interest (POI) suggestions based on a place name query:
    * Create `SuggestParameters`.
    * Add "POI" to the parameters' `categories` collection.
    * Call `locatorTask.SuggestAsync(placeQueryString, suggestParameters)` to get a list of `SuggestResults`.
    * The `SuggestResult` will have a `label` to display in the search suggestions list.
4. Use one of the suggestions or a user-written query to find the locations of POIs:
    * Create `GeocodeParameters`.
    * Set the parameters' `searchArea` to the envelope.
    * Call `locatorTask.GeocodeAsync(suggestionLabelOrPlaceQueryString, geocodeParameters)` to get a list of `GeocodeResult`s.
    * Display the places of interest using the results' `DisplayLocation`s.

## Relevant API

* GeocodeParameters
* GeocodeResult
* LocatorTask
* SuggestParameters
* SuggestResult

## About the data

This sample uses the world locator service "https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer".

## Tags

POI, businesses, geocode, locations, locator, places of interest, point of interest, search, suggestions
