# Add features with shared templates

Create features from preset and group shared templates, then save or discard the local edits.

![Add features with shared templates](AddFeaturesWithSharedTemplate.jpg)

## Use case

Preset and group shared templates support guided, repeatable, high-quality editing. They combine default attributes, symbology, geometry-construction settings, and related or associated features, so field staff can create complete, consistently configured feature sets with only a few choices.

## How to use the sample

Click on a shared template to create features. Draw geometry. Save or undo local edits.

## How it works

1. Load the public [Parks and Grounds Assets](https://arcgisruntime.maps.arcgis.com/home/item.html?id=dd64a70d17de4f16a93d2203c4cf1ab3) web map.
2. Determine an `ISharedTemplateSource` from the map layers.
3. Call `ISharedTemplateSource.QuerySharedTemplatesAsync()` to build a template picker.
4. Call `SharedTemplate.CreateSwatchAsync(layerId)`  to generate a swatch image for each template, falling back to a default image when a swatch is unavailable.
5. `SharedTemplate.GetDefaultConstructionTool(layerId)` to determine the appropriate GeometryEditor tool for creating the template’s geometry.
6. After the user selects **Complete**, call `GeometryEditor.Stop()` and use the returned geometry to create features with `ISharedTemplateSource.CreateFeaturesAsync(sharedTemplate, geometry)`. Then call `ISharedTemplateSource.AddFeaturesAsync()` to add the resulting feature set to the geodatabase.
7. Select **Save** to apply local edits with `ServiceGeodatabase.ApplyEditsAsync()`, or select **Cancel** to discard them with `ServiceGeodatabase.UndoLocalEditsAsync()`.

## Relevant API

* GeometryEditor
* ISharedTemplateSource
* ServiceGeodatabase
* SharedTemplate
* SharedTemplateFeatureCreationSet
* SharedTemplateQueryParameters

## Tags

edit, feature, group, preset, shared template, shared template source, template
