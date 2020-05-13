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

namespace ArcGISRuntimeXamarin.Samples.Project
{
    [Register("Project")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Project",
        category: "Geometry",
        description: "Project a point from one spatial reference to another.",
        instructions: "Tap anywhere on the map. A callout will display the clicked location's coordinate in the original (basemap's) spatial reference and in the projected spatial reference.",
        tags: new[] { "WGS 84", "Web Mercator", "coordinate system", "coordinates", "latitude", "longitude", "projected", "projection", "spatial reference" })]
    public class Project : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;

        public Project()
        {
            Title = "Project";
        }

        private async void Initialize()
        {
            // Show a map in the default WebMercator spatial reference.
            _myMapView.Map = new Map(Basemap.CreateNationalGeographic());

            // Add a graphics overlay for showing the tapped point.
            GraphicsOverlay overlay = new GraphicsOverlay();
            SimpleMarkerSymbol markerSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.Red, 5);
            overlay.Renderer = new SimpleRenderer(markerSymbol);
            _myMapView.GraphicsOverlays.Add(overlay);

            // Zoom to Minneapolis.
            Envelope startingEnvelope = new Envelope(-10995912.335747, 5267868.874421, -9880363.974046, 5960699.183877,
                SpatialReferences.WebMercator);
            await _myMapView.SetViewpointGeometryAsync(startingEnvelope);
        }

        private void MapView_Tapped(object sender, GeoViewInputEventArgs e)
        {
            // Get the tapped point - this is in the map's spatial reference,
            // which in this case is WebMercator because that is the SR used by the included basemaps.
            MapPoint tappedPoint = e.Location;

            // Update the graphics.
            _myMapView.GraphicsOverlays[0].Graphics.Clear();
            _myMapView.GraphicsOverlays[0].Graphics.Add(new Graphic(tappedPoint));

            // Project the point to WGS84
            MapPoint projectedPoint = (MapPoint) GeometryEngine.Project(tappedPoint, SpatialReferences.Wgs84);

            // Format the results in strings.
            string originalCoords = $"Original: {tappedPoint.X:F4}, {tappedPoint.Y:F4}";
            string projectedCoords = $"Projected: {projectedPoint.X:F4}, {projectedPoint.Y:F4}";

            // Define a callout and show it in the map view.
            CalloutDefinition calloutDef = new CalloutDefinition(projectedCoords, originalCoords);
            _myMapView.ShowCalloutAt(tappedPoint, calloutDef);
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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _myMapView.GeoViewTapped += MapView_Tapped;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _myMapView.GeoViewTapped -= MapView_Tapped;
        }
    }
}