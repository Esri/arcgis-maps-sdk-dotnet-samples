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
using System;
using System.Linq;
using Windows.UI.Popups;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArcGISRuntime.WinUI.Samples.DeleteFeatures
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Delete features (feature service)",
        category: "Data",
        description: "Delete features from an online feature service.",
        instructions: "To delete a feature, tap it, then click 'Delete incident'.",
        tags: new[] { "Service", "deletion", "feature", "online", "table" })]
    public partial class DeleteFeatures
    {
        // Path to the feature service.
        private const string FeatureServiceUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/DamageAssessment/FeatureServer/0";

        // Hold a reference to the feature layer.
        private FeatureLayer _damageLayer;

        public DeleteFeatures()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create the map with streets basemap.
            MyMapView.Map = new Map(Basemap.CreateStreets());

            // Create the feature table, referring to the Damage Assessment feature service.
            ServiceFeatureTable damageTable = new ServiceFeatureTable(new Uri(FeatureServiceUrl));

            // Create a feature layer to visualize the features in the table.
            _damageLayer = new FeatureLayer(damageTable);

            // Add the layer to the map.
            MyMapView.Map.OperationalLayers.Add(_damageLayer);

            // Listen for user taps on the map - on tap, a callout will be shown.
            MyMapView.GeoViewTapped += MapView_Tapped;

            // Zoom to the United States.
            MyMapView.SetViewpointCenterAsync(new MapPoint(-10800000, 4500000, SpatialReferences.WebMercator), 3e7);
        }

        private async void MapView_Tapped(object sender, GeoViewInputEventArgs e)
        {
            // Clear any existing selection.
            _damageLayer.ClearSelection();

            // Dismiss any existing callouts.
            MyMapView.DismissCallout();

            try
            {
                // Perform an identify to determine if a user tapped on a feature.
                IdentifyLayerResult identifyResult = await MyMapView.IdentifyLayerAsync(_damageLayer, e.Position, 2, false);

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
                await new MessageDialog(ex.ToString()).ShowAsync();
            }
        }

        private void ShowDeletionCallout(Feature tappedFeature)
        {
            // Create a button for deleting the feature.
            Button deleteButton = new Button();
            deleteButton.Content = "Delete incident";
            deleteButton.Padding = new Thickness(5);
            deleteButton.Tag = tappedFeature;

            // Handle button clicks.
            deleteButton.Click += DeleteButton_Click;

            // Show the callout.
            MyMapView.ShowCalloutAt((MapPoint) tappedFeature.Geometry, deleteButton);
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Dismiss the callout.
            MyMapView.DismissCallout();

            try
            {
                // Get the feature to delete from the layer.
                Button deleteButton = (Button) sender;
                Feature featureToDelete = (Feature) deleteButton.Tag;

                // Delete the feature.
                await _damageLayer.FeatureTable.DeleteFeatureAsync(featureToDelete);

                // Sync the change with the service.
                ServiceFeatureTable serviceTable = (ServiceFeatureTable) _damageLayer.FeatureTable;
                await serviceTable.ApplyEditsAsync();

                // Show a message confirming the deletion.
                await new MessageDialog($"Deleted feature with ID {featureToDelete.Attributes["objectid"]}", "Success!").ShowAsync();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.ToString(), "Couldn't delete feature.").ShowAsync();
            }
        }
    }
}
