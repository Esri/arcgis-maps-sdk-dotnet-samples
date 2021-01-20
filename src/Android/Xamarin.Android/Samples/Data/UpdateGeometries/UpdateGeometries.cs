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
using Android.Widget;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Linq;
using Android.Views;

namespace ArcGISRuntimeXamarin.Samples.UpdateGeometries
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Update geometries (feature service)",
        category: "Data",
        description: "Update a feature's location in an online feature service.",
        instructions: "Tap a feature to select it. Tap again to set the updated location for that feature. An alert will be shown confirming success or failure.",
        tags: new[] { "editing", "feature layer", "feature table", "moving", "service", "updating" })]
    public class UpdateGeometries : Activity
    {
        // Hold a reference to the MapView.
        private MapView _myMapView;

        // URL to the feature service.
        private const string FeatureServiceUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/DamageAssessment/FeatureServer/0";

        // Hold a reference to the feature layer.
        private FeatureLayer _damageLayer;

        // Hold a reference to the selected feature.
        private ArcGISFeature _selectedFeature;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Update geometries (feature service)";

            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create the map with streets basemap.
            _myMapView.Map = new Map(BasemapStyle.ArcGISStreets);

            // Create the feature table, referring to the Damage Assessment feature service.
            ServiceFeatureTable damageTable = new ServiceFeatureTable(new Uri(FeatureServiceUrl));

            // Create a feature layer to visualize the features in the table.
            _damageLayer = new FeatureLayer(damageTable);

            // Add the layer to the map.
            _myMapView.Map.OperationalLayers.Add(_damageLayer);

            // Listen for user taps on the map - on tap, a feature will be selected.
            _myMapView.GeoViewTapped += MapView_Tapped;

            // Zoom to the United States.
            _myMapView.SetViewpointCenterAsync(new MapPoint(-10800000, 4500000, SpatialReferences.WebMercator), 3e7);
        }

        private void MapView_Tapped(object sender, GeoViewInputEventArgs e)
        {
            // Select the feature if none selected, move the feature otherwise.
            if (_selectedFeature == null)
            {
                // Select the feature.
                TrySelectFeature(e);
            }
            else
            {
                // Move the feature.
                MoveSelectedFeature(e);
            }
        }

        private async void MoveSelectedFeature(GeoViewInputEventArgs tapEventDetails)
        {
            try
            {
                // Get the MapPoint from the EventArgs for the tap.
                MapPoint destinationPoint = tapEventDetails.Location;

                // Normalize the point - needed when the tapped location is over the international date line.
                destinationPoint = (MapPoint) GeometryEngine.NormalizeCentralMeridian(destinationPoint);

                // Load the feature.
                await _selectedFeature.LoadAsync();

                // Update the geometry of the selected feature.
                _selectedFeature.Geometry = destinationPoint;

                // Apply the edit to the feature table.
                await _selectedFeature.FeatureTable.UpdateFeatureAsync(_selectedFeature);

                // Push the update to the service.
                ServiceFeatureTable serviceTable = (ServiceFeatureTable) _selectedFeature.FeatureTable;
                await serviceTable.ApplyEditsAsync();
                ShowMessage("Success!", $"Moved feature {_selectedFeature.Attributes["objectid"]}");
            }
            catch (Exception ex)
            {
                ShowMessage("Error when moving feature", ex.ToString());
            }
            finally
            {
                // Reset the selection.
                _damageLayer.ClearSelection();
                _selectedFeature = null;
            }
        }

        private async void TrySelectFeature(GeoViewInputEventArgs tapEventDetails)
        {
            try
            {
                // Perform an identify to determine if a user tapped on a feature.
                IdentifyLayerResult identifyResult = await _myMapView.IdentifyLayerAsync(_damageLayer, tapEventDetails.Position, 10, false);

                // Do nothing if there are no results.
                if (!identifyResult.GeoElements.Any())
                {
                    return;
                }

                // Get the tapped feature.
                _selectedFeature = (ArcGISFeature)identifyResult.GeoElements.First();

                // Select the feature.
                _damageLayer.SelectFeature(_selectedFeature);
            }
            catch (Exception ex)
            {
                ShowMessage("Problem selecting feature", ex.ToString());
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

            // Create the help label.
            TextView helpLabel = new TextView(this);
            helpLabel.Text = "Tap to select a feature. Tap again to move it.";
            helpLabel.TextAlignment = TextAlignment.Center;
            helpLabel.Gravity = GravityFlags.Center;

            // Add the help label to the layout.
            layout.AddView(helpLabel);

            // Create the map view.
            _myMapView = new MapView(this);

            // Add the map view to the layout.
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}