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
using System.Windows.Controls;

namespace ArcGISRuntime.WPF.Samples.WmsIdentify
{
    public partial class WmsIdentify
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

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private async void Initialize()
        {
            // Apply an imagery basemap to the map
            MyMapView.Map = new Map(Basemap.CreateImagery());

            // Create a new WMS layer displaying the specified layers from the service
            myWmsLayer = new WmsLayer(wmsUrl, wmsLayerNames);

            // Load the layer
            await myWmsLayer.LoadAsync();

            // Add the layer to the map
            MyMapView.Map.OperationalLayers.Add(myWmsLayer);

            // Subscribe to tap events - starting point for feature identification
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            // Perform the identify operation
            IdentifyLayerResult myIdentifyResult = await MyMapView.IdentifyLayerAsync(myWmsLayer, e.Position, 20, false);

            // Return if there's nothing to show
            if (myIdentifyResult.GeoElements.Count() < 1)
            {
                return;
            }

            // Retrieve the identified feature, which is always a WmsFeature for WMS layers
            WmsFeature identifiedFeature = myIdentifyResult.GeoElements.First() as WmsFeature;

            // Retrieve teh WmsFeature's HTML content
            string htmlContent = identifiedFeature.Attributes["HTML"].ToString();

            // Show a callout with the HTML content
            ShowHtmlCallout(htmlContent, e.Location);
        }

        private void ShowHtmlCallout(string htmlContent, MapPoint position)
        {
            // Create the web browser control
            WebBrowser htmlView = new WebBrowser() { Height = 200, Width = 400 };

            // Display the string content as an HTML document
            htmlView.NavigateToString(htmlContent);

            // Create the callout with the browser
            MyMapView.ShowCalloutAt(position, htmlView);
        }
    }
}