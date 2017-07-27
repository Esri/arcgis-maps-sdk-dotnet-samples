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

namespace ArcGISRuntime.WPF.Samples.OpenMobileMap
{
    public partial class OpenMobileMap
    {
        // Hold a reference to a Mobile Map Package
        private MobileMapPackage MyMapPackage;

        public OpenMobileMap()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private async void Initialize()
        {
            // Load the Mobile Map Package
            //     File is located in Resources/MobileMapPackages/Yellowstone.mmpk
            //     Build Action is Content; Copy if newer
            MyMapPackage = await MobileMapPackage.OpenAsync("Resources\\MobileMapPackages\\Yellowstone.mmpk");

            // Check that there is at least one map
            if (MyMapPackage.Maps.Count > 0)
            {
                // Display the first map in the package
                MyMapView.Map = MyMapPackage.Maps.First();
            }
        }
    }
}