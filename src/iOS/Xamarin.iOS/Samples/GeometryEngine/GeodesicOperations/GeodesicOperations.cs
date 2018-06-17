// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using CoreGraphics;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.GeodesicOperations
{
    [Register("GeodesicOperations")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Geodesic operations",
        "GeometryEngine",
        "This sample demonstrates how to use the Geometry engine to calculate a geodesic path between two points and measure its distance.",
        "Tap on the map to set the end point of a path from New York City. The geodesic path and geodesic distance will be displayed.")]
    public class GeodesicOperations : UIViewController
    {
        // Create and hold references to UI controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UITextView _distanceLabel = new UITextView
        {
            Text = "Tap to set an end point.",
            TextAlignment = UITextAlignment.Center
        };

        // Hold references to the graphics.
        private Graphic _startLocationGraphic;
        private Graphic _endLocationGraphic;
        private Graphic _pathGraphic;

        public GeodesicOperations()
        {
            Title = "Geodesic operations";
        }

        private void Initialize()
        {
            // Create and show a new map with imagery basemap.
            _myMapView.Map = new Map(Basemap.CreateImagery());

            // Create the graphics overlay and add it to the map view.
            GraphicsOverlay graphicsOverlay = new GraphicsOverlay();
            _myMapView.GraphicsOverlays.Add(graphicsOverlay);

            // Add a graphic at JFK to serve as the origin.
            MapPoint start = new MapPoint(-73.7781, 40.6413, SpatialReferences.Wgs84);
            SimpleMarkerSymbol startMarker = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Blue, 10);
            _startLocationGraphic = new Graphic(start, startMarker);

            // Create the graphic for the destination.
            _endLocationGraphic = new Graphic
            {
                Symbol = startMarker
            };

            // Create the graphic for the path.
            _pathGraphic = new Graphic
            {
                Symbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, System.Drawing.Color.Blue, 5)
            };

            // Add the graphics to the overlay.
            graphicsOverlay.Graphics.Add(_startLocationGraphic);
            graphicsOverlay.Graphics.Add(_endLocationGraphic);
            graphicsOverlay.Graphics.Add(_pathGraphic);

            // Update end location when the user taps.
            _myMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        private void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs geoViewInputEventArgs)
        {
            // Get the tapped point, projected to WGS84.
            MapPoint destination = (MapPoint)GeometryEngine.Project(geoViewInputEventArgs.Location, SpatialReferences.Wgs84);

            // Update the destination graphic.
            _endLocationGraphic.Geometry = destination;

            // Get the points that define the route polyline.
            PointCollection polylinePoints = new PointCollection(SpatialReferences.Wgs84)
            {
                (MapPoint)_startLocationGraphic.Geometry,
                destination
            };

            // Create the polyline for the two points.
            Polyline routeLine = new Polyline(polylinePoints);

            // Densify the polyline to show the geodesic curve.
            Geometry pathGeometry = GeometryEngine.DensifyGeodetic(routeLine, 1, LinearUnits.Kilometers, GeodeticCurveType.Geodesic);

            // Apply the curved line to the path graphic.
            _pathGraphic.Geometry = pathGeometry;

            // Calculate and show the distance.
            double distance = GeometryEngine.LengthGeodetic(pathGeometry, LinearUnits.Kilometers, GeodeticCurveType.Geodesic);
            _distanceLabel.Text = $"{(int)distance} kilometers";
        }

        private void CreateLayout()
        {
            // Add the views.
            View.AddSubviews(_myMapView, _distanceLabel);

            // Make sure the map attribution isn't covered by the distance label.
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
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat frameHeight = 30;

                // Reposition the controls.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, frameHeight, 0);
                _distanceLabel.Frame = new CGRect(0, View.Bounds.Height - frameHeight, View.Bounds.Width, frameHeight);

                base.ViewDidLayoutSubviews();
            }
            catch (NullReferenceException)
            {
            }
        }
    }
}