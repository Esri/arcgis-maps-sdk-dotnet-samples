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
using Color = System.Drawing.Color;

namespace ArcGIS.WPF.Samples.ScenePropertiesExpressions
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Scene properties expressions",
        category: "GraphicsOverlay",
        description: "Update the orientation of a graphic using expressions based on its attributes.",
        instructions: "Adjust the heading and pitch sliders to rotate the cone.",
        tags: new[] { "3D", "expression", "graphics", "heading", "pitch", "rotation", "scene", "symbology" })]
    public partial class ScenePropertiesExpressions
    {
        public ScenePropertiesExpressions()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Set up the scene with an imagery basemap.
            MySceneView.Scene = new Scene(BasemapStyle.ArcGISImageryStandard);

            // Set the initial viewpoint for the scene.
            MapPoint point = new MapPoint(83.9, 28.4, 1000, SpatialReferences.Wgs84);
            Camera initialCamera = new Camera(point, 1000, 0, 50, 0);
            MySceneView.SetViewpointCamera(initialCamera);

            // Create a graphics overlay.
            GraphicsOverlay overlay = new GraphicsOverlay();
            overlay.SceneProperties.SurfacePlacement = SurfacePlacement.Relative;
            MySceneView.GraphicsOverlays.Add(overlay);

            // Add a renderer using rotation expressions.
            SimpleRenderer renderer = new SimpleRenderer();
            renderer.SceneProperties.HeadingExpression = "[HEADING]";
            renderer.SceneProperties.PitchExpression = "[PITCH]";

            // Apply the renderer to the graphics overlay.
            overlay.Renderer = renderer;

            // Create a red cone graphic.
            SimpleMarkerSceneSymbol coneSymbol = SimpleMarkerSceneSymbol.CreateCone(Color.Red, 100, 100);
            coneSymbol.Pitch = -90;
            MapPoint conePoint = new MapPoint(83.9, 28.41, 200, SpatialReferences.Wgs84);
            Graphic cone = new Graphic(conePoint, coneSymbol);

            // Add the cone graphic to the overlay.
            overlay.Graphics.Add(cone);

            // Listen for changes in slider values and update graphic properties.
            HeadingSlider.ValueChanged += (sender, e) => { cone.Attributes["HEADING"] = HeadingSlider.Value; };
            PitchSlider.ValueChanged += (sender, e) => { cone.Attributes["PITCH"] = PitchSlider.Value; };
        }
    }
}