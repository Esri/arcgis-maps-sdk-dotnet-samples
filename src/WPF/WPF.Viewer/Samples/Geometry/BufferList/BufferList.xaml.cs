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
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;

namespace ArcGIS.WPF.Samples.BufferList
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Buffer list",
        category: "Geometry",
        description: "Generate multiple individual buffers or a single unioned buffer around multiple points.",
        instructions: "Click/tap on the map to add points. Click the \"Create Buffer(s)\" button to draw buffer(s) around the points (the size of the buffer is determined by the value entered by the user). Check the check box if you want the result to union (combine) the buffers. Click the \"Clear\" button to start over. The red dashed envelope shows the area where you can expect reasonable results for planar buffer operations with the North Central Texas State Plane spatial reference.",
        tags: new[] { "analysis", "buffer", "geometry", "planar" })]
    public partial class BufferList
    {
        // A polygon that defines the valid area of the spatial reference used.
        private Polygon _spatialReferenceArea;

        // A Random object to create RGB color values.
        private Random _random = new Random();

        public BufferList()
        {
            InitializeComponent();

            // Create the map and graphics overlays.
            Initialize();
        }

        private void Initialize()
        {
            // Create a spatial reference that's suitable for creating planar buffers in north central Texas (State Plane).
            SpatialReference statePlaneNorthCentralTexas = SpatialReference.Create(32038);

            // Define a polygon that represents the valid area of use for the spatial reference.
            // This information is available at https://developers.arcgis.com/net/latest/wpf/guide/pdf/projected_coordinate_systems_rt100_3_0.pdf
            List<MapPoint> spatialReferenceExtentCoords = new List<MapPoint>
            {
                new MapPoint(-103.070, 31.720, SpatialReferences.Wgs84),
                new MapPoint(-103.070, 34.580, SpatialReferences.Wgs84),
                new MapPoint(-94.000, 34.580, SpatialReferences.Wgs84),
                new MapPoint(-94.00, 31.720, SpatialReferences.Wgs84)
            };
            _spatialReferenceArea = new Polygon(spatialReferenceExtentCoords);
            _spatialReferenceArea = (Polygon)_spatialReferenceArea.Project(statePlaneNorthCentralTexas);

            // Create a map that uses the North Central Texas state plane spatial reference.
            Map bufferMap = new Map(statePlaneNorthCentralTexas);

            // Add some base layers (counties, cities, and highways).
            Uri usaLayerSource = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer");
            ArcGISMapImageLayer usaLayer = new ArcGISMapImageLayer(usaLayerSource);
            bufferMap.Basemap.BaseLayers.Add(usaLayer);

            // Use a new EnvelopeBuilder to expand the spatial reference extent 120%.
            EnvelopeBuilder envBuilder = new EnvelopeBuilder(_spatialReferenceArea.Extent);
            envBuilder.Expand(1.2);

            // Set the map's initial extent to the expanded envelope.
            Envelope startingEnvelope = envBuilder.ToGeometry();
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
            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Color.Red, 5);
            Graphic spatialReferenceExtentGraphic = new Graphic(_spatialReferenceArea, lineSymbol);

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
                // Get the input map point (in the map's coordinate system, State Plane for North Central Texas).
                MapPoint tapMapPoint = e.Location;

                // Check if the point coordinates are within the spatial reference envelope.
                bool withinValidExent = _spatialReferenceArea.Contains(tapMapPoint);

                // If the input point is not within the valid extent for the spatial reference, warn the user and return.
                if (!withinValidExent)
                {
                    MessageBox.Show("Location is not valid to buffer using the defined spatial reference.", "Out of bounds");
                    return;
                }

                // Get the buffer radius (in miles) from the text box.
                double bufferDistanceMiles = Convert.ToDouble(BufferDistanceMilesTextBox.Text);

                // Use a helper method to get the buffer distance in feet (unit that's used by the spatial reference).
                double bufferDistanceFeet = LinearUnits.Miles.ConvertTo(LinearUnits.Feet, bufferDistanceMiles);

                // Create a simple marker symbol (red circle) to display where the user tapped/clicked on the map.
                SimpleMarkerSymbol tapSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.Red, 10);

                // Create a new graphic to show the tap location.
                Graphic tapGraphic = new Graphic(tapMapPoint, tapSymbol)
                {
                    // Specify a z-index value on the point graphic to make sure it draws on top of the buffer polygons.
                    ZIndex = 2
                };

                // Store the specified buffer distance as an attribute with the graphic.
                tapGraphic.Attributes["distance"] = bufferDistanceFeet;

                // Add the tap point graphic to the buffer graphics overlay.
                MyMapView.GraphicsOverlays["buffers"].Graphics.Add(tapGraphic);
            }
            catch (Exception ex)
            {
                // Display an error message.
                MessageBox.Show(ex.Message, "Error creating buffer point");
            }
        }

        private void BufferButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Call a function to delete any existing buffer polygons so they can be recreated.
                ClearBufferPolygons();

                // Check if the user wants to create a single unioned buffer or independent buffers around each map point.
                bool areBuffersUnioned = UnionCheckBox.IsChecked == true;

                // Iterate all point graphics and create a list of map points and buffer distances for each.
                List<MapPoint> bufferMapPoints = new List<MapPoint>();
                List<double> bufferDistances = new List<double>();
                foreach (Graphic bufferGraphic in MyMapView.GraphicsOverlays["buffers"].Graphics)
                {
                    // Only use point graphics.
                    if (bufferGraphic.Geometry.GeometryType == GeometryType.Point)
                    {
                        // Get the geometry (map point) from the graphic.
                        MapPoint bufferLocation = bufferGraphic.Geometry as MapPoint;

                        // Read the "distance" attribute to get the buffer distance entered when the point was tapped.
                        double bufferDistanceFeet = (double)bufferGraphic.Attributes["distance"];

                        // Add the point and the corresponding distance to the lists.
                        bufferMapPoints.Add(bufferLocation);
                        bufferDistances.Add(bufferDistanceFeet);
                    }
                }

                // Call GeometryEngine.Buffer with a list of map points and a list of buffered distances.
                IEnumerable<Geometry> bufferPolygons = GeometryEngine.Buffer(bufferMapPoints, bufferDistances, areBuffersUnioned);

                // Create the outline for the buffered polygons.
                SimpleLineSymbol bufferPolygonOutlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.DarkBlue, 3);

                // Loop through all the geometries in the buffer results. There will be one buffered polygon if
                // the result geometries were unioned. Otherwise, there will be one buffer per input geometry.
                foreach (Geometry poly in bufferPolygons)
                {
                    // Create a random color to use for buffer polygon fill.
                    Color bufferPolygonColor = GetRandomColor();

                    // Create simple fill symbol for the buffered polygon using the fill color and outline.
                    SimpleFillSymbol bufferPolygonFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, bufferPolygonColor, bufferPolygonOutlineSymbol);

                    // Create a new graphic for the buffered polygon using the fill symbol.
                    Graphic bufferPolygonGraphic = new Graphic(poly, bufferPolygonFillSymbol)
                    {
                        // Specify a z-index of 0 to ensure the polygons draw below the tap points.
                        ZIndex = 0
                    };

                    // Add the buffered polygon graphic to the graphics overlay.
                    MyMapView.GraphicsOverlays[0].Graphics.Add(bufferPolygonGraphic);
                }
            }
            catch (Exception ex)
            {
                // Display an error message if there is a problem generating the buffers.
                MessageBox.Show(ex.Message, "Unable to create buffer polygons");
            }
        }

        private Color GetRandomColor()
        {
            // Get a byte array with three random values.
            var colorBytes = new byte[3];
            _random.NextBytes(colorBytes);

            // Use the random bytes to define red, green, and blue values for a new color.
            return Color.FromArgb(155, colorBytes[0], colorBytes[1], colorBytes[2]);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear all graphics (tap points and buffer polygons).
            MyMapView.GraphicsOverlays["buffers"].Graphics.Clear();
        }

        private void ClearBufferPolygons()
        {
            // Get the collection of graphics in the graphics overlay (points and buffer polygons).
            GraphicCollection bufferGraphics = MyMapView.GraphicsOverlays["buffers"].Graphics;

            // Loop (backwards) through all graphics.
            for (int i = bufferGraphics.Count - 1; i >= 0; i--)
            {
                // If the graphic is a polygon, remove it from the overlay.
                Graphic thisGraphic = bufferGraphics[i];
                if (thisGraphic.Geometry.GeometryType == GeometryType.Polygon)
                {
                    bufferGraphics.RemoveAt(i);
                }
            }
        }
    }
}