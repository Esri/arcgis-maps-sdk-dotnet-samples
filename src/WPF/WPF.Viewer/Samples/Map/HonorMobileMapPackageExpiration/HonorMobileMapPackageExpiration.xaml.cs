// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.HonorMobileMapPackageExpiration
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Honor mobile map package expiration date",
        category: "Map",
        description: "Access the expiration information of an expired mobile map package.",
        instructions: "Load the sample. The author of the MMPK used in this sample chose to set the MMPK's map as still readable, even if it's expired. The sample presents expiration information to the user.",
        tags: new[] { "expiration", "mmpk" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("174150279af74a2ba6f8b87a567f480b")]
    public partial class HonorMobileMapPackageExpiration
    {
        public HonorMobileMapPackageExpiration()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                // Path to the mobile map package.
                string mobileMapPackagePath = DataManager.GetDataFolder("174150279af74a2ba6f8b87a567f480b", "LothianRiversAnno.mmpk");

                // Create a mobile map package.
                MobileMapPackage mobileMapPackage = new MobileMapPackage(mobileMapPackagePath);

                // Load the mobile map package.
                await mobileMapPackage.LoadAsync();

                // Check if the map package is expired.
                if (mobileMapPackage.Expiration?.IsExpired == true)
                {
                    // Get the expiration of the mobile map package.
                    Expiration expiration = mobileMapPackage.Expiration;

                    // Get the expiration message.
                    string expirationMessage = expiration.Message;

                    // Get the expiration date.
                    string expirationDate = expiration.DateTime.ToString("F");

                    // Set the expiration message.
                    ExpirationLabel.Text = $"{expirationMessage}\nExpiration date: {expirationDate}";

                    // Check if the map is accessible after expiration.
                    if (expiration.Type == ExpirationType.AllowExpiredAccess && mobileMapPackage.Maps.Count > 0)
                    {
                        // Set the mapview to the map from the mobile map package.
                        MyMapView.Map = mobileMapPackage.Maps[0];
                    }
                    else if (expiration.Type == ExpirationType.PreventExpiredAccess)
                    {
                        MessageBox.Show("The author of this mobile map package has disallowed access after the expiration date.", "Error");
                    }
                }
                else if (mobileMapPackage.Maps.Any())
                {
                    // Set the mapview to the map from the mobile map package.
                    MyMapView.Map = mobileMapPackage.Maps[0];
                }
                else
                {
                    MessageBox.Show("Failed to load the mobile map package.", "Error");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}