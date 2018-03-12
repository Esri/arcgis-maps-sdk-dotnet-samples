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
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UI.GeoAnalysis;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using ArcGISRuntime.Samples.Managers;
using UIKit;

namespace ArcGISRuntime.Samples.ViewshedGeoElement
{
    [Register("ViewshedGeoElement")]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData("07d62a792ab6496d9b772a24efea45d0")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Viewshed (GeoElement)",
        "Analysis",
        "This sample demonstrates how to display a live viewshed analysis for a moving GeoElement. The analysis is offset vertically so that the viewpoint is from the top of the GeoElement (in this case, a model of a tank).",
        "Tap on the scene to see the tank move to that point.",
        "Featured")]
    public class ViewshedGeoElement : UIViewController
    {
        // Create and hold reference to the used MapView.
        private readonly SceneView _mySceneView = new SceneView();

        // URLs to the scene layer with buildings and the elevation source
        private readonly Uri _elevationUri = new Uri("https://scene.arcgis.com/arcgis/rest/services/BREST_DTM_1M/ImageServer");
        private readonly Uri _buildingsUri = new Uri("https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Buildings_Brest/SceneServer/layers/0");

        // Graphic and overlay for showing the tank
        private readonly GraphicsOverlay _tankOverlay = new GraphicsOverlay();
        private Graphic _tank;

        // Animation properties
        private MapPoint _tankEndPoint;

        // Units for geodetic calculation (used in animating tank)
        private readonly LinearUnit _metersUnit = (LinearUnit)Unit.FromUnitId(9001);
        private readonly AngularUnit _degreesUnit = (AngularUnit)Unit.FromUnitId(9102);

        public ViewshedGeoElement()
        {
            Title = "Viewshed (GeoElement)";
        }

        private async Task Initialize()
        {
            // Create the scene with an imagery basemap.
            _mySceneView.Scene = new Scene(Basemap.CreateImagery());

            // Add the elevation surface.
            ArcGISTiledElevationSource tiledElevationSource = new ArcGISTiledElevationSource(_elevationUri);
            Surface baseSurface = new Surface
            {
                ElevationSources = { tiledElevationSource }
            };
            _mySceneView.Scene.BaseSurface = baseSurface;

            // Add buildings.
            _mySceneView.Scene.OperationalLayers.Add(new ArcGISSceneLayer(_buildingsUri));

            // Configure the graphics overlay for the tank and add the overlay to the SceneView.
            _tankOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Relative;
            _mySceneView.GraphicsOverlays.Add(_tankOverlay);

            // Configure the heading expression for the tank; this will allow the
            //     viewshed to update automatically based on the tank's position.
            SimpleRenderer renderer3D = new SimpleRenderer();
            renderer3D.SceneProperties.HeadingExpression = "[HEADING]";
            _tankOverlay.Renderer = renderer3D;

            // Create the tank graphic - get the model path.
            string modelPath = GetModelPath();
            // - Create the symbol and make it 10x larger (to be the right size relative to the scene).
            ModelSceneSymbol tankSymbol = await ModelSceneSymbol.CreateAsync(new Uri(modelPath), 10);
            // - Adjust the position.
            tankSymbol.Heading = 90;
            // - The tank will be positioned relative to the scene surface by its bottom.
            //     This ensures that the tank is on the ground rather than partially under it.
            tankSymbol.AnchorPosition = SceneSymbolAnchorPosition.Bottom;
            // - Create the graphic.
            _tank = new Graphic(new MapPoint(-4.506390, 48.385624, SpatialReferences.Wgs84), tankSymbol);
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

            // Create a camera controller to orbit the tank.
            OrbitGeoElementCameraController cameraController = new OrbitGeoElementCameraController(_tank, 200.0)
            {
                CameraPitchOffset = 45.0
            };
            // - Apply the camera controller to the SceneView.
            _mySceneView.CameraController = cameraController;

            // Create a timer; this will enable animating the tank.
            Timer animationTimer = new Timer(60)
            {
                Enabled = true,
                AutoReset = true
            };
            // - Move the tank every time the timer expires.
            animationTimer.Elapsed += (o, e) =>
            {
                AnimateTank();
            };
            // - Start the timer.
            animationTimer.Start();

            // Allow the user to click to define a new destination.
            _mySceneView.GeoViewTapped += (sender, args) => { _tankEndPoint = args.Location; };
        }

        private void AnimateTank()
        {
            // Return if the tank already arrived.
            if (_tankEndPoint == null)
            {
                return;
            }

            // Get current location and distance from the destination.
            MapPoint location = (MapPoint)_tank.Geometry;
            GeodeticDistanceResult distance = GeometryEngine.DistanceGeodetic(
                location, _tankEndPoint, _metersUnit, _degreesUnit, GeodeticCurveType.Geodesic);

            // Move the tank a short distance.
            location = GeometryEngine.MoveGeodetic(new List<MapPoint>() { location }, 1.0, _metersUnit, distance.Azimuth1, _degreesUnit,
                GeodeticCurveType.Geodesic).First();
            _tank.Geometry = location;

            // Rotate to face the destination.
            double heading = (double)_tank.Attributes["HEADING"];
            heading = heading + ((distance.Azimuth1 - heading) / 10);
            _tank.Attributes["HEADING"] = heading;

            // Clear the destination if the tank already arrived.
            if (distance.Distance < 5)
            {
                _tankEndPoint = null;
            }
        }

        private static string GetModelPath()
        {
            // Returns the tank model.
            return DataManager.GetDataFolder("07d62a792ab6496d9b772a24efea45d0", "bradle.3ds");
        }

        private void CreateLayout()
        {
            // Add MapView to the page.
            View.AddSubviews(_mySceneView);
        }

        public override async void ViewDidLoad()
        {
            CreateLayout();
            await Initialize();

            base.ViewDidLoad();
        }

        public override void ViewDidLayoutSubviews()
        {
            // Setup the visual frame for the MapView.
            _mySceneView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            base.ViewDidLayoutSubviews();
        }
    }
}