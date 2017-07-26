// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using System.Linq;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.OpenMobileMap
{
    public partial class OpenMobileMap : ContentPage
    {
        // Hold a reference to a Mobile Map Package
        private MobileMapPackage MyMapPackage;

        public OpenMobileMap()
        {
            InitializeComponent ();

            Title = "Open mobile map (map package)";

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private async void Initialize()
        {
            // Choose the appropriate file path for the MMPK based on platform
            string filepath;
            switch(Device.RuntimePlatform)
            {
                case Device.Windows:
                    filepath = "Resources\\MobileMapPackages\\Yellowstone.mmpk";
                    break;
                case Device.iOS:
                case Device.Android:
                default:
                    filepath = "MobileMapPackages/Yellowstone.mmpk";
                    break;
            }
            // Load the Mobile Map Package from the Bundle
            //     File is located in Resources/MobileMapPackages/Yellowstone.mmpk
            //     Build Action is Content; Do not copy to Output Directory
            MyMapPackage = await MobileMapPackage.OpenAsync(filepath);

            // Display the first map in the package
            MyMapView.Map = MyMapPackage.Maps.First();
        }
    }
}
