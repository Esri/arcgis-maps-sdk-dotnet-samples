// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System.Windows;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System.Drawing;

namespace ArcGISRuntime.WPF.Samples.Buffer
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Buffer",
        "GeometryEngine",
        "This sample demonstrates how to use the GeometryEngine.Buffer to generate a polygon from an input geometry with a buffer distance.",
        "Tap on the map to specify a map point location. A buffer will created and displayed based upon the buffer value (in miles) specified in the textbox. Repeat the procedure to add additional map point and buffers. The generated buffers can overlap and are independent of each other.",
        "")]
    public partial class Buffer
    {
        private double _planarBufferAreaTotal;
        private double _geodesicBufferAreaTotal;

        public Buffer()
        {
            InitializeComponent();

            // Create a map with a topographic basemap.
            Map bufferMap = new Map(Basemap.CreateTopographic());

            // Assign the map to the MapView.
            MyMapView.Map = bufferMap;
            
            // Wire up the MapView's GeoViewTapped event handler.
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        private void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            try
            {
                // Get the location tapped by the user (a map point in the WebMercator projected coordinate system).
                MapPoint userTapPoint = e.Location;

                // Get the buffer distance (miles) entered in the text box.
                double bufferInMiles = System.Convert.ToDouble(BufferDistanceMilesTextBox.Text);

                // Convert the input distance to meters. There are 1609.34 meters in one mile.
                double bufferInMeters = bufferInMiles * 1609.34;

                // Call a function to create a planar buffer graphic around the input location at the specified distance.
                Graphic planarBufferGraphic = CreateBufferPlanar(userTapPoint, bufferInMeters);

                // Call a function to create a geodesic buffer graphic using the same location and distance.
                Graphic geodesicBufferGraphic = CreateBufferGeodesic(userTapPoint, bufferInMeters);

                // Create a graphic for the user tap location with a white cross (+).
                SimpleMarkerSymbol locationSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, Color.White, 10);
                Graphic locationGraphic = new Graphic(userTapPoint, locationSymbol);

                // Get the graphics overlays.
                GraphicsOverlay planarBufferGraphicsOverlay = MyMapView.GraphicsOverlays["PlanarPolys"];
                GraphicsOverlay geodesicBufferGraphicsOverlay = MyMapView.GraphicsOverlays["GeodesicPolys"];
                GraphicsOverlay tapPointGraphicsOverlay = MyMapView.GraphicsOverlays["TapPoints"];

                // Add the buffer polygon and tap location graphics to the graphic overlay.
                planarBufferGraphicsOverlay.Graphics.Add(planarBufferGraphic);
                geodesicBufferGraphicsOverlay.Graphics.Add(geodesicBufferGraphic);
                tapPointGraphicsOverlay.Graphics.Add(locationGraphic);

                // Get the area for each new polygon.
                double areaPlanar = GeometryEngine.AreaGeodetic(planarBufferGraphic.Geometry, AreaUnits.SquareMiles);
                double areaGeodesic = GeometryEngine.AreaGeodetic(geodesicBufferGraphic.Geometry, AreaUnits.SquareMiles);

                // Add to the total area for each type of buffer polygon and calculate the percent difference.
                _planarBufferAreaTotal += areaPlanar;
                _geodesicBufferAreaTotal += areaGeodesic;
                double areaDifference = System.Math.Abs(_planarBufferAreaTotal - _geodesicBufferAreaTotal);
                double percentDifference = areaDifference / _geodesicBufferAreaTotal;

                // Show the total area for each type and the percent difference.
                BufferAreaPlanarTextBox.Text = string.Format("{0:F2}", _planarBufferAreaTotal);
                BufferAreaGeodesicTextBox.Text = string.Format("{0:F2}", _geodesicBufferAreaTotal);
                BufferDifferenceTextBox.Text = string.Format("{0:P}", percentDifference);
            }
            catch (System.Exception ex)
            {
                // Display an error message if there is a problem generating the buffer polygon.
                MessageBox.Show(ex.Message, "Geometry Engine Failed!");
            }
        }

        private Graphic CreateBufferGeodesic(MapPoint location, double distanceMeters)
        {
            Geometry bufferGeometryGeodesic = GeometryEngine.BufferGeodetic(location, distanceMeters, LinearUnits.Meters, double.NaN, GeodeticCurveType.Geodesic);
            Color bufferColor = Color.FromArgb(150, 255, 0, 0);
            SimpleLineSymbol outlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, bufferColor, 2);
            SimpleFillSymbol bufferFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, bufferColor, outlineSymbol);
            Graphic bufferGraphic = new Graphic(bufferGeometryGeodesic, bufferFillSymbol);

            return bufferGraphic;
        }

        private Graphic CreateBufferPlanar(MapPoint location, double distanceMeters)
        {
            Geometry bufferGeometryPlanar = GeometryEngine.Buffer(location, distanceMeters);
            Color bufferColor = Color.FromArgb(150, 0, 0, 255);
            SimpleLineSymbol outlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, bufferColor, 2);
            SimpleFillSymbol bufferFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, bufferColor, outlineSymbol);
            Graphic bufferGraphic = new Graphic(bufferGeometryPlanar, bufferFillSymbol);

            return bufferGraphic;
        }
        
        private void ShowBufferSwatches(SimpleFillSymbol planarSymbol, SimpleFillSymbol geodesicSymbol)
        {
            Color planarBufferColor = planarSymbol.Color;
            System.Windows.Media.Color planarLabelColor = System.Windows.Media.Color.FromArgb(
                planarBufferColor.A, 
                planarBufferColor.R, 
                planarBufferColor.G, 
                planarBufferColor.B);

            Color geodesicBufferColor = geodesicSymbol.Color;
            System.Windows.Media.Color geodesicLabelColor = System.Windows.Media.Color.FromArgb(
                planarBufferColor.A,
                planarBufferColor.R,
                planarBufferColor.G,
                planarBufferColor.B);

            BufferSwatchPlanarLabel.Background = new System.Windows.Media.SolidColorBrush(planarLabelColor);
            BufferSwatchGeodesicLabel.Background = new System.Windows.Media.SolidColorBrush(geodesicLabelColor);
        }

        private void ClearBuffersButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear the buffer and point graphics.
            foreach (GraphicsOverlay ov in MyMapView.GraphicsOverlays)
            {
                ov.Graphics.Clear();
            }

            // Clear the values from the area text boxes.
            BufferAreaPlanarTextBox.Text = "";
            BufferAreaGeodesicTextBox.Text = "";
            BufferDifferenceTextBox.Text = "";

            // Clear the area totals;
            _planarBufferAreaTotal = 0.0;
            _geodesicBufferAreaTotal = 0.0;
        }
    }
}