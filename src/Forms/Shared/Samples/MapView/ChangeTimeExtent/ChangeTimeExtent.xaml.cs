// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using System;
using Xamarin.Forms;

namespace ArcGISRuntime.Samples.ChangeTimeExtent
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Change time extent",
        "MapView",
        "This sample demonstrates how to filter data in layers by applying a time extent to a MapView.",
        "Switch between the available options and observe how the data is filtered.")]
    public partial class ChangeTimeExtent : ContentPage
    {
        // Hold two map service URIs, one for use with an ArcGISMapImageLayer, the other for use with a FeatureLayer.
        private readonly Uri _mapServerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Hurricanes/MapServer");
        private readonly Uri _featureLayerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Earthquakes_Since1970/MapServer/0");

        public ChangeTimeExtent()
        {
            InitializeComponent();

            Title = "Change time extent";

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap and initial location.
            Map myMap = new Map(Basemap.CreateTopographic());

            // Assign the map to the MapView.
            MyMapView.Map = myMap;

            // Load the layers from the corresponding URIs.
            ArcGISMapImageLayer myImageryLayer = new ArcGISMapImageLayer(_mapServerUri);
            FeatureLayer myFeatureLayer = new FeatureLayer(_featureLayerUri);

            // Add the layers to the map.
            MyMapView.Map.OperationalLayers.Add(myImageryLayer);
            MyMapView.Map.OperationalLayers.Add(myFeatureLayer);
        }

        private void twoThousand_Clicked(object sender, EventArgs e)
        {
            // Hard-coded start value: August 4th, 2000.
            DateTime start = new DateTime(2000, 8, 4);

            // Hard-coded end value: September 4th, 2000.
            DateTime end = new DateTime(2000, 9, 4);

            // Set the time extent on the map with the hard-coded values.
            MyMapView.TimeExtent = new TimeExtent(start, end);
        }

        private void twoThousandFive_Clicked(object sender, EventArgs e)
        {
            // Hard-coded start value: January 1st, 2005.
            DateTime start = new DateTime(2005, 1, 1);

            // Hard-coded end value: December 31st, 2005.
            DateTime end = new DateTime(2005, 12, 31);

            // Set the time extent on the map with the hard-coded values.
            MyMapView.TimeExtent = new TimeExtent(start, end);
        }
    }
}