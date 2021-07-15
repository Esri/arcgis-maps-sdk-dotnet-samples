// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using ArcGISRuntime;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geocoding;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.OfflineGeocode
{
    [Register("OfflineGeocode")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Offline geocode",
        category: "Search",
        description: "Geocode addresses to locations and reverse geocode locations to addresses offline.",
        instructions: "Type the address in the Search menu option or select from the list to `Geocode` the address and view the result on the map. Tap the location you want to reverse geocode. Tap the pin to see the full address.",
        tags: new[] { "geocode", "geocoder", "locator", "offline", "package", "query", "search" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("22c3083d4fa74e3e9b25adfc9f8c0496", "344e3b12368543ef84045ef9aa3c32ba")]
    public class OfflineGeocode : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UISearchBar _addressSearchBar;

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
            Title = "Offline geocode";
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

            try
            {
                // Get the path to the locator.
                string locatorPath = DataManager.GetDataFolder("344e3b12368543ef84045ef9aa3c32ba", "san-diego-locator.loc");

                // Load the geocoder.
                _geocoder = await LocatorTask.CreateAsync(new Uri(locatorPath));

                // Enable controls now that the geocoder is ready.
                _addressSearchBar.UserInteractionEnabled = true;
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        private async void UpdateSearch()
        {
            // Get the text in the search bar.
            string enteredText = _addressSearchBar.Text;

            // Clear existing marker.
            _myMapView.GraphicsOverlays[0].Graphics.Clear();
            _myMapView.DismissCallout();

            // Return if the textbox is empty or the geocoder isn't ready.
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
                    new UIAlertView("No results", "No results found.", (IUIAlertViewDelegate) null, "OK", null).Show();
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
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
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
                    new UIAlertView("No results", "No results found.", (IUIAlertViewDelegate) null, "OK", null).Show();
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
                new UIAlertView("Error", ex.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        private void AddressSearchBar_Clicked(object sender, EventArgs e)
        {
            UpdateSearch();

            // Dismiss the keyboard.
            _addressSearchBar.ResignFirstResponder();
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
            _myMapView.GeoViewTapped += MyMapView_GeoViewTapped;
            _addressSearchBar.ListButtonClicked += AddressSearch_ListButtonClicked;
            _addressSearchBar.SearchButtonClicked += AddressSearchBar_Clicked;
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