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
using Xamarin.Forms;
using Colors = System.Drawing.Color;

namespace ArcGISRuntime.Samples.FormatCoordinates
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Format coordinates",
        "Geometry",
        "This sample demonstrates how to convert between `MapPoint` and string representations of a point using various coordinate systems.",
        "Tap on the map to see the point in several coordinate systems. Update one of the coordinates and select 'recalculate' to see the point converted into other coordinate systems. ")]
    public partial class FormatCoordinates : ContentPage
    {
        // Hold a reference to the most recently updated field
        private Entry _selectedEntry;

        public FormatCoordinates()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private void Initialize()
        {
            // Set the initial field selection
            _selectedEntry = DecimalDegreesTextField;

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

        private void InputTextChanged(object sender, EventArgs e)
        {
            // Keep track of the last edited field
            _selectedEntry = (Entry)sender;
        }

        private void UpdateUIFromMapPoint(MapPoint selectedPoint)
        {
            // Remove event handlers temporarily
            UtmTextField.TextChanged -= InputTextChanged;
            DmsTextField.TextChanged -= InputTextChanged;
            DecimalDegreesTextField.TextChanged -= InputTextChanged;
            UsngTextField.TextChanged -= InputTextChanged;

            try
            {
                // Check if the selected point can be formatted into coordinates.
                CoordinateFormatter.ToLatitudeLongitude(selectedPoint, LatitudeLongitudeFormat.DecimalDegrees, 0);
            }
            catch (Exception e)
            {
                // Check if the excpetion is because the coordinates are out of range.
                if (e.Message == "Invalid argument: coordinates are out of range")
                {
                    // Set all of the text fields to contain the error message.
                    DecimalDegreesTextField.Text = "Out of range";
                    DmsTextField.Text = "Out of range";
                    UtmTextField.Text = "Out of range";
                    UsngTextField.Text = "Out of range";

                    // Clear the selectionss symbol.
                    MyMapView.GraphicsOverlays[0].Graphics.Clear();

                    // Restore event handlers
                    UtmTextField.TextChanged += InputTextChanged;
                    DmsTextField.TextChanged += InputTextChanged;
                    DecimalDegreesTextField.TextChanged += InputTextChanged;
                    UsngTextField.TextChanged += InputTextChanged;
                }
                return;
            }

            // Update the decimal degrees text
            DecimalDegreesTextField.Text =
                CoordinateFormatter.ToLatitudeLongitude(selectedPoint, LatitudeLongitudeFormat.DecimalDegrees, 4);

            // Update the degrees, minutes, seconds text
            DmsTextField.Text = CoordinateFormatter.ToLatitudeLongitude(selectedPoint,
                LatitudeLongitudeFormat.DegreesMinutesSeconds, 1);

            // Update the UTM text
            UtmTextField.Text = CoordinateFormatter.ToUtm(selectedPoint, UtmConversionMode.NorthSouthIndicators, true);

            // Update the USNG text
            UsngTextField.Text = CoordinateFormatter.ToUsng(selectedPoint, 4, true);

            // Clear existing graphics overlays
            MyMapView.GraphicsOverlays[0].Graphics.Clear();

            // Create a symbol to symbolize the point
            SimpleMarkerSymbol symbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.X, Colors.Yellow, 20);

            // Create the graphic
            Graphic symbolGraphic = new Graphic(selectedPoint, symbol);

            // Add the graphic to the graphics overlay
            MyMapView.GraphicsOverlays[0].Graphics.Add(symbolGraphic);

            // Restore event handlers
            UtmTextField.TextChanged += InputTextChanged;
            DmsTextField.TextChanged += InputTextChanged;
            DecimalDegreesTextField.TextChanged += InputTextChanged;
            UsngTextField.TextChanged += InputTextChanged;
        }

        private void RecalculateFields(object sender, EventArgs e)
        {
            // Hold the entered point
            MapPoint enteredPoint = null;

            // Update the point based on which text sent the event
            try
            {
                switch (_selectedEntry.Placeholder)
                {
                    case "Decimal Degrees":
                    case "Degrees, Minutes, Seconds":
                        enteredPoint =
                            CoordinateFormatter.FromLatitudeLongitude(_selectedEntry.Text, MyMapView.SpatialReference);
                        break;

                    case "UTM":
                        enteredPoint =
                            CoordinateFormatter.FromUtm(_selectedEntry.Text, MyMapView.SpatialReference, UtmConversionMode.NorthSouthIndicators);
                        break;

                    case "USNG":
                        enteredPoint =
                            CoordinateFormatter.FromUsng(_selectedEntry.Text, MyMapView.SpatialReference);
                        break;
                }
            }
            catch (Exception ex)
            {
                // The coordinate is malformed, warn and return
                Application.Current.MainPage.DisplayAlert("Invalid Format", ex.Message, "OK");
                return;
            }

            // Update the UI from the MapPoint
            UpdateUIFromMapPoint(enteredPoint);
        }
    }
}