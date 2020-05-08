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
        name: "Find place",
        category: "Search",
        description: "Find places of interest near a location or within a specific area.",
        instructions: "Choose a type of place in the first field and an area to search within in the second field. Tap the Search button to show the results of the query on the map. Tap on a result pin to show its name and address. If you pan away from the result area, a \"Redo search in this area\" button will appear. Tap it to query again for the currently viewed area on the map.",
        tags: new[] { "POI", "businesses", "geocode", "locations", "locator", "places of interest", "point of interest", "search", "suggestions" })]
    [ArcGISRuntime.Samples.Shared.Attributes.EmbeddedResource(@"PictureMarkerSymbols\pin_star_blue.png")]
    public class FindPlace : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UITextField _searchBox;
        private UITextField _locationBox;
        private UITableView _suggestionView;
        private UIButton _searchButton;
        private UIButton _searchInViewButton;
        private UIActivityIndicatorView _activityView;

        // The LocatorTask provides geocoding services.
        private LocatorTask _geocoder;

        // Service URI to be provided to the LocatorTask (geocoder).
        private readonly Uri _serviceUri =
            new Uri("https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer");

        // Hold a suggestion source for the suggestion list view.
        private SuggestionSource _mySuggestionSource;

        // Keep track of whether the search or location is being actively edited.
        private bool _locationSearchActive = false;

        public FindPlace()
        {
            Title = "Find place";
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
            _searchInViewButton.Enabled = true;
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

            // Get the current device location.
            return _myMapView.LocationDisplay.Location.Position;
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
                new UIAlertView("alert", "No results found", (IUIAlertViewDelegate) null, "OK", null)
                    .Show(); // 2. Show a message.
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
            IReadOnlyList<IdentifyGraphicsOverlayResult> results =
                await _myMapView.IdentifyGraphicsOverlaysAsync(e.Position, 12, false);

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
        private async Task<List<string>> GetSuggestResultsAsync(string searchText, string location = "",
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

            // Return as a list of strings (corresponding to the label property on each result).
            return results.Select(result => result.Label).ToList();
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
            List<string> results = await GetSuggestResultsAsync(searchText);

            // Quit if there are no results.
            if (!results.Any())
            {
                return;
            }

            // Add a 'current location' option to the list.
            results.Insert(0, "Current location");

            // Update the list of options.
            _mySuggestionSource.TableItems = results;

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
            List<string> results = await GetSuggestResultsAsync(searchText, locationText, true);

            // Quit if there are no results.
            if (!results.Any())
            {
                return;
            }

            // Update the list of options.
            _mySuggestionSource.TableItems = results;

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
            try
            {
                // Dismiss callout, if any.
                UserInteracted();

                // Hide the suggestions.
                _suggestionView.Hidden = true;

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
            try
            {
                // Dismiss callout, if any.
                UserInteracted();

                // Hide the suggestions.
                _suggestionView.Hidden = true;

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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = UIColor.White};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            UIView formContainer = new UIView {BackgroundColor = UIColor.FromWhiteAlpha(1f, .8f)};
            formContainer.TranslatesAutoresizingMaskIntoConstraints = false;

            _searchBox = new UITextField();
            _searchBox.TranslatesAutoresizingMaskIntoConstraints = false;
            _searchBox.Text = "Coffee";
            _searchBox.BorderStyle = UITextBorderStyle.RoundedRect;
            _searchBox.LeftView = new UIView(new CGRect(0, 0, 5, 20));
            _searchBox.LeftViewMode = UITextFieldViewMode.Always;

            _locationBox = new UITextField();
            _locationBox.TranslatesAutoresizingMaskIntoConstraints = false;
            _locationBox.Text = "Current location";
            _locationBox.BorderStyle = UITextBorderStyle.RoundedRect;
            _locationBox.LeftView = new UIView(new CGRect(0, 0, 5, 20));
            _locationBox.LeftViewMode = UITextFieldViewMode.Always;

            _searchButton = new UIButton(UIButtonType.RoundedRect);
            _searchButton.BackgroundColor = UIColor.White;
            _searchButton.TranslatesAutoresizingMaskIntoConstraints = false;
            _searchButton.SetTitle("Search all", UIControlState.Normal);
            _searchButton.SetTitleColor(UIColor.Gray, UIControlState.Disabled);
            _searchButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _searchButton.Layer.CornerRadius = 5;
            _searchButton.Layer.BorderColor = View.TintColor.CGColor;
            _searchButton.Layer.BorderWidth = 1;

            _searchInViewButton = new UIButton(UIButtonType.RoundedRect);
            _searchInViewButton.BackgroundColor = UIColor.White;
            _searchInViewButton.TranslatesAutoresizingMaskIntoConstraints = false;
            _searchInViewButton.SetTitle("Search in view", UIControlState.Normal);
            _searchInViewButton.SetTitleColor(UIColor.Gray, UIControlState.Disabled);
            _searchInViewButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _searchInViewButton.Layer.CornerRadius = 5;
            _searchInViewButton.Layer.BorderColor = View.TintColor.CGColor;
            _searchInViewButton.Layer.BorderWidth = 1;

            _activityView = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
            _activityView.TranslatesAutoresizingMaskIntoConstraints = false;
            _activityView.HidesWhenStopped = true;
            _activityView.BackgroundColor = UIColor.FromWhiteAlpha(0, .5f);

            _suggestionView = new UITableView();
            _suggestionView.TranslatesAutoresizingMaskIntoConstraints = false;
            _suggestionView.Hidden = true;
            _mySuggestionSource = new SuggestionSource(null, this);
            _suggestionView.Source = _mySuggestionSource;
            _suggestionView.RowHeight = 24;

            // Add the views.
            View.AddSubviews(_myMapView, formContainer, _searchBox, _locationBox, _searchButton,
                _searchInViewButton, _activityView, _suggestionView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(formContainer.BottomAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),

                _searchBox.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor, 8),
                _searchBox.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor, 8),
                _searchBox.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor, -8),

                _locationBox.TopAnchor.ConstraintEqualTo(_searchBox.BottomAnchor, 8),
                _locationBox.LeadingAnchor.ConstraintEqualTo(_searchBox.LeadingAnchor),
                _locationBox.TrailingAnchor.ConstraintEqualTo(_searchBox.TrailingAnchor),

                _searchButton.TopAnchor.ConstraintEqualTo(_locationBox.BottomAnchor, 8),
                _searchButton.LeadingAnchor.ConstraintEqualTo(_searchBox.LeadingAnchor),
                _searchButton.TrailingAnchor.ConstraintEqualTo(View.CenterXAnchor, -4),
                _searchButton.HeightAnchor.ConstraintEqualTo(32),

                _searchInViewButton.TopAnchor.ConstraintEqualTo(_searchButton.TopAnchor),
                _searchInViewButton.LeadingAnchor.ConstraintEqualTo(View.CenterXAnchor, 4),
                _searchInViewButton.TrailingAnchor.ConstraintEqualTo(_searchBox.TrailingAnchor),
                _searchInViewButton.HeightAnchor.ConstraintEqualTo(32),

                formContainer.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                formContainer.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                formContainer.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                formContainer.BottomAnchor.ConstraintEqualTo(_searchInViewButton.BottomAnchor, 8),

                _activityView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _activityView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _activityView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _activityView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),

                _suggestionView.TopAnchor.ConstraintEqualTo(formContainer.BottomAnchor, 8),
                _suggestionView.LeadingAnchor.ConstraintEqualTo(_locationBox.LeadingAnchor, 8),
                _suggestionView.TrailingAnchor.ConstraintEqualTo(_locationBox.TrailingAnchor, -8),
                _suggestionView.HeightAnchor.ConstraintEqualTo(_suggestionView.RowHeight * 4)
            });
        }

        private bool HandleTextField(UITextField textField)
        {
            // This method allows pressing 'return' to dismiss the software keyboard.
            textField.ResignFirstResponder();
            return true;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _myMapView.GeoViewTapped += MapView_GeoViewTapped;
            _searchButton.TouchUpInside += SearchButton_Touched;
            _searchInViewButton.TouchUpInside += SearchRestrictedButton_Touched;
            _searchBox.AllEditingEvents += SearchBox_TextChanged;
            _locationBox.AllEditingEvents += LocationBox_TextChanged;
            _searchBox.ShouldReturn += HandleTextField;
            _locationBox.ShouldReturn += HandleTextField;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _myMapView.GeoViewTapped -= MapView_GeoViewTapped;
            _searchButton.TouchUpInside -= SearchButton_Touched;
            _searchInViewButton.TouchUpInside -= SearchRestrictedButton_Touched;
            _searchBox.AllEditingEvents -= SearchBox_TextChanged;
            _locationBox.AllEditingEvents -= LocationBox_TextChanged;
            _searchBox.ShouldReturn -= HandleTextField;
            _locationBox.ShouldReturn -= HandleTextField;

            // Stop the location data source.
            _myMapView.LocationDisplay?.DataSource?.StopAsync();
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
        [Weak] private FindPlace Owner;

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
            UITableViewCell cell = tableView.DequeueReusableCell(CellId) ??
                                   new UITableViewCell(UITableViewCellStyle.Default, CellId);

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