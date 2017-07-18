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
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.FindAddress
{
    [Register("FindAddress")]
    public class FindAddress : UIViewController
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

        private Uri _serviceUri = new Uri("https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer");

        // Create UI elements
        private MapView _myMapView = new MapView();

        private UISearchBar _addressSearchBar = new UISearchBar();
        private UIButton _suggestButton = new UIButton();

        public FindAddress()
        {
            Title = "Find address";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            // Set up the visual frame for the MapView and search bar.
            // Ensures that the layout is updated appropriately on rotation
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
            _addressSearchBar.Frame = new CoreGraphics.CGRect(0, TopLayoutGuide.Length, View.Bounds.Width, 44);

            base.ViewDidLayoutSubviews();
        }

        /// <summary>
        /// Creates the initial layout for the app
        /// </summary>
		private void CreateLayout()
        {
            // Add MapView  & search bar to the view
            View.AddSubviews(_myMapView);
            View.AddSubview(_addressSearchBar);

            // Configure the search bar to support search-as-you-type
            _addressSearchBar.TextChanged += _AddressSearch_TextChanged;

            // Configure the search bar to support popover address suggestion
            _addressSearchBar.ShowsSearchResultsButton = true;
            _addressSearchBar.ListButtonClicked += _addressSearch_ListButtonClicked;

            // Disable user interaction until the geocoder is ready
            _addressSearchBar.UserInteractionEnabled = false;

			// Enable tap-for-info pattern on results
			_myMapView.GeoViewTapped += _myMapView_GeoViewTapped;
        }

        private async void Initialize()
        {
            // Populate map view with new map instance
            Map myMap = new Map(Basemap.CreateImageryWithLabels());
            _myMapView.Map = myMap;

            // Initialize the geocoder with the provided service Uri
            _geocoder = await LocatorTask.CreateAsync(_serviceUri);

            // Enable controls now that the geocoder is ready
            _addressSearchBar.UserInteractionEnabled = true;
        }

        private async void _AddressSearch_TextChanged(object sender, UISearchBarTextChangedEventArgs e)
        {
            UpdateSearch();
        }

        private async void UpdateSearch()
        {
            String enteredText = _addressSearchBar.Text;

            // Clear existing marker
            _myMapView.GraphicsOverlays.Clear();

            // Return gracefully if the textbox is empty or the geocoder isn't ready
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
            Graphic point = await GraphicForPoint(addresses[0].DisplayLocation);

            // Show the marker on the map
            resultOverlay.Graphics.Add(point);
            _myMapView.GraphicsOverlays.Add(resultOverlay);

            // Update the map extent to show the marker
            await _myMapView.SetViewpointGeometryAsync(addresses[0].Extent);
        }

        /// <summary>
        /// Returns a graphic for use in an overlay corresponding with a search result
        /// </summary>
        private async Task<Graphic> GraphicForPoint(MapPoint point)
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

        private void _addressSearch_ListButtonClicked(object sender, EventArgs e)
        {
            // Create the alert view
            UIAlertController alert = UIAlertController.Create("Suggestions", "Location searches to try", UIAlertControllerStyle.Alert);

            // Populate the view with one action per address suggestion
            foreach (string address in _addresses)
            {
                alert.AddAction(UIAlertAction.Create(address, UIAlertActionStyle.Default, (obj) =>
                {
                    _addressSearchBar.Text = address;
                    UpdateSearch();
                }));
            }

            // Show the alert view
            PresentViewController(alert, true, null);
        }

        /// <summary>
        /// Handle tap event on the map; displays callouts showing the address for a tapped search result
        /// </summary>
        private async void _myMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Search for the graphics underneath the user's tap
            IReadOnlyList<IdentifyGraphicsOverlayResult> results = await _myMapView.IdentifyGraphicsOverlaysAsync(e.Position, 12, false);

            // Return gracefully if there was no result
            if (results.Count < 1 || results.First().Graphics.Count < 1) { return; }

            // Reverse geocode to get addresses
            IReadOnlyList<GeocodeResult> addresses = await _geocoder.ReverseGeocodeAsync(e.Location);

            // Format addresses
            GeocodeResult address = addresses.First();
            String calloutTitle = address.Attributes["City"] + ", " + address.Attributes["Region"];
            String calloutDetail = address.Attributes["MetroArea"].ToString();

            // Display the Callout
            MapPoint point = _myMapView.ScreenToLocation(e.Position);
            _myMapView.ShowCalloutAt(point, new CalloutDefinition(calloutTitle, calloutDetail));
        }
    }
}