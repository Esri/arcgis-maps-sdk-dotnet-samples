// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISMapsSDK.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGISMapsSDK.WinUI.Samples.OpenMobileMap
{
    [ArcGISMapsSDK.Samples.Shared.Attributes.Sample(
        name: "Open mobile map package",
        category: "Map",
        description: "Display a map from a mobile map package.",
        instructions: "When the sample opens, it will automatically display the map in the mobile map package. Pan and zoom to observe the data from the mobile map package.",
        tags: new[] { "mmpk", "mobile map package", "offline" })]
    [ArcGISMapsSDK.Samples.Shared.Attributes.OfflineData("e1f3a7254cb845b09450f54937c16061")]
    public partial class OpenMobileMap
    {
        public OpenMobileMap()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
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
                MyMapView.Map = myMapPackage.Maps.First();
            }
            catch (Exception e)
            {
                await new MessageDialog2(e.ToString(), "Error").ShowAsync();
            }
        }

        /// <summary>
        /// This abstracts away platform & sample viewer-specific code for accessing local files.
        /// </summary>
        /// <returns>String that is the path to the file on disk.</returns>
        private static string GetMmpkPath()
        {
            return DataManager.GetDataFolder("e1f3a7254cb845b09450f54937c16061", "Yellowstone.mmpk");
        }
    }
}