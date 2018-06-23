// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using CoreGraphics;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
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
        // Create and hold references to UI controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UIToolbar _toolbar = new UIToolbar();
        private readonly UILabel _rotationLabel = new UILabel();

        private readonly UISlider _rotationSlider = new UISlider
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

            CreateLayout();
        }

        private void CreateLayout()
        {
            // Show a streets basemap.
            _myMapView.Map = new Map(Basemap.CreateStreets());

            // Create the label to display the MapView rotation value.
            _rotationLabel.Text = $"{_myMapView.MapRotation:0}°";
            _rotationLabel.TextAlignment = UITextAlignment.Center;

            // Configure the slider.
            _rotationSlider.ValueChanged += (s, e) =>
            {
                _myMapView.SetViewpointRotationAsync(_rotationSlider.Value);
                _rotationLabel.Text = $"{_rotationSlider.Value:0}°";
            };

            // Add the controls to the view.
            View.AddSubviews(_myMapView, _toolbar, _rotationLabel, _rotationSlider);
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat margin = 5;
                nfloat controlHeight = 30;
                nfloat toolbarHeight = controlHeight + (2 * margin);
                nfloat labelWidth = 50;
                nfloat sliderMargin = 50;

                // Reposition the views.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, toolbarHeight, 0);
                _toolbar.Frame = new CGRect(0, View.Bounds.Height - toolbarHeight, View.Bounds.Width, toolbarHeight);
                _rotationSlider.Frame = new CGRect(sliderMargin, _toolbar.Frame.Top + margin, View.Bounds.Width - labelWidth - margin - sliderMargin, controlHeight);
                _rotationLabel.Frame = new CGRect(View.Bounds.Width - labelWidth - (2 * margin), _toolbar.Frame.Top + margin, labelWidth, controlHeight);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }
    }
}