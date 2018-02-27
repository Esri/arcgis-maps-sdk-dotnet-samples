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
using Xamarin.Forms;

#if WINDOWS_UWP
using Colors = Windows.UI.Colors;
#else
using Colors = System.Drawing.Color;
#endif

namespace ArcGISRuntime.Samples.FeatureLayerExtrusion
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Feature layer extrusion",
        "Symbology",
        "This sample demonstrates how to apply extrusion to a renderer on a feature layer.",
        "")]
    public partial class FeatureLayerExtrusion : ContentPage
    {
        public FeatureLayerExtrusion()
        {
            InitializeComponent ();

            Title = "Feature layer extrusion";

            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Define the Uri for the service feature table (US state polygons)
                var myServiceFeatureTable_Uri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer/3");

                // Create a new service feature table from the Uri
                ServiceFeatureTable myServiceFeatureTable = new ServiceFeatureTable(myServiceFeatureTable_Uri);

                // Create a new feature layer from the service feature table
                FeatureLayer myFeatureLayer = new FeatureLayer(myServiceFeatureTable);

                // Set the rendering mode of the feature layer to be dynamic (needed for extrusion to work)
                myFeatureLayer.RenderingMode = FeatureRenderingMode.Dynamic;

                // Create a new simple line symbol for the feature layer
                SimpleLineSymbol mySimpleLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.Black, 1);

                // Create a new simple fill symbol for the feature layer 
                SimpleFillSymbol mysimpleFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Colors.Blue, mySimpleLineSymbol);

                // Create a new simple renderer for the feature layer
                SimpleRenderer mySimpleRenderer = new SimpleRenderer(mysimpleFillSymbol);

                // Get the scene properties from the simple renderer
                RendererSceneProperties myRendererSceneProperties = mySimpleRenderer.SceneProperties;

                // Set the extrusion mode for the scene properties to be base height
                myRendererSceneProperties.ExtrusionMode = ExtrusionMode.BaseHeight;

                // Set the initial extrusion expression
                myRendererSceneProperties.ExtrusionExpression = "[POP2007] / 10";

                // Set the feature layer's renderer to the define simple renderer
                myFeatureLayer.Renderer = mySimpleRenderer;

                // Create a new scene with the topographic backdrop 
                Scene myScene = new Scene(BasemapType.Topographic);

                // Set the scene view's scene to the newly create one
                MySceneView.Scene = myScene;

                // Add the feature layer to the scene's operational layer collection
                myScene.OperationalLayers.Add(myFeatureLayer);

                // Create a new map point to define where to look on the scene view
                MapPoint myMapPoint = new MapPoint(-10974490, 4814376, 0, SpatialReferences.WebMercator);

                // Create a new orbit location camera controller using the map point and defined distance
                OrbitLocationCameraController myOrbitLocationCameraController = new OrbitLocationCameraController(myMapPoint, 20000000);

                // Set the scene view's camera controller to the orbit location camera controller
                MySceneView.CameraController = myOrbitLocationCameraController;
            }
            catch (Exception ex)
            {
                // Something went wrong, display the error
                //MessageBox.Show(ex.ToString());
                await DisplayAlert("Error",ex.ToString(), "OK");
            }
        }

        private void ChangeExtrusionExpression()
        {
            // Get the first layer from the scene view's operation layers, it should be a feature layer
            FeatureLayer myFeatureLayer = (FeatureLayer)MySceneView.Scene.OperationalLayers[0];

            // Get the renderer from the feature layer
            Renderer myRenderer = myFeatureLayer.Renderer;

            // Get the scene properties from the feature layer's renderer
            RendererSceneProperties myRendererSceneProperties = myRenderer.SceneProperties;

            // Toggle the feature layer's scene properties renderer extrusion expression and change the button text
            if (Button_ToggleExtrusionData.Text == "Population Density")
            {
                myRendererSceneProperties.ExtrusionExpression = "[POP07_SQMI] * 5000";
                Button_ToggleExtrusionData.Text = "Total Population";
            }
            else if (Button_ToggleExtrusionData.Text == "Total Population")
            {
                myRendererSceneProperties.ExtrusionExpression = "[POP2007] / 10";
                Button_ToggleExtrusionData.Text = "Population Density";
            }
        }

        private void Button_ToggleExtrusionData_Click(object sender, EventArgs e)
        {
            // Call the function to change the feature layer's renderer scene properties extrusion expression
            ChangeExtrusionExpression();
        }
    }
}
