// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ArcGIS.WPF.Samples.IdentifyRasterCell
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Identify raster cell",
        category: "Layers",
        description: "Get the cell value of a local raster at the tapped location and display the result in a callout.",
        instructions: "Tap or move your cursor over an area of the raster to identify a raster cell and display it's attributes in a callout.",
        tags: new[] { "NDVI", "band", "cell", "cell value", "continuous", "discrete", "identify", "pixel", "pixel value", "raster" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("b5f977c78ec74b3a8857ca86d1d9b318")]
    public partial class IdentifyRasterCell
    {
        // Raster layer to display raster data on the map.
        private RasterLayer _rasterLayer;

        // Boolean to prevent concurrent identify calls.
        private bool _isIdentifying = false;

        // Store the next identify cell task.
        private Action _nextIdentifyAction = null;

        public IdentifyRasterCell()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Define a new map with Wgs84 Spatial Reference.
            var map = new Map(BasemapStyle.ArcGISOceans);
            map.InitialViewpoint = new Viewpoint(-34.1, 18.6, 9);

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

            // Disable magnifier.
            MyMapView.InteractionOptions = new MapViewInteractionOptions
            {
                IsMagnifierEnabled = false
            };
        }

        private void MouseMoved(object sender, MouseEventArgs mouseArgs)
        {
            // Get the curent mouse position.
            Point position = mouseArgs.GetPosition(MyMapView);

            // Identify the raster cell at that position.
            _ = IdentifyCell(position);
        }

        private async Task IdentifyCell(Point position)
        {
            // Check if a cell is already being identified.
            if (_isIdentifying)
            {
                _nextIdentifyAction = () => _ = IdentifyCell(position);
                return;
            }

            // Set variable to true to prevent concurrent identify calls.
            _isIdentifying = true;

            try
            {
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
            finally
            {
                _isIdentifying = false;
            }

            // Check if there is a new position to identify.
            if (_nextIdentifyAction != null)
            {
                Action action = _nextIdentifyAction;

                // Clear the queued identify action.
                _nextIdentifyAction = null;

                // Run the next action.
                action();
            }
        }
    }
}