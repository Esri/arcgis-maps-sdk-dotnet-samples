# Edit geometries with programmatic reticle tool

Use the Programmatic Reticle Tool to edit and create geometries with programmatic operations to facilitate workflows such as those using buttons rather than tap interactions.

![EditGeometriesWithProgrammaticReticleTool](editgeometrieswithprogrammaticreticletool.jpg)

## Use case

A field worker can use a button driven workflow to mark important features on a map. They can digitize features like sample or observation locations, fences, pipelines, and building footprints using point, multipoint, polyline, and polygon geometries. To create and edite geometries, workers can use a vertex-based reticle tool to specify vertex locations by panning the map to position the reticle over a feature of interest. Using a button driven workflow they can then place new vertices or pick up, move and drop existing vertices.

## How to use the sample

To create a new geometry, select the geometry type you want to create (i.e. points, multipoints, polyline, or polygon) in the settngs view. Press the button to start the geometry editor, pan the map to position the reticle then press the button to place a vertex. To edit an existing geometry, tap the geometry to be edited in the map and perform edits by positioning the reticle over a vertex and pressing the button to pick it up. The vertex can be moved by panning the map and dropped in a new position by pressing the button again.

Vertices can be selected and the viewpoint can be updated to their position by tapping them. 

Vertex creation can be disabled using the switch in the settings view, when this switch is toggled off the feedback vertex and feedback lines under the reticle will no longer be visible. New vertex creation is prevented, existing vertices can be picked up and moved but mid-vertices cannot be selected or picked up and will not grow when hovered.

Use the buttons in the settings view to undo or redo changes made to the geometry and the cancel and done buttons to discard and save changes.

## How it works

1. Create a `GeometryEditor` and set it to the MapView using `MyMapView.GeometryEditor`.
2. Start the `GeometryEditor` using `GeometryEditor.Start(GeometryType)` to create a new geometry or `GeometryEditor.Start(Geometry)` to edit an existing geometry.
    * If using the Geometry Editor to edit an existing geometry, the geometry must be retrieved from the graphics overlay being used to visualize the geometry prior to calling the start method. To do this:
        * Use `MapView.IdentifyGraphicsOverlayAsync(...)` to identify graphics at the location of a tap.
        * Access the `MapView.IdentifyGraphicsOverlayAsync(...)`.
        * Find the desired graphic in the `results.FirstOrDefault()` list.
        * Access the geometry associated with the `Graphic` using `Graphic.Geometry` - this will be used in the `GeometryEditor.Start(Geometry)` method.
3. Create a `ProgrammaticReticleTool` and set the `GeometryEditor.Tool`.
4. Add event handlers to listen to `GeometryEditor.HoveredElementChanged` and `GeometryEditorPickedUpElementChanged`.
    * These events can be used to determine the effect a button press will have and set the button text accordingly.
5. Listen to tap events when the geometry editor is active to select and navigate to tapped vertices and mid-vertices. 
    * To retrieve the tapped element and update the viewpoint:
        * Use `MapView.IdentifyGeometryEditorAsync(...)` to identify geometry editor elements at the location of the tap. 
        * Access the `MapView.IdentifyGeometryEditorAsync(...)`.
        * Find the desired element in the `results.FirstOrDefault()` list.
        * Depending on whether or not the element is a `GeometryEditorVertex` or `GeometryEditorMidVertex` use `GeometryEditor.SelectVertex(...)` or `GeometryEditor.SelectMidVertex(...)` to select it.
        * Update the viewpoint using `MapView.SetViewpoint(...)`.
6. Enable and disable the vertex creation preview using `ProgrammaticReticleTool.VertexCreationPreviewEnabled`.
    * To prevent mid-vertex growth when hovered use `ProgrammaticReticleTool.Style.GrowEffect.ApplyToMidVertices`.
6. Check to see if undo and redo are possible during an editing session using `GeometryEditor.CanUndo` and `GeometryEditor.CanRedo`. If it's possible, use `GeometryEditor.Undo()` and `GeometryEditor.Redo()`.
    * A picked up element can be returned to its previous position using `GeometryEditor.CancelCurrentAction()`, this can be useful to undo a pick up without undoing any change to the geometry.
7. Check whether the currently selected `GeometryEditorElement` can be deleted (`GeometryEditor.SelectedElement.CanDelete`). If the element can be deleted, delete using `GeometryEditor.DeleteSelectedElement()`.
8. Call `GeometryEditor.Stop()` to finish the editing session and store the `Graphic`. The `GeometryEditor` does not automatically handle the visualization of a geometry output from an editing session. This must be done manually by propagating the geometry returned into a `Graphic` added to a `GraphicsOverlay`.
    * To create a new `Graphic` in the `GraphicsOverlay`:
        * Using `Graphic(Geometry)`, create a new Graphic with the geometry returned by the `GeometryEditor.Stop()` method.
        * Append the `Graphic` to the `GraphicsOverlay`(i.e. `GraphicsOverlay.Graphics.Add(Graphic)`).
    * To update the geometry underlying an existing `Graphic` in the `GraphicsOverlay`:
        * Replace the existing `Graphic`'s `Geometry` property with the geometry returned by the `GeometryEditor.Stop()` method.

## Relevant API

* Geometry
* GeometryEditor
* Graphic
* GraphicsOverlay
* MapView
* ProgrammaticReticleTool

## Additional information

The sample demonstrates a number of workflows which can be altered depending on desired app functionality:
* picking up a hovered element combines selection and pick up, this can be separated into two steps to require selection before pick up. 
* tapping a vertex or mid-vertex selects it and updates the viewpoint to its position, this could be changed to not update the viewpoint or also pick up the element.

With the hovered and picked up element changed events and the programmatic APIs on the `ProgrammaticReticleTool` a broad range of editing experiences can be implemented.

## Tags

draw, edit, freehand, geometry editor, programmatic, reticle, sketch, vertex