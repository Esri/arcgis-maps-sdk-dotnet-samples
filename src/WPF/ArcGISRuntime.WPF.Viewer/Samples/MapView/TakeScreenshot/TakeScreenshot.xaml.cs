// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using System.Windows;

namespace ArcGISRuntime.WPF.Samples.TakeScreenshot
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Take screenshot",
        "MapView",
        "This sample demonstrates how you can take screenshot of a map. Click 'take screenshot' button to take a screenshot of the visible area of the map. Created image is shown in the sample after creation.",
        "")]
    public partial class TakeScreenshot
    {
        public TakeScreenshot()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateImagery());

            // Provide used Map to the MapView
            MyMapView.Map = myMap;
        }

        private async void OnScreenshotButtonClicked(object sender, RoutedEventArgs e)
        {
            // Export the image from mapview and assign it to the imageview
            imageView.Source = await Esri.ArcGISRuntime.UI.RuntimeImageExtensions.ToImageSourceAsync(await MyMapView.ExportImageAsync());
        }
    }
}
