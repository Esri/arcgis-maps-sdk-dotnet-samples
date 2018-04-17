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

namespace ArcGISRuntime.Samples.Buffer
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Buffer",
        "GeometryEngine",
        "This sample demonstrates how to use the GeometryEngine.Buffer to generate a polygon from an input geometry with a buffer distance.",
        "Tap on the map to specify a map point location. A buffer will created and displayed based upon the buffer value (in miles) specified in the textbox. Repeat the procedure to add additional map point and buffers. The generated buffers can overlap and are independent of each other.",
        "")]
    public class Buffer : Activity
    {
        // Create and hold reference to the used MapView.
        private MapView _myMapView = new MapView();

        // Create an EditText to enter a buffer value (in miles). 
        private EditText _bufferDistanceMilesEditText;      

        // Graphics overlay to display buffer related graphics.
        private GraphicsOverlay _graphicsOverlay;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Title = "Buffer";

            // Create the UI, setup the control references and execute initialization  
            CreateLayout(); 
            Initialize();
        }

        private void Initialize()
        {
            // Create a map with a topographic basemap.
            Map theMap = new Map(Basemap.CreateTopographic());

            // Create an envelope that covers the Dallas/Fort Worth area.
            Geometry startingEnvelope = new Envelope(-10863035.97, 3838021.34, -10744801.344, 3887145.299, SpatialReferences.WebMercator);

            // Set the map's initial extent to the envelope.
            theMap.InitialViewpoint = new Viewpoint(startingEnvelope);

            // Assign the map to the MapView.
            _myMapView.Map = theMap;

            // Create a graphics overlay to show the buffer-related graphics.
            _graphicsOverlay = new GraphicsOverlay();

            // Add the created graphics overlay to the MapView.
            _myMapView.GraphicsOverlays.Add(_graphicsOverlay);

            // Wire up the MapView's GeoViewTapped event handler.
            _myMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        private void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Create a map point (in the WebMercator projected coordinate system) from the GUI screen coordinate.
                MapPoint userTappedMapPoint = _myMapView.ScreenToLocation(e.Position);

                // Get the buffer size from the UITextField.
                double bufferInMiles = System.Convert.ToDouble(_bufferDistanceMilesEditText.Text);

                // Create a variable to be the buffer size in meters. There are 1609.34 meters in one mile.
                double bufferInMeters = bufferInMiles * 1609.34;

                // Get a buffered polygon from the GeometryEngine Buffer operation centered on the map point. 
                // Note: The input distance to the Buffer operation is in meters. This matches the backdrop 
                // basemap units which is also meters.
                Geometry bufferGeometry = GeometryEngine.Buffer(userTappedMapPoint, bufferInMeters);

                // Create the outline (a simple line symbol) for the buffered polygon. It will be a solid, thick, green line.
                SimpleLineSymbol bufferSimpleLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Green, 5);

                // Create the color that will be used for the fill of the buffered polygon. It will be a semi-transparent, green color.
                System.Drawing.Color bufferFillColor = System.Drawing.Color.FromArgb(125, 0, 255, 0);

                // Create simple fill symbol for the buffered polygon. It will be solid, semi-transparent, green fill with a solid, 
                // thick, green outline.
                SimpleFillSymbol bufferSimpleFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, bufferFillColor, bufferSimpleLineSymbol);

                // Create a new graphic for the buffered polygon using the defined simple fill symbol.
                Graphic bufferGraphic = new Graphic(bufferGeometry, bufferSimpleFillSymbol);

                // Add the buffered polygon graphic to the graphic overlay.
                _graphicsOverlay.Graphics.Add(bufferGraphic);

                // Create a simple marker symbol to display where the user tapped/clicked on the map. The marker symbol will be a 
                // solid, red circle.
                SimpleMarkerSymbol userTappedSimpleMarkerSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Red, 5);

                // Create a new graphic for the spot where the user clicked on the map using the simple marker symbol. 
                Graphic userTappedGraphic = new Graphic(userTappedMapPoint, userTappedSimpleMarkerSymbol);

                // Add the user tapped/clicked map point graphic to the graphic overlay.
                _graphicsOverlay.Graphics.Add(userTappedGraphic);
            }
            catch (System.Exception ex)
            {
                // Display an error message if there is a problem generating the buffer polygon.
                AlertDialog.Builder alertBuilder = new AlertDialog.Builder(this);
                alertBuilder.SetTitle("Geometry Engine Failed!");
                alertBuilder.SetMessage(ex.ToString());
                alertBuilder.Show();
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create a TextView for instructions.
            TextView bufferInstructionsTextView = new TextView(this);
            bufferInstructionsTextView.Text = "Buffer (miles):";
            layout.AddView(bufferInstructionsTextView);

            // Create a EditText for the buffer value.
            _bufferDistanceMilesEditText = new EditText(this);
            _bufferDistanceMilesEditText.Text = "10";
            layout.AddView(_bufferDistanceMilesEditText);

            // Add the map view to the layout.
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}