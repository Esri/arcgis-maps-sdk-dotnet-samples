// Copyright 2025 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;

namespace ArcGIS.Samples.ApplyRenderersToSceneLayer
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Apply renderers to scene layer",
        category: "Layers",
        description: "Change the appearance of a 3D object scene layer with different renderers.",
        instructions: "Wait for the scene layer to load. The original scene layer displays 3D textured buildings. Tap on the \"Select Renderer\" dropdown menu and choose a different renderer to change how the buildings are visualized. Each renderer applies different symbology to the scene layer. Setting the renderer to null will remove any applied symbology, reverting the buildings to their original textured appearance.",
        tags: new[] { "3d tiles", "OGC", "OGC API", "layers", "scene", "service" })]
    public partial class ApplyRenderersToSceneLayer
    {
        // ArcGIS Service for world elevation and the Helsinki buildings scene.
        private readonly Uri _elevationSourceUrl = new("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer");
        private readonly Uri _sceneLayerUrl = new("https://services.arcgis.com/V6ZHFr6zdgNZuVG0/arcgis/rest/services/Helsinki_buildings/SceneServer");

        private ArcGISSceneLayer _sceneLayer;

        public ApplyRenderersToSceneLayer()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create new Scene.
            Scene myScene = new(BasemapStyle.ArcGISImageryStandard);

            // Create and add an elevation source for the Scene.
            ArcGISTiledElevationSource elevationSrc = new(_elevationSourceUrl);
            myScene.BaseSurface.ElevationSources.Add(elevationSrc);

            // Create new scene layer from the URL.
            _sceneLayer = new(_sceneLayerUrl);

            // Add created layer to the operational layers collection.
            myScene.OperationalLayers.Add(_sceneLayer);

            // Set the scene on the SceneView to visualize the 3D tiles layer.
            MySceneView.Scene = myScene;

            // Load the scene layer before setting up the view.
            await _sceneLayer.LoadAsync();

            // Create a camera with an initial viewpoint.
            // Camera constructor parameters: latitude, longitude, altitude, heading, pitch, and roll.
            var sceneCamera = new Camera(60.1646, 24.9600, 360, 48.35, 57.84, 0.0);
            MySceneView.SetViewpointCamera(sceneCamera);

            // Populate the picker.
            RendererPicker.ItemsSource = new[]
            {
                "SimpleRenderer - Buildings without texture",
                "UniqueValueRenderer - Buildings by usage",
                "ClassBreakRenderer - Buildings by year completed",
                "NullRenderer - Buildings with original texture"
            };

            // Default to original texture.
            RendererPicker.SelectedIndex = 3;
        }

        private void RendererPicker_SelectionChanged(object sender, EventArgs e)
        {
            ApplyRenderer(RendererPicker.SelectedIndex);
        }

        // Switches the selected renderer according to the renderer type.
        private void ApplyRenderer(int index)
        {
            switch (index)
            {
                case 0:
                    ApplySimpleRenderer();
                    break;

                case 1:
                    ApplyUniqueValueRenderer();
                    break;

                case 2:
                    ApplyClassBreakRenderer();
                    break;

                case 3:
                    ApplyNullRenderer();
                    break;
            }
        }

        // Simple renderer using a multilayer mesh symbol with a material fill layer (colorMixMode = Replace) and edges.
        private void ApplySimpleRenderer()
        {
            var material = new MaterialFillSymbolLayer(System.Drawing.Color.Yellow)
            {
                ColorMixMode = ColorMixMode.Replace,
                Edges = new SymbolLayerEdges3D(System.Drawing.Color.Black, 0.5)
            };
            var mesh = new MultilayerMeshSymbol(material);
            _sceneLayer.Renderer = new SimpleRenderer(mesh);
        }

        // Unique value renderer using multilayer mesh symbols by building usage
        // The default Multiply colorMixMode blends the new color with the texture.
        private void ApplyUniqueValueRenderer()
        {
            static MultilayerMeshSymbol Mesh(int a, int r, int g, int b) =>
                new(new MaterialFillSymbolLayer(System.Drawing.Color.FromArgb(a, r, g, b)));

            var defaultMesh = Mesh(255, 230, 230, 230);

            List<UniqueValue> uniqueValues =
                 [
                     new("commercial buildings", "commercial buildings",
                        Mesh(200, 245, 213, 169),
                        [ "general or commercial" ]),

                    new("residential buildings", "residential buildings",
                        Mesh(255, 210, 254, 208),
                        [ "residential" ]),

                    new("other", "other",
                        Mesh(150, 253, 198, 227),
                        [ "other" ])
                 ];

            _sceneLayer.Renderer = new UniqueValueRenderer(
                new List<string> { "usage" },
                uniqueValues: uniqueValues,
                defaultLabel: string.Empty,
                defaultSymbol: defaultMesh);
        }

        // Class breaks renderer uses multilayer mesh symbols based on the building's year of completion.
        // The Tint colorMixMode applies the new color over a desaturated texture.
        private void ApplyClassBreakRenderer()
        {
            // Local helper to make the mesh symbol with tint.
            static MultilayerMeshSymbol Mesh(int a, int r, int g, int b) =>
                new(new MaterialFillSymbolLayer(System.Drawing.Color.FromArgb(a, r, g, b))
                {
                    ColorMixMode = ColorMixMode.Tint
                });

            var classBreaks = new List<ClassBreak>
                {
                    new("before 1900",  "before 1900",  1725, 1899,  Mesh(255, 230, 238, 207)),
                    new("1900 - 1956",  "1900 - 1956", 1900, 1956,  Mesh(255, 155, 196, 193)),
                    new("1957 - 2000",  "1957 - 2000", 1957, 2000,  Mesh(255, 105, 168, 183)),
                    new("after 2000",   "after 2000",  2001, 3000,  Mesh(255,  75, 126, 152))
                };

            _sceneLayer.Renderer = new ClassBreaksRenderer("yearCompleted", classBreaks);
        }

        private void ApplyNullRenderer()
        {
            _sceneLayer.Renderer = null;
        }
    }
}