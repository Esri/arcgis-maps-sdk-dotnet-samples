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
        "Open mobile map package",
        "Map",
        "Display a map from a mobile map package.",
        "When the sample opens, it will automatically display the map in the mobile map package. Pan and zoom to observe the data from the mobile map package.",
        "mmpk", "mobile map package", "offline")]
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
                // Open the map package.
                MobileMapPackage myMapPackage = await MobileMapPackage.OpenAsync(filepath);

                // Load the package.
                await myMapPackage.LoadAsync();

                // Display the first map in the package.
                _myMapView.Map = myMapPackage.Maps.First();
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
            View = new UIView() { BackgroundColor = UIColor.White };

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