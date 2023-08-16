// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
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

namespace ArcGIS.WPF.Samples.OfflineGeocode
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Offline geocode",
        category: "Search",
        description: "Geocode addresses to locations and reverse geocode locations to addresses offline.",
        instructions: "Type the address in the Search menu option or select from the list to `Geocode` the address and view the result on the map. Tap the location you want to reverse geocode. Tap the pin to see the full address.",
        tags: new[] { "geocode", "geocoder", "locator", "offline", "package", "query", "search" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("22c3083d4fa74e3e9b25adfc9f8c0496", "3424d442ebe54f3cbf34462382d3aebe")]
    public partial class OfflineGeocode
    {
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

        public OfflineGeocode()
        {
            InitializeComponent();

            // Setup the control references and execute initialization
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Get the offline tile package and use it as a basemap.
            string basemapPath = DataManager.GetDataFolder("22c3083d4fa74e3e9b25adfc9f8c0496", "streetmap_SD.tpkx");
            ArcGISTiledLayer tiledBasemapLayer = new ArcGISTiledLayer(new TileCache(basemapPath));

            // Create new Map with basemap.
            Map myMap = new Map(new Basemap(tiledBasemapLayer));

            // Provide Map to the MapView.
            MyMapView.Map = myMap;

            // Add a graphics overlay for showing pins.
            MyMapView.GraphicsOverlays.Add(new GraphicsOverlay());

            // Enable tap-for-info pattern on results.
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;

            // Update suggestions.
            SearchBox.ItemsSource = _addresses;

            // Initialize the LocatorTask with the provided service Uri.
            try
            {
                // Get the path to the locator.
                string locatorPath = DataManager.GetDataFolder("3424d442ebe54f3cbf34462382d3aebe", "SanDiego_StreetAddress.loc");

                // Load the geocoder.
                _geocoder = await LocatorTask.CreateAsync(new Uri(locatorPath));

                // Enable UI controls now that the LocatorTask is ready.
                SearchBox.IsEnabled = true;
                SearchButton.IsEnabled = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
            }
        }

        private async Task UpdateSearch()
        {
            // Get the text in the search bar.
            string enteredText = SearchBox.Text;

            // Clear existing marker.
            MyMapView.GraphicsOverlays[0].Graphics.Clear();
            MyMapView.DismissCallout();

            // Return if the textbox is empty or the geocoder isn't ready.
            if (string.IsNullOrWhiteSpace(enteredText) || _geocoder == null) return;

            try
            {
                // Get suggestions based on the input text.
                IReadOnlyList<GeocodeResult> geocodeResults = await _geocoder.GeocodeAsync(enteredText);

                // Stop if there are no suggestions.
                if (!geocodeResults.Any())
                {
                    MessageBox.Show("No results found");
                    return;
                }

                // Get the full address for the first suggestion.
                GeocodeResult firstSuggestion = geocodeResults.First();
                IReadOnlyList<GeocodeResult> addresses = await _geocoder.GeocodeAsync(firstSuggestion.Label);

                // Stop if the geocoder does not return a result.
                if (addresses.Count < 1) return;

                // Show a graphic for the address.
                Graphic point = await GraphicForPoint(addresses.First().DisplayLocation);
                MyMapView.GraphicsOverlays[0].Graphics.Add(point);

                // Update the map extent to show the marker.
                MyMapView.SetViewpoint(new Viewpoint(addresses.First().Extent));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
            }
        }

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Clear existing callout and graphics.
                MyMapView.DismissCallout();
                MyMapView.GraphicsOverlays[0].Graphics.Clear();

                // Add a graphic for the tapped point.
                Graphic pinGraphic = await GraphicForPoint(e.Location);
                MyMapView.GraphicsOverlays[0].Graphics.Add(pinGraphic);

                // Reverse geocode to get addresses.
                ReverseGeocodeParameters parameters = new ReverseGeocodeParameters();
                parameters.ResultAttributeNames.Add("*");
                parameters.MaxResults = 1;
                IReadOnlyList<GeocodeResult> addresses = await _geocoder.ReverseGeocodeAsync(e.Location, parameters);

                // Skip if there are no results.
                if (!addresses.Any())
                {
                    MessageBox.Show("No results found.", "No results");
                    return;
                }

                // Get the first result.
                GeocodeResult address = addresses.First();

                // Use the address as the callout title.
                string calloutTitle = address.Attributes["StAddr"].ToString();
                string calloutDetail = address.Attributes["City"].ToString() + ", " + address.Attributes["RegionAbbr"] + " " + address.Attributes["Postal"];

                // Define the callout.
                CalloutDefinition calloutBody = new CalloutDefinition(calloutTitle, calloutDetail);

                // Show the callout on the map at the tapped location.
                MyMapView.ShowCalloutForGeoElement(pinGraphic, e.Position, calloutBody);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }

        private void OnSuggestionChosen(object sender, SelectionChangedEventArgs e)
        {
            // Return if the user is typing.
            if (SearchBox.SelectedValue == null) return;

            // Update the search.
            SearchBox.Text = SearchBox.SelectedValue.ToString();
            _ = UpdateSearch();
        }

        private void Search_Clicked(object sender, RoutedEventArgs e)
        {
            _ = UpdateSearch();
        }

        private async Task<Graphic> GraphicForPoint(MapPoint point)
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
                // The image is a pin; offset the image so that the pinpoint
                //     is on the point rather than the image's true center.
                pinSymbol.LeaderOffsetX = 30;
                pinSymbol.OffsetY = 14;
            }

            return new Graphic(point, pinSymbol);
        }
    }
}