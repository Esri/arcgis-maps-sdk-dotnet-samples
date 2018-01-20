﻿// Copyright 2018 Esri.
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ArcGISRuntime.WPF.Samples.FormatCoordinates
{
    public partial class FormatCoordinates
    {
        // Hold a reference to the most recently selected textbox
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
            UpdateUiFromMapPoint(startingPoint);

            // Subscribe to text change events
            UtmTextField.TextChanged += InputTextChanged;
            DmsTextField.TextChanged += InputTextChanged;
            DecimalDegreesTextField.TextChanged += InputTextChanged;
            UsngTextField.TextChanged += InputTextChanged;

            // Subscribe to map tap events to enable tapping on map to update coordinates
            MyMapView.GeoViewTapped += (sender, args) => { UpdateUiFromMapPoint(args.Location); };
        }

        private void InputTextChanged(object sender, TextChangedEventArgs e)
        {
            // Keep track of the last edited field
            _selectedTextBox = (TextBox)sender;
        }

        private void UpdateUiFromMapPoint(MapPoint startingPoint)
        {
            // Update the decimal degrees textbox
            DecimalDegreesTextField.Text =
                CoordinateFormatter.ToLatitudeLongitude(startingPoint, LatitudeLongitudeFormat.DecimalDegrees, 4);

            // Update the degrees, minutes, seconds textbox
            DmsTextField.Text = CoordinateFormatter.ToLatitudeLongitude(startingPoint,
                LatitudeLongitudeFormat.DegreesMinutesSeconds, 1);

            // Update the UTM textbox
            UtmTextField.Text = CoordinateFormatter.ToUtm(startingPoint, UtmConversionMode.LatitudeBandIndicators, true);

            // Update the USNG textbox
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

        private void RecalculateFields(object sender, RoutedEventArgs e)
        {
            // Hold the entered point
            MapPoint enteredPoint = null;

            // Update the point based on which textbox sent the event
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
                            CoordinateFormatter.FromUtm(_selectedTextBox.Text, MyMapView.SpatialReference, UtmConversionMode.LatitudeBandIndicators);
                        break;

                    case "USNG":
                        enteredPoint =
                            CoordinateFormatter.FromUsng(_selectedTextBox.Text, MyMapView.SpatialReference);
                        break;
                }
            }
            catch (Exception)
            {
                // The coordinate is malformed, return
                // Sample doesn't handle this because coordinates can be invalid while the user is experimenting
                return;
            }

            // Return if getting the point from text failed
            if (enteredPoint == null)
            {
                return;
            }

            // Update the UI from the MapPoint
            UpdateUiFromMapPoint(enteredPoint);
        }
    }
}