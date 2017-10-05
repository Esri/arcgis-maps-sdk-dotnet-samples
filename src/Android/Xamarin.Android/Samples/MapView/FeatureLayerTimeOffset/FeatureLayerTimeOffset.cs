// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;

namespace ArcGISRuntimeXamarin.Samples.FeatureLayerTimeOffset
{
    [Activity]
    public class FeatureLayerTimeOffset : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Hold references to the UI controls
        TextView _redLabel;
        TextView _blueLabel;
        TextView _timeLabel;
        SeekBar _timeSlider;

        // Hold the feature layer URI "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Hurricanes/MapServer/0"
        private Uri _featureLayerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Hurricanes/MapServer");

        // Hold a reference to the original time extent
        private TimeExtent originalExtent;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Feature layer time offset";

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
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
            _myMapView.Map = myMap;

            // Ensure the no offset layer is loaded
            await noOffsetLayer.LoadAsync();

            // Store a reference to the original time extent
            originalExtent = noOffsetLayer.FullTimeExtent;
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create the UI controls
            _redLabel = new TextView(this) { Text = "Red hurricanes offset 1 Year" };
            _redLabel.SetTextColor(Android.Graphics.Color.Red);
            _blueLabel = new TextView(this) { Text = "Blue hurricanes not offset" };
            _blueLabel.SetTextColor(Android.Graphics.Color.Blue);
            _timeLabel = new TextView(this) { Text = "" };
            _timeSlider = new SeekBar(this) { Max = 100 };

            // Add the controls to the layout
            layout.AddView(_redLabel);
            layout.AddView(_blueLabel);
            layout.AddView(_timeLabel);
            layout.AddView(_timeSlider);

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);

            // Subscribe to slider value change notifications
            _timeSlider.ProgressChanged += _timeSlider_ProgressChanged;
        }

        void _timeSlider_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            // Get the value of the slider
            double value = (double)e.Progress / 100;

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
            _myMapView.TimeExtent = new TimeExtent(newStart, newEnd);

            // Update the label
            _timeLabel.Text = String.Format("{0} - {1}", newStart.ToString("d"), newEnd.ToString("d"));
        }
    }
}