// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.TakeScreenshot
{
    [Register("TakeScreenshot")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Take screenshot",
        category: "MapView",
        description: "Take a screenshot of the map.",
        instructions: "Pan and zoom to find an interesting location, then use the button to take a screenshot. The screenshot will be displayed. Note that there may be a small delay if the map is still rendering when you push the button.",
        tags: new[] { "capture", "export", "image", "print", "screen capture", "screenshot", "share", "shot" })]
    public class TakeScreenshot : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIView _overlayView;
        private UIImageView _overlayImageView;
        private UIBarButtonItem _screenshotButton;
        private UIBarButtonItem _closePreviewButton;

        public TakeScreenshot()
        {
            Title = "Take a screenshot";
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
                // Wait for rendering to finish before taking the screenshot.
                await WaitForRenderCompleteAsync(_myMapView);

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

        private static Task WaitForRenderCompleteAsync(MapView mapview)
        {
            // The task completion source manages the task, including marking it as finished when the time comes.
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            // If the map is currently finished drawing, set the result immediately.
            if (mapview.DrawStatus == DrawStatus.Completed)
            {
                tcs.SetResult(null);
            }
            // Otherwise, configure a callback and a timeout to either set the result when
            // the map is finished drawing or set the result after 2000 ms.
            else
            {
                // Define a cancellation token source for 2000 ms.
                const int timeoutMs = 2000;
                var ct = new CancellationTokenSource(timeoutMs);

                // Register the callback that sets the task result after 2000 ms.
                ct.Token.Register(() =>
                    tcs.TrySetResult(null), false);


                // Define a local function that will set the task result and unregister itself when the map finishes drawing.
                void DrawCompleteHandler(object s, DrawStatusChangedEventArgs e)
                {
                    if (e.Status == DrawStatus.Completed)
                    {
                        mapview.DrawStatusChanged -= DrawCompleteHandler;
                        tcs.TrySetResult(null);
                    }
                }

                // Register the draw complete event handler.
                mapview.DrawStatusChanged += DrawCompleteHandler;
            }

            // Return the task.
            return tcs.Task;
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

            _screenshotButton = new UIBarButtonItem();
            _screenshotButton.Title = "Take screenshot";

            _closePreviewButton = new UIBarButtonItem();
            _closePreviewButton.Title = "Close preview";
            _closePreviewButton.Enabled = false;

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

            _overlayView = new UIView() { BackgroundColor = UIColor.White };
            _overlayView.TranslatesAutoresizingMaskIntoConstraints = false;
            _overlayView.BackgroundColor = UIColor.White;
            _overlayView.Layer.BorderColor = UIColor.Black.CGColor;
            _overlayView.Layer.BorderWidth = 2;
            _overlayView.Hidden = true;

            // Add the views.
            _overlayView.AddSubview(_overlayImageView);
            View.AddSubviews(_myMapView, toolbar, _overlayView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
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

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _screenshotButton.Clicked += OnScreenshotButtonClicked;
            _closePreviewButton.Clicked += OnCloseImageViewClicked;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _screenshotButton.Clicked -= OnScreenshotButtonClicked;
            _closePreviewButton.Clicked -= OnCloseImageViewClicked;
        }
    }
}