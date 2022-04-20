// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.UI.Controls;
using System.Threading.Tasks;
using Xamarin.Forms;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using System;
using System.Diagnostics;

namespace ArcGISRuntimeXamarin.Samples.ShowDeviceLocationUsingIndoorPositioning
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Show device location using indoor positioning",
        "Map",
        "Show your device's real-time location while inside a building by using signals from indoor positioning beacons.",
        "")]
    public partial class ShowDeviceLocationUsingIndoorPositioning : ContentPage
    {
        public ShowDeviceLocationUsingIndoorPositioning()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            Uri uri = new Uri("https://viennardc.maps.arcgis.com");

            // Handle the login to the feature service.
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(async (info) =>
            {
                try
                {
                    // WARNING: Never hardcode login information in a production application. This is done solely for the sake of the sample.
                    string sampleUser = "tester_viennardc";
                    string samplePass = "password.testing12345";
                    return await AuthenticationManager.Current.GenerateCredentialAsync(uri, sampleUser, samplePass);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return null;
                }
            });

            MyMapView.Map = new Map(uri);

        }
    }
}
