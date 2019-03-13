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
using Colors = System.Drawing.Color;

namespace ArcGISRuntime.Samples.Buffer
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Buffer",
        "Geometry",
        "This sample demonstrates how to use `GeometryEngine.Buffer` to create polygons from a map location and linear distance (radius). For each input location, the sample creates two buffer polygons (using the same distance) and displays them on the map using different symbols. One polygon is calculated using the `planar` (flat) coordinate space of the map's spatial reference. The other is created using a `geodesic` technique that considers the curved shape of the Earth's surface (which is generally a more accurate representation). Distortion in the map increases as you move away from the standard parallels of the spatial reference's projection. This map is in Web Mercator so areas near the equator are the most accurate. As you move the buffer location north or south from that line, you'll see a greater difference in the polygon size and shape. Planar operations are generally faster, but performance improvement may only be noticeable for large operations (buffering a great number or complex geometry).\nCreating buffers is a core concept in GIS proximity analysis, allowing you to visualize and locate geographic features contained within a polygon. For example, suppose you wanted to visualize areas of your city where alcohol sales are prohibited because they are within 500 meters of a school. The first step in this proximity analysis would be to generate 500 meter buffer polygons around all schools in the city. Any such businesses you find inside one of the resulting polygons are violating the law. If you are using planar buffers, make sure that the input locations and distance are suited to the spatial reference you're using. Remember that you can also create your buffers using geodesic and then project them to the spatial reference you need for display or analysis. For more information about using buffer analysis, see [How buffer analysis works](https://pro.arcgis.com/en/pro-app/tool-reference/analysis/how-buffer-analysis-works.htm) in the ArcGIS Pro documentation.",
        "1. Tap on the map.\n2. A planar and a geodesic buffer will be created at the tap location using the distance (miles) specified in the text box.\n3. Continue tapping to create additional buffers. Notice that buffers closer to the equator are similar in size. As you move north or south from the equator, however, the geodesic polygons appear larger. Geodesic polygons are in fact a better representation of the true shape and size of the buffer.\n 4. Click `Clear` to remove all buffers and start again.",
        "Buffer, Geodesic, Planar")]
    [ArcGISRuntime.Samples.Shared.Attributes.AndroidLayoutAttribute("Buffer.axml")]
    public class Buffer : Activity
    {
        // Map view control to display the map and buffers.
        private MapView _myMapView = new MapView();

        // Create an EditText to enter a buffer value (in miles). 
        private EditText _bufferDistanceMilesEditText;

        // Text view controls to show the buffer colors in the UI.
        private TextView _geodesicSwatch;
        private TextView _planarSwatch;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Title = "Buffer";

            // Create the UI.
            CreateLayout();

            // Initialize the map and graphics overlays.
            Initialize();
        }

        private void Initialize()
        {
            // Create a map with a topographic basemap and add it to the map view.
            _myMapView.Map = new Map(Basemap.CreateTopographic());

            // Handle the MapView's GeoViewTapped event to create buffers.
            _myMapView.GeoViewTapped += MyMapView_GeoViewTapped;

            // Create a fill symbol for geodesic buffer polygons.            
            Colors geodesicBufferColor = Colors.FromArgb(120, 255, 0, 0);
            SimpleLineSymbol geodesicOutlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, geodesicBufferColor, 2);
            SimpleFillSymbol geodesicBufferFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, geodesicBufferColor, geodesicOutlineSymbol);

            // Create a fill symbol for planar buffer polygons.            
            Colors planarBufferColor = Colors.FromArgb(120, 0, 0, 255);
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
            _myMapView.GraphicsOverlays.Add(geodesicPolysOverlay);

            // Create a graphics overlay to display planar polygons, set its renderer and add it to the map view.
            GraphicsOverlay planarPolysOverlay = new GraphicsOverlay
            {
                Id = "PlanarPolys",
                Renderer = new SimpleRenderer(planarBufferFillSymbol)
            };
            _myMapView.GraphicsOverlays.Add(planarPolysOverlay);

            // Create a graphics overlay to display tap locations for buffers, set its renderer and add it to the map view.
            GraphicsOverlay tapLocationsOverlay = new GraphicsOverlay
            {
                Id = "TapPoints",
                Renderer = new SimpleRenderer(tapSymbol)
            };
            _myMapView.GraphicsOverlays.Add(tapLocationsOverlay);

            // Show the colors for each type of buffer in the UI.
            ShowBufferSwatches(planarBufferColor, geodesicBufferColor);
        }

        private void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Get the location tapped by the user (a map point in the WebMercator projected coordinate system).
                MapPoint userTapPoint = e.Location;

                // Get the buffer distance (miles) entered in the text box.
                double bufferInMiles = System.Convert.ToDouble(_bufferDistanceMilesEditText.Text);

                // Call a helper method to convert the input distance to meters.
                double bufferInMeters = LinearUnits.Miles.ToMeters(bufferInMiles);

                // Create a planar buffer graphic around the input location at the specified distance.
                Geometry bufferGeometryPlanar = GeometryEngine.Buffer(userTapPoint, bufferInMeters);
                Graphic planarBufferGraphic = new Graphic(bufferGeometryPlanar);

                // Create a geodesic buffer graphic using the same location and distance.
                Geometry bufferGeometryGeodesic = GeometryEngine.BufferGeodetic(userTapPoint, bufferInMeters, LinearUnits.Meters, double.NaN, GeodeticCurveType.Geodesic);
                Graphic geodesicBufferGraphic = new Graphic(bufferGeometryGeodesic);

                // Create a graphic for the user tap location.
                Graphic locationGraphic = new Graphic(userTapPoint);

                // Get the graphics overlays.
                GraphicsOverlay planarBufferGraphicsOverlay = _myMapView.GraphicsOverlays["PlanarPolys"];
                GraphicsOverlay geodesicBufferGraphicsOverlay = _myMapView.GraphicsOverlays["GeodesicPolys"];
                GraphicsOverlay tapPointGraphicsOverlay = _myMapView.GraphicsOverlays["TapPoints"];

                // Add the buffer polygons and tap location graphics to the appropriate graphic overlays.
                planarBufferGraphicsOverlay.Graphics.Add(planarBufferGraphic);
                geodesicBufferGraphicsOverlay.Graphics.Add(geodesicBufferGraphic);
                tapPointGraphicsOverlay.Graphics.Add(locationGraphic);
            }
            catch (System.Exception ex)
            {
                // Display an error message if there is a problem generating the buffer polygon.
                AlertDialog.Builder alertBuilder = new AlertDialog.Builder(this);
                alertBuilder.SetTitle("There was a problem generating buffers.");
                alertBuilder.SetMessage(ex.ToString());
                alertBuilder.Show();
            }
        }

        private void ShowBufferSwatches(Colors planarBufferColor, Colors geodesicBufferColor)
        {
            // Create Android.Graphics.Colors to represent the System.Drawing.Colors used for the buffers.
            Android.Graphics.Color planarLabelColor = Android.Graphics.Color.Argb(planarBufferColor.A,
                planarBufferColor.R,
                planarBufferColor.G,
                planarBufferColor.B);
            Android.Graphics.Color geodesicLabelColor = Android.Graphics.Color.Argb(geodesicBufferColor.A,
                geodesicBufferColor.R,
                geodesicBufferColor.G,
                geodesicBufferColor.B);

            // Show buffer symbol colors in the UI by setting the appropriate text view fill color.
            _planarSwatch.SetBackgroundColor(planarLabelColor);
            _geodesicSwatch.SetBackgroundColor(geodesicLabelColor);
        }

        private void ClearBuffersButton_Click(object sender, System.EventArgs e)
        {
            // Clear the buffer and point graphics.
            foreach (GraphicsOverlay ov in _myMapView.GraphicsOverlays)
            {
                ov.Graphics.Clear();
            }
        }

        private void CreateLayout()
        {
            // Load the layout for the sample from the .axml file.
            SetContentView(Resource.Layout.Buffer);

            // Reference controls that will be needed to create the buffers.
            _myMapView = FindViewById<MapView>(Resource.Id.buffer_mapView);
            _bufferDistanceMilesEditText = FindViewById<EditText>(Resource.Id.buffer_distanceText);

            // Add a click event handler for the clear button.
            Button clearBuffersButton = FindViewById<Button>(Resource.Id.buffer_clearButton);
            clearBuffersButton.Click += ClearBuffersButton_Click;

            // Get the text views used to show the buffer colors.
            _geodesicSwatch = FindViewById<TextView>(Resource.Id.buffer_geodesicSwatchLabel);
            _planarSwatch = FindViewById<TextView>(Resource.Id.buffer_planarSwatchLabel);
        }
    }
}