// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CoreGraphics;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.FindPlace
{
    [Register("FindPlace")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Find place",
        "Search",
        "This sample demonstrates how to use geocode functionality to search for points of interest, around a location or within an extent.",
        "1. Enter a point of interest you'd like to search for (e.g. 'Starbucks')\n2. Enter a search location or accept the default 'Current Location'\n3. Select 'search all' to get all results, or press 'search view' to only get results within the current extent.")]
    [ArcGISRuntime.Samples.Shared.Attributes.EmbeddedResource(@"PictureMarkerSymbols\pin_star_blue.png")]
    public class FindPlace : UIViewController
    {
        // The LocatorTask provides geocoding services.
        private LocatorTask _geocoder;

        // Service URI to be provided to the LocatorTask (geocoder).
        private readonly Uri _serviceUri = new Uri("https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer");

        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UITextField _searchBox = new UITextField();
        private readonly UITextField _locationBox = new UITextField();
        private readonly UITableView _suggestionView = new UITableView();
        private readonly UIToolbar _toolbar = new UIToolbar();

        private readonly UIButton _searchButton = new UIButton(UIButtonType.RoundedRect)
        {
            BackgroundColor = UIColor.White
        };

        private readonly UIButton _searchRestrictedButton = new UIButton(UIButtonType.RoundedRect)
        {
            BackgroundColor = UIColor.White
        };

        private readonly UIActivityIndicatorView _activityView = new UIActivityIndicatorView
        {
            Hidden = true
        };

        // Hold a suggestion source for the suggestion list view.
        private SuggestionSource _mySuggestionSource;

        // Keep track of whether the search or location is being actively edited.
        private bool _locationSearchActive = false;

        public FindPlace()
        {
            Title = "Find place";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references, and execute initialization.
            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Size.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat controlHeight = 30;
                nfloat margin = 5;
                nfloat width = View.Frame.Width - 2 * margin;
                nfloat halfWidth = View.Frame.Width / 2 - 2 * margin;

                // Reposition the views.
                _toolbar.Frame = new CGRect(0, topMargin, View.Bounds.Width, controlHeight * 3 + margin * 4);
                _myMapView.ViewInsets = new UIEdgeInsets(_toolbar.Frame.Bottom, 0, 0, 0);
                _searchBox.Frame = new CGRect(margin, topMargin += margin, width, controlHeight);
                _locationBox.Frame = new CGRect(margin, topMargin += margin + controlHeight, width, controlHeight);
                _searchButton.Frame = new CGRect(margin, topMargin += margin + controlHeight, halfWidth, controlHeight);
                _searchRestrictedButton.Frame = new CGRect(halfWidth + 3 * margin, topMargin, halfWidth, controlHeight);
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _suggestionView.Frame = new CGRect(2 * margin, topMargin + controlHeight, width - 2 * margin, 8 * controlHeight);
                _activityView.Frame = new CGRect(0, _toolbar.Frame.Top, View.Bounds.Width, _toolbar.Frame.Height);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private void CreateLayout()
        {
            // Set the text on the two buttons.
            _searchButton.SetTitle("Search all", UIControlState.Normal);
            _searchRestrictedButton.SetTitle("Search in view", UIControlState.Normal);

            // Set the default location and search text.
            _locationBox.Text = "Current location";
            _searchBox.Text = "Coffee";

            // Allow pressing 'return' to dismiss the keyboard.
            _locationBox.ShouldReturn += textField =>
            {
                textField.ResignFirstResponder();
                return true;
            };
            _searchBox.ShouldReturn += textField =>
            {
                textField.ResignFirstResponder();
                return true;
            };

            // Gray out the buttons when they are disabled.
            _searchButton.SetTitleColor(UIColor.Gray, UIControlState.Disabled);
            _searchRestrictedButton.SetTitleColor(UIColor.Gray, UIControlState.Disabled);

            // Change button color when enabled.
            _searchButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _searchRestrictedButton.SetTitleColor(View.TintColor, UIControlState.Normal);

            // Hide the activity indicator (progress bar) when stopped.
            _activityView.HidesWhenStopped = true;
            _activityView.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge;
            _activityView.BackgroundColor = UIColor.FromWhiteAlpha(0, .5f);

            // Make the textboxes look nice.
            _locationBox.BorderStyle = UITextBorderStyle.RoundedRect;
            _searchBox.BorderStyle = UITextBorderStyle.RoundedRect;

            // Make the buttons look nice.
            _searchButton.Layer.CornerRadius = 5;
            _searchRestrictedButton.Layer.CornerRadius = 5;

            // Make sure the suggestion list is hidden by default.
            _suggestionView.Hidden = true;

            // Create the suggestion source.
            _mySuggestionSource = new SuggestionSource(null, this);

            // Set the source for the table view.
            _suggestionView.Source = _mySuggestionSource;

            // Enable tap-for-info pattern on results.
            _myMapView.GeoViewTapped += MapView_GeoViewTapped;

            // Listen for taps on the search buttons.
            _searchButton.TouchUpInside += SearchButton_Touched;
            _searchRestrictedButton.TouchUpInside += SearchRestrictedButton_Touched;

            // Listen for text-changed events.
            _searchBox.AllEditingEvents += SearchBox_TextChanged;
            _locationBox.AllEditingEvents += LocationBox_TextChanged;

            // Set padding on the text views.
            _searchBox.LeftView = new UIView(new CGRect(0, 0, 5, 20));
            _searchBox.LeftViewMode = UITextFieldViewMode.Always;
            _locationBox.LeftView = new UIView(new CGRect(0, 0, 5, 20));
            _locationBox.LeftViewMode = UITextFieldViewMode.Always;

            // Add the views.
            View.AddSubviews(_myMapView, _toolbar, _searchBox, _locationBox, _searchButton,
                _searchRestrictedButton, _activityView, _suggestionView);
        }

        private async void Initialize()
        {
            // Show a new map with streets basemap.
            _myMapView.Map = new Map(Basemap.CreateStreets());

            // Initialize the geocoder with the provided service URL
            _geocoder = await LocatorTask.CreateAsync(_serviceUri);

            // Subscribe to location changed event so that map can zoom to location.
            _myMapView.LocationDisplay.LocationChanged += LocationDisplay_LocationChanged;

            // Enable location display on the map.
            _myMapView.LocationDisplay.IsEnabled = true;

            // Enable controls now that the geocoder is ready.
            _locationBox.Enabled = true;
            _searchBox.Enabled = true;
            _searchButton.Enabled = true;
            _searchRestrictedButton.Enabled = true;
        }

        private void LocationDisplay_LocationChanged(object sender, Esri.ArcGISRuntime.Location.Location e)
        {
            // Return if position is null; event is raised with null location after.
            if (e.Position == null)
            {
                return;
            }

            // Unsubscribe from further events; only want to zoom to location once.
            ((LocationDisplay) sender).LocationChanged -= LocationDisplay_LocationChanged;

            // Need to use this to interact with UI elements because this function is called from a background thread.
            InvokeOnMainThread(() => _myMapView.SetViewpoint(new Viewpoint(e.Position, 100000)));
        }

        /// <summary>
        /// Gets the map point corresponding to the text in the location textbox.
        /// If the text is 'Current Location', the returned map point will be the device's location.
        /// </summary>
        private async Task<MapPoint> GetSearchMapPoint(string locationText)
        {
            // Get the point for the search text.
            if (locationText != "Current location")
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

        /// <summary>
        /// Runs a search and populates the map with results based on the provided information.
        /// </summary>
        /// <param name="enteredText">Results to search for.</param>
        /// <param name="locationText">Location around which to find results.</param>
        /// <param name="restrictToExtent">If true, limits results to only those that are within the current extent.</param>
        private async Task UpdateSearchAsync(string enteredText, string locationText, bool restrictToExtent = false)
        {
            // Clear any existing markers.
            _myMapView.GraphicsOverlays.Clear();

            // Return gracefully if the textbox is empty or the geocoder isn't ready.
            if (string.IsNullOrWhiteSpace(enteredText) || _geocoder == null)
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
                parameters.SearchArea = _myMapView.VisibleArea;
            }

            // Show the progress bar.
            _activityView.StartAnimating();

            // Get the location information.
            IReadOnlyList<GeocodeResult> locations = await _geocoder.GeocodeAsync(enteredText, parameters);

            // Stop gracefully and show a message if the geocoder does not return a result.
            if (locations.Count < 1)
            {
                _activityView.StopAnimating(); // 1. Hide the progress bar.
                new UIAlertView("alert", "No results found", (IUIAlertViewDelegate) null, "OK", null).Show(); // 2. Show a message.
                return; // 3. Stop.
            }

            // Create the GraphicsOverlay so that results can be drawn on the map.
            GraphicsOverlay resultOverlay = new GraphicsOverlay();

            // Add each address to the map.
            foreach (GeocodeResult location in locations)
            {
                // Get the Graphic to display.
                Graphic point = await GraphicForPointAsync(location.DisplayLocation);

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
            _activityView.StopAnimating();

            // Add the GraphicsOverlay to the MapView.
            _myMapView.GraphicsOverlays.Add(resultOverlay);

            // Update the map viewpoint.
            await _myMapView.SetViewpointGeometryAsync(resultOverlay.Extent, 50);
        }

        /// <summary>
        /// Creates and returns a Graphic associated with the given MapPoint.
        /// </summary>
        private async Task<Graphic> GraphicForPointAsync(MapPoint point)
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

        /// <summary>
        /// Shows a callout for any tapped graphics.
        /// </summary>
        private async void MapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
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

            // Get the title; manually added to the point's attributes in UpdateSearchAsync.
            string title = matchingGraphic.Attributes["Match_Title"] as string;

            // Get the address; manually added to the point's attributes in UpdateSearchAsync.
            string address = matchingGraphic.Attributes["Match_Address"] as string;

            // Define the callout.
            CalloutDefinition calloutBody = new CalloutDefinition(title, address);

            // Show the callout on the map at the tapped location.
            _myMapView.ShowCalloutAt(e.Location, calloutBody);
        }

        /// <summary>
        /// Returns a list of suggestions based on the input search text and limited by the specified parameters.
        /// </summary>
        /// <param name="searchText">Text to get suggestions for.</param>
        /// <param name="location">Location around which to look for suggestions.</param>
        /// <param name="poiOnly">If true, restricts suggestions to only Points of Interest (e.g. businesses, parks),
        /// rather than all matching results.</param>
        /// <returns>List of suggestions as strings or null if suggestions couldn't be retrieved.</returns>
        private async Task<IEnumerable<string>> GetSuggestResultsAsync(string searchText, string location = "", bool poiOnly = false)
        {
            // Quit if string is null, empty, or whitespace.
            if (String.IsNullOrWhiteSpace(searchText))
            {
                return null;
            }

            // Quit if the geocoder isn't ready.
            if (_geocoder == null)
            {
                return null;
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

            // Return as a list of strings (corresponding to the label property on each result).
            return results.Select(result => result.Label);
        }

        /// <summary>
        /// Method used to keep the suggestions up-to-date for the location box.
        /// </summary>
        private async void LocationBox_TextChanged(object sender, EventArgs e)
        {
            // Dismiss callout, if any.
            UserInteracted();

            // Set the currently-updated text field.
            _locationSearchActive = true;

            // Get the current text.
            string searchText = _locationBox.Text;

            // Get the results.
            IEnumerable<string> results = await GetSuggestResultsAsync(searchText);
            List<string> resultList = results?.ToList();

            // Quit if there are no results.
            if (resultList == null || !resultList.Any())
            {
                return;
            }

            // Add a 'current location' option to the list.
            resultList.Insert(0, "Current location");

            // Update the list of options.
            _mySuggestionSource.TableItems = resultList;

            // Force the view to refresh.
            _suggestionView.ReloadData();

            // Show the view.
            _suggestionView.Hidden = false;
        }

        /// <summary>
        /// Method used to keep the suggestions up-to-date for the search box.
        /// </summary>
        private async void SearchBox_TextChanged(object sender, EventArgs e)
        {
            // Dismiss callout, if any.
            UserInteracted();

            // Set the currently-updated text field.
            _locationSearchActive = false;

            // Get the current text.
            string searchText = _searchBox.Text;

            // Get the current search location.
            string locationText = _locationBox.Text;

            // Convert the list into a usable format for the suggest box.
            IEnumerable<string> results = await GetSuggestResultsAsync(searchText, locationText, true);
            List<string> resultList = results?.ToList();

            // Quit if there are no results.
            if (resultList == null || !resultList.Any())
            {
                return;
            }

            // Update the list of options.
            _mySuggestionSource.TableItems = resultList;

            // Force the view to refresh.
            _suggestionView.ReloadData();

            // Show the view.
            _suggestionView.Hidden = false;
        }

        /// <summary>
        /// Method called to start a search that is restricted to results within the current extent.
        /// </summary>
        private async void SearchRestrictedButton_Touched(object sender, EventArgs e)
        {
            // Dismiss callout, if any.
            try
            {
                UserInteracted();

                // Get the search text.
                string searchText = _searchBox.Text;

                // Get the location text.
                string locationText = _locationBox.Text;

                // Run the search.
                await UpdateSearchAsync(searchText, locationText, true);
            }
            // Uncaught exceptions in async void method will crash the app.
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Method called to start an unrestricted search.
        /// </summary>
        private async void SearchButton_Touched(object sender, EventArgs e)
        {
            // Dismiss callout, if any.
            try
            {
                UserInteracted();

                // Get the search text.
                string searchText = _searchBox.Text;

                // Get the location text.
                string locationText = _locationBox.Text;

                // Run the search.
                await UpdateSearchAsync(searchText, locationText, false);
            }
            // Uncaught exceptions in async void method will crash the app.
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        /// <summary>
        /// Called by the UITableView's data source to indicate that a suggestion was selected.
        /// </summary>
        /// <param name="text">The selected suggestion.</param>
        public void AcceptSuggestion(string text)
        {
            // Update the text for the currently active text box.
            if (_locationSearchActive)
            {
                _locationBox.Text = text;
            }
            else
            {
                _searchBox.Text = text;
            }

            // Hide the suggestion view.
            _suggestionView.Hidden = true;

            // Reset the suggestion items.
            _mySuggestionSource.TableItems = new List<string>();
        }

        /// <summary>
        /// Method to handle hiding the callout, should be called by all UI event handlers.
        /// </summary>
        private void UserInteracted()
        {
            // Hide the callout.
            _myMapView.DismissCallout();
        }
    }
    /// <summary>
    /// Class defines how a UITableView renders its contents.
    /// This implements the suggestion UI for the table view.
    /// </summary>
    public class SuggestionSource : UITableViewSource
    {
        // List of strings; these will be the suggestions.
        public List<string> TableItems = new List<string>();

        // Used when re-using cells to ensure that a cell of the right type is used.
        private const string CellId = "TableCell";

        // Hold a reference to the owning view controller; this will be the active instance of FindPlace.
        private FindPlace Owner { get; }

        public SuggestionSource(List<string> items, FindPlace owner)
        {
            // Set the items.
            if (items != null)
            {
                TableItems = items;
            }

            // Set the owner.
            Owner = owner;
        }

        /// <summary>
        /// This method gets a table view cell for the suggestion at the specified index.
        /// </summary>
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Try to get a re-usable cell (this is for performance). If there are no cells, create a new one.
            UITableViewCell cell = tableView.DequeueReusableCell(CellId) ?? new UITableViewCell(UITableViewCellStyle.Default, CellId);

            // Set the text on the cell.
            cell.TextLabel.Text = TableItems[indexPath.Row];

            // Return the cell.
            return cell;
        }

        /// <summary>
        /// This method allows the UITableView to know how many rows to render.
        /// </summary>
        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return TableItems.Count;
        }

        /// <summary>
        /// Method called when a row is selected; notifies the primary view.
        /// </summary>
        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            // Deselect the row.
            tableView.DeselectRow(indexPath, true);

            // Accept the suggestion.
            Owner.AcceptSuggestion(TableItems[indexPath.Row]);
        }
    }
}