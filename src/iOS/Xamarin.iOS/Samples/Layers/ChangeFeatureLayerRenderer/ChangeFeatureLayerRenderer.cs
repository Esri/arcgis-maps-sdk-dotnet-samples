// Copyright 2016 Esri.
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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ChangeFeatureLayerRenderer
{
    [Register("ChangeFeatureLayerRenderer")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Change Renderer",
        "Layers",
        "This sample demonstrates how to change renderer for a feature layer. It also shows how to reset the renderer back to the default.",
        "")]
    public class ChangeFeatureLayerRenderer : UIViewController
    {
        // Create and hold a reference to the MapView.
        private MapView _myMapView;

        //Create and hold reference to the feature layer.
        private FeatureLayer _featureLayer;

        public ChangeFeatureLayerRenderer()
        {
            Title = "Change feature layer renderer";
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

            // Create and set initial map area.
            Envelope initialLocation = new Envelope(-1.30758164047166E7, 4014771.46954516, -1.30730056797177E7, 4016869.78617381, SpatialReferences.WebMercator);

            // Set the initial viewpoint for map.
            map.InitialViewpoint = new Viewpoint(initialLocation);

            // Provide used Map to the MapView.
            _myMapView.Map = map;

            // Create URI to the used feature service.
            Uri serviceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/PoolPermits/FeatureServer/0");

            // Initialize feature table using a URL to a feature service.
            ServiceFeatureTable featureTable = new ServiceFeatureTable(serviceUri);

            // Initialize a new feature layer based on the feature table.
            _featureLayer = new FeatureLayer(featureTable);

            // Make sure that the feature layer gets loaded.
            await _featureLayer.LoadAsync();

            // Check for the load status. If the layer is loaded then add it to map.
            if (_featureLayer.LoadStatus == Esri.ArcGISRuntime.LoadStatus.Loaded)
            {
                // Add the feature layer to the map.
                map.OperationalLayers.Add(_featureLayer);
            }
        }

        private void OnOverrideButtonClicked(object sender, EventArgs e)
        {
            // Create a symbol to be used in the renderer.
            SimpleLineSymbol symbol = new SimpleLineSymbol
            {
                Color = Color.Blue,
                Width = 2,
                Style = SimpleLineSymbolStyle.Solid
            };

            // Create and apply a new renderer using the symbol just created.
            _featureLayer.Renderer = new SimpleRenderer(symbol);
        }

        private void OnResetButtonClicked(object sender, EventArgs e)
        {
            // Reset the renderer to default.
            _featureLayer.ResetRenderer();
        }

        private void CreateLayout()
        {
            // Create a MapView.
            _myMapView = new MapView();

            // Create a button to reset the renderer.
            UIBarButtonItem resetButton = new UIBarButtonItem
            {
                Title = "Reset",
                Style = UIBarButtonItemStyle.Plain
            };
            resetButton.Clicked += OnResetButtonClicked;

            // Create a button to apply new renderer.
            UIBarButtonItem overrideButton = new UIBarButtonItem
            {
                Title = "Override",
                Style = UIBarButtonItemStyle.Plain
            };
            overrideButton.Clicked += OnOverrideButtonClicked;

            // Add the buttons to the toolbar.
            SetToolbarItems(new[] {resetButton, new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace, null), overrideButton}, false);

            // Show the toolbar.
            NavigationController.ToolbarHidden = false;

            // Add MapView to the page.
            View.AddSubviews(_myMapView);
        }
    }
}