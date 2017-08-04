// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System.Linq;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;
using System.IO;
using ArcGISRuntimeXamarin.Managers;

namespace ArcGISRuntimeXamarin.Samples.OpenMobileMap
{
    [Register("OpenMobileMap")]
    public class OpenMobileMap : UIViewController
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

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
            base.ViewDidLayoutSubviews();

            // Update the UI to account for new layout
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
        }

        private async void Initialize()
        {
            // The mobile map package will be downloaded from ArcGIS Online
            // The data manager (a component of the sample viewer, *NOT* the runtime
            //     handles the offline data process

            // The desired MMPK is expected to be called Yellowstone.mmpk
            string filename = "Yellowstone.mmpk";

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "OpenMobileMap", filename);

            // Variable to hold the MobileMapPackage
            MobileMapPackage myMapPackage;

            // Open the package, try downloading if that fails
            try
            {
                myMapPackage = await MobileMapPackage.OpenAsync(filepath);
            }
            catch (FileNotFoundException)
            {
                // Download the package
                await DataManager.GetData("e1f3a7254cb845b09450f54937c16061", "OpenMobileMap");
                // try again
                myMapPackage = await MobileMapPackage.OpenAsync(filepath);
            }

            // Check that there is at least one map
            if (myMapPackage.Maps.Count > 0)
            {
                // Display the first map in the package
                _myMapView.Map = myMapPackage.Maps.First();
            }
        }

        private void CreateLayout()
        {
            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            // Add MapView to the page
            View.AddSubview(_myMapView);
        }
    }
}