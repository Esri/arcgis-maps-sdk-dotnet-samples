# Edit features using feature forms

Display and edit feature attributes using feature forms.

![Image of Edit features using feature forms sample](EditFeaturesUsingFeatureForms.jpg)

## Use case

Feature forms help enhance the accuracy, efficiency, and user experience of attribute editing in your application. Forms can be authored as part of the web map using [Field Maps Designer](https://www.arcgis.com/apps/fieldmaps/) or using Map Viewer. This allows a simplified user experience to edit feature attribute data on the web map.

## How to use the sample

Tap a feature on the map to open a sheet displaying the feature form. Select form elements in the list and perform edits to update the field values. Tap the submit icon to commit the changes on the web map.

## How it works

1. Add a `Map` to the `MapView` using `Portal` URL and item ID.
2. When the map is tapped, perform an identify operation to check if the tapped location is an `ArcGISFeature`.
3. Create a `FeatureForm` object using the identified `ArcGISFeature`.
  * **Note:** If the feature's `FeatureLayer`, `ArcGISFeatureTable`, or the `SubtypeSublayer` has an authored `FeatureFormDefinition`, then this definition will be used to create the `FeatureForm`. If such a definition is not found, a default definition is generated.
4. Use the `FeatureForm` toolkit component to display the feature form configuration by providing the created `featureForm` object.
5. Optionally, you can add a `ValidationErrorVisibility` option to the `FeatureForm` toolkit component that determines the visibility of validation errors.
6. Once edits are added to the form fields, check if the validation errors list are empty using `featureForm.ValidationErrors` to verify that there are no errors.
7. To commit edits on the service geodatabase:
    1. Call `FinishEditingAsync()` to save edits to the database.
    2. Retrieve the backing service feature table's geodatabase using `serviceFeatureTable?.ServiceGeodatabase`.
    3. Verify the service geodatabase can commit changes back to the service using `serviceGeodatabase.ServiceInfo?.CanUseServiceGeodatabaseApplyEdits`
    4. If apply edits are allowed, call `ApplyEditsAsync()` to apply local edits to the online service.
    5. If edits are not allowed on the `ServiceGeodatabase`, then apply edits to the `ServiceFeatureTable` using `ApplyEditsAsync()`

## Relevant API

* ArcGISFeature
* FeatureForm
* FeatureLayer
* FieldFormElement
* GroupFormElement
* ServiceFeatureTable
* ServiceGeodatabase

## About the data

This sample uses a feature forms enabled [Feature Form Places web map](https://www.arcgis.com/home/item.html?id=516e4d6aeb4c495c87c41e11274c767f), which contains fictional places in San Diego of various hotels, restaurants, and shopping centers, with relevant reviews and ratings.

## Additional information

Follow the [tutorial](https://doc.arcgis.com/en/arcgis-online/create-maps/create-form-mv.htm) to create your own form using the Map Viewer. This sample uses the FeatureForm and GeoViewCompose Toolkit modules to be able to implement a Composable MapView which displays a Composable FeatureForm UI.

This sample uses the feature forms [toolkit](https://github.com/Esri/arcgis-maps-sdk-dotnet-toolkit/tree/main/src/Toolkit/Toolkit/UI/Controls/FeatureForm) component. For information about setting up the toolkit, as well as code for the underlying component, visit the [toolkit repository](https://github.com/Esri/arcgis-maps-sdk-dotnet-toolkit).

## Tags

edits, feature, featureforms, form, toolkit
