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
using System; using System.Threading.Tasks; 
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xamarin.Forms;
#if XAMARIN_ANDROID
using ArcGISRuntime.Droid;
#endif

namespace ArcGISRuntime.Samples.FindPlace
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Find place",
        category: "Search",
        description: "Find places of interest near a location or within a specific area.",
        instructions: "Choose a type of place in the first field and an area to search within in the second field. Tap the Search button to show the results of the query on the map. Tap on a result pin to show its name and address. If you pan away from the result area, a \"Redo search in this area\" button will appear. Tap it to query again for the currently viewed area on the map.",
        tags: new[] { "POI", "businesses", "geocode", "locations", "locator", "places of interest", "point of interest", "search", "suggestions" })]
    [ArcGISRuntime.Samples.Shared.Attributes.EmbeddedResource(@"PictureMarkerSymbols\pin_star_blue.png")]
    public partial class FindPlace : ContentPage, IDisposable
    {
        // The LocatorTask provides geocoding services
        private LocatorTask _geocoder;
        private SearchBar _lastInteractedBar;

        // Service Uri to be provided to the LocatorTask (geocoder).
        private Uri _serviceUri = new Uri("https://geocode-api.arcgis.com/arcgis/rest/services/World/GeocodeServer");

        public FindPlace()
        {
            InitializeComponent();

            _ = Initialize();
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        private async Task Initialize()
        {
            // Create new Map with basemap.
            Map myMap = new Map(BasemapStyle.ArcGISStreets);

            // Assign the map to the MapView.
            MyMapView.Map = myMap;

            // Wait for the MapView to load.
            MyMapView.PropertyChanged += async (o, e) =>
            {
                if (e.PropertyName == nameof(MyMapView.LocationDisplay) && MyMapView.LocationDisplay != null)
                {
                    // Subscribe to location changed event so that map can zoom to location
                    MyMapView.LocationDisplay.LocationChanged += LocationDisplay_LocationChanged;

                    try
                    {
                        // Permission request only needed on Android.
#if XAMARIN_ANDROID
                        // See implementation in MainActivity.cs in the Android platform project.
                        MainActivity.Instance.AskForLocationPermission(MyMapView);
#else
                        await MyMapView.LocationDisplay.DataSource.StartAsync();
                        MyMapView.LocationDisplay.IsEnabled = true;
#endif
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        await Application.Current.MainPage.DisplayAlert("Couldn't start location", ex.Message, "OK");
                    }
                }
            };

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
            // Return if position is null; event is raised with null location after.
            if (e.Position == null) { return; }

            // Unsubscribe from further events; only want to zoom to location once.
            ((LocationDisplay)sender).LocationChanged -= LocationDisplay_LocationChanged;

            // Zoom to the location.
            MyMapView.SetViewpointCenterAsync(e.Position, 100000);
        }

        private async Task<MapPoint> GetSearchMapPoint(string locationText)
        {
            // Get the map point for the search text.
            if (locationText != "Current Location")
            {
                // Geocode the location.
                IReadOnlyList<GeocodeResult> locations = await _geocoder.GeocodeAsync(locationText);

                // return if there are no results.
                if (!locations.Any()) { return null; }

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

        private async void UpdateSearch(string enteredText, string locationText, bool restrictToExtent = false)
        {
            // Clear any existing markers.
            MyMapView.GraphicsOverlays.Clear();

            // Return gracefully if the textbox is empty or the geocoder isn't ready.
            if (String.IsNullOrWhiteSpace(enteredText) || _geocoder == null) { return; }

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
                // Get the current map extent.
                Geometry extent = MyMapView.VisibleArea;

                // Update the search parameters.
                parameters.SearchArea = extent;
            }

            // Show the progress bar.
            MyProgressBar.IsVisible = true;

            // Get the location information.
            IReadOnlyList<GeocodeResult> locations = await _geocoder.GeocodeAsync(enteredText, parameters);

            // Stop gracefully and show a message if the geocoder does not return a result.
            if (locations.Count < 1)
            {
                MyProgressBar.IsVisible = false; // 1. Hide the progress bar.
                ShowStatusMessage("No results found"); // 2. Show a message.
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
                    point.Attributes["Match_Address"] = addresses.First().Label;
                }

                // Add the Graphic to the GraphicsOverlay.
                resultOverlay.Graphics.Add(point);
            }

            // Hide the progress bar.
            MyProgressBar.IsVisible = false;

            // Add the GraphicsOverlay to the MapView.
            MyMapView.GraphicsOverlays.Add(resultOverlay);

            // Update the map viewpoint.
            await MyMapView.SetViewpointGeometryAsync(resultOverlay.Extent, 50);
        }

        private async Task<Graphic> GraphicForPoint(MapPoint point)
        {
#if WINDOWS_UWP
            // Get current assembly that contains the image.
            Assembly currentAssembly = GetType().GetTypeInfo().Assembly;
#else
            // Get current assembly that contains the image.
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
#endif

            // Get image as a stream from the resources.
            // Picture is defined as EmbeddedResource and DoNotCopy.
            Stream resourceStream = currentAssembly.GetManifestResourceStream(
                "ArcGISRuntime.Resources.PictureMarkerSymbols.pin_star_blue.png");

            // Create new symbol using asynchronous factory method from stream.
            PictureMarkerSymbol pinSymbol = await PictureMarkerSymbol.CreateAsync(resourceStream);
            pinSymbol.Width = 60;
            pinSymbol.Height = 60;
            // The image is a pin; offset the image so that the pinpoint
            //     is on the point rather than the image's true center.
            pinSymbol.LeaderOffsetX = 30;
            pinSymbol.OffsetY = 14;
            return new Graphic(point, pinSymbol);
        }

        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.Xamarin.Forms.GeoViewInputEventArgs e)
        {
            // Search for the graphics underneath the user's tap.
            IReadOnlyList<IdentifyGraphicsOverlayResult> results = await MyMapView.IdentifyGraphicsOverlaysAsync(e.Position, 12, false);

            // Clear callouts and return if there was no result.
            if (results.Count < 1 || results.First().Graphics.Count < 1) { MyMapView.DismissCallout(); return; }

            // Get the first graphic from the first result.
            Graphic matchingGraphic = results.First().Graphics.First();

            // Get the title; manually added to the point's attributes in UpdateSearch.
            string title = matchingGraphic.Attributes["Match_Title"] as String;

            // Get the address; manually added to the point's attributes in UpdateSearch.
            string address = matchingGraphic.Attributes["Match_Address"] as String;

            // Define the callout.
            CalloutDefinition calloutBody = new CalloutDefinition(title, address);

            // Show the callout on the map at the tapped location.
            MyMapView.ShowCalloutAt(e.Location, calloutBody);
        }

        private async Task<List<string>> GetSuggestResults(string searchText, string location = "", bool poiOnly = false)
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
            if (poiOnly) { parameters.Categories.Add("POI"); }

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

            // Return the list
            return results.Select(result => result.Label).ToList();
        }

        private void ShowStatusMessage(string message)
        {
            // Display the message to the user.
            Application.Current.MainPage.DisplayAlert("Alert", message, "OK");
        }

        private async void MyLocationBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Dismiss callout, if any.
            UserInteracted();

            // Get the current text.
            string searchText = MyLocationBox.Text;

            // Get the results.
            List<string> results = await GetSuggestResults(searchText);

            // Quit if there are no results.
            if (!results.Any()) { return; }

            // Add a 'current location' option to the list.
            results.Insert(0, "Current Location");

            // Update the list of options.
            lstViewSuggestions.ItemsSource = results;
        }

        private async void MySearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Dismiss callout, if any.
            UserInteracted();

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
            lstViewSuggestions.ItemsSource = results;
        }

        private void MySearchRestrictedButton_Clicked(object sender, EventArgs e)
        {
            // Dismiss callout, if any.
            UserInteracted();

            // Get the search text.
            string searchText = MySearchBox.Text;

            // Get the location text.
            string locationText = MyLocationBox.Text;

            // Run the search.
            UpdateSearch(searchText, locationText, true);
        }

        private void MySearchButton_Clicked(object sender, EventArgs e)
        {
            // Dismiss callout, if any.
            UserInteracted();

            // Get the search text.
            string searchText = MySearchBox.Text;

            // Get the location text.
            string locationText = MyLocationBox.Text;

            // Run the search.
            UpdateSearch(searchText, locationText);
        }

        private void MyLocationBox_Unfocused(object sender, FocusEventArgs e)
        {
            // Dismiss callout, if any.
            UserInteracted();

            // Hide the suggestion list.
            lstViewSuggestions.IsVisible = false;

            // Show the map view.
            MyMapView.IsVisible = true;
        }

        private void MySearchBox_Focused(object sender, FocusEventArgs e)
        {
            // Track last used control for autocomplete selection purposes.
            _lastInteractedBar = ((SearchBar)sender);

            // Dismiss callout, if any.
            UserInteracted();

            // Show the suggestion list.
            lstViewSuggestions.IsVisible = true;

            // Hide the map view.
            MyMapView.IsVisible = false;
        }

        private void lstViewSuggestions_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            // Verify selected item has a value.
            if (e.SelectedItem == null) return;

            // Dismiss callout, if any.
            UserInteracted();

            // Get the text of the selected item.
            string suggestion = e.SelectedItem.ToString();

            // Update the location search box if it has focus.
            if (MyLocationBox.IsFocused)
            {
                MyLocationBox.Text = suggestion;
            }
            else if (MySearchBox.IsFocused)
            {
                // Otherwise, update the search box.
                MySearchBox.Text = suggestion;
            }
            // Work around focus behavior on some platforms (e.g. Android)
            else if (_lastInteractedBar != null)
            {
                _lastInteractedBar.Text = suggestion;
            }
        }

        private void UserInteracted()
        {
            // Hide the callout.
            MyMapView.DismissCallout();
        }

        public void Dispose()
        {
            // Stop the location data source.
            MyMapView.LocationDisplay?.DataSource?.StopAsync();
        }
    }
}