// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using CoreGraphics;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;
using WebKit;

namespace ArcGISRuntime.Samples.WmsIdentify
{
    [Register("WmsIdentify")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Identify WMS features",
        category: "Layers",
        description: "Identify features in a WMS layer and display the associated popup content.",
        instructions: "Tap a feature to identify it. The HTML content associated with the feature will be displayed in a web view.",
        tags: new[] { "IdentifyLayerAsync", "OGC", "ShowCalloutAt", "WMS", "callout", "web map service" })]
    public class WmsIdentify : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private WKWebView _webView;
        private UIStackView _stackView;

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

        private async void Initialize()
        {
            // Show an imagery basemap.
            _myMapView.Map = new Map(Basemap.CreateImagery());

            // Create a new WMS layer displaying the specified layers from the service.
            _wmsLayer = new WmsLayer(_wmsUrl, _wmsLayerNames);

            try
            {
                // Load the layer.
                await _wmsLayer.LoadAsync();

                // Add the layer to the map.
                _myMapView.Map.OperationalLayers.Add(_wmsLayer);

                // Zoom to the layer's extent.
                _myMapView.SetViewpoint(new Viewpoint(_wmsLayer.FullExtent));
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        private async void _myMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
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

                // Note that the service returns a boilerplate HTML result if there is no feature found.
                //    This would be a good place to check if the result looks like it includes feature details. 

                // Show a preview with the HTML content.
                _webView.LoadHtmlString(new NSString(htmlContent), new NSUrl(""));
            }
            catch (Exception ex)
            {
                new UIAlertView("Error", ex.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

            _webView = new WKWebView(new CGRect(), new WKWebViewConfiguration());

            _myMapView = new MapView();

            _stackView = new UIStackView(new UIView[] {_myMapView, _webView})
            {
                Alignment = UIStackViewAlignment.Fill,
                Distribution = UIStackViewDistribution.FillEqually,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Add the views.
            View.AddSubview(_stackView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _stackView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _stackView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _stackView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _stackView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor)
            });

            SetLayoutOrientation();
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);
            SetLayoutOrientation();
        }

        private void SetLayoutOrientation()
        {
            if (View.TraitCollection.VerticalSizeClass == UIUserInterfaceSizeClass.Compact)
            {
                // Landscape
                _stackView.Axis = UILayoutConstraintAxis.Horizontal;
            }
            else
            {
                // Portrait
                _stackView.Axis = UILayoutConstraintAxis.Vertical;
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _myMapView.GeoViewTapped += _myMapView_GeoViewTapped;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _myMapView.GeoViewTapped -= _myMapView_GeoViewTapped;
        }
    }
}