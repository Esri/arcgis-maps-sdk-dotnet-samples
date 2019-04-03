// Copyright 2018 Esri.
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
using System.Windows;
using Esri.ArcGISRuntime.UI.Controls;

namespace ArcGISRuntime.WPF.Samples.TakeScreenshot
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Take a screenshot",
        "MapView",
        "This sample demonstrates how you can take screenshot of a map. Click 'take screenshot' button to take a screenshot of the visible area of the map. Created image is shown in the sample after creation.",
        "")]
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
            MyMapView.Map = new Map(Basemap.CreateImagery());
        }

        private async void OnScreenshotButtonClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                // Wait for rendering to finish before taking the screenshot.
                await WaitForRenderCompleteAsync(MyMapView);

                // Export the image from MapView.
                RuntimeImage image = await MyMapView.ExportImageAsync();

                // Display the image in the UI.
                ImageView.Source = await image.ToImageSourceAsync();

                // Make the image visible.
                ImageView.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
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