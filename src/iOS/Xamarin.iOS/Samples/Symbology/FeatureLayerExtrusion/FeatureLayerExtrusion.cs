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
        name: "Feature layer extrusion",
        category: "Symbology",
        description: "Extrude features based on their attributes.",
        instructions: "Press the button to switch between using population density and total population for extrusion. Higher extrusion directly corresponds to higher attribute values.",
        tags: new[] { "3D", "extrude", "extrusion", "extrusion expression", "height", "renderer", "scene" })]
    public class FeatureLayerExtrusion : UIViewController
    {
        // Hold references to UI controls.
        private SceneView _mySceneView;
        private UISegmentedControl _extrusionFieldButton;

        public FeatureLayerExtrusion()
        {
            Title = "Feature layer extrusion";
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

        private void ToggleExtrusionButton_Clicked(object sender, EventArgs e)
        {
            // Get the first layer from the scene view's operation layers, it should be a feature layer.
            FeatureLayer censusFeatureLayer = (FeatureLayer) _mySceneView.Scene.OperationalLayers[0];

            // Get the renderer from the feature layer.
            Renderer censusRenderer = censusFeatureLayer.Renderer;

            // Get the scene properties from the feature layer's renderer.
            RendererSceneProperties sceneProperties = censusRenderer.SceneProperties;

            // Toggle the feature layer's scene properties renderer extrusion expression and change the button text.
            if (((UISegmentedControl) sender).SelectedSegment == 0)
            {
                // An offset of 100000 is added to ensure that polygons for large areas (like Alaska)
                // with low populations will be extruded above the curvature of the Earth.
                sceneProperties.ExtrusionExpression = "[POP07_SQMI] * 5000 + 100000";
            }
            else
            {
                sceneProperties.ExtrusionExpression = "[POP2007] / 10";
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

            _mySceneView = new SceneView();
            _mySceneView.TranslatesAutoresizingMaskIntoConstraints = false;

            _extrusionFieldButton = new UISegmentedControl("Population density", "Total population")
            {
                BackgroundColor = ApplicationTheme.BackgroundColor,
                SelectedSegment = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Add the views.
            View.AddSubviews(_mySceneView, _extrusionFieldButton);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _mySceneView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                _mySceneView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _mySceneView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _mySceneView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),

                _extrusionFieldButton.LeadingAnchor.ConstraintEqualTo(View.LayoutMarginsGuide.LeadingAnchor),
                _extrusionFieldButton.TrailingAnchor.ConstraintEqualTo(View.LayoutMarginsGuide.TrailingAnchor),
                _extrusionFieldButton.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor, 8)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _extrusionFieldButton.ValueChanged += ToggleExtrusionButton_Clicked;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _extrusionFieldButton.ValueChanged -= ToggleExtrusionButton_Clicked;
        }
    }
}