// Copyright 2016 Esri.
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
using Foundation;
using System;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.AddGraphicsRenderer
{
    [Register("AddGraphicsRenderer")]
    public class AddGraphicsRenderer : UIViewController
    {
        // Constant holding offset where the MapView control should start
        private const int yPageOffset = 60;

        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        public AddGraphicsRenderer()
        {
            Title = "Add graphics (Renderer)";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create a map with 'Imagery with Labels' basemap and an initial location
            Map myMap = new Map(BasemapType.ImageryWithLabels, 34.056295, -117.195800, 14);

            // Create graphics when MapView's viewpoint is initialized
            _myMapView.ViewpointChanged += OnViewpointChanged;

            // Assign the map to the MapView
            _myMapView.Map = myMap;
        }

        private void OnViewpointChanged(object sender, EventArgs e)
        {
            // Unhook the event
            _myMapView.ViewpointChanged -= OnViewpointChanged;

            // Get area that is shown in a MapView
            Polygon visibleArea = _myMapView.VisibleArea;
   
            // Get extent of that area
            Envelope extent = visibleArea.Extent;
            
            // Get central point of the extent
            MapPoint centerPoint = extent.GetCenter();

            // Create values inside the visible extent for creating graphic
            var extentWidth = extent.Width / 5;
            var extentHeight = extent.Height / 10;

            // Create point collection
            PointCollection points = new PointCollection(SpatialReferences.WebMercator)
                {
                    new MapPoint(centerPoint.X - extentWidth * 2, centerPoint.Y - extentHeight * 2),
                    new MapPoint(centerPoint.X - extentWidth * 2, centerPoint.Y + extentHeight * 2),
                    new MapPoint(centerPoint.X + extentWidth * 2, centerPoint.Y + extentHeight * 2),
                    new MapPoint(centerPoint.X + extentWidth * 2, centerPoint.Y - extentHeight * 2)
                };

            // Create overlay to where graphics are shown
            GraphicsOverlay overlay = new GraphicsOverlay();

            // Add points to the graphics overlay
            foreach (var point in points)
            {
                // Create new graphic and add it to the overlay
                overlay.Graphics.Add(new Graphic(point));
            }

            // Create symbol for points
            SimpleMarkerSymbol pointSymbol = new SimpleMarkerSymbol()
            {
                Color = System.Drawing.Color.Yellow,
                Size = 30,
                Style = SimpleMarkerSymbolStyle.Square,
            };

            // Create simple renderer with symbol
            SimpleRenderer renderer = new SimpleRenderer(pointSymbol);

            // Set renderer to graphics overlay
            overlay.Renderer = renderer;

            // Make sure that the UI changes are done in the UI thread
            InvokeOnMainThread(() =>
            {
                // Add created overlay to the MapView
                _myMapView.GraphicsOverlays.Add(overlay);
            });
        }

        private void CreateLayout()
        {
            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(
                0, yPageOffset, View.Bounds.Width, View.Bounds.Height - yPageOffset);

            // Add MapView to the page
            View.AddSubviews(_myMapView);
        }
    }
}