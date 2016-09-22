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
using System;
using Xamarin.Forms;

#if WINDOWS_UWP
using Colors = Windows.UI.Colors;
#else
using Colors = System.Drawing.Color;
#endif

namespace ArcGISRuntimeXamarin.Samples.AddGraphicsRenderer
{
    public partial class AddGraphicsRenderer : ContentPage
    {
        public AddGraphicsRenderer()
        {
            InitializeComponent();

            Title = "Add graphics (Renderer)";
            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            // Create a map with 'Imagery with Labels' basemap and an initial location
            Map myMap = new Map(BasemapType.ImageryWithLabels, 34.056295, -117.195800, 14);

            // Create graphics when MapView's viewpoint is initialized
            MyMapView.ViewpointChanged += OnViewpointChanged; 
            
            // Assign the map to the MapView
            MyMapView.Map = myMap;
        }

        private void OnViewpointChanged(object sender, EventArgs e)
        {
            // Unhook the event
            MyMapView.ViewpointChanged -= OnViewpointChanged;

            // Get area that is shown in a MapView
            Polygon visibleArea = MyMapView.VisibleArea;

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
                Color = Colors.Yellow,
                Size = 30,
                Style = SimpleMarkerSymbolStyle.Square,
            };

            // Create simple renderer with symbol
            SimpleRenderer renderer = new SimpleRenderer(pointSymbol);

            // Set renderer to graphics overlay
            overlay.Renderer = renderer;

            // Make sure that the UI changes are done in the UI thread
            Device.BeginInvokeOnMainThread(() =>
            {
                // Add created overlay to the MapView
                MyMapView.GraphicsOverlays.Add(overlay);
            });
        }
    }
}
