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
using System.Linq;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.UWP.Samples.WmsIdentify
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Identify WMS features",
        "Layers",
        "This sample demonstrates how to identify WMS features and display the associated content for an identified WMS feature.",
        "Tap or click on a feature. A callout appears with the returned content for the WMS feature. Note that due to the nature of the WMS service implementation, an empty callout is shown when there is no result; an application might inspect the HTML to determine if the HTML actually contains a feature.")]
    public partial class WmsIdentify
    {
        // Create and hold the URL to the WMS service showing EPA water info
        private Uri _wmsUrl = new Uri("https://watersgeo.epa.gov/arcgis/services/OWPROGRAM/SDWIS_WMERC/MapServer/WMSServer?request=GetCapabilities&service=WMS");

        // Create and hold a list of uniquely-identifying WMS layer names to display
        private List<String> _wmsLayerNames = new List<string> { "4" };

        // Hold the WMS layer
        private WmsLayer _wmsLayer;

        public WmsIdentify()
        {
            InitializeComponent();

            // Setup the control references and execute initialization
            Initialize();
        }

        private async void Initialize()
        {
            // Apply an imagery basemap to the map
            MyMapView.Map = new Map(Basemap.CreateImagery());

            // Create a new WMS layer displaying the specified layers from the service
            _wmsLayer = new WmsLayer(_wmsUrl, _wmsLayerNames);

            // Load the layer
            await _wmsLayer.LoadAsync();

            // Add the layer to the map
            MyMapView.Map.OperationalLayers.Add(_wmsLayer);

            // Zoom to the layer's extent
            MyMapView.SetViewpoint(new Viewpoint(_wmsLayer.FullExtent));

            // Subscribe to tap events - starting point for feature identification
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            // Perform the identify operation
            IdentifyLayerResult myIdentifyResult = await MyMapView.IdentifyLayerAsync(_wmsLayer, e.Position, 20, false);

            // Return if there's nothing to show
            if (myIdentifyResult.GeoElements.Count() < 1)
            {
                return;
            }

            // Retrieve the identified feature, which is always a WmsFeature for WMS layers
            WmsFeature identifiedFeature = (WmsFeature)myIdentifyResult.GeoElements[0];

            // Retrieve the WmsFeature's HTML content
            string htmlContent = identifiedFeature.Attributes["HTML"].ToString();

            // Note that the service returns a boilerplate HTML result if there is no feature found;
            //    here might be a good place to check for that and filter out spurious results

            // Show a callout with the HTML content
            ShowHtmlCallout(htmlContent, e.Location);
        }

        private void ShowHtmlCallout(string htmlContent, MapPoint position)
        {
            // Create the web browser control
            WebView htmlView = new WebView() { Height = 100, Width = 200 };

            // Display the string content as an HTML document
            htmlView.NavigateToString(htmlContent);

            // Create the callout with the browser
            MyMapView.ShowCalloutAt(position, htmlView);
        }
    }
}