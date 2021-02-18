# Surface placement

Position graphics relative to a surface using different surface placement modes.

![Image of surface placement](SurfacePlacements.jpg)

## Use case

Depending on the use case, data might be displayed at an absolute height (e.g. flight data recorded with altitude information), at a relative height to the terrain (e.g. transmission lines positioned relative to the ground), at a relative height to objects in the scene (e.g. extruded polygons, integrated mesh scene layer), or draped directly onto the terrain (e.g. location markers, area boundaries).

## How to use the sample

The application loads a scene showing four points that use individual surface placement modes (Absolute, Relative, Relative to Scene, and either Draped Billboarded or Draped Flat). Use the toggle to change the draped mode and the slider to dynamically adjust the Z value of the graphics. Explore the scene by zooming in/out and by panning around to observe the effects of the surface placement rules.

## How it works

1. Create a `GraphicsOverlay` for each placement mode, setting `SceneProperties.SurfacePlacement`:
    * `Absolute`, position graphic using only its Z value.
    * `Relative`, position graphic using its Z value plus the elevation of the surface.
    * `DrapedBillboarded`, position graphic upright on the surface and always facing the camera, not using its z value.
    * `DrapedFlat`, position graphic flat on the surface, not using its z value.
    * `RelativeToScene`, position graphic using its Z value plus the altitude values of the scene.
2. Add graphics to the graphics overlay, `GraphicsOverlayGraphics.Add(Graphic)`.
3. Add each graphics overlay to the scene view by calling `SceneView.GraphicsOverlays.Add(overlay)`.

## Relevant API

* Graphic
* GraphicsOverlay
* SceneProperties.SurfacePlacement
* SceneProperties
* Surface

## About the data

The scene launches with a view of Brest, France. Four points are shown hovering with positions defined by each of the different surface placement modes.

## Additional information

This sample uses an elevation service to add elevation/terrain to the scene. Graphics are positioned relative to that surface for the `DrapedBillboarded`, `DrapedFlat`, and `Relative` surface placement modes. It also uses a scene layer containing 3D models of buildings. Graphics are positioned relative to that scene layer for the `RelativeToScene` surface placement mode. 

## Tags

3D, absolute, altitude, draped, elevation, floating, relative, scenes, sea level, surface placement