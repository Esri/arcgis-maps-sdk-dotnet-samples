// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.WmsIdentify
{
    public partial class WmsIdentify : ContentPage
    {
        // Hold the URL to the WMS service showing EPA water info
        private Uri wmsUrl = new Uri("https://watersgeo.epa.gov/arcgis/services/OWPROGRAM/SDWIS_WMERC/MapServer/WMSServer?request=GetCapabilities&service=WMS");

        // Hold a list of uniquely-identifying WMS layer names to display
        private List<String> wmsLayerNames = new List<string> { "4" };

        // Hold the WMS layer
        private WmsLayer myWmsLayer;

        public WmsIdentify()
        {
            InitializeComponent();

            Title = "Identify WMS Feature";

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private async void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateImagery());

            // Provide used Map to the MapView
            MyMapView.Map = myMap;

            // Create a new WMS layer displaying the specified layers from the service
            myWmsLayer = new WmsLayer(wmsUrl, wmsLayerNames);

            // Load the layer
            await myWmsLayer.LoadAsync();

            // Add the layer to the map
            MyMapView.Map.OperationalLayers.Add(myWmsLayer);

            // Subscribe to tap events - starting point for feature identification
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.Xamarin.Forms.GeoViewInputEventArgs e)
        {
            // Perform the identify operation
            IdentifyLayerResult myIdentifyResult = await MyMapView.IdentifyLayerAsync(myWmsLayer, e.Position, 20, false);

            // Return if there's nothing to show
            if (myIdentifyResult.GeoElements.Count < 1)
            {
                return;
            }

            // Retrieve the identified feature, which is always a WmsFeature for WMS layers
            WmsFeature identifiedFeature = (WmsFeature)myIdentifyResult.GeoElements[0];

            // Retrieve the WmsFeature's HTML content
            string htmlContent = identifiedFeature.Attributes["HTML"].ToString();

            // Show a page with the HTML content
            await Navigation.PushAsync(new WmsIdentifyResultDisplayPage(htmlContent));
        }
    }

    public class WmsIdentifyResultDisplayPage : ContentPage
    {
        public WmsIdentifyResultDisplayPage(string htmlContent)
        {
            Title = "WMS Identify Result";

            // Create the web browser control
            WebView htmlView = new WebView();

            // Display the string content as an HTML document
            htmlView.Source = new HtmlWebViewSource() { Html = htmlContent };

            // Create and add a layout to the page
            Grid layout = new Grid();
            this.Content = layout;

            // Add the webview to the page
            layout.Children.Add(htmlView);
        }
    }
}