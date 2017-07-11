// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reflection;

namespace ArcGISRuntime.UWP.Samples.FindAddress
{
    public partial class FindAddress
    {

        // Create the Locator Task to perform geocoding work with an online service
        private LocatorTask _geocoder = new LocatorTask(new System.Uri("https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer"));

        // List of addresses for use in suggestions
        string[] _addresses = {
            "277 N Avenida Caballeros, Palm Springs, CA",
            "380 New York St, Redlands, CA 92373",
            "Београд",
            "Москва",
            "北京"
        };

        public FindAddress()
        {
            InitializeComponent();

            // Setup the control references and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateTopographic());

            // Assign the map to the MapView
            MyMapView.Map = myMap;
            
        }

        private void mySearchField_TextChanged(object sender, TextChangedEventArgs e)
        {
            updateSearch();
        }

        private async void updateSearch()
        {
            var enteredText = MySearchBox.Text;

            // Clear existing marker
            MyMapView.GraphicsOverlays.Clear();

            // Return gracefully if the textbox is empty
            if (string.IsNullOrWhiteSpace(enteredText)) { return; }

            // Get the nearest suggestion to entered text
            IReadOnlyList<SuggestResult> suggestions = await _geocoder.SuggestAsync(enteredText);

            // Stop gracefully if there are no suggestions
            if (suggestions.Count < 1) { return; }

            // Get the full address for the first suggestion
            IReadOnlyList<GeocodeResult> addresses = await _geocoder.GeocodeAsync(suggestions[0].Label);

            // Stop gracegully if the geocoder does not return a result
            if (addresses.Count < 1) { return; }

            // Place a marker on the map
            var resultOverlay = new GraphicsOverlay();
            var point = await _graphicForPoint(addresses[0].DisplayLocation);

            // Record the address with the overlay for easy recall when the graphic is tapped
            point.Attributes.Add("Address", addresses[0].Label);
            resultOverlay.Graphics.Add(point);
            MyMapView.GraphicsOverlays.Add(resultOverlay);
            await MyMapView.SetViewpointGeometryAsync(addresses[0].Extent);
        }

        /// <summary>
        /// Creates a graphic for the specified map point asynchronously
        /// </summary>
        /// <returns>The for point.</returns>
        /// <param name="point">Point.</param>
        private async Task<Graphic> _graphicForPoint(MapPoint point)
        {
            // Get current assembly that contains the image
            var currentAssembly = this.GetType().GetTypeInfo().Assembly;

            // Get image as a stream from the resources
            // Picture is defined as EmbeddedResource and DoNotCopy
            var resourceStream = currentAssembly.GetManifestResourceStream(
                "ArcGISRuntime.UWP.Resources.PictureMarkerSymbols.pin_star_red.png");

            // Create new symbol using asynchronous factory method from stream
            PictureMarkerSymbol pinSymbol = await PictureMarkerSymbol.CreateAsync(resourceStream);
            pinSymbol.Width = 15;
            pinSymbol.Height = 30;
            pinSymbol.OffsetX = pinSymbol.Width / 2;
            pinSymbol.OffsetY = pinSymbol.Height / 2;
            return new Graphic(point, pinSymbol);
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            MySearchBox.Text = ((MenuFlyoutItem)sender).Text;
            updateSearch();
        }
    }
}
