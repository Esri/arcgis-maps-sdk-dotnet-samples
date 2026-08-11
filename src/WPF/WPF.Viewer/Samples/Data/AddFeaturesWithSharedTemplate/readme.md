# Add features with shared templates

Create features from preset and group shared templates, then save or discard the local edits.

![Add features with shared templates](AddFeaturesWithSharedTemplate.jpg)

## Use case

Shared templates provide a consistent feature-creation experience across editing applications. A template can provide default attributes, symbology, geometry-construction settings, and related or associated features. This is useful when field staff need to create complete, valid features with a small number of guided choices.

## How to use the sample

Click on a shared template to create features. Draw geometry. Save or undo local edits.

## How it works

1. Load the public [Parks and Grounds Assets](https://arcgisruntime.maps.arcgis.com/home/item.html?id=dd64a70d17de4f16a93d2203c4cf1ab3) web map.
2. Determine an `ISharedTemplateSource` from the map layers' geodatabase.
3. Call `ServiceGeodatabase.QuerySharedTemplatesAsync()` to build a template picker with only the preset and group template types.
4. Call `SharedTemplate.CreateSwatchAsync(layerId)` to create an image for each template picker item; use a default image when swatch is not available.
5. Call `SharedTemplate.GetDefaultConstructionTool(layerId)` to determine the type of geometry to create with `GeometryEditor` .
6. Call `GeometryEditor.Stop()` when the user selects **Complete**, then use the returned geometry with `ServiceGeodatabase.CreateFeaturesAsync(sharedTemplate, geometry)` and `ServiceGeodatabase.AddFeaturesAsync()` to commit the feature set to the geodatabase.
7. Select **Save** to call `ServiceGeodatabase.ApplyEditsAsync()`, or **Cancel** to call `ServiceGeodatabase.UndoLocalEditsAsync()`.

## Relevant API

* GeometryEditor
* ServiceGeodatabase
* SharedTemplate
* SharedTemplateFeatureCreationSet
* SharedTemplateQueryParameters

## Tags

edit, feature, preset, shared template, template
