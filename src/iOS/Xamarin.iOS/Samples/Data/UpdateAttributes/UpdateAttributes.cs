// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.UpdateAttributes
{
    [Register("UpdateAttributes")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Update attributes (feature service)",
        category: "Data",
        description: "Update feature attributes in an online feature service.",
        instructions: "To change the feature's damage property, tap the feature to select it, and update the damage type using the drop down.",
        tags: new[] { "attribute", "coded value", "coded value domain", "domain", "editing", "value" })]
    public class UpdateAttributes : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIButton _changeValueButton;

        // URL to the feature service.
        private const string FeatureServiceUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/DamageAssessment/FeatureServer/0";

        // Name of the field that will be updated.
        private const string AttributeFieldName = "typdamage";

        // Hold a reference to the feature layer.
        private FeatureLayer _damageLayer;

        // Hold a reference to the selected feature.
        private ArcGISFeature _selectedFeature;

        // Hold a reference to the domain.
        private CodedValueDomain _domain;

        public UpdateAttributes()
        {
            Title = "Update attributes (feature service)";
        }

        private void Initialize()
        {
            // Create the map with streets basemap.
            _myMapView.Map = new Map(Basemap.CreateStreets());

            // Create the feature table, referring to the Damage Assessment feature service.
            ServiceFeatureTable damageTable = new ServiceFeatureTable(new Uri(FeatureServiceUrl));

            // When the table loads, use it to discover the domain of the typdamage field.
            damageTable.Loaded += DamageTable_Loaded;

            // Create a feature layer to visualize the features in the table.
            _damageLayer = new FeatureLayer(damageTable);

            // Add the layer to the map.
            _myMapView.Map.OperationalLayers.Add(_damageLayer);

            // Zoom to the United States.
            _myMapView.SetViewpointCenterAsync(new MapPoint(-10800000, 4500000, SpatialReferences.WebMercator), 3e7);
        }

        private void DamageTable_Loaded(object sender, EventArgs e)
        {
            // Unsubscribe from event.
            ((ServiceFeatureTable) sender).Loaded -= DamageTable_Loaded;

            // This code needs to work with the UI, so it needs to run on the UI thread.
            BeginInvokeOnMainThread(() =>
            {
                // Get the relevant field from the table.
                ServiceFeatureTable table = (ServiceFeatureTable) sender;
                Field typeDamageField = table.Fields.First(field => field.Name == AttributeFieldName);

                // Get the domain for the field.
                _domain = (CodedValueDomain) typeDamageField.Domain;
            });
        }

        private async void MapView_Tapped(object sender, GeoViewInputEventArgs e)
        {
            // Skip if the sample isn't ready yet.
            if (_damageLayer == null) return;

            // Clear any existing selection.
            _damageLayer.ClearSelection();

            // Dismiss any existing callouts.
            _myMapView.DismissCallout();

            try
            {
                // Perform an identify to determine if a user tapped on a feature.
                IdentifyLayerResult identifyResult = await _myMapView.IdentifyLayerAsync(_damageLayer, e.Position, 8, false);

                // Do nothing if there are no results.
                if (!identifyResult.GeoElements.Any())
                {
                    return;
                }

                // Get the tapped feature.
                _selectedFeature = (ArcGISFeature) identifyResult.GeoElements.First();

                // Select the feature.
                _damageLayer.SelectFeature(_selectedFeature);

                // Update the UI for the selection.
                ShowFeatureCallout();
            }
            catch (Exception ex)
            {
                ShowMessage("Error selecting feature.", ex.ToString());
            }
        }

        private void ShowFeatureCallout()
        {
            // Get the current value.
            string currentAttributeValue = _selectedFeature.Attributes[AttributeFieldName].ToString();

            // Unsubscribe from previous event handler.
            if (_changeValueButton != null) _changeValueButton.TouchUpInside -= ShowDamageTypeChoices;

            // Set up the UI for the callout.
            _changeValueButton = new UIButton();
            _changeValueButton.SetTitle($"{currentAttributeValue} - Edit", UIControlState.Normal);
            _changeValueButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _changeValueButton.TouchUpInside += ShowDamageTypeChoices;

            // Show the callout.
            _myMapView.ShowCalloutAt((MapPoint) _selectedFeature.Geometry, _changeValueButton);
        }

        private void ShowDamageTypeChoices(object sender, EventArgs e)
        {
            // Show the currently selected attribute value.
            string message = $"Current value is {_selectedFeature.Attributes[AttributeFieldName]}";

            // Create the alert controller - will show current value and the options for changing the value.
            UIAlertController alertController = UIAlertController.Create("Choose a damage type.", message, UIAlertControllerStyle.Alert);

            // Get the list of valid choices - all of the coded values that aren't currently selected.
            IEnumerable<CodedValue> possibleChoices = _domain.CodedValues.Where(value => value.Name != _selectedFeature.Attributes[AttributeFieldName].ToString());

            // Add an action (shows as button) for every choice.
            foreach (CodedValue codeValue in possibleChoices)
            {
                alertController.AddAction(UIAlertAction.Create(codeValue.Name, UIAlertActionStyle.Default, action => UpdateDamageType(codeValue.Name)));
            }

            // Allow the user to cancel.
            alertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

            // Show the alert.
            PresentViewController(alertController, true, null);
        }

        private async void UpdateDamageType(string selectedAttributeValue)
        {
            try
            {
                // Load the feature.
                await _selectedFeature.LoadAsync();

                // Update the attribute value.
                _selectedFeature.SetAttributeValue(AttributeFieldName, selectedAttributeValue);

                // Update the table.
                await _selectedFeature.FeatureTable.UpdateFeatureAsync(_selectedFeature);

                // Update the service.
                ServiceFeatureTable table = (ServiceFeatureTable) _selectedFeature.FeatureTable;
                await table.ApplyEditsAsync();

                ShowMessage("Success!", $"Edited feature {_selectedFeature.Attributes["objectid"]}");
            }
            catch (Exception ex)
            {
                ShowMessage("Failed to edit feature", ex.ToString());
            }
            finally
            {
                // Clear the selection.
                _damageLayer.ClearSelection();
                _selectedFeature = null;

                // Dismiss any callout.
                _myMapView.DismissCallout();
            }
        }

        private void ShowMessage(string title, string message)
        {
            // Create the alert controller.
            UIAlertController alertController = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
            alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));

            // Show the alert.
            PresentViewController(alertController, true, null);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = UIColor.White };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            UILabel helpLabel = new UILabel
            {
                Text = "Tap to select features to edit.",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Center,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f),
                TextColor = UIColor.White,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Add the views.
            View.AddSubviews(_myMapView, helpLabel);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                helpLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                helpLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                helpLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                helpLabel.HeightAnchor.ConstraintEqualTo(40)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _myMapView.GeoViewTapped += MapView_Tapped;
            if (_changeValueButton != null) _changeValueButton.TouchUpInside += ShowDamageTypeChoices;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _myMapView.GeoViewTapped -= MapView_Tapped;
            if (_changeValueButton != null) _changeValueButton.TouchUpInside -= ShowDamageTypeChoices;
        }
    }
}