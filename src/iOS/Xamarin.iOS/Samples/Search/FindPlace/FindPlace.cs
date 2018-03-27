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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntime.Samples.FindPlace
{
    /// <summary>
    /// Class defines how a UITableView renders its contents.
    /// This implements the suggestion UI for the table view.
    /// </summary>
    public class SuggestionSource : UITableViewSource
    {
        // List of strings; these will be the suggestions
        public List<String> TableItems = new List<string>();

        // Used when re-using cells to ensure that a cell of the right type is used
        private string CellId = "TableCell";

        // Hold a reference to the owning view controller; this will be the active instance of FindPlace
        public FindPlace Owner { get; set; }

        public SuggestionSource(List<String> items, FindPlace owner)
        {
            // Set the items
            if (items != null)
            {
                TableItems = items;
            }

            // Set the owner
            Owner = owner;
        }

        /// <summary>
        /// This method gets a table view cell for the suggestion at the specified index
        /// </summary>
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Try to get a re-usable cell (this is for performance)
            UITableViewCell cell = tableView.DequeueReusableCell(CellId);

            // If there are no cells, create a new one
            if (cell == null)
            {
                cell = new UITableViewCell(UITableViewCellStyle.Default, CellId);
            }

            // Get the specific item to display
            String item = TableItems[indexPath.Row];

            // Set the text on the cell
            cell.TextLabel.Text = item;

            // Return the cell
            return cell;
        }

        /// <summary>
        /// This method allows the UITableView to know how many rows to render
        /// </summary>
        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return TableItems.Count;
        }

        /// <summary>
        /// Method called when a row is selected; notifies the primary view
        /// </summary>
        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            // Deselect the row
            tableView.DeselectRow(indexPath, true);

            // Accept the suggestion
            Owner.AcceptSuggestion(TableItems[indexPath.Row]);
        }
    }

    [Register("FindPlace")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Find place",
        "Search",
        "This sample demonstrates how to use geocode functionality to search for points of interest, around a location or within an extent.",
        "1. Enter a point of interest you'd like to search for (e.g. 'Starbucks')\n2. Enter a search location or accept the default 'Current Location'\n3. Select 'search all' to get all results, or press 'search view' to only get results within the current extent.")]
    [ArcGISRuntime.Samples.Shared.Attributes.EmbeddedResource(@"PictureMarkerSymbols\pin_star_blue.png")]
    public class FindPlace : UIViewController
    {
        // The LocatorTask provides geocoding services
        private LocatorTask _geocoder;

        // Service Uri to be provided to the LocatorTask (geocoder)
        private Uri _serviceUri = new Uri("https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer");

        // Create the MapView
        private MapView _myMapView = new MapView();

        // Create the search box
        private UITextField _mySearchBox = new UITextField();

        // Create the location search box
        private UITextField _myLocationBox = new UITextField();

        // Create the unrestricted search button
        private UIButton _mySearchButton = new UIButton();

        // Create the restricted search button
        private UIButton _mySearchRestrictedButton = new UIButton();

        // Create the progress indicator
        private UIActivityIndicatorView _myProgressBar = new UIActivityIndicatorView()
        {
            Hidden = true
        };

        // Hold a suggestion source for the suggestion list view
        private SuggestionSource _mySuggestionSource;

        // Create the view for showing suggestions
        private UITableView _mySuggestionView = new UITableView();

        // Keep track of whether the search or location is being actively edited
        private bool _locationSearchActive = false;

        public FindPlace()
        {
            Title = "Find place";
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
            // Get the height of the top bar
            nfloat topHeight = NavigationController.NavigationBar.Frame.Size.Height + 20;

            // Set a standard height for the controls
            nfloat height = 30;

            // Set a standard margin for the controls
            nfloat margin = 5;

            // Set a standard width for the controls
            nfloat width = View.Frame.Width - 2 * (nfloat)margin;

            // Set a standard width for a half-size control
            nfloat halfWidth = View.Frame.Width / 2 - 2 * (nfloat)margin;

            // The search box is the topmost control and fills the width of the screen
            _mySearchBox.Frame = new CoreGraphics.CGRect(margin, (topHeight += margin), width, height);

            // The location box is the second control and fills the width of the screen
            _myLocationBox.Frame = new CoreGraphics.CGRect(margin, (topHeight += margin + height), width, height);

            // The search all button takes up half the width in the third row
            _mySearchButton.Frame = new CoreGraphics.CGRect(margin, (topHeight += margin + height), halfWidth, height);

            // The search restricted button takes up half the width in the third row
            _mySearchRestrictedButton.Frame = new CoreGraphics.CGRect(halfWidth + 3 * margin, topHeight, halfWidth, height);

            // The progress bar is below the buttons
            _myProgressBar.Frame = new CoreGraphics.CGRect(margin, topHeight + margin + height, width, height);

            // The mapview fills the entire view
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            // The table view appears on top of the map view
            _mySuggestionView.Frame = new CoreGraphics.CGRect(2 * margin, (topHeight += height), width - 2 * margin, 8 * height);

            base.ViewDidLayoutSubviews();
        }

        /// <summary>
        /// Creates the initial layout for the app
        /// </summary>
		private void CreateLayout()
        {
            // Set the text on the two buttons
            _mySearchButton.SetTitle("Search All", UIControlState.Normal);
            _mySearchRestrictedButton.SetTitle("Search in View", UIControlState.Normal);

            // Set the default location and search text
            _myLocationBox.Text = "Current Location";
            _mySearchBox.Text = "Coffee";

            // Allow pressing 'return' to dismiss the keyboard
            _myLocationBox.ShouldReturn += (textField) => { textField.ResignFirstResponder(); return true; };
            _mySearchBox.ShouldReturn += (textField) => { textField.ResignFirstResponder(); return true; };

            // Gray out the buttons when they are disabled
            _mySearchButton.SetTitleColor(UIColor.Gray, UIControlState.Disabled);
            _mySearchRestrictedButton.SetTitleColor(UIColor.Gray, UIControlState.Disabled);

            // Change button color when enabled
            _mySearchButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _mySearchRestrictedButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);

            // Color the textboxes and buttons to appear over the mapview
            UIColor background = UIColor.FromWhiteAlpha(.85f, .95f);
            _mySearchBox.BackgroundColor = background;
            _myLocationBox.BackgroundColor = background;
            _mySearchButton.BackgroundColor = background;
            _mySearchRestrictedButton.BackgroundColor = background;
            _myProgressBar.BackgroundColor = background;

            // Hide the activity indicator (progress bar) when stopped
            _myProgressBar.HidesWhenStopped = true;

            // Set radii to make it look nice
            _mySearchRestrictedButton.Layer.CornerRadius = 5;
            _mySearchButton.Layer.CornerRadius = 5;
            _myLocationBox.Layer.CornerRadius = 5;
            _mySearchBox.Layer.CornerRadius = 5;

            // Make sure the suggestion list is hidden by default
            _mySuggestionView.Hidden = true;

            // Create the suggestion source
            _mySuggestionSource = new SuggestionSource(null, this);

            // Set the source for the table view
            _mySuggestionView.Source = _mySuggestionSource;

            // Enable tap-for-info pattern on results
            _myMapView.GeoViewTapped += _myMapView_GeoViewTapped;

            // Listen for taps on the search buttons
            _mySearchButton.TouchUpInside += _mySearchButton_Clicked;
            _mySearchRestrictedButton.TouchUpInside += _mySearchRestrictedButton_Click;

            // Listen for text-changed events
            _mySearchBox.AllEditingEvents += _mySearchBox_TextChanged;
            _myLocationBox.AllEditingEvents += _myLocationBox_TextChanged;

            // Add the views
            View.AddSubviews(_myMapView, _mySearchBox, _myLocationBox, _mySearchButton, _mySearchRestrictedButton, _myProgressBar, _mySuggestionView);
        }

        private async void Initialize()
        {
            // Get a new instance of the Imagery with Labels basemap
            Basemap _basemap = Basemap.CreateStreets();

            // Create a new Map with the basemap
            Map myMap = new Map(_basemap);

            // Populate the MapView with the Map
            _myMapView.Map = myMap;

            // Initialize the geocoder with the provided service Uri
            _geocoder = await LocatorTask.CreateAsync(_serviceUri);

            // Subscribe to location changed event so that map can zoom to location
            _myMapView.LocationDisplay.LocationChanged += LocationDisplay_LocationChanged;

            // Enable location display on the map
            _myMapView.LocationDisplay.IsEnabled = true;

            // Enable controls now that the geocoder is ready
            _myLocationBox.Enabled = true;
            _mySearchBox.Enabled = true;
            _mySearchButton.Enabled = true;
            _mySearchRestrictedButton.Enabled = true;
        }

        private void LocationDisplay_LocationChanged(object sender, Esri.ArcGISRuntime.Location.Location e)
        {
            // Return if position is null; event is raised with null location after
            if (e.Position == null) { return; }

            // Unsubscribe from further events; only want to zoom to location once
            ((LocationDisplay)sender).LocationChanged -= LocationDisplay_LocationChanged;

            // Need to use this to interact with UI elements because this function is called from a background thread
            InvokeOnMainThread(() =>
            {
                _myMapView.SetViewpoint(new Viewpoint(e.Position, 100000));
            });
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
        private async void UpdateSearch(string enteredText, string locationText, bool restrictToExtent = false)
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
            _myProgressBar.StartAnimating();

            // Get the location information
            IReadOnlyList<GeocodeResult> locations = await _geocoder.GeocodeAsync(enteredText, parameters);

            // Stop gracefully and show a message if the geocoder does not return a result
            if (locations.Count < 1)
            {
                _myProgressBar.StopAnimating(); // 1. Hide the progress bar
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
            _myProgressBar.StopAnimating();

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
            UIAlertView alertView = new UIAlertView("alert", message, null, "OK", null);
            alertView.Show();
        }

        /// <summary>
        /// Method used to keep the suggestions up-to-date for the location box
        /// </summary>
        private async void _myLocationBox_TextChanged(object sender, EventArgs e)
        {
            // Dismiss callout, if any
            UserInteracted();

            // Set the currently-updated text field
            _locationSearchActive = true;

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

            // Update the list of options
            _mySuggestionSource.TableItems = mutableResults.ToList();

            // Force the view to refresh
            _mySuggestionView.ReloadData();

            // Show the view
            _mySuggestionView.Hidden = false;
        }

        /// <summary>
        /// Method used to keep the suggestions up-to-date for the search box
        /// </summary>
        private async void _mySearchBox_TextChanged(object sender, EventArgs e)
        {
            // Dismiss callout, if any
            UserInteracted();

            // Set the currently-updated text field
            _locationSearchActive = false;

            // Get the current text
            string searchText = _mySearchBox.Text;

            // Get the current search location
            string locationText = _myLocationBox.Text;

            // Convert the list into a usable format for the suggest box
            IEnumerable<String> results = await GetSuggestResults(searchText, locationText, true);

            // Quit if there are no results
            if (results == null || results.Count() == 0) { return; }

            // Update the list of options
            _mySuggestionSource.TableItems = results.ToList();

            // Force the view to refresh
            _mySuggestionView.ReloadData();

            // Show the view
            _mySuggestionView.Hidden = false;
        }

        /// <summary>
        /// Method called to start a search that is restricted to results within the current extent
        /// </summary>
        private void _mySearchRestrictedButton_Click(object sender, EventArgs e)
        {
            // Dismiss callout, if any
            UserInteracted();

            // Get the search text
            string searchText = _mySearchBox.Text;

            // Get the location text
            string locationText = _myLocationBox.Text;

            // Run the search
            UpdateSearch(searchText, locationText, true);
        }

        /// <summary>
        /// Method called to start an unrestricted search
        /// </summary>
        private void _mySearchButton_Clicked(object sender, EventArgs e)
        {
            // Dismiss callout, if any
            UserInteracted();

            // Get the search text
            string searchText = _mySearchBox.Text;

            // Get the location text
            string locationText = _myLocationBox.Text;

            // Run the search
            UpdateSearch(searchText, locationText, false);
        }

        /// <summary>
        /// Called by the UITableView's data source to indicate that a suggestion was selected
        /// </summary>
        /// <param name="text">The selected suggestion</param>
        public void AcceptSuggestion(string text)
        {
            // Update the text for the currently active text box
            if (_locationSearchActive)
            {
                _myLocationBox.Text = text;
            }
            else
            {
                _mySearchBox.Text = text;
            }

            // Hide the suggestion view
            _mySuggestionView.Hidden = true;

            // Reset the suggestion items
            _mySuggestionSource.TableItems = new List<string>();
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