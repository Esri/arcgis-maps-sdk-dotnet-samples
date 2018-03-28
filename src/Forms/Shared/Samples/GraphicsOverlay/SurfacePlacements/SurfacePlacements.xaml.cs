// Copyright 2017 Esri.
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
using System;
using Xamarin.Forms;

#if WINDOWS_UWP
using Colors = Windows.UI.Colors;
#else
using Colors = System.Drawing.Color;
#endif

namespace ArcGISRuntime.Samples.SurfacePlacements
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Surface placement",
        "GraphicsOverlay",
        "This sample demonstrates how to position graphics using different Surface Placements.",
        "")]
    public partial class SurfacePlacements : ContentPage
    {
        public SurfacePlacements()
        {
            InitializeComponent();

            Title = "Surface placement";
            // Create the UI and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            // Create new Scene
            Scene myScene = new Scene();

            // Set Scene's base map property
            myScene.Basemap = Basemap.CreateImagery();

            // Create a camera with coordinates showing layer data 
            Camera camera = new Camera(53.04, -4.04, 1300, 0, 90.0, 0);

            // Assign the Scene to the SceneView
            MySceneView.Scene = myScene;

            // Create ElevationSource from elevation data Uri
            ArcGISTiledElevationSource elevationSource = new ArcGISTiledElevationSource(
                new Uri("http://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer"));

            // Add elevationSource to BaseSurface's ElevationSources
            MySceneView.Scene.BaseSurface.ElevationSources.Add(elevationSource);

            // Set view point of scene view using camera 
            MySceneView.SetViewpointCameraAsync(camera);

            // Create overlays with elevation modes
            GraphicsOverlay drapedOverlay = new GraphicsOverlay();
            drapedOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Draped;
            MySceneView.GraphicsOverlays.Add(drapedOverlay);

            GraphicsOverlay relativeOverlay = new GraphicsOverlay();
            relativeOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Relative;
            MySceneView.GraphicsOverlays.Add(relativeOverlay);

            GraphicsOverlay absoluteOverlay = new GraphicsOverlay();
            absoluteOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;
            MySceneView.GraphicsOverlays.Add(absoluteOverlay);

            // Create point for graphic location
            MapPoint point = new MapPoint(-4.04, 53.06, 1000, camera.Location.SpatialReference);

            // Create a red circle symbol
            SimpleMarkerSymbol circleSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Colors.Red, 10);

            // Create a text symbol for each elevation mode
            TextSymbol drapedText = new TextSymbol("DRAPED", Colors.White, 10,
                Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Left,
                Esri.ArcGISRuntime.Symbology.VerticalAlignment.Middle);

            TextSymbol relativeText = new TextSymbol("RELATIVE", Colors.White, 10,
                Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Left,
                Esri.ArcGISRuntime.Symbology.VerticalAlignment.Middle);

            TextSymbol absoluteText = new TextSymbol("ABSOLUTE", Colors.White, 10,
                Esri.ArcGISRuntime.Symbology.HorizontalAlignment.Left,
                Esri.ArcGISRuntime.Symbology.VerticalAlignment.Middle);

            // Add the point graphic and text graphic to the corresponding graphics overlay
            drapedOverlay.Graphics.Add(new Graphic(point, circleSymbol));
            drapedOverlay.Graphics.Add(new Graphic(point, drapedText));

            relativeOverlay.Graphics.Add(new Graphic(point, circleSymbol));
            relativeOverlay.Graphics.Add(new Graphic(point, relativeText));

            absoluteOverlay.Graphics.Add(new Graphic(point, circleSymbol));
            absoluteOverlay.Graphics.Add(new Graphic(point, absoluteText));

        }

    }
}