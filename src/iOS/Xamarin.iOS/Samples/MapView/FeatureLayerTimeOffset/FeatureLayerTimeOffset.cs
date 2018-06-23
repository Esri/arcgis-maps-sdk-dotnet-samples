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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using UIKit;

namespace ArcGISRuntime.Samples.FeatureLayerTimeOffset
{
    [Register("FeatureLayerTimeOffset")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Feature layer time offset",
        "MapView",
        "This sample demonstrates how to show data from the same service side-by-side with a time offset. This allows for the comparison of data over time.",
        "")]
    public class FeatureLayerTimeOffset : UIViewController
    {
        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UIStackView _stackView = new UIStackView();
        private readonly UIToolbar _topToolbar = new UIToolbar();
        private readonly UIToolbar _bottomToolbar = new UIToolbar();

        private readonly UILabel _redLabel = new UILabel
        {
            Text = "Red hurricanes offset 10 days",
            TextColor = UIColor.Red,
            TextAlignment = UITextAlignment.Center
        };

        private readonly UILabel _blueLabel = new UILabel
        {
            Text = "Blue hurricanes not offset",
            TextColor = UIColor.Blue,
            TextAlignment = UITextAlignment.Center
        };

        private readonly UILabel _timeLabel = new UILabel
        {
            TextColor = UIColor.Black,
            TextAlignment = UITextAlignment.Center
        };

        private readonly UISlider _timeSlider = new UISlider
        {
            MinValue = 0,
            MaxValue = 1
        };

        // Hold the feature layer URI.
        private readonly Uri _featureLayerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Hurricanes/MapServer/0");

        // Hold a reference to the original time extent.
        private TimeExtent _originalExtent;

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
            try
            {
                nfloat topHeight = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat controlHeight = 30;
                nfloat margin = 5;
                nfloat sliderMargin = 50;

                // Reposition the views.
                _topToolbar.Frame = new CoreGraphics.CGRect(0, topHeight, View.Bounds.Width, controlHeight * 2 + margin * 3);
                _bottomToolbar.Frame = new CoreGraphics.CGRect(0, View.Bounds.Height - 2 * controlHeight - 3 * margin, View.Bounds.Width, 2 * controlHeight + 3 * margin);
                _redLabel.Frame = new CoreGraphics.CGRect(margin, topHeight + margin, View.Bounds.Width - 2 * margin, controlHeight);
                _blueLabel.Frame = new CoreGraphics.CGRect(margin, topHeight + 2 * margin + controlHeight, View.Bounds.Width - 2 * margin, controlHeight);
                _timeLabel.Frame = new CoreGraphics.CGRect(margin, View.Bounds.Height - 2 * controlHeight - 2 * margin, View.Bounds.Width - 2 * margin, controlHeight);
                _timeSlider.Frame = new CoreGraphics.CGRect(sliderMargin, View.Bounds.Height - controlHeight - margin, View.Bounds.Width - 2 * sliderMargin, controlHeight);
                _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topHeight + _topToolbar.Frame.Height, 0, _bottomToolbar.Frame.Height, 0);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private async void Initialize()
        {
            // Create new Map with oceans basemap.
            Map myMap = new Map(Basemap.CreateOceans());

            // Create the hurricanes feature layer once.
            FeatureLayer noOffsetLayer = new FeatureLayer(_featureLayerUri);

            // Apply a blue dot renderer to distinguish hurricanes without offsets.
            SimpleMarkerSymbol blueDot = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Blue, 10);
            noOffsetLayer.Renderer = new SimpleRenderer(blueDot);

            // Add the non-offset layer to the map.
            myMap.OperationalLayers.Add(noOffsetLayer);

            // Create the offset hurricanes feature layer.
            FeatureLayer withOffsetLayer = new FeatureLayer(_featureLayerUri);

            // Apply a red dot renderer to distinguish these hurricanes from the non-offset hurricanes.
            SimpleMarkerSymbol redDot = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Red, 10);
            withOffsetLayer.Renderer = new SimpleRenderer(redDot);

            // Apply the time offset (red hurricane dots will be from 10 days before the current extent).
            withOffsetLayer.TimeOffset = new TimeValue(10, Esri.ArcGISRuntime.ArcGISServices.TimeUnit.Days);

            // Add the layer to the map.
            myMap.OperationalLayers.Add(withOffsetLayer);

            // Apply the Map to the MapView.
            _myMapView.Map = myMap;

            // Ensure the no offset layer is loaded.
            await noOffsetLayer.LoadAsync();

            // Store a reference to the original time extent.
            _originalExtent = noOffsetLayer.FullTimeExtent;

            // Update the time extent set on the map.
            UpdateTimeExtent();
        }

        private void CreateLayout()
        {
            // Add the views to the top UIStackView.
            _stackView.AddArrangedSubview(_redLabel);
            _stackView.AddArrangedSubview(_blueLabel);
            _stackView.AddArrangedSubview(_timeLabel);
            _stackView.AddArrangedSubview(_timeSlider);

            // Subscribe to slider value change notifications.
            _timeSlider.ValueChanged += TimeSlider_ValueChanged;

            // Add MapView to the page.
            View.AddSubviews(_myMapView, _topToolbar, _bottomToolbar, _redLabel, _blueLabel, _timeLabel, _timeSlider);
        }

        private void TimeSlider_ValueChanged(object sender, EventArgs e)
        {
            UpdateTimeExtent();
        }

        private void UpdateTimeExtent()
        {
            // Get the value of the slider.
            double value = _timeSlider.Value;

            // Calculate the number of days that value corresponds to.
            // 1. Get the interval.
            TimeSpan interval = _originalExtent.EndTime - _originalExtent.StartTime;

            // 2. Store the interval as days.
            double days = interval.TotalDays;

            // 3. Scale the interval by the value from the slider.
            double desiredInterval = value * days;

            // 4. Create a new TimeSpan.
            TimeSpan newOffset = new TimeSpan((int) desiredInterval, 0, 0, 0);

            // Determine the new starting offset.
            DateTime newStart = _originalExtent.StartTime.DateTime.Add(newOffset);

            // Determine the new ending offset.
            DateTime newEnd = newStart.AddDays(10);

            // Reset the new DateTimeOffset if it is outside of the extent.
            if (newEnd > _originalExtent.EndTime)
            {
                newEnd = _originalExtent.EndTime.DateTime;
            }

            // Do nothing if out of bounds.
            if (newEnd < newStart)
            {
                return;
            }

            // Apply the new extent.
            _myMapView.TimeExtent = new TimeExtent(newStart, newEnd);

            // Update the label.
            _timeLabel.Text = String.Format("{0} - {1}", newStart.ToString("d"), newEnd.ToString("d"));
        }
    }
}