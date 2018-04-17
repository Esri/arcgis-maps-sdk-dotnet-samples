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
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        //overlay view for holding imageview
        private UIView _overlayView;

        //UIImage view for the screenshot
        private UIImageView _overlayImageView;

        //Button for closing ImageView
        private UIBarButtonItem _closeImageViewButton;

        // Constant holding offset where the MapView control should start
        private const int yPageOffset = 64;

        public TakeScreenshot()
        {
            Title = "Take screenshot";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Create the UI, setup the control references and execute initialization 
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
            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            _overlayView.Frame = new CoreGraphics.CGRect(10, 80, _myMapView.Frame.Width - 20, _myMapView.Frame.Height - 75);

            _overlayImageView.Frame = new CoreGraphics.CGRect(0, 0, _overlayView.Frame.Width, _overlayView.Frame.Height);
           
            base.ViewDidLayoutSubviews();
        }

        private void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateImagery());

            // Provide used Map to the MapView
            _myMapView.Map = myMap;
        }

        private void OnCloseImageViewClicked(object sender, EventArgs e)
        {
            _overlayView.Hidden = true;
        }

        private async void OnScreenshotButtonClicked(object sender, EventArgs e)
        {
            // Export the image from mapview and assign it to the imageview
            _overlayImageView.Image = await Esri.ArcGISRuntime.UI.RuntimeImageExtensions.ToImageSourceAsync(await _myMapView.ExportImageAsync());
            // Enable the button to close image view
            _closeImageViewButton.Enabled = true;
            // Show the overlay view
            _overlayView.Hidden = false;
        }

        private void CreateLayout()
        {
            // Setup the visual frame for the MapView
            _myMapView = new MapView();
    
            // Create a button to take the screenshot
            var screenshotButton = new UIBarButtonItem() { Title = "Screenshot", Style = UIBarButtonItemStyle.Plain };
            screenshotButton.Clicked += OnScreenshotButtonClicked;

            // Initialize a button to close imageview
            _closeImageViewButton = new UIBarButtonItem() { Title = "Close Preview", Style = UIBarButtonItemStyle.Plain };
            _closeImageViewButton.Clicked += OnCloseImageViewClicked; 
            _closeImageViewButton.Enabled = false;

            // Add the buttons to the toolbar
            SetToolbarItems(new UIBarButtonItem[] {screenshotButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace, null),
                _closeImageViewButton}, false);

            // Show the toolbar
            NavigationController.ToolbarHidden = false;

            // Add the new View as an overlayview
            _overlayView = new UIView();

            // Create a new image view to hold the screenshot image
            _overlayImageView = new UIImageView();
            _overlayImageView.Layer.BorderColor = UIColor.White.CGColor;
            _overlayImageView.Layer.BorderWidth = 2;

            // Add the image view to the overlay view
            _overlayView.AddSubview(_overlayImageView);
            // Hide the image view
            _overlayView.Hidden = true;

            // Add MapView and overlay view to the page
            View.AddSubviews(_myMapView, _overlayView);
        }
    }
}