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
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Linq;
using Windows.UI.Popups;

namespace ArcGISRuntime.UWP.Samples.UpdateGeometries
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Update geometries (feature service)",
        category: "Data",
        description: "Update a feature's location in an online feature service.",
        instructions: "Tap a feature to select it. Tap again to set the updated location for that feature. An alert will be shown confirming success or failure.",
        tags: new[] { "editing", "feature layer", "feature table", "moving", "service", "updating" })]
    public partial class UpdateGeometries
    {
        // URL to the feature service.
        private const string FeatureServiceUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/DamageAssessment/FeatureServer/0";

        // Hold a reference to the feature layer.
        private FeatureLayer _damageLayer;

        // Hold a reference to the selected feature.
        private ArcGISFeature _selectedFeature;

        public UpdateGeometries()
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

            // Listen for user taps on the map - on tap, a feature will be selected.
            MyMapView.GeoViewTapped += MapView_Tapped;

            // Zoom to the United States.
            MyMapView.SetViewpointCenterAsync(new MapPoint(-10800000, 4500000, SpatialReferences.WebMercator), 3e7);
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
                await new MessageDialog($"Moved feature {_selectedFeature.Attributes["objectid"]}", "Success!").ShowAsync();
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.ToString(), "Error when moving feature.").ShowAsync();
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
                IdentifyLayerResult identifyResult = await MyMapView.IdentifyLayerAsync(_damageLayer, tapEventDetails.Position, 10, false);

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
                await new MessageDialog(ex.ToString(), "There was a problem.").ShowAsync();
            }
        }
    }
}