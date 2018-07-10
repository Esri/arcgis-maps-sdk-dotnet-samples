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
        private readonly MapView _myMapView = new MapView();
        private readonly UIView _overlayView = new UIView();
        private readonly UIImageView _overlayImageView = new UIImageView();

        private readonly UIBarButtonItem _closeImageViewButton = new UIBarButtonItem
        {
            Title = "Close preview",
            Style = UIBarButtonItemStyle.Plain
        };

        public TakeScreenshot()
        {
            Title = "Take screenshot";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references and execute initialization.
            CreateLayout();
            Initialize();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            NavigationController.ToolbarHidden = true;
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;

                // Reposition the controls.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, 0, 0);
                _overlayView.Frame = new CGRect(10, 80, _myMapView.Frame.Width - 20, _myMapView.Frame.Height - 75);
                _overlayImageView.Frame = new CGRect(0, 0, _overlayView.Frame.Width, _overlayView.Frame.Height);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
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
            _closeImageViewButton.Enabled = false;
        }

        private async void OnScreenshotButtonClicked(object sender, EventArgs e)
        {
            // Export the image from the MapView.
            RuntimeImage exportedImage = await _myMapView.ExportImageAsync();

            // Convert the exported image to a suitable display format, then display it.
            _overlayImageView.Image = await exportedImage.ToImageSourceAsync();

            // Enable the button to close image view.
            _closeImageViewButton.Enabled = true;

            // Show the overlay view.
            _overlayView.Hidden = false;
        }

        private void CreateLayout()
        {
            // Configure the UI.
            UIBarButtonItem screenshotButton = new UIBarButtonItem
            {
                Title = "Screenshot",
                Style = UIBarButtonItemStyle.Plain
            };
            screenshotButton.Clicked += OnScreenshotButtonClicked;

            // Initialize a button to close image preview.
            _closeImageViewButton.Clicked += OnCloseImageViewClicked;
            _closeImageViewButton.Enabled = false;

            // Add the buttons to the toolbar.
            SetToolbarItems(new[]
            {
                screenshotButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace, null),
                _closeImageViewButton
            }, false);

            // Show the toolbar.
            NavigationController.ToolbarHidden = false;

            // Create a new image view to hold the screenshot image.
            _overlayImageView.Layer.BorderColor = UIColor.White.CGColor;
            _overlayImageView.Layer.BorderWidth = 2;

            // Add the image view to the overlay view.
            _overlayView.AddSubview(_overlayImageView);
            // Hide the image view.
            _overlayView.Hidden = true;

            // Add controls to the view.
            View.AddSubviews(_myMapView, _overlayView);
        }
    }
}