// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.FeatureLayerSelection
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Feature layer selection",
        category: "Layers",
        description: "Select features in a feature layer.",
        instructions: "Click on a feature in the map. All features within a given tolerance (in pixels) of the click will be selected.",
        tags: new[] { "features", "layers", "select", "selection", "tolerance" })]
    public partial class FeatureLayerSelection
    {
        // Hold a reference to the feature layer.
        private FeatureLayer _featureLayer;

        public FeatureLayerSelection()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create new Map with basemap.
            Map myMap = new Map(BasemapStyle.ArcGISLightGray);

            // Create envelope to be used as a target extent for map's initial viewpoint.
            Envelope myEnvelope = new Envelope(-6603299.491810, 1679677.742046, 9002253.947487, 8691318.054732, SpatialReferences.WebMercator);

            // Set the initial viewpoint for map.
            myMap.InitialViewpoint = new Viewpoint(myEnvelope);

            // Provide used Map to the MapView.
            MyMapView.Map = myMap;

            // Set the selection color.
            MyMapView.SelectionProperties.Color = Color.Cyan;

            // Create Uri for the feature service.
            Uri featureServiceUri = new Uri(
                "https://services1.arcgis.com/4yjifSiIG17X0gW4/arcgis/rest/services/GDP_per_capita_1960_2016/FeatureServer/0");

            // Initialize feature table using a URL to feature server.
            ServiceFeatureTable featureTable = new ServiceFeatureTable(featureServiceUri);

            // Initialize a new feature layer based on the feature table.
            _featureLayer = new FeatureLayer(featureTable);

            try
            {
                // Make sure that used feature layer is loaded before hooking into the tapped event
                // This prevents trying to do selection on the layer that isn't initialized.
                await _featureLayer.LoadAsync();

                // Check for the load status. If the layer is loaded then add it to map.
                if (_featureLayer.LoadStatus == LoadStatus.Loaded)
                {
                    // Add the feature layer to the map.
                    myMap.OperationalLayers.Add(_featureLayer);

                    // Add tap event handler for mapview.
                    MyMapView.GeoViewTapped += OnMapViewTapped;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
            }
        }

        private async void OnMapViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Define the selection tolerance.
                double tolerance = 15;

                // Convert the tolerance to map units.
                double mapTolerance = tolerance * MyMapView.UnitsPerPixel;

                // Get the tapped point.
                MapPoint geometry = e.Location;

                // Normalize the geometry if wrap-around is enabled.
                //    This is necessary because of how wrapped-around map coordinates are handled by Runtime.
                //    Without this step, querying may fail because wrapped-around coordinates are out of bounds.
                if (MyMapView.IsWrapAroundEnabled)
                {
                    geometry = (MapPoint)geometry.NormalizeCentralMeridian();
                }

                // Define the envelope around the tap location for selecting features.
                Envelope selectionEnvelope = new Envelope(geometry.X - mapTolerance, geometry.Y - mapTolerance, geometry.X + mapTolerance,
                    geometry.Y + mapTolerance, MyMapView.Map.SpatialReference);

                // Define the query parameters for selecting features.
                QueryParameters queryParams = new QueryParameters
                {
                    // Set the geometry to selection envelope for selection by geometry.
                    Geometry = selectionEnvelope
                };

                // Select the features based on query parameters defined above.
                await _featureLayer.SelectFeaturesAsync(queryParams, SelectionMode.New);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }
    }
}