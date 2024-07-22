// Copyright 2018 Esri.
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
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGIS.WPF.Samples.FindPlace
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Find place",
        category: "Search",
        description: "Find places of interest near a location or within a specific area.",
        instructions: "Choose a type of place in the first field and an area to search within in the second field. Click the Search button to show the results of the query on the map. Click on a result pin to show its name and address. If you pan away from the result area, a \"Redo search in this area\" button will appear. Click it to query again for the currently viewed area on the map.",
        tags: new[] { "POI", "businesses", "geocode", "locations", "locator", "places of interest", "point of interest", "search", "suggestions" })]
    [ArcGIS.Samples.Shared.Attributes.EmbeddedResource(@"PictureMarkerSymbols\pin_star_blue.png")]
    public partial class FindPlace
    {
        // Flag used to help prevent search completion from competing with user input.
        private bool _waitFlag;

        // The LocatorTask provides geocoding services.
        private LocatorTask _geocoder;

        // Service Uri to be provided to the LocatorTask (geocoder).
        private readonly Uri _serviceUri =
            new Uri("https://geocode-api.arcgis.com/arcgis/rest/services/World/GeocodeServer");

        public FindPlace()
        {
            InitializeComponent();

            // Setup the control references and execute initialization.
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Add event handler for when this sample is unloaded.
            Unloaded += SampleUnloaded;

            // Show a map with a streets basemap.
            MyMapView.Map = new Map(BasemapStyle.ArcGISStreets);

            // Subscribe to location changed events (to support zooming to current location).
            MyMapView.LocationDisplay.LocationChanged += LocationDisplay_LocationChanged;

            // Enable location display.
            MyMapView.LocationDisplay.IsEnabled = true;

            // Enable tap-for-info pattern on results.
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;

            // Initialize the LocatorTask with the provided service Uri.
            _geocoder = await LocatorTask.CreateAsync(_serviceUri);

            // Enable all controls now that the locator task is ready.
            MySearchBox.IsEnabled = true;
            MyLocationBox.IsEnabled = true;
            MySearchButton.IsEnabled = true;
            MySearchRestrictedButton.IsEnabled = true;
        }

        private void LocationDisplay_LocationChanged(object sender, Esri.ArcGISRuntime.Location.Location e)
        {
            // Return if position is null; event is called with null position when location display is turned on.
            if (e.Position == null)
            {
                return;
            }

            // Unsubscribe from the event (only want to zoom once).
            ((LocationDisplay)sender).LocationChanged -= LocationDisplay_LocationChanged;

            // Zoom to the location.
            MyMapView.SetViewpointCenterAsync(e.Position, 100000);
        }

        /// <summary>
        /// Gets the map point corresponding to the text in the location textbox.
        /// If the text is 'Current Location', the returned map point will be the device's location.
        /// </summary>
        private async Task<MapPoint> GetSearchMapPoint(string locationText)
        {
            // Get the map point for the search text.
            if (locationText != "Current Location")
            {
                // Geocode the location.
                IReadOnlyList<GeocodeResult> locations = await _geocoder.GeocodeAsync(locationText);

                // return if there are no results.
                if (!locations.Any())
                {
                    return null;
                }

                // Get the first result.
                GeocodeResult result = locations.First();

                // Return the map point.
                return result.DisplayLocation;
            }
            else
            {
                // Get the current device location (if available).
                return MyMapView.LocationDisplay.Location?.Position;
            }
        }

        /// <summary>
        /// Runs a search and populates the map with results based on the provided information.
        /// </summary>
        /// <param name="enteredText">Results to search for.</param>
        /// <param name="locationText">Location around which to find results.</param>
        /// <param name="restrictToExtent">If true, limits results to only those that are within the current extent.</param>
        private async Task UpdateSearch(string enteredText, string locationText, bool restrictToExtent = false)
        {
            try
            {
                // Clear any existing markers.
                MyMapView.GraphicsOverlays.Clear();

                // Return gracefully if the textbox is empty or the geocoder isn't ready.
                if (String.IsNullOrWhiteSpace(enteredText) || _geocoder == null)
                {
                    return;
                }

                // Create the geocode parameters.
                GeocodeParameters parameters = new GeocodeParameters();

                // Request that the "Address" attribute is included with results, to display in callouts.
                parameters.ResultAttributeNames.Add("Address");

                // Get the MapPoint for the current search location.
                MapPoint searchLocation = await GetSearchMapPoint(locationText);

                // Update the geocode parameters if the map point is not null.
                if (searchLocation != null)
                {
                    parameters.PreferredSearchLocation = searchLocation;

                    // Raise MinScore to a non-zero value, otherwise PreferredSearchLocation has no effect.
                    parameters.MinScore = 1;
                }

                // Update the search area if desired.
                if (restrictToExtent)
                {
                    // Update the search parameters with the current map extent.
                    parameters.SearchArea = MyMapView.VisibleArea;
                }

                // Show the progress bar.
                MyProgressBar.Visibility = Visibility.Visible;

                // Get the location information.
                IReadOnlyList<GeocodeResult> locations = await _geocoder.GeocodeAsync(enteredText, parameters);

                // Stop gracefully and show a message if the geocoder does not return a result.
                if (locations.Count < 1)
                {
                    MyProgressBar.Visibility = Visibility.Collapsed; // 1. Hide the progress bar.
                    MessageBox.Show("No results found"); // 2. Show a message.
                    return; // 3. Stop.
                }

                // Create the GraphicsOverlay so that results can be drawn on the map.
                GraphicsOverlay resultOverlay = new GraphicsOverlay();
                var symbol = await GetPinSymbolAsync();

                // Add each address to the map.
                foreach (GeocodeResult location in locations)
                {
                    // Get the Graphic to display.
                    var point = new Graphic(location.DisplayLocation, symbol);

                    // Add the specific result data to the point.
                    point.Attributes["Match_Title"] = location.Label;
                    point.Attributes["Match_Address"] = location.Attributes["Address"];

                    // Add the Graphic to the GraphicsOverlay.
                    resultOverlay.Graphics.Add(point);
                }

                // Hide the progress bar.
                MyProgressBar.Visibility = Visibility.Collapsed;

                // Add the GraphicsOverlay to the MapView.
                MyMapView.GraphicsOverlays.Add(resultOverlay);

                // Update the map viewpoint.
                await MyMapView.SetViewpointGeometryAsync(resultOverlay.Extent, 50);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        /// <summary>
        /// Creates and returns a "Pin" symbol used to mark search results on the MapView.
        /// </summary>
        private async Task<Esri.ArcGISRuntime.Symbology.Symbol> GetPinSymbolAsync()
        {
            // Hold a reference to the picture marker symbol.
            PictureMarkerSymbol pinSymbol;

            // Get current assembly that contains the image.
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            // Get the resource name of the blue pin image.
            string resourceStreamName = this.GetType().Assembly.GetManifestResourceNames().Single(str => str.EndsWith("pin_star_blue.png"));

            // Load the blue pin resource stream.
            using (Stream resourceStream = this.GetType().Assembly.
                       GetManifestResourceStream(resourceStreamName))
            {
                // Create new symbol using asynchronous factory method from stream.
                pinSymbol = await PictureMarkerSymbol.CreateAsync(resourceStream);
                pinSymbol.Width = 60;
                pinSymbol.Height = 60;
                // The symbol is a pin; centering it on the point is incorrect.
                // The values below center the pin and offset it so that the pinpoint is accurate.
                pinSymbol.LeaderOffsetX = 30;
                pinSymbol.OffsetY = 14;
            }

            return pinSymbol;
        }

        /// <summary>
        /// Shows a callout for any tapped graphics.
        /// </summary>
        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Search for the graphics underneath the user's tap.
                IReadOnlyList<IdentifyGraphicsOverlayResult> results = await MyMapView.IdentifyGraphicsOverlaysAsync(e.Position, 12, false);

                // Clear callouts and return if there was no result.
                if (results.Count < 1 || results.First().Graphics.Count < 1)
                {
                    MyMapView.DismissCallout();
                    return;
                }

                // Get the first graphic from the first result.
                Graphic matchingGraphic = results.First().Graphics.First();

                // Get the title; manually added to the point's attributes in UpdateSearch.
                string title = matchingGraphic.Attributes["Match_Title"].ToString();

                // Get the address; manually added to the point's attributes in UpdateSearch.
                string address = matchingGraphic.Attributes["Match_Address"].ToString();

                // Define the callout.
                CalloutDefinition calloutBody = new CalloutDefinition(title, address);

                // Show the callout on the map at the tapped location.
                MyMapView.ShowCalloutAt(e.Location, calloutBody);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        /// <summary>
        /// Returns a list of suggestions based on the input search text and limited by the specified parameters.
        /// </summary>
        /// <param name="searchText">Text to get suggestions for.</param>
        /// <param name="location">Location around which to look for suggestions.</param>
        /// <param name="poiOnly">If true, restricts suggestions to only Points of Interest (e.g. businesses, parks),
        /// rather than all matching results.</param>
        /// <returns>List of suggestions as strings.</returns>
        private async Task<List<string>> GetSuggestResults(string searchText, string location = "",
            bool poiOnly = false)
        {
            // Quit if string is null, empty, or whitespace.
            if (String.IsNullOrWhiteSpace(searchText))
            {
                return new List<string>();
            }

            // Quit if the geocoder isn't ready.
            if (_geocoder == null)
            {
                return new List<string>();
            }

            // Create geocode parameters.
            SuggestParameters parameters = new SuggestParameters();

            // Restrict suggestions to points of interest if desired.
            if (poiOnly)
            {
                parameters.Categories.Add("POI");
            }

            // Set the location for the suggest parameters.
            if (!String.IsNullOrWhiteSpace(location))
            {
                // Get the MapPoint for the current search location.
                MapPoint searchLocation = await GetSearchMapPoint(location);

                // Update the geocode parameters if the map point is not null.
                if (searchLocation != null)
                {
                    parameters.PreferredSearchLocation = searchLocation;
                }
            }

            // Get the updated results from the query so far.
            IReadOnlyList<SuggestResult> results = await _geocoder.SuggestAsync(searchText, parameters);

            // Convert the list into a list of strings (corresponding to the label property on each result) and return.
            return results.Select(result => result.Label).ToList();
        }

        /// <summary>
        /// Method used to keep the suggestions up-to-date for the search box.
        /// </summary>
        private async void MySearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                // Don't update results immediately; makes search-as-you-type more comfortable
                if (_waitFlag)
                {
                    return;
                }

                _waitFlag = true;
                await Task.Delay(150);
                _waitFlag = false;

                // Dismiss callout, if any.
                MyMapView.DismissCallout();

                // Get the current text.
                string searchText = MySearchBox.Text;

                // Get the current search location.
                string locationText = MyLocationBox.Text;

                // Convert the list into a usable format for the suggest box.
                List<string> results = await GetSuggestResults(searchText, locationText, true);

                // Quit if there are no results.
                if (!results.Any())
                {
                    return;
                }

                // Update the list of options.
                MySearchBox.ItemsSource = results;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        /// <summary>
        /// Method used to keep the suggestions up-to-date for the location box.
        /// </summary>
        private async void MyLocationBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                // Don't update results immediately; makes search-as-you-type more comfortable
                if (_waitFlag)
                {
                    return;
                }

                _waitFlag = true;
                await Task.Delay(150);
                _waitFlag = false;

                // Dismiss callout, if any.
                MyMapView.DismissCallout();

                // Get the current text.
                string searchText = MyLocationBox.Text;

                // Get the results.
                List<string> results = await GetSuggestResults(searchText);

                // Quit if there are no results.
                if (!results.Any())
                {
                    return;
                }

                // Add a 'current location' option to the list.
                results.Insert(0, "Current Location");

                // Update the list of options.
                MyLocationBox.ItemsSource = results;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        /// <summary>
        /// Method called to start a search that is restricted to results within the current extent.
        /// </summary>
        private void MySearchRestrictedButton_Click(object sender, RoutedEventArgs e)
        {
            // Dismiss callout, if any.
            MyMapView.DismissCallout();

            // Get the search text.
            string searchText = MySearchBox.Text;

            // Get the location text.
            string locationText = MyLocationBox.Text;

            // Run the search.
            _ = UpdateSearch(searchText, locationText, true);
        }

        /// <summary>
        /// Method called to start an unrestricted search.
        /// </summary>
        private void MySearchButton_Click(object sender, RoutedEventArgs e)
        {
            // Dismiss callout, if any.
            MyMapView.DismissCallout();

            // Get the search text.
            string searchText = MySearchBox.Text;

            // Get the location text.
            string locationText = MyLocationBox.Text;

            // Run the search.
            _ = UpdateSearch(searchText, locationText);
        }

        private void SampleUnloaded(object sender, RoutedEventArgs e)
        {
            // Stop the location data source.
            MyMapView.LocationDisplay?.DataSource?.StopAsync();
        }
    }
}