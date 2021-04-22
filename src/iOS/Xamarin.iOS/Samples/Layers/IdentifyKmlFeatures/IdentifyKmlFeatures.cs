// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Linq;
using ArcGISRuntime;
using CoreGraphics;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;
using WebKit;

namespace ArcGISRuntimeXamarin.Samples.IdentifyKmlFeatures
{
    [Register("IdentifyKmlFeatures")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Identify KML features",
        category: "Layers",
        description: "Show a callout with formatted content for a KML feature.",
        instructions: "Tap a feature to identify it. Feature information will be displayed in a callout.",
        tags: new[] { "KML", "KMZ", "Keyhole", "NOAA", "NWS", "OGC", "weather" })]
    public class IdentifyKmlFeatures : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private WKWebView _webView;
        private UIStackView _stackView;

        // Hold a reference to the KML layer for use in identify operations.
        private KmlLayer _forecastLayer;

        // Initial view envelope.
        private readonly Envelope _usEnvelope = new Envelope(-144.619561355187, 18.0328662832097, -66.0903762761083, 67.6390975806745, SpatialReferences.Wgs84);

        public IdentifyKmlFeatures()
        {
            Title = "Identify KML features";
        }

        private void Initialize()
        {
            // Set up the basemap.
            _myMapView.Map = new Map(BasemapStyle.ArcGISDarkGray);

            // Create the dataset.
            KmlDataset dataset = new KmlDataset(new Uri("https://www.wpc.ncep.noaa.gov/kml/noaa_chart/WPC_Day1_SigWx_latest.kml"));

            // Create the layer from the dataset.
            _forecastLayer = new KmlLayer(dataset);

            // Add the layer to the map.
            _myMapView.Map.OperationalLayers.Add(_forecastLayer);

            // Zoom to the extent of the United States.
            _myMapView.SetViewpoint(new Viewpoint(_usEnvelope));
        }

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Clear any existing popups.
            _myMapView.DismissCallout();

            try
            {
                // Perform identify on the KML layer and get the results.
                IdentifyLayerResult identifyResult = await _myMapView.IdentifyLayerAsync(_forecastLayer, e.Position, 2, false);

                // Return if there are no results that are KML placemarks.
                if (!identifyResult.GeoElements.OfType<KmlGeoElement>().Any())
                {
                    return;
                }

                // Get the first identified feature that is a KML placemark
                KmlNode firstIdentifiedPlacemark = identifyResult.GeoElements.OfType<KmlGeoElement>().First().KmlNode;

                if (string.IsNullOrEmpty(firstIdentifiedPlacemark.Description))
                {
                    firstIdentifiedPlacemark.Description = "Weather condition";
                }

                // Show a preview with the HTML content.
                _webView.LoadHtmlString(new NSString(firstIdentifiedPlacemark.BalloonContent), new NSUrl(""));
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
            _myMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _myMapView.GeoViewTapped -= MyMapView_GeoViewTapped;
        }
    }
}