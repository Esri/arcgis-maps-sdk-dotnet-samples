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
using System.Windows;

namespace ArcGIS.WPF.Samples.ChangeTimeExtent
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Change time extent",
        category: "MapView",
        description: "Filter data in layers by applying a time extent to a MapView.",
        instructions: "Switch between the available options and observe how the data is filtered.",
        tags: new[] { "data", "filter", "time", "time frame", "time span" })]
    public partial class ChangeTimeExtent
    {
        // Hold two map service URIs, one for use with an ArcGISMapImageLayer, the other for use with a FeatureLayer.
        private readonly Uri _mapServerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Hurricanes/MapServer");
        private readonly Uri _featureLayerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Earthquakes_Since1970/MapServer/0");

        public ChangeTimeExtent()
        {
            InitializeComponent();

            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap and initial location.
            Map map = new Map(BasemapStyle.ArcGISTopographic);

            // Assign the map to the MapView.
            MyMapView.Map = map;

            // Load the layers from the corresponding URIs.
            ArcGISMapImageLayer imageryLayer = new ArcGISMapImageLayer(_mapServerUri);
            FeatureLayer pointLayer = new FeatureLayer(_featureLayerUri);

            // Add the layers to the map.
            MyMapView.Map.OperationalLayers.Add(imageryLayer);
            MyMapView.Map.OperationalLayers.Add(pointLayer);
        }

        private void twoThousand_Click(object sender, RoutedEventArgs e)
        {
            // Hard-coded start value: January 1st, 2000.
            DateTime start = new DateTime(2000, 1, 1);

            // Hard-coded end value: December 31st, 2000.
            DateTime end = new DateTime(2000, 12, 31);

            // Set the time extent on the map with the hard-coded values.
            MyMapView.TimeExtent = new TimeExtent(start, end);
        }

        private void twoThousandFive_Click(object sender, RoutedEventArgs e)
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