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
using System.Drawing;
using System.Windows;

namespace ArcGIS.WPF.Samples.Buffer
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Buffer",
        category: "Geometry",
        description: "Create a buffer around a map point and display the results as a `Graphic`",
        instructions: "1. Tap on the map.",
        tags: new[] { "analysis", "buffer", "euclidean", "geodesic", "geometry", "planar" })]
    public partial class Buffer
    {
        public Buffer()
        {
            InitializeComponent();

            // Initialize the map and graphics overlays.
            Initialize();
        }

        private void Initialize()
        {
            // Create a map with a topographic basemap and add it to the map view.
            MyMapView.Map = new Map(BasemapStyle.ArcGISTopographic);

            // Handle the MapView's GeoViewTapped event to create buffers.
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;

            // Create a fill symbol for geodesic buffer polygons.
            Color geodesicBufferColor = Color.FromArgb(120, 255, 0, 0);
            SimpleLineSymbol geodesicOutlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, geodesicBufferColor, 2);
            SimpleFillSymbol geodesicBufferFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, geodesicBufferColor, geodesicOutlineSymbol);

            // Create a fill symbol for planar buffer polygons.
            Color planarBufferColor = Color.FromArgb(120, 0, 0, 255);
            SimpleLineSymbol planarOutlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, planarBufferColor, 2);
            SimpleFillSymbol planarBufferFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, planarBufferColor, planarOutlineSymbol);

            // Create a marker symbol for tap locations.
            SimpleMarkerSymbol tapSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, Color.White, 14);

            // Create a graphics overlay to display geodesic polygons, set its renderer and add it to the map view.
            GraphicsOverlay geodesicPolysOverlay = new GraphicsOverlay
            {
                Id = "GeodesicPolys",
                Renderer = new SimpleRenderer(geodesicBufferFillSymbol)
            };
            MyMapView.GraphicsOverlays.Add(geodesicPolysOverlay);

            // Create a graphics overlay to display planar polygons, set its renderer and add it to the map view.
            GraphicsOverlay planarPolysOverlay = new GraphicsOverlay
            {
                Id = "PlanarPolys",
                Renderer = new SimpleRenderer(planarBufferFillSymbol)
            };
            MyMapView.GraphicsOverlays.Add(planarPolysOverlay);

            // Create a graphics overlay to display tap locations for buffers, set its renderer and add it to the map view.
            GraphicsOverlay tapLocationsOverlay = new GraphicsOverlay
            {
                Id = "TapPoints",
                Renderer = new SimpleRenderer(tapSymbol)
            };
            MyMapView.GraphicsOverlays.Add(tapLocationsOverlay);

            // Show the colors for each type of buffer in the UI.
            ShowBufferSwatches(planarBufferColor, geodesicBufferColor);
        }

        private void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            try
            {
                // Get the location tapped by the user (a map point in the WebMercator projected coordinate system).
                MapPoint userTapPoint = e.Location;

                // Get the buffer distance (miles) entered in the text box.
                double bufferInMiles = System.Convert.ToDouble(BufferDistanceMilesTextBox.Text);

                // Call a helper method to convert the input distance to meters.
                double bufferInMeters = LinearUnits.Miles.ToMeters(bufferInMiles);

                // Create a planar buffer graphic around the input location at the specified distance.
                Geometry bufferGeometryPlanar = userTapPoint.Buffer(bufferInMeters);
                Graphic planarBufferGraphic = new Graphic(bufferGeometryPlanar);

                // Create a geodesic buffer graphic using the same location and distance.
                Geometry bufferGeometryGeodesic = userTapPoint.BufferGeodetic(bufferInMeters, LinearUnits.Meters, double.NaN, GeodeticCurveType.Geodesic);
                Graphic geodesicBufferGraphic = new Graphic(bufferGeometryGeodesic);

                // Create a graphic for the user tap location.
                Graphic locationGraphic = new Graphic(userTapPoint);

                // Get the graphics overlays.
                GraphicsOverlay planarBufferGraphicsOverlay = MyMapView.GraphicsOverlays["PlanarPolys"];
                GraphicsOverlay geodesicBufferGraphicsOverlay = MyMapView.GraphicsOverlays["GeodesicPolys"];
                GraphicsOverlay tapPointGraphicsOverlay = MyMapView.GraphicsOverlays["TapPoints"];

                // Add the buffer polygons and tap location graphics to the appropriate graphic overlays.
                planarBufferGraphicsOverlay.Graphics.Add(planarBufferGraphic);
                geodesicBufferGraphicsOverlay.Graphics.Add(geodesicBufferGraphic);
                tapPointGraphicsOverlay.Graphics.Add(locationGraphic);
            }
            catch (System.Exception ex)
            {
                // Display an error message if there is a problem generating the buffers.
                MessageBox.Show(ex.Message, "Error creating buffers");
            }
        }

        private void ShowBufferSwatches(Color planarBufferColor, Color geodesicBufferColor)
        {
            // Create an equivalent System.Windows.Media.Color for each of the buffer symbol colors (System.Drawing.Color).
            System.Windows.Media.Color planarLabelColor = System.Windows.Media.Color.FromArgb(
                planarBufferColor.A,
                planarBufferColor.R,
                planarBufferColor.G,
                planarBufferColor.B);
            System.Windows.Media.Color geodesicLabelColor = System.Windows.Media.Color.FromArgb(
                geodesicBufferColor.A,
                geodesicBufferColor.R,
                geodesicBufferColor.G,
                geodesicBufferColor.B);

            // Show buffer symbol colors in the UI by setting the appropriate Ellipse object fill color.
            BufferSwatchPlanarEllipse.Fill = new System.Windows.Media.SolidColorBrush(planarLabelColor);
            BufferSwatchGeodesicEllipse.Fill = new System.Windows.Media.SolidColorBrush(geodesicLabelColor);
        }

        private void ClearBuffersButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear the buffer and point graphics.
            foreach (GraphicsOverlay ov in MyMapView.GraphicsOverlays)
            {
                ov.Graphics.Clear();
            }
        }
    }
}