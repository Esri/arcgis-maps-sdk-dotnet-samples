// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI;

namespace ArcGIS.Samples.WmsIdentify
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Identify WMS features",
        category: "Layers",
        description: "Identify features in a WMS layer and display the associated popup content.",
        instructions: "Tap a feature to identify it. The HTML content associated with the feature will be displayed in a web view.",
        tags: new[] { "OGC", "WMS", "callout", "identify", "web map service" })]
    public partial class WmsIdentify : ContentPage
    {
        // Create and hold the URL to the WMS service showing EPA water info
        private Uri _wmsUrl = new Uri("https://sampleserver6.arcgisonline.com/arcgis/services/SampleWorldCities/MapServer/WMSServer?request=GetCapabilities&service=WMS");

        // Create and hold a list of uniquely-identifying WMS layer names to display
        private List<String> _wmsLayerNames = new List<string> { "1" };

        // Hold the WMS layer
        private WmsLayer _wmsLayer;

        public WmsIdentify()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(BasemapStyle.ArcGISImageryStandard);

            // Provide used Map to the MapView
            MyMapView.Map = myMap;

            // Disabling the WrapAroundMode property for the Mapview
            MyMapView.WrapAroundMode = WrapAroundMode.Disabled;

            // Create a new WMS layer displaying the specified layers from the service
            _wmsLayer = new WmsLayer(_wmsUrl, _wmsLayerNames);

            try
            {
                // Load the layer
                await _wmsLayer.LoadAsync();

                // Add the layer to the map
                MyMapView.Map.OperationalLayers.Add(_wmsLayer);

                // Zoom to the layer's extent
                MyMapView.SetViewpoint(new Viewpoint(_wmsLayer.FullExtent));

                // Subscribe to tap events - starting point for feature identification
                MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
            }
            catch (Exception e)
            {
                await Application.Current.Windows[0].Page.DisplayAlert("Error", e.ToString(), "OK");
            }
        }

        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.Maui.GeoViewInputEventArgs e)
        {
            try
            {
                // Perform the identify operation
                IdentifyLayerResult myIdentifyResult = await MyMapView.IdentifyLayerAsync(_wmsLayer, e.Position, 20, false);

                // Return if there's nothing to show
                if (myIdentifyResult.GeoElements.Count < 1)
                {
                    return;
                }

                // Retrieve the identified feature, which is always a WmsFeature for WMS layers
                WmsFeature identifiedFeature = (WmsFeature)myIdentifyResult.GeoElements[0];

                // Retrieve the WmsFeature's HTML content
                string htmlContent = identifiedFeature.Attributes["HTML"].ToString();

                // Note that the service returns a boilerplate HTML result if there is no feature found.
                // This test should work for most arcGIS-based WMS services, but results may vary.
                if (!htmlContent.Contains("OBJECTID"))
                {
                    // Return without showing the result
                    return;
                }

                // Show a page with the HTML content
                await Shell.Current.Navigation.PushAsync(new WmsIdentifyResultDisplayPage(htmlContent));
            }
            catch (Exception ex)
            {
                await Application.Current.Windows[0].Page.DisplayAlert("Error", ex.ToString(), "OK");
            }
        }
    }

    public class WmsIdentifyResultDisplayPage : ContentPage
    {
        public WmsIdentifyResultDisplayPage(string htmlContent)
        {
            Title = "WMS identify result";

            // Create the web browser control
            Content = new WebView
            {
                // Display the string content as an HTML document
                Source = new HtmlWebViewSource() { Html = htmlContent }
            };
        }
    }
}