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
using System.Collections.Generic;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.UWP.Samples.BufferList
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Buffer list",
        "GeometryEngine",
        "This sample demonstrates how to use the GeometryEngine.Buffer to generate one or more polygon from a series of input geometries and matching series of buffer distances. The option to union all the resulting buffer(s) is provided.",
        "Tap on the map in several locations to create center map-points to generate buffer(s). You can optionally change the buffer distance(in miles) by adjusting the value in the text box before each tap on the map. Then click on the 'Create Buffer(s)' button. If the 'Union the buffer(s)' checkbox is checked the resulting output buffer will be one polygon(possibly multi - part). If the 'Union the buffer(s)' checkbox is un-checked the resulting output will have one buffer polygon per input map point.",
        "")]
    public partial class BufferList
    {
        // Graphics overlay to display buffer related graphics.
        private GraphicsOverlay _graphicsOverlay;

        // List of geometry values (MapPoints in this case) that will be used by the GeometryEngine.Buffer operation.
        private List<Geometry> _bufferPointsList = new List<Geometry>();

        // List of buffer distance values (in meters) that will be used by the GeometryEngine.Buffer operation.
        private List<double> _bufferDistancesList = new List<double>();

        public BufferList()
        {
            InitializeComponent();

            // Create a map with a topographic basemap.
            Map theMap = new Map(Basemap.CreateTopographic());

            // Create an envelope that covers the Dallas/Fort Worth area.
            Geometry startingEnvelope = new Envelope(-10863035.97, 3838021.34, -10744801.344, 3887145.299, SpatialReferences.WebMercator);

            // Set the map's initial extent to the envelope.
            theMap.InitialViewpoint = new Viewpoint(startingEnvelope);

            // Assign the map to the MapView.
            MyMapView.Map = theMap;

            // Create a graphics overlay to show the buffer-related graphics.
            _graphicsOverlay = new GraphicsOverlay();

            // Add the created graphics overlay to the MapView.
            MyMapView.GraphicsOverlays.Add(_graphicsOverlay);

            // Wire up the MapView's GeoViewTapped event handler.
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            try
            {
                // Create a map point (in the WebMercator projected coordinate system) from the GUI screen coordinate.
                MapPoint userTappedMapPoint = MyMapView.ScreenToLocation(e.Position);

                // Get the buffer size (in miles) from the text box.
                double bufferDistanceInMiles = System.Convert.ToDouble(BufferDistanceMilesTextBox.Text);

                // Create a variable to be the buffer size in meters. There are 1609.34 meters in one mile.
                double bufferDistanceInMeters = bufferDistanceInMiles * 1609.34;

                // Add the map point to the list that will be used by the GeometryEngine.Buffer operation.
                _bufferPointsList.Add(userTappedMapPoint);

                // Add the buffer distance to the list that will be used by the GeometryEngine.Buffer operation.
                _bufferDistancesList.Add(bufferDistanceInMeters);

                // Create a simple marker symbol to display where the user tapped/clicked on the map. The marker symbol will be a 
                // solid, red circle.
                SimpleMarkerSymbol userTappedSimpleMarkerSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Windows.UI.Colors.Red, 10);

                // Create a new graphic for the spot where the user clicked on the map using the simple marker symbol. 
                Graphic userTappedGraphic = new Graphic(userTappedMapPoint, userTappedSimpleMarkerSymbol);

                // Specify a ZIndex value on the user input map point graphic to assist with the drawing order of mixed geometry types 
                // being added to a single GraphicCollection. The lower the ZIndex value, the lower in the visual stack the graphic is 
                // drawn. Typically, Polygons would have the lowest ZIndex value (ex: 0), then Polylines (ex: 1), and finally MapPoints (ex: 2).
                userTappedGraphic.ZIndex = 2;

                // Add the user tapped/clicked map point graphic to the graphic overlay.
                _graphicsOverlay.Graphics.Add(userTappedGraphic);

            }
            catch (System.Exception ex)
            {
                // Display an error message.
                MessageDialog theMessageDialog = new MessageDialog("Error creating list of map points for buffer: " + ex.Message);
                await theMessageDialog.ShowAsync();
            }
        }

        private async void BufferButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the boolean value whether to create a single unioned buffer (true) or independent buffer around each map point (false).
                bool unionBufferBool = (bool)UnionCheckBox.IsChecked;

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
                    SimpleLineSymbol bufferPolygonSimpleLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Windows.UI.Colors.Green, 5);

                    // Create the color that will be used for the fill of the buffered polygon. It will be a semi-transparent, yellow color.
                    Windows.UI.Color bufferPolygonFillColor = Windows.UI.Color.FromArgb(155, 255, 255, 0);

                    // Create simple fill symbol for the buffered polygon. It will be solid, semi-transparent, yellow fill with a solid, 
                    // thick, green outline.
                    SimpleFillSymbol bufferPolygonSimpleFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, bufferPolygonFillColor, bufferPolygonSimpleLineSymbol);

                    // Create a new graphic for the buffered polygon using the defined simple fill symbol.
                    Graphic bufferPolygonGraphic = new Graphic(oneGeometry, bufferPolygonSimpleFillSymbol);

                    // Specify a ZIndex value on the buffered polygon graphic to assist with the drawing order of mixed geometry types being added
                    // to a single GraphicCollection. The lower the ZIndex value, the lower in the visual stack the graphic is drawn. Typically, 
                    // Polygons would have the lowest ZIndex value (ex: 0), then Polylines (ex: 1), and finally MapPoints (ex: 2).
                    bufferPolygonGraphic.ZIndex = 0;

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
                MessageDialog theMessageDialog = new MessageDialog("Geometry Engine Failed: " + ex.Message);
                await theMessageDialog.ShowAsync();
            }
        }

    }
}