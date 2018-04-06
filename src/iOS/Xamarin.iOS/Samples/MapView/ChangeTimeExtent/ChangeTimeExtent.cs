// Copyright 2018 Esri.
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
using CoreGraphics;
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
        // Hold two map service URIs, one for use with an ArcGISMapImageLayer, the other for use with a FeatureLayer.
        private readonly Uri _mapServerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Hurricanes/MapServer");
        private readonly Uri _featureLayerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Earthquakes_Since1970/MapServer/0");

        // Create and hold buttons for changing the time extent.
        private readonly UIButton _twoThousandButton = new UIButton();
        private readonly UIButton _twoThousandFiveButton = new UIButton();

        // Create and hold a reference to a help label.
        private readonly UILabel _helpLabel = new UILabel()
        {
            TextColor = UIColor.Red,
            Text = "Tap a year to filter the data.",
            TextAlignment = UITextAlignment.Center
        };

        // Create and hold reference to the used MapView.
        private MapView _myMapView = new MapView();

        public ChangeTimeExtent()
        {
            Title = "Change time extent";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap and initial location.
            Map map = new Map(Basemap.CreateTopographic());

            // Assign the map to the MapView.
            _myMapView.Map = map;

            // Load the layers from the corresponding URIs.
            ArcGISMapImageLayer imageryLayer = new ArcGISMapImageLayer(_mapServerUri);
            FeatureLayer pointLayer = new FeatureLayer(_featureLayerUri);

            // Add the layers to the map.
            _myMapView.Map.OperationalLayers.Add(imageryLayer);
            _myMapView.Map.OperationalLayers.Add(pointLayer);
        }

        private void TwoThousandButton_Clicked(object sender, EventArgs e)
        {
            // Hard-coded start value: January 1st, 2000.
            DateTime start = new DateTime(2000, 1, 1);

            // Hard-coded end value: December 31st, 2000.
            DateTime end = new DateTime(2000, 12, 31);

            // Set the time extent on the map with the hard-coded values.
            _myMapView.TimeExtent = new TimeExtent(start, end);
        }

        private void TwoThousandFiveButton_Clicked(object sender, EventArgs e)
        {
            // Hard-coded start value: January 1st, 2005.
            DateTime start = new DateTime(2005, 1, 1);

            // Hard-coded end value: December 31st, 2005.
            DateTime end = new DateTime(2005, 12, 31);

            // Set the time extent on the map with the hard-coded values.
            _myMapView.TimeExtent = new TimeExtent(start, end);
        }

        private void CreateLayout()
        {
            // Create the buttons.
            _twoThousandButton.SetTitle("2000", UIControlState.Normal);
            _twoThousandFiveButton.SetTitle("2005", UIControlState.Normal);

            // Set a more visible color.
            _twoThousandButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _twoThousandFiveButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);

            // Add MapView and buttons to the page.
            View.AddSubviews(_myMapView, _helpLabel, _twoThousandButton, _twoThousandFiveButton);

            // Add event handlers for button clicked events.
            _twoThousandButton.TouchUpInside += TwoThousandButton_Clicked;
            _twoThousandFiveButton.TouchUpInside += TwoThousandFiveButton_Clicked;
        }

        public override void ViewDidLayoutSubviews()
        {
            int buttonHeight = 60;
            nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;

            // Setup the visual frame for the help label
            _helpLabel.Frame = new CGRect(5, topMargin + 5, View.Bounds.Width - 10, 20);

            // Setup the visual frame for the buttons.
            _twoThousandButton.Frame = new CGRect(0, topMargin + 20, View.Bounds.Width / 2, buttonHeight);
            _twoThousandFiveButton.Frame = new CGRect(View.Bounds.Width / 2, topMargin + 20, View.Bounds.Width / 2, buttonHeight);

            // Setup the visual frame for the MapView.
            _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            base.ViewDidLayoutSubviews();
        }
    }
}