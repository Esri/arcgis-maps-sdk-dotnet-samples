// Copyright 2018 Esri.
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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ChangeTimeExtent
{
    [Register("ChangeTimeExtent")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Change time extent",
        "MapView",
        "This sample demonstrates how to filter data in layers by applying a time extent to a MapView.",
        "Switch between the available options and observe how the data is filtered.")]
    public class ChangeTimeExtent : UIViewController
    {
        // Hold a reference to the MapView.
        private MapView _myMapView;

        private readonly Uri _mapServerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Hurricanes/MapServer");
        private readonly Uri _featureLayerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Earthquakes_Since1970/MapServer/0");

        public ChangeTimeExtent()
        {
            Title = "Change time extent";
        }

        private void Initialize()
        {
            // Show a topographic basemap.
            _myMapView.Map = new Map(Basemap.CreateTopographic());

            // Load the layers from the corresponding URIs.
            ArcGISMapImageLayer imageryLayer = new ArcGISMapImageLayer(_mapServerUri);
            FeatureLayer pointLayer = new FeatureLayer(_featureLayerUri);

            // Add the layers to the map.
            _myMapView.Map.OperationalLayers.Add(imageryLayer);
            _myMapView.Map.OperationalLayers.Add(pointLayer);
        }

        private void _timeExtentsButton_ValueChanged(object sender, EventArgs e)
        {
            DateTime start;
            DateTime end;

            switch (((UISegmentedControl) sender).SelectedSegment)
            {
                case 0:
                    // Hard-coded values: January 1st, 2000 - December 31st, 2000.
                    start = new DateTime(2000, 1, 1);
                    end = new DateTime(2000, 12, 31);
                    _myMapView.TimeExtent = new TimeExtent(start, end);
                    break;
                case 1:
                    // Hard-coded values: January 1st, 2005 - December 31st, 2005.
                    start = new DateTime(2005, 1, 1);
                    end = new DateTime(2005, 12, 31);
                    _myMapView.TimeExtent = new TimeExtent(start, end);
                    break;
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView();

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            UISegmentedControl timeExtentsButton = new UISegmentedControl("2000", "2005")
            {
                BackgroundColor = UIColor.FromWhiteAlpha(0, .7f),
                TintColor = UIColor.White,
                TranslatesAutoresizingMaskIntoConstraints = false,
                // Clean up borders of segmented control - avoid corner pixels.
                ClipsToBounds = true,
                Layer = {CornerRadius = 5}
            };
            timeExtentsButton.ValueChanged += _timeExtentsButton_ValueChanged;

            // Add the views.
            View.AddSubviews(_myMapView, timeExtentsButton);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),

                timeExtentsButton.LeadingAnchor.ConstraintEqualTo(View.LayoutMarginsGuide.LeadingAnchor),
                timeExtentsButton.TrailingAnchor.ConstraintEqualTo(View.LayoutMarginsGuide.TrailingAnchor),
                timeExtentsButton.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor, 8)
            });
        }
    }
}