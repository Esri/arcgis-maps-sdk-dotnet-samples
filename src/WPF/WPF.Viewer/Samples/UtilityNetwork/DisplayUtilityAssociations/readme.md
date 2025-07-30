# Display utility associations

Create graphics for utility associations in a utility network.

![Image of display utility associations](DisplayUtilityAssociations.jpg)

## Use case

Visualizing utility associations can help you to better understand trace results and the topology of your utility network. For example, connectivity associations allow you to model connectivity between two junctions that don't have geometric coincidence (are not in the same location); structural attachment associations allow you to model equipment that may be attached to structures; and containment associations allow you to model features contained within other features.

## How to use the sample

Pan and zoom around the map. Observe graphics that show utility associations between junctions.

## How it works

1. Create and load a `Map` with a web map item URL that contains a `UtilityNetwork`.
2. Get and load the first `UtilityNetwork` from the web map.
3. Create a `GraphicsOverlay` for the utility associations.
4. Add an event handler for the `ViewpointChanged` event of the `MapView`.
5. When the sample starts and every time the viewpoint changes, do the following steps.
6. Get the geometry of the mapview's extent using `GetCurrentViewpoint(ViewpointType.BoundingGeometry)?.TargetGeometry?.Extent`.
7. Get the associations that are within the current extent using `GetAssociationsAsync(extent)`.
8. Get the `UtilityAssociationType` for each association.
9. Create a `Graphic` using the `Geometry` property of the association and a preferred symbol.
10. Add the graphic to the graphics overlay.

## Relevant API

* GraphicsOverlay
* UtilityAssociation
* UtilityAssociationType
* UtilityNetwork

## About the data

The [Naperville Electric Map](https://sampleserver7.arcgisonline.com/portal/home/item.html?id=be0e4637620a453584118107931f718b) web map contains a utility network used to run the subnetwork-based trace in this sample. Authentication is required and handled within the sample code.

## Additional information

Using utility network on ArcGIS Enterprise 10.8 requires an ArcGIS Enterprise member account licensed with the [Utility Network user type extension](https://enterprise.arcgis.com/en/portal/latest/administer/windows/license-user-type-extensions.htm#ESRI_SECTION1_41D78AD9691B42E0A8C227C113C0C0BF). Please refer to the [utility network services documentation](https://enterprise.arcgis.com/en/server/latest/publish-services/windows/utility-network-services.htm).

## Tags

associating, association, attachment, connectivity, containment, relationships
