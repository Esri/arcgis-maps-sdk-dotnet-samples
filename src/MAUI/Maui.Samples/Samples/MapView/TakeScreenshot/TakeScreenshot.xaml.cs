// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;

namespace ArcGIS.Samples.TakeScreenshot
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
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

                // Create image bitmap by getting stream from the exported image.
                var buffer = await exportedImage.GetEncodedBufferAsync();
                byte[] data = new byte[buffer.Length];
                buffer.Read(data, 0, data.Length);
                var bitmap = ImageSource.FromStream(() => new MemoryStream(data));

                // Add elements into the layout.
                ScreenshotImage.Source = bitmap;

                ScreenshotView.IsVisible = true;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private static Task WaitForRenderCompleteAsync(Esri.ArcGISRuntime.Maui.MapView mapview)
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
            ScreenshotView.IsVisible = false;
            ScreenshotButton.IsEnabled = true;
        }
    }
}