// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArcGIS.WinUI.Samples.TakeScreenshot
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Take screenshot",
        category: "MapView",
        description: "Take a screenshot of the map.",
        instructions: "Pan and zoom to find an interesting location, then use the button to take a screenshot. The screenshot will be displayed. Note that there may be a small delay if the map is still rendering when you push the button.",
        tags: new[] { "capture", "export", "image", "print", "screen capture", "screenshot", "share", "shot" })]
    public partial class TakeScreenshot
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

        private async void OnTakeScreenshotButtonClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                // Wait for rendering to finish before taking the screenshot.
                await WaitForRenderCompleteAsync(MyMapView);

                // Export the image from the map view.
                RuntimeImage image = await MyMapView.ExportImageAsync();

                // Convert the image to a displayable format.
                ImageSource exportedImage = await image.ToImageSourceAsync();

                // Set the screenshot view to the new exported image.
                ScreenshotView.Source = exportedImage;

                // Make the screenshot view visible in the UI.
                ScreenshotView.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                await new MessageDialog2(ex.ToString(), "Error").ShowAsync();
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
    }
}