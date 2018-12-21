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
            Initialize();
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

        public override void LoadView()
        {
            View = new UIView {BackgroundColor = UIColor.White};
            
            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;
            
            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            
            View.AddSubviews(_myMapView, toolbar);

            toolbar.Items = new[]
            {
                new UIBarButtonItem("Reset", UIBarButtonItemStyle.Plain, OnResetButtonClicked),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem("Apply expression", UIBarButtonItemStyle.Plain, OnApplyExpressionClicked), 
            };
            
            _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor).Active = true;

            toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor).Active = true;
            toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
        }
    }
}