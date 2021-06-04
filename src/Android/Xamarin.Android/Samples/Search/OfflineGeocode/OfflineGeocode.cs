// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using ArcGISRuntime.Samples.Managers;
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

namespace ArcGISRuntimeXamarin.Samples.OfflineGeocode
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Offline geocode",
        category: "Search",
        description: "Geocode addresses to locations and reverse geocode locations to addresses offline.",
        instructions: "Type the address in the Search menu option or select from the list to `Geocode` the address and view the result on the map. Tap the location you want to reverse geocode. Tap the pin to see the full address.",
        tags: new[] { "geocode", "geocoder", "locator", "offline", "package", "query", "search" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("22c3083d4fa74e3e9b25adfc9f8c0496", "344e3b12368543ef84045ef9aa3c32ba")]
    public class OfflineGeocode : Activity
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private EditText _addressSearchBar;
        private Button _suggestButton;
        private Button _searchButton;

        // Addresses for suggestion.
        private readonly string[] _addresses =
        {
            "910 N Harbor Dr, San Diego, CA 92101",
            "2920 Zoo Dr, San Diego, CA 92101",
            "111 W Harbor Dr, San Diego, CA 92101",
            "868 4th Ave, San Diego, CA 92101",
            "750 A St, San Diego, CA 92101"
        };

        // The LocatorTask provides geocoding services.
        private LocatorTask _geocoder;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Offline geocode";

            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            // Get the offline tile package and use it as a basemap.
            string basemapPath = DataManager.GetDataFolder("22c3083d4fa74e3e9b25adfc9f8c0496", "streetmap_SD.tpkx");
            ArcGISTiledLayer tiledBasemapLayer = new ArcGISTiledLayer(new TileCache(basemapPath));

            // Create new Map with basemap.
            Map myMap = new Map(new Basemap(tiledBasemapLayer));

            // Provide Map to the MapView.
            _myMapView.Map = myMap;

            // Add a graphics overlay for showing pins.
            _myMapView.GraphicsOverlays.Add(new GraphicsOverlay());

            // Enable tap-for-info pattern on results.
            _myMapView.GeoViewTapped += MyMapView_GeoViewTapped;

            try
            {
                // Get the path to the locator.
                string locatorPath = DataManager.GetDataFolder("344e3b12368543ef84045ef9aa3c32ba", "san-diego-locator.loc");

                // Load the geocoder.
                _geocoder = await LocatorTask.CreateAsync(new Uri(locatorPath));

                // Enable interaction now that the geocoder is ready.
                _suggestButton.Enabled = true;
                _addressSearchBar.Enabled = true;
                _searchButton.Enabled = true;
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.ToString()).SetTitle("Error").Show();
            }
        }

        private void _searchHintButton_Click(object sender, EventArgs e)
        {
            // Create an AlertDialog.
            AlertDialog.Builder builder = new AlertDialog.Builder(this);

            // Provide the addresses; lambda updates the search with the selected item.
            builder.SetTitle("Suggestions").SetItems(_addresses, (_sender, _e) =>
            {
                string address = _addresses[_e.Which]; // get the selected address
                _addressSearchBar.Text = address;
                updateSearch();
            });

            // Show the dialog.
            AlertDialog alert = builder.Create();
            alert.Show();
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            updateSearch();
        }

        private async void updateSearch()
        {
            // Get the text in the search bar.
            string enteredText = _addressSearchBar.Text;

            // Clear existing marker.
            _myMapView.GraphicsOverlays[0].Graphics.Clear();
            _myMapView.DismissCallout();

            // Return gracefully if the textbox is empty or the geocoder isn't ready.
            if (String.IsNullOrWhiteSpace(enteredText) || _geocoder == null)
            {
                return;
            }

            try
            {
                // Get suggestions based on the input text.
                IReadOnlyList<GeocodeResult> geocodeResults = await _geocoder.GeocodeAsync(enteredText);

                // Stop if there are no suggestions.
                if (!geocodeResults.Any())
                {
                    new AlertDialog.Builder(this).SetMessage("No results found.").SetTitle("No results").Show();
                    return;
                }

                // Get the full address for the first suggestion.
                GeocodeResult firstSuggestion = geocodeResults.First();
                IReadOnlyList<GeocodeResult> addresses = await _geocoder.GeocodeAsync(firstSuggestion.Label);

                // Stop if the geocoder does not return a result.
                if (addresses.Count < 1)
                {
                    return;
                }

                // Show a graphic for the address.
                Graphic point = await GraphicForPoint(addresses.First().DisplayLocation);
                _myMapView.GraphicsOverlays[0].Graphics.Add(point);

                // Update the map extent to show the marker.
                _myMapView.SetViewpoint(new Viewpoint(addresses.First().Extent));
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.ToString()).SetTitle("Error").Show();
            }
        }

        private async Task<Graphic> GraphicForPoint(MapPoint point)
        {
            // Get current assembly that contains the image
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            // Get image as a stream from the resources
            // Picture is defined as EmbeddedResource and DoNotCopy
            Stream resourceStream = currentAssembly.GetManifestResourceStream(
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

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Clear existing callout and graphics.
                _myMapView.DismissCallout();
                _myMapView.GraphicsOverlays[0].Graphics.Clear();

                // Add a graphic for the tapped point.
                Graphic pinGraphic = await GraphicForPoint(e.Location);
                _myMapView.GraphicsOverlays[0].Graphics.Add(pinGraphic);

                // Reverse geocode to get addresses.
                ReverseGeocodeParameters parameters = new ReverseGeocodeParameters();
                parameters.ResultAttributeNames.Add("*");
                parameters.MaxResults = 1;
                IReadOnlyList<GeocodeResult> addresses = await _geocoder.ReverseGeocodeAsync(e.Location, parameters);

                // Skip if there are no results.
                if (!addresses.Any())
                {
                    new AlertDialog.Builder(this).SetMessage("No results found.").SetTitle("No results").Show();
                    return;
                }

                // Get the first result.
                GeocodeResult address = addresses.First();

                // Use the address as the callout title.
                string calloutTitle = address.Attributes["Street"].ToString();
                string calloutDetail = address.Attributes["City"] + ", " + address.Attributes["State"] + " " + address.Attributes["ZIP"];

                // Define the callout.
                CalloutDefinition calloutBody = new CalloutDefinition(calloutTitle, calloutDetail);

                // Show the callout on the map at the tapped location.
                _myMapView.ShowCalloutForGeoElement(pinGraphic, e.Position, calloutBody);
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.ToString()).SetTitle("Error").Show();
            }
        }

        private void CreateLayout()
        {
            // Initialize the layout.
            LinearLayout layout = new LinearLayout(this) {Orientation = Orientation.Vertical};
            LinearLayout searchBarLayout = new LinearLayout(this);

            // Add the search bar.
            _addressSearchBar = new EditText(this)
            {
                LayoutParameters = new LinearLayout.LayoutParams(
                    ViewGroup.LayoutParams.MatchParent,
                    ViewGroup.LayoutParams.MatchParent,
                    1.0f
                ),
                InputType = InputTypes.ClassText | InputTypes.TextVariationNormal
            };
            _addressSearchBar.SetMaxLines(1);
            layout.AddView(searchBarLayout);
            searchBarLayout.AddView(_addressSearchBar);

            // Add a search button.
            _searchButton = new Button(this) {Text = "Search"};
            searchBarLayout.AddView(_searchButton);

            // Add a suggestion button.
            _suggestButton = new Button(this) {Text = "Suggest"};
            layout.AddView(_suggestButton);

            // Add the MapView to the layout.
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Keep the search bar from overflowing into multiple lines.
            _addressSearchBar.SetMaxLines(1);

            // Show the layout in the app.
            SetContentView(layout);

            // Disable the buttons and search bar until the geocoder is ready.
            _suggestButton.Enabled = false;
            _addressSearchBar.Enabled = false;
            _searchButton.Enabled = false;

            // Hook up the UI event handlers for suggestion & search.
            _suggestButton.Click += _searchHintButton_Click;
            _searchButton.Click += SearchButton_Click;
        }
    }
}