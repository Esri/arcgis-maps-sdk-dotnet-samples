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
using System;
using Color = System.Drawing.Color;

namespace ArcGISRuntime.WPF.Samples.SceneSymbols
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Scene symbols",
        "Symbology",
        "Show various kinds of 3D symbols in a scene.",
        "",
        "Scenes", "Symbols", "Graphics", "graphics overlay", "3D", "cone", "cylinder", "tube", "sphere", "diamond", "tetrahedron")]
    public partial class SceneSymbols
    {
        private readonly string _elevationServiceUrl = "http://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer";

        public SceneSymbols()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Configure the scene with an imagery basemap.
            MySceneView.Scene = new Scene(Basemap.CreateImagery());

            // Add a surface to the scene for elevation.
            ArcGISTiledElevationSource elevationSource = new ArcGISTiledElevationSource(new Uri(_elevationServiceUrl));
            Surface surface = new Surface();
            surface.ElevationSources.Add(elevationSource);
            MySceneView.Scene.BaseSurface = surface;

            // Create the graphics overlay.
            GraphicsOverlay overlay = new GraphicsOverlay();

            // Set the surface placement mode for the overlay.
            overlay.SceneProperties.SurfacePlacement = SurfacePlacement.Absolute;

            // Create a graphic for each symbol type and add it to the scene.
            int index = 0;
            Color[] colors = {Color.Red, Color.Green, Color.Blue, Color.Purple, Color.Turquoise, Color.White};
            foreach (SimpleMarkerSceneSymbolStyle symbolStyle in Enum.GetValues(typeof(SimpleMarkerSceneSymbolStyle)))
            {
                // Create the symbol.
                SimpleMarkerSceneSymbol symbol = new SimpleMarkerSceneSymbol(symbolStyle, colors[index], 200, 200, 200, SceneSymbolAnchorPosition.Center);

                // Offset each symbol so that they aren't in the same spot.
                double positionOffset = 0.01 * index;
                MapPoint point = new MapPoint(44.975 + positionOffset, 29, 500, SpatialReferences.Wgs84);

                // Create the graphic from the geometry and the symbol.
                Graphic graphic = new Graphic(point, symbol);

                // Add the graphic to the overlay.
                overlay.Graphics.Add(graphic);

                // Increment the index.
                index++;
            }

            // Show the graphics overlay in the scene.
            MySceneView.GraphicsOverlays.Add(overlay);

            // Set the initial viewpoint.
            Camera initalViewpoint = new Camera(29, 45, 6000, 0, 0, 0);
            MySceneView.SetViewpointCamera(initalViewpoint);
        }
    }
}