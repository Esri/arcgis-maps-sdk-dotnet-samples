// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArcGISRuntime.Samples.TakeScreenshot
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Take screenshot",
        "MapView",
        "Take a screenshot of the map.",
        "Pan and zoom to find an interesting location, then use the button to take a screenshot. The screenshot will be displayed. Note that there may be a small delay if the map is still rendering when you push the button.",
        "capture", "export", "image", "print", "screen capture", "screenshot", "share", "shot")]
    public class TakeScreenshot : Activity
    {
        // Hold a reference to the map view.
        private MapView _myMapView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Take a screenshot";

            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Show an imagery basemap.
            _myMapView.Map = new Map(Basemap.CreateImagery());
        }

        private async void OnTakeScreenshotClicked(object sender, EventArgs e)
        {
            try
            {
                // Wait for rendering to finish before taking the screenshot.
                await WaitForRenderCompleteAsync(_myMapView);

                // Export the image from map view.
                RuntimeImage exportedImage = await _myMapView.ExportImageAsync();

                // Create an image button (this will display the exported map view image).
                ImageButton myImageButton = new ImageButton(this)
                {
                    // Define the size of the image button to be 2/3 the size of the map view.
                    LayoutParameters = new Android.Views.ViewGroup.LayoutParams((int) (_myMapView.Width * .667), (int) (_myMapView.Height * .667))
                };

                // Set the source of the image button to be that of the exported map view image.
                myImageButton.SetImageBitmap(await exportedImage.ToImageSourceAsync());

                // Make the image that was captured from the map view export to fit within (aka scale-to-fit) the image button.
                myImageButton.SetScaleType(ImageView.ScaleType.FitCenter);

                // Define a popup with a single image button control and make the size of the popup to be 2/3 the size of the map view.
                PopupWindow myPopupWindow = new PopupWindow(myImageButton, (int) (_myMapView.Width * .667), (int) (_myMapView.Height * .667));

                // Display the popup in the middle of the map view.
                myPopupWindow.ShowAtLocation(_myMapView, Android.Views.GravityFlags.Center, 0, 0);

                // Define a lambda event handler to close the popup when the user clicks on the image button.
                myImageButton.Click += (s, a) => myPopupWindow.Dismiss();
            }
            catch (Exception ex)
            {
                // Display any errors to the user if capturing the map view image did not work.
                AlertDialog.Builder alertBuilder = new AlertDialog.Builder(this);
                alertBuilder.SetTitle("ExportImageAsync error");
                alertBuilder.SetMessage("Capturing image failed. " + ex.Message);
                alertBuilder.Show();
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

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            LinearLayout layout = new LinearLayout(this) {Orientation = Orientation.Vertical};

            // Add a button to take a screen shot, with wired up event.
            Button takeScreenshotButton = new Button(this)
            {
                Text = "Capture"
            };
            takeScreenshotButton.Click += OnTakeScreenshotClicked;
            layout.AddView(takeScreenshotButton);

            // Add the map view to the layout.
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}