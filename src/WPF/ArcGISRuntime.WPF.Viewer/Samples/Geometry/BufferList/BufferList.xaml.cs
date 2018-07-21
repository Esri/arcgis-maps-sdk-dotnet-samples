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
using System.Collections.Generic;
using System.Windows;

namespace ArcGISRuntime.WPF.Samples.BufferList
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Buffer list",
        "Geometry",
        "This sample demonstrates how to use the GeometryEngine.Buffer to generate one or more polygon from a series of input geometries and matching series of buffer distances. The option to union all the resulting buffer(s) is provided.",
        "Tap on the map in several locations to create center map-points to generate buffer(s). You can optionally change the buffer distance(in miles) by adjusting the value in the text box before each tap on the map. Then click on the 'Create Buffer(s)' button. If the 'Union the buffer(s)' checkbox is checked the resulting output buffer will be one polygon(possibly multi - part). If the 'Union the buffer(s)' checkbox is un-checked the resulting output will have one buffer polygon per input map point.",
        "")]
    public partial class BufferList
    {
        // A spatial reference that's suitable for creating planar buffers in north central Texas (State Plane).
        private SpatialReference _statePlaneNorthCentralTexas = new SpatialReference(32038);

        // An envelope that represents the valid area of use for the spatial reference.
        private Envelope _spatialReferenceArea = new Envelope(-103.070, 31.720, -94.000, 34.580, SpatialReferences.Wgs84);

        public BufferList()
        {
            InitializeComponent();

            // Create the map and graphics overlays.
            Initialize();
        }

        private void Initialize()
        {
            // Create a map with a topographic basemap.
            Map bufferMap = new Map(Basemap.CreateTopographic());

            // Set the map's initial extent to an envelope that covers the Dallas/Fort Worth area.
            Envelope startingEnvelope = new Envelope(-99.000, 32.750, -96.000, 33.750, SpatialReferences.Wgs84);
            bufferMap.InitialViewpoint = new Viewpoint(startingEnvelope);

            // Assign the map to the MapView.
            MyMapView.Map = bufferMap;

            // Create a graphics overlay to show the buffer polygon graphics.
            GraphicsOverlay bufferGraphicsOverlay = new GraphicsOverlay
            {
                // Give the overlay an ID so it can be found later.
                Id = "buffers"
            };

            // Create a graphic to show the spatial reference's valid extent (envelope) with a dashed red line.
            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, System.Drawing.Color.Red, 5);
            SimpleFillSymbol fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.Color.Transparent, lineSymbol);
            Graphic spatialReferenceExtentGraphic = new Graphic(_spatialReferenceArea, fillSymbol);

            // Add the graphic to a new overlay.
            GraphicsOverlay spatialReferenceGraphicsOverlay = new GraphicsOverlay();
            spatialReferenceGraphicsOverlay.Graphics.Add(spatialReferenceExtentGraphic);

            // Add the graphics overlays to the MapView.
            MyMapView.GraphicsOverlays.Add(bufferGraphicsOverlay);
            MyMapView.GraphicsOverlays.Add(spatialReferenceGraphicsOverlay);

            // Wire up the MapView's GeoViewTapped event handler.
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        private void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Get the input map point (in the map's coordinate system, Web Mercator).
                MapPoint tapMapPoint = e.Location;

                // Call a function to check if the point falls inside the valid extent for the spatial reference.
                bool withinValidExent = CheckMapPoint(tapMapPoint);

                // If the input point is not within the valid extent for the spatial reference, warn the user and return.
                if (!withinValidExent)
                {
                    MessageBox.Show("Location is not valid to buffer using the defined spatial reference.", "Out of bounds");
                    return;
                }

                // Project the point to an appropriate spatial reference for the area of interest (North Central Texas State Plane).
                MapPoint projectedMapPoint = GeometryEngine.Project(tapMapPoint, _statePlaneNorthCentralTexas) as MapPoint;
                
                // Get the buffer radius (in miles) from the text box.
                double bufferDistanceMiles = System.Convert.ToDouble(BufferDistanceMilesTextBox.Text);

                // Use a helper method to get the buffer distance in feet (unit that's used by the spatial reference).
                double bufferDistanceFeet = LinearUnits.Miles.ConvertTo(LinearUnits.Feet, bufferDistanceMiles);

                // Create a simple marker symbol (red circle) to display where the user tapped/clicked on the map. 
                SimpleMarkerSymbol tapSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Red, 10);

                // Create a new graphic to show the tap location. 
                Graphic tapGraphic = new Graphic(projectedMapPoint, tapSymbol)
                {
                    // Specify a ZIndex value on the user input map point graphic to assist with the drawing order of mixed geometry types 
                    // being added to a single GraphicCollection. The lower the ZIndex value, the lower in the visual stack the graphic is 
                    // drawn. Typically, Polygons would have the lowest ZIndex value (ex: 0), then Polylines (ex: 1), and finally MapPoints (ex: 2).
                    ZIndex = 2
                };

                // Store the specified buffer distance as an attribute with the graphic.
                tapGraphic.Attributes["distance"] = bufferDistanceFeet;

                // Add the tap point graphic to the buffer graphics overlay.
                MyMapView.GraphicsOverlays["buffers"].Graphics.Add(tapGraphic);
            }
            catch (System.Exception ex)
            {
                // Display an error message.
                MessageBox.Show(ex.Message, "Error creating buffer point");
            }
        }

        private bool CheckMapPoint(MapPoint inPoint)
        {
            // Project the input point to geographic coordinates to get latitude and longitude.
            MapPoint geographicPoint = GeometryEngine.Project(inPoint, SpatialReferences.Wgs84) as MapPoint;

            // Check if the point coordinates are within the spatial reference envelope.
            bool isValid = geographicPoint.X < _spatialReferenceArea.XMax && geographicPoint.X > _spatialReferenceArea.XMin &&
                           geographicPoint.Y < _spatialReferenceArea.YMax && geographicPoint.Y > _spatialReferenceArea.YMin;

            return isValid;
        }

        private void BufferButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the boolean value whether to create a single unioned buffer (true) or independent buffer around each map point (false).
                bool areBuffersUnioned = UnionCheckBox.IsChecked == true;

                // Iterate all point graphics and create a list of map points and buffer distances for each.
                List<MapPoint> bufferMapPoints = new List<MapPoint>();
                List<double> bufferDistances = new List<double>();
                foreach (Graphic bufferGraphic in MyMapView.GraphicsOverlays["buffers"].Graphics)
                {
                    // Only use point graphics.
                    if (bufferGraphic.Geometry.GeometryType == GeometryType.Point)
                    {
                        // Get the graphic geometry (map point) from the graphic.
                        MapPoint bufferLocation = bufferGraphic.Geometry as MapPoint;

                        // Read the "distance" attribute to get the buffer distance entered when the point was tapped.
                        double bufferDistanceFeet = (double)bufferGraphic.Attributes["distance"];

                        // Add the point and the corresponding distance to the lists.
                        bufferMapPoints.Add(bufferLocation);
                        bufferDistances.Add(bufferDistanceFeet);
                    }
                }

                // Call GeometryEngine.Buffer with a list of map points and a list of buffered distances. 
                // The input distances are in feet to match the state plane spatial reference used for the points. 
                // Provide the value of the unionResult parameter to create either a single unioned buffer polygon
                // or individual buffers around each input point.
                IEnumerable<Geometry> bufferPolygons = GeometryEngine.Buffer(bufferMapPoints, bufferDistances, areBuffersUnioned);
                
                // Create the outline for the buffered polygon.
                SimpleLineSymbol bufferPolygonOutlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.DarkBlue, 3);
                
                // Create the color that will be used for buffer polygon fill.
                System.Drawing.Color bufferPolygonColor = System.Drawing.Color.FromArgb(155, 255, 180, 60);
                
                // Create simple fill symbol for the buffered polygon using the fill color and outline.
                SimpleFillSymbol bufferPolygonFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, bufferPolygonColor, bufferPolygonOutlineSymbol);
                
                // Loop through all the geometries in the buffer results. There will be one buffered polygon if
                // the result geometries were unioned. Otherwise, there will be one buffer per input geometry.
                foreach (Geometry poly in bufferPolygons)
                {
                    // Create a new graphic for the buffered polygon using the fill symbol.
                    Graphic bufferPolygonGraphic = new Graphic(poly, bufferPolygonFillSymbol)
                    {
                        // NOTE: While you can control the positional placement of a graphic within the GraphicCollection of a GraphicsOverlay, 
                        // it does not impact the drawing order in the GUI. If you have mixed geometries (i.e. Polygon, Polyline, MapPoint) within
                        // a single GraphicsCollection, the way to control the drawing order is to specify the Graphic.ZIndex. 
                        ZIndex = 0
                    };

                    // Add the buffered polygon graphic to the graphics overlay.                    
                    MyMapView.GraphicsOverlays[0].Graphics.Add(bufferPolygonGraphic);
                }
            }
            catch (System.Exception ex)
            {
                // Display an error message if there is a problem generating the buffer polygon.
                MessageBox.Show(ex.Message, "Unable to create buffer polygons");
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear all buffer graphics (tap points and buffer polygons).
            MyMapView.GraphicsOverlays["buffers"].Graphics.Clear();
        }
    }
}