# Apply renderers to scene layer

Change the appearance of a 3D object scene layer with different renderers.

## Use case

A scene layer of 3D buildings hosted on ArcGIS Online comes with a preset renderer that defines how the buildings are
displayed in the application. However, the fill color may sometimes blend into the basemap, making the buildings
difficult to distinguish. To enhance visualization, you can apply a custom renderer with a more contrasting fill color,
helping the 3D buildings stand out more clearly. Additionally, you can use a unique value renderer to represent
different building uses, or a class breaks renderer to visualize building ages - valuable insights for urban planning
and analysis.

## How to use the sample

Wait for the scene layer to load. The original scene layer displays 3D textured buildings. Tap on the "Select Renderer"
dropdown menu and choose a different renderer to change how the buildings are visualized. Each renderer applies
different symbology to the scene layer. Setting the renderer to null will remove any applied symbology, reverting the
buildings to their original textured appearance.

## Implementation Overview

1. Create an `ArcGISSceneLayer` using a service URL.
2. Add the scene layer to an `ArcGISScene` and display it via a `SceneView`.
3. Configure multiple renderers:
   - A `SimpleRenderer` with a `MultilayerMeshSymbol` that includes a fill color and styled edges.
   - A `UniqueValueRenderer` that applies different `MultilayerMeshSymbol` settings based on the building usage attribute.
   - A `ClassBreaksRenderer` that changes the symbol based on the building's year of completion.
4. Update the scene layer's `Renderer` property with the selected renderer.
5. Setting the `Renderer` property to `null` restores the original textured appearance.

## Relevant API

* ArcGISSceneLayer  
* ArcGISScene  
* SceneView  
* SimpleRenderer  
* UniqueValueRenderer  
* ClassBreaksRenderer  
* MultilayerMeshSymbol  
* SymbolLayerEdges3D  

## About the Data

This sample uses the [Helsinki 3D Buildings Scene](https://www.arcgis.com/home/item.html?id=fdfa7e3168e74bf5b846fc701180930b) from ArcGIS Online, showcasing 3D textured buildings in Helsinki, Finland.

## Relevant API

* ArcGISSceneLayer
* ClassBreaksRenderer
* MaterialFillSymbolLayer
* MultilayerMeshSymbol
* SceneView
* SimpleRenderer
* SymbolLayerEdges3D
* UniqueValueRenderer

## About the data

This sample displays a [Helsinki 3D buildings scene](https://www.arcgis.com/home/item.html?id=fdfa7e3168e74bf5b846fc701180930b) hosted on ArcGIS Online, showing 3D textured buildings in Helsinki, Finland.

## Tags

3D, buildings, renderer, scene layer, symbology, visualization