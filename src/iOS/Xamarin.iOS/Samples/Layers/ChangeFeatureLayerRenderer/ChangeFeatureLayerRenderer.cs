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
        "Change feature layer renderer",
        "Layers",
        "This sample demonstrates how to change renderer for a feature layer. It also shows how to reset the renderer back to the default.",
        "")]
    public class ChangeFeatureLayerRenderer : UIViewController
    {
        // Hold a reference to the MapView.
        private MapView _myMapView;

        // Create and hold reference to the feature layer.
        private FeatureLayer _featureLayer;

        public ChangeFeatureLayerRenderer()
        {
            Title = "Change feature layer renderer";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap.
            Map map = new Map(Basemap.CreateTopographic());

            // Create and set initial map area.
            Envelope initialLocation = new Envelope(-1.30758164047166E7, 4014771.46954516, -1.30730056797177E7,
                4016869.78617381, SpatialReferences.WebMercator);

            // Set the initial viewpoint for map.
            map.InitialViewpoint = new Viewpoint(initialLocation);

            // Provide used Map to the MapView.
            _myMapView.Map = map;

            // Create URI to the used feature service.
            Uri serviceUri =
                new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/PoolPermits/FeatureServer/0");

            // Initialize feature table using a URL to a feature service.
            ServiceFeatureTable featureTable = new ServiceFeatureTable(serviceUri);

            // Initialize a new feature layer based on the feature table.
            _featureLayer = new FeatureLayer(featureTable);
            map.OperationalLayers.Add(_featureLayer);
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

        public override void LoadView()
        {
            // Create the MapView.
            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            // Create the toolbar.
            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;

            // Add the views to the layout.
            View = new UIView {BackgroundColor = UIColor.White};
            View.AddSubviews(_myMapView, toolbar);

            // Add the button.
            toolbar.Items = new[]
            {
                new UIBarButtonItem("Reset", UIBarButtonItemStyle.Plain, OnResetButtonClicked),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem("Override renderer", UIBarButtonItemStyle.Plain, OnOverrideButtonClicked),
            };

            // Set up constraints.
            _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _myMapView.TopAnchor.ConstraintEqualTo(View.TopAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;

            toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor).Active = true;
            toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
        }
    }
}