// Copyright 2022 Esri.
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
using System.Diagnostics;
using System.Reflection;

namespace ArcGIS.Samples.FindPlace
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Find place",
        category: "Search",
        description: "Find places of interest near a location or within a specific area.",
        instructions: "Choose a type of place in the first field and an area to search within in the second field. Tap the Search button to show the results of the query on the map. Tap on a result pin to show its name and address. If you pan away from the result area, a \"Redo search in this area\" button will appear. Tap it to query again for the currently viewed area on the map.",
        tags: new[] { "POI", "businesses", "geocode", "locations", "locator", "places of interest", "point of interest", "search", "suggestions" })]
    [ArcGIS.Samples.Shared.Attributes.EmbeddedResource(@"PictureMarkerSymbols\pin_star_blue.png")]
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

            await MyMapView.Map.LoadAsync();

            // Subscribe to location changed event so that map can zoom to location
            MyMapView.LocationDisplay.LocationChanged += LocationDisplay_LocationChanged;

            try
            {
                // Check if location permission granted.
                var status = Microsoft.Maui.ApplicationModel.PermissionStatus.Unknown;
                status = await Microsoft.Maui.ApplicationModel.Permissions.CheckStatusAsync<Microsoft.Maui.ApplicationModel.Permissions.LocationWhenInUse>();

                // Request location permission if not granted.
                if (status != Microsoft.Maui.ApplicationModel.PermissionStatus.Granted)
                {
                    status = await Microsoft.Maui.ApplicationModel.Permissions.RequestAsync<Microsoft.Maui.ApplicationModel.Permissions.LocationWhenInUse>();
                }

                // Start the location display once permission is granted.
                if (status == Microsoft.Maui.ApplicationModel.PermissionStatus.Granted)
                {
                    await MyMapView.LocationDisplay.DataSource.StartAsync();
                    MyMapView.LocationDisplay.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await Application.Current.MainPage.DisplayAlert("Couldn't start location", ex.Message, "OK");
            }
                

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
                // Get the current device location (if available).
                return MyMapView.LocationDisplay.Location?.Position;
            }
        }

        private async Task UpdateSearch(string enteredText, string locationText, bool restrictToExtent = false)
        {
            // Clear any existing markers.
            MyMapView.GraphicsOverlays.Clear();

            // Return gracefully if the textbox is empty or the geocoder isn't ready.
            if (String.IsNullOrWhiteSpace(enteredText) || _geocoder == null) { return; }

            // Create the geocode parameters.
            GeocodeParameters parameters = new GeocodeParameters();
            try
            {
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
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async Task<Graphic> GraphicForPoint(MapPoint point)
        {
            // Get current assembly that contains the image.
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            // Get image as a stream from the resources.
            // Picture is defined as EmbeddedResource and DoNotCopy.
            Stream resourceStream = currentAssembly.GetManifestResourceStream(
                "ArcGIS.Resources.PictureMarkerSymbols.pin_star_blue.png");

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

        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.Maui.GeoViewInputEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void ShowStatusMessage(string message)
        {
            // Display the message to the user.
            Application.Current.MainPage.DisplayAlert("Alert", message, "OK");
        }

        private void MyLocationBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Dismiss callout, if any.
            UserInteracted();
        }

        private void MySearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Dismiss callout, if any.
            UserInteracted();
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
            _ = UpdateSearch(searchText, locationText, true);
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
            _ = UpdateSearch(searchText, locationText);
        }

        private void MyLocationBox_Unfocused(object sender, FocusEventArgs e)
        {
            // Dismiss callout, if any.
            UserInteracted();

            // Show the map view.
            MyMapView.IsVisible = true;
        }

        private void MySearchBox_Focused(object sender, FocusEventArgs e)
        {
            // Track last used control for autocomplete selection purposes.
            _lastInteractedBar = ((SearchBar)sender);

            // Dismiss callout, if any.
            UserInteracted();

            // Hide the map view.
            MyMapView.IsVisible = false;
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