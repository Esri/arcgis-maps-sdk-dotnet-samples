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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.WmsIdentify
{
    [Register("WmsIdentify")]
    public class WmsIdentify : UIViewController
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Hold the URL to the WMS service showing EPA water info
        private Uri wmsUrl = new Uri("https://watersgeo.epa.gov/arcgis/services/OWPROGRAM/SDWIS_WMERC/MapServer/WMSServer?request=GetCapabilities&service=WMS");

        // Hold a list of uniquely-identifying WMS layer names to display
        private List<String> _wmsLayerNames = new List<string> { "4" };

        // Hold the WMS layer
        private WmsLayer _myWmsLayer;

        // Hold the webview for displaying identify result content
        private WebKit.WKWebView _myWebview = new WebKit.WKWebView(new CoreGraphics.CGRect(), new WebKit.WKWebViewConfiguration());

        // Button for dismissing the web view
        private UIButton _myCloseButton = new UIButton();

        public WmsIdentify()
        {
            Title = "Identify WMS Features";
        }

        private void CreateLayout()
        {
            // Add MapView to the page
            View.AddSubviews(_myMapView);

            // Set close result button text
            _myCloseButton.SetTitle("Close Result", UIControlState.Normal);
            _myCloseButton.SetTitleColor(UIColor.Red, UIControlState.Normal);

            // Hide the results controls from the view
            _myWebview.Hidden = true;
            _myCloseButton.Hidden = true;
            View.AddSubviews(_myWebview, _myCloseButton);

            // Subscribe to close result events
            _myCloseButton.TouchUpInside += (sender, e) => { HideHtml(); };
        }

        public override void ViewDidLoad()
        {
            CreateLayout();
            Initialize();

            base.ViewDidLoad();
        }

        public override void ViewDidLayoutSubviews()
        {
            // Setup the visual frame for the MapView, web view, and close result buttons
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
            _myWebview.Frame = new CoreGraphics.CGRect(50, 50, View.Bounds.Width - 100, View.Bounds.Height - 120);
            _myCloseButton.Frame = new CoreGraphics.CGRect(10, View.Bounds.Height - 20, View.Bounds.Width, 20);
            base.ViewDidLayoutSubviews();
        }

        private async void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateImagery());

            // Provide used Map to the MapView
            _myMapView.Map = myMap;

            // Create a new WMS layer displaying the specified layers from the service
            _myWmsLayer = new WmsLayer(wmsUrl, _wmsLayerNames);

            // Load the layer
            await _myWmsLayer.LoadAsync();

            // Add the layer to the map
            _myMapView.Map.OperationalLayers.Add(_myWmsLayer);

            // Zoom to the layer's extent
            _myMapView.SetViewpoint(new Viewpoint(_myWmsLayer.FullExtent));

            // Subscribe to tap events - starting point for feature identification
            _myMapView.GeoViewTapped += _myMapView_GeoViewTapped;
        }

        private async void _myMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Perform the identify operation
            IdentifyLayerResult myIdentifyResult = await _myMapView.IdentifyLayerAsync(_myWmsLayer, e.Position, 20, false);

            // Return if there's nothing to show
            if (myIdentifyResult.GeoElements.Count < 1)
            {
                return;
            }

            // Retrieve the identified feature, which is always a WmsFeature for WMS layers
            WmsFeature identifiedFeature = (WmsFeature)myIdentifyResult.GeoElements[0];

            // Retrieve the WmsFeature's HTML content
            string htmlContent = identifiedFeature.Attributes["HTML"].ToString();

            // Show a preview with the HTML content
            ShowHtml(htmlContent, e.Location);
        }

        private void ShowHtml(string htmlContent, MapPoint position)
        {
            // Load the HTML content
            _myWebview.LoadHtmlString(new NSString(htmlContent), new NSUrl(""));

            // Update the webview frame and add to view
            _myWebview.Frame = new CoreGraphics.CGRect(50, 50, View.Bounds.Width - 100, View.Bounds.Height - 120);
            _myWebview.Hidden = false;

            // Update the close button frame and add to view
            _myCloseButton.Frame = new CoreGraphics.CGRect(10, View.Bounds.Height - 20, View.Bounds.Width, 20);
            _myCloseButton.Hidden = false;
        }

        private void HideHtml()
        {
            _myWebview.Hidden = true;
            _myCloseButton.Hidden = true;
        }
    }
}