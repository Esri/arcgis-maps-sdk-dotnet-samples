// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using CoreGraphics;
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
        // Create and hold references to the UI controls.
        private readonly UIToolbar _toolbar = new UIToolbar();
        private readonly MapView _myMapView = new MapView();
        private readonly UIButton _twoThousandButton = new UIButton();
        private readonly UIButton _twoThousandFiveButton = new UIButton();
        private readonly Uri _mapServerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Hurricanes/MapServer");
        private readonly Uri _featureLayerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Earthquakes_Since1970/MapServer/0");

        private readonly UILabel _helpLabel = new UILabel
        {
            TextColor = UIColor.Red,
            Text = "Tap a year to filter the data.",
            TextAlignment = UITextAlignment.Center
        };

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
            // Show a topographic basemap.
            _myMapView.Map = new Map(Basemap.CreateTopographic());

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
            _twoThousandButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _twoThousandFiveButton.SetTitleColor(View.TintColor, UIControlState.Normal);

            // Add MapView and buttons to the page.
            View.AddSubviews(_myMapView, _toolbar, _helpLabel, _twoThousandButton, _twoThousandFiveButton);

            // Add event handlers for button clicked events.
            _twoThousandButton.TouchUpInside += TwoThousandButton_Clicked;
            _twoThousandFiveButton.TouchUpInside += TwoThousandFiveButton_Clicked;
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat controlHeight = 30;
                nfloat margin = 5;

                // Reposition the views.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _toolbar.Frame = new CGRect(0, View.Bounds.Height - 2 * controlHeight - 3 * margin, View.Bounds.Width, controlHeight * 2 + margin * 3);
                _helpLabel.Frame = new CGRect(margin, View.Bounds.Height - 2 * controlHeight - 2 * margin, View.Bounds.Width - 2 * margin, controlHeight);
                _twoThousandButton.Frame = new CGRect(margin, View.Bounds.Height - controlHeight - margin, View.Bounds.Width / 2 - margin, controlHeight);
                _twoThousandFiveButton.Frame = new CGRect(View.Bounds.Width / 2 + margin, View.Bounds.Height - controlHeight - margin, View.Bounds.Width / 2 - margin, controlHeight);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, _toolbar.Frame.Height, 0);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }
    }
}