# Trace utility network

Discover connected features in a utility network using connected, subnetwork, upstream, and downstream traces.

![Image of trace utility network](traceutilitynetwork.jpg)

## Use case

You can use a trace to visualize and validate the network topology of a utility network for quality assurance. Subnetwork traces are used for validating whether subnetworks, such as circuits or zones, are defined or edited appropriately.

## How to use the sample

Tap on one or more features while 'Add starting locations' or 'Add barriers' is selected. When a junction feature is identified, you may be prompted to select a terminal. When an edge feature is identified, the distance from the tapped location to the beginning of the edge feature will be computed. Select the type of trace using the drop down menu. Tap 'Trace' to initiate a trace on the network. Tap 'Reset' to clear the trace parameters and start over.

## How it works

1. Create a `MapView` and subscribe to its `GeoViewTapped` event.
2. Create and load a `Map` with a web map item URL that contains a `UtilityNetwork`.
3. Get and load the first `UtilityNetwork` from the web map.
4. Get the `ServiceGeodatabase` from the utility network and fetch the line `FeatureLayer` from the `ServiceGeodatabase`'s tables.
5. Add a `GraphicsOverlay` with symbology that distinguishes starting locations from barriers.
6. Identify features on the map and add a `Graphic` that represents its purpose (starting location or barrier) at the tapped location.
7. Create a `UtilityElement` for the identified feature.
8. Determine the type of this element using its `UtilityNetworkSource.SourceType` property.
9. If the element is a junction with more than one terminal, display a terminal picker. Then set the junction's `UtilityTerminal` property with the selected terminal.
10. If an edge, set its `FractionAlongEdge` property using `GeometryEngine.FractionAlong`.
11. Add this `UtilityElement` to a collection of starting locations or barriers.
12. Create `UtilityTraceParameters` with the selected trace type along with the collected starting locations and barriers (if applicable).
13. Set the `UtilityTraceParameters.TraceConfiguration` with the tier's `UtilityTier.GetDefaultTraceConfiguration()` result.
14. Run a `UtilityNetwork.TraceAsync` with the specified parameters.
15. For every `FeatureLayer` in the map, select the features returned with `GetFeaturesForElementsAsync` from the elements matching their `UtilityNetworkSource.FeatureTable` with the layer's `FeatureTable`.

## Relevant API

* FractionAlong
* ServiceGeodatabase
* UtilityAssetType
* UtilityDomainNetwork
* UtilityElement
* UtilityElementTraceResult
* UtilityNetwork
* UtilityNetworkDefinition
* UtilityNetworkSource
* UtilityTerminal
* UtilityTier
* UtilityTraceConfiguration
* UtilityTraceParameters
* UtilityTraceResult
* UtilityTraceType
* UtilityTraversability

## About the data

The [Naperville Electric Web Map](https://sampleserver7.arcgisonline.com/portal/home/item.html?id=be0e4637620a453584118107931f718b) contains a utility network used to run the subnetwork-based trace shown in this sample. Authentication is required and handled within the sample code.

## Additional information

Using utility network on ArcGIS Enterprise 10.8 requires an ArcGIS Enterprise member account licensed with the [Utility Network user type extension](https://enterprise.arcgis.com/en/portal/latest/administer/windows/license-user-type-extensions.htm#ESRI_SECTION1_41D78AD9691B42E0A8C227C113C0C0BF). Please refer to the [utility network services documentation](https://enterprise.arcgis.com/en/server/latest/publish-services/windows/utility-network-services.htm).

A `UtilityNetworkTraceTool` component can be used for various utility network related use cases. For information about setting up the toolkit, as well as code for the component, visit the [toolkit repository](https://github.com/Esri/arcgis-maps-sdk-dotnet-toolkit).

## Tags

condition barriers, downstream trace, network analysis, subnetwork trace, toolkit, trace configuration, traversability, upstream trace, utility network, validate consistency
