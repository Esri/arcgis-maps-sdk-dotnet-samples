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
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.TakeScreenshot
{
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
            // Export the image from mapview and assign it to the imageview
            var exportedImage = await MyMapView.ExportImageAsync();

            // Create layout for sublayers page
            // Create root layout
            var layout = new StackLayout();

            var closeButton = new Button
            {
                Text = "Close"
            };
            closeButton.Clicked += CloseButton_Clicked;

            // Create image using exported image source
            var image = new Image()
            {
                Source = exportedImage,
                Margin = new Thickness(10)
            };

            // Add elements into the layout
            layout.Children.Add(closeButton);
            layout.Children.Add(image);

            // Create internal page for the navigation page
            var screenshotPage = new ContentPage()
            {
                Content = layout,
                Title = "Screenshot"
            };

            // Navigate to the sublayers page
            await Navigation.PushAsync(screenshotPage);
        }

        private async void CloseButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
