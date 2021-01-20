// Copyright 2016 Esri.
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

namespace ArcGISRuntime.Samples.FeatureLayerDefinitionExpression
{
    [Register("FeatureLayerDefinitionExpression")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Feature layer definition expression",
        category: "Layers",
        description: "Limit the features displayed on a map with a definition expression.",
        instructions: "Press the 'Apply Expression' button to limit the features requested from the feature layer to those specified by the SQL query definition expression. Tap the 'Reset Expression' button to remove the definition expression on the feature layer, which returns all the records.",
        tags: new[] { "SQL", "definition expression", "filter", "limit data", "query", "restrict data", "where clause" })]
    public class FeatureLayerDefinitionExpression : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIBarButtonItem _resetButton;
        private UIBarButtonItem _applyExpressionButton;

        // Create and hold reference to the feature layer.
        private FeatureLayer _featureLayer;

        public FeatureLayerDefinitionExpression()
        {
            Title = "Feature layer definition expression";
        }

        private async void Initialize()
        {
            // Create new Map with basemap.
            Map map = new Map(BasemapStyle.ArcGISTopographic);

            // Create a point the map should zoom to.
            MapPoint mapPoint = new MapPoint(-13630484, 4545415, SpatialReferences.WebMercator);

            // Set the initial viewpoint for map.
            map.InitialViewpoint = new Viewpoint(mapPoint, 90000);

            // Provide used Map to the MapView.
            _myMapView.Map = map;

            // Create the URI for the feature service.
            Uri featureServiceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/SF311/FeatureServer/0");

            // Initialize feature table using a URL to feature server.
            ServiceFeatureTable featureTable = new ServiceFeatureTable(featureServiceUri);

            // Initialize a new feature layer based on the feature table.
            _featureLayer = new FeatureLayer(featureTable);

            // Load the layer.
            await _featureLayer.LoadAsync();

            // Check for the load status. If the layer is loaded then add it to map.
            if (_featureLayer.LoadStatus == Esri.ArcGISRuntime.LoadStatus.Loaded)
            {
                // Add the feature layer to the map.
                map.OperationalLayers.Add(_featureLayer);
            }
        }

        private void OnApplyExpressionClicked(object sender, EventArgs e)
        {
            // Adding definition expression to show specific features only.
            _featureLayer.DefinitionExpression = "req_Type = 'Tree Maintenance or Damage'";
        }

        private void OnResetButtonClicked(object sender, EventArgs e)
        {
            // Reset the definition expression to see all features again.
            _featureLayer.DefinitionExpression = "";
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

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _resetButton = new UIBarButtonItem();
            _resetButton.Title = "Reset";

            _applyExpressionButton = new UIBarButtonItem();
            _applyExpressionButton.Title = "Apply expression";

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                _resetButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _applyExpressionButton
            };

            // Add the views.
            View.AddSubviews(_myMapView, toolbar);

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
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _resetButton.Clicked += OnResetButtonClicked;
            _applyExpressionButton.Clicked += OnApplyExpressionClicked;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _resetButton.Clicked -= OnResetButtonClicked;
            _applyExpressionButton.Clicked -= OnApplyExpressionClicked;
        }
    }
}