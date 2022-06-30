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
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ArcGISRuntime.Samples.TakeScreenshot
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Take screenshot",
        category: "MapView",
        description: "Take a screenshot of the map.",
        instructions: "Pan and zoom to find an interesting location, then use the button to take a screenshot. The screenshot will be displayed. Note that there may be a small delay if the map is still rendering when you push the button.",
        tags: new[] { "capture", "export", "image", "print", "screen capture", "screenshot", "share", "shot" })]
    public partial class TakeScreenshot : ContentPage
    {
        public TakeScreenshot()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Show an imagery basemap.
            MyMapView.Map = new Map(BasemapStyle.ArcGISImageryStandard);
        }

        private async void OnTakeScreenshotClicked(object sender, EventArgs e)
        {
            try
            {
                ScreenshotButton.IsEnabled = false;

                // Wait for rendering to finish before taking the screenshot.
                await WaitForRenderCompleteAsync(MyMapView);

                // Export the image from the map view.
                RuntimeImage exportedImage = await MyMapView.ExportImageAsync();

                // Create layout for sublayers page
                // Create root layout
                StackLayout layout = new StackLayout();

                Button closeButton = new Button
                {
                    Text = "Close"
                };
                closeButton.Clicked += CloseButton_Clicked;

                // Create image bitmap by getting stream from the exported image.
                // NOTE: currently broken on UWP due to Xamarin.Forms bug https://github.com/xamarin/Xamarin.Forms/issues/5188.
                var buffer = await exportedImage.GetEncodedBufferAsync();
                byte[] data = new byte[buffer.Length];
                buffer.Read(data, 0, data.Length);
                var bitmap = ImageSource.FromStream(() => new MemoryStream(data));
                Image image = new Image()
                {
                    Source = bitmap,
                    Margin = new Thickness(10)
                };

                // Add elements into the layout.
                layout.Children.Add(closeButton);
                layout.Children.Add(image);

                // Create internal page for the navigation page.
                ContentPage screenshotPage = new ContentPage()
                {
                    Content = layout,
                    Title = "Screenshot"
                };

                // Navigate to the sublayers page.
                await Navigation.PushAsync(screenshotPage);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.ToString(), "OK");
            }
            finally
            {
                ScreenshotButton.IsEnabled = true;
            }
        }

        private static Task WaitForRenderCompleteAsync(Esri.ArcGISRuntime.Xamarin.Forms.MapView mapview)
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

        private void CloseButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }
    }
}