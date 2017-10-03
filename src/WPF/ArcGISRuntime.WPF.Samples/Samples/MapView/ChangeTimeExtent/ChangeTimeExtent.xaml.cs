// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Portal;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace ArcGISRuntime.WPF.Samples.ChangeTimeExtent
{
    public partial class ChangeTimeExtent
    {
        public ChangeTimeExtent()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private async void Initialize()
        {
            // Create new Map with basemap and initial location
            Map myMap = new Map(Basemap.CreateTopographic());

            // Assign the map to the MapView
            MyMapView.Map = myMap;
            ArcGISPortal portal = await ArcGISPortal.CreateAsync();

            PortalItem NationalWeatherModel = await PortalItem.CreateAsync(portal, "b9ec31770ee643509b942dfdec393b7f");

            ArcGISMapImageLayer layer = new ArcGISMapImageLayer(NationalWeatherModel);

            MyMapView.Map.OperationalLayers.Add(layer);
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DateTime Now = DateTime.Now;
            MyMapView.TimeExtent = new Esri.ArcGISRuntime.TimeExtent(new DateTimeOffset(Now));
        }

        private void Button_Click_1(object sender, System.Windows.RoutedEventArgs e)
        {
            MyMapView.TimeExtent = new Esri.ArcGISRuntime.TimeExtent(new DateTimeOffset(DateTime.Now.AddHours(12)));
        }
    }
}