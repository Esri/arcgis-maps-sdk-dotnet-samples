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
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System.IO;
using System.Linq;
using ArcGISRuntime.Samples.Managers;

namespace ArcGISRuntime.Samples.OpenMobileMap
{
    [Activity]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData("e1f3a7254cb845b09450f54937c16061")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Open mobile map (map package)",
        "Map",
        "This sample demonstrates how to open a map from a mobile map package.",
        "The map package will be downloaded from an ArcGIS Online portal automatically.")]
    public class OpenMobileMap : Activity
    {
        private MapView _myMapView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Open mobile map (map package)";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            // Get the path to the mobile map package
            string filepath = GetMmpkPath();

            // Open the map package
            MobileMapPackage myMapPackage = await MobileMapPackage.OpenAsync(filepath);

            // Check that there is at least one map
            if (myMapPackage.Maps.Count > 0)
            {
                // Display the first map in the package
                _myMapView.Map = myMapPackage.Maps.First();
            }
        }

        /// <summary>
        /// This abstracts away platform & sample viewer-specific code for accessing local files
        /// </summary>
        /// <returns>String that is the path to the file on disk</returns>
        private string GetMmpkPath()
        {
            return DataManager.GetDataFolder("e1f3a7254cb845b09450f54937c16061", "Yellowstone.mmpk");
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add a map view to the layout
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}