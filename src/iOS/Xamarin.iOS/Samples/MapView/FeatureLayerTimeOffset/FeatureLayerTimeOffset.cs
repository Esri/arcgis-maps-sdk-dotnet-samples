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
        name: "Feature layer time offset",
        category: "MapView",
        description: "Display a time-enabled feature layer with a time offset.",
        instructions: "When the sample loads, you'll see hurricane tracks visualized in red and blue. The red hurricane tracks occurred 10 days before the tracks displayed in blue. Adjust the slider to move the interval to visualize how storms progress over time.",
        tags: new[] { "change", "range", "time", "time extent", "time offset", "time-aware", "time-enabled" })]
    public class FeatureLayerTimeOffset : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
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

        private async void Initialize()
        {
            // Create new Map with oceans basemap.
            Map myMap = new Map(BasemapStyle.ArcGISOceans);

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

            try
            {
                // Ensure the no offset layer is loaded.
                await noOffsetLayer.LoadAsync();

                // Store a reference to the original time extent.
                _originalExtent = noOffsetLayer.FullTimeExtent;

                // Update the time extent set on the map.
                UpdateTimeExtent();
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate)null, "OK", null).Show();
            }
        }

        private void TimeSlider_ValueChanged(object sender, EventArgs e)
        {
            UpdateTimeExtent();
        }

        private void UpdateTimeExtent()
        {
            // Check that the original extent has loaded.
            if (_originalExtent == null) return;

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
            TimeSpan newOffset = new TimeSpan((int)desiredInterval, 0, 0, 0);

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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView { BackgroundColor = ApplicationTheme.BackgroundColor };

            UIToolbar topToolbar = new UIToolbar { TranslatesAutoresizingMaskIntoConstraints = false };

            _myMapView = new MapView { TranslatesAutoresizingMaskIntoConstraints = false };

            _timeLabel = new UILabel();

            _timeSlider = new UISlider
            {
                MinValue = 0,
                MaxValue = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            UIToolbar bottomToolbar = new UIToolbar();
            bottomToolbar.TranslatesAutoresizingMaskIntoConstraints = false;

            bottomToolbar.Items = new[]
            {
                new UIBarButtonItem { CustomView = _timeSlider, Width = 100 },
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem { CustomView = _timeLabel},
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
            };

            UIStackView legendView = new UIStackView();
            legendView.TranslatesAutoresizingMaskIntoConstraints = false;
            legendView.Axis = UILayoutConstraintAxis.Horizontal;
            legendView.Spacing = 8;

            UIView redIcon = new UIView();
            redIcon.TranslatesAutoresizingMaskIntoConstraints = false;
            redIcon.BackgroundColor = UIColor.Red;
            redIcon.WidthAnchor.ConstraintEqualTo(16).Active = true;
            redIcon.HeightAnchor.ConstraintEqualTo(16).Active = true;
            redIcon.ClipsToBounds = true;
            redIcon.Layer.CornerRadius = 8;
            legendView.AddArrangedSubview(redIcon);

            UILabel _redLabel = new UILabel
            {
                Text = "Offset 10 days",
                TextColor = UIColor.Red,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            legendView.AddArrangedSubview(_redLabel);

            UIView spacer = new UIView();
            spacer.TranslatesAutoresizingMaskIntoConstraints = false;
            spacer.SetContentCompressionResistancePriority((float)UILayoutPriority.DefaultLow, UILayoutConstraintAxis.Horizontal);
            legendView.AddArrangedSubview(spacer);

            UIView blueIcon = new UIView();
            blueIcon.BackgroundColor = UIColor.Blue;
            blueIcon.TranslatesAutoresizingMaskIntoConstraints = false;
            blueIcon.WidthAnchor.ConstraintEqualTo(16).Active = true;
            blueIcon.HeightAnchor.ConstraintEqualTo(16).Active = true;
            blueIcon.ClipsToBounds = true;
            blueIcon.Layer.CornerRadius = 8;
            legendView.AddArrangedSubview(blueIcon);

            UILabel _blueLabel = new UILabel
            {
                Text = "No offset",
                TextColor = UIColor.Blue,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            legendView.AddArrangedSubview(_blueLabel);

            // Add the views.
            View.AddSubviews(topToolbar, _myMapView, bottomToolbar);
            topToolbar.AddSubview(legendView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                topToolbar.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                topToolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                topToolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.TopAnchor.ConstraintEqualTo(topToolbar.BottomAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(bottomToolbar.TopAnchor),

                bottomToolbar.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor),
                bottomToolbar.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor),
                bottomToolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),

                legendView.LeftAnchor.ConstraintEqualTo(topToolbar.SafeAreaLayoutGuide.LeftAnchor, 8),
                legendView.RightAnchor.ConstraintEqualTo(topToolbar.SafeAreaLayoutGuide.RightAnchor, -8),
                legendView.TopAnchor.ConstraintEqualTo(topToolbar.SafeAreaLayoutGuide.TopAnchor, 8),
                legendView.BottomAnchor.ConstraintEqualTo(topToolbar.SafeAreaLayoutGuide.BottomAnchor, -8),

                redIcon.CenterYAnchor.ConstraintEqualTo(blueIcon.CenterYAnchor),
                blueIcon.CenterYAnchor.ConstraintEqualTo(topToolbar.CenterYAnchor),

                _timeLabel.WidthAnchor.ConstraintEqualTo(150),
                _timeSlider.WidthAnchor.ConstraintEqualTo(600),
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _timeSlider.ValueChanged += TimeSlider_ValueChanged;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _timeSlider.ValueChanged -= TimeSlider_ValueChanged;
        }
    }
}