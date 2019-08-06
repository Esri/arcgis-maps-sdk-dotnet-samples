// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UI.GeoAnalysis;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ViewshedGeoElement
{
    [Register("ViewshedGeoElement")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("07d62a792ab6496d9b772a24efea45d0")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Viewshed (GeoElement)",
        "Analysis",
        "Display a live viewshed analysis for a moving GeoElement.",
        "Tap on the scene to see the tank move to that point.",
        "Featured")]
    public class ViewshedGeoElement : UIViewController
    {
        // Hold references to UI controls.
        private SceneView _mySceneView;

        // URLs to the scene layer with buildings and the elevation source.
        private readonly Uri _elevationUri = new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer");
        private readonly Uri _buildingsUri = new Uri("https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Building_Johannesburg/SceneServer");

        // Graphic and overlay for showing the tank.
        private readonly GraphicsOverlay _tankOverlay = new GraphicsOverlay();
        private Graphic _tank;

        // Animation properties.
        private MapPoint _tankEndPoint;

        // Units for geodetic calculation (used in animating tank).
        private readonly LinearUnit _metersUnit = LinearUnits.Meters;
        private readonly AngularUnit _degreesUnit = AngularUnits.Degrees;

        // Timer for running the animation.
        private Timer _animationTimer;

        public ViewshedGeoElement()
        {
            Title = "Viewshed (GeoElement)";
        }

        private async void Initialize()
        {
            // Create the scene with an imagery basemap.
            _mySceneView.Scene = new Scene(Basemap.CreateImagery());

            // Add the elevation surface.
            ArcGISTiledElevationSource tiledElevationSource = new ArcGISTiledElevationSource(_elevationUri);
            _mySceneView.Scene.BaseSurface = new Surface
            {
                ElevationSources = {tiledElevationSource}
            };

            // Add buildings.
            ArcGISSceneLayer sceneLayer = new ArcGISSceneLayer(_buildingsUri);
            _mySceneView.Scene.OperationalLayers.Add(sceneLayer);

            // Configure the graphics overlay for the tank and add the overlay to the SceneView.
            _tankOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Relative;
            _mySceneView.GraphicsOverlays.Add(_tankOverlay);

            // Configure the heading expression for the tank; this will allow the
            //     viewshed to update automatically based on the tank's position.
            _tankOverlay.Renderer = new SimpleRenderer
            {
                SceneProperties = {HeadingExpression = "[HEADING]"}
            };

            try
            {
                // Create the tank graphic - get the model path.
                string modelPath = DataManager.GetDataFolder("07d62a792ab6496d9b772a24efea45d0", "bradle.3ds");
                // - Create the symbol and make it 10x larger (to be the right size relative to the scene).
                ModelSceneSymbol tankSymbol = await ModelSceneSymbol.CreateAsync(new Uri(modelPath), 10);
                // - Adjust the position.
                tankSymbol.Heading = 90;
                // - The tank will be positioned relative to the scene surface by its bottom.
                //     This ensures that the tank is on the ground rather than partially under it.
                tankSymbol.AnchorPosition = SceneSymbolAnchorPosition.Bottom;
                // - Create the graphic.
                _tank = new Graphic(new MapPoint(28.047199, -26.189105, SpatialReferences.Wgs84), tankSymbol);
                // - Update the heading.
                _tank.Attributes["HEADING"] = 0.0;
                // - Add the graphic to the overlay.
                _tankOverlay.Graphics.Add(_tank);

                // Create a viewshed for the tank.
                GeoElementViewshed geoViewshed = new GeoElementViewshed(
                    geoElement: _tank,
                    horizontalAngle: 90.0,
                    verticalAngle: 40.0,
                    minDistance: 0.1,
                    maxDistance: 250.0,
                    headingOffset: 0.0,
                    pitchOffset: 0.0)
                {
                    // Offset viewshed observer location to top of tank.
                    OffsetZ = 3.0
                };

                // Create the analysis overlay and add to the scene.
                AnalysisOverlay overlay = new AnalysisOverlay();
                overlay.Analyses.Add(geoViewshed);
                _mySceneView.AnalysisOverlays.Add(overlay);

                // Create and use a camera controller to orbit the tank.
                _mySceneView.CameraController = new OrbitGeoElementCameraController(_tank, 200.0)
                {
                    CameraPitchOffset = 45.0
                };

                // Create a timer; this will enable animating the tank.
                _animationTimer = new Timer(60)
                {
                    Enabled = true,
                    AutoReset = true
                };
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        private void UpdateAnimation(object sender, EventArgs e) => AnimateTank();

        private void SceneViewTapped(object sender, GeoViewInputEventArgs e) => _tankEndPoint = e.Location;

        private void AnimateTank()
        {
            // Return if the tank already arrived.
            if (_tankEndPoint == null)
            {
                return;
            }

            // Get current location and distance from the destination.
            MapPoint location = (MapPoint) _tank.Geometry;
            GeodeticDistanceResult distance = GeometryEngine.DistanceGeodetic(
                location, _tankEndPoint, _metersUnit, _degreesUnit, GeodeticCurveType.Geodesic);

            // Move the tank a short distance.
            location = GeometryEngine.MoveGeodetic(new List<MapPoint> {location}, 1.0, _metersUnit, distance.Azimuth1, _degreesUnit,
                GeodeticCurveType.Geodesic).First();
            _tank.Geometry = location;

            // Rotate to face the destination.
            double heading = (double) _tank.Attributes["HEADING"];
            heading += (distance.Azimuth1 - heading) / 10;
            _tank.Attributes["HEADING"] = heading;

            // Clear the destination if the tank already arrived.
            if (distance.Distance < 5)
            {
                _tankEndPoint = null;
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            _mySceneView = new SceneView();
            _mySceneView.TranslatesAutoresizingMaskIntoConstraints = false;

            View = new UIView {BackgroundColor = UIColor.White};

            // Add the views.
            View.AddSubviews(_mySceneView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _mySceneView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _mySceneView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _mySceneView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _mySceneView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _animationTimer.Elapsed += UpdateAnimation;
            _animationTimer.Start();
            _mySceneView.GeoViewTapped += SceneViewTapped;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _mySceneView.GeoViewTapped -= SceneViewTapped;

            // End the timer and unsubscribe.
            _animationTimer.Stop();
            _animationTimer.Elapsed -= UpdateAnimation;
        }
    }
}