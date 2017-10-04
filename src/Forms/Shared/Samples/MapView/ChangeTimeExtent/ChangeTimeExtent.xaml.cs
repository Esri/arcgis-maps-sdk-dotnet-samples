// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.ChangeTimeExtent
{
    public partial class ChangeTimeExtent : ContentPage
    {
        public ChangeTimeExtent()
        {
            InitializeComponent ();

            Title = "Change time extent";

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private void Button1_Clicked(object sender, EventArgs e)
        {

        }

        private void Button2_Clicked(object sender, EventArgs e)
        {

        }

        private void Initialize()
        {
            // Create new Map with basemap and initial location
            Map myMap = new Map(Basemap.CreateTopographic());

            // Assign the map to the MapView
            MyMapView.Map = myMap;
        }
    }
}
