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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace ArcGISRuntime.UWP.Samples.TakeScreenshot
{
    public partial class TakeScreenshot
    {
        public TakeScreenshot()
        {
            InitializeComponent();

            // Setup the control references and execute initialization 
            Initialize();
        }

        private async void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateImagery());

            // Provide used Map to the MapView
            MyMapView.Map = myMap;
        }

        private async void OnTakeScreenshotButtonClicked(object sender, RoutedEventArgs e)
        {
            // Export the image from mapview and assign it to the imageview
            var exportedImage = await Esri.ArcGISRuntime.UI.RuntimeImageExtensions.ToImageSourceAsync(await MyMapView.ExportImageAsync());

            // Create dialog that is used to show the picture
            var dialog = new ContentDialog()
            {
                Title = "Screenshot",
                MaxWidth = ActualWidth,
                MaxHeight = ActualHeight
            };

            // Create Image
            var imageView = new Image()
            {
                Source = exportedImage,
                Margin = new Thickness(10)  
            };

            // Set image as a content
            dialog.Content = imageView;

            // Show dialog as a full screen overlay. 
            await dialog.ShowAsync();
        }
    }
}
