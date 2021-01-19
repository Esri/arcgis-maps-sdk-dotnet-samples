// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.EditKmlGroundOverlay
{
    [Register("EditKmlGroundOverlay")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Edit KML ground overlay",
        category: "Layers",
        description: "Edit the values of a KML ground overlay.",
        instructions: "Use the slider to adjust the opacity of the ground overlay.",
        tags: new[] { "KML", "KMZ", "Keyhole", "OGC", "imagery", "Featured" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("1f3677c24b2c446e96eaf1099292e83e")]
    public class EditKmlGroundOverlay : UIViewController
    {
        // Hold references to UI controls.
        private SceneView _mySceneView;
        private UISlider _slider;
        private UILabel _valueLabel;

        // Uri of the image for the ground overlay.
        private readonly Uri _imageryUri = new Uri(DataManager.GetDataFolder("1f3677c24b2c446e96eaf1099292e83e", "1944.jpg"));

        // The ground overlay KML node.
        private KmlGroundOverlay _overlay;

        public EditKmlGroundOverlay()
        {
            Title = "Edit KML ground overlay";
        }

        private void Initialize()
        {
            // Create a scene for the sceneview.
            Scene myScene = new Scene(BasemapStyle.ArcGISImageryStandard);
            _mySceneView.Scene = myScene;

            // Create a geometry for the ground overlay.
            Envelope overlayGeometry = new Envelope(-123.066227926904, 44.04736963555683, -123.0796942287304, 44.03878298600624, SpatialReferences.Wgs84);

            // Create a KML Icon for the overlay image.
            KmlIcon overlayImage = new KmlIcon(_imageryUri);

            // Create the KML ground overlay.
            _overlay = new KmlGroundOverlay(overlayGeometry, overlayImage);

            // Set the rotation of the ground overlay.
            _overlay.Rotation = -3.046024799346924;

            // Create a KML dataset with the ground overlay as the root node.
            KmlDataset dataset = new KmlDataset(_overlay);

            // Create a KML layer for the scene view.
            KmlLayer layer = new KmlLayer(dataset);

            // Add the layer to the map.
            _mySceneView.Scene.OperationalLayers.Add(layer);

            // Move the viewpoint to the ground overlay.
            _mySceneView.SetViewpoint(new Viewpoint(_overlay.Geometry, new Camera(_overlay.Geometry.Extent.GetCenter(), 1250, 45, 60, 0)));

            // Set the default value for the slider.
            _slider.Value = 255;
        }

        private void SliderValueChanged(object sender, EventArgs e)
        {
            // Change the color of the KML ground overlay image to edit the alpha-value. (Other color values are left as-is in the original image.)
            _overlay.Color = System.Drawing.Color.FromArgb((int)((UISlider)sender).Value, 0, 0, 0);

            // Display the value.
            _valueLabel.Text = ((int)((UISlider)sender).Value).ToString();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView { BackgroundColor = ApplicationTheme.BackgroundColor };

            _mySceneView = new SceneView();
            _mySceneView.TranslatesAutoresizingMaskIntoConstraints = false;

            UILabel opacityLabel = new UILabel() { Text = "Opacity ", TranslatesAutoresizingMaskIntoConstraints = false };

            _slider = new UISlider()
            {
                MinValue = 0,
                MaxValue = 255,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            _valueLabel = new UILabel() { Text = "255", TranslatesAutoresizingMaskIntoConstraints = false };

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem {CustomView = opacityLabel},
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem {CustomView = _slider, Width = 200},
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem {CustomView = _valueLabel}
            };

            // Add the views.
            View.AddSubviews(_mySceneView, toolbar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _mySceneView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _mySceneView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _mySceneView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _mySceneView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),

                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor)
            });
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
            _slider.ValueChanged += SliderValueChanged;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _slider.ValueChanged -= SliderValueChanged;
        }
    }
}