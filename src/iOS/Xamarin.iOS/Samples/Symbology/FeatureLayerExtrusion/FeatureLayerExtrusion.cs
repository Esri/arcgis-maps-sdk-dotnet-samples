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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Drawing;
using UIKit;

namespace ArcGISRuntime.Samples.FeatureLayerExtrusion
{
    [Register("FeatureLayerExtrusion")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Feature layer extrusion",
        "Symbology",
        "This sample demonstrates how to apply extrusion to a renderer on a feature layer.",
        "")]
    public class FeatureLayerExtrusion : UIViewController
    {
        // Constant holding offset where the SceneView control should start
        private const int yPageOffset = 60;

        // Create and hold reference to the used SceneView
        private SceneView _mySceneView = new SceneView();

        // Create button
        private UIButton _button_ToggleExtrusionData;

        public FeatureLayerExtrusion()
        {
            Title = "Feature layer extrusion";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references
            CreateLayout();

            Initialize();
        }

        private void Initialize()
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
                SimpleLineSymbol mySimpleLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Black, 1);

                // Create a new simple fill symbol for the feature layer 
                SimpleFillSymbol mysimpleFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.Blue, mySimpleLineSymbol);

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
                _mySceneView.Scene = myScene;

                // Add the feature layer to the scene's operational layer collection
                myScene.OperationalLayers.Add(myFeatureLayer);

                // Create a new map point to define where to look on the scene view
                MapPoint myMapPoint = new MapPoint(-10974490, 4814376, 0, SpatialReferences.WebMercator);

                // Create a new orbit location camera controller using the map point and defined distance
                OrbitLocationCameraController myOrbitLocationCameraController = new OrbitLocationCameraController(myMapPoint, 20000000);

                // Set the scene view's camera controller to the orbit location camera controller
                _mySceneView.CameraController = myOrbitLocationCameraController;
            }
            catch (Exception ex)
            {
                // Something went wrong, display the error
                UIAlertController alert = UIAlertController.Create("Error", ex.Message, UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);
            }
        }

        private void ChangeExtrusionExpression()
        {
            // Get the first layer from the scene view's operation layers, it should be a feature layer
            FeatureLayer myFeatureLayer = (FeatureLayer)_mySceneView.Scene.OperationalLayers[0];

            // Get the renderer from the feature layer
            Renderer myRenderer = myFeatureLayer.Renderer;

            // Get the scene properties from the feature layer's renderer
            RendererSceneProperties myRendererSceneProperties = myRenderer.SceneProperties;

            // Toggle the feature layer's scene properties renderer extrusion expression and change the button text
            if (_button_ToggleExtrusionData.Title(UIControlState.Normal) == "Population Density")
            {
                myRendererSceneProperties.ExtrusionExpression = "[POP07_SQMI] * 5000";
                _button_ToggleExtrusionData.SetTitle("Total Population", UIControlState.Normal);
            }
            else if (_button_ToggleExtrusionData.Title(UIControlState.Normal) == "Total Population")
            {
                myRendererSceneProperties.ExtrusionExpression = "[POP2007] / 10";
                _button_ToggleExtrusionData.SetTitle("Population Density", UIControlState.Normal);
            }
        }

        public override void ViewDidLayoutSubviews()
        {
            // Setup the visual frame for the MapView
            _mySceneView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            // Setup the visual frame for button1
            _button_ToggleExtrusionData.Frame = new CoreGraphics.CGRect(0, yPageOffset, View.Bounds.Width, 40);

            base.ViewDidLayoutSubviews();
        }

        private void OnButton_ToggleExtrusionData_Clicked(object sender, EventArgs e)
        {
            // Call the function to change the feature layer's renderer scene properties extrusion expression
            ChangeExtrusionExpression();
        }

        private void CreateLayout()
        {

            // Create a button
            _button_ToggleExtrusionData = new UIButton();
            _button_ToggleExtrusionData.SetTitle("Population Density", UIControlState.Normal);
            _button_ToggleExtrusionData.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _button_ToggleExtrusionData.BackgroundColor = UIColor.White;

            // Hook the touch event for the button
            _button_ToggleExtrusionData.TouchUpInside += OnButton_ToggleExtrusionData_Clicked;

            // Add SceneView to the page
            View.AddSubviews(_mySceneView, _button_ToggleExtrusionData);
        }
    }
}