# Sketch on map

Use the Sketch Editor to edit or sketch a new point, line, or polygon geometry on to a map.

![Image of sketch on map](SketchOnMap.jpg)

## Use case

A field worker could annotate features of interest on a map (via the GUI) such as location of dwellings (marked as points), geological features (polylines), or areas of glaciation (polygons).

## How to use the sample

Choose which geometry type to sketch from one of the available buttons. Choose from points, multipoints, polylines, polygons, freehand polylines, freehand polygons, circles, ellipses, triangles, arrows and rectangles.

Use the control panel to cancel the sketch, undo or redo changes made to the sketch and to save the sketch to the graphics overlay. There is also the option to select a saved graphic and edit its geometry using the Sketch Editor. The graphics overlay can be cleared using the clear all button.

## How it works

1. Use `SketchEditor.StartAsync()` to start sketching. If editing an existing graphic's geometry, use `SketchEditor.StartAsync(graphic.Geometry)`.
2. Use the `UndoCommand` and `RedoCommand` to undo and redo edits in the sketch.
3. Use a `CompleteCommand` to finish the sketch and get the `Geometry` result. Use the `CancelCommand` to cancel the sketch.
4. Create a `Graphic` for the geometry and add it to the `GraphicsOverlay` in the map view.

## Relevant API

* Geometry
* Graphic
* GraphicsOverlay
* MapView
* SketchCreationMode
* SketchEditor

## Tags

draw, edit
