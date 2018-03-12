// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Drawing;

namespace ArcGISRuntime.Samples.FormatCoordinates
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Format coordinates",
        "Geometry",
        "This sample demonstrates how to convert between `MapPoint` and string representations of a point using various coordinate systems.",
        "Tap on the map to see the point in several coordinate systems. Update one of the coordinates and select 'recalculate' to see the point converted into other coordinate systems. ")]
    public class FormatCoordinates : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();
        private EditText _DecimalDegreesEditText;
        private EditText _DmsEditText;
        private EditText _UtmEditText;
        private EditText _UsngEditText;

        // Hold last edited field
        private EditText _lastEdited;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Format coordinates";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create the map
            _myMapView.Map = new Map(Basemap.CreateStreets());

            // Add the graphics overlay to the map
            _myMapView.GraphicsOverlays.Add(new GraphicsOverlay());

            // Create the starting point
            MapPoint startingPoint = new MapPoint(0, 0, SpatialReferences.WebMercator);

            // Update the UI with the initial point
            UpdateUIFromMapPoint(startingPoint);

            // Subscribe to map tap events to enable tapping on map to update coordinates
            _myMapView.GeoViewTapped += (sender, args) => { UpdateUIFromMapPoint(args.Location); };
        }

        private void InputTextChanged(object sender, EventArgs e)
        {
            // Keep track of the last edited field
            _lastEdited = (EditText)sender;
        }

        private void RecalculateFields(object sender, EventArgs e)
        {
            // Hold the entered point
            MapPoint enteredPoint = null;

            // Set a default last edited field to prevent NullReferenceException
            if (_lastEdited == null)
            {
                _lastEdited = _DecimalDegreesEditText;
            }

            // Update the point based on which text sent the event
            try
            {
                switch (_lastEdited.Hint)
                {
                    case "Decimal Degrees":
                    case "Degrees, Minutes, Seconds":
                        enteredPoint =
                            CoordinateFormatter.FromLatitudeLongitude(_lastEdited.Text, _myMapView.SpatialReference);
                        break;

                    case "UTM":
                        enteredPoint =
                            CoordinateFormatter.FromUtm(_lastEdited.Text, _myMapView.SpatialReference, UtmConversionMode.NorthSouthIndicators);
                        break;

                    case "USNG":
                        enteredPoint =
                            CoordinateFormatter.FromUsng(_lastEdited.Text, _myMapView.SpatialReference);
                        break;
                }
            }
            catch (Exception ex)
            {
                // The coordinate is malformed, warn and return
                // Display the message to the user
                var builder = new AlertDialog.Builder(this);
                builder.SetMessage(ex.Message).SetTitle("Invalid Format").Show();
                return;
            }

            // Update the UI from the MapPoint
            UpdateUIFromMapPoint(enteredPoint);
        }

        private void UpdateUIFromMapPoint(MapPoint startingPoint)
        {
            // Clear event subscriptions - prevents an infinite loop
            _UtmEditText.TextChanged -= InputTextChanged;
            _DmsEditText.TextChanged -= InputTextChanged;
            _DecimalDegreesEditText.TextChanged -= InputTextChanged;
            _UsngEditText.TextChanged -= InputTextChanged;

            // Update the decimal degrees text
            _DecimalDegreesEditText.Text =
                CoordinateFormatter.ToLatitudeLongitude(startingPoint, LatitudeLongitudeFormat.DecimalDegrees, 4);

            // Update the degrees, minutes, seconds text
            _DmsEditText.Text = CoordinateFormatter.ToLatitudeLongitude(startingPoint,
                LatitudeLongitudeFormat.DegreesMinutesSeconds, 1);

            // Update the UTM text
            _UtmEditText.Text = CoordinateFormatter.ToUtm(startingPoint, UtmConversionMode.NorthSouthIndicators, true);

            // Update the USNG text
            _UsngEditText.Text = CoordinateFormatter.ToUsng(startingPoint, 4, true);

            // Clear existing graphics overlays
            _myMapView.GraphicsOverlays[0].Graphics.Clear();

            // Create a symbol to symbolize the point
            SimpleMarkerSymbol symbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.X, Color.Yellow, 20);

            // Create the graphic
            Graphic symbolGraphic = new Graphic(startingPoint, symbol);

            // Add the graphic to the graphics overlay
            _myMapView.GraphicsOverlays[0].Graphics.Add(symbolGraphic);

            // Restore event subscriptions
            _UtmEditText.TextChanged += InputTextChanged;
            _DmsEditText.TextChanged += InputTextChanged;
            _DecimalDegreesEditText.TextChanged += InputTextChanged;
            _UsngEditText.TextChanged += InputTextChanged;
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Help Label
            TextView helpLabel = new TextView(this) { Text = "Edit the coordinates or tap the map to see conversions.\n\n" };
            layout.AddView(helpLabel);

            // Decimal Degrees
            _DecimalDegreesEditText = new EditText(this) { Hint = "Decimal Degrees" };
            TextView ddTextLabel = new TextView(this) { Text = "Decimal Degrees:" };
            layout.AddView(ddTextLabel);
            layout.AddView(_DecimalDegreesEditText);

            // Degrees, Minutes, Seconds
            _DmsEditText = new EditText(this) { Hint = "Degrees, Minutes, Seconds" };
            TextView dmsTextLabel = new TextView(this) { Text = "Degrees, Minutes, Seconds: " };
            layout.AddView(dmsTextLabel);
            layout.AddView(_DmsEditText);

            // UTM
            _UtmEditText = new EditText(this) { Hint = "UTM" };
            TextView utmTextLabel = new TextView(this) { Text = "UTM:" };
            layout.AddView(utmTextLabel);
            layout.AddView(_UtmEditText);

            // USNG
            _UsngEditText = new EditText(this) { Hint = "USNG" };
            TextView usngTextLabel = new TextView(this) { Text = "USNG" };
            layout.AddView(usngTextLabel);
            layout.AddView(_UsngEditText);

            // Button to allow for recalculating
            Button recalculateButton = new Button(this) { Text = "Recalculate" };
            recalculateButton.Click += RecalculateFields;
            layout.AddView(recalculateButton);

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}