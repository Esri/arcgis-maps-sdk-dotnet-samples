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

namespace ArcGISRuntime.WPF.Samples.FindPlace
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Find place",
        category: "Search",
        description: "Find places of interest near a location or within a specific area.",
        instructions: "Choose a type of place in the first field and an area to search within in the second field. Click the Search button to show the results of the query on the map. Click on a result pin to show its name and address. If you pan away from the result area, a \"Redo search in this area\" button will appear. Click it to query again for the currently viewed area on the map.",
        tags: new[] { "POI", "businesses", "geocode", "locations", "locator", "places of interest", "point of interest", "search", "suggestions" })]
    [ArcGISRuntime.Samples.Shared.Attributes.EmbeddedResource(@"PictureMarkerSymbols\pin_star_blue.png")]
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
                // Get the current device location.
                return MyMapView.LocationDisplay.Location.Position;
            }
        }

        /// <summary>
        /// Runs a search and populates the map with results based on the provided information.
        /// </summary>
        /// <param name="enteredText">Results to search for.</param>
        /// <param name="locationText">Location around which to find results.</param>
        /// <param name="restrictToExtent">If true, limits results to only those that are within the current extent.</param>
        private async void UpdateSearch(string enteredText, string locationText, bool restrictToExtent = false)
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

            // Get the MapPoint for the current search location.
            MapPoint searchLocation = await GetSearchMapPoint(locationText);

            // Update the geocode parameters if the map point is not null.
            if (searchLocation != null)
            {
                parameters.PreferredSearchLocation = searchLocation;
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

            // Add each address to the map.
            foreach (GeocodeResult location in locations)
            {
                // Get the Graphic to display.
                Graphic point = await GraphicForPoint(location.DisplayLocation);

                // Add the specific result data to the point.
                point.Attributes["Match_Title"] = location.Label;

                // Get the address for the point.
                IReadOnlyList<GeocodeResult> addresses = await _geocoder.ReverseGeocodeAsync(location.DisplayLocation);

                // Add the first suitable address if possible.
                if (addresses.Any())
                {
                    point.Attributes["Match_Address"] = addresses[0].Label;
                }

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

        /// <summary>
        /// Creates and returns a Graphic associated with the given MapPoint.
        /// </summary>
        private async Task<Graphic> GraphicForPoint(MapPoint point)
        {
            // Get current assembly that contains the image.
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            // Get image as a stream from the resources.
            // Picture is defined as EmbeddedResource and DoNotCopy.
            Stream resourceStream = currentAssembly.GetManifestResourceStream(
                "ArcGISRuntime.Resources.PictureMarkerSymbols.pin_star_blue.png");

            // Create new symbol using asynchronous factory method from stream.
            PictureMarkerSymbol pinSymbol = await PictureMarkerSymbol.CreateAsync(resourceStream);
            pinSymbol.Width = 60;
            pinSymbol.Height = 60;
            // The symbol is a pin; centering it on the point is incorrect.
            // The values below center the pin and offset it so that the pinpoint is accurate.
            pinSymbol.LeaderOffsetX = 30;
            pinSymbol.OffsetY = 14;
            return new Graphic(point, pinSymbol);
        }

        /// <summary>
        /// Shows a callout for any tapped graphics.
        /// </summary>
        private void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            _ = MyMapView_GeoViewTappedTask(sender, e);
        }

        private async Task MyMapView_GeoViewTappedTask(object sender, GeoViewInputEventArgs e)
        {
            // Search for the graphics underneath the user's tap.
            IReadOnlyList<IdentifyGraphicsOverlayResult> results =
                await MyMapView.IdentifyGraphicsOverlaysAsync(e.Position, 12, false);

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
        private void MySearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _ = MySearchBox_TextChangedTask(sender, e);
        }

        private async Task MySearchBox_TextChangedTask(object sender, TextChangedEventArgs e)
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

        /// <summary>
        /// Method used to keep the suggestions up-to-date for the location box.
        /// </summary>
        private void MyLocationBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _ = MyLocationBox_TextChangedTask(sender, e);
        }

        private async Task MyLocationBox_TextChangedTask(object sender, TextChangedEventArgs e)
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
            UpdateSearch(searchText, locationText, true);
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
            UpdateSearch(searchText, locationText);
        }

        private void SampleUnloaded(object sender, RoutedEventArgs e)
        {
            // Stop the location data source.
            MyMapView.LocationDisplay?.DataSource?.StopAsync();
        }
    }
}