// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.IO;
using System.Linq;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

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
        // Hold references to UI controls.
        private MapView _myMapView;

        public OpenMobileMap()
        {
            Title = "Open mobile map (map package)";
        }

        private async void Initialize()
        {
            // Get the path to the mobile map package.
            string filepath = DataManager.GetDataFolder("e1f3a7254cb845b09450f54937c16061", "Yellowstone.mmpk");

            try
            {
                // Load directly or unpack then load as needed by the map package.
                if (await MobileMapPackage.IsDirectReadSupportedAsync(filepath))
                {
                    // Open the map package.
                    MobileMapPackage myMapPackage = await MobileMapPackage.OpenAsync(filepath);

                    // Display the first map in the package.
                    _myMapView.Map = myMapPackage.Maps.First();
                }
                else
                {
                    // Create a path for the unpacked package.
                    string unpackedPath = filepath + "unpacked";

                    // Unpack the package.
                    await MobileMapPackage.UnpackAsync(filepath, unpackedPath);

                    // Open the package.
                    MobileMapPackage package = await MobileMapPackage.OpenAsync(unpackedPath);

                    // Load the package.
                    await package.LoadAsync();

                    // Show the first map.
                    _myMapView.Map = package.Maps.First();
                }
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView();

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            // Add the views.
            View.AddSubviews(_myMapView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });
        }
    }
}