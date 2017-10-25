// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.SetInitialMapLocation
{
    public partial class SetInitialMapLocation : ContentPage
    {
        public SetInitialMapLocation()
        {
            InitializeComponent ();

            Title = "Set initial map location";

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            // Create a map with 'Imagery with Labels' basemap and an initial location
            Map myMap = new Map(BasemapType.ImageryWithLabels, -33.867886, -63.985, 16);

            // Assign the map to the MapView
            MyMapView.Map = myMap;
        }
    }
}
