// Copyright 2018 Esri.
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
using System;
using System.Windows;

namespace ArcGIS.WPF.Samples.FeatureLayerExtrusion
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Feature layer extrusion",
        category: "Symbology",
        description: "Extrude features based on their attributes.",
        instructions: "Press the button to switch between using population density and total population for extrusion. Higher extrusion directly corresponds to higher attribute values.",
        tags: new[] { "3D", "extrude", "extrusion", "extrusion expression", "height", "renderer", "scene" })]
    public partial class FeatureLayerExtrusion
    {
        public FeatureLayerExtrusion()
        {
            InitializeComponent();

            Initialize();
        }

        private void Initialize()
        {
            try
            {
                // Define the Uri for the service feature table (US state polygons).
                Uri serviceFeatureTableUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer/3");

                // Create a new service feature table from the Uri.
                ServiceFeatureTable myServiceFeatureTable = new ServiceFeatureTable(serviceFeatureTableUri);

                // Create a new feature layer from the service feature table.
                FeatureLayer myFeatureLayer = new FeatureLayer(myServiceFeatureTable)
                {
                    // Set the rendering mode of the feature layer to be dynamic (needed for extrusion to work).
                    RenderingMode = FeatureRenderingMode.Dynamic
                };

                // Create a new simple line symbol for the feature layer.
                SimpleLineSymbol mySimpleLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Black, 1);

                // Create a new simple fill symbol for the feature layer.
                SimpleFillSymbol mysimpleFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.Color.Blue, mySimpleLineSymbol);

                // Create a new simple renderer for the feature layer.
                SimpleRenderer mySimpleRenderer = new SimpleRenderer(mysimpleFillSymbol);

                // Get the scene properties from the simple renderer.
                RendererSceneProperties myRendererSceneProperties = mySimpleRenderer.SceneProperties;

                // Set the extrusion mode for the scene properties.
                myRendererSceneProperties.ExtrusionMode = ExtrusionMode.AbsoluteHeight;

                // Set the initial extrusion expression to the 2007 population field from the data set divided by 10.
                // This reduces the height of the extruded features for better visibility.
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
                MessageBox.Show(ex.ToString());
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
            if (ToggleDataButton.Content.ToString() == "Show population density")
            {
                // An offset of 100000 is added to ensure that polygons for large areas (like Alaska)
                // with low populations will be extruded above the curvature of the Earth.
                myRendererSceneProperties.ExtrusionExpression = "[POP07_SQMI] * 5000 + 100000";
                ToggleDataButton.Content = "Show total population";
            }
            else if (ToggleDataButton.Content.ToString() == "Show total population")
            {
                myRendererSceneProperties.ExtrusionExpression = "[POP2007] / 10";
                ToggleDataButton.Content = "Show population density";
            }
        }

        private void Button_ToggleExtrusionData_Click(object sender, RoutedEventArgs e)
        {
            // Call the function to change the feature layer's renderer scene properties extrusion expression.
            ChangeExtrusionExpression();
        }
    }
}