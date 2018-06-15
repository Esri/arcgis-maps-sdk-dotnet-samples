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
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media;

namespace ArcGISRuntime.UWP.Samples.Buffer
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Buffer",
        "GeometryEngine",
        "This sample demonstrates how to use GeometryEngine to create planar and geodesic buffer polygons from a map location and buffer distance. It illustrates the difference between planar and geodesic results.",
        "1. Tap on the map.\n2. A planar and a geodesic buffer will be created at the tap location using the distance (miles) specified in the text box.\n3. Continue tapping to create additional buffers. Notice that buffers closer to the equator are similar in size. As you move north or south from the equator, however, the geodesic polygons appear larger. Geodesic polygons are in fact a better representation of the true shape and size of the buffer.\n 4. Click `Clear` to remove all buffers and start again.",
        "")]
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
            MyMapView.Map = new Map(Basemap.CreateTopographic());

            // Handle the MapView's GeoViewTapped event to create buffers.
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;

            // Create a fill symbol for geodesic buffer polygons.            
            System.Drawing.Color geodesicBufferColor = System.Drawing.Color.FromArgb(120, 255, 0, 0);
            SimpleLineSymbol geodesicOutlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, geodesicBufferColor, 2);
            SimpleFillSymbol geodesicBufferFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, geodesicBufferColor, geodesicOutlineSymbol);

            // Create a fill symbol for planar buffer polygons.            
            System.Drawing.Color planarBufferColor = System.Drawing.Color.FromArgb(120, 0, 0, 255);
            SimpleLineSymbol planarOutlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, planarBufferColor, 2);
            SimpleFillSymbol planarBufferFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, planarBufferColor, planarOutlineSymbol);

            // Create a marker symbol for tap locations.
            SimpleMarkerSymbol tapSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, System.Drawing.Color.White, 14);

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

                // Convert the input distance to meters. There are 1609.34 meters in one mile.
                double bufferInMeters = bufferInMiles * 1609.34;

                // Create a planar buffer graphic around the input location at the specified distance.
                Esri.ArcGISRuntime.Geometry.Geometry bufferGeometryPlanar = GeometryEngine.Buffer(userTapPoint, bufferInMeters);
                Graphic planarBufferGraphic = new Graphic(bufferGeometryPlanar);

                // Create a geodesic buffer graphic using the same location and distance.
                Esri.ArcGISRuntime.Geometry.Geometry bufferGeometryGeodesic = GeometryEngine.BufferGeodetic(userTapPoint, bufferInMeters, LinearUnits.Meters, double.NaN, GeodeticCurveType.Geodesic);
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
                MessageDialog dialog = new MessageDialog(ex.Message, "Error creating buffers");
                dialog.ShowAsync();
            }
        }

        private void ShowBufferSwatches(System.Drawing.Color planarBufferColor, System.Drawing.Color geodesicBufferColor)
        {
            // Create an equivalent System.Windows.Media.Color for each of the buffer symbol colors (System.Drawing.Color).
            Color planarLabelColor = Color.FromArgb(
                planarBufferColor.A,
                planarBufferColor.R,
                planarBufferColor.G,
                planarBufferColor.B);
            Color geodesicLabelColor = Color.FromArgb(
                geodesicBufferColor.A,
                geodesicBufferColor.R,
                geodesicBufferColor.G,
                geodesicBufferColor.B);

            // Show buffer symbol colors in the UI by setting the appropriate Ellipse object fill color.
            BufferSwatchPlanarEllipse.Fill = new SolidColorBrush(planarLabelColor);
            BufferSwatchGeodesicEllipse.Fill = new SolidColorBrush(geodesicLabelColor);
        }

        private void ClearBuffersButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Clear the buffer and point graphics.
            foreach (GraphicsOverlay ov in MyMapView.GraphicsOverlays)
            {
                ov.Graphics.Clear();
            }
        }
    }
}