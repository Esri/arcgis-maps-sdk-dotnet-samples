// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Drawing;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.SurfacePlacements
{
    [Register("SurfacePlacements")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Surface placement",
        "GraphicsOverlay",
        "This sample demonstrates how to position graphics using different Surface Placements.",
        "")]
    public class SurfacePlacements : UIViewController
    {
        // Create and hold a reference to the SceneView.
        private SceneView _mySceneView;

        public SurfacePlacements()
        {
            Title = "Surface placement";
        }

        public override void LoadView()
        {
            base.LoadView();

            _mySceneView = new SceneView();
            _mySceneView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubviews(_mySceneView);

            _mySceneView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _mySceneView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
            _mySceneView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _mySceneView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Initialize();
        }

        private void Initialize()
        {
            // Create new scene with imagery basemap.
            Scene scene = new Scene(Basemap.CreateImagery());

            // Create a camera with coordinates showing layer data.
            Camera camera = new Camera(53.04, -4.04, 1300, 0, 90.0, 0);

            // Assign the Scene to the SceneView.
            _mySceneView.Scene = scene;

            // Create ElevationSource from elevation data Uri.
            ArcGISTiledElevationSource elevationSource = new ArcGISTiledElevationSource(
                new Uri("http://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer"));

            // Add elevationSource to BaseSurface's ElevationSources.
            _mySceneView.Scene.BaseSurface.ElevationSources.Add(elevationSource);

            // Set view point of scene view using camera.
            _mySceneView.SetViewpointCameraAsync(camera);

            // Create overlays with elevation modes.
            GraphicsOverlay drapedOverlay = new GraphicsOverlay
            {
                SceneProperties = {SurfacePlacement = SurfacePlacement.Draped}
            };
            _mySceneView.GraphicsOverlays.Add(drapedOverlay);

            GraphicsOverlay relativeOverlay = new GraphicsOverlay
            {
                SceneProperties = {SurfacePlacement = SurfacePlacement.Relative}
            };
            _mySceneView.GraphicsOverlays.Add(relativeOverlay);

            GraphicsOverlay absoluteOverlay = new GraphicsOverlay
            {
                SceneProperties = {SurfacePlacement = SurfacePlacement.Absolute}
            };
            _mySceneView.GraphicsOverlays.Add(absoluteOverlay);

            // Create point for graphic location.
            MapPoint point = new MapPoint(-4.04, 53.06, 1000, camera.Location.SpatialReference);

            // Create a red circle symbol.
            SimpleMarkerSymbol circleSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.Red, 10);

            // Create a text symbol for each elevation mode
            TextSymbol drapedText = new TextSymbol("DRAPED", Color.FromArgb(255, 255, 255, 255), 10, HorizontalAlignment.Center, VerticalAlignment.Middle);
            drapedText.OffsetY += 20;

            TextSymbol relativeText = new TextSymbol("RELATIVE", Color.FromArgb(255, 255, 255, 255), 10, HorizontalAlignment.Center, VerticalAlignment.Middle);
            relativeText.OffsetY += 20;

            TextSymbol absoluteText = new TextSymbol("ABSOLUTE", Color.FromArgb(255, 255, 255, 255), 10, HorizontalAlignment.Center, VerticalAlignment.Middle);
            absoluteText.OffsetY += 20;

            // Add the point graphic and text graphic to the corresponding graphics overlay.
            drapedOverlay.Graphics.Add(new Graphic(point, circleSymbol));
            drapedOverlay.Graphics.Add(new Graphic(point, drapedText));

            relativeOverlay.Graphics.Add(new Graphic(point, circleSymbol));
            relativeOverlay.Graphics.Add(new Graphic(point, relativeText));

            absoluteOverlay.Graphics.Add(new Graphic(point, circleSymbol));
            absoluteOverlay.Graphics.Add(new Graphic(point, absoluteText));
        }
    }
}