// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using System;

namespace ArcGISRuntime.UWP.Samples.FeatureLayerTimeOffset
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Feature layer time offset",
        "MapView",
        "This sample demonstrates how to show data from the same service side-by-side with a time offset. This allows for the comparison of data over time.",
        "")]
    public sealed partial class FeatureLayerTimeOffset
    {
        // Hold the feature layer URI
        private Uri _featureLayerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Hurricanes/MapServer/0");

        // Hold a reference to the original time extent
        private TimeExtent _originalExtent;

        public FeatureLayerTimeOffset()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private async void Initialize()
        {
            // Create new Map
            Map myMap = new Map(Basemap.CreateOceans());

            // Create the hurricanes feature layer once
            FeatureLayer noOffsetLayer = new FeatureLayer(_featureLayerUri);

            // Apply a blue dot renderer to distinguish hurricanes without offsets
            SimpleMarkerSymbol blueDot = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Windows.UI.Colors.Blue, 10);
            noOffsetLayer.Renderer = new SimpleRenderer(blueDot);

            // Add the non-offset layer to the map
            myMap.OperationalLayers.Add(noOffsetLayer);

            // Create the offset hurricanes feature layer
            FeatureLayer withOffsetLayer = new FeatureLayer(_featureLayerUri);

            // Apply a red dot renderer to distinguish these hurricanes from the non-offset hurricanes
            SimpleMarkerSymbol redDot = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Windows.UI.Colors.Red, 10);
            withOffsetLayer.Renderer = new SimpleRenderer(redDot);

            // Apply the time offset (red hurricane dots will be from 10 days before the current extent)
            withOffsetLayer.TimeOffset = new TimeValue(10, Esri.ArcGISRuntime.ArcGISServices.TimeUnit.Days);

            // Add the layer to the map
            myMap.OperationalLayers.Add(withOffsetLayer);

            // Apply the Map to the MapView
            MyMapView.Map = myMap;

            // Ensure the no offset layer is loaded
            await noOffsetLayer.LoadAsync();

            // Store a reference to the original time extent
            _originalExtent = noOffsetLayer.FullTimeExtent;

            // Update the time extent set on the map
            UpdateTimeExtent();
        }

        private void MyTimeSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            UpdateTimeExtent();
        }

        private void UpdateTimeExtent()
        {
            // Get the value of the slider
            double value = MyTimeSlider.Value / 100;

            // Calculate the number of days that value corresponds to
            // 1. Get the interval
            TimeSpan interval = _originalExtent.EndTime - _originalExtent.StartTime;

            // 2. Store the interval as days
            double days = interval.TotalDays;

            // 3. Scale the interval by the value from the slider
            double desiredInterval = value * days;

            // 4. Create a new TimeSpan
            TimeSpan newOffset = new TimeSpan((int)desiredInterval, 0, 0, 0);

            // Determine the new starting offset
            DateTime newStart = _originalExtent.StartTime.DateTime.Add(newOffset);

            // Determine the new ending offset
            DateTime newEnd = newStart.AddDays(10);

            // Reset the new DateTimeOffset if it is outside of the extent
            if (newEnd > _originalExtent.EndTime)
            {
                newEnd = _originalExtent.EndTime.DateTime;
            }

            // Do nothing if out of bounds
            if (newEnd < newStart) { return; }

            // Apply the new extent
            MyMapView.TimeExtent = new TimeExtent(newStart, newEnd);

            // Update the label
            lblCurrentDate.Text = String.Format("{0} - {1}", newStart.ToString("d"), newEnd.ToString("d"));
        }
    }
}