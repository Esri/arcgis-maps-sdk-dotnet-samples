// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntimeXamarin.Managers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.IO;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.RasterLayerFile
{
    [Register("RasterLayerFile")]
    public class RasterLayerFile : UIViewController
    {
        // Reference to the MapView used in the sample
        private MapView _myMapView;

        public RasterLayerFile()
        {
            Title = "Raster layer (file)";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the layout
            CreateLayout();

            // Initialize the app
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            // Set up the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            base.ViewDidLayoutSubviews();
        }

        private async void Initialize()
        {
            // Get the file name
            String filepath = GetRasterPath();

            // Load the raster file
            Raster myRasterFile = new Raster(filepath);

            // Create the layer
            RasterLayer myRasterLayer = new RasterLayer(myRasterFile);

            // Load the layer
            await myRasterLayer.LoadAsync();

            // Add the layer to the map
            _myMapView.Map.OperationalLayers.Add(myRasterLayer);

            // Get the raster's extent in a viewpoint
            Viewpoint myFullRasterExtent = new Viewpoint(myRasterLayer.FullExtent);

            // Zoom to the extent
            _myMapView.Map.InitialViewpoint = myFullRasterExtent;
        }

        private void CreateLayout()
        {
            // Create the mapview
            _myMapView = new MapView();

            // Add an imagery basemap
            _myMapView.Map = new Map(Basemap.CreateImagery());

            // Add the map to the view
            View.AddSubviews(_myMapView);
        }

        private string GetRasterPath()
        {
            #region offlinedata
            // The desired raster is expected to be called Shasta.tif
            string filename = "Shasta.tif";

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

			// Return the full path; Item ID is 7c4c679ab06a4df19dc497f577f111bd
			return Path.Combine(folder, "SampleData", "RasterLayerFile", filename);
            #endregion offlinedata
        }
    }
}