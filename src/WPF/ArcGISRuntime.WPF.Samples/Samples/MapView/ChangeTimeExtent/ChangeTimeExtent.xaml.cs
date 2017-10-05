// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime;
using System;

namespace ArcGISRuntime.WPF.Samples.ChangeTimeExtent
{
    public partial class ChangeTimeExtent
    {
        // Hold two map service URIs, one for use with a ArcGISMapImageLayer, the other for use with a FeatureLayer
        private Uri _mapServerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Hurricanes/MapServer");

        private Uri _featureLayerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Hurricanes/MapServer/1");

        public ChangeTimeExtent()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap and initial location
            Map myMap = new Map(Basemap.CreateTopographic());

            // Assign the map to the MapView
            MyMapView.Map = myMap;

            // Load the layers from the corresponding URIs
            ArcGISMapImageLayer myImageryLayer = new ArcGISMapImageLayer(_mapServerUri);
            FeatureLayer myFeatureLayer = new FeatureLayer(_featureLayerUri);

            // Add the layers to the map
            MyMapView.Map.OperationalLayers.Add(myImageryLayer);
            MyMapView.Map.OperationalLayers.Add(myFeatureLayer);
        }

        private void Button_Click_1(object sender, System.Windows.RoutedEventArgs e)
        {
            // Hard-coded start value: August 4th, 2000
            DateTime start = new DateTime(2000, 8, 4);

            // Hard-coded end value: September 4th, 2000
            DateTime end = new DateTime(2000, 9, 4);

            // Set the time extent on the map with the hard-coded values
            MyMapView.TimeExtent = new TimeExtent(new DateTimeOffset(start), new DateTimeOffset(end));
        }

        private void Button_Click_2(object sender, System.Windows.RoutedEventArgs e)
        {
            // Hard-coded start value: September 22nd, 2000
            DateTime start = new DateTime(2000, 9, 22);

            // Hard-coded end value: October 22nd, 2000
            DateTime end = new DateTime(2000, 10, 22);

            // Set the time extent on the map with the hard-coded values
            MyMapView.TimeExtent = new TimeExtent(start, end);
        }
    }
}