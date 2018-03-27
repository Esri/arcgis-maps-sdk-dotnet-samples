// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ArcGISRuntime.Samples.FindPlace
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Find place",
        "Search",
        "This sample demonstrates how to use geocode functionality to search for points of interest, around a location or within an extent.",
        "1. Enter a point of interest you'd like to search for (e.g. 'Starbucks')\n2. Enter a search location or accept the default 'Current Location'\n3. Select 'search all' to get all results, or press 'search view' to only get results within the current extent.")]
    [ArcGISRuntime.Samples.Shared.Attributes.EmbeddedResource(@"PictureMarkerSymbols\pin_star_blue.png")]
    public class FindPlace : Activity
    {
        // The LocatorTask provides geocoding services via a service
        private LocatorTask _geocoder;

        // Service Uri to be provided to the LocatorTask (geocoder)
        private Uri _serviceUri = new Uri("https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer");

        // UI Elements
        private MapView _myMapView = new MapView();

        private AutoCompleteTextView _mySearchBox;
        private AutoCompleteTextView _myLocationBox;
        private Button _mySearchButton;
        private Button _mySearchRestrictedButton;
        private ProgressBar _myProgressBar;

        // List of suggestions
        private List<String> _suggestions = new List<string>();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Find place";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateStreets());

            // Provide Map to the MapView
            _myMapView.Map = myMap;

            // Wire up the map view to support tapping on address markers
            _myMapView.GeoViewTapped += _myMapView_GeoViewTapped;

            // Subscribe to location events to zoom to current location
            _myMapView.LocationDisplay.LocationChanged += LocationDisplay_LocationChanged;

            // Enable location display
            _myMapView.LocationDisplay.IsEnabled = true;

            // Initialize the LocatorTask with the provided service Uri
            _geocoder = await LocatorTask.CreateAsync(_serviceUri);

            // Enable controls now that the geocoder is ready
            _mySearchBox.Enabled = true;
            _myLocationBox.Enabled = true;
            _mySearchButton.Enabled = true;
            _mySearchRestrictedButton.Enabled = true;
        }

        private void LocationDisplay_LocationChanged(object sender, Esri.ArcGISRuntime.Location.Location e)
        {
            // Return if no location
            if (e.Position == null) { return; }

            // Unsubscribe; only want to zoom to location once
            ((LocationDisplay)sender).LocationChanged -= LocationDisplay_LocationChanged;
            RunOnUiThread(() =>
            {
                _myMapView.SetViewpoint(new Viewpoint(e.Position, 10000));
            });
        }

        private void CreateLayout()
        {
            // Vertical stack layout
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Search bar
            _mySearchBox = new AutoCompleteTextView(this) { Text = "Coffee" };
            layout.AddView(_mySearchBox);

            // Location search bar
            _myLocationBox = new AutoCompleteTextView(this) { Text = "Current Location" };
            layout.AddView(_myLocationBox);

            // Disable multi-line searches
            _mySearchBox.SetMaxLines(1);
            _myLocationBox.SetMaxLines(1);

            // Search buttons; horizontal layout
            var searchButtonLayout = new LinearLayout(this) { Orientation = Orientation.Horizontal };
            _mySearchButton = new Button(this) { Text = "Search All" };
            _mySearchRestrictedButton = new Button(this) { Text = "Search View" };

            // Add the buttons to the layout
            searchButtonLayout.AddView(_mySearchButton);
            searchButtonLayout.AddView(_mySearchRestrictedButton);

            // Progress bar
            _myProgressBar = new ProgressBar(this) { Indeterminate = true, Visibility = Android.Views.ViewStates.Gone };
            layout.AddView(_myProgressBar);

            // Add the layout to the view
            layout.AddView(searchButtonLayout);

            // Add the mapview to the view
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);

            // Disable the buttons and search bar until the geocoder is ready
            _mySearchBox.Enabled = false;
            _myLocationBox.Enabled = false;
            _mySearchButton.Enabled = false;
            _mySearchRestrictedButton.Enabled = false;

            // Hook up the UI event handlers for suggestion & search
            _mySearchBox.TextChanged += _mySearchBox_TextChanged;
            _myLocationBox.TextChanged += _myLocationBox_TextChanged;
            _mySearchButton.Click += _mySearchButton_Click;
            _mySearchRestrictedButton.Click += _mySearchRestrictedButton_Click;
        }

        /// <summary>
        /// Gets the map point corresponding to the text in the location textbox.
        /// If the text is 'Current Location', the returned map point will be the device's location.
        /// </summary>
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
                return _myMapView.LocationDisplay.Location.Position;
            }
        }

        /// <summary>
        /// Runs a search and populates the map with results based on the provided information
        /// </summary>
        /// <param name="enteredText">Results to search for</param>
        /// <param name="locationText">Location around which to find results</param>
        /// <param name="restrictToExtent">If true, limits results to only those that are within the current extent</param>
        private async Task UpdateSearch(string enteredText, string locationText, bool restrictToExtent = false)
        {
            // Clear any existing markers
            _myMapView.GraphicsOverlays.Clear();

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
                Geometry extent = _myMapView.VisibleArea;

                // Update the search parameters
                parameters.SearchArea = extent;
            }

            // Show the progress bar
            _myProgressBar.Visibility = Android.Views.ViewStates.Visible;

            // Get the location information
            IReadOnlyList<GeocodeResult> locations = await _geocoder.GeocodeAsync(enteredText, parameters);

            // Stop gracefully and show a message if the geocoder does not return a result
            if (locations.Count < 1)
            {
                _myProgressBar.Visibility = Android.Views.ViewStates.Gone; // 1. Hide the progress bar
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
            _myProgressBar.Visibility = Android.Views.ViewStates.Gone;

            // Add the GraphicsOverlay to the MapView
            _myMapView.GraphicsOverlays.Add(resultOverlay);

            // Create a viewpoint for the extent containing all graphics
            Viewpoint viewExtent = new Viewpoint(resultOverlay.Extent);

            // Update the map viewpoint
            _myMapView.SetViewpoint(viewExtent);
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
        /// Shows a callout for any tapped graphics
        /// </summary>
        private async void _myMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Search for the graphics underneath the user's tap
            IReadOnlyList<IdentifyGraphicsOverlayResult> results = await _myMapView.IdentifyGraphicsOverlaysAsync(e.Position, 12, false);

            // Clear callouts and return if there was no result
            if (results.Count < 1 || results.First().Graphics.Count < 1) { _myMapView.DismissCallout(); return; }

            // Get the first graphic from the first result
            Graphic matchingGraphic = results.First().Graphics.First();

            // Get the title; manually added to the point's attributes in UpdateSearch
            String title = matchingGraphic.Attributes["Match_Title"] as String;

            // Get the address; manually added to the point's attributes in UpdateSearch
            String address = matchingGraphic.Attributes["Match_Address"] as String;

            // Define the callout
            CalloutDefinition calloutBody = new CalloutDefinition(title, address);

            // Show the callout on the map at the tapped location
            _myMapView.ShowCalloutAt(e.Location, calloutBody);
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
            var builder = new AlertDialog.Builder(this);
            builder.SetMessage(message).SetTitle("Alert").Show();
        }

        /// <summary>
        /// Method used to keep the suggestions up-to-date for the search box
        /// </summary>
        private async void _mySearchBox_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            // Dismiss callout, if any
            UserInteracted();

            // Get the current text
            string searchText = _mySearchBox.Text;

            // Get the current search location
            string locationText = _myLocationBox.Text;

            // Convert the list into a usable format for the suggest box
            List<String> results = (await GetSuggestResults(searchText, locationText, true)).ToList();

            // Quit if there are no results
            if (results == null || !results.Any()) { return; }

            // Create an array adapter to provide autocomplete suggestions
            ArrayAdapter adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, results);

            // Apply the adapter
            _mySearchBox.Adapter = adapter;
        }

        /// <summary>
        /// Method used to keep the suggestions up-to-date for the location box
        /// </summary>
        private async void _myLocationBox_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            // Dismiss callout, if any
            UserInteracted();

            // Get the current text
            string searchText = _myLocationBox.Text;

            // Get the results
            IEnumerable<String> results = await GetSuggestResults(searchText);

            // Quit if there are no results
            if (results == null || results.Count() == 0) { return; }

            // Get a modifiable list from the results
            List<String> mutableResults = results.ToList();

            // Add a 'current location' option to the list
            mutableResults.Insert(0, "Current Location");

            // Create an array adapter to provide autocomplete suggestions
            ArrayAdapter adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, mutableResults);

            // Apply the adapter
            _myLocationBox.Adapter = adapter;
        }

        /// <summary>
        /// Method called to start an unrestricted search
        /// </summary>
        private async void _mySearchButton_Click(object sender, EventArgs e)
        {
            // Dismiss callout, if any
            UserInteracted();

            // Get the search text
            string searchText = _mySearchBox.Text;

            // Get the location text
            string locationText = _myLocationBox.Text;

            // Run the search
            await UpdateSearch(searchText, locationText);
        }

        /// <summary>
        /// Method called to start a search that is restricted to results within the current extent
        /// </summary>
        private async void _mySearchRestrictedButton_Click(object sender, EventArgs e)
        {
            // Dismiss callout, if any
            UserInteracted();

            // Get the search text
            string searchText = _mySearchBox.Text;

            // Get the location text
            string locationText = _myLocationBox.Text;

            // Run the search
            await UpdateSearch(searchText, locationText, true);
        }

        /// <summary>
        /// Method to handle hiding the callout, should be called by all UI event handlers
        /// </summary>
        private void UserInteracted()
        {
            // Hide the callout
            _myMapView.DismissCallout();
        }
    }
}