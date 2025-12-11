# Filter building scene layer

Explore details of a building scene by using filters and sublayer visibility.

![Image of a building scene layer](FilterBuildingSceneLayer.jpg)

## Use case

Buildings and their component parts (in this example, structural, electrical, or architectural) can be difficult to explain and visualize. An architectural firm might share a 3D building model visualization with clients and contractors to let them explore these components by floor and component type.

## How to use the sample

In the filter controls, select floor and category options to filter what parts of the Building Scene Layer are displayed in the scene. Click on any of the building features to identify them.

## How it works

1. Create a `Scene` with the URL to a Building Scene Layer service.
2. Create a `LocalSceneView` and add the scene.
3. Retrieve the `BuildingSceneLayer` from the scene's operational layers.
4. The Scene Settings panel displays filtering options.
5. Select a floor from the "Floor" dropdown to view the internal details of each floor or "All" to view the entire model.
6. Expand the categories to show or hide individual items in the building model. The entire category may be shown or hidden as well.
7. Click on any of the building features to view the attributes of the feature.

## Relevant API

* BuildingComponentSublayer
* BuildingFilter
* BuildingFilterBlock
* BuildingSceneLayer
* LocalSceneView
* Scene

## About the data

This sample uses the [Esri Building E Local Scene](https://www.arcgis.com/home/item.html?id=b7c387d599a84a50aafaece5ca139d44) web scene, which contains a Building Scene Layer representing Building E on the Esri Campus in Redlands, CA. The Revit BIM model was brought into ArcGIS using the BIM capabilities in ArcGIS Pro and published to the web as a Building Scene Layer.

## Additional information

Buildings in a Building Scene Layer can be very complex models composed of sublayers containing internal and external features of the structure. Sublayers may include structural components like columns, architectural components like floors and windows, and electrical components.

Applying filters to the Building Scene Layer can highlight features of interest in the model. Filters are made up of filter blocks, which contain several properties that allow control over the filter's function. Setting the filter mode to X-Ray, for instance, will render features with a semi-transparent white color so other interior features can be seen. In addition, toggling the visibility of sublayers can show or hide all the features of a sublayer.

## Tags

3D, building scene layer, layers
