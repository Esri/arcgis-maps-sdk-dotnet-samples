// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.IO;
using ArcGISRuntime.Samples.Managers;

namespace ArcGISRuntime.Samples.RasterLayerFile
{
    [Activity(Label = "RasterLayerFile")]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData("7c4c679ab06a4df19dc497f577f111bd")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Raster layer (file)",
        "Layers",
        "This sample demonstrates how to use a raster layer created from a local raster file.",
        "The raster file is downloaded by the sample viewer automatically. Note that due to a known bug, this sample may crash in emulators running Android 4.4 (API level 19). All other platform versions are unaffected.")]
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

            // Create the mapview
            _myMapView = new MapView(this);

            // Add an imagery basemap
            _myMapView.Map = new Map(Basemap.CreateImagery());

            // Add the mapview to the layout
            layout.AddView(_myMapView);

            // Set the layout as the sample view
            SetContentView(layout);
        }

        private async void Initialize()
        {
            // Add an imagery basemap
            Map myMap = new Map(Basemap.CreateImagery());

            // Wait for the map to load
            await myMap.LoadAsync();

            // Get the file name
            String filepath = GetRasterPath();

            // Load the raster file
            Raster myRasterFile = new Raster(filepath);

            // Create the layer
            RasterLayer myRasterLayer = new RasterLayer(myRasterFile);

            // Add the layer to the map
            myMap.OperationalLayers.Add(myRasterLayer);

            // Wait for the layer to load
            await myRasterLayer.LoadAsync();

            // Set the viewpoint
            myMap.InitialViewpoint = new Viewpoint(myRasterLayer.FullExtent);

            // Add map to the mapview
            _myMapView.Map = myMap;
        }

        private string GetRasterPath()
        {
            return DataManager.GetDataFolder("7c4c679ab06a4df19dc497f577f111bd", "raster-file", "Shasta.tif");
        }
    }
}