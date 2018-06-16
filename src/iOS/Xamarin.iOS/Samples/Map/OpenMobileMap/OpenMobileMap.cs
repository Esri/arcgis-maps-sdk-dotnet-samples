// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Linq;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;
using ArcGISRuntime.Samples.Managers;

namespace ArcGISRuntime.Samples.OpenMobileMap
{
    [Register("OpenMobileMap")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("e1f3a7254cb845b09450f54937c16061")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Open mobile map (map package)",
        "Map",
        "This sample demonstrates how to open a mobile map from a map package.",
        "The map package will be downloaded from an ArcGIS Online portal automatically.")]
    public class OpenMobileMap : UIViewController
    {
        // Create and hold a reference to the MapView.
        private readonly MapView _myMapView = new MapView();

        public OpenMobileMap()
        {
            Title = "Open mobile map (map package)";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;

            // Reposition controls.
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
            _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, 0, 0);

            base.ViewDidLayoutSubviews();
        }

        private async void Initialize()
        {
            // Get the path to the mobile map package.
            string filepath = DataManager.GetDataFolder("e1f3a7254cb845b09450f54937c16061", "Yellowstone.mmpk");

            // Open the map package.
            MobileMapPackage myMapPackage = await MobileMapPackage.OpenAsync(filepath);

            // Check that there is at least one map.
            if (myMapPackage.Maps.Count > 0)
            {
                // Display the first map in the package.
                _myMapView.Map = myMapPackage.Maps.First();
            }
        }

        private void CreateLayout()
        {
            // Add MapView to the page.
            View.AddSubview(_myMapView);
        }
    }
}