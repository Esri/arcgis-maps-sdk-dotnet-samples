// Copyright 2019 Esri.
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
using Foundation;
using System.Drawing;
using UIKit;

namespace ArcGISRuntime.Samples.AddGraphicsRenderer
{
    [Register("AddGraphicsRenderer")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Add graphics with renderer",
        "GraphicsOverlay",
        "A renderer allows you to change the style of all graphics in a graphics overlay by referencing a single symbol style.",
        "Run the sample and view graphics for points, lines, and polygons, which are stylized using renderers.",
        "GraphicsOverlay", "SimpleMarkerSymbol", "SimpleRenderer")]
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
            // Create a map with 'Imagery with Labels' basemap.
            Map myMap = new Map(Basemap.CreateImageryWithLabels());

            // Assign the map to the MapView.
            _myMapView.Map = myMap;

            // Create a center point for the graphics.
            MapPoint centerPoint = new MapPoint(-117.195800, 34.056295, SpatialReferences.Wgs84);

            // Create an envelope from that center point.
            Envelope pointExtent = new Envelope(centerPoint, .07, .035);

            // Create a collection of points on the corners of the envelope.
            PointCollection points = new PointCollection(SpatialReferences.Wgs84)
            {
                new MapPoint(pointExtent.XMax, pointExtent.YMax),
                new MapPoint(pointExtent.XMax, pointExtent.YMin),
                new MapPoint(pointExtent.XMin, pointExtent.YMax),
                new MapPoint(pointExtent.XMin, pointExtent.YMin),
            };

            // Create overlay to where graphics are shown.
            GraphicsOverlay overlay = new GraphicsOverlay();

            // Add points to the graphics overlay.
            foreach (MapPoint point in points)
            {
                // Create new graphic and add it to the overlay.
                overlay.Graphics.Add(new Graphic(point));
            }

            // Create symbol for points.
            SimpleMarkerSymbol pointSymbol = new SimpleMarkerSymbol()
            {
                Color = Color.Yellow,
                Size = 30,
                Style = SimpleMarkerSymbolStyle.Square
            };

            // Create simple renderer with symbol.
            SimpleRenderer renderer = new SimpleRenderer(pointSymbol);

            // Set renderer to graphics overlay.
            overlay.Renderer = renderer;

            // Add created overlay to the MapView.
            _myMapView.GraphicsOverlays.Add(overlay);

            // Center the MapView on the points.
            _myMapView.SetViewpointGeometryAsync(pointExtent, 50);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = UIColor.White };

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