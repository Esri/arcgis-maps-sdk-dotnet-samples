// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using UIKit;
using Foundation;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.Data;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;

namespace ArcGISRuntimeXamarin.Samples.FindAddress
{
	[Register("FindAddress")]
	public class FindAddress : UIViewController
	{
		// Create and hold reference to the used MapView
		private MapView _myMapView = new MapView();
        // Create UI elements
        private UISearchBar _addressSearch = new UISearchBar();
        private UIButton _addressHelperButton = new UIButton();
        private LocatorTask _GeocodeTask;
        // URL pointing to geocoding service
        private Uri _GeocodeServiceUri = new Uri("https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer");

        // List of addresses for use in suggestions
		string[] _addresses = {
			"277 N Avenida Caballeros, Palm Springs, CA",
			"380 New York St, Redlands, CA 92373",
			"Београд",
            "Москва", 
            "北京"
		};
		
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
            // Set up the visual frame for mapview and search bar.
            // Ensures that the layout is updated appropriately upon rotation/relayout
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
            _addressSearch.Frame = new CoreGraphics.CGRect(0, TopLayoutGuide.Length, View.Bounds.Width, 44);

			base.ViewDidLayoutSubviews();
		}

        /// <summary>
        /// Creates the initial layout for the app
        /// </summary>
		private void CreateLayout()
        {
			// Add MapView  & search bar to the view
			View.AddSubviews(_myMapView);
            View.AddSubview(_addressSearch);

            // Configure the search bar to support search-as-you-type
            _addressSearch.TextChanged += _AddressSearch_TextChanged;

            //Configure the search bar to support flyout address suggestion
            _addressSearch.ShowsSearchResultsButton = true;
            _addressSearch.ListButtonClicked += _addressSearch_ListButtonClicked;
   		}

		private async void Initialize()
		{
			// Populate map view with new map instance
			Map myMap = new Map(Basemap.CreateImageryWithLabels());
			_myMapView.Map = myMap;

			// Set up the geocoder
			_GeocodeTask = await Esri.ArcGISRuntime.Tasks.Geocoding.LocatorTask.CreateAsync(_GeocodeServiceUri, null);

			// Enable tap-for-info pattern on results
			_myMapView.GeoViewTapped += _myMapView_GeoViewTapped;
		}


		async void _AddressSearch_TextChanged(object sender, UISearchBarTextChangedEventArgs e)
        {
            // Remove any old results
            _myMapView.GraphicsOverlays.Clear();

            // Return if the search hasn't returned any results
            if (String.IsNullOrWhiteSpace(e.SearchText)) { return; }

            // Get suggestions from the geocoding service
            IReadOnlyList<SuggestResult> suggestions = await _GeocodeTask.SuggestAsync(e.SearchText);
            if (suggestions.Count > 0) 
            {
                // Get the location for the first suggestion
                IReadOnlyList<GeocodeResult> locations = await _GeocodeTask.GeocodeAsync(suggestions[0]);

                // Return gracefully if there are no results
                if (locations.Count == 0) { return; }

                // Set up a graphics overlay 
				GraphicsOverlay resultOverlay = new GraphicsOverlay();
                Graphic point = await _graphicForPoint(locations[0].DisplayLocation);

                // Record the address with the overlay for easy recall when the graphic is tapped
                point.Attributes.Add("Address", locations[0].Label);
                resultOverlay.Graphics.Add(point);
				_myMapView.GraphicsOverlays.Add(resultOverlay);
                _myMapView.SetViewpointGeometryAsync(locations[0].Extent);
            }

        }

        /// <summary>
        /// Returns a graphic for use in an overlay corresponding with a search result
        /// </summary>
        /// <returns>Graphic/Symbol ready for use in a graphicsOverlay</returns>
        /// <param name="point">MapPoint representing the graphic's map location</param>
        private async Task<Graphic> _graphicForPoint(MapPoint point) {
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

        void _addressSearch_ListButtonClicked(object sender, EventArgs e)
        {
            // Create the alert view
			UIAlertController alert = UIAlertController.Create("Suggestions", "Location searches to try", UIAlertControllerStyle.Alert);

            // Populate the view with one action per address suggestion
			foreach (string address in _addresses)
			{
                alert.AddAction(UIAlertAction.Create(address, UIAlertActionStyle.Default, (obj) => { 
                    _addressSearch.Text = address; 
                    _AddressSearch_TextChanged(this, new UISearchBarTextChangedEventArgs(address));
                }));
			}

            // Show the alert view
			PresentViewController(alert, true, null);
        }

        /// <summary>
        /// Handle tap event on the map; displays callouts showing the address for a tapped search result
        /// </summary>
        async void _myMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
			// Search for the graphics underneath the user's tap
			IReadOnlyList<IdentifyGraphicsOverlayResult> results = await _myMapView.IdentifyGraphicsOverlaysAsync(e.Position, 12, false);

			// Return gracefully if there was no result
			if (results.Count == 0) { return; }

			// Reverse geocode to get addresses
			IReadOnlyList<GeocodeResult> addresses = await _GeocodeTask.ReverseGeocodeAsync(e.Location);

			// Format addresses
			GeocodeResult address = addresses.First();
			String calloutTitle = $"{address.Attributes["City"]}, {address.Attributes["Region"]}";
			String calloutDetail = $"{address.Attributes["MetroArea"]}";

			// Display the callout
			if (results[0].Graphics.Count > 0)
			{
				MapPoint point = _myMapView.ScreenToLocation(e.Position);
				_myMapView.ShowCalloutAt(point, new CalloutDefinition(calloutTitle, calloutDetail));
			}
        }
    }
}