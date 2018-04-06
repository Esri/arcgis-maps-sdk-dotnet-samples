// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System.Drawing;
using CoreGraphics;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.NearestVertex
{
    [Register("NearestVertex")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Nearest vertex",
        "GeometryEngine",
        "This sample demonstrates how to use the Geometry engine find the nearest vertex and nearest coordinate of a polygon to a point. The distance for each is shown.",
        "Tap on the map. The nearest point/coordinate and nearest vertex in the polygon will be shown.")]
    public class NearestVertex : UIViewController
    {
        // Label to show the distance (and an initial prompt)
        private readonly UITextView _distanceLabel = new UITextView()
        {
            TextColor = UIColor.Red,
            Text = "Tap to see the nearest vertex and point.",
            TextAlignment = UITextAlignment.Center
        };

        private readonly MapView _myMapView = new MapView();

        // Hold references to the graphics overlay and the polygon graphic
        private GraphicsOverlay _graphicsOverlay;
        private Graphic _polygonGraphic;

        // Hold references to the graphics for the user and results points
        private Graphic _tappedLocationGraphic;
        private Graphic _nearestVertexGraphic;
        private Graphic _nearestCoordinateGraphic;

        public NearestVertex()
        {
            Title = "Nearest vertex";
        }

        private void Initialize()
        {
            // Configure the basemap
            _myMapView.Map = new Map(Basemap.CreateTopographic());

            // Create the graphics overlay and set the selection color
            _graphicsOverlay = new GraphicsOverlay();

            // Add the overlay to the MapView
            _myMapView.GraphicsOverlays.Add(_graphicsOverlay);

            // Create the point collection that defines the polygon
            PointCollection polygonPoints = new PointCollection(SpatialReferences.WebMercator)
            {
                new MapPoint(-5991501.677830, 5599295.131468),
                new MapPoint(-6928550.398185, 2087936.739807),
                new MapPoint(-3149463.800709, 1840803.011362),
                new MapPoint(-1563689.043184, 3714900.452072),
                new MapPoint(-3180355.516764, 5619889.608838)
            };

            // Create the polygon
            Polygon polygonGeometry = new Polygon(polygonPoints);

            // Define and apply the symbology
            SimpleLineSymbol polygonOutlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Green, 2);
            SimpleFillSymbol polygonFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.ForwardDiagonal, Color.Green, polygonOutlineSymbol);

            // Create the graphic and add it to the graphics overlay
            _polygonGraphic = new Graphic(polygonGeometry, polygonFillSymbol);
            _graphicsOverlay.Graphics.Add(_polygonGraphic);

            // Create the graphics and symbology for the tapped point, the nearest vertex, and the nearest coordinate
            SimpleMarkerSymbol tappedLocationSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.X, Color.Orange, 15);
            SimpleMarkerSymbol nearestCoordinateSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, Color.Red, 10);
            SimpleMarkerSymbol nearestVertexSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.Blue, 15);
            _nearestCoordinateGraphic = new Graphic { Symbol = nearestCoordinateSymbol };
            _tappedLocationGraphic = new Graphic { Symbol = tappedLocationSymbol };
            _nearestVertexGraphic = new Graphic { Symbol = nearestVertexSymbol };
            
            _graphicsOverlay.Graphics.Add(_tappedLocationGraphic);
            _graphicsOverlay.Graphics.Add(_nearestVertexGraphic);
            _graphicsOverlay.Graphics.Add(_nearestCoordinateGraphic);

            // Listen for taps; the spatial relationships will be updated in the handler
            _myMapView.GeoViewTapped += MapViewTapped;

            // Center the map on the polygon
            MapPoint centerPoint = new MapPoint(-4487263.495911, 3699176.480377, SpatialReferences.WebMercator);
            _myMapView.SetViewpointCenterAsync(centerPoint, 200000000);
        }

        private void MapViewTapped(object sender, GeoViewInputEventArgs geoViewInputEventArgs)
        {
            // Get the tapped location
            MapPoint tappedLocation = geoViewInputEventArgs.Location;

            // Show the tapped location
            _tappedLocationGraphic.Geometry = tappedLocation;

            // Get the nearest vertex in the polygon
            ProximityResult nearestVertexResult = GeometryEngine.NearestVertex(_polygonGraphic.Geometry, tappedLocation);

            // Get the nearest coordinate in the polygon
            ProximityResult nearestCoordinateResult =
                GeometryEngine.NearestCoordinate(_polygonGraphic.Geometry, tappedLocation);

            // Get the distance to the nearest vertex in the polygon
            int distanceVertex = (int)(nearestVertexResult.Distance / 1000);

            // Get the distance to the nearest coordinate in the polygon
            int distanceCoordinate = (int)(nearestCoordinateResult.Distance / 1000);

            // Show the nearest vertex in blue
            _nearestVertexGraphic.Geometry = nearestVertexResult.Coordinate;

            // Show the nearest coordinate in red
            _nearestCoordinateGraphic.Geometry = nearestCoordinateResult.Coordinate;

            // Show the distances in the UI
            _distanceLabel.Text = $"Vertex dist: {distanceVertex} km, Point dist: {distanceCoordinate} km";
        }

        private void CreateLayout()
        {
            // Add the views
            View.AddSubviews(_myMapView, _distanceLabel);

            // Make sure the map attribution isn't covered by the distance label
            _myMapView.ViewInsets = new UIEdgeInsets(0, 0, 30, 0);
        }

        public override void ViewDidLoad()
        {
            CreateLayout();
            Initialize();

            base.ViewDidLoad();
        }

        public override void ViewDidLayoutSubviews()
        {
            _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
            _distanceLabel.Frame = new CGRect(0, View.Bounds.Height - 30, View.Bounds.Width, 30);
            base.ViewDidLayoutSubviews();
        }
    }
}