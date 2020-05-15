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
using System.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.WPF.Samples.FormatCoordinates
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Format coordinates",
        category: "Geometry",
        description: "Format coordinates in a variety of common notations.",
        instructions: "Click on the map to see a callout with the clicked location's coordinate formatted in 4 different ways. You can also put a coordinate string in any of these formats in the text field. Hit Enter and the coordinate string will be parsed to a map location which the callout will move to.",
        tags: new[] { "USNG", "UTM", "convert", "coordinate", "decimal degrees", "degree minutes seconds", "format", "latitude", "longitude" })]
    public partial class FormatCoordinates
    {
        // Hold a reference to the most recently selected text
        private TextBox _selectedTextBox;

        public FormatCoordinates()
        {
            InitializeComponent();

            Initialize();
        }

        private void Initialize()
        {
            // Initialize the selection
            _selectedTextBox = DecimalDegreesTextField;

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
            _selectedTextBox = (TextBox)sender;
        }

        private void UpdateUIFromMapPoint(MapPoint selectedPoint)
        {
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
                    DecimalDegreesTextField.Text = "Coordinates are out of range";
                    DmsTextField.Text = "Coordinates are out of range";
                    UtmTextField.Text = "Coordinates are out of range";
                    UsngTextField.Text = "Coordinates are out of range";

                    // Clear the selectionss symbol.
                    MyMapView.GraphicsOverlays[0].Graphics.Clear();
                }
                return;
            }

            // Update the decimal degrees text
            DecimalDegreesTextField.Text = CoordinateFormatter.ToLatitudeLongitude(selectedPoint, LatitudeLongitudeFormat.DecimalDegrees, 4);

            // Update the degrees, minutes, seconds text
            DmsTextField.Text = CoordinateFormatter.ToLatitudeLongitude(selectedPoint, LatitudeLongitudeFormat.DegreesMinutesSeconds, 1);

            // Update the UTM text
            UtmTextField.Text = CoordinateFormatter.ToUtm(selectedPoint, UtmConversionMode.NorthSouthIndicators, true);

            // Update the USNG text
            UsngTextField.Text = CoordinateFormatter.ToUsng(selectedPoint, 4, true);

            // Clear existing graphics overlays
            MyMapView.GraphicsOverlays[0].Graphics.Clear();

            // Create a symbol to symbolize the point
            SimpleMarkerSymbol symbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.X, Color.Yellow, 20);

            // Create the graphic
            Graphic symbolGraphic = new Graphic(selectedPoint, symbol);

            // Add the graphic to the graphics overlay
            MyMapView.GraphicsOverlays[0].Graphics.Add(symbolGraphic);
        }

        private void RecalculateFields(object sender, RoutedEventArgs e)
        {
            // Hold the entered point
            MapPoint enteredPoint = null;

            // Update the point based on which text sent the event
            try
            {
                switch (_selectedTextBox.Tag.ToString())
                {
                    case "Decimal Degrees":
                    case "Degrees, Minutes, Seconds":
                        enteredPoint =
                            CoordinateFormatter.FromLatitudeLongitude(_selectedTextBox.Text, MyMapView.SpatialReference);
                        break;

                    case "UTM":
                        enteredPoint =
                            CoordinateFormatter.FromUtm(_selectedTextBox.Text, MyMapView.SpatialReference, UtmConversionMode.NorthSouthIndicators);
                        break;

                    case "USNG":
                        enteredPoint =
                            CoordinateFormatter.FromUsng(_selectedTextBox.Text, MyMapView.SpatialReference);
                        break;
                }
            }
            catch (Exception ex)
            {
                // The coordinate is malformed, warn and return
                MessageBox.Show(ex.Message, "Invalid format");
                return;
            }

            // Update the UI from the MapPoint
            UpdateUIFromMapPoint(enteredPoint);
        }
    }
}