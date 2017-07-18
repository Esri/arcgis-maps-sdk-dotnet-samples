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

namespace ArcGISRuntimeXamarin.Samples.FindAddress
{
    [Activity]
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

        private Uri _serviceUri = new Uri("https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer");

        // UI Elements
        private MapView _myMapView;
        private EditText _addressSearchBar;
        private Button _suggestButton;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Find Address";

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
        }

        private void CreateLayout()
        {
            //initialize the layout
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };
            var searchBarLayout = new RelativeLayout(this);
            _addressSearchBar = new EditText(this);
            _suggestButton = new Button(this) { Text = "Suggest" };
            layout.AddView(searchBarLayout);
            searchBarLayout.AddView(_addressSearchBar);
            searchBarLayout.AddView(_suggestButton);
			// Create the MapView
			_myMapView = new MapView();
            layout.AddView(_myMapView);
            var x = (RelativeLayout.LayoutParams)_suggestButton.LayoutParameters;
            x.AddRule(LayoutRules.AlignParentEnd);
            var y = (RelativeLayout.LayoutParams)_addressSearchBar.LayoutParameters;
            y.AddRule(LayoutRules.AlignParentStart);
            _addressSearchBar.SetMaxLines(1);
            // Show the layout in the app
            SetContentView(layout);

            // Disable the buttons and search bar until the geocoder is ready
            _suggestButton.Enabled = false;
            _addressSearchBar.Enabled = false;

            // Hook up the UI event handlers for suggestion & search
            _suggestButton.Click += _searchHintButton_Click;
            _addressSearchBar.TextChanged += _searchBar_TextChanged;

        }

        /// <summary>
        /// Provide address suggestions
        /// </summary>
        private void _searchHintButton_Click(object sender, EventArgs e)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle("Suggestions")
                   .SetItems(_addresses, (_sender, _e) =>
            {
                var address = _addresses[_e.Which]; // get the selected address
                _addressSearchBar.Text = address;
                updateSearch();
            });

            AlertDialog alert = builder.Create();
            alert.Show();
        }

        private void _searchBar_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            updateSearch();
        }

        private async void updateSearch()
        {
            var enteredText = _addressSearchBar.Text;

            // Clear existing marker
            _myMapView.GraphicsOverlays.Clear();

            // Return gracefully if the textbox is empty
            if (string.IsNullOrWhiteSpace(enteredText) || _geocoder == null) { return; }

            // Get the nearest suggestion to entered text
            IReadOnlyList<SuggestResult> suggestions = await _geocoder.SuggestAsync(enteredText);

            // Stop gracefully if there are no suggestions
            if (suggestions.Count < 1) { return; }

            // Get the full address for the first suggestion
            IReadOnlyList<GeocodeResult> addresses = await _geocoder.GeocodeAsync(suggestions[0].Label);

            // Stop gracefully if the geocoder does not return a result
            if (addresses.Count < 1) { return; }

            // Place a marker on the map
            GraphicsOverlay resultOverlay = new GraphicsOverlay();
            Graphic point = await _graphicForPoint(addresses[0].DisplayLocation);

            // Record the address with the overlay for easy recall when the graphic is tapped
            point.Attributes.Add("Address", addresses[0].Label);
            resultOverlay.Graphics.Add(point);
            _myMapView.GraphicsOverlays.Add(resultOverlay);
            await _myMapView.SetViewpointGeometryAsync(addresses[0].Extent);
        }

        /// <summary>
        /// Creates a graphic for the specified map point asynchronously
        /// </summary>
        private async Task<Graphic> _graphicForPoint(MapPoint point)
        {
            // Get current assembly that contains the image
            var currentAssembly = Assembly.GetExecutingAssembly();

            // Get image as a stream from the resources
            // Picture is defined as EmbeddedResource and DoNotCopy
            var resourceStream = currentAssembly.GetManifestResourceStream(
                "ArcGISRuntimeXamarin.Resources.PictureMarkerSymbols.pin_star_red.png");

            // Create new symbol using asynchronous factory method from stream
            PictureMarkerSymbol pinSymbol = await PictureMarkerSymbol.CreateAsync(resourceStream);
            pinSymbol.Width = 15;
            pinSymbol.Height = 30;
            pinSymbol.OffsetX = pinSymbol.Width / 2;
            pinSymbol.OffsetY = pinSymbol.Height / 2;
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
            if (results.Count == 0) { return; }

            // Reverse geocode to get addresses
            IReadOnlyList<GeocodeResult> addresses = await _geocoder.ReverseGeocodeAsync(e.Location);

            // Format addresses
            GeocodeResult address = addresses.First();
            String calloutTitle = address.Attributes["City"] + ", " + address.Attributes["Region"];
            String calloutDetail = address.Attributes["MetroArea"].ToString();

            // Display the callout
            if (results[0].Graphics.Count > 0)
            {
                MapPoint point = _myMapView.ScreenToLocation(e.Position);
                _myMapView.ShowCalloutAt(point, new CalloutDefinition(calloutTitle, calloutDetail));
            }
        }
    }
}