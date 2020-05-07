// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Threading.Tasks;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.UseDistanceCompositeSym
{
    [Register("UseDistanceCompositeSym")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Distance composite scene symbol",
        "Symbology",
        "Change a graphic's symbol based on the camera's proximity to it.",
        "The sample starts looking at a plane. Zoom out from the plane to see it turn into a cone. Keeping zooming out and it will turn into a point.",
        "3D", "data", "graphic", "range", "symbol")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("681d6f7694644709a7c830ec57a2d72b")]
    public class UseDistanceCompositeSym : UIViewController
    {
        // Hold references to UI controls.
        private SceneView _mySceneView;

        public UseDistanceCompositeSym()
        {
            Title = "Distance composite symbol";
        }

        private async void Initialize()
        {
            try
            {
                // Create a new Scene with an imagery basemap.
                Scene myScene = new Scene(Basemap.CreateImagery());

                // Add the Scene to the SceneView.
                _mySceneView.Scene = myScene;

                // Create a new GraphicsOverlay and add it to the SceneView.
                GraphicsOverlay graphicsOverlay = new GraphicsOverlay();
                graphicsOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Relative;
                _mySceneView.GraphicsOverlays.Add(graphicsOverlay);

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
                _mySceneView.CameraController = cameraController;
            }
            catch (Exception e)
            {
                // Something went wrong, display the error.
                UIAlertController alert = UIAlertController.Create("Error", e.Message, UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);
            }
        }

        private async Task<DistanceCompositeSceneSymbol> CreateCompositeSymbol()
        {
            // Get the path to the 3D model.
            string modelPath = GetModelPath();

            // Create three symbols for displaying a feature according to its distance from the camera.
            // First, a 3D model symbol (airplane) for when the camera is near the feature.
            ModelSceneSymbol plane3DSymbol = await ModelSceneSymbol.CreateAsync(new System.Uri(modelPath), 1.0);

            // 3D (blue cone) symbol for when the feature is at an intermediate range.
            SimpleMarkerSceneSymbol coneSym = new SimpleMarkerSceneSymbol(SimpleMarkerSceneSymbolStyle.Cone, System.Drawing.Color.LightSkyBlue, 15, 6, 3, SceneSymbolAnchorPosition.Center)
            {
                // The cone will point in the same direction as the plane.
                Pitch = -90
            };

            // Simple marker symbol (circle) when the feature is far from the camera.
            SimpleMarkerSymbol markerSym = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.LightSkyBlue, 10.0);

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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = UIColor.White };

            _mySceneView = new SceneView();
            _mySceneView.TranslatesAutoresizingMaskIntoConstraints = false;

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
    }
}