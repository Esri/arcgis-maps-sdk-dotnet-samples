// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.FeatureLayerRenderingModeScene
{
    [Register("FeatureLayerRenderingModeScene")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Feature layer rendering mode (scene)",
        category: "Layers",
        description: "Render features in a scene statically or dynamically by setting the feature layer rendering mode.",
        instructions: "Tap the button to trigger the same zoom animation on both static and dynamicly rendered scenes.",
        tags: new[] { "3D", "dynamic", "feature layer", "features", "rendering", "static" })]
    public class FeatureLayerRenderingModeScene : UIViewController
    {
        // Hold references to UI controls.
        private SceneView _staticSceneView;
        private SceneView _dynamicSceneView;
        private UIStackView _stackView;
        private UIBarButtonItem _zoomButton;

        // Points for demonstrating zoom.
        private readonly MapPoint _zoomedOutPoint = new MapPoint(-118.37, 34.46, SpatialReferences.Wgs84);
        private readonly MapPoint _zoomedInPoint = new MapPoint(-118.45, 34.395, SpatialReferences.Wgs84);

        // Viewpoints for each zoom level.
        private Camera _zoomedOutCamera;
        private Camera _zoomedInCamera;

        // URI for the feature service.
        private const string FeatureService = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Energy/Geology/FeatureServer/";

        // Hold the current zoom state.
        private bool _zoomed;

        public FeatureLayerRenderingModeScene()
        {
            Title = "Feature layer rendering mode (Scene)";
        }

        private void Initialize()
        {
            // Initialize the cameras (viewpoints) with two points.
            _zoomedOutCamera = new Camera(_zoomedOutPoint, 42000, 0, 0, 0);
            _zoomedInCamera = new Camera(_zoomedInPoint, 2500, 90, 75, 0);

            // Create the scene for displaying the feature layer in static mode.
            Scene staticScene = new Scene
            {
                InitialViewpoint = new Viewpoint(_zoomedOutPoint, _zoomedOutCamera)
            };

            // Create the scene for displaying the feature layer in dynamic mode.
            Scene dynamicScene = new Scene
            {
                InitialViewpoint = new Viewpoint(_zoomedOutPoint, _zoomedOutCamera)
            };

            foreach (string identifier in new[] {"8", "9", "0"})
            {
                // Create the table.
                ServiceFeatureTable serviceTable = new ServiceFeatureTable(new Uri(FeatureService + identifier));

                // Create and add the static layer.
                FeatureLayer staticLayer = new FeatureLayer(serviceTable)
                {
                    RenderingMode = FeatureRenderingMode.Static
                };
                staticScene.OperationalLayers.Add(staticLayer);

                // Create and add the dynamic layer.
                FeatureLayer dynamicLayer = (FeatureLayer) staticLayer.Clone();
                dynamicLayer.RenderingMode = FeatureRenderingMode.Dynamic;
                dynamicScene.OperationalLayers.Add(dynamicLayer);
            }

            // Add the scenes to the scene views.
            _staticSceneView.Scene = staticScene;
            _dynamicSceneView.Scene = dynamicScene;
        }

        private void _zoomButton_TouchUpInside(object sender, EventArgs e)
        {
            // Zoom out if zoomed.
            if (_zoomed)
            {
                _staticSceneView.SetViewpointCameraAsync(_zoomedOutCamera, new TimeSpan(0, 0, 5));
                _dynamicSceneView.SetViewpointCameraAsync(_zoomedOutCamera, new TimeSpan(0, 0, 5));
            }
            else // Zoom in otherwise.
            {
                _staticSceneView.SetViewpointCameraAsync(_zoomedInCamera, new TimeSpan(0, 0, 5));
                _dynamicSceneView.SetViewpointCameraAsync(_zoomedInCamera, new TimeSpan(0, 0, 5));
            }

            // Toggle zoom state.
            _zoomed = !_zoomed;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = ApplicationTheme.BackgroundColor};

            _staticSceneView = new SceneView();
            _staticSceneView.TranslatesAutoresizingMaskIntoConstraints = false;
            _dynamicSceneView = new SceneView();
            _dynamicSceneView.TranslatesAutoresizingMaskIntoConstraints = false;

            _stackView = new UIStackView(new UIView[] {_staticSceneView, _dynamicSceneView});
            _stackView.TranslatesAutoresizingMaskIntoConstraints = false;
            _stackView.Distribution = UIStackViewDistribution.FillEqually;

            _zoomButton = new UIBarButtonItem();
            _zoomButton.Title = "Zoom";

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _zoomButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
            };

            UILabel staticLabel = new UILabel
            {
                Text = "Static",
                BackgroundColor = UIColor.FromWhiteAlpha(0f, .6f),
                TextColor = UIColor.White,
                TextAlignment = UITextAlignment.Center,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            UILabel dynamicLabel = new UILabel
            {
                Text = "Dynamic",
                BackgroundColor = UIColor.FromWhiteAlpha(0f, .6f),
                TextColor = UIColor.White,
                TextAlignment = UITextAlignment.Center,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Add the views.
            View.AddSubviews(_stackView, toolbar, staticLabel, dynamicLabel);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _stackView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _stackView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),
                _stackView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _stackView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),

                staticLabel.TopAnchor.ConstraintEqualTo(_staticSceneView.TopAnchor),
                staticLabel.HeightAnchor.ConstraintEqualTo(40),
                staticLabel.LeadingAnchor.ConstraintEqualTo(_staticSceneView.LeadingAnchor),
                staticLabel.TrailingAnchor.ConstraintEqualTo(_staticSceneView.TrailingAnchor),

                dynamicLabel.TopAnchor.ConstraintEqualTo(_dynamicSceneView.TopAnchor),
                dynamicLabel.HeightAnchor.ConstraintEqualTo(40),
                dynamicLabel.LeadingAnchor.ConstraintEqualTo(_dynamicSceneView.LeadingAnchor),
                dynamicLabel.TrailingAnchor.ConstraintEqualTo(_dynamicSceneView.TrailingAnchor)
            });
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);
            if (View.TraitCollection.VerticalSizeClass == UIUserInterfaceSizeClass.Compact)
            {
                _stackView.Axis = UILayoutConstraintAxis.Horizontal;
            }
            else
            {
                _stackView.Axis = UILayoutConstraintAxis.Vertical;
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _zoomButton.Clicked += _zoomButton_TouchUpInside;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _zoomButton.Clicked -= _zoomButton_TouchUpInside;
        }
    }
}