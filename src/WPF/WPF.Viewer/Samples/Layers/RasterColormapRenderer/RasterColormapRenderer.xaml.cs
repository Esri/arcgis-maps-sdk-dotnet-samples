// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.RasterColormapRenderer
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Colormap renderer",
        category: "Layers",
        description: "Apply a colormap renderer to a raster.",
        instructions: "Pan and zoom to explore the effect of the colormap applied to the raster.",
        tags: new[] { "colormap", "data", "raster", "renderer", "visualization" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("95392f99970d4a71bd25951beb34a508")]
    public partial class RasterColormapRenderer
    {
        // A single band raster file.
        private readonly string _rasterPath = DataManager.GetDataFolder("95392f99970d4a71bd25951beb34a508", "shasta", "ShastaBW.tif");

        public RasterColormapRenderer()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Add an imagery basemap.
            Map map = new Map(BasemapStyle.ArcGISImageryStandard);

            // Load the raster file.
            Raster rasterFile = new Raster(_rasterPath);

            // Create the layer.
            RasterLayer rasterLayer = new RasterLayer(rasterFile);

            // Create a color map where values 0-149 are red and 150-249 are yellow.
            IEnumerable<Color> colors = new int[250]
               .Select((c, i) => i < 150 ? Color.Red : Color.Yellow);

            // Create a colormap renderer.
            ColormapRenderer colormapRenderer = new ColormapRenderer(colors);

            // Set the colormap renderer on the raster layer.
            rasterLayer.Renderer = colormapRenderer;

            // Add the layer to the map.
            map.OperationalLayers.Add(rasterLayer);

            // Add map to the mapview.
            MyMapView.Map = map;

            try
            {
                // Wait for the layer to load.
                await rasterLayer.LoadAsync();

                // Set the viewpoint.
                await MyMapView.SetViewpointGeometryAsync(rasterLayer.FullExtent, 15);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                MessageBox.Show(e.Message, "Error");
            }
        }
    }
}