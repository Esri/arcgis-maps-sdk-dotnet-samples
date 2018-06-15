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
using System;
using System.Collections.Generic;
using UIKit;

namespace ArcGISRuntime.Samples.BufferList
{
    [Register("BufferList")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Buffer list",
        "GeometryEngine",
        "This sample demonstrates how to use the GeometryEngine.Buffer to generate one or more polygon from a series of input geometries and matching series of buffer distances. The option to union all the resulting buffer(s) is provided.",
        "Tap on the map in several locations to create center map-points to generate buffer(s). You can optionally change the buffer distance(in miles) by adjusting the value in the text field before each tap on the map. Then click on the 'Create Buffer(s)' button. If the 'Union the buffer(s)' switch is 'on' the resulting output buffer will be one polygon(possibly multi - part). If the 'Union the buffer(s)' switch is 'off' the resulting output will have one buffer polygon per input map point.",
        "")]
    public class BufferList : UIViewController
    {
        // Create and hold reference to the used MapView.
        private MapView _myMapView = new MapView();

        // Graphics overlay to display buffer-related graphics.
        private GraphicsOverlay _graphicsOverlay;

        // List of geometry values (MapPoints in this case) that will be used by the GeometryEngine.Buffer operation.
        private readonly List<Geometry> _bufferPointsList = new List<Geometry>();

        // List of buffer distance values (in meters) that will be used by the GeometryEngine.Buffer operation.
        private readonly List<double> _bufferDistancesList = new List<double>();

        private UILabel _sampleInstructionsLabel;

        // Create a UILabel to display instructions.
        private UILabel _bufferDistanceInstructionLabel;

        // Create UITextField to enter a buffer value (in miles). 
        private UITextField _bufferDistanceEntry;

        // Create a UISwitch to toggle whether to union the buffered results.
        private UISwitch _unionBufferSwitch;

        // Create a UIButton to create a unioned buffer.
        private UIButton _bufferButton;

        // Create toolbars to put behind the controls.
        private readonly UIToolbar _helpToolbar = new UIToolbar();
        private readonly UIToolbar _controlsToolbar = new UIToolbar();

        public BufferList()
        {
            Title = "Buffer list";
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
            nfloat topStart = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
            nfloat controlHeight = 30;
            nfloat margin = 5;

            // Setup the visual frames for the views.
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
            _helpToolbar.Frame = new CoreGraphics.CGRect(0, topStart, View.Bounds.Width, controlHeight * 3 + margin * 2);
            _controlsToolbar.Frame = new CoreGraphics.CGRect(0, View.Bounds.Height - (2 * controlHeight) - (3 * margin), View.Bounds.Width, (2 * controlHeight) + (3 * margin));
            _sampleInstructionsLabel.Frame = new CoreGraphics.CGRect(margin, topStart + margin, View.Bounds.Width - (2 * margin), 3 * controlHeight);
            _bufferDistanceInstructionLabel.Frame = new CoreGraphics.CGRect(margin, View.Bounds.Height - (2 * controlHeight) - (2* margin), 175, controlHeight);
            _bufferDistanceEntry.Frame = new CoreGraphics.CGRect(175 + 30, View.Bounds.Height - (2 * controlHeight) - (2 * margin), 50, controlHeight);
            _unionBufferSwitch.Frame = new CoreGraphics.CGRect(View.Bounds.Width - 75 + margin, View.Bounds.Height - (2 * controlHeight) - (2 * margin), 75 - (2 * margin), controlHeight);
            _bufferButton.Frame = new CoreGraphics.CGRect(margin, View.Bounds.Height - controlHeight - margin, View.Bounds.Width - (2 * margin), controlHeight);

            base.ViewDidLayoutSubviews();
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

            // Create a graphics overlay to show the buffered related graphics.
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

                // Get the buffer size (in miles) from the text field.
                double bufferDistanceInMiles = System.Convert.ToDouble(_bufferDistanceEntry.Text);

                // Create a variable to be the buffer size in meters. There are 1609.34 meters in one mile.
                double bufferDistanceInMeters = bufferDistanceInMiles * 1609.34;

                // Add the map point to the list that will be used by the GeometryEngine.Buffer operation.
                _bufferPointsList.Add(userTappedMapPoint);

                // Add the buffer distance to the list that will be used by the GeometryEngine.Buffer operation.
                _bufferDistancesList.Add(bufferDistanceInMeters);

                // Create a simple marker symbol to display where the user tapped/clicked on the map. The marker symbol will be a 
                // solid, red circle.
                SimpleMarkerSymbol userTappedSimpleMarkerSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Red, 10);

                // Create a new graphic for the spot where the user clicked on the map using the simple marker symbol. 
                Graphic userTappedGraphic = new Graphic(userTappedMapPoint, userTappedSimpleMarkerSymbol)
                {
                    ZIndex = 2
                };

                // Specify a ZIndex value on the user input map point graphic to assist with the drawing order of mixed geometry types 
                // being added to a single GraphicCollection. The lower the ZIndex value, the lower in the visual stack the graphic is 
                // drawn. Typically, Polygons would have the lowest ZIndex value (ex: 0), then Polylines (ex: 1), and finally MapPoints (ex: 2).

                // Add the user tapped/clicked map point graphic to the graphic overlay.
                _graphicsOverlay.Graphics.Add(userTappedGraphic);
            }
            catch (System.Exception ex)
            {
                // Display an error message if there is a problem generating the buffer polygon.
                UIAlertController alertController = UIAlertController.Create("Geometry Engine Failed!", ex.Message, UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alertController, true, null);
            }
        }

        private void BufferButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Get the boolean value whether to create a single unioned buffer (true) or independent buffer around each map point (false).
                bool unionBufferBool = _unionBufferSwitch.On;

                // Create an IEnumerable that contains buffered polygon(s) from the GeometryEngine Buffer operation based on a list of map 
                // points and list of buffered distances. The input distances used in the Buffer operation are in meters; this matches the 
                // backdrop basemap units which are also meters. If the unionResult parameter is true create a single unioned buffer, else
                // independent buffers will be created around each map point.
                IEnumerable<Geometry> theIEnumerableOfGeometryBuffer = GeometryEngine.Buffer(_bufferPointsList, _bufferDistancesList, unionBufferBool);

                // Loop through all the geometries in the IEnumerable from the GeometryEngine Buffer operation. There should only be one buffered 
                // polygon returned from the IEnumerable collection if the bool unionResult parameter was set to true in the GeometryEngine.Buffer 
                // operation. If the bool unionResult parameter was set to false there will be one buffer per input geometry.
                foreach (Geometry oneGeometry in theIEnumerableOfGeometryBuffer)
                {
                    // Create the outline (a simple line symbol) for the buffered polygon. It will be a solid, thick, green line.
                    SimpleLineSymbol bufferPolygonSimpleLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Green, 5);

                    // Create the color that will be used for the fill of the buffered polygon. It will be a semi-transparent, yellow color.
                    System.Drawing.Color bufferPolygonFillColor = System.Drawing.Color.FromArgb(155, 255, 255, 0);

                    // Create simple fill symbol for the buffered polygon. It will be solid, semi-transparent, yellow fill with a solid, 
                    // thick, green outline.
                    SimpleFillSymbol bufferPolygonSimpleFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, bufferPolygonFillColor, bufferPolygonSimpleLineSymbol);

                    // Create a new graphic for the buffered polygon using the defined simple fill symbol.
                    Graphic bufferPolygonGraphic = new Graphic(oneGeometry, bufferPolygonSimpleFillSymbol)
                    {
                        // Specify a ZIndex value on the buffered polygon graphic to assist with the drawing order of mixed geometry types being added
                        // to a single GraphicCollection. The lower the ZIndex value, the lower in the visual stack the graphic is drawn. Typically, 
                        // Polygons would have the lowest ZIndex value (ex: 0), then Polylines (ex: 1), and finally MapPoints (ex: 2).
                        ZIndex = 0
                    };

                    // Add the buffered polygon graphic to the graphic overlay.
                    // NOTE: While you can control the positional placement of a graphic within the GraphicCollection of a GraphicsOverlay, 
                    // it does not impact the drawing order in the GUI. If you have mixed geometries (i.e. Polygon, Polyline, MapPoint) within
                    // a single GraphicsCollection, the way to control the drawing order is to specify the Graphic.ZIndex. 
                    _graphicsOverlay.Graphics.Insert(0, bufferPolygonGraphic);
                }
            }
            catch (System.Exception ex)
            {
                // Display an error message if there is a problem generating the buffer polygon.
                UIAlertController alertController = UIAlertController.Create("Geometry Engine Failed!", ex.Message, UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alertController, true, null);
            }
        }

        private void CreateLayout()
        {

            // Create a UITextView for the overall sample instructions.
            _sampleInstructionsLabel = new UILabel
            {
                Text = "Tap on the map to create several points. You can specify the buffer distance for each point. " +
                       "Tap 'Create buffer(s)'. If the switch is 'on' the resulting output buffer will be unioned (one polygon). Otherwise, the result will have one buffer per point.",
                Lines = 4,
                AdjustsFontSizeToFitWidth = true
            };

            // Create a UILabel for instructions.
            _bufferDistanceInstructionLabel = new UILabel
            {
                Text = "Buffer distance (miles):",
                AdjustsFontSizeToFitWidth = true
            };

            // Create a UITextFiled for the buffer value.
            _bufferDistanceEntry = new UITextField
            {
                Text = "10",
                AdjustsFontSizeToFitWidth = true,
                VerticalAlignment = UIControlContentVerticalAlignment.Center
            };
            // - Allow pressing 'return' to dismiss the keyboard
            _bufferDistanceEntry.ShouldReturn += textField => { textField.ResignFirstResponder(); return true; };

            // Create a UISwitch for toggling the union of the buffer geometries.
            _unionBufferSwitch = new UISwitch
            {
                On = true,
                HorizontalAlignment = UIControlContentHorizontalAlignment.Right
            };

            // Create a UIButton to create the buffers.
            _bufferButton = new UIButton();
            _bufferButton.SetTitle("Create buffer(s)", UIControlState.Normal);
            _bufferButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            // - Hook to touch event to do querying
            _bufferButton.TouchUpInside += BufferButton_Click;

            // Add the MapView and other controls to the page.
            View.AddSubviews(_myMapView, _helpToolbar, _controlsToolbar, _sampleInstructionsLabel, _bufferDistanceInstructionLabel, _bufferDistanceEntry, _unionBufferSwitch, _bufferButton);
        }
    }
}