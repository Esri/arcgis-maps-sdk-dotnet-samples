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
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

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
        // Raster layer to display raster data on the map.
        private RasterLayer _rasterLayer;

        public IdentifyRasterCell()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            // Define a new map with Wgs84 Spatial Reference.
            var map = new Map(BasemapType.Oceans, latitude: -34.1, longitude: 18.6, levelOfDetail: 9);

            // Get the file name for the raster.
            string filepath = DataManager.GetDataFolder("b5f977c78ec74b3a8857ca86d1d9b318", "SA_EVI_8Day_03May20.tif");

            // Load the raster file.
            var raster = new Raster(filepath);

            // Initialize the raster layer.
            _rasterLayer = new RasterLayer(raster);

            // Add the raster layer to the map.
            map.OperationalLayers.Add(_rasterLayer);

            // Add map to the map view.
            MyMapView.Map = map;

            try
            {
                // Wait for the layer to load.
                await _rasterLayer.LoadAsync();

                // Set the viewpoint.
                await MyMapView.SetViewpointGeometryAsync(_rasterLayer.FullExtent);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }

            // Listen for mouse movement to start the identify operation.
            MyMapView.MouseMove += MouseMoved;
        }

        private async void MouseMoved(object sender, MouseEventArgs e)
        {
            try
            {
                // Get the curent mouse position.
                Point position = e.GetPosition(MyMapView);

                // Get the result for where the user hovered on the raster layer.
                IdentifyLayerResult identifyResult = await MyMapView.IdentifyLayerAsync(_rasterLayer, position, 1, false, 1);

                // If no cell was identified, dismiss the callout.
                if (!identifyResult.GeoElements.Any())
                {
                    MyMapView.DismissCallout();
                    return;
                }

                // Create a StringBuilder to display information to the user.
                var stringBuilder = new StringBuilder();

                // Get the identified raster cell.
                GeoElement cell = identifyResult.GeoElements.First();

                // Loop through the attributes (key/value pairs).
                foreach (KeyValuePair<string, object> keyValuePair in cell.Attributes)
                {
                    // Add the key/value pair to the string builder.
                    stringBuilder.AppendLine($"{keyValuePair.Key}: {keyValuePair.Value}");
                }

                // Get the x and y values of the cell.
                double x = cell.Geometry.Extent.XMin;
                double y = cell.Geometry.Extent.YMin;

                // Add the X & Y coordinates where the user clicked raster cell to the string builder.
                stringBuilder.AppendLine($"X: {Math.Round(x, 4)}\nY: {Math.Round(y, 4)}");

                // Create a callout using the string.
                var definition = new CalloutDefinition(stringBuilder.ToString());

                // Display the call out in the map view.
                MyMapView.ShowCalloutAt(MyMapView.ScreenToLocation(position), definition);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().ToString());
            }
        }
    }
}