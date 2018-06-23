// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.RasterLayerFile
{
    [Register("RasterLayerFile")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("7c4c679ab06a4df19dc497f577f111bd")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Raster layer (file)",
        "Layers",
        "This sample demonstrates how to use a raster layer created from a local raster file.",
        "The raster file is downloaded by the sample viewer automatically.")]
    public class RasterLayerFile : UIViewController
    {
        // Create and hold a reference to the MapView.
        private MapView _myMapView;

        public RasterLayerFile()
        {
            Title = "Raster layer (file)";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;

                // Reposition controls.
                _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, 0, 0);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private async void Initialize()
        {
            // Create a new map with imagery basemap.
            Map map = new Map(Basemap.CreateImagery());

            // Get the file name.
            string filepath = DataManager.GetDataFolder("7c4c679ab06a4df19dc497f577f111bd", "raster-file", "Shasta.tif");

            // Load the raster file.
            Raster rasterFile = new Raster(filepath);

            // Create the layer.
            RasterLayer rasterLayer = new RasterLayer(rasterFile);

            // Add the layer to the map.
            map.OperationalLayers.Add(rasterLayer);

            // Wait for the layer to load.
            await rasterLayer.LoadAsync();

            // Set the viewpoint.
            map.InitialViewpoint = new Viewpoint(rasterLayer.FullExtent);

            // Add map to the mapview.
            _myMapView.Map = map;
        }

        private void CreateLayout()
        {
            // Create the mapview.
            _myMapView = new MapView();

            // Add the map to the view.
            View.AddSubviews(_myMapView);
        }
    }
}