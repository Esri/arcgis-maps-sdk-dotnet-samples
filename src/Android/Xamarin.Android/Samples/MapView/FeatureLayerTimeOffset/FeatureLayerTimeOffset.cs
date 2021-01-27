// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI.Controls;
using System;

namespace ArcGISRuntime.Samples.FeatureLayerTimeOffset
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Feature layer time offset",
        category: "MapView",
        description: "Display a time-enabled feature layer with a time offset.",
        instructions: "When the sample loads, you'll see hurricane tracks visualized in red and blue. The red hurricane tracks occurred 10 days before the tracks displayed in blue. Adjust the slider to move the interval to visualize how storms progress over time.",
        tags: new[] { "change", "range", "time", "time extent", "time offset", "time-aware", "time-enabled" })]
    public class FeatureLayerTimeOffset : Activity
    {
        // Hold a reference to the map view
        private MapView _myMapView;

        // Hold references to the UI controls
        private TextView _redLabel;

        private TextView _blueLabel;
        private TextView _timeLabel;
        private SeekBar _timeSlider;

        private Uri _featureLayerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Hurricanes/MapServer/0");

        // Hold a reference to the original time extent
        private TimeExtent _originalExtent;

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
            Map myMap = new Map(BasemapStyle.ArcGISOceans);

            // Create the hurricanes feature layer once
            FeatureLayer noOffsetLayer = new FeatureLayer(_featureLayerUri);

            // Apply a blue dot renderer to distinguish hurricanes without offsets
            SimpleMarkerSymbol blueDot = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Blue, 10);
            noOffsetLayer.Renderer = new SimpleRenderer(blueDot);

            // Add the non-offset layer to the map
            myMap.OperationalLayers.Add(noOffsetLayer);

            // Create the offset hurricanes feature layer
            FeatureLayer withOffsetLayer = new FeatureLayer(_featureLayerUri);

            // Center the Viewpoint on the FeatureLayer once the feature layer has loaded.
            withOffsetLayer.Loaded += (s, e) => { _myMapView.SetViewpointGeometryAsync(withOffsetLayer.FullExtent, 50); };

            // Apply a red dot renderer to distinguish these hurricanes from the non-offset hurricanes
            SimpleMarkerSymbol redDot = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Red, 10);
            withOffsetLayer.Renderer = new SimpleRenderer(redDot);

            // Apply the time offset (red hurricane dots will be from 10 days before the current extent)
            withOffsetLayer.TimeOffset = new TimeValue(10, Esri.ArcGISRuntime.ArcGISServices.TimeUnit.Days);

            // Add the layer to the map
            myMap.OperationalLayers.Add(withOffsetLayer);

            // Apply the Map to the MapView
            _myMapView.Map = myMap;

            try
            {
                // Ensure the no offset layer is loaded
                await noOffsetLayer.LoadAsync();

                // Store a reference to the original time extent
                _originalExtent = noOffsetLayer.FullTimeExtent;

                // Update the time extent set on the map
                UpdateTimeExtent();
            }
            catch (Exception e)
            {
                new AlertDialog.Builder(this).SetMessage(e.ToString()).SetTitle("Error").Show();
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create the UI controls
            _redLabel = new TextView(this) { Text = "Red hurricanes offset 10 days", Gravity = GravityFlags.CenterHorizontal };
            _redLabel.SetTextColor(Android.Graphics.Color.Red);
            _blueLabel = new TextView(this) { Text = "Blue hurricanes not offset", Gravity = GravityFlags.CenterHorizontal };
            _blueLabel.SetTextColor(Android.Graphics.Color.Blue);
            _timeLabel = new TextView(this) { Text = "", Gravity = GravityFlags.CenterHorizontal };
            _timeSlider = new SeekBar(this) { Max = 100 };

            // Add the controls to the layout
            layout.AddView(_redLabel);
            layout.AddView(_blueLabel);
            layout.AddView(_timeLabel);
            layout.AddView(_timeSlider);

            // Add the map view to the layout
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);

            // Subscribe to slider value change notifications
            _timeSlider.ProgressChanged += _timeSlider_ProgressChanged;
        }

        private void _timeSlider_ProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            UpdateTimeExtent();
        }

        private void UpdateTimeExtent()
        {
            // Get the value of the slider
            double value = (double)_timeSlider.Progress / 100.0;

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
            _myMapView.TimeExtent = new TimeExtent(newStart, newEnd);

            // Update the label
            _timeLabel.Text = $"{newStart:d} - {newEnd:d}";
        }
    }
}