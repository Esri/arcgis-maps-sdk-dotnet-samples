// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Drawing;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ScenePropertiesExpressions
{
    [Register("ScenePropertiesExpressions")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Scene properties expressions",
        category: "GraphicsOverlay",
        description: "Update the orientation of a graphic using expressions based on its attributes.",
        instructions: "Adjust the heading and pitch sliders to rotate the cone.",
        tags: new[] { "3D", "expression", "graphics", "heading", "pitch", "rotation", "scene", "symbology" })]
    public class ScenePropertiesExpressions : UIViewController
    {
        // Hold references to UI controls.
        private SceneView _mySceneView;
        private UISlider _headingSlider;
        private UISlider _pitchSlider;
        private UILabel _pitchValueLabel;
        private UILabel _headingValueLabel;
        private Graphic _cone;

        public ScenePropertiesExpressions()
        {
            Title = "Scene properties expressions";
        }

        private void Initialize()
        {
            // Set up the scene with an imagery basemap.
            _mySceneView.Scene = new Scene(Basemap.CreateImagery());

            // Set the initial viewpoint for the scene.
            MapPoint point = new MapPoint(83.9, 28.4, 1000, SpatialReferences.Wgs84);
            Camera initialCamera = new Camera(point, 1000, 0, 50, 0);
            _mySceneView.SetViewpointCamera(initialCamera);

            // Create a graphics overlay.
            GraphicsOverlay overlay = new GraphicsOverlay();
            overlay.SceneProperties.SurfacePlacement = SurfacePlacement.Relative;
            _mySceneView.GraphicsOverlays.Add(overlay);

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
            _cone = new Graphic(conePoint, coneSymbol);

            // Add the cone graphic to the overlay.
            overlay.Graphics.Add(_cone);
        }

        private void HeightSlider_ValueChanged(object sender, EventArgs e)
        {
            _cone.Attributes["HEADING"] = _headingSlider.Value;
            _headingValueLabel.Text = _headingSlider.Value.ToString("###");
        }

        private void PitchSlider_ValueChanged(object sender, EventArgs e)
        {
            _cone.Attributes["PITCH"] = _pitchSlider.Value;
            _pitchValueLabel.Text = _pitchSlider.Value.ToString("###");
        }

        public override void LoadView()
        {
            // Create and configure views.
            _mySceneView = new SceneView();
            _mySceneView.TranslatesAutoresizingMaskIntoConstraints = false;

            _headingSlider = new UISlider();
            _headingSlider.TranslatesAutoresizingMaskIntoConstraints = false;
            _headingSlider.MinValue = 0;
            _headingSlider.MaxValue = 360;

            _pitchSlider = new UISlider();
            _pitchSlider.TranslatesAutoresizingMaskIntoConstraints = false;
            _pitchSlider.MinValue = -90;
            _pitchSlider.MaxValue = 90;

            UILabel headingLabel = new UILabel();
            headingLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            headingLabel.Text = "Heading: ";

            UILabel pitchLabel = new UILabel();
            pitchLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            pitchLabel.Text = "Pitch:";

            _pitchValueLabel = new UILabel();
            _pitchValueLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            _pitchValueLabel.TextAlignment = UITextAlignment.Center;

            _headingValueLabel = new UILabel();
            _headingValueLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            _headingValueLabel.TextAlignment = UITextAlignment.Center;

            View = new UIView {BackgroundColor = UIColor.White};

            // Add the views.
            View.AddSubviews(_mySceneView, _headingSlider, _pitchSlider, headingLabel, _headingValueLabel, pitchLabel, _pitchValueLabel);

            // Configure constraints.
            _mySceneView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _mySceneView.BottomAnchor.ConstraintEqualTo(headingLabel.TopAnchor).Active = true;
            _mySceneView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _mySceneView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;

            pitchLabel.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor, -8).Active = true;
            pitchLabel.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor, 8).Active = true;
            pitchLabel.HeightAnchor.ConstraintEqualTo(36).Active = true;

            headingLabel.BottomAnchor.ConstraintEqualTo(pitchLabel.TopAnchor).Active = true;
            headingLabel.LeadingAnchor.ConstraintEqualTo(pitchLabel.LeadingAnchor).Active = true;
            headingLabel.TrailingAnchor.ConstraintEqualTo(pitchLabel.TrailingAnchor).Active = true;
            headingLabel.HeightAnchor.ConstraintEqualTo(36).Active = true;

            _headingSlider.BottomAnchor.ConstraintEqualTo(headingLabel.BottomAnchor).Active = true;
            _headingSlider.TrailingAnchor.ConstraintEqualTo(_headingValueLabel.LeadingAnchor, -8).Active = true;
            _headingSlider.LeadingAnchor.ConstraintEqualTo(headingLabel.TrailingAnchor).Active = true;

            _pitchSlider.LeadingAnchor.ConstraintEqualTo(pitchLabel.TrailingAnchor).Active = true;
            _pitchSlider.TrailingAnchor.ConstraintEqualTo(_pitchValueLabel.LeadingAnchor, -8).Active = true;
            _pitchSlider.BottomAnchor.ConstraintEqualTo(pitchLabel.BottomAnchor).Active = true;

            _pitchValueLabel.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor).Active = true;
            _pitchValueLabel.WidthAnchor.ConstraintEqualTo(50).Active = true;
            _pitchValueLabel.BottomAnchor.ConstraintEqualTo(pitchLabel.BottomAnchor).Active = true;

            _headingValueLabel.TrailingAnchor.ConstraintEqualTo(_pitchValueLabel.TrailingAnchor).Active = true;
            _headingValueLabel.LeadingAnchor.ConstraintEqualTo(_pitchValueLabel.LeadingAnchor).Active = true;
            _headingValueLabel.BottomAnchor.ConstraintEqualTo(headingLabel.BottomAnchor).Active = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _headingSlider.ValueChanged += HeightSlider_ValueChanged;
            _pitchSlider.ValueChanged += PitchSlider_ValueChanged;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _headingSlider.ValueChanged -= HeightSlider_ValueChanged;
            _pitchSlider.ValueChanged -= PitchSlider_ValueChanged;
        }
    }
}