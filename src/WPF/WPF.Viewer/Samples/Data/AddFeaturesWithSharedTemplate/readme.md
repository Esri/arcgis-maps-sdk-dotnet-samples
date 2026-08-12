# Add features with shared templates

Create features from preset and group shared templates, then save or discard the local edits.

![Add features with shared templates](AddFeaturesWithSharedTemplate.jpg)

## Use case

Preset and group shared templates support guided, repeatable, high-quality editing. Preset templates place predefined feature arrangements, while group templates create related feature sets relative to a user-defined base geometry. By automatically applying attributes, symbology, geometry settings, and feature relationships, they help field staff create consistent, fully configured assets with only a few choices.

## How to use the sample

Click on a shared template to create features. Draw geometry. Save or undo local edits.

## How it works

1. Load the public [Parks and Grounds Assets](https://arcgisruntime.maps.arcgis.com/home/item.html?id=dd64a70d17de4f16a93d2203c4cf1ab3) web map.
2. Determine the `ISharedTemplateSource` by inspecting map layers, identifying `FeatureLayer` instances backed by a `ServiceFeatureTable`, and retrieving their `ServiceGeodatabase`.
3. Call `ISharedTemplateSource.QuerySharedTemplatesAsync()` to populate a template picker. Store each template's `layerId`, display its name and type, and use native contextual help to show its description.
4. Call `SharedTemplate.CreateSwatchAsync(layerId)`  to generate a swatch image for each template, falling back to a default image when a swatch is unavailable.
5. Call `SharedTemplate.GetDefaultConstructionTool(layerId)` to get the template’s default construction method. Use its `SharedTemplate.ToolType` to choose whether the `GeometryEditor` draws a point or polyline.
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
