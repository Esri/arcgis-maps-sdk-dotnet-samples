﻿// Copyright 2019 Esri.
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
        "Delete features (feature service)",
        "Data",
        "Delete features from a feature service.",
        "")]
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
                long featureId = (long) identifyResult.GeoElements.First().Attributes["objectid"];

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
                await ((Page)Parent).DisplayAlert("Error", ex.ToString(), "OK");
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

                // Sync the change with the service.
                ServiceFeatureTable serviceTable = (ServiceFeatureTable) _damageLayer.FeatureTable;
                await serviceTable.ApplyEditsAsync();

                // Show a message confirming the deletion.
                await ((Page)Parent).DisplayAlert("Success!", $"Deleted feature with ID {_tappedFeature.Attributes["objectid"]}", "OK");
            }
            catch (Exception ex)
            {
                await ((Page)Parent).DisplayAlert("Error", ex.ToString(), "OK");
            }
        }
    }
}
