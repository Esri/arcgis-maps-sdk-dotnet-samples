// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.UseDistanceCompositeSym
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Distance composite scene symbol",
        category: "Symbology",
        description: "Change a graphic's symbol based on the camera's proximity to it.",
        instructions: "The sample starts looking at a plane. Zoom out from the plane to see it turn into a cone. Keeping zooming out and it will turn into a point.",
        tags: new[] { "3D", "data", "graphic", "range", "symbol" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("681d6f7694644709a7c830ec57a2d72b")]
    public partial class UseDistanceCompositeSym
    {
        public UseDistanceCompositeSym()
        {
            InitializeComponent();

            // Create the Scene, basemap, graphic, and composite symbol.
            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                // Create a new Scene with an imagery basemap.
                Scene myScene = new Scene(BasemapStyle.ArcGISImageryStandard);

                // Add the Scene to the SceneView.
                MySceneView.Scene = myScene;

                // Create a new GraphicsOverlay and add it to the SceneView.
                GraphicsOverlay graphicsOverlay = new GraphicsOverlay();
                graphicsOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Relative;
                MySceneView.GraphicsOverlays.Add(graphicsOverlay);

                // Call a function to create a new distance composite symbol with three ranges.
                DistanceCompositeSceneSymbol compositeSymbol = await CreateCompositeSymbol();

                // Create a new point graphic with the composite symbol, add it to the graphics overlay.
                MapPoint locationPoint = new MapPoint(-2.708471, 56.096575, 5000, SpatialReferences.Wgs84);
                Graphic pointGraphic = new Graphic(locationPoint, compositeSymbol);
                graphicsOverlay.Graphics.Add(pointGraphic);

                // Add an orbit camera controller to lock the camera to the graphic.
                OrbitGeoElementCameraController cameraController = new OrbitGeoElementCameraController(pointGraphic, 20)
                {
                    CameraPitchOffset = 80,
                    CameraHeadingOffset = -30
                };
                MySceneView.CameraController = cameraController;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
            }
        }

        private async Task<DistanceCompositeSceneSymbol> CreateCompositeSymbol()
        {
            // Get the path to the 3D model.
            string modelPath = GetModelPath();

            // Create three symbols for displaying a feature according to its distance from the camera.
            // First, a 3D model symbol (airplane) for when the camera is near the feature.
            ModelSceneSymbol plane3DSymbol = await ModelSceneSymbol.CreateAsync(new Uri(modelPath), 1.0);

            // 3D (blue cone) symbol for when the feature is at an intermediate range.
            SimpleMarkerSceneSymbol coneSym = new SimpleMarkerSceneSymbol(SimpleMarkerSceneSymbolStyle.Cone, Color.LightSkyBlue, 15, 6, 3, SceneSymbolAnchorPosition.Center)
            {
                // The cone will point in the same direction as the plane.
                Pitch = -90
            };

            // Simple marker symbol (circle) when the feature is far from the camera.
            SimpleMarkerSymbol markerSym = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.LightSkyBlue, 10.0);

            // Create three new ranges for displaying each symbol.
            DistanceSymbolRange closeRange = new DistanceSymbolRange(plane3DSymbol, 0, 100);
            DistanceSymbolRange midRange = new DistanceSymbolRange(coneSym, 100, 500);
            DistanceSymbolRange farRange = new DistanceSymbolRange(markerSym, 500, 0);

            // Create a new DistanceCompositeSceneSymbol and add the ranges.
            DistanceCompositeSceneSymbol compositeSymbol = new DistanceCompositeSceneSymbol();
            compositeSymbol.Ranges.Add(closeRange);
            compositeSymbol.Ranges.Add(midRange);
            compositeSymbol.Ranges.Add(farRange);

            // Return the new composite symbol.
            return compositeSymbol;
        }

        private static string GetModelPath()
        {
            return DataManager.GetDataFolder("681d6f7694644709a7c830ec57a2d72b", "Bristol.dae");
        }
    }
}