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

namespace ArcGISRuntimeXamarin.Samples.MapRotation
{
    [Register("MapRotation")]
    public class MapRotation : UIViewController
    {

        private MapView _myMapView;
        private int _yOffset = 60;
        private UIToolbar _toolbar;

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

            // Create a label to display the MapView rotation value
            UILabel rotationLabel = new UILabel();
            rotationLabel.Frame = new CoreGraphics.CGRect(View.Bounds.Width - 60, 8, View.Bounds.Width, 24);
            rotationLabel.Text = string.Format("{0:0}°", _myMapView.MapRotation);

            // Create a slider to control the MapView rotation
            UISlider rotationSlider = new UISlider()
            {
                MinValue = 0,
                MaxValue = 360,
                Value = (float)_myMapView.MapRotation
            };
            rotationSlider.Frame = new CoreGraphics.CGRect(10, 8, View.Bounds.Width - 100, 24);
            rotationSlider.ValueChanged += (Object s, EventArgs e) =>
            {
                _myMapView.SetViewpointRotationAsync(rotationSlider.Value);
                rotationLabel.Text = string.Format("{0:0}°", rotationSlider.Value);
            };

            // Create a UIBarButtonItem where its view is the rotation slider
            UIBarButtonItem barButtonSlider = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
            barButtonSlider.CustomView = rotationSlider;

            // Create a UIBarButtonItem where its view is the rotation label
            UIBarButtonItem barButtonLabel = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
            barButtonLabel.CustomView = rotationLabel;

            // Create a toolbar on the bottom of the display 
            _toolbar = new UIToolbar();
            _toolbar.AutosizesSubviews = true;

            // Add the bar button items to an array of UIBarButtonItems
            UIBarButtonItem[] barButtonItems = new UIBarButtonItem[] { barButtonSlider, barButtonLabel };

            // Add the UIBarButtonItems array to the toolbar
            _toolbar.SetItems(barButtonItems, true);

            View.AddSubviews(_myMapView, _toolbar);
        }

        public override void ViewDidLayoutSubviews()
        {
            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
            // Setup the visual frame for the Toolbar
            _toolbar.Frame = new CoreGraphics.CGRect(0, View.Bounds.Height - 40, View.Bounds.Width, View.Bounds.Height);


            base.ViewDidLayoutSubviews();
        }
    }
}