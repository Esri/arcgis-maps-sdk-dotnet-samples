// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Google.Android.Material.Snackbar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ArcGISRuntime.Samples.FindPlace
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Find place",
        category: "Search",
        description: "Find places of interest near a location or within a specific area.",
        instructions: "Choose a type of place in the first field and an area to search within in the second field. Tap the Search button to show the results of the query on the map. Tap on a result pin to show its name and address. If you pan away from the result area, a \"Redo search in this area\" button will appear. Tap it to query again for the currently viewed area on the map.",
        tags: new[] { "POI", "businesses", "geocode", "locations", "locator", "places of interest", "point of interest", "search", "suggestions" })]
    [ArcGISRuntime.Samples.Shared.Attributes.EmbeddedResource(@"PictureMarkerSymbols\pin_star_blue.png")]
    public partial class FindPlace : Activity
    {
        // The LocatorTask provides geocoding services via a service.
        private LocatorTask _geocoder;

        // Service Uri to be provided to the LocatorTask (geocoder).
        private Uri _serviceUri = new Uri("https://geocode-api.arcgis.com/arcgis/rest/services/World/GeocodeServer");

        // Hold references to the UI controls.
        private MapView _myMapView;
        private AutoCompleteTextView _mySearchBox;
        private AutoCompleteTextView _myLocationBox;
        private Button _mySearchButton;
        private Button _mySearchRestrictedButton;
        private ProgressBar _myProgressBar;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Find place";

            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            // Create new Map with basemap.
            Map myMap = new Map(BasemapStyle.ArcGISStreets);

            // Provide Map to the MapView.
            _myMapView.Map = myMap;

            // Wire up the map view to support tapping on address markers.
            _myMapView.GeoViewTapped += _myMapView_GeoViewTapped;

            // Ask for location permissions. Events wired up in OnRequestPermissionsResult.
            AskForLocationPermission();

            // Initialize the LocatorTask with the provided service Uri.
            _geocoder = await LocatorTask.CreateAsync(_serviceUri);

            // Enable controls now that the geocoder is ready.
            _mySearchBox.Enabled = true;
            _myLocationBox.Enabled = true;
            _mySearchButton.Enabled = true;
            _mySearchRestrictedButton.Enabled = true;
        }

        private void LocationDisplay_LocationChanged(object sender, Esri.ArcGISRuntime.Location.Location e)
        {
            // Return if no location.
            if (e.Position == null)
            {
                return;
            }

            // Unsubscribe; only want to zoom to location once.
            ((LocationDisplay)sender).LocationChanged -= LocationDisplay_LocationChanged;

            // Zoom to the location.
            _myMapView.SetViewpointCenterAsync(e.Position, 100000);
        }

        private void CreateLayout()
        {
            // Vertical stack layout.
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Search bar.
            _mySearchBox = new AutoCompleteTextView(this) { Text = "Coffee" };
            layout.AddView(_mySearchBox);

            // Location search bar.
            _myLocationBox = new AutoCompleteTextView(this) { Text = "Current Location" };
            layout.AddView(_myLocationBox);

            // Disable multi-line searches.
            _mySearchBox.SetMaxLines(1);
            _myLocationBox.SetMaxLines(1);

            // Search buttons; horizontal layout.
            LinearLayout.LayoutParams param = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent,
                1.0f
            );
            LinearLayout searchButtonLayout = new LinearLayout(this) { Orientation = Orientation.Horizontal };
            _mySearchButton = new Button(this) { Text = "Search All", LayoutParameters = param };
            _mySearchRestrictedButton = new Button(this) { Text = "Search View", LayoutParameters = param };

            // Add the buttons to the layout.
            searchButtonLayout.AddView(_mySearchButton);
            searchButtonLayout.AddView(_mySearchRestrictedButton);

            // Progress bar.
            _myProgressBar = new ProgressBar(this) { Indeterminate = true, Visibility = Android.Views.ViewStates.Gone };
            layout.AddView(_myProgressBar);

            // Add the layout to the view.
            layout.AddView(searchButtonLayout);

            // Add the mapview to the view.
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);

            // Disable the buttons and search bar until the geocoder is ready.
            _mySearchBox.Enabled = false;
            _myLocationBox.Enabled = false;
            _mySearchButton.Enabled = false;
            _mySearchRestrictedButton.Enabled = false;

            // Hook up the UI event handlers for suggestion & search.
            _mySearchBox.TextChanged += _mySearchBox_TextChanged;
            _myLocationBox.TextChanged += _myLocationBox_TextChanged;
            _mySearchButton.Click += _mySearchButton_Click;
            _mySearchRestrictedButton.Click += _mySearchRestrictedButton_Click;
        }

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
                return _myMapView.LocationDisplay.Location.Position;
            }
        }

        private async Task UpdateSearch(string enteredText, string locationText, bool restrictToExtent = false)
        {
            // Clear any existing markers.
            _myMapView.GraphicsOverlays.Clear();

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
                // Get the current map extent.
                Geometry extent = _myMapView.VisibleArea;

                // Update the search parameters.
                parameters.SearchArea = extent;
            }

            // Show the progress bar.
            _myProgressBar.Visibility = Android.Views.ViewStates.Visible;

            // Get the location information.
            IReadOnlyList<GeocodeResult> locations = await _geocoder.GeocodeAsync(enteredText, parameters);

            // Stop gracefully and show a message if the geocoder does not return a result.
            if (locations.Count < 1)
            {
                _myProgressBar.Visibility = Android.Views.ViewStates.Gone; // 1. Hide the progress bar.
                ShowMessage("No results found", "Alert"); // 2. Show a message.
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
            _myProgressBar.Visibility = Android.Views.ViewStates.Gone;

            // Add the GraphicsOverlay to the MapView.
            _myMapView.GraphicsOverlays.Add(resultOverlay);

            // Update the map viewpoint.
            await _myMapView.SetViewpointGeometryAsync(resultOverlay.Extent, 50);
        }

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
            // The image is a pin; offset the image so that the pinpoint
            //     is on the point rather than the image's true center.
            pinSymbol.LeaderOffsetX = 30;
            pinSymbol.OffsetY = 14;
            return new Graphic(point, pinSymbol);
        }

        private async void _myMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Search for the graphics underneath the user's tap.
            IReadOnlyList<IdentifyGraphicsOverlayResult> results = await _myMapView.IdentifyGraphicsOverlaysAsync(e.Position, 12, false);

            // Clear callouts and return if there was no result.
            if (results.Count < 1 || results.First().Graphics.Count < 1)
            {
                _myMapView.DismissCallout();
                return;
            }

            // Get the first graphic from the first result.
            Graphic matchingGraphic = results.First().Graphics.First();

            // Get the title; manually added to the point's attributes in UpdateSearch.
            string title = matchingGraphic.Attributes["Match_Title"] as string;

            // Get the address; manually added to the point's attributes in UpdateSearch.
            string address = matchingGraphic.Attributes["Match_Address"] as string;

            // Define the callout.
            CalloutDefinition calloutBody = new CalloutDefinition(title, address);

            // Show the callout on the map at the tapped location.
            _myMapView.ShowCalloutAt(e.Location, calloutBody);
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

            // Return the list.
            return results.Select(result => result.Label).ToList();
        }

        private async void _mySearchBox_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            // Dismiss callout, if any.
            UserInteracted();

            // Get the current text.
            string searchText = _mySearchBox.Text;

            // Get the current search location.
            string locationText = _myLocationBox.Text;

            // Convert the list into a usable format for the suggest box.
            List<string> results = await GetSuggestResults(searchText, locationText, true);

            // Quit if there are no results.
            if (!results.Any())
            {
                return;
            }

            // Create an array adapter to provide autocomplete suggestions.
            ArrayAdapter adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, results);

            // Apply the adapter.
            _mySearchBox.Adapter = adapter;
        }

        private async void _myLocationBox_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            // Dismiss callout, if any.
            UserInteracted();

            // Get the current text.
            string searchText = _myLocationBox.Text;

            // Get the results.
            List<string> results = await GetSuggestResults(searchText);

            // Quit if there are no results.
            if (!results.Any())
            {
                return;
            }

            // Add a 'current location' option to the list.
            results.Insert(0, "Current Location");

            // Create an array adapter to provide autocomplete suggestions.
            ArrayAdapter adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, results);

            // Apply the adapter.
            _myLocationBox.Adapter = adapter;
        }

        private async void _mySearchButton_Click(object sender, EventArgs e)
        {
            // Dismiss callout, if any.
            UserInteracted();

            // Get the search text.
            string searchText = _mySearchBox.Text;

            // Get the location text.
            string locationText = _myLocationBox.Text;

            // Run the search.
            await UpdateSearch(searchText, locationText);
        }

        private async void _mySearchRestrictedButton_Click(object sender, EventArgs e)
        {
            // Dismiss callout, if any.
            UserInteracted();

            // Get the search text.
            string searchText = _mySearchBox.Text;

            // Get the location text.
            string locationText = _myLocationBox.Text;

            // Run the search.
            await UpdateSearch(searchText, locationText, true);
        }

        private void UserInteracted()
        {
            // Hide the callout.
            _myMapView.DismissCallout();
        }

        private void ShowMessage(string message, string title = "Error") => new AlertDialog.Builder(this).SetTitle(title).SetMessage(message).Show();

        protected override void OnDestroy()
        {
            // Stop the location data source.
            _myMapView?.LocationDisplay?.DataSource?.StopAsync();

            base.OnDestroy();
        }
    }

    #region Location Display Permissions

    public partial class FindPlace : ActivityCompat.IOnRequestPermissionsResultCallback
    {
        private const int LocationPermissionRequestCode = 99;

        private async void AskForLocationPermission()
        {
            // Only check if permission hasn't been granted yet.
            if (ContextCompat.CheckSelfPermission(this, LocationService) != Permission.Granted)
            {
                // The Fine location permission will be requested.
                var requiredPermissions = new[] { Manifest.Permission.AccessFineLocation };

                // Only prompt the user first if the system says to.
                if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.AccessFineLocation))
                {
                    // A snackbar is a small notice that shows on the bottom of the view.
                    Snackbar.Make(_myMapView,
                            "Location permission is needed to display location on the map.",
                            Snackbar.LengthIndefinite)
                        .SetAction("OK",
                            delegate
                            {
                                // When the user presses 'OK', the system will show the standard permission dialog.
                                // Once the user has accepted or denied, OnRequestPermissionsResult is called with the result.
                                ActivityCompat.RequestPermissions(this, requiredPermissions, LocationPermissionRequestCode);
                            }
                        ).Show();
                }
                else
                {
                    // When the user presses 'OK', the system will show the standard permission dialog.
                    // Once the user has accepted or denied, OnRequestPermissionsResult is called with the result.
                    this.RequestPermissions(requiredPermissions, LocationPermissionRequestCode);
                }
            }
            else
            {
                try
                {
                    // Explicit DataSource.LoadAsync call is used to surface any errors that may arise.
                    await _myMapView.LocationDisplay.DataSource.StartAsync();
                    _myMapView.LocationDisplay.IsEnabled = true;
                    _myMapView.LocationDisplay.LocationChanged += LocationDisplay_LocationChanged;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                    ShowMessage(ex.Message, "Failed to start location display.");
                }
            }
        }

        public override async void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            // Ignore other location requests.
            if (requestCode != LocationPermissionRequestCode)
            {
                return;
            }

            // If the permissions were granted, enable location.
            if (grantResults.Length == 1 && grantResults[0] == Permission.Granted)
            {
                System.Diagnostics.Debug.WriteLine("User affirmatively gave permission to use location. Enabling location.");
                try
                {
                    // Explicit DataSource.LoadAsync call is used to surface any errors that may arise.
                    await _myMapView.LocationDisplay.DataSource.StartAsync();
                    _myMapView.LocationDisplay.IsEnabled = true;
                    _myMapView.LocationDisplay.LocationChanged += LocationDisplay_LocationChanged;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                    ShowMessage(ex.Message, "Failed to start location display.");
                }
            }
            else
            {
                ShowMessage("Location permissions not granted.", "Failed to start location display.");
            }
        }
    }

    #endregion Location Display Permissions
}