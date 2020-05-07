// Copyright 2019 Esri.
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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Drawing;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.DisplaySubtypeFeatureLayer
{
    [Register("DisplaySubtypeFeatureLayer")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Display subtype feature layer",
        "Layers",
        "Displays a composite layer of all the subtype values in a feature class.",
        "The sample loads with the sublayer visible on the map. Change the sublayer's visibiliy, renderer, and minimum scale using the on screen controls. Setting the minimum scale will change its value to that of the current map scale. Zoom in and out to see the sublayer become visible based on its new scale range.",
        "asset group", "feature layer", "labeling", "sublayer", "subtype", "symbology", "utility network", "visible scale range", "Featured")]
    public class DisplaySubtypeFeatureLayer : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIBarButtonItem _rendererButton;
        private UIBarButtonItem _minScaleButton;
        private UISegmentedControl _visibilityControl;
        private UILabel _mapScaleLabel;
        private UILabel _minScaleLabel;

        // Reference to a sublayer.
        private SubtypeSublayer _sublayer;

        // JSON for labeling features from the sublayer.
        private const string _labelJSON = "{ \"labelExpression\":\"[nominalvoltage]\",\"labelPlacement\":\"esriServerPointLabelPlacementAboveRight\",\"useCodedValues\":true,\"symbol\":{\"angle\":0,\"backgroundColor\":[0,0,0,0],\"borderLineColor\":[0,0,0,0],\"borderLineSize\":0,\"color\":[0,0,255,255],\"font\":{\"decoration\":\"none\",\"size\":10.5,\"style\":\"normal\",\"weight\":\"normal\"},\"haloColor\":[255,255,255,255],\"haloSize\":2,\"horizontalAlignment\":\"center\",\"kerning\":false,\"type\":\"esriTS\",\"verticalAlignment\":\"middle\",\"xoffset\":0,\"yoffset\":0}}";

        // Renderers for the sublayer.
        private Renderer _defaultRenderer;
        private Renderer _customRenderer;

        public DisplaySubtypeFeatureLayer()
        {
            Title = "Display subtype feature layer";
        }

        private async void Initialize()
        {
            try
            {
                // Starting viewpoint for the map view.
                Viewpoint _startingViewpoint = new Viewpoint(new Envelope(-9812691.11079696, 5128687.20710657, -9812377.9447607, 5128865.36767282, SpatialReferences.WebMercator));

                // Create the map.
                _myMapView.Map = new Map(Basemap.CreateStreetsNightVector()) { InitialViewpoint = _startingViewpoint };

                // NOTE: This layer supports any ArcGIS Feature Table that define subtypes.
                SubtypeFeatureLayer subtypeFeatureLayer = new SubtypeFeatureLayer(new ServiceFeatureTable(new Uri("https://sampleserver7.arcgisonline.com/arcgis/rest/services/UtilityNetwork/NapervilleElectric/FeatureServer/100")));
                _myMapView.Map.OperationalLayers.Add(subtypeFeatureLayer);

                // Select sublayer to control.
                await subtypeFeatureLayer.LoadAsync();

                // Select the sublayer of street lights by name.
                _sublayer = subtypeFeatureLayer.GetSublayerBySubtypeName("Street Light");

                // Set the label definitions using the JSON.
                _sublayer.LabelDefinitions.Add(LabelDefinition.FromJson(_labelJSON));

                // Enable labels for the sub layer.
                _sublayer.LabelsEnabled = true;

                // Get the default renderer for the sublayer.
                _defaultRenderer = _sublayer.Renderer.Clone();

                // Create a custom renderer for the sublayer.
                _customRenderer = new SimpleRenderer()
                {
                    Symbol = new SimpleMarkerSymbol()
                    {
                        Color = Color.Salmon,
                        Style = SimpleMarkerSymbolStyle.Diamond,
                        Size = 20,
                    }
                };

                // Update the UI for displaying the current map scale.
                _myMapView.ViewpointChanged += ViewpointChanged;
                _mapScaleLabel.Text = $"Current map scale: 1:{(int)_myMapView.MapScale}";
            }
            catch (Exception ex)
            {
                new UIAlertView(ex.GetType().Name, ex.Message, (IUIAlertViewDelegate)null, "OK", null).Show();
            }
        }

        private void ViewpointChanged(object sender, EventArgs e)
        {
            // Update the label showing the current map scale.
            _mapScaleLabel.Text = $"Current map scale: 1:{(int)_myMapView.MapScale}";
        }

        private void ChangeRenderer(object sender, EventArgs e)
        {
            // Check if the current renderer is the custom renderer.
            if (_sublayer.Renderer == _customRenderer)
            {
                _sublayer.Renderer = _defaultRenderer;
            }
            else
            {
                _sublayer.Renderer = _customRenderer;
            }
        }

        private void ChangeMinScale(object sender, EventArgs e)
        {
            // Set the minimum scale of the sublayer.
            // NOTE: You may also update Sublayer.MaxScale
            _sublayer.MinScale = _myMapView.MapScale;

            // Update the UI to show the current minimum.
            _minScaleLabel.Text = $"Current min scale: 1:{(int)_sublayer.MinScale}";
        }

        private void ChangeVisibility(object sender, EventArgs e)
        {
            // Set the visiblity of the sublayer.
            _sublayer.IsVisible = _visibilityControl.SelectedSegment == 0;
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView { BackgroundColor = UIColor.White };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _minScaleButton = new UIBarButtonItem();
            _minScaleButton.Title = "Set min scale";

            _rendererButton = new UIBarButtonItem();
            _rendererButton.Title = "Change rendering";

            _visibilityControl = new UISegmentedControl("Visible", "Not visible");
            _visibilityControl.SelectedSegment = 0;
            _visibilityControl.BackgroundColor = UIColor.White;
            _visibilityControl.TranslatesAutoresizingMaskIntoConstraints = false;

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _minScaleButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _rendererButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
            };

            _mapScaleLabel = new UILabel
            {
                Text = "Current map scale:",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Center,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f),
                TextColor = UIColor.White,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            _minScaleLabel = new UILabel
            {
                Text = "Sublayer min scale:",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Center,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f),
                TextColor = UIColor.White,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Add the views.
            View.AddSubviews(_myMapView, toolbar, _mapScaleLabel, _minScaleLabel, _visibilityControl);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),

                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                _mapScaleLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _mapScaleLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _mapScaleLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _mapScaleLabel.HeightAnchor.ConstraintEqualTo(25),

                _minScaleLabel.TopAnchor.ConstraintEqualTo(_mapScaleLabel.BottomAnchor),
                _minScaleLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _minScaleLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _minScaleLabel.HeightAnchor.ConstraintEqualTo(25),

                _visibilityControl.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor, -25),
                _visibilityControl.LeadingAnchor.ConstraintEqualTo(View.LayoutMarginsGuide.LeadingAnchor),
                _visibilityControl.TrailingAnchor.ConstraintEqualTo(View.LayoutMarginsGuide.TrailingAnchor),
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _rendererButton.Clicked += ChangeRenderer;
            _minScaleButton.Clicked += ChangeMinScale;
            _visibilityControl.ValueChanged += ChangeVisibility;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _rendererButton.Clicked -= ChangeRenderer;
            _minScaleButton.Clicked -= ChangeMinScale;
            _visibilityControl.ValueChanged -= ChangeVisibility;
            _myMapView.ViewpointChanged -= ViewpointChanged;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }
    }
}