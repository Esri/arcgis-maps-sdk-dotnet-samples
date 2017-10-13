// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime;
using Foundation;
using System;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ChangeTimeExtent
{
    [Register("ChangeTimeExtent")]
    public class ChangeTimeExtent : UIViewController
    {
        // Hold two map service URIs, one for use with a ArcGISMapImageLayer, the other for use with a FeatureLayer
        private Uri _mapServerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Hurricanes/MapServer");

        private Uri _featureLayerUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Hurricanes/MapServer/1");

        // Create and hold buttons for changing the time extent
        private UIButton _intervalButton1 = new UIButton();

        private UIButton _intervalButton2 = new UIButton();

        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        public ChangeTimeExtent()
        {
            Title = "Change time extent";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        private void CreateLayout()
        {
            // Create the buttons
            _intervalButton1.SetTitle("Interval 1", UIControlState.Normal);
            _intervalButton2.SetTitle("Interval 2", UIControlState.Normal);

            // Set a more visible color
            _intervalButton1.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _intervalButton2.SetTitleColor(UIColor.Blue, UIControlState.Normal);

            // Add MapView and buttons to the page
            View.AddSubviews(_myMapView, _intervalButton1, _intervalButton2);

            // Add event handlers for button clicked events
            _intervalButton1.TouchUpInside += _intervalButton1_Clicked;
            _intervalButton2.TouchUpInside += _intervalButton2_Clicked;
        }

        public override void ViewDidLayoutSubviews()
        {
            // Hold height of buttons
            int height = 60;

            // Setup the visual frame for the buttons
            _intervalButton1.Frame = new CoreGraphics.CGRect(0, NavigationController.TopLayoutGuide.Length + height, View.Bounds.Width / 2, height);
            _intervalButton2.Frame = new CoreGraphics.CGRect(View.Bounds.Width / 2, NavigationController.TopLayoutGuide.Length + height, View.Bounds.Width / 2, height);

            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            base.ViewDidLayoutSubviews();
        }

        private void Initialize()
        {
            // Create new Map with basemap and initial location
            Map myMap = new Map(Basemap.CreateTopographic());

            // Assign the map to the MapView
            _myMapView.Map = myMap;

            // Load the layers from the corresponding URIs
            ArcGISMapImageLayer myImageryLayer = new ArcGISMapImageLayer(_mapServerUri);
            FeatureLayer myFeatureLayer = new FeatureLayer(_featureLayerUri);

            // Add the layers to the map
            _myMapView.Map.OperationalLayers.Add(myImageryLayer);
            _myMapView.Map.OperationalLayers.Add(myFeatureLayer);
        }

        private void _intervalButton1_Clicked(object sender, EventArgs e)
        {
            // Hard-coded start value: August 4th, 2000
            DateTime start = new DateTime(2000, 8, 4);

            // Hard-coded end value: September 4th, 2000
            DateTime end = new DateTime(2000, 9, 4);

            // Set the time extent on the map with the hard-coded values
            _myMapView.TimeExtent = new TimeExtent(new DateTimeOffset(start), new DateTimeOffset(end));
        }

        private void _intervalButton2_Clicked(object sender, EventArgs e)
        {
            // Hard-coded start value: September 22nd, 2000
            DateTime start = new DateTime(2000, 9, 22);

            // Hard-coded end value: October 22nd, 2000
            DateTime end = new DateTime(2000, 10, 22);

            // Set the time extent on the map with the hard-coded values
            _myMapView.TimeExtent = new TimeExtent(start, end);
        }
    }
}