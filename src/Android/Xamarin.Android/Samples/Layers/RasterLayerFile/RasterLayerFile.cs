// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.Controls;
using ArcGISRuntime.Samples.Managers;

namespace ArcGISRuntime.Samples.RasterLayerFile
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData("7c4c679ab06a4df19dc497f577f111bd")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Raster layer (file)",
        category: "Layers",
        description: "Create and use a raster layer made from a local raster file.",
        instructions: "When the sample starts, a raster will be loaded from a file and displayed in the map view.",
        tags: new[] { "data", "image", "import", "layer", "raster", "visualization" })]
    public class RasterLayerFile : Activity
    {
        // Reference to the MapView used in the sample
        private MapView _myMapView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Raster layer (file)";

            // Create the layout
            CreateLayout();

            // Initialize the app
            Initialize();
        }

        private void CreateLayout()
        {
            // Create a stack layout
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the mapview to the layout
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Set the layout as the sample view
            SetContentView(layout);
        }

        private async void Initialize()
        {
            // Add an imagery basemap
            Map myMap = new Map(BasemapStyle.ArcGISImageryStandard);

            // Get the file name
            string filepath = GetRasterPath();

            // Load the raster file
            Raster myRasterFile = new Raster(filepath);

            // Create the layer
            RasterLayer myRasterLayer = new RasterLayer(myRasterFile);

            // Add the layer to the map
            myMap.OperationalLayers.Add(myRasterLayer);

            // Add map to the mapview
            _myMapView.Map = myMap;

            try
            {
                // Wait for the layer to load
                await myRasterLayer.LoadAsync();

                // Set the viewpoint
                await _myMapView.SetViewpointGeometryAsync(myRasterLayer.FullExtent);
            }
            catch (Exception e)
            {
                new AlertDialog.Builder(this).SetMessage(e.ToString()).SetTitle("Error").Show();
            }
        }

        private string GetRasterPath()
        {
            return DataManager.GetDataFolder("7c4c679ab06a4df19dc497f577f111bd", "raster-file", "Shasta.tif");
        }
    }
}