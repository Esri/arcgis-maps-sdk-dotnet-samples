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
using System;
using System.Linq;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.DeleteFeatures
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Delete features (feature service)",
        category: "Data",
        description: "Delete features from an online feature service.",
        instructions: "To delete a feature, tap it, then tap 'Delete incident'.",
        tags: new[] { "Service", "deletion", "feature", "online", "table" })]
    public partial class DeleteFeatures : ContentPage
    {
        // Path to the feature service.
        private const string FeatureServiceUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/DamageAssessment/FeatureServer/0";

        // Hold a reference to the feature layer.
        private FeatureLayer _damageLayer;

        // Hold a reference to the most recently tapped feature.
        private Feature _tappedFeature;

        public DeleteFeatures()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                // Create the map with streets basemap.
                MyMapView.Map = new Map(BasemapStyle.ArcGISStreets);

                // Create a service geodatabase from the feature service.
                ServiceGeodatabase serviceGeodatabase = new ServiceGeodatabase(new Uri(FeatureServiceUrl));
                await serviceGeodatabase.LoadAsync();

                // Gets the feature table from the service geodatabase, referring to the Damage Assessment feature service.
                // Creating a feature table from the feature service will cause the service geodatbase to be null.
                ServiceFeatureTable damageTable = serviceGeodatabase.GetTable(0);

                // Create a feature layer to visualize the features in the table.
                _damageLayer = new FeatureLayer(damageTable);

                // Add the layer to the map.
                MyMapView.Map.OperationalLayers.Add(_damageLayer);

                // Listen for user taps on the map - on tap, a callout will be shown.
                MyMapView.GeoViewTapped += MapView_Tapped;

                // Zoom to the United States.
                _ = MyMapView.SetViewpointCenterAsync(new MapPoint(-10800000, 4500000, SpatialReferences.WebMercator), 3e7);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private async void MapView_Tapped(object sender, Esri.ArcGISRuntime.Xamarin.Forms.GeoViewInputEventArgs e)
        {
            // Clear any existing selection.
            _damageLayer.ClearSelection();

            // Reconfigure the button.
            DeleteButton.IsEnabled = false;
            DeleteButton.Text = "Delete feature";

            try
            {
                // Perform an identify to determine if a user tapped on a feature.
                IdentifyLayerResult identifyResult = await MyMapView.IdentifyLayerAsync(_damageLayer, e.Position, 5, false);

                // Do nothing if there are no results.
                if (!identifyResult.GeoElements.Any())
                {
                    return;
                }

                // Otherwise, get the ID of the first result.
                long featureId = (long)identifyResult.GeoElements.First().Attributes["objectid"];

                // Get the feature by constructing a query and running it.
                QueryParameters qp = new QueryParameters();
                qp.ObjectIds.Add(featureId);
                FeatureQueryResult queryResult = await _damageLayer.FeatureTable.QueryFeaturesAsync(qp);
                _tappedFeature = queryResult.First();

                // Select the feature.
                _damageLayer.SelectFeature(_tappedFeature);

                // Update the button to allow deleting the feature.
                ConfigureDeletionButton(_tappedFeature);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private void ConfigureDeletionButton(Feature tappedFeature)
        {
            // Create a button for deleting the feature.
            DeleteButton.Text = $"Delete incident {tappedFeature.Attributes["objectid"]}";
            DeleteButton.IsEnabled = true;
        }

        private async void DeleteButton_Click(object sender, EventArgs e)
        {
            // Reconfigure the button.
            DeleteButton.IsEnabled = false;
            DeleteButton.Text = "Delete feature";

            // Guard against null.
            if (_tappedFeature == null)
            {
                return;
            }

            try
            {
                // Delete the feature.
                await _damageLayer.FeatureTable.DeleteFeatureAsync(_tappedFeature);

                // Sync the change with the service geodatabase.
                ServiceFeatureTable serviceTable = (ServiceFeatureTable)_damageLayer.FeatureTable;
                await serviceTable.ServiceGeodatabase.ApplyEditsAsync();

                // Show a message confirming the deletion.
                await Application.Current.MainPage.DisplayAlert("Success!", $"Deleted feature with ID {_tappedFeature.Attributes["objectid"]}", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.ToString(), "OK");
            }
        }
    }
}