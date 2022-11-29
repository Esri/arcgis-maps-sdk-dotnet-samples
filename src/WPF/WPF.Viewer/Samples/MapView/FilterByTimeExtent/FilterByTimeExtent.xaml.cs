// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Toolkit.UI;
using System;
using System.Threading.Tasks;

namespace ArcGIS.WPF.Samples.FilterByTimeExtent
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Filter by time extent",
        category: "MapView",
        description: "The time slider provides controls that allow you to visualize temporal data by applying a specific time extent to a map view.",
        instructions: "Use the slider at the bottom of the map to customize the date range for which you want to view the data. The date for the hurricanes sample data ranges from September 1st, 2005 to December 31st, 2005. Once the start and end dates have been selected, the map view will update to only show the relevant data that falls in the time extent selected. Use the play button to step through the data one day at a time. Use the previous and next buttons to go back and forth in 2 day increments as demonstrated below.",
        tags: new[] { "animate", "data", "filter", "time", "time extent", "time frame", "toolkit" })]
    public partial class FilterByTimeExtent
    {
        // URL to the feature service.
        private const string FeatureServiceUrl = "https://services5.arcgis.com/N82JbI5EYtAkuUKU/ArcGIS/rest/services/Hurricane_time_enabled_layer_2005_1_day/FeatureServer/0";

        // Hold a reference to the feature table.
        private ServiceFeatureTable _featureTable;

        public FilterByTimeExtent()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create the map.
            MyMapView.Map = new Map(BasemapStyle.ArcGISTopographic);

            // Set the initial map location.
            MyMapView.SetViewpoint(new Viewpoint(29.979774, -58.495293, 150000000));

            // Create the feature table, referring to the feature service.
            _featureTable = new ServiceFeatureTable(new Uri(FeatureServiceUrl));

            // Create a feature layer to visualize the features in the table.
            FeatureLayer featureLayer = new FeatureLayer(_featureTable);

            // Add the layer to the map.
            MyMapView.Map.OperationalLayers.Add(featureLayer);

            // Initialize time slider properties, including full extent and time intervals, based on the feature layer.
            await MyTimeSlider.InitializeTimePropertiesAsync(featureLayer);
        }

        private void TimeSlider_CurrentExtentChanged(object sender, TimeExtentChangedEventArgs e)
        {
            // Update the time extent on the map view.
            MyMapView.TimeExtent = e.NewExtent;
        }
    }
}