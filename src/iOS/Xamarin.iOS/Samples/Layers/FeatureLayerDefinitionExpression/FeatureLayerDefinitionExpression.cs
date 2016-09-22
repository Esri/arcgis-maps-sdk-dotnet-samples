// Copyright 2016 Esri.
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
using Esri.ArcGISRuntime.UI;
using Foundation;
using System;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.FeatureLayerDefinitionExpression
{
    [Register("FeatureLayerDefinitionExpression")]
    public class FeatureLayerDefinitionExpression : UIViewController
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Create and hold reference to the feature layer
        private FeatureLayer _featureLayer;

        public FeatureLayerDefinitionExpression()
        {
            this.Title = "Feature layer definition expression";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateTopographic());

            // Create a mappoint the map should zoom to
            MapPoint mapPoint = new MapPoint(-13630484, 4545415, SpatialReferences.WebMercator);

            // Set the initial viewpoint for map
            myMap.InitialViewpoint = new Viewpoint(mapPoint, 90000);

            // Provide used Map to the MapView
            _myMapView.Map = myMap;

            // Create the uri for the feature service
            Uri featureServiceUri = new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/SF311/FeatureServer/0");

            // Initialize feature table using a url to feature server url
            ServiceFeatureTable featureTable = new ServiceFeatureTable(featureServiceUri);

            // Initialize a new feature layer based on the feature table
            _featureLayer = new FeatureLayer(featureTable);

            // Load the layer
            await _featureLayer.LoadAsync();

            // Check for the load status. If the layer is loaded then add it to map
            if (_featureLayer.LoadStatus == Esri.ArcGISRuntime.LoadStatus.Loaded)
            {
                //Add the feature layer to the map
                myMap.OperationalLayers.Add(_featureLayer);
            }
        }

        private void OnApplyExpressionClicked(object sender, EventArgs e)
        {
            // Adding definition expression to show specific features only
            _featureLayer.DefinitionExpression = "req_Type = 'Tree Maintenance or Damage'";
        }

        private void OnResetButtonClicked(object sender, EventArgs e)
        {
            // Reset the definition expression to see all features again
            _featureLayer.DefinitionExpression = "";
        }

        private void CreateLayout()
        {
            // Setup the visual frame for the MapView
            _myMapView = new MapView()
            {
                Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height)
            };

            // Create a button to reset the renderer
            var resetButton = new UIBarButtonItem() { Title = "Reset", Style = UIBarButtonItemStyle.Plain };
            resetButton.Clicked += OnResetButtonClicked;

            // Create a button to apply new renderer
            var expressionButton = new UIBarButtonItem() { Title = "Apply Expression", Style = UIBarButtonItemStyle.Plain };
            expressionButton.Clicked += OnApplyExpressionClicked;

            // Add the buttons to the toolbar
            SetToolbarItems(new UIBarButtonItem[] {resetButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace, null),
                expressionButton}, false);

            // Show the toolbar
            NavigationController.ToolbarHidden = false;

            // Add MapView to the page
            View.AddSubviews(_myMapView);
        }
    }
}