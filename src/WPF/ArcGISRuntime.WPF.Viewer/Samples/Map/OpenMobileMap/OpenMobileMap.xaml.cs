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
using System.Windows;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;

namespace ArcGISRuntime.WPF.Samples.OpenMobileMap
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Open mobile map (map package)",
        "Map",
        "This sample demonstrates how to open a mobile map from a map package.",
        "The map package will be downloaded from an ArcGIS Online portal automatically.")]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData("e1f3a7254cb845b09450f54937c16061")]
    public partial class OpenMobileMap
    {
        public OpenMobileMap()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            // Get the path to the mobile map package.
            string filepath = GetMmpkPath();

            try
            {
                // Load directly or unpack then load as needed by the map package.
                if (await MobileMapPackage.IsDirectReadSupportedAsync(filepath))
                {
                    // Open the map package.
                    MobileMapPackage myMapPackage = await MobileMapPackage.OpenAsync(filepath);

                    // Display the first map in the package.
                    MyMapView.Map = myMapPackage.Maps.First();
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
                    MyMapView.Map = package.Maps.First();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
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