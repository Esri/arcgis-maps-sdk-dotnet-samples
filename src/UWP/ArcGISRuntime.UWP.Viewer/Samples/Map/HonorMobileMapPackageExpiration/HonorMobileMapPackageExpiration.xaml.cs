﻿// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using System;
using ArcGISRuntime.Samples.Managers;
using Windows.UI.Popups;
using System.Linq;

namespace ArcGISRuntime.UWP.Samples.HonorMobileMapPackageExpiration
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Honor mobile map package expiration date",
        "Map",
        "Access the expiration information of an expired mobile map package.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("174150279af74a2ba6f8b87a567f480b")]
    public partial class HonorMobileMapPackageExpiration
    {
        public HonorMobileMapPackageExpiration()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
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
                        await new MessageDialog("The author of this mobile map package has disallowed access after the expiration date.", "Error").ShowAsync();
                    }
                }
                else if (mobileMapPackage.Maps.Any())
                {
                    // Set the mapview to the map from the mobile map package.
                    MyMapView.Map = mobileMapPackage.Maps[0];
                }
                else
                {
                    await new MessageDialog("Failed to load the mobile map package.", "Error").ShowAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
