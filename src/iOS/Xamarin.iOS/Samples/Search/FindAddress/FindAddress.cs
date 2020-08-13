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
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.FindAddress
{
    [Register("FindAddress")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Find address",
        category: "Search",
        description: "Find the location for an address.",
        instructions: "For simplicity, the sample comes loaded with a set of suggested addresses. Choose an address from the suggestions or submit your own address to show its location on the map in a callout.",
        tags: new[] { "address", "geocode", "locator", "search" })]
    [ArcGISRuntime.Samples.Shared.Attributes.EmbeddedResource(@"PictureMarkerSymbols\pin_star_blue.png")]
    public class FindAddress : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UISearchBar _addressSearchBar;

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

        // Service URI to be provided to the LocatorTask (geocoder).
        private readonly Uri _serviceUri = new Uri("https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer");

        public FindAddress()
        {
            Title = "Find address";
        }

        private async void Initialize()
        {
            // Show a labeled imagery basemap.
            _myMapView.Map = new Map(Basemap.CreateImageryWithLabels());

            try
            {
                // Initialize the geocoder with the provided service URL.
                _geocoder = await LocatorTask.CreateAsync(_serviceUri);

                // Enable controls now that the geocoder is ready.
                _addressSearchBar.UserInteractionEnabled = true;
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        private void AddressSearchBar_Clicked(object sender, EventArgs e)
        {
            UpdateSearch();

            // Dismiss the keyboard.
            _addressSearchBar.ResignFirstResponder();
        }

        private async void UpdateSearch()
        {
            // Get the text in the search bar.
            string enteredText = _addressSearchBar.Text;

            // Clear existing marker.
            _myMapView.GraphicsOverlays.Clear();

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
                SuggestResult firstSuggestion = suggestions.First();
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
                _myMapView.GraphicsOverlays.Add(resultOverlay);

                // Update the map extent to show the marker.
                _myMapView.SetViewpoint(new Viewpoint(addresses.First().Extent));
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        /// <summary>
        /// Creates and returns a Graphic associated with the given MapPoint.
        /// </summary>
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

        private void AddressSearch_ListButtonClicked(object sender, EventArgs e)
        {
            // Create the alert view.
            UIAlertController alert = UIAlertController.Create("Suggestions", "Location searches to try", UIAlertControllerStyle.Alert);

            // Populate the view with one action per address suggestion.
            foreach (string address in _addresses)
            {
                alert.AddAction(UIAlertAction.Create(address, UIAlertActionStyle.Default, obj =>
                {
                    _addressSearchBar.Text = address;
                    UpdateSearch();
                }));
            }

            // Show the alert view.
            PresentViewController(alert, true, null);
        }

        /// <summary>
        /// Handle tap event on the map; displays callouts showing the address for a tapped search result.
        /// </summary>
        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Search for the graphics underneath the user's tap.
            try
            {
                IReadOnlyList<IdentifyGraphicsOverlayResult> results = await _myMapView.IdentifyGraphicsOverlaysAsync(e.Position, 12, false);

                // Return gracefully if there was no result.
                if (results.Count < 1 || results.First().Graphics.Count < 1)
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
                _myMapView.ShowCalloutAt(e.Location, calloutBody);
            }
            catch (Exception ex)
            {
                new UIAlertView("Error", ex.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = ApplicationTheme.BackgroundColor};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _addressSearchBar = new UISearchBar();
            _addressSearchBar.TranslatesAutoresizingMaskIntoConstraints = false;
            _addressSearchBar.UserInteractionEnabled = false;
            _addressSearchBar.ShowsSearchResultsButton = true;

            // Add the views.
            View.AddSubviews(_myMapView, _addressSearchBar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _addressSearchBar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _addressSearchBar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _addressSearchBar.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),

                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _myMapView.TopAnchor.ConstraintEqualTo(_addressSearchBar.BottomAnchor)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _addressSearchBar.ListButtonClicked += AddressSearch_ListButtonClicked;
            _addressSearchBar.SearchButtonClicked += AddressSearchBar_Clicked;
            _myMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _myMapView.GeoViewTapped -= MyMapView_GeoViewTapped;
            _addressSearchBar.ListButtonClicked -= AddressSearch_ListButtonClicked;
            _addressSearchBar.SearchButtonClicked -= AddressSearchBar_Clicked;
        }
    }
}