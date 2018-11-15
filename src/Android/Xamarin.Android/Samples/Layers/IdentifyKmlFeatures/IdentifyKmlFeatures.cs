// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Linq;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI.Controls;

namespace ArcGISRuntimeXamarin.Samples.IdentifyKmlFeatures
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Identify KML features",
        "Layers",
        "Identify KML features and show popups.",
        "")]
    public class IdentifyKmlFeatures : Activity
    {
        // Hold references to UI controls.
        private MapView _myMapView = new MapView();
        private WebView _htmlView;
        private LinearLayout _sampleLayout;
        private LinearLayout.LayoutParams _layoutParams;

        // Hold a reference to the KML layer for use in identify operations.
        private KmlLayer _forecastLayer;

        // Initial view envelope.
        private readonly Envelope _usEnvelope = new Envelope(-144.619561355187, 18.0328662832097, -66.0903762761083, 67.6390975806745, SpatialReferences.Wgs84);

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Identify KML features";

            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Set up the basemap.
            _myMapView.Map = new Map(Basemap.CreateDarkGrayCanvasVector());

            // Create the dataset.
            KmlDataset dataset = new KmlDataset(new Uri("https://www.wpc.ncep.noaa.gov/kml/noaa_chart/WPC_Day1_SigWx.kml"));

            // Create the layer from the dataset.
            _forecastLayer = new KmlLayer(dataset);

            // Add the layer to the map.
            _myMapView.Map.OperationalLayers.Add(_forecastLayer);

            // Zoom to the extent of the United States.
            _myMapView.SetViewpoint(new Viewpoint(_usEnvelope));

            // Listen for taps to identify features.
            _myMapView.GeoViewTapped += MyMapView_GeoViewTapped;
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

                // Display the string content as an HTML document. 
                ShowResult(firstIdentifiedPlacemark.BalloonContent);
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.ToString()).SetTitle("Error").Show();
            }
        }

        private void ShowResult(string htmlContent)
        {
            // Display the content in a web view. Note that the web view needs to be re-created each time.
            _sampleLayout.RemoveView(_htmlView);
            _htmlView = new WebView(this)
            {
                LayoutParameters = _layoutParams
            };
            _htmlView.LoadData(htmlContent, "text/html", "UTF-8");
            _sampleLayout.AddView(_htmlView);
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            _sampleLayout = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical
            };

            // Configuration for having the mapview and webview fill the screen.
            _layoutParams = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent,
                1.0f
            );

            // Create and add the webview
            _htmlView = new WebView(this)
            {
                LayoutParameters = _layoutParams
            };

            _myMapView.LayoutParameters = _layoutParams;

            // Create and add a help label
            TextView helpLabel = new TextView(this)
            {
                Text = "Tap to identify features."
            };
            helpLabel.SetTextColor(Color.Black);
            _sampleLayout.AddView(helpLabel);

            // Add the map view to the layout
            _sampleLayout.AddView(_myMapView);
            _sampleLayout.AddView(_htmlView);

            // Make the background white to hide the flash when the webview is removed/re-created.
            _sampleLayout.SetBackgroundColor(Color.White);

            // Show the layout in the app
            SetContentView(_sampleLayout);
        }
    }
}
