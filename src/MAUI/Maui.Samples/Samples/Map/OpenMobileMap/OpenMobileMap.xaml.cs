// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;

namespace ArcGIS.Samples.OpenMobileMap
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Open mobile map package",
        category: "Map",
        description: "Display a map from a mobile map package.",
        instructions: "When the sample opens, it will automatically display the map in the mobile map package. Pan and zoom to observe the data from the mobile map package.",
        tags: new[] { "mmpk", "mobile map package", "offline" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("e1f3a7254cb845b09450f54937c16061")]
    public partial class OpenMobileMap : ContentPage
    {
        public OpenMobileMap()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
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
                MyMapView.Map = myMapPackage.Maps.First();
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Error", e.ToString(), "OK");
            }
        }
    }
}