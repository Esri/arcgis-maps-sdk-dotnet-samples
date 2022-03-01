// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.GeoAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Popups;
using Microsoft.UI.Xaml;

namespace ArcGISRuntime.WinUI.Samples.ViewshedGeoElement
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Viewshed for GeoElement",
        category: "Analysis",
        description: "Analyze the viewshed for an object (GeoElement) in a scene.",
        instructions: "Tap to set a destination for the vehicle (a GeoElement). The vehicle will 'drive' towards the tapped location. The viewshed analysis will update as the vehicle moves.",
        tags: new[] { "3D", "analysis", "buildings", "model", "scene", "viewshed", "visibility analysis" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("07d62a792ab6496d9b772a24efea45d0")]
    public partial class ViewshedGeoElement
    {
        // URLs to the scene layer with buildings and the elevation source
        private readonly Uri _elevationUri = new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer");
        private readonly Uri _buildingsUri = new Uri("https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/Building_Johannesburg/SceneServer");

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
            InitializeComponent();

            // Setup the control references and execute initialization
            Initialize();
        }

        private async void Initialize()
        {
            // Create the scene with an imagery basemap.
            MySceneView.Scene = new Scene(BasemapStyle.ArcGISImagery);

            // Add the elevation surface.
            ArcGISTiledElevationSource tiledElevationSource = new ArcGISTiledElevationSource(_elevationUri);
            Surface baseSurface = new Surface
            {
                ElevationSources = { tiledElevationSource }
            };
            MySceneView.Scene.BaseSurface = baseSurface;

            // Add buildings.
            ArcGISSceneLayer sceneLayer = new ArcGISSceneLayer(_buildingsUri);
            MySceneView.Scene.OperationalLayers.Add(sceneLayer);

            // Configure graphics overlay for the tank and add the overlay to the SceneView.
            _tankOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Relative;
            MySceneView.GraphicsOverlays.Add(_tankOverlay);

            // Configure heading expression for tank; this will allow the
            //     viewshed to update automatically based on the tank's position.
            SimpleRenderer renderer3D = new SimpleRenderer();
            renderer3D.SceneProperties.HeadingExpression = "[HEADING]";
            _tankOverlay.Renderer = renderer3D;

            try
            {
                // Create the tank graphic - get the model path.
                string modelPath = GetModelPath();
                // - Create the symbol and make it 10x larger (to be the right size relative to the scene).
                ModelSceneSymbol tankSymbol = await ModelSceneSymbol.CreateAsync(new Uri(modelPath), 10);
                // - Adjust the position.
                tankSymbol.Heading = 90;
                // - The tank will be positioned relative to the scene surface by its bottom.
                //       This ensures that the tank is on the ground rather than partially under it.
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
                MySceneView.AnalysisOverlays.Add(overlay);

                // Create a camera controller to orbit the tank.
                OrbitGeoElementCameraController cameraController = new OrbitGeoElementCameraController(_tank, 200.0)
                {
                    CameraPitchOffset = 45.0
                };
                // - Apply the camera controller to the SceneView.
                MySceneView.CameraController = cameraController;

                // Create a timer; this will enable animating the tank.
                DispatcherTimer animationTimer = new DispatcherTimer()
                {
                    Interval = new TimeSpan(0, 0, 0, 0, 60)
                };
                // - Move the tank every time the timer expires.
                animationTimer.Tick += (o, e) => { AnimateTank(); };
                // - Start the timer.
                animationTimer.Start();

                // Allow the user to click to define a new destination.
                MySceneView.GeoViewTapped += (sender, args) => { _tankEndPoint = args.Location; };
            }
            catch (Exception e)
            {
                await new MessageDialog2(e.ToString(), "Error").ShowAsync();
            }
        }

        private void AnimateTank()
        {
            // Return if the tank already arrived.
            if (_tankEndPoint == null)
            {
                return;
            }

            // Get the current location and distance from the destination.
            MapPoint location = (MapPoint)_tank.Geometry;
            GeodeticDistanceResult distance = GeometryEngine.DistanceGeodetic(
                location, _tankEndPoint, _metersUnit, _degreesUnit, GeodeticCurveType.Geodesic);

            // Move the tank a short distance.
            location = GeometryEngine.MoveGeodetic(new List<MapPoint>() { location }, 1.0, _metersUnit, distance.Azimuth1, _degreesUnit,
                GeodeticCurveType.Geodesic).First();
            _tank.Geometry = location;

            // Rotate to face the destination.
            double heading = (double)_tank.Attributes["HEADING"];
            heading = heading + (distance.Azimuth1 - heading) / 10;
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
    }
}