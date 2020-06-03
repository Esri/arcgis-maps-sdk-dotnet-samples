// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Linq;

namespace ArcGISRuntimeXamarin.Samples.UpdateAttributes
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Update attributes (feature service)",
        category: "Data",
        description: "Update feature attributes in an online feature service.",
        instructions: "To change the feature's damage property, tap the feature to select it, and update the damage type using the drop down.",
        tags: new[] { "amend", "attribute", "details", "edit", "editing", "information", "value" })]
    public class UpdateAttributes : Activity
    {
        // Hold a reference to the MapView.
        private MapView _myMapView;

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

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Update attributes (feature service)";

            CreateLayout();
            Initialize();
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
            // This code needs to work with the UI, so it needs to run on the UI thread.
            RunOnUiThread(() =>
            {
                // Get the relevant field from the table.
                ServiceFeatureTable table = (ServiceFeatureTable) sender;
                Field typeDamageField = table.Fields.First(field => field.Name == AttributeFieldName);

                // Get the domain for the field.
                _domain = (CodedValueDomain) typeDamageField.Domain;

                // Listen for user taps on the map - this will select the feature.
                _myMapView.GeoViewTapped += MapView_Tapped;
            });
        }

        private async void MapView_Tapped(object sender, GeoViewInputEventArgs e)
        {
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
                _selectedFeature = (ArcGISFeature)identifyResult.GeoElements.First();

                // Select the feature.
                _damageLayer.SelectFeature(_selectedFeature);

                // Update the UI for the selection.
                ShowFeatureOptions();
            }
            catch (Exception ex)
            {
                ShowMessage("Error selecting feature.", ex.ToString());
            }
        }

        private void ShowFeatureOptions()
        {
            // Get the current value.
            string currentAttributeValue = _selectedFeature.Attributes[AttributeFieldName].ToString();

            // Set up the UI for the callout.
            Button changeValueButton = new Button(this);
            changeValueButton.Text = $"{currentAttributeValue} - Edit";
            changeValueButton.Click += ShowDamageTypeChoices;

            // Show the callout.
            _myMapView.ShowCalloutAt((MapPoint) _selectedFeature.Geometry, changeValueButton);
        }

        private void ShowDamageTypeChoices(object sender, EventArgs e)
        {
            // Create menu to show options.
            PopupMenu menu = new PopupMenu(this, (Button) sender);
            menu.MenuItemClick += (o, menuArgs) => { UpdateDamageType(menuArgs.Item.ToString()); };

            // Create menu options.
            foreach (CodedValue codeValue in _domain.CodedValues)
            {
                menu.Menu.Add(codeValue.Name);
            }

            // Show menu in the view.
            menu.Show();
        }

        private async void UpdateDamageType(string selectedAttributeValue)
        {
            try
            {
                // Load the feature.
                await _selectedFeature.LoadAsync();

                if (_selectedFeature.Attributes[AttributeFieldName].ToString() == selectedAttributeValue)
                {
                    throw new Exception("Old and new attribute values are the same.");
                }

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
            // Display the message to the user.
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetMessage(message).SetTitle(title).Show();
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            var layout = new LinearLayout(this) {Orientation = Orientation.Vertical};

            // Create the MapView.
            _myMapView = new MapView(this);

            // Create the help label.
            TextView helpLabel = new TextView(this);
            helpLabel.Text = "Tap to select a feature to edit.";
            helpLabel.TextAlignment = TextAlignment.Center;
            helpLabel.Gravity = GravityFlags.Center;

            // Add the help label to the layout.
            layout.AddView(helpLabel);

            // Add the map view to the layout.
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}