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
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Linq;

namespace ArcGISRuntime.Samples.OpenMobileMap
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("e1f3a7254cb845b09450f54937c16061")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Open mobile map package",
        "Map",
        "Display a map from a mobile map package.",
        "When the sample opens, it will automatically display the map in the mobile map package. Pan and zoom to observe the data from the mobile map package.",
        "mmpk", "mobile map package", "offline")]
    public class OpenMobileMap : Activity
    {
        private MapView _myMapView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Open mobile map (map package)";

            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            // Get the path to the mobile map package.
            string filepath = GetMmpkPath();

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
                new AlertDialog.Builder(this).SetMessage(e.ToString()).SetTitle("Error").Show();
            }
        }

        /// <summary>
        /// This abstracts away platform & sample viewer-specific code for accessing local files.
        /// </summary>
        /// <returns>String that is the path to the file on disk.</returns>
        private string GetMmpkPath()
        {
            return DataManager.GetDataFolder("e1f3a7254cb845b09450f54937c16061", "Yellowstone.mmpk");
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add a map view to the layout.
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}