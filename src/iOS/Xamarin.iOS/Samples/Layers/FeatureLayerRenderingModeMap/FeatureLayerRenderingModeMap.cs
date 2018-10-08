// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreGraphics;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.FeatureLayerRenderingModeMap
{
    [Register("FeatureLayerRenderingModeMap")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Feature layer rendering mode (Map)",
        "Layers",
        "This sample demonstrates how to use load settings to set preferred rendering mode for feature layers, specifically static or dynamic rendering modes.",
        "")]
    public class FeatureLayerRenderingModeMap : UIViewController
    {
        // Hold references to the UI controls.
        private MapView _myMapViewTop;
        private MapView _myMapViewBottom;
        private UIButton _zoomButton;
        private UILabel _staticLabel;
        private UILabel _dynamicLabel;

        // Hold references to the two views.
        private Viewpoint _zoomOutPoint;
        private Viewpoint _zoomInPoint;

        public FeatureLayerRenderingModeMap()
        {
            Title = "Feature layer rendering mode (Map)";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat centerLine = (View.Bounds.Height - topMargin) / 2;
                nfloat buttonWidth = 150;
                nfloat startingLeft = View.Bounds.Width / 2 - buttonWidth / 2;

                // Reposition the views.
                _myMapViewTop.Frame = new CGRect(0, topMargin, View.Bounds.Width, centerLine);
                _myMapViewBottom.Frame = new CGRect(0, centerLine + topMargin, View.Bounds.Width, View.Bounds.Height - topMargin - centerLine);
                _staticLabel.Frame = new CGRect(10, topMargin + 5, View.Bounds.Width / 2, 30);
                _dynamicLabel.Frame = new CGRect(10, centerLine + topMargin + 30, View.Bounds.Width / 2, 30);
                _zoomButton.Frame = new CGRect(startingLeft, centerLine + topMargin - 15, buttonWidth, 30);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private void Initialize()
        {
            // Viewpoint locations for map view to zoom in and out to.
            _zoomOutPoint = new Viewpoint(new MapPoint(-118.37, 34.46, SpatialReferences.Wgs84), 650000, 0);
            _zoomInPoint = new Viewpoint(new MapPoint(-118.45, 34.395, SpatialReferences.Wgs84), 50000, 90);

            // Configure the maps.
            _myMapViewBottom.Map = new Map();
            _myMapViewTop.Map = new Map();

            // Create service feature table using a point, polyline, and polygon service.
            ServiceFeatureTable pointServiceFeatureTable = new ServiceFeatureTable(new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Energy/Geology/FeatureServer/0"));
            ServiceFeatureTable polylineServiceFeatureTable = new ServiceFeatureTable(new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Energy/Geology/FeatureServer/8"));
            ServiceFeatureTable polygonServiceFeatureTable = new ServiceFeatureTable(new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Energy/Geology/FeatureServer/9"));

            // Create feature layers from service feature tables.
            List<FeatureLayer> featureLayers = new List<FeatureLayer>
            {
                new FeatureLayer(polygonServiceFeatureTable),
                new FeatureLayer(polylineServiceFeatureTable),
                new FeatureLayer(pointServiceFeatureTable)
            };

            // Add each layer to the map as a static layer and a dynamic layer.
            foreach (FeatureLayer layer in featureLayers)
            {
                // Add the static layer to the top map view.
                layer.RenderingMode = FeatureRenderingMode.Static;
                _myMapViewTop.Map.OperationalLayers.Add(layer);

                // Add the dynamic layer to the bottom map view.
                FeatureLayer dynamicLayer = (FeatureLayer) layer.Clone();
                dynamicLayer.RenderingMode = FeatureRenderingMode.Dynamic;
                _myMapViewBottom.Map.OperationalLayers.Add(dynamicLayer);
            }

            // Set the view point of both MapViews.
            _myMapViewTop.SetViewpoint(_zoomOutPoint);
            _myMapViewBottom.SetViewpoint(_zoomOutPoint);
        }

        private void CreateLayout()
        {
            // Create and hold a reference to the used MapView.
            _myMapViewTop = new MapView();
            _myMapViewBottom = new MapView();

            // Hide the top attribution bar because there is already another one visible.
            _myMapViewTop.IsAttributionTextVisible = false;

            // Add a button at the bottom to show webmap choices.
            _zoomButton = new UIButton(UIButtonType.RoundedRect)
            {
                BackgroundColor = View.TintColor
            };
            _zoomButton.Layer.CornerRadius = 5;
            _zoomButton.SetTitle("Animated zoom", UIControlState.Normal);
            _zoomButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            _zoomButton.TouchUpInside += OnZoomClick;

            // Create and add the labels.
            _staticLabel = new UILabel
            {
                Text = "Static",
                TextColor = UIColor.Black,
                ShadowColor = UIColor.White
            };

            _dynamicLabel = new UILabel
            {
                Text = "Dynamic",
                TextColor = UIColor.Black,
                ShadowColor = UIColor.White
            };

            // Add MapView to the page.
            View.AddSubviews(_myMapViewTop, _myMapViewBottom, _zoomButton, _staticLabel, _dynamicLabel);

            // Set the view background.
            View.BackgroundColor = UIColor.White;
        }

        private async void OnZoomClick(object sender, EventArgs e)
        {
            // Initiate task to zoom both map views in.  
            Task t1 = _myMapViewTop.SetViewpointAsync(_zoomInPoint, TimeSpan.FromSeconds(5));
            Task t2 = _myMapViewBottom.SetViewpointAsync(_zoomInPoint, TimeSpan.FromSeconds(5));
            await Task.WhenAll(t1, t2);

            // Delay start of next set of zoom tasks.
            await Task.Delay(2000);

            // Initiate task to zoom both map views out. 
            Task t3 = _myMapViewTop.SetViewpointAsync(_zoomOutPoint, TimeSpan.FromSeconds(5));
            Task t4 = _myMapViewBottom.SetViewpointAsync(_zoomOutPoint, TimeSpan.FromSeconds(5));
            await Task.WhenAll(t3, t4);
        }
    }
}