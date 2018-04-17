// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using UIKit;

namespace ArcGISRuntime.Samples.MapRotation
{
    [Register("MapRotation")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Map rotation",
        "MapView",
        "This sample illustrates how to rotate a map.",
        "")]
    public class MapRotation : UIViewController
    {
        private MapView _myMapView;
        private UIToolbar _toolbar = new UIToolbar();
        private UILabel _rotationLabel = new UILabel();

        private UISlider _rotationSlider = new UISlider()
        {
            MinValue = 0,
            MaxValue = 360
        };

        public MapRotation()
        {
            Title = "Map rotation";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create a new MapView control and provide its location coordinates on the frame
            _myMapView = new MapView();

            // Create a new Map instance with the basemap
            var myBasemap = Basemap.CreateStreets();
            Map myMap = new Map(myBasemap);

            // Assign the Map to the MapView
            _myMapView.Map = myMap;

            // Create the label to display the MapView rotation value
            _rotationLabel.Text = string.Format("{0:0}°", _myMapView.MapRotation);

            // Configure the slider
            _rotationSlider.ValueChanged += (s, e) =>
            {
                _myMapView.SetViewpointRotationAsync(_rotationSlider.Value);
                _rotationLabel.Text = string.Format("{0:0}°", _rotationSlider.Value);
            };

            View.AddSubviews(_myMapView, _toolbar, _rotationLabel, _rotationSlider);
        }

        public override void ViewDidLayoutSubviews()
        {
            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
            // Setup the visual frame for the Toolbar
            _toolbar.Frame = new CoreGraphics.CGRect(0, View.Bounds.Height - 50, View.Bounds.Width, 50);
            _rotationSlider.Frame = new CoreGraphics.CGRect(10, _toolbar.Frame.Top + 10, View.Bounds.Width - 50 - 20, _toolbar.Frame.Height - 20);
            _rotationLabel.Frame = new CoreGraphics.CGRect(View.Bounds.Width - 50 - 10, _toolbar.Frame.Top + 10, 50, _toolbar.Frame.Height - 20);
            base.ViewDidLayoutSubviews();
        }
    }
}