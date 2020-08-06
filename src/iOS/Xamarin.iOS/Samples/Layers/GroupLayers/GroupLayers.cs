// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Linq;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.GroupLayers
{
    [Register("GroupLayers")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Group layers",
        category: "Layers",
        description: "Group a collection of layers together and toggle their visibility as a group.",
        instructions: "The layers in the map will be displayed in a table of contents. Toggle the checkbox next to a layer's name to change its visibility. Turning a group layer's visibility off will override the visibility of its child layers.",
        tags: new[] { "group layer", "layers" })]
    public class GroupLayers : UIViewController
    {
        // Hold references to UI controls.
        private SceneView _mySceneView;
        private UIScrollView _layerContainerView;
        private UIStackView _layerStackView;
        private NSLayoutConstraint[] _verticalConstraints;
        private NSLayoutConstraint[] _horizontalConstraints;

        public GroupLayers()
        {
            Title = "Group layers";
        }

        private async void Initialize()
        {
            // Create the layers.
            ArcGISSceneLayer devOne = new ArcGISSceneLayer(new Uri("https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/DevA_Trees/SceneServer"));
            FeatureLayer devTwo = new FeatureLayer(new Uri("https://services.arcgis.com/P3ePLMYs2RVChkJx/arcgis/rest/services/DevA_Pathways/FeatureServer/1"));
            ArcGISSceneLayer devThree = new ArcGISSceneLayer(new Uri("https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/DevB_BuildingShells/SceneServer"));
            ArcGISSceneLayer nonDevOne = new ArcGISSceneLayer(new Uri("https://tiles.arcgis.com/tiles/P3ePLMYs2RVChkJx/arcgis/rest/services/DevA_BuildingShells/SceneServer"));
            FeatureLayer nonDevTwo = new FeatureLayer(new Uri("https://services.arcgis.com/P3ePLMYs2RVChkJx/arcgis/rest/services/DevelopmentProjectArea/FeatureServer/0"));

            // Create the group layer and add sublayers.
            GroupLayer gLayer = new GroupLayer();
            gLayer.Name = "Group: Dev A";
            gLayer.Layers.Add(devOne);
            gLayer.Layers.Add(devTwo);
            gLayer.Layers.Add(devThree);

            // Create the scene with a basemap.
            _mySceneView.Scene = new Scene(Basemap.CreateImagery());

            // Add the top-level layers to the scene.
            _mySceneView.Scene.OperationalLayers.Add(gLayer);
            _mySceneView.Scene.OperationalLayers.Add(nonDevOne);
            _mySceneView.Scene.OperationalLayers.Add(nonDevTwo);

            // Wait for all of the layers in the group layer to load.
            await Task.WhenAll(gLayer.Layers.ToList().Select(m => m.LoadAsync()).ToList());

            // Zoom to the extent of the group layer.
            _mySceneView.SetViewpoint(new Viewpoint(gLayer.FullExtent));

            // Add the layer list to the UI.
            foreach (Layer layer in _mySceneView.Scene.OperationalLayers)
            {
                AddLayersToUI(layer);
            }
        }

        private async void AddLayersToUI(Layer layer, int nestLevel = 0)
        {
            // Wait for the layers to load - ensures that the UI will be up-to-date.
            await layer.LoadAsync();

            // Add a row for the current layer.
            _layerStackView.AddArrangedSubview(ViewForLayer(layer, nestLevel));

            // Add rows for any children of this layer if it is a group layer.
            if (layer is GroupLayer layerGroup)
            {
                foreach (Layer child in layerGroup.Layers)
                {
                    AddLayersToUI(child, nestLevel + 1);
                }
            }
        }

        private UIView ViewForLayer(Layer layer, int nestLevel = 0)
        {
            // Create the view that holds the row.
            UIView rowContainer = new UIView();
            rowContainer.TranslatesAutoresizingMaskIntoConstraints = false;

            // Create and configure the visibility toggle.
            UISwitch toggleSwitch = new UISwitch();
            toggleSwitch.TranslatesAutoresizingMaskIntoConstraints = false;
            toggleSwitch.On = true;
            toggleSwitch.ValueChanged += (sender, args) => layer.IsVisible = !layer.IsVisible;

            // Create and configure the label.
            UILabel nameLabel = new UILabel();
            nameLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            nameLabel.Text = layer.Name;

            // Add the views to the row.
            rowContainer.AddSubviews(toggleSwitch, nameLabel);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                // Constant implements nesting/hierarchy display.
                toggleSwitch.LeadingAnchor.ConstraintEqualTo(rowContainer.LeadingAnchor, 8 + nestLevel * toggleSwitch.IntrinsicContentSize.Width),
                toggleSwitch.CenterYAnchor.ConstraintEqualTo(rowContainer.CenterYAnchor),
                nameLabel.LeadingAnchor.ConstraintEqualTo(toggleSwitch.TrailingAnchor, 8),
                nameLabel.TrailingAnchor.ConstraintEqualTo(rowContainer.TrailingAnchor),
                nameLabel.CenterYAnchor.ConstraintEqualTo(rowContainer.CenterYAnchor),
                rowContainer.HeightAnchor.ConstraintEqualTo(40)
            });

            return rowContainer;
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = ApplicationTheme.BackgroundColor};

            _mySceneView = new SceneView();
            _mySceneView.TranslatesAutoresizingMaskIntoConstraints = false;

            _layerContainerView = new UIScrollView();
            _layerContainerView.TranslatesAutoresizingMaskIntoConstraints = false;
            _layerContainerView.BackgroundColor = ApplicationTheme.BackgroundColor;

            _layerStackView = new UIStackView();
            _layerStackView.TranslatesAutoresizingMaskIntoConstraints = false;
            _layerStackView.Axis = UILayoutConstraintAxis.Vertical;
            _layerContainerView.AddSubview(_layerStackView);

            // Add the views.
            View.AddSubviews(_mySceneView, _layerContainerView);

            // Lay out the views.
            _verticalConstraints = new[]
            {
                _layerContainerView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _layerContainerView.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor),
                _layerContainerView.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor),
                _layerContainerView.HeightAnchor.ConstraintEqualTo(6 * 40),
                _mySceneView.TopAnchor.ConstraintEqualTo(_layerContainerView.BottomAnchor),
                _mySceneView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _mySceneView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _mySceneView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _layerStackView.WidthAnchor.ConstraintEqualTo(_layerContainerView.WidthAnchor)
            };
            _horizontalConstraints = new[]
            {
                _layerContainerView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _layerContainerView.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor),
                _layerContainerView.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                _layerContainerView.TrailingAnchor.ConstraintEqualTo(_mySceneView.LeadingAnchor),
                _layerContainerView.TrailingAnchor.ConstraintEqualTo(View.CenterXAnchor),
                _mySceneView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _mySceneView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _mySceneView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _layerStackView.WidthAnchor.ConstraintEqualTo(_layerContainerView.WidthAnchor)
            };

            ApplyConstraints();
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);
            ApplyConstraints();
        }

        private void ApplyConstraints()
        {
            NSLayoutConstraint.DeactivateConstraints(_horizontalConstraints);
            NSLayoutConstraint.DeactivateConstraints(_verticalConstraints);

            if (View.TraitCollection.VerticalSizeClass == UIUserInterfaceSizeClass.Compact)
            {
                NSLayoutConstraint.ActivateConstraints(_horizontalConstraints);
            }
            else
            {
                NSLayoutConstraint.ActivateConstraints(_verticalConstraints);
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }
    }
}