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

namespace ArcGISRuntimeXamarin.Samples.DeleteFeatures
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Delete features (feature service)",
        category: "Data",
        description: "Delete features from an online feature service.",
        instructions: "To delete a feature, tap it, then tap 'Delete incident'.",
        tags: new[] { "Service", "deletion", "feature", "online", "table" })]
    public class DeleteFeatures : Activity
    {
        // Hold a reference to the MapView.
        private MapView _myMapView;

        // URL to the feature service.
        private const string FeatureServiceUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/DamageAssessment/FeatureServer/0";

        // Hold a reference to the feature layer.
        private FeatureLayer _damageLayer;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Delete features";

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

            // Listen for user taps on the map - on tap, a callout will be shown.
            _myMapView.GeoViewTapped += MapView_Tapped;

            // Zoom to the United States.
            _myMapView.SetViewpointCenterAsync(new MapPoint(-10800000, 4500000, SpatialReferences.WebMercator), 3e7);
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

                // Otherwise, get the ID of the first result.
                long featureId = (long) identifyResult.GeoElements.First().Attributes["objectid"];

                // Get the feature by constructing a query and running it.
                QueryParameters qp = new QueryParameters();
                qp.ObjectIds.Add(featureId);
                FeatureQueryResult queryResult = await _damageLayer.FeatureTable.QueryFeaturesAsync(qp);
                Feature tappedFeature = queryResult.First();

                // Select the feature.
                _damageLayer.SelectFeature(tappedFeature);

                // Show the callout.
                ShowDeletionCallout(tappedFeature);
            }
            catch (Exception ex)
            {
                ShowMessage(ex.ToString(), "There was a problem.");
            }
        }

        private void ShowDeletionCallout(Feature tappedFeature)
        {
            // Create a button for deleting the feature.
            Button deleteButton = new Button(this);
            deleteButton.Text = "Delete feature";

            // Handle button clicks.
            deleteButton.Click += (o, e) => { DeleteFeature(tappedFeature); };

            // Show the callout.
            _myMapView.ShowCalloutAt((MapPoint) tappedFeature.Geometry, deleteButton);
        }

        private async void DeleteFeature(Feature featureToDelete)
        {
            // Dismiss the callout.
            _myMapView.DismissCallout();

            try
            {
                // Delete the feature.
                await _damageLayer.FeatureTable.DeleteFeatureAsync(featureToDelete);

                // Sync the change with the service.
                ServiceFeatureTable serviceTable = (ServiceFeatureTable) _damageLayer.FeatureTable;
                await serviceTable.ApplyEditsAsync();

                // Show a message confirming the deletion.
                ShowMessage($"Deleted feature with ID {featureToDelete.Attributes["objectid"]}", "Success!");
            }
            catch (Exception ex)
            {
                ShowMessage(ex.ToString(), "Couldn't delete feature");
            }
        }

        private void ShowMessage(string message, string title)
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
            helpLabel.Text = "Tap to select a feature for deletion.";
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