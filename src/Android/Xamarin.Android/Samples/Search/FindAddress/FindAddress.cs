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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;

namespace ArcGISRuntime.Samples.FindAddress
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Find address",
        "Search",
        "This sample demonstrates how you can use the LocatorTask API to geocode an address and display it with a pin on the map. Tapping the pin displays the reverse-geocoded address in a callout.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.EmbeddedResource(@"PictureMarkerSymbols\pin_star_blue.png")]
    public class FindAddress : Activity
    {
        // Addresses for suggestion
        private string[] _addresses = {
            "277 N Avenida Caballeros, Palm Springs, CA",
            "380 New York St, Redlands, CA 92373",
            "Београд",
            "Москва",
            "北京"
        };

        // The LocatorTask provides geocoding services via a service
        private LocatorTask _geocoder;

        // Service Uri to be provided to the LocatorTask (geocoder)
        private Uri _serviceUri = new Uri("https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer");

        // UI Elements
        private MapView _myMapView = new MapView();

        private EditText _addressSearchBar;
        private Button _suggestButton;
        private Button _searchButton;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Find address";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateImagery());

            // Provide Map to the MapView
            _myMapView.Map = myMap;

            // Wire up the map view to support tapping on address markers
            _myMapView.GeoViewTapped += _myMapView_GeoViewTapped;

            // Initialize the LocatorTask with the provided service Uri
            _geocoder = await LocatorTask.CreateAsync(_serviceUri);

            // Enable interaction now that the geocoder is ready
            _suggestButton.Enabled = true;
            _addressSearchBar.Enabled = true;
            _searchButton.Enabled = true;
        }

        private void CreateLayout()
        {
            //initialize the layout
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };
            var searchBarLayout = new RelativeLayout(this);
            // Add the search bar
            _addressSearchBar = new EditText(this);

            layout.AddView(searchBarLayout);
            searchBarLayout.AddView(_addressSearchBar);
            // Add a search button
            _searchButton = new Button(this) { Text = "Search" };
            searchBarLayout.AddView(_searchButton);
            // Add a suggestion button
            _suggestButton = new Button(this) { Text = "Suggest" };
            layout.AddView(_suggestButton);
            // Add the MapView to the layout
            layout.AddView(_myMapView);
            var x = (RelativeLayout.LayoutParams)_searchButton.LayoutParameters;
            x.AddRule(LayoutRules.AlignParentEnd);
            var y = (RelativeLayout.LayoutParams)_addressSearchBar.LayoutParameters;
            y.AddRule(LayoutRules.AlignParentStart);
            // Keep the search bar from overflowing into multiple lines
            _addressSearchBar.SetMaxLines(1);
            // Show the layout in the app
            SetContentView(layout);

            // Disable the buttons and search bar until the geocoder is ready
            _suggestButton.Enabled = false;
            _addressSearchBar.Enabled = false;
            _searchButton.Enabled = false;

            // Hook up the UI event handlers for suggestion & search
            _suggestButton.Click += _searchHintButton_Click;
            _searchButton.Click += SearchButton_Click;
        }

        /// <summary>
        /// Provide address suggestions
        /// </summary>
        private void _searchHintButton_Click(object sender, EventArgs e)
        {
            // Create an AlertDialog
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            // Provide the addresses; lambda updates the search with the selected item
            builder.SetTitle("Suggestions").SetItems(_addresses, (_sender, _e) =>
            {
                var address = _addresses[_e.Which]; // get the selected address
                _addressSearchBar.Text = address;
                updateSearch();
            });
            // Show the dialog
            AlertDialog alert = builder.Create();
            alert.Show();
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            updateSearch();
        }

        private async void updateSearch()
        {
            // Get the text in the search bar
            String enteredText = _addressSearchBar.Text;

            // Clear existing marker
            _myMapView.GraphicsOverlays.Clear();

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
            _myMapView.GraphicsOverlays.Add(resultOverlay);

            // Update the map extent to show the marker
            await _myMapView.SetViewpointGeometryAsync(addresses.First().Extent);
        }

        /// <summary>
        /// Creates and returns a Graphic associated with the given MapPoint
        /// </summary>
        private async Task<Graphic> GraphicForPoint(MapPoint point)
        {
            // Get current assembly that contains the image
            var currentAssembly = Assembly.GetExecutingAssembly();

            // Get image as a stream from the resources
            // Picture is defined as EmbeddedResource and DoNotCopy
            var resourceStream = currentAssembly.GetManifestResourceStream(
                "ArcGISRuntime.Resources.PictureMarkerSymbols.pin_star_blue.png");

            // Create new symbol using asynchronous factory method from stream
            PictureMarkerSymbol pinSymbol = await PictureMarkerSymbol.CreateAsync(resourceStream);
            pinSymbol.Width = 60;
            pinSymbol.Height = 60;
            // The image is a pin; offset the image so that the pinpoint
            //     is on the point rather than the image's true center
            pinSymbol.LeaderOffsetX = 30;
            pinSymbol.OffsetY = 14;
            return new Graphic(point, pinSymbol);
        }

        /// <summary>
        /// Responds to map-tapped events to provide callouts for markers
        /// </summary>
        private async void _myMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Search for the graphics underneath the user's tap
            IReadOnlyList<IdentifyGraphicsOverlayResult> results = await _myMapView.IdentifyGraphicsOverlaysAsync(e.Position, 12, false);

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
            MapPoint point = _myMapView.ScreenToLocation(e.Position);

            // Define the callout
            CalloutDefinition calloutBody = new CalloutDefinition(calloutTitle, calloutDetail);

            // Show the callout on the map at the tapped location
            _myMapView.ShowCalloutAt(point, calloutBody);
        }
    }
}