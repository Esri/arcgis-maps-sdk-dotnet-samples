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
using Windows.UI.Popups;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.UWP.Samples.FindAddress
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Find address",
        "Search",
        "This sample demonstrates how you can use the LocatorTask API to geocode an address and display it with a pin on the map. Tapping the pin displays the reverse-geocoded address in a callout.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.EmbeddedResource(@"PictureMarkerSymbols\pin_star_blue.png")]
    public partial class FindAddress
    {
        // Addresses for suggestion.
        private readonly string[] _addresses =
        {
            "277 N Avenida Caballeros, Palm Springs, CA",
            "380 New York St, Redlands, CA 92373",
            "Београд",
            "Москва",
            "北京"
        };

        // The LocatorTask provides geocoding services.
        private LocatorTask _geocoder;

        // Service Uri to be provided to the LocatorTask (geocoder).
        private readonly Uri _serviceUri =
            new Uri("https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer");

        public FindAddress()
        {
            InitializeComponent();

            // Setup the control references and execute initialization.
            Initialize();
        }

        private async void Initialize()
        {
            // Create new Map with basemap.
            Map myMap = new Map(Basemap.CreateImageryWithLabels());

            // Enable tap-for-info pattern on results.
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;

            // Assign the map to the MapView.
            MyMapView.Map = myMap;

            try
            {
                // Initialize the LocatorTask with the provided service Uri.
                _geocoder = await LocatorTask.CreateAsync(_serviceUri);

                // Enable UI controls now that the geocoder is ready.
                AutoSuggestBox.IsEnabled = true;
                AutoSuggestBox.IsSuggestionListOpen = true;
            }
            catch (Exception e)
            {
                await new MessageDialog(e.ToString(), "Error").ShowAsync();
            }
        }

        private async void UpdateSearch()
        {
            // Get the text in the search bar.
            string enteredText = AutoSuggestBox.Text;

            // Clear existing marker.
            MyMapView.GraphicsOverlays.Clear();

            // Return gracefully if the textbox is empty or the geocoder isn't ready.
            if (String.IsNullOrWhiteSpace(enteredText) || _geocoder == null)
            {
                return;
            }

            try
            {
                // Get suggestions based on the input text.
                IReadOnlyList<SuggestResult> suggestions = await _geocoder.SuggestAsync(enteredText);

                // Stop gracefully if there are no suggestions.
                if (suggestions.Count < 1)
                {
                    return;
                }

                // Get the full address for the first suggestion.
                SuggestResult firstSuggestion = suggestions[0];
                IReadOnlyList<GeocodeResult> addresses = await _geocoder.GeocodeAsync(firstSuggestion.Label);

                // Stop gracefully if the geocoder does not return a result.
                if (addresses.Count < 1)
                {
                    return;
                }

                // Place a marker on the map - 1. Create the overlay.
                GraphicsOverlay resultOverlay = new GraphicsOverlay();
                // 2. Get the Graphic to display.
                Graphic point = await GraphicForPoint(addresses.First().DisplayLocation);
                // 3. Add the Graphic to the GraphicsOverlay.
                resultOverlay.Graphics.Add(point);
                // 4. Add the GraphicsOverlay to the MapView.
                MyMapView.GraphicsOverlays.Add(resultOverlay);

                // Update the map extent to show the marker.
                MyMapView.SetViewpoint(new Viewpoint(addresses.First().Extent));
            }
            catch (Exception e)
            {
                await new MessageDialog(e.ToString(), "Error").ShowAsync();
            }
        }

        /// <summary>
        /// Creates and returns a Graphic associated with the given MapPoint.
        /// </summary>
        private async Task<Graphic> GraphicForPoint(MapPoint point)
        {
            // Get current assembly that contains the image.
            Assembly currentAssembly = GetType().GetTypeInfo().Assembly;

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
        /// Handle tap event on the map; displays callouts showing the address for a tapped search result.
        /// </summary>
        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Search for the graphics underneath the user's tap.
                IReadOnlyList<IdentifyGraphicsOverlayResult> results =
                    await MyMapView.IdentifyGraphicsOverlaysAsync(e.Position, 12, false);

                // Return gracefully if there was no result.
                if (results.Count < 1 || results[0].Graphics.Count < 1)
                {
                    return;
                }

                // Reverse geocode to get addresses.
                IReadOnlyList<GeocodeResult> addresses = await _geocoder.ReverseGeocodeAsync(e.Location);

                // Get the first result.
                GeocodeResult address = addresses.First();
                // Use the city and region for the Callout Title.
                string calloutTitle = address.Attributes["City"] + ", " + address.Attributes["Region"];
                // Use the metro area for the Callout Detail.
                string calloutDetail = address.Attributes["MetroArea"].ToString();

                // Define the callout.
                CalloutDefinition calloutBody = new CalloutDefinition(calloutTitle, calloutDetail);

                // Show the callout on the map at the tapped location.
                MyMapView.ShowCalloutAt(e.Location, calloutBody);
            }
            catch (Exception ex)
            {
                await new MessageDialog(ex.ToString(), "Error").ShowAsync();
            }
        }

        private void Search_Submitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            UpdateSearch();
        }

        private void AutoSuggestBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            AutoSuggestBox.Text = args.SelectedItem.ToString();
            UpdateSearch();
        }
    }
}