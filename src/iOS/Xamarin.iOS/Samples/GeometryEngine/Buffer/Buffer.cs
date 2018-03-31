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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.Buffer
{
    [Register("Buffer")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Buffer",
        "GeometryEngine",
        "This sample demonstrates how to use the GeometryEngine.Buffer to generate a polygon from an input geometry with a buffer distance.",
        "Tap on the map to specify a map point location. A buffer will created and displayed based upon the buffer value (in miles) specified in the textbox. Repeat the procedure to add additional map point and buffers. The generated buffers can overlap and are independent of each other.",
        "")]
    public class Buffer : UIViewController
    {
        // Constant holding offset where the MapView control should start.
        private const int _yPageOffset = 60;

        // Create and hold reference to the used MapView.
        private MapView _myMapView = new MapView();

        // Create a UILabel to display the instructions.
        private UILabel _InstructionsUILabel;

        // Create UITextField to enter a buffer value (in miles). 
        private UITextField _BufferUITextField;

        // Graphics overlay to display buffer related graphics.
        private GraphicsOverlay _GraphicsOverlay;

        public Buffer()
        {
            Title = "Buffer";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references and execute initialization. 
            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            // Setup the visual frame for the MapView.
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            // Setup the visual frame for the instructions UILabel.
            _InstructionsUILabel.Frame = new CoreGraphics.CGRect(0, _yPageOffset, View.Bounds.Width, 40);

            // Setup the visual frame for the buffer value UITextField.
            _BufferUITextField.Frame = new CoreGraphics.CGRect(150, _yPageOffset, View.Bounds.Width, 40);

            base.ViewDidLayoutSubviews();
        }

        private void Initialize()
        {
            // Create a map with a topographic basemap.
            Map theMap = new Map(Basemap.CreateTopographic());

            // Create an envelope that covers the Dallas/Fort Worth area.
            Geometry theGeometry = new Envelope(-10863035.97, 3838021.34, -10744801.344, 3887145.299, SpatialReferences.WebMercator);

            // Set the map's initial extent to the envelope.
            theMap.InitialViewpoint = new Viewpoint(theGeometry);

            // Assign the map to the MapView.
            _myMapView.Map = theMap;

            // Create a graphics overlay to show the buffered related graphics.
            _GraphicsOverlay = new GraphicsOverlay();

            // Add the created graphics overlay to the MapView.
            _myMapView.GraphicsOverlays.Add(_GraphicsOverlay);

            // Wire up the MapView's GeoViewTapped event handler.
            _myMapView.GeoViewTapped += OnMapViewTapped;
        }

        private void OnMapViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Create a map point (in the WebMercator projected coordinate system) from the GUI screen coordinate.
                MapPoint theMapPoint = _myMapView.ScreenToLocation(e.Position);

                // Get the buffer size from the UITextField.
                double theBufferInMiles = System.Convert.ToDouble(_BufferUITextField.Text);

                // Create a variable to be the buffer size in meters. There are 1609.34 meters in one mile.
                double theBufferInMeters = theBufferInMiles * 1609.34;

                // Get a buffered polygon from the GeometryEngine Buffer operation centered on the map point. 
                // Note: The input distance to the Buffer operation is in meters. This matches the backdrop 
                // basemap units which is also meters.
                Geometry theGeometryBuffer = GeometryEngine.Buffer(theMapPoint, theBufferInMeters);

                // Create the outline (a simple line symbol) for the buffered polygon. It will be a solid, thick, green line.
                SimpleLineSymbol theSimpleLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Green, 5);

                // Create the color that will be used for the fill of the buffered polygon. It will be a semi-transparent, green color.
                System.Drawing.Color theFillColor = System.Drawing.Color.FromArgb(125, 0, 255, 0);

                // Create simple fill symbol for the buffered polygon. It will be solid, semi-transparent, green fill with a solid, 
                // thick, green outline.
                SimpleFillSymbol theSimpleFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, theFillColor, theSimpleLineSymbol);

                // Create a new graphic for the buffered polygon using the defined simple fill symbol.
                Graphic thePolygonGraphic = new Graphic(theGeometryBuffer, theSimpleFillSymbol);

                // Add the buffered polygon graphic to the graphic overlay.
                _GraphicsOverlay.Graphics.Add(thePolygonGraphic);

                // Create a simple marker symbol to display where the user tapped/clicked on the map. The marker symbol will be a 
                // solid, red circle.
                SimpleMarkerSymbol theSimpleMarkerSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Red, 5);

                // Create a new graphic for the spot where the user clicked on the map using the simple marker symbol. 
                Graphic theUserInputGraphic = new Graphic(theMapPoint, theSimpleMarkerSymbol);

                // Add the user tapped/clicked map point graphic to the graphic overlay.
                _GraphicsOverlay.Graphics.Add(theUserInputGraphic);
            }
            catch (System.Exception ex)
            {
                // Display an error message if there is a problem generating the buffer polygon.
                UIAlertController alertController = UIAlertController.Create("Geometry Engine Failed!", ex.Message, UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alertController, true, null);
                return;
            }
        }

        private void CreateLayout()
        {
            // Create the UILabel for instructions.
            _InstructionsUILabel = new UILabel();
            _InstructionsUILabel.Text = "Buffer (miles):";
            _InstructionsUILabel.AdjustsFontSizeToFitWidth = true;
            _InstructionsUILabel.BackgroundColor = UIColor.White;

            // Create UITextFiled for the buffer value.
            _BufferUITextField = new UITextField();
            _BufferUITextField.Text = "10";
            _BufferUITextField.AdjustsFontSizeToFitWidth = true;
            _BufferUITextField.BackgroundColor = UIColor.White;
            // - Allow pressing 'return' to dismiss the keyboard
            _BufferUITextField.ShouldReturn += (textField) => { textField.ResignFirstResponder(); return true; };

            // Add the MapView and other controls to the page.
            View.AddSubviews(_myMapView, _InstructionsUILabel, _BufferUITextField);
        }
    }
}