// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;

namespace ArcGISRuntime.Samples.IdentifyGraphics
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Identify graphics",
        category: "GraphicsOverlay",
        description: "Display an alert message when a graphic is clicked.",
        instructions: "Select a graphic to identify it. You will see an alert message displayed.",
        tags: new[] { "graphics", "identify" })]
    public class IdentifyGraphics : Activity
    {
        // Hold a reference to the map view
        private MapView _myMapView;

        // Graphics overlay to host graphics
        private GraphicsOverlay _polygonOverlay;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Identify graphics";

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create a map with 'Imagery with Labels' basemap and an initial location
            Map myMap = new Map(BasemapStyle.ArcGISTopographic);

            // Create graphics overlay with graphics
            CreateOverlay();

            _myMapView.GeoViewTapped += OnMapViewTapped;

            // Assign the map to the MapView
            _myMapView.Map = myMap;
        }

        private void CreateOverlay()
        {
            // Create polygon builder and add polygon corners into it
            PolygonBuilder builder = new PolygonBuilder(SpatialReferences.WebMercator);
            builder.AddPoint(new MapPoint(-20e5, 20e5));
            builder.AddPoint(new MapPoint(20e5, 20e5));
            builder.AddPoint(new MapPoint(20e5, -20e5));
            builder.AddPoint(new MapPoint(-20e5, -20e5));

            // Get geometry from the builder
            Polygon polygonGeometry = builder.ToGeometry();

            // Create symbol for the polygon
            SimpleFillSymbol polygonSymbol = new SimpleFillSymbol(
                SimpleFillSymbolStyle.Solid,
                System.Drawing.Color.Yellow,
                null);

            // Create new graphic
            Graphic polygonGraphic = new Graphic(polygonGeometry, polygonSymbol);

            // Create overlay to where graphics are shown
            _polygonOverlay = new GraphicsOverlay();
            _polygonOverlay.Graphics.Add(polygonGraphic);

            // Add created overlay to the MapView
            _myMapView.GraphicsOverlays.Add(_polygonOverlay);
        }

        private async void OnMapViewTapped(object sender, GeoViewInputEventArgs e)
        {
            double tolerance = 10d; // Use larger tolerance for touch
            int maximumResults = 1; // Only return one graphic  
            bool onlyReturnPopups = false; // Don't only return popups

            try
            {
                // Use the following method to identify graphics in a specific graphics overlay
                IdentifyGraphicsOverlayResult identifyResults = await _myMapView.IdentifyGraphicsOverlayAsync(
                    _polygonOverlay,
                    e.Position,
                    tolerance, 
                    onlyReturnPopups, 
                    maximumResults);

                // Check if we got results
                if (identifyResults.Graphics.Count > 0)
                {
                    // Make sure that the UI changes are done in the UI thread
                    RunOnUiThread(() =>
                    {
                        AlertDialog.Builder alert = new AlertDialog.Builder(this);
                        alert.SetMessage("Tapped on graphic");
                        alert.Show();
                    });
                }
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.ToString()).SetTitle("Error").Show();
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create a TextView for instructions.
            TextView sampleInstructionsTextView = new TextView(this)
            {
                Text = "Tap on the graphic to identify it."
            };
            layout.AddView(sampleInstructionsTextView);

            // Add the map view to the layout
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}