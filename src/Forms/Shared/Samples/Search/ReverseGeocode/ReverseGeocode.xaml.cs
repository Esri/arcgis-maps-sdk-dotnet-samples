﻿// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.ReverseGeocode
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Reverse geocode",
        "Search",
        "Use an online geocoding service to find the address for a point.",
        "")]
    public partial class ReverseGeocode : ContentPage
    {
        // Service Uri to be provided to the LocatorTask (geocoder).
        private readonly Uri _serviceUri = new Uri("https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer");

        // The LocatorTask provides geocoding services.
        private LocatorTask _geocoder;

        public ReverseGeocode()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            // Create new Map with basemap.
            Map myMap = new Map(Basemap.CreateImageryWithLabels());

            // Provide used Map to the MapView.
            MyMapView.Map = myMap;

            // Add a graphics overlay to the map for showing where the user tapped.
            MyMapView.GraphicsOverlays.Add(new GraphicsOverlay());

            // Enable tap-for-info pattern on results.
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;

            // Initialize the LocatorTask with the provided service Uri.
            try
            {
                _geocoder = await LocatorTask.CreateAsync(_serviceUri);
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Error", e.ToString(), "OK");
            }

            // Set the initial viewpoint.
            await MyMapView.SetViewpointCenterAsync(34.058, -117.195, 5e4);
        }

        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.Xamarin.Forms.GeoViewInputEventArgs e)
        {
            try
            {
                // Clear the existing graphics & callouts.
                MyMapView.DismissCallout();
                MyMapView.GraphicsOverlays[0].Graphics.Clear();

                // Add a graphic for the tapped point.
                Graphic pinGraphic = await GraphicForPoint(e.Location);
                MyMapView.GraphicsOverlays[0].Graphics.Add(pinGraphic);

                // Normalize the geometry - needed if the user crosses the international date line.
                MapPoint normalizedPoint = (MapPoint)GeometryEngine.NormalizeCentralMeridian(e.Location);

                // Reverse geocode to get addresses.
                IReadOnlyList<GeocodeResult> addresses = await _geocoder.ReverseGeocodeAsync(normalizedPoint);

                // Get the first result.
                GeocodeResult address = addresses.First();

                // Use the city and region for the Callout Title.
                string calloutTitle = address.Attributes["Address"].ToString();

                // Use the metro area for the Callout Detail.
                string calloutDetail = address.Attributes["City"] +
                                       " " + address.Attributes["Region"] +
                                       " " + address.Attributes["CountryCode"];

                // Define the callout.
                CalloutDefinition calloutBody = new CalloutDefinition(calloutTitle, calloutDetail);

                // Show the callout on the map at the tapped location.
                MyMapView.ShowCalloutForGeoElement(pinGraphic, e.Position, calloutBody);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await Application.Current.MainPage.DisplayAlert("No results", "No results found", "OK");
            }
        }

        private async Task<Graphic> GraphicForPoint(MapPoint point)
        {
#if WINDOWS_UWP
            // Get current assembly that contains the image.
            Assembly currentAssembly = GetType().GetTypeInfo().Assembly;
#else
            // Get current assembly that contains the image.
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
#endif

            // Get image as a stream from the resources.
            // Picture is defined as EmbeddedResource and DoNotCopy.
            Stream resourceStream = currentAssembly.GetManifestResourceStream(
                "ArcGISRuntime.Resources.PictureMarkerSymbols.pin_star_blue.png");

            // Create new symbol using asynchronous factory method from stream.
            PictureMarkerSymbol pinSymbol = await PictureMarkerSymbol.CreateAsync(resourceStream);
            pinSymbol.Width = 60;
            pinSymbol.Height = 60;
            // The image is a pin; offset the image so that the pinpoint
            //     is on the point rather than the image's true center.
            pinSymbol.LeaderOffsetX = 30;
            pinSymbol.OffsetY = 14;
            return new Graphic(point, pinSymbol);
        }
    }
}