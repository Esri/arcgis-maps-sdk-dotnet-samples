// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace ArcGISRuntime.WPF.Samples.IdentifyRasterCell
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Identify raster cell",
        "Layers",
        "",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("b5f977c78ec74b3a8857ca86d1d9b318")]
    public partial class IdentifyRasterCell
    {
        // Define a raster layer
        private RasterLayer _rasterLayer;

        public IdentifyRasterCell()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            // Define a new map with Wgs84 Spatial Reference
            var map = new Map(BasemapType.Oceans, latitude: -34.1, longitude: 18.6, levelOfDetail: 9);

            // Get the file name for the raster
            string filepath = DataManager.GetDataFolder("b5f977c78ec74b3a8857ca86d1d9b318",  "SA_EVI_8Day_03May20.tif");

            // Load the raster file
            var raster = new Raster(filepath);

            // Initialize the raster layer
            _rasterLayer = new RasterLayer(raster);

            // Add the raster layer to the map
            map.OperationalLayers.Add(_rasterLayer);

            // Add map to the map view
            MyMapView.Map = map;

            try
            {
                // Wait for the layer to load
                await _rasterLayer.LoadAsync();

                // Set the viewpoint
                await MyMapView.SetViewpointGeometryAsync(_rasterLayer.FullExtent);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, e.GetType().ToString());
            }

            // Listen for taps/clicks to start the identify operation.
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            try
            {
                // Get the identify value for where the user clicked on the raster layer
                IdentifyLayerResult identifyResult = await MyMapView.IdentifyLayerAsync(_rasterLayer, e.Position, 1, false, 10);

                // Get the read only list of geo elements (they contain RasterCell's)
                IReadOnlyList<GeoElement> cells = identifyResult.GeoElements;

                // Create a StringBuilder to display information to the user
                StringBuilder stringBuilder = new StringBuilder();

                // Loop through each RasterCell
                foreach (RasterCell cell in cells)
                {
                    // Loop through the attributes (key/value pairs)
                    foreach (KeyValuePair<string, object> keyValuePair in cell.Attributes)
                    {
                        // Add the key/value pair to the string builder
                        stringBuilder.AppendLine($"{keyValuePair.Key}: {keyValuePair.Value}");
                    }

                    // Shorten the X & Y values a little to show better in the call out
                    double x = cell.Geometry.Extent.XMin;
                    double y = cell.Geometry.Extent.YMin;

                    // Format the X & Y values as a human readable string
                    string calloutMessage = $"X: {Math.Round(x, 4)}\nY: {Math.Round(y, 4)}";

                    // Add the X & Y coordinates where the user clicked raster cell to the string builder
                    stringBuilder.AppendLine(calloutMessage);

                    // Define a call out based on the string builder
                    CalloutDefinition definition = new CalloutDefinition(stringBuilder.ToString());

                    // Display the call out in the map view
                    MyMapView.ShowCalloutAt(e.Location, definition);
                }
            }
            catch (Exception ex)
            {
                // Show any errors
                MessageBox.Show(ex.ToString(), "Error");
            }
        }
    }
}