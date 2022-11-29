// Copyright 2021 Esri.
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
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ArcGIS.WPF.Samples.IdentifyKmlFeatures
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Identify KML features",
        category: "Layers",
        description: "Show a callout with formatted content for a KML feature.",
        instructions: "Tap a feature to identify it. Feature information will be displayed in a callout.",
        tags: new[] { "KML", "KMZ", "Keyhole", "NOAA", "NWS", "OGC", "weather" })]
    public partial class IdentifyKmlFeatures
    {
        // Hold a reference to the KML layer for use in identify operations.
        private KmlLayer _forecastLayer;

        // Initial view envelope.
        private readonly Envelope _usEnvelope = new Envelope(-144.619561355187, 18.0328662832097, -66.0903762761083, 67.6390975806745, SpatialReferences.Wgs84);

        public IdentifyKmlFeatures()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Set up the basemap.
            MyMapView.Map = new Map(BasemapStyle.ArcGISDarkGray);

            // Create the dataset.
            KmlDataset dataset = new KmlDataset(new Uri("https://www.wpc.ncep.noaa.gov/kml/noaa_chart/WPC_Day1_SigWx_latest.kml"));

            // Create the layer from the dataset.
            _forecastLayer = new KmlLayer(dataset);

            // Add the layer to the map.
            MyMapView.Map.OperationalLayers.Add(_forecastLayer);

            // Zoom to the extent of the United States.
            MyMapView.SetViewpoint(new Viewpoint(_usEnvelope));

            // Listen for taps to identify features.
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Clear any existing popups.
            MyMapView.DismissCallout();

            try
            {
                // Perform identify on the KML layer and get the results.
                IdentifyLayerResult identifyResult = await MyMapView.IdentifyLayerAsync(_forecastLayer, e.Position, 2, false);

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

                // Create a browser to show the feature popup HTML.
                WebBrowser browser = new WebBrowser
                {
                    Width = 400,
                    Height = 100
                };
                browser.NavigateToString(firstIdentifiedPlacemark.BalloonContent);

                // Create and show the callout.
                MyMapView.ShowCalloutAt(e.Location, browser);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }
    }
}