// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
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
        // Hold references to the UI controls.
        private MapView _myMapView;
        private UIView _overlayView;
        private UIImageView _overlayImageView;
        private UIBarButtonItem _screenshotButton;
        private UIBarButtonItem _closePreviewButton;

        public TakeScreenshot()
        {
            Title = "Take screenshot";
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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = UIColor.White};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _screenshotButton = new UIBarButtonItem("Take screenshot", UIBarButtonItemStyle.Plain, OnScreenshotButtonClicked);
            _closePreviewButton = new UIBarButtonItem("Close preview", UIBarButtonItemStyle.Plain, OnCloseImageViewClicked) { Enabled = false };
            
            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                _screenshotButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _closePreviewButton
            };

            _overlayImageView = new UIImageView();
            _overlayImageView.TranslatesAutoresizingMaskIntoConstraints = false;
            _overlayImageView.ContentMode = UIViewContentMode.ScaleAspectFit;

            _overlayView = new UIView();
            _overlayView.TranslatesAutoresizingMaskIntoConstraints = false;
            _overlayView.BackgroundColor = UIColor.White;
            _overlayView.Layer.BorderColor = UIColor.Black.CGColor;
            _overlayView.Layer.BorderWidth = 2;
            _overlayView.Hidden = true;

            // Add the views.
            _overlayView.AddSubview(_overlayImageView);
            View.AddSubviews(_myMapView, toolbar, _overlayView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new []
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),

                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                _overlayView.WidthAnchor.ConstraintEqualTo(View.WidthAnchor, 0.9f),
                _overlayView.HeightAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.HeightAnchor, 0.8f),
                _overlayView.CenterXAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.CenterXAnchor),
                _overlayView.CenterYAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.CenterYAnchor),

                _overlayImageView.LeadingAnchor.ConstraintEqualTo(_overlayView.LeadingAnchor),
                _overlayImageView.TrailingAnchor.ConstraintEqualTo(_overlayView.TrailingAnchor),
                _overlayImageView.TopAnchor.ConstraintEqualTo(_overlayView.TopAnchor),
                _overlayImageView.BottomAnchor.ConstraintEqualTo(_overlayView.BottomAnchor)
            });
        }
    }
}