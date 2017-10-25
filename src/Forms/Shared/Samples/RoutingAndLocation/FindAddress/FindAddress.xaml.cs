﻿// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.UI;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.FindAddress
{
    public partial class FindAddress : ContentPage
    {
        // Addresses for suggestion
        private string[] _addresses = {
            "277 N Avenida Caballeros, Palm Springs, CA",
            "380 New York St, Redlands, CA 92373",
            "Београд",
            "Москва",
            "北京"
        };

        // The LocatorTask provides geocoding services
        private LocatorTask _geocoder;

        // Service Uri to be provided to the LocatorTask (geocoder)
        private Uri _serviceUri = new Uri("https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer");

        public FindAddress()
        {
            InitializeComponent();

            Title = "Find Address";

            // Create the UI, setup the control references and execute initialization
            Initialize();
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        private async void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateImageryWithLabels());

            // Assign the map to the MapView
            MyMapView.Map = myMap;

            // Initialize the LocatorTask with the provided service Uri
            _geocoder = await LocatorTask.CreateAsync(_serviceUri);

            // Enable the UI controls now that the LocatorTask is ready
            MySuggestButton.IsEnabled = true;
            MySearchBar.IsEnabled = true;
        }

        private void Handle_TextChanged(object sender, System.EventArgs e)
        {
            updateSearch();
        }

        private async void updateSearch()
        {
            // Get the text in the search bar
            String enteredText = MySearchBar.Text;

            // Clear existing marker
            MyMapView.GraphicsOverlays.Clear();

            // Return gracefully if the textbox is empty or the geocoder isn't ready
            if (string.IsNullOrWhiteSpace(enteredText) || _geocoder == null) { return; }

            // Get suggestions based on the input text
            IReadOnlyList<SuggestResult> suggestions = await _geocoder.SuggestAsync(enteredText);

            // Stop gracefully if there are no suggestions
            if (suggestions.Count < 1) { return; }

            // Get the full address for the first suggestion
            SuggestResult firstSuggestion = suggestions.First();
            IReadOnlyList<GeocodeResult> addresses = await _geocoder.GeocodeAsync(firstSuggestion.Label);

            // Stop gracefully if the geocoder does not return a result
            if (addresses.Count < 1) { return; }

            // Place a marker on the map - 1. Create the overlay
            GraphicsOverlay resultOverlay = new GraphicsOverlay();
            // 2. Get the Graphic to display
            Graphic point = await GraphicForPoint(addresses.First().DisplayLocation);
            // 3. Add the Graphic to the GraphicsOverlay
            resultOverlay.Graphics.Add(point);
            // 4. Add the GraphicsOverlay to the MapView
            MyMapView.GraphicsOverlays.Add(resultOverlay);

            // Update the map extent to show the marker
            await MyMapView.SetViewpointGeometryAsync(addresses.First().Extent);
        }

        /// <summary>
        /// Creates and returns a Graphic associated with the given MapPoint
        /// </summary>
		private async Task<Graphic> GraphicForPoint(MapPoint point)
        {
#if WINDOWS_UWP
            // Get current assembly that contains the image
            var currentAssembly = GetType().GetTypeInfo().Assembly;
#else
            // Get current assembly that contains the image
            var currentAssembly = Assembly.GetExecutingAssembly();
#endif

            // Get image as a stream from the resources
            // Picture is defined as EmbeddedResource and DoNotCopy
            var resourceStream = currentAssembly.GetManifestResourceStream(
                "ArcGISRuntimeXamarin.Resources.PictureMarkerSymbols.pin_star_blue.png");

            // Create new symbol using asynchronous factory method from stream
            PictureMarkerSymbol pinSymbol = await PictureMarkerSymbol.CreateAsync(resourceStream);
            pinSymbol.Width = 60;
            pinSymbol.Height = 60;
            // The image is a pin; offset the image so that the pinpoint
            //     is on the point rather than the image's true center
            pinSymbol.OffsetX = pinSymbol.Width / 2;
            pinSymbol.OffsetY = pinSymbol.Height / 2;
            return new Graphic(point, pinSymbol);
        }

        private async void SuggestionButtonTapped(object sender, System.EventArgs e)
        {
            // Display the list of suggestions; returns the selected option
            String action = await DisplayActionSheet("Choose an address to geocode", "Cancel", null, _addresses);
            // Update the search
            MySearchBar.Text = action;
            updateSearch();
        }

        /// <summary>
        /// Handle tap event on the map; displays callouts showing the address for a tapped search result
        /// </summary>
        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.Xamarin.Forms.GeoViewInputEventArgs e)
        {
            // Search for the graphics underneath the user's tap
            IReadOnlyList<IdentifyGraphicsOverlayResult> results = await MyMapView.IdentifyGraphicsOverlaysAsync(e.Position, 12, false);

            // Return gracefully if there was no result
            if (results.Count < 1 || results.First().Graphics.Count < 1) { return; }

            // Reverse geocode to get addresses
            IReadOnlyList<GeocodeResult> addresses = await _geocoder.ReverseGeocodeAsync(e.Location);

            // Get the first result
            GeocodeResult address = addresses.First();
            // Use the city and region for the Callout Title
            String calloutTitle = address.Attributes["City"] + ", " + address.Attributes["Region"];
            // Use the metro area for the Callout Detail
            String calloutDetail = address.Attributes["MetroArea"].ToString();

            // Use the MapView to convert from the on-screen location to the on-map location
            MapPoint point = MyMapView.ScreenToLocation(e.Position);

            // Define the callout
            CalloutDefinition calloutBody = new CalloutDefinition(calloutTitle, calloutDetail);

            // Show the callout on the map at the tapped location
            MyMapView.ShowCalloutAt(point, calloutBody);
        }
    }
}