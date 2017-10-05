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

namespace ArcGISRuntime.WPF.Samples.FeatureLayerTimeOffset
{
    public partial class FeatureLayerTimeOffset
    {
        // Hold the feature layer URI "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Hurricanes/MapServer/0"
        private Uri _featureLayerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Hurricanes/MapServer");

        // Hold a reference to the original time extent
        private TimeExtent originalExtent;

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

            // Add the hurricanes feature layer once
            ArcGISMapImageLayer noOffsetLayer = new ArcGISMapImageLayer(_featureLayerUri); // TODO - Change to FeatureLayer
            //noOffsetLayer.Renderer = new SimpleRenderer(new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Windows.Media.Color.FromRgb(255, 0, 0), 10));
            myMap.OperationalLayers.Add(noOffsetLayer);

            // Add the hurricanes feature layer again, now with 10 day offset
            ArcGISMapImageLayer withOffsetLayer = new ArcGISMapImageLayer(_featureLayerUri); // TODO - Change to FeatureLayer
            //withOffsetLayer.Renderer = new SimpleRenderer(new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Windows.Media.Color.FromRgb(0, 0, 255), 10));
            withOffsetLayer.TimeOffset = new TimeValue(1, Esri.ArcGISRuntime.ArcGISServices.TimeUnit.Years);
            myMap.OperationalLayers.Add(withOffsetLayer);

            // Apply the Map to the MapView
            MyMapView.Map = myMap;

            // Ensure the no offset layer is loaded
            await noOffsetLayer.LoadAsync();

            // Store a reference to the original time extent
            originalExtent = noOffsetLayer.FullTimeExtent;
        }

        private void MyTimeSlider_ValueChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            // Get the value of the slider
            double value = e.NewValue;

            // Calculate the number of days that value corresponds to
            // 1. Get the interval
            TimeSpan interval = originalExtent.EndTime - originalExtent.StartTime;
            // 2. Store the interval as days
            double days = interval.TotalDays;
            // 3. Scale the interval by the value from the slider
            double desiredInterval = value * days;
            // 4. Create a new TimeSpan
            TimeSpan newOffset = new TimeSpan((int)desiredInterval, 0, 0, 0);

            // Determine the new starting offset
            DateTime newStart = originalExtent.StartTime.DateTime.Add(newOffset);

            // Determine the new ending offset
            DateTime newEnd = newStart.AddDays(10);

            // Reset the new DateTimeOffset if it is outside of the extent
            if (newEnd > originalExtent.EndTime)
            {
                newEnd = originalExtent.EndTime.DateTime;
            }

            // Do nothing if out of bounds
            if (newEnd < newStart) { return; }

            // Apply the new extent
            MyMapView.TimeExtent = new TimeExtent(newStart, newEnd);

            // Update the label
            lblCurrentDate.Content = String.Format("{0} - {1}", newStart.ToShortDateString(), newEnd.ToShortDateString());
        }
    }
}
