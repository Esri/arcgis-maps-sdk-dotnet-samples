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

namespace ArcGISRuntime.Samples.FindAddress
{
    [Register("FindAddress")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Find address",
        "Search",
        "This sample demonstrates how you can use the LocatorTask API to geocode an address and display it with a pin on the map. Tapping the pin displays the reverse-geocoded address in a callout.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.EmbeddedResource(@"PictureMarkerSymbols\pin_star_blue.png")]
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

        // Service Uri to be provided to the LocatorTask (geocoder)
        private Uri _serviceUri = new Uri("https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer");

        // Create the MapView
        private MapView _myMapView = new MapView();

        // Create UI elements
        private UISearchBar _addressSearchBar = new UISearchBar();

        private UIButton _suggestButton = new UIButton();

        public FindAddress()
        {
            Title = "Find address";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references, and execute initialization
            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            // Set up the visual frame for the MapView and search bar
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

            // Configure the search bar to support search
            _addressSearchBar.SearchButtonClicked += _addressSearchBar_Clicked;

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
            // Get a new instance of the Imagery with Labels basemap
            Basemap _basemap = Basemap.CreateImageryWithLabels();

            // Create a new Map with the basemap
            Map myMap = new Map(_basemap);

            // Populate the MapView with the Map
            _myMapView.Map = myMap;

            // Initialize the geocoder with the provided service Uri
            _geocoder = await LocatorTask.CreateAsync(_serviceUri);

            // Enable controls now that the geocoder is ready
            _addressSearchBar.UserInteractionEnabled = true;
        }

        private void _addressSearchBar_Clicked(object sender, EventArgs e)
        {
            UpdateSearch();
            // Dismiss the keyboard
            _addressSearchBar.ResignFirstResponder();
        }

        private async void UpdateSearch()
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