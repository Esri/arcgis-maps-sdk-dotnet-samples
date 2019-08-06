# Update attributes (feature service)

Update feature attributes in an online feature service.

![](UpdateAttributes.jpg)

## How to use the sample

Tap a feature to select it, then click the button to select a value.

## How it works

To get a Feature from a `ServiceFeatureTable` and update its attributes:

* Create a service feature table from a URL.
  * When the table loads, you can get the domain to determine which options to present in your UI.
* Create a feature layer from the service feature table.
* Select features from the feature layer.
* To update the feature's attribute, first load it, then use the `ArcGISFeature.SetAttributeValue()`.
* Update the table with `ServiceFeatureTable.UpdateFeatureAsync()`.
* After a change, apply the changes on the server using `ServiceFeatureTable.ApplyEditsAsync()`.

## Relevant API

* Map
* ArcGISFeature
* FeatureLayer
* MapView
* ServiceFeatureTable

## Tags

Editing, attribute, value, domain, coded value, coded value domain