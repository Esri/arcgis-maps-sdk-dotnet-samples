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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.FeatureLayerTimeOffset
{
    [Register("FeatureLayerTimeOffset")]
    public class FeatureLayerTimeOffset : UIViewController
    {
        // Create and hold reference to the UI controls
        private MapView _myMapView = new MapView();
        private UILabel _redLabel = new UILabel() { Text = "Red hurricanes offset 1 Year", TextColor = UIColor.Red };
        private UILabel _blueLabel = new UILabel() { Text = "Blue hurricanes not offset", TextColor = UIColor.Blue };
        private UILabel _timeLabel = new UILabel() { TextColor = UIColor.Black };
        private UISlider _timeSlider = new UISlider() { MinValue = 0, MaxValue = 1 };
        private UIStackView _stackView = new UIStackView();

        // Hold the feature layer URI "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Hurricanes/MapServer/0"
        private Uri _featureLayerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Hurricanes/MapServer");

        // Hold a reference to the original time extent
        private TimeExtent originalExtent;

        public FeatureLayerTimeOffset()
        {
            Title = "Feature layer time offset";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            nfloat topHeight = NavigationController.TopLayoutGuide.Length + 60;

            // Setup the visual frames for the MapView and StackView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
            _stackView.Frame = new CoreGraphics.CGRect(0, topHeight, View.Bounds.Width, 150);

            base.ViewDidLayoutSubviews();
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
            nfloat topHeight = NavigationController.TopLayoutGuide.Length + 60;

            //set up UIStackView for laying out controls
            _stackView = new UIStackView(new CoreGraphics.CGRect(0, topHeight, View.Bounds.Width, 150));
            _stackView.Axis = UILayoutConstraintAxis.Vertical;
            _stackView.Alignment = UIStackViewAlignment.Fill;
            _stackView.Distribution = UIStackViewDistribution.FillProportionally;
            _stackView.BackgroundColor = UIColor.Gray;

            // Add the views to the UIStackView
            _stackView.AddArrangedSubview(_redLabel);
            _stackView.AddArrangedSubview(_blueLabel);
            _stackView.AddArrangedSubview(_timeLabel);
            _stackView.AddArrangedSubview(_timeSlider);

            // Add MapView to the page
            View.AddSubviews(_myMapView);

            // Add UIStackView to the page
            View.AddSubviews(_stackView);

            // Subscribe to slider value change notifications
            _timeSlider.ValueChanged += _timeSlider_ValueChanged;
        }

        private void _timeSlider_ValueChanged(object sender, EventArgs e)
        {
            // Get the value of the slider
            double value = _timeSlider.Value;

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