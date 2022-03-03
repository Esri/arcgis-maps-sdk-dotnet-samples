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
using System.Windows;

namespace ArcGISRuntime.WPF.Samples.AddFeatures
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Add features",
        category: "Data",
        description: "Add features to a feature layer.",
        instructions: "Click on a location on the map to add a feature at that location.",
        tags: new[] { "edit", "feature", "online service" })]
    public partial class AddFeatures
    {
        // URL to the feature service.
        private const string FeatureServiceUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/DamageAssessment/FeatureServer/0";

        // Hold a reference to the feature table.
        private ServiceFeatureTable _damageFeatureTable;

        public AddFeatures()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            // Create the map with streets basemap.
            MyMapView.Map = new Map(BasemapStyle.ArcGISStreets);

            ServiceGeodatabase serviceGeodatabase = new ServiceGeodatabase(new Uri(FeatureServiceUrl));
            await serviceGeodatabase.LoadAsync();

            // Create the feature table, referring to the Damage Assessment feature service.
            _damageFeatureTable = serviceGeodatabase.GetTable(0);

            // Create a feature layer to visualize the features in the table.
            FeatureLayer damageLayer = new FeatureLayer(_damageFeatureTable);

            // Add the layer to the map.
            MyMapView.Map.OperationalLayers.Add(damageLayer);

            // Listen for user taps on the map - this will select the feature.
            MyMapView.GeoViewTapped += MapView_Tapped;

            // Zoom to the United States.
            MyMapView.SetViewpointCenterAsync(new MapPoint(-10800000, 4500000, SpatialReferences.WebMercator), 3e7);
        }

        private async void MapView_Tapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Create the feature.
                ArcGISFeature feature = (ArcGISFeature)_damageFeatureTable.CreateFeature();

                // Get the normalized geometry for the tapped location and use it as the feature's geometry.
                MapPoint tappedPoint = (MapPoint)GeometryEngine.NormalizeCentralMeridian(e.Location);
                feature.Geometry = tappedPoint;

                // Set feature attributes.
                feature.SetAttributeValue("typdamage", "Minor");
                feature.SetAttributeValue("primcause", "Earthquake");

                // Add the feature to the table.
                await _damageFeatureTable.AddFeatureAsync(feature);

                // Apply the edits to the service.
                await _damageFeatureTable.ApplyEditsAsync();

                // Update the feature to get the updated objectid - a temporary ID is used before the feature is added.
                feature.Refresh();

                // Confirm feature addition.
                MessageBox.Show("Created feature " + feature.Attributes["objectid"], "Success!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error adding feature");
            }
        }
    }
}