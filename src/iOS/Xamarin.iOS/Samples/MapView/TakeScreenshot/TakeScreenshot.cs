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
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.TakeScreenshot
{
    [Register("TakeScreenshot")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Take screenshot",
        "MapView",
        "This sample demonstrates how you can take screenshot of a map. The app has a Screenshot button in the bottom toolbar you can tap to take screenshot of the visible area of the map. You can pan or zoom to a specific location and tap on the button, which also shows you the preview of the image produced. You can tap on the Close Preview button to close image preview.",
        "")]
    public class TakeScreenshot : UIViewController
    {
        // Create and hold references to the UI controls.
        private MapView _myMapView;
        private UIView _overlayView;
        private UIImageView _overlayImageView;
        private UIBarButtonItem _screenshotButton;
        private UIBarButtonItem _closePreviewButton;
        private UIToolbar _toolbar;

        public TakeScreenshot()
        {
            Title = "Take screenshot";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        private void Initialize()
        {
            // Show an imagery basemap.
            _myMapView.Map = new Map(Basemap.CreateImagery());
        }

        private void OnCloseImageViewClicked(object sender, EventArgs e)
        {
            _overlayView.Hidden = true;

            // Disable the button to close image view.
            _closePreviewButton.Enabled = false;
        }

        private async void OnScreenshotButtonClicked(object sender, EventArgs e)
        {
            try
            {
                // Export the image from the MapView.
                RuntimeImage exportedImage = await _myMapView.ExportImageAsync();

                // Convert the exported image to a suitable display format, then display it.
                _overlayImageView.Image = await exportedImage.ToImageSourceAsync();

                // Enable the button to close image view.
                _closePreviewButton.Enabled = true;

                // Show the overlay view.
                _overlayView.Hidden = false;
            }
            catch (Exception ex)
            {
                new UIAlertView("Error", ex.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        public override void LoadView()
        {
            View = new UIView { BackgroundColor = UIColor.White };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;
            
            _toolbar = new UIToolbar();
            _toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            
            _screenshotButton = new UIBarButtonItem("Take screenshot", UIBarButtonItemStyle.Plain, OnScreenshotButtonClicked);
            _closePreviewButton = new UIBarButtonItem("Close preview", UIBarButtonItemStyle.Plain, OnCloseImageViewClicked);
            _closePreviewButton.Enabled = false;

            _overlayImageView = new UIImageView();
            _overlayImageView.TranslatesAutoresizingMaskIntoConstraints = false;
            _overlayImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            
            _overlayView = new UIView();
            _overlayView.TranslatesAutoresizingMaskIntoConstraints = false;
            _overlayView.BackgroundColor = UIColor.White;
            _overlayView.Layer.BorderColor = UIColor.Black.CGColor;
            _overlayView.Layer.BorderWidth = 2;
            _overlayView.Hidden = true;
            
            _overlayView.AddSubview(_overlayImageView);
            View.AddSubviews(_myMapView, _toolbar, _overlayView);

            _toolbar.Items = new[]
            {
                _screenshotButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _closePreviewButton
            };

            _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(_toolbar.TopAnchor).Active = true;

            _toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor).Active = true;
            _toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;

            _overlayView.WidthAnchor.ConstraintEqualTo(View.WidthAnchor, 0.9f).Active = true;
            _overlayView.HeightAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.HeightAnchor, 0.8f).Active = true;
            _overlayView.CenterXAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.CenterXAnchor).Active = true;
            _overlayView.CenterYAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.CenterYAnchor).Active = true;

            _overlayImageView.LeadingAnchor.ConstraintEqualTo(_overlayView.LeadingAnchor).Active = true;
            _overlayImageView.TrailingAnchor.ConstraintEqualTo(_overlayView.TrailingAnchor).Active = true;
            _overlayImageView.TopAnchor.ConstraintEqualTo(_overlayView.TopAnchor).Active = true;
            _overlayImageView.BottomAnchor.ConstraintEqualTo(_overlayView.BottomAnchor).Active = true;
        }
    }
}