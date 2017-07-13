// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Data;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace ArcGISRuntimeXamarin.Samples.FindAddress
{
    public partial class FindAddress : Xamarin.Forms.ContentPage
    {
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
            InitializeComponent();

            Title = "Find Address";

            // Create the UI, setup the control references and execute initialization 
            Initialize();
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        private async void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateImagery());

            // Assign the map to the MapView
            MyMapView.Map = myMap;

            // Set up the geocoder
            _GeocodeTask = await Esri.ArcGISRuntime.Tasks.Geocoding.LocatorTask.CreateAsync(_GeocodeServiceUri, null);
        }

        async void Handle_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            MyMapView.GraphicsOverlays.Clear();
            // Gracefully handle empty search query
            if (String.IsNullOrWhiteSpace(e.NewTextValue)) { return; }

            // Get the first suggestion based on the input text
            IReadOnlyList<SuggestResult> suggestions = await _GeocodeTask.SuggestAsync(e.NewTextValue);

            // Gracefully stop if there were no results
            if (suggestions.Count < 1) { return; }

            // Geocode the first suggestion result
            IReadOnlyList<GeocodeResult> locations = await _GeocodeTask.GeocodeAsync(suggestions[0].Label);

            // Gracefully stop if there were no results
            if (locations.Count < 1) { return; }

			// Set up a graphics overlay 
			GraphicsOverlay resultOverlay = new GraphicsOverlay();
			Graphic point = await _graphicForPoint(locations[0].DisplayLocation);

			// Record the address with the overlay for easy recall when the graphic is tapped
			point.Attributes.Add("Address", locations[0].Label);
			resultOverlay.Graphics.Add(point);
			MyMapView.GraphicsOverlays.Add(resultOverlay);
			MyMapView.SetViewpointGeometryAsync(locations[0].Extent);
        }

		/// <summary>
		/// Returns a graphic for use in an overlay corresponding with a search result
		/// </summary>
		/// <returns>Graphic/Symbol ready for use in a graphicsOverlay</returns>
		/// <param name="point">MapPoint representing the graphic's map location</param>
		private async Task<Graphic> _graphicForPoint(MapPoint point)
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
				"ArcGISRuntimeXamarin.Resources.PictureMarkerSymbols.pin_star_red.png");

			// Create new symbol using asynchronous factory method from stream
			PictureMarkerSymbol pinSymbol = await PictureMarkerSymbol.CreateAsync(resourceStream);
			pinSymbol.Width = 15;
			pinSymbol.Height = 30;
			pinSymbol.OffsetX = pinSymbol.Width / 2;
			pinSymbol.OffsetY = pinSymbol.Height / 2;
			return new Graphic(point, pinSymbol);
		}

        async void Handle_Clicked(object sender, System.EventArgs e)
        {
            String action = await DisplayActionSheet("Choose an address to geocode", "Cancel", null, _addresses);
            String oldValue = MySearchBar.Text;
            MySearchBar.Text = action;
            Handle_TextChanged(this, new Xamarin.Forms.TextChangedEventArgs(oldValue, action));
        }

		/// <summary>
		/// Handle tap event on the map; displays callouts showing the address for a tapped search result
		/// </summary>
        async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.Xamarin.Forms.GeoViewInputEventArgs e)
        {
			// Search for the graphics underneath the user's tap
			IReadOnlyList<IdentifyGraphicsOverlayResult> results = await MyMapView.IdentifyGraphicsOverlaysAsync(e.Position, 12, false);

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
				MapPoint point = MyMapView.ScreenToLocation(e.Position);
				MyMapView.ShowCalloutAt(point, new CalloutDefinition(calloutTitle, calloutDetail));
			}
        }
    }
}
