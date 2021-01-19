// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Webkit;
using ArcGISRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Linq;

namespace ArcGISRuntimeXamarin.Samples.IdentifyKmlFeatures
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Identify KML features",
        category: "Layers",
        description: "Show a callout with formatted content for a KML feature.",
        instructions: "Tap a feature to identify it. Feature information will be displayed in a callout.",
        tags: new[] { "KML", "KMZ", "Keyhole", "NOAA", "NWS", "OGC", "weather" })]
    [ArcGISRuntime.Samples.Shared.Attributes.AndroidLayout("IdentifyKMLFeatures.axml")]
    public class IdentifyKmlFeatures : Activity
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private WebView _htmlView;

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
            _myMapView.Map = new Map(BasemapStyle.ArcGISDarkGray);

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
                _htmlView.LoadDataWithBaseURL(string.Empty, firstIdentifiedPlacemark.BalloonContent, "text/html", "UTF-8", null);
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.ToString()).SetTitle("Error").Show();
            }
        }

        private void CreateLayout()
        {
            // Load the layout from the axml resource.
            SetContentView(Resource.Layout.IdentifyKMLFeatures);

            _myMapView = FindViewById<MapView>(Resource.Id.mapView);
            _htmlView = FindViewById<WebView>(Resource.Id.webView);
        }
    }
}