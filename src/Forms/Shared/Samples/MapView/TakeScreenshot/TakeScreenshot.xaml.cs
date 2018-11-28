// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using System;
using System.IO;
using Esri.ArcGISRuntime.UI;
using Xamarin.Forms;

namespace ArcGISRuntime.Samples.TakeScreenshot
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Take screenshot",
        "MapView",
        "This sample demonstrates how you can take screenshot of a map. Click 'capture' button to take a screenshot of the visible area of the map. Created image is shown in the sample after creation.",
        "")]
    public partial class TakeScreenshot : ContentPage
    {
        public TakeScreenshot()
        {
            InitializeComponent();
            Title = "Take screenshot";

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            // Create a new Map instance with the basemap  
            Basemap myBasemap = Basemap.CreateStreets();
            Map myMap = new Map(myBasemap);

            // Assign the map to the MapView
            MyMapView.Map = myMap;
        }

        private async void OnTakeScreenshotClicked(object sender, EventArgs e)
        {
            try
            {
                // Export the image from mapview and assign it to the imageview
                RuntimeImage exportedImage = await MyMapView.ExportImageAsync();

                // Create layout for sublayers page
                // Create root layout
                StackLayout layout = new StackLayout();

                Button closeButton = new Button
                {
                    Text = "Close"
                };
                closeButton.Clicked += CloseButton_Clicked;

                // Create image bitmap by getting stream from the exported image
                var buffer = await exportedImage.GetEncodedBufferAsync();
                byte[] data = new byte[buffer.Length];
                buffer.Read(data, 0, data.Length);
                var bitmap = ImageSource.FromStream(() => new MemoryStream(data));
                Image image = new Image()
                {
                    Source = bitmap,
                    Margin = new Thickness(10)
                };

                // Add elements into the layout
                layout.Children.Add(closeButton);
                layout.Children.Add(image);

                // Create internal page for the navigation page
                ContentPage screenshotPage = new ContentPage()
                {
                    Content = layout,
                    Title = "Screenshot"
                };

                // Navigate to the sublayers page
                await Navigation.PushAsync(screenshotPage);
            }
            catch (Exception ex)
            {
                await ((Page)Parent).DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private async void CloseButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
