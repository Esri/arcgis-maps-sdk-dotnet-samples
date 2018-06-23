// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using CoreGraphics;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using UIKit;
using WebKit;

namespace ArcGISRuntime.Samples.WmsIdentify
{
    [Register("WmsIdentify")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Identify WMS features",
        "Layers",
        "This sample demonstrates how to identify WMS features and display the associated content for an identified WMS feature.",
        "Tap or click on a feature. A callout appears with the returned content for the WMS feature. Note that due to the nature of the WMS service implementation, an empty callout is shown when there is no result; an application might inspect the HTML to determine if the HTML actually contains a feature.")]
    public class WmsIdentify : UIViewController
    {
        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();
        private WKWebView _webView;

        // Create and hold the URL to the WMS service showing EPA water info.
        private readonly Uri _wmsUrl = new Uri("https://watersgeo.epa.gov/arcgis/services/OWPROGRAM/SDWIS_WMERC/MapServer/WMSServer?request=GetCapabilities&service=WMS");

        // Create and hold a list of uniquely-identifying WMS layer names to display.
        private readonly List<string> _wmsLayerNames = new List<string> {"4"};

        // Hold the WMS layer.
        private WmsLayer _wmsLayer;

        public WmsIdentify()
        {
            Title = "Identify WMS features";
        }

        private void CreateLayout()
        {
            // Create the webview for showing the identify result.
            _webView = new WKWebView(new CGRect(), new WKWebViewConfiguration());

            // Add the controls to the view.
            View.AddSubviews(_myMapView, _webView);
        }

        public override void ViewDidLoad()
        {
            CreateLayout();
            Initialize();

            base.ViewDidLoad();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat webviewHeight = 200;

                // Reposition controls.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height - webviewHeight);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, 0, 0);
                _webView.Frame = new CGRect(0, View.Bounds.Height - webviewHeight, View.Bounds.Width, webviewHeight);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private async void Initialize()
        {
            // Show an imagery basemap.
            _myMapView.Map = new Map(Basemap.CreateImagery());

            // Create a new WMS layer displaying the specified layers from the service.
            _wmsLayer = new WmsLayer(_wmsUrl, _wmsLayerNames);

            // Load the layer.
            await _wmsLayer.LoadAsync();

            // Add the layer to the map.
            _myMapView.Map.OperationalLayers.Add(_wmsLayer);

            // Zoom to the layer's extent.
            _myMapView.SetViewpoint(new Viewpoint(_wmsLayer.FullExtent));

            // Subscribe to tap events - starting point for feature identification.
            _myMapView.GeoViewTapped += _myMapView_GeoViewTapped;
        }

        private async void _myMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Perform the identify operation.
            IdentifyLayerResult myIdentifyResult = await _myMapView.IdentifyLayerAsync(_wmsLayer, e.Position, 20, false);

            // Return if there's nothing to show.
            if (myIdentifyResult.GeoElements.Count < 1)
            {
                return;
            }

            // Retrieve the identified feature, which is always a WmsFeature for WMS layers.
            WmsFeature identifiedFeature = (WmsFeature) myIdentifyResult.GeoElements[0];

            // Retrieve the WmsFeature's HTML content.
            string htmlContent = identifiedFeature.Attributes["HTML"].ToString();

            // Note that the service returns a boilerplate HTML result if there is no feature found;
            //    here might be a good place to check for that and filter out spurious results.

            // Show a preview with the HTML content.
            _webView.LoadHtmlString(new NSString(htmlContent), new NSUrl(""));
        }
    }
}