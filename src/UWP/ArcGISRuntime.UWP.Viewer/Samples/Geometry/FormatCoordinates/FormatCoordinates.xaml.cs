// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.UWP.Samples.FormatCoordinates
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Format coordinates",
        "Geometry",
        "This sample demonstrates how to convert between `MapPoint` and string representations of a point using various coordinate systems.",
        "Tap on the map to see the point in several coordinate systems. Update one of the coordinates and select 'recalculate' to see the point converted into other coordinate systems. ")]
    public partial class FormatCoordinates
    {
        // Hold a reference to the text that was selected last
        private TextBox _selectedTextField;

        public FormatCoordinates()
        {
            InitializeComponent();

            Initialize();
        }

        private void Initialize()
        {
            // Set the initial selected text
            _selectedTextField = DecimalDegreesTextField;

            // Create the map
            MyMapView.Map = new Map(Basemap.CreateNavigationVector());

            // Add the graphics overlay to the map
            MyMapView.GraphicsOverlays.Add(new GraphicsOverlay());

            // Create the starting point
            MapPoint startingPoint = new MapPoint(0, 0, SpatialReferences.WebMercator);

            // Update the UI with the initial point
            UpdateUIFromMapPoint(startingPoint);

            // Subscribe to text change events
            UtmTextField.TextChanged += InputTextChanged;
            DmsTextField.TextChanged += InputTextChanged;
            DecimalDegreesTextField.TextChanged += InputTextChanged;
            UsngTextField.TextChanged += InputTextChanged;

            // Subscribe to map tap events to enable tapping on map to update coordinates
            MyMapView.GeoViewTapped += (sender, args) => { UpdateUIFromMapPoint(args.Location); };
        }

        private void InputTextChanged(object sender, TextChangedEventArgs e)
        {
            // Keep track of the last edited field
            _selectedTextField = (TextBox)sender;
        }

        private void UpdateUIFromMapPoint(MapPoint startingPoint)
        {
            // Update the decimal degrees text
            DecimalDegreesTextField.Text =
                CoordinateFormatter.ToLatitudeLongitude(startingPoint, LatitudeLongitudeFormat.DecimalDegrees, 4);

            // Update the degrees, minutes, seconds text
            DmsTextField.Text = CoordinateFormatter.ToLatitudeLongitude(startingPoint,
                LatitudeLongitudeFormat.DegreesMinutesSeconds, 1);

            // Update the UTM text
            UtmTextField.Text = CoordinateFormatter.ToUtm(startingPoint, UtmConversionMode.NorthSouthIndicators, true);

            // Update the USNG text
            UsngTextField.Text = CoordinateFormatter.ToUsng(startingPoint, 4, true);

            // Clear existing graphics overlays
            MyMapView.GraphicsOverlays[0].Graphics.Clear();

            // Create a symbol to symbolize the point
            SimpleMarkerSymbol symbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.X, Colors.Yellow, 20);

            // Create the graphic
            Graphic symbolGraphic = new Graphic(startingPoint, symbol);

            // Add the graphic to the graphics overlay
            MyMapView.GraphicsOverlays[0].Graphics.Add(symbolGraphic);
        }

        private async void RecalculateFields(object sender, RoutedEventArgs e)
        {
            // Hold the entered point
            MapPoint enteredPoint = null;

            // Update the point based on which text sent the event
            try
            {
                switch (_selectedTextField.Tag.ToString())
                {
                    case "Decimal Degrees":
                    case "Degrees, Minutes, Seconds":
                        enteredPoint =
                            CoordinateFormatter.FromLatitudeLongitude(_selectedTextField.Text, MyMapView.SpatialReference);
                        break;

                    case "UTM":
                        enteredPoint =
                            CoordinateFormatter.FromUtm(_selectedTextField.Text, MyMapView.SpatialReference, UtmConversionMode.NorthSouthIndicators);
                        break;

                    case "USNG":
                        enteredPoint =
                            CoordinateFormatter.FromUsng(_selectedTextField.Text, MyMapView.SpatialReference);
                        break;
                }
            }
            catch (Exception ex)
            {
                // The coordinate is malformed, warn and return
                await new MessageDialog(ex.Message, "Invalid format").ShowAsync();
                return;
            }

            // Update the UI from the MapPoint
            UpdateUIFromMapPoint(enteredPoint);
        }
    }
}