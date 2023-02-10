// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;

using Colors = System.Drawing.Color;

namespace ArcGIS.Samples.FeatureLayerExtrusion
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Feature layer extrusion",
        category: "Symbology",
        description: "Extrude features based on their attributes.",
        instructions: "Press the button to switch between using population density and total population for extrusion. Higher extrusion directly corresponds to higher attribute values.",
        tags: new[] { "3D", "extrude", "extrusion", "extrusion expression", "height", "renderer", "scene" })]
    public partial class FeatureLayerExtrusion : ContentPage
    {
        public FeatureLayerExtrusion()
        {
            InitializeComponent();

            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                // Define the Uri for the service feature table (US state polygons).
                Uri myServiceFeatureTable_Uri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer/3");

                // Create a new service feature table from the Uri.
                ServiceFeatureTable myServiceFeatureTable = new ServiceFeatureTable(myServiceFeatureTable_Uri);

                // Create a new feature layer from the service feature table.
                FeatureLayer myFeatureLayer = new FeatureLayer(myServiceFeatureTable)
                {
                    // Set the rendering mode of the feature layer to be dynamic (needed for extrusion to work).
                    RenderingMode = FeatureRenderingMode.Dynamic
                };

                // Create a new simple line symbol for the feature layer.
                SimpleLineSymbol mySimpleLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.Black, 1);

                // Create a new simple fill symbol for the feature layer.
                SimpleFillSymbol mysimpleFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Colors.Blue, mySimpleLineSymbol);

                // Create a new simple renderer for the feature layer.
                SimpleRenderer mySimpleRenderer = new SimpleRenderer(mysimpleFillSymbol);

                // Get the scene properties from the simple renderer.
                RendererSceneProperties myRendererSceneProperties = mySimpleRenderer.SceneProperties;

                // Set the extrusion mode for the scene properties.
                myRendererSceneProperties.ExtrusionMode = ExtrusionMode.AbsoluteHeight;

                // Set the initial extrusion expression to the 2007 population field divided of 10 for better visibility.
                myRendererSceneProperties.ExtrusionExpression = "[POP2007] / 10";

                // Set the feature layer's renderer to the define simple renderer.
                myFeatureLayer.Renderer = mySimpleRenderer;

                // Create a new scene with the topographic backdrop.
                Scene myScene = new Scene(BasemapStyle.ArcGISTopographic);

                // Set the scene view's scene to the newly create one.
                MySceneView.Scene = myScene;

                // Add the feature layer to the scene's operational layer collection.
                myScene.OperationalLayers.Add(myFeatureLayer);

                // Create a new map point to define where to look on the scene view.
                MapPoint myMapPoint = new MapPoint(-10974490, 4814376, 0, SpatialReferences.WebMercator);

                // Create a new orbit location camera controller using the map point and defined distance.
                OrbitLocationCameraController myOrbitLocationCameraController = new OrbitLocationCameraController(myMapPoint, 20000000);

                // Set the scene view's camera controller to the orbit location camera controller.
                MySceneView.CameraController = myOrbitLocationCameraController;
            }
            catch (Exception ex)
            {
                // Something went wrong, display the error.
                await Application.Current.MainPage.DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private void ChangeExtrusionExpression()
        {
            // Get the first layer from the scene view's operation layers, it should be a feature layer.
            FeatureLayer myFeatureLayer = (FeatureLayer)MySceneView.Scene.OperationalLayers[0];

            // Get the renderer from the feature layer.
            Renderer myRenderer = myFeatureLayer.Renderer;

            // Get the scene properties from the feature layer's renderer.
            RendererSceneProperties myRendererSceneProperties = myRenderer.SceneProperties;

            // Toggle the feature layer's scene properties renderer extrusion expression and change the button text.
            if (ToggleExtrusionDataButton.Text == "Show population density")
            {
                // An offset of 100000 is added to ensure that polygons for large areas (like Alaska)
                // with low populations will be extruded above the curvature of the Earth.
                myRendererSceneProperties.ExtrusionExpression = "[POP07_SQMI] * 5000 + 100000";
                ToggleExtrusionDataButton.Text = "Show total population";
            }
            else if (ToggleExtrusionDataButton.Text == "Show total population")
            {
                myRendererSceneProperties.ExtrusionExpression = "[POP2007] / 10";
                ToggleExtrusionDataButton.Text = "Show population density";
            }
        }

        private void ToggleExtrusionData_Click(object sender, EventArgs e)
        {
            // Call the function to change the feature layer's renderer scene properties extrusion expression.
            ChangeExtrusionExpression();
        }
    }
}