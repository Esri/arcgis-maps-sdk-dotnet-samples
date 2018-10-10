// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
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
        // Hold references to the UI controls.
        private MapView _myMapView;
        private UIToolbar _topToolbar;
        private UIToolbar _bottomToolbar;
        private UILabel _redLabel;
        private UILabel _blueLabel;
        private UILabel _timeLabel;
        private UISlider _timeSlider;

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
            Initialize();
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

            // Listen for slider changes.
            _timeSlider.ValueChanged += TimeSlider_ValueChanged;
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
            _timeLabel.Text = $"{newStart:MMM d} - {newEnd:MMM d}";
        }

        public override void LoadView()
        {
            View = new UIView();
            View.BackgroundColor = UIColor.White;

            _topToolbar = new UIToolbar();
            _topToolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(_topToolbar);


            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(_myMapView);

            _bottomToolbar = new UIToolbar();
            _bottomToolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(_bottomToolbar);

            UIStackView legendView = new UIStackView();
            legendView.TranslatesAutoresizingMaskIntoConstraints = false;
            legendView.Axis = UILayoutConstraintAxis.Horizontal;
            legendView.Spacing = 8;
            _topToolbar.AddSubview(legendView);

            UIView redIcon = new UIView();
            redIcon.TranslatesAutoresizingMaskIntoConstraints = false;
            redIcon.BackgroundColor = UIColor.Red;
            redIcon.WidthAnchor.ConstraintEqualTo(16).Active = true;
            redIcon.HeightAnchor.ConstraintEqualTo(16).Active = true;
            redIcon.ClipsToBounds = true;
            redIcon.Layer.CornerRadius = 8;

            UIView blueIcon = new UIView();
            blueIcon.BackgroundColor = UIColor.Blue;
            blueIcon.TranslatesAutoresizingMaskIntoConstraints = false;
            blueIcon.WidthAnchor.ConstraintEqualTo(16).Active = true;
            blueIcon.HeightAnchor.ConstraintEqualTo(16).Active = true;
            blueIcon.ClipsToBounds = true;
            blueIcon.Layer.CornerRadius = 8;

            legendView.LeftAnchor.ConstraintEqualTo(_topToolbar.SafeAreaLayoutGuide.LeftAnchor, 8).Active = true;
            legendView.RightAnchor.ConstraintEqualTo(_topToolbar.SafeAreaLayoutGuide.RightAnchor, -8).Active = true;
            legendView.TopAnchor.ConstraintEqualTo(_topToolbar.SafeAreaLayoutGuide.TopAnchor, 8).Active = true;
            legendView.BottomAnchor.ConstraintEqualTo(_topToolbar.SafeAreaLayoutGuide.BottomAnchor, -8).Active = true;

            _redLabel = new UILabel
            {
                Text = "Offset 10 days",
                TextColor = UIColor.Red,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            _blueLabel = new UILabel
            {
                Text = "No offset",
                TextColor = UIColor.Blue,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            UIView spacer = new UIView();
            spacer.TranslatesAutoresizingMaskIntoConstraints = false;
            spacer.SetContentCompressionResistancePriority((float) UILayoutPriority.DefaultLow, UILayoutConstraintAxis.Horizontal);

            legendView.AddArrangedSubview(redIcon);
            legendView.AddArrangedSubview(_redLabel);
            legendView.AddArrangedSubview(spacer);
            legendView.AddArrangedSubview(blueIcon);
            legendView.AddArrangedSubview(_blueLabel);

            redIcon.CenterYAnchor.ConstraintEqualTo(blueIcon.CenterYAnchor).Active = true;
            blueIcon.CenterYAnchor.ConstraintEqualTo(_topToolbar.CenterYAnchor).Active = true;

            _timeLabel = new UILabel
            {
                TextColor = UIColor.Black,
                TextAlignment = UITextAlignment.Center,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            _timeLabel.WidthAnchor.ConstraintEqualTo(150).Active = true;

            _timeSlider = new UISlider
            {
                MinValue = 0,
                MaxValue = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            _timeSlider.WidthAnchor.ConstraintEqualTo(600).Active = true;

            _bottomToolbar.Items = new[]
            {
                new UIBarButtonItem(_timeLabel),
                new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace) {Width = 0},
                new UIBarButtonItem(_timeSlider)
            };

            _topToolbar.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _topToolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _topToolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;

            _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _myMapView.TopAnchor.ConstraintEqualTo(_topToolbar.BottomAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(_bottomToolbar.TopAnchor).Active = true;

            _bottomToolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _bottomToolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _bottomToolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor).Active = true;
        }
    }
}