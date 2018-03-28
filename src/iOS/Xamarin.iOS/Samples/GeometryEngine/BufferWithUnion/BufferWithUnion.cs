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

namespace ArcGISRuntime.Samples.BufferWithUnion
{
    [Register("BufferWithUnion")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Buffer with Union",
        "GeometryEngine",
        "This sample demonstrates how to use the GeometryEngine.Buffer to generate a union polygon from a series of input geometries and matching series of buffer distances.",
        "Tap on the map in several locations to specify a series of map points. Then click on the 'Make a Unioned Buffer' button. A unioned polygon will be created and displayed. It is possible that the polygon generated will have multiple parts depending on the spatial dispersion of the input map points.",
        "")]
    public class BufferWithUnion : UIViewController
    {
        // Constant holding offset where the MapView control should start.
        private const int _yPageOffset = 60;

        // Create and hold reference to the used MapView.
        private MapView _myMapView = new MapView();

        // Create a UIButton to create a unioned buffer.
        private UIButton _theUnionBufferButton;

        // Graphics overlay to display buffer related graphics.
        private GraphicsOverlay _theGraphicsOverlay;

        // List of geometry values (MapPoints in this case) that will be used by the GeometryEngine.Buffer operation.
        private List<Geometry> _theBufferPoints = new List<Geometry>();

        // List of buffer distance values (in meters) that will be used by the GeometryEngine.Buffer operation.
        private List<double> _theBufferDistances = new List<double>();

        public BufferWithUnion()
        {
            Title = "Buffer with Union";
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

            // Setup the visual frame for the UIButton to make the unioned buffer.
            _theUnionBufferButton.Frame = new CoreGraphics.CGRect(0, _yPageOffset, View.Bounds.Width, 40);

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
            _theGraphicsOverlay = new GraphicsOverlay();

            // Add the created graphics overlay to the MapView.
            _myMapView.GraphicsOverlays.Add(_theGraphicsOverlay);

            // Wire up the MapView's GeoViewTapped event handler.
            _myMapView.GeoViewTapped += OnMapViewTapped;
        }

        private void OnMapViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Create a map point (in the WebMercator projected coordinate system) from the GUI screen coordinate.
                MapPoint theMapPoint = _myMapView.ScreenToLocation(e.Position);

                // Specify a hard-coded buffer size.
                double theBufferInMiles = 10;

                // Create a variable to be the buffer size in meters. There are 1609.34 meters in one mile.
                double theBufferInMeters = theBufferInMiles * 1609.34;

                // Add the map point to the list that will be used by the GeometryEngine.Buffer operation.
                _theBufferPoints.Add(theMapPoint);

                // Add the buffer distance to the list that will be used by the GeometryEngine.Buffer operation.
                _theBufferDistances.Add(theBufferInMeters);

                // Create a simple marker symbol to display where the user tapped/clicked on the map. The marker symbol will be a 
                // solid, red circle.
                SimpleMarkerSymbol theSimpleMarkerSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Red, 10);

                // Create a new graphic for the spot where the user clicked on the map using the simple marker symbol. 
                Graphic theUserInputGraphic = new Graphic(theMapPoint, theSimpleMarkerSymbol);

                // Specify a ZIndex value on the user input map point graphic to assist with the drawing order of mixed geometry types 
                // being added to a single GraphicCollection. The lower the ZIndex value, the lower in the visual stack the graphic is 
                // drawn. Typically, Polygons would have the lowest ZIndex value (ex: 0), then Polylines (ex: 1), and finally MapPoints (ex: 2).
                theUserInputGraphic.ZIndex = 2;

                // Add the user tapped/clicked map point graphic to the graphic overlay.
                _theGraphicsOverlay.Graphics.Add(theUserInputGraphic);
            }
            catch (System.Exception ex)
            {
                // Display an error message if there is a problem generating the buffer polygon.
                UIAlertController alertController = UIAlertController.Create("Error creating list of map points for buffer!", ex.Message, UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alertController, true, null);
                return;
            }
        }

        private void OnMakeUnionBufferClicked(object sender, EventArgs e)
        {
            try
            {
                // Create an IEnumerable that contains a single buffered polygon from the GeometryEngine Buffer operation based on a list 
                // of map points and list of buffered distances. The input distances used in the Buffer operation are in meters; this matches 
                // the backdrop basemap units which are also meters. There should only be one returned polygon geometry in the IEnumerable
                // collection because the bool unionResult parameter is set to true. 
                IEnumerable<Geometry> theIEnumerableOfGeometryBuffer = GeometryEngine.Buffer(_theBufferPoints, _theBufferDistances, true);

                // Create an empty geometry that will be used to hold the single buffered polygon from the GeometryEngine Buffer operation.
                Geometry theBufferedGeometry = null;

                // Loop through all the geometries in the IEnumerable from the GeometryEngine Buffer operation. In this case however, there
                // should only be one buffered polygon returned from the IEnumerable collection because the bool unionResult parameter was 
                // set to true in the GeometryEngine.Buffer operation. 
                foreach (Geometry oneGeometry in theIEnumerableOfGeometryBuffer)
                {
                    // Set the polygon geometry from the buffer operation.
                    theBufferedGeometry = oneGeometry;
                }

                // Create the outline (a simple line symbol) for the buffered polygon. It will be a solid, thick, green line.
                SimpleLineSymbol theSimpleLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Green, 5);

                // Create the color that will be used for the fill of the buffered polygon. It will be a semi-transparent, yellow color.
                System.Drawing.Color theFillColor = System.Drawing.Color.FromArgb(155, 255, 255, 0);

                // Create simple fill symbol for the buffered polygon. It will be solid, semi-transparent, yellow fill with a solid, 
                // thick, green outline.
                SimpleFillSymbol theSimpleFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, theFillColor, theSimpleLineSymbol);

                // Create a new graphic for the buffered polygon using the defined simple fill symbol.
                Graphic thePolygonGraphic = new Graphic(theBufferedGeometry, theSimpleFillSymbol);

                // Specify a ZIndex value on the buffered polygon graphic to assist with the drawing order of mixed geometry types being added
                // to a single GraphicCollection. The lower the ZIndex value, the lower in the visual stack the graphic is drawn. Typically, 
                // Polygons would have the lowest ZIndex value (ex: 0), then Polylines (ex: 1), and finally MapPoints (ex: 2).
                thePolygonGraphic.ZIndex = 0;

                // Add the buffered polygon graphic to the graphic overlay.
                // NOTE: While you can control the positional placement of a graphic within the GraphicCollection of a GraphicsOverlay, 
                // it does not impact the drawing order in the GUI. If you have mixed geometries (i.e. Polygon, Polyline, MapPoint) within
                // a single GraphicsCollection, the way to control the drawing order is to specify the Graphic.ZIndex. 
                _theGraphicsOverlay.Graphics.Insert(0, thePolygonGraphic);
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
            // Create button to create the unoined buffer.
            _theUnionBufferButton = new UIButton();
            _theUnionBufferButton.SetTitle("Make Unioned Buffer", UIControlState.Normal);
            _theUnionBufferButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _theUnionBufferButton.BackgroundColor = UIColor.White;
            // - Hook to touch event to do querying
            _theUnionBufferButton.TouchUpInside += OnMakeUnionBufferClicked;

            // Add the MapView and other controls to the page.
            View.AddSubviews(_myMapView, _theUnionBufferButton);
        }
    }
}