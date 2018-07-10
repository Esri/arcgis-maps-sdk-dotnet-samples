// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Drawing;
using CoreGraphics;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
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
        // Create and hold references to the UI controls.
        private readonly SceneView _mySceneView = new SceneView();
        private readonly UIButton _toggleExtrusionButton = new UIButton();
        private readonly UIToolbar _toolbar = new UIToolbar();

        public FeatureLayerExtrusion()
        {
            Title = "Feature layer extrusion";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references.
            CreateLayout();

            Initialize();
        }

        private void Initialize()
        {
            try
            {
                // Define the URI for the service feature table (US state polygons).
                Uri featureTableUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer/3");

                // Create a new service feature table from the URI.
                ServiceFeatureTable censusServiceFeatureTable = new ServiceFeatureTable(featureTableUri);

                // Create a new feature layer from the service feature table.
                FeatureLayer censusFeatureLayer = new FeatureLayer(censusServiceFeatureTable)
                {
                    // Set the rendering mode of the feature layer to be dynamic (needed for extrusion to work).
                    RenderingMode = FeatureRenderingMode.Dynamic
                };

                // Create a new simple line symbol for the feature layer.
                SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Black, 1);

                // Create a new simple fill symbol for the feature layer.
                SimpleFillSymbol fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.Blue, lineSymbol);

                // Create a new simple renderer for the feature layer.
                SimpleRenderer renderer = new SimpleRenderer(fillSymbol);

                // Get the scene properties from the simple renderer.
                RendererSceneProperties sceneProperties = renderer.SceneProperties;

                // Set the extrusion mode for the scene properties.
                sceneProperties.ExtrusionMode = ExtrusionMode.AbsoluteHeight;

                // Set the initial extrusion expression.
                sceneProperties.ExtrusionExpression = "[POP2007] / 10";

                // Set the feature layer's renderer to the define simple renderer.
                censusFeatureLayer.Renderer = renderer;

                // Create a new scene with a topographic basemap.
                Scene myScene = new Scene(BasemapType.Topographic);

                // Set the scene view's scene to the newly create one.
                _mySceneView.Scene = myScene;

                // Add the feature layer to the scene's operational layer collection.
                myScene.OperationalLayers.Add(censusFeatureLayer);

                // Create a new map point to define where to look on the scene view.
                MapPoint myMapPoint = new MapPoint(-10974490, 4814376, 0, SpatialReferences.WebMercator);

                // Create and use an orbit location camera controller defined by a point and distance.
                _mySceneView.CameraController = new OrbitLocationCameraController(myMapPoint, 20000000);
            }
            catch (Exception ex)
            {
                // Something went wrong, display the error.
                UIAlertController alert = UIAlertController.Create("Error", ex.Message, UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);
            }
        }

        private void ChangeExtrusionExpression()
        {
            // Get the first layer from the scene view's operation layers, it should be a feature layer.
            FeatureLayer censusFeatureLayer = (FeatureLayer) _mySceneView.Scene.OperationalLayers[0];

            // Get the renderer from the feature layer.
            Renderer censusRenderer = censusFeatureLayer.Renderer;

            // Get the scene properties from the feature layer's renderer.
            RendererSceneProperties sceneProperties = censusRenderer.SceneProperties;

            // Toggle the feature layer's scene properties renderer extrusion expression and change the button text.
            if (_toggleExtrusionButton.Title(UIControlState.Normal) == "Show population density")
            {
                // An offset of 100000 is added to ensure that polygons for large areas (like Alaska)
                // with low populations will be extruded above the curvature of the Earth.
                sceneProperties.ExtrusionExpression = "[POP07_SQMI] * 5000 + 100000";
                _toggleExtrusionButton.SetTitle("Show total population", UIControlState.Normal);
            }
            else if (_toggleExtrusionButton.Title(UIControlState.Normal) == "Show total population")
            {
                sceneProperties.ExtrusionExpression = "[POP2007] / 10";
                _toggleExtrusionButton.SetTitle("Show population density", UIControlState.Normal);
            }
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat margin = 5;
                nfloat controlHeight = 30;
                nfloat toolbarHeight = controlHeight + 2 * margin;

                // Reposition the views.
                _mySceneView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _mySceneView.ViewInsets = new UIEdgeInsets(topMargin, 0, toolbarHeight, 0);
                _toolbar.Frame = new CGRect(0, View.Bounds.Height - controlHeight - 2 * margin, View.Bounds.Width, controlHeight + 2 * margin);
                _toggleExtrusionButton.Frame = new CGRect(margin, _toolbar.Frame.Top + 5, View.Bounds.Width - 2 * margin, controlHeight);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private void ToggleExtrusionButton_Clicked(object sender, EventArgs e)
        {
            // Call the function to change the feature layer's renderer scene properties extrusion expression.
            ChangeExtrusionExpression();
        }

        private void CreateLayout()
        {
            // Configure the button.
            _toggleExtrusionButton.SetTitle("Show population density", UIControlState.Normal);
            _toggleExtrusionButton.SetTitleColor(View.TintColor, UIControlState.Normal);

            // Handle button touches.
            _toggleExtrusionButton.TouchUpInside += ToggleExtrusionButton_Clicked;

            // Add controls to the view.
            View.AddSubviews(_mySceneView, _toolbar, _toggleExtrusionButton);
        }
    }
}