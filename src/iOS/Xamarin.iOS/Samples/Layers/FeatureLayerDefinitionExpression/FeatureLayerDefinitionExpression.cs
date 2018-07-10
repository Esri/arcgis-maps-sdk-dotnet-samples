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
        "Feature layer definition expression",
        "Layers",
        "This sample demonstrates how to apply definition expression to a feature layer for filtering features. It also shows how to reset the definition expression.",
        "")]
    public class FeatureLayerDefinitionExpression : UIViewController
    {
        // Create and hold a reference to the MapView.
        private MapView _myMapView = new MapView();

        // Create and hold reference to the feature layer.
        private FeatureLayer _featureLayer;

        public FeatureLayerDefinitionExpression()
        {
            Title = "Feature layer definition expression";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CreateLayout();
            Initialize();
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
            NavigationController.ToolbarHidden = true;
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;

                // Reposition controls.
                _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, 0, 0);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private async void Initialize()
        {
            // Create new Map with basemap.
            Map map = new Map(Basemap.CreateTopographic());

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

        private void CreateLayout()
        {
            // Create MapView.
            _myMapView = new MapView();

            // Create a button to reset the renderer.
            UIBarButtonItem resetButton = new UIBarButtonItem
            {
                Title = "Reset",
                Style = UIBarButtonItemStyle.Plain
            };
            resetButton.Clicked += OnResetButtonClicked;

            // Create a button to apply new renderer.
            UIBarButtonItem expressionButton = new UIBarButtonItem
            {
                Title = "Apply Expression",
                Style = UIBarButtonItemStyle.Plain
            };
            expressionButton.Clicked += OnApplyExpressionClicked;

            // Add the buttons to the toolbar.
            SetToolbarItems(new[] {resetButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace, null), expressionButton}, false);

            // Show the toolbar.
            NavigationController.ToolbarHidden = false;

            // Add MapView to the page.
            View.AddSubviews(_myMapView);
        }
    }
}