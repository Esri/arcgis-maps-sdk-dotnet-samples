﻿// Copyright 2017 Esri.
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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.FeatureLayerRenderingModeMap
{
    [Register("FeatureLayerRenderingModeMap")]
    public class FeatureLayerRenderingModeMap : UIViewController
    {
        private MapView _myMapViewTop;
        private MapView _myMapViewBottom;

        private UIButton _zoomButton;

        private Viewpoint _zoomOutPoint;
        private Viewpoint _zoomInPoint;

        public FeatureLayerRenderingModeMap()
        {
            this.Title = "Feature Layer Rendering Mode (Map)";
        }
        
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the layout
            CreateLayout();

            // Initialize the app
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            // Setup the visual frame for the MapViews
            _myMapViewTop.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height/2);
            _myMapViewBottom.Frame = new CoreGraphics.CGRect(0, View.Bounds.Height / 2, View.Bounds.Width, View.Bounds.Height/2);

            base.ViewDidLayoutSubviews();
        }

        private  void Initialize()
        {
            // Viewpoint locations for map view to zoom in and out to.
            _zoomOutPoint = new Viewpoint(new MapPoint(-118.37, 34.46, SpatialReferences.Wgs84), 650000, 0);
            _zoomInPoint = new Viewpoint(new MapPoint(-118.45, 34.395, SpatialReferences.Wgs84), 50000, 90);

            // Set the Map property of both of the MapViews
            _myMapViewTop.Map = new Map();
            _myMapViewBottom.Map = new Map();

            // Set the top map to render all features in static rendering mode
            _myMapViewTop.Map.LoadSettings.PreferredPointFeatureRenderingMode = FeatureRenderingMode.Static;
            _myMapViewTop.Map.LoadSettings.PreferredPolylineFeatureRenderingMode = FeatureRenderingMode.Static;
            _myMapViewTop.Map.LoadSettings.PreferredPolygonFeatureRenderingMode = FeatureRenderingMode.Static;

            // Set the bottom map to render all features in dynamic rendering mode
            _myMapViewBottom.Map.LoadSettings.PreferredPointFeatureRenderingMode = FeatureRenderingMode.Dynamic;
            _myMapViewBottom.Map.LoadSettings.PreferredPolylineFeatureRenderingMode = FeatureRenderingMode.Dynamic;
            _myMapViewBottom.Map.LoadSettings.PreferredPolygonFeatureRenderingMode = FeatureRenderingMode.Dynamic;

            // Create service feature table using a point, polyline, and polygon service.
            ServiceFeatureTable pointServiceFeatureTable = new ServiceFeatureTable(new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Energy/Geology/FeatureServer/0"));
            ServiceFeatureTable polylineServiceFeatureTable = new ServiceFeatureTable(new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Energy/Geology/FeatureServer/8"));
            ServiceFeatureTable polygonServiceFeatureTable = new ServiceFeatureTable(new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Energy/Geology/FeatureServer/9"));

            // Create feature layer from service feature tables.
            FeatureLayer pointFeatureLayer = new FeatureLayer(pointServiceFeatureTable);
            FeatureLayer polylineFeatureLayer = new FeatureLayer(polylineServiceFeatureTable);
            FeatureLayer polygonFeatureLayer = new FeatureLayer(polygonServiceFeatureTable);

            // Add each layer to top map.
            _myMapViewTop.Map.OperationalLayers.Add(pointFeatureLayer.Clone());
            _myMapViewTop.Map.OperationalLayers.Add(polylineFeatureLayer.Clone());
            _myMapViewTop.Map.OperationalLayers.Add(polygonFeatureLayer.Clone());

            // Add each layer to top map.
            _myMapViewBottom.Map.OperationalLayers.Add(pointFeatureLayer);
            _myMapViewBottom.Map.OperationalLayers.Add(polylineFeatureLayer);
            _myMapViewBottom.Map.OperationalLayers.Add(polygonFeatureLayer);

            // Set the view point of both MapViews.
            _myMapViewTop.SetViewpoint(_zoomOutPoint);
            _myMapViewBottom.SetViewpoint(_zoomOutPoint);
        }

        private void CreateLayout()
        {
            // Create and hold reference to the used MapView
            _myMapViewTop = new MapView();
            _myMapViewBottom = new MapView();

            // Add a button at the bottom to show webmap choices
            _zoomButton = new UIButton(UIButtonType.Custom)
            {
                Frame = new CoreGraphics.CGRect(
                    0, View.Bounds.Height - 40, View.Bounds.Width, 40),
                BackgroundColor = UIColor.White
            };

            // Create button to show map options
            _zoomButton.SetTitle("Zoom", UIControlState.Normal);
            _zoomButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            _zoomButton.TouchUpInside += OnZoomClick;

            // Add MapView to the page
            View.AddSubviews(_myMapViewTop, _myMapViewBottom, _zoomButton);
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