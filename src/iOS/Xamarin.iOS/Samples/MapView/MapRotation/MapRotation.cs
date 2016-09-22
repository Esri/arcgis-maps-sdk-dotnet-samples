// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Foundation;
using System;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.MapRotation
{
    [Register("MapRotation")]
    public class MapRotation : UIViewController
    {
        public MapRotation()
        {
            Title = "Map rotation";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create a variable to hold the yOffset where the MapView control should start
            var yOffset = 60;

            // Create a new MapView control and provide its location coordinates on the frame
            MapView myMapView = new MapView();
            myMapView.Frame = new CoreGraphics.CGRect(0, yOffset, View.Bounds.Width, View.Bounds.Height - yOffset);

            // Create a new Map instance with the basemap  
            var myBasemap = Basemap.CreateStreets();
            Map myMap = new Map(myBasemap);

            // Assign the Map to the MapView
            myMapView.Map = myMap;

            // Create a label to display the MapView rotation value
            UILabel rotationLabel = new UILabel();
            rotationLabel.Frame = new CoreGraphics.CGRect(View.Bounds.Width - 60, 8, View.Bounds.Width, 24);
            rotationLabel.Text = string.Format("{0:0}°", myMapView.MapRotation);

            // Create a slider to control the MapView rotation
            UISlider rotationSlider = new UISlider()
            {
                MinValue = 0,
                MaxValue = 360,
                Value = (float)myMapView.MapRotation
            };
            rotationSlider.Frame = new CoreGraphics.CGRect(10, 8, View.Bounds.Width - 100, 24);
            rotationSlider.ValueChanged += (Object s, EventArgs e) =>
            {
                myMapView.SetViewpointRotationAsync(rotationSlider.Value);
                rotationLabel.Text = string.Format("{0:0}°", myMapView.MapRotation);
            };

            // Create a UIBarButtonItem where its view is the rotation slider
            UIBarButtonItem barButtonSlider = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
            barButtonSlider.CustomView = rotationSlider;

            // Create a UIBarButtonItem where its view is the rotation label
            UIBarButtonItem barButtonLabel = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
            barButtonLabel.CustomView = rotationLabel;

            // Create a toolbar on the bottom of the display 
            UIToolbar toolbar = new UIToolbar();
            toolbar.Frame = new CoreGraphics.CGRect(0, View.Bounds.Height - 40, View.Bounds.Width, View.Bounds.Height);
            toolbar.AutosizesSubviews = true;

            // Add the bar button items to an array of UIBarButtonItems
            UIBarButtonItem[] barButtonItems = new UIBarButtonItem[] { barButtonSlider, barButtonLabel };

            // Add the UIBarButtonItems array to the toolbar
            toolbar.SetItems(barButtonItems, true);

            View.AddSubviews(myMapView, toolbar);
        }
    }
}