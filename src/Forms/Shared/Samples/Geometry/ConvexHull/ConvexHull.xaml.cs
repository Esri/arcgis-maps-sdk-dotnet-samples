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
using Esri.ArcGISRuntime.Xamarin.Forms;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Color = System.Drawing.Color;

namespace ArcGISRuntime.Samples.ConvexHull
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Convex hull",
        "Geometry",
        "This sample demonstrates how to use the GeometryEngine.ConvexHull operation to generate a polygon that encloses a series of user-tapped map points.",
        "Tap on the map in several places, then click the 'Convex Hull' button.",
        "Analysis", "ConvexHull", "GeometryEngine")]
    public partial class ConvexHull : ContentPage
    {
        // Graphics overlay to display the hull.
        private GraphicsOverlay _graphicsOverlay;

        // List of geometry values (MapPoints in this case) that will be used by the GeometryEngine.ConvexHull operation.
        private List<Geometry> _inputPointsList = new List<Geometry>();

        // List of geometry values (MapPoints in this case) that will be used by the GeometryEngine.ConvexHull operation.
        private PointCollection _inputPointCollection = new PointCollection(SpatialReferences.WebMercator);

        public ConvexHull()
        {
            InitializeComponent();

            Initialize();
        }

        private void Initialize()
        {
            // Disable the button to create a hull.
            ConvexHullButton.IsEnabled = false;

            // Create a map with a topographic basemap.
            Map theMap = new Map(Basemap.CreateTopographic());

            // Assign the map to the MapView.
            MyMapView.Map = theMap;

            // Create an overlay to hold the lines of the hull.
            _graphicsOverlay = new GraphicsOverlay();

            // Add the graphics overlay to the MapView.
            MyMapView.GraphicsOverlays.Add(_graphicsOverlay);

            // Wire up the MapView's GeoViewTapped event handler.
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        private void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Add the map point to the list that will be used by the GeometryEngine.ConvexHull operation.
                _inputPointCollection.Add(e.Location);

                // Check if there are at least three points.
                if (_inputPointCollection.Count > 2)
                {
                    // Enable the button for creating hulls.
                    ConvexHullButton.IsEnabled = true;
                }

                // Create a simple marker symbol to display where the user tapped/clicked on the map. The marker symbol
                // will be a solid, red circle.
                SimpleMarkerSymbol userTappedSimpleMarkerSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.Red, 10);

                // Create a new graphic for the spot where the user clicked on the map using the simple marker symbol.
                Graphic userTappedGraphic = new Graphic(e.Location, new Dictionary<string, object>() { { "Type", "Point" } }, userTappedSimpleMarkerSymbol) { ZIndex = 0 };

                // Set the Z index for the user tapped graphic so that it appears above the convex hull graphic(s) added later.
                userTappedGraphic.ZIndex = 1;

                // Add the user tapped/clicked map point graphic to the graphic overlay.
                _graphicsOverlay.Graphics.Add(userTappedGraphic);
            }
            catch (System.Exception ex)
            {
                // Display an error message if there is a problem adding user tapped graphics.
                Application.Current.MainPage.DisplayAlert("Error", "Can't add user tapped graphic: " + ex.Message, "OK");
            }
        }

        private void ConvexHullButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Create a multi-point geometry from the user tapped input map points.
                Multipoint inputMultipoint = new Multipoint(_inputPointCollection);

                // Get the returned result from the convex hull operation.
                Geometry convexHullGeometry = GeometryEngine.ConvexHull(inputMultipoint);

                // Create a simple line symbol for the outline of the convex hull graphic(s).
                SimpleLineSymbol convexHullSimpleLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Blue, 4);

                // Create the simple fill symbol for the convex hull graphic(s) - comprised of a fill style, fill
                // color and outline. It will be a hollow (i.e.. see-through) polygon graphic with a thick red outline.
                SimpleFillSymbol convexHullSimpleFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Null, Color.Red,
                    convexHullSimpleLineSymbol);

                // Create the graphic for the convex hull - comprised of a polygon shape and fill symbol.
                Graphic convexHullGraphic = new Graphic(convexHullGeometry, new Dictionary<string, object>() { { "Type", "Hull" } }, convexHullSimpleFillSymbol) { ZIndex = 1 };

                // Remove any existing convex hull graphics from the overlay.
                foreach (Graphic g in new List<Graphic>(_graphicsOverlay.Graphics))
                {
                    if ((string)g.Attributes["Type"] == "Hull")
                    {
                        _graphicsOverlay.Graphics.Remove(g);
                    }
                }
                // Add the convex hull graphic to the graphics overlay.
                _graphicsOverlay.Graphics.Add(convexHullGraphic);

                // Disable the button after has been used.
                ConvexHullButton.IsEnabled = false;
            }
            catch (System.Exception ex)
            {
                // Display an error message if there is a problem generating convex hull operation.
                Application.Current.MainPage.DisplayAlert("Geometry Engine Failed", "Error performing convex hull: " + ex.Message, "OK");
            }
        }

        private void ResetButton_Clicked(object sender, EventArgs e)
        {
            // Clear the existing points and graphics.
            _inputPointCollection.Clear();
            _graphicsOverlay.Graphics.Clear();

            // Disable the convex hull button.
            ConvexHullButton.IsEnabled = false;
        }
    }
}