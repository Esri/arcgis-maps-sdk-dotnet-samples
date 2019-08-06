// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.AddGraphicsRenderer
{
    [Register("AddGraphicsRenderer")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Add graphics (SimpleRenderer)",
        "GraphicsOverlay",
        "This sample demonstrates how you add graphics and set a renderer on a graphic overlays.",
        "")]
    public class AddGraphicsRenderer : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;

        public AddGraphicsRenderer()
        {
            Title = "Add graphics (Renderer)";
        }

        private void Initialize()
        {
            // Create a map with 'Imagery with Labels' basemap and an initial location.
            Map map = new Map(BasemapType.ImageryWithLabels, 34.056295, -117.195800, 14);

            // Create graphics when MapView's viewpoint is initialized.
            _myMapView.ViewpointChanged += OnViewpointChanged;

            // Assign the map to the MapView.
            _myMapView.Map = map;
        }

        private void OnViewpointChanged(object sender, EventArgs e)
        {
            // Unhook the event.
            _myMapView.ViewpointChanged -= OnViewpointChanged;

            // Get area that is shown in a MapView.
            Polygon visibleArea = _myMapView.VisibleArea;

            // Get extent of that area.
            Envelope extent = visibleArea.Extent;

            // Get central point of the extent.
            MapPoint centerPoint = extent.GetCenter();

            // Create values inside the visible extent for creating graphic.
            double extentWidth = extent.Width / 5;
            double extentHeight = extent.Height / 10;

            // Create point collection.
            PointCollection points = new PointCollection(SpatialReferences.WebMercator)
            {
                new MapPoint(centerPoint.X - extentWidth * 2, centerPoint.Y - extentHeight * 2),
                new MapPoint(centerPoint.X - extentWidth * 2, centerPoint.Y + extentHeight * 2),
                new MapPoint(centerPoint.X + extentWidth * 2, centerPoint.Y + extentHeight * 2),
                new MapPoint(centerPoint.X + extentWidth * 2, centerPoint.Y - extentHeight * 2)
            };

            GraphicsOverlay overlay = new GraphicsOverlay();

            // Add points to the graphics overlay.
            foreach (MapPoint point in points)
            {
                // Create new graphic and add it to the overlay.
                overlay.Graphics.Add(new Graphic(point));
            }

            // Create symbol for points.
            SimpleMarkerSymbol pointSymbol = new SimpleMarkerSymbol
            {
                Color = System.Drawing.Color.Yellow,
                Size = 30,
                Style = SimpleMarkerSymbolStyle.Square
            };

            // Create and used simple renderer with symbol.
            overlay.Renderer = new SimpleRenderer(pointSymbol);

            // Make sure that the UI changes are done in the UI thread.
            InvokeOnMainThread(() =>
            {
                // Add created overlay to the MapView.
                _myMapView.GraphicsOverlays.Add(overlay);
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView();

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            // Add the views.
            View.AddSubviews(_myMapView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });
        }
    }
}