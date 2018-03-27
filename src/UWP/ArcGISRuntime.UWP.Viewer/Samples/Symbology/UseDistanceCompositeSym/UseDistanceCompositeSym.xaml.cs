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
using Windows.UI;

namespace ArcGISRuntime.UWP.Samples.UseDistanceCompositeSym
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Distance composite symbol",
        "Symbology",
        "This sample demonstrates how to create a `DistanceCompositeSceneSymbol` with unique marker symbols to display at various distances from the camera.",
        "1. When the Scene initially loads, you will be looking at a point graphic symbolized with a 3D cone symbol.\n2. Zoom slightly closer to the graphic and the symbol will change to a blue 3D cube symbol.\n3. Zoom farther from the graphic and the symbol will change back to a 3D cone and then to a simple marker symbol.")]
    public sealed partial class UseDistanceCompositeSym
    {
        // URL for an image service to use as an elevation source
        private string _elevationSourceUrl = @"http://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer";

        public UseDistanceCompositeSym()
        {
            InitializeComponent();

            // Create the Scene, basemap, graphic, and composite symbol
            Initialize();
        }

        private void Initialize()
        {
            // Create a new Scene with an imagery basemap
            Scene myScene = new Scene(Basemap.CreateImagery());

            // Create an elevation source for the Scene
            ArcGISTiledElevationSource elevationSrc = new ArcGISTiledElevationSource(new Uri(_elevationSourceUrl));
            myScene.BaseSurface.ElevationSources.Add(elevationSrc);

            // Add the Scene to the SceneView
            MySceneView.Scene = myScene;

            // Create a new GraphicsOverlay and add it to the SceneView
            GraphicsOverlay graphicsOverlay = new GraphicsOverlay();
            graphicsOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Relative;
            MySceneView.GraphicsOverlays.Add(graphicsOverlay);

            // Call a function to create a new distance composite symbol with three ranges
            DistanceCompositeSceneSymbol compositeSymbol = CreateCompositeSymbol();
            
            // Create a new point graphic with the composite symbol, add it to the graphics overlay
            MapPoint locationPoint = new MapPoint(-2.708471, 56.096575, 5000, SpatialReferences.Wgs84);
            Graphic pointGraphic = new Graphic(locationPoint, compositeSymbol);
            graphicsOverlay.Graphics.Add(pointGraphic);

            // Set the viewpoint with a new camera focused on the graphic
            Camera newCamera = new Camera(new MapPoint(-2.708471, 56.096575, 5000, SpatialReferences.Wgs84), 1500, 0, 80, 0);
            MySceneView.SetViewpointCameraAsync(newCamera);
        }

        private DistanceCompositeSceneSymbol CreateCompositeSymbol()
        {
            // Create three symbols for displaying a feature according to its distance from the camera
            // First, a 3D (blue cube) symbol for when the camera is near the feature
            SimpleMarkerSceneSymbol cubeSym = new SimpleMarkerSceneSymbol(SimpleMarkerSceneSymbolStyle.Cube, Colors.Blue, 125, 125, 125, SceneSymbolAnchorPosition.Center);

            // 3D (red cone) symbol for when the feature is at an intermediate range
            SimpleMarkerSceneSymbol coneSym = new SimpleMarkerSceneSymbol(SimpleMarkerSceneSymbolStyle.Cone, Colors.Red, 75, 75, 75, SceneSymbolAnchorPosition.Bottom);

            // Simple marker symbol (circle) when the feature is far from the camera
            SimpleMarkerSymbol markerSym = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Colors.Yellow, 10.0);

            // Create three new ranges for displaying each symbol
            DistanceSymbolRange closeRange = new DistanceSymbolRange(cubeSym, 0, 999);
            DistanceSymbolRange midRange = new DistanceSymbolRange(coneSym, 1000, 1999);
            DistanceSymbolRange farRange = new DistanceSymbolRange(markerSym, 2000, 0);

            // Create a new DistanceCompositeSceneSymbol and add the ranges
            DistanceCompositeSceneSymbol compositeSymbol = new DistanceCompositeSceneSymbol();
            compositeSymbol.Ranges.Add(closeRange);
            compositeSymbol.Ranges.Add(midRange);
            compositeSymbol.Ranges.Add(farRange);

            // Return the new composite symbol
            return compositeSymbol;
        }
    }
}
