// Copyright 2017 Esri.
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
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ArcGISRuntime.Samples.FindPlace
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Find place",
        "Search",
        "This sample demonstrates how to use geocode functionality to search for points of interest, around a location or within an extent.",
        "1. Enter a point of interest you'd like to search for (e.g. 'Starbucks')\n2. Enter a search location or accept the default 'Current Location'\n3. Select 'search all' to get all results, or press 'search view' to only get results within the current extent.")]
    [ArcGISRuntime.Samples.Shared.Attributes.EmbeddedResource(@"PictureMarkerSymbols\pin_star_blue.png")]
    public partial class FindPlace : ContentPage
    {
        // The LocatorTask provides geocoding services
        private LocatorTask _geocoder;

        // Service Uri to be provided to the LocatorTask (geocoder)
        private Uri _serviceUri = new Uri("https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer");

        public FindPlace()
        {
            InitializeComponent();

            Title = "Find place";

            // Create the UI, setup the control references and execute initialization
            Initialize();
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        private async void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateStreets());

            // Assign the map to the MapView
            MyMapView.Map = myMap;

            // Subscribe to location changed event so that map can zoom to location
            MyMapView.LocationDisplay.LocationChanged += LocationDisplay_LocationChanged;

            // Enable location display
            MyMapView.LocationDisplay.IsEnabled = true;

            // Initialize the LocatorTask with the provided service Uri
            _geocoder = await LocatorTask.CreateAsync(_serviceUri);

            // Enable all controls now that the locator task is ready
            MySearchBox.IsEnabled = true;
            MyLocationBox.IsEnabled = true;
            MySearchButton.IsEnabled = true;
            MySearchRestrictedButton.IsEnabled = true;
        }

        private void LocationDisplay_LocationChanged(object sender, Esri.ArcGISRuntime.Location.Location e)
        {
            // Return if position is null; event is raised with null location after
            if (e.Position == null) { return; }

            // Unsubscribe from further events; only want to zoom to location once
            ((LocationDisplay)sender).LocationChanged -= LocationDisplay_LocationChanged;

            // Need to use this to interact with UI elements because this function is called from a background thread
            Device.BeginInvokeOnMainThread(() =>
            {
                MyMapView.SetViewpoint(new Viewpoint(e.Position, 100000));
            });
        }

        /// <summary>
        /// Gets the map point corresponding to the text in the location textbox.
        /// If the text is 'Current Location', the returned map point will be the device's location.
        /// </summary>
        /// <param name="locationText"></param>
        /// <returns></returns>
        private async Task<MapPoint> GetSearchMapPoint(string locationText)
        {
            // Get the map point for the search text
            if (locationText != "Current Location")
            {
                // Geocode the location
                IReadOnlyList<GeocodeResult> locations = await _geocoder.GeocodeAsync(locationText);

                // return if there are no results
                if (locations.Count() < 1) { return null; }

                // Get the first result
                GeocodeResult result = locations.First();

                // Return the map point
                return result.DisplayLocation;
            }
            else
            {
                // Get the current device location
                return MyMapView.LocationDisplay.Location.Position;
            }
        }

        /// <summary>
        /// Runs a search and populates the map with results based on the provided information
        /// </summary>
        /// <param name="enteredText">Results to search for</param>
        /// <param name="locationText">Location around which to find results</param>
        /// <param name="restrictToExtent">If true, limits results to only those that are within the current extent</param>
        private async void UpdateSearch(string enteredText, string locationText, bool restrictToExtent = false)
        {
            // Clear any existing markers
            MyMapView.GraphicsOverlays.Clear();

            // Return gracefully if the textbox is empty or the geocoder isn't ready
            if (string.IsNullOrWhiteSpace(enteredText) || _geocoder == null) { return; }

            // Create the geocode parameters
            GeocodeParameters parameters = new GeocodeParameters();

            // Get the MapPoint for the current search location
            MapPoint searchLocation = await GetSearchMapPoint(locationText);

            // Update the geocode parameters if the map point is not null
            if (searchLocation != null)
            {
                parameters.PreferredSearchLocation = searchLocation;
            }

            // Update the search area if desired
            if (restrictToExtent)
            {
                // Get the current map extent
                Geometry extent = MyMapView.VisibleArea;

                // Update the search parameters
                parameters.SearchArea = extent;
            }

            // Show the progress bar
            MyProgressBar.IsVisible = true;

            // Get the location information
            IReadOnlyList<GeocodeResult> locations = await _geocoder.GeocodeAsync(enteredText, parameters);

            // Stop gracefully and show a message if the geocoder does not return a result
            if (locations.Count < 1)
            {
                MyProgressBar.IsVisible = false; // 1. Hide the progress bar
                ShowStatusMessage("No results found"); // 2. Show a message
                return; // 3. Stop
            }

            // Create the GraphicsOverlay so that results can be drawn on the map
            GraphicsOverlay resultOverlay = new GraphicsOverlay();

            // Add each address to the map
            foreach (GeocodeResult location in locations)
            {
                // Get the Graphic to display
                Graphic point = await GraphicForPoint(location.DisplayLocation);

                // Add the specific result data to the point
                point.Attributes["Match_Title"] = location.Label;

                // Get the address for the point
                IReadOnlyList<GeocodeResult> addresses = await _geocoder.ReverseGeocodeAsync(location.DisplayLocation);

                // Add the first suitable address if possible
                if (addresses.Count() > 0)
                {
                    point.Attributes["Match_Address"] = addresses.First().Label;
                }

                // Add the Graphic to the GraphicsOverlay
                resultOverlay.Graphics.Add(point);
            }

            // Hide the progress bar
            MyProgressBar.IsVisible = false;

            // Add the GraphicsOverlay to the MapView
            MyMapView.GraphicsOverlays.Add(resultOverlay);

            // Create a viewpoint for the extent containing all graphics
            Viewpoint viewExtent = new Viewpoint(resultOverlay.Extent);

            // Update the map viewpoint
            MyMapView.SetViewpoint(viewExtent);
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
        /// Shows a callout for any tapped graphics
        /// </summary>
        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.Xamarin.Forms.GeoViewInputEventArgs e)
        {
            // Search for the graphics underneath the user's tap
            IReadOnlyList<IdentifyGraphicsOverlayResult> results = await MyMapView.IdentifyGraphicsOverlaysAsync(e.Position, 12, false);

            // Clear callouts and return if there was no result
            if (results.Count < 1 || results.First().Graphics.Count < 1) { MyMapView.DismissCallout(); return; }

            // Get the first graphic from the first result
            Graphic matchingGraphic = results.First().Graphics.First();

            // Get the title; manually added to the point's attributes in UpdateSearch
            String title = matchingGraphic.Attributes["Match_Title"] as String;

            // Get the address; manually added to the point's attributes in UpdateSearch
            String address = matchingGraphic.Attributes["Match_Address"] as String;

            // Define the callout
            CalloutDefinition calloutBody = new CalloutDefinition(title, address);

            // Show the callout on the map at the tapped location
            MyMapView.ShowCalloutAt(e.Location, calloutBody);
        }

        /// <summary>
        /// Returns a list of suggestions based on the input search text and limited by the specified parameters
        /// </summary>
        /// <param name="searchText">Text to get suggestions for</param>
        /// <param name="location">Location around which to look for suggestions</param>
        /// <param name="poiOnly">If true, restricts suggestions to only Points of Interest (e.g. businesses, parks),
        /// rather than all matching results</param>
        /// <returns>List of suggestions as strings</returns>
        private async Task<IEnumerable<String>> GetSuggestResults(string searchText, string location = "", bool poiOnly = false)
        {
            // Quit if string is null, empty, or whitespace
            if (String.IsNullOrWhiteSpace(searchText)) { return null; }

            // Quit if the geocoder isn't ready
            if (_geocoder == null) { return null; }

            // Create geocode parameters
            SuggestParameters parameters = new SuggestParameters();

            // Restrict suggestions to points of interest if desired
            if (poiOnly) { parameters.Categories.Add("POI"); }

            // Set the location for the suggest parameters
            if (!String.IsNullOrWhiteSpace(location))
            {
                // Get the MapPoint for the current search location
                MapPoint searchLocation = await GetSearchMapPoint(location);

                // Update the geocode parameters if the map point is not null
                if (searchLocation != null)
                {
                    parameters.PreferredSearchLocation = searchLocation;
                }
            }

            // Get the updated results from the query so far
            IReadOnlyList<SuggestResult> results = await _geocoder.SuggestAsync(searchText, parameters);

            // Convert the list into a list of strings (corresponding to the label property on each result)
            IEnumerable<String> formattedResults = results.Select(result => result.Label);

            // Return the list
            return formattedResults;
        }

        /// <summary>
        /// Method abstracts the platform-specific message box functionality to maximize re-use of common code
        /// </summary>
        /// <param name="message">Text of the message to show.</param>
        private void ShowStatusMessage(string message)
        {
            // Display the message to the user
            DisplayAlert("Alert", message, "OK");
        }

        /// <summary>
        /// Method used to keep the suggestions up-to-date for the location box
        /// </summary>
        private async void MyLocationBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Dismiss callout, if any
            UserInteracted();

            // Get the current text
            string searchText = MyLocationBox.Text;

            // Get the results
            IEnumerable<String> results = await GetSuggestResults(searchText);

            // Quit if there are no results
            if (results == null || results.Count() == 0) { return; }

            // Get a modifiable list from the results
            List<String> mutableResults = results.ToList();

            // Add a 'current location' option to the list
            mutableResults.Insert(0, "Current Location");

            // Update the list of options
            lstViewSuggestions.ItemsSource = mutableResults;
        }

        /// <summary>
        /// Method used to keep the suggestions up-to-date for the search box
        /// </summary>
        private async void MySearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Dismiss callout, if any
            UserInteracted();

            // Get the current text
            string searchText = MySearchBox.Text;

            // Get the current search location
            string locationText = MyLocationBox.Text;

            // Convert the list into a usable format for the suggest box
            IEnumerable<String> results = await GetSuggestResults(searchText, locationText, true);

            // Quit if there are no results
            if (results == null || results.Count() == 0) { return; }

            // Update the list of options
            lstViewSuggestions.ItemsSource = results;
        }

        /// <summary>
        /// Method called to start a search that is restricted to results within the current extent
        /// </summary>
        private void MySearchRestrictedButton_Clicked(object sender, EventArgs e)
        {
            // Dismiss callout, if any
            UserInteracted();

            // Get the search text
            string searchText = MySearchBox.Text;

            // Get the location text
            string locationText = MyLocationBox.Text;

            // Run the search
            UpdateSearch(searchText, locationText, true);
        }

        /// <summary>
        /// Method called to start an unrestricted search
        /// </summary>
        private void MySearchButton_Clicked(object sender, EventArgs e)
        {
            // Dismiss callout, if any
            UserInteracted();

            // Get the search text
            string searchText = MySearchBox.Text;

            // Get the location text
            string locationText = MyLocationBox.Text;

            // Run the search
            UpdateSearch(searchText, locationText);
        }

        /// <summary>
        /// Show the map and hide suggestions
        /// </summary>
        private void MyLocationBox_Unfocused(object sender, FocusEventArgs e)
        {
            // Dismiss callout, if any
            UserInteracted();

            // Hide the suggestion list
            lstViewSuggestions.IsVisible = false;

            // Show the map view
            MyMapView.IsVisible = true;
        }

        /// <summary>
        /// Hide the map view and show suggestions
        /// </summary>
        private void MySearchBox_Focused(object sender, FocusEventArgs e)
        {
            // Dismiss callout, if any
            UserInteracted();

            // Show the suggestion list
            lstViewSuggestions.IsVisible = true;

            // Hide the map view
            MyMapView.IsVisible = false;
        }

        /// <summary>
        /// Update the appropriate textbox with the selected suggestion
        /// </summary>
        private void lstViewSuggestions_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            // Dismiss callout, if any
            UserInteracted();

            // Get the text of the selected item
            String suggestion = e.SelectedItem.ToString();

            // Update the location search box if it has focus
            if (MyLocationBox.IsFocused)
            {
                MyLocationBox.Text = suggestion;
            }
            else
            {
                // Otherwise, update the search box
                MySearchBox.Text = suggestion;
            }
        }

        /// <summary>
        /// Method to handle hiding the callout, should be called by all UI event handlers
        /// </summary>
        private void UserInteracted()
        {
            // Hide the callout
            MyMapView.DismissCallout();
        }
    }
}