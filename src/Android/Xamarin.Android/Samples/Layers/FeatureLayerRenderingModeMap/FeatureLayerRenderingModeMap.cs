// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArcGISRuntime.Samples.FeatureLayerRenderingModeMap
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.AndroidLayout("FeatureLayerRenderingModeMapLayout.axml")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Feature Layer Rendering Mode (Map)",
        "Layers",
        "This sample demonstrates how to use load settings to set preferred rendering mode for feature layers, specifically static or dynamic rendering modes.",
        "")]
    public class FeatureLayerRenderingModeMap : Activity
    {
        // Create variables to hold MapView instances  
        private MapView _myMapViewTop;
        private MapView _myMapViewBottom;

        // Viewpoint locations for map view to zoom in and out to.
        private Viewpoint _zoomOutPoint = new Viewpoint(new MapPoint(-118.37, 34.46, SpatialReferences.Wgs84), 650000, 0);
        private Viewpoint _zoomInPoint = new Viewpoint(new MapPoint(-118.45, 34.395, SpatialReferences.Wgs84), 50000, 90);

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Feature Layer Rendering Mode (Map)";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        private void CreateLayout()
        { 
            // Show the layout in the app
            SetContentView(Resource.Layout.FeatureLayerRenderingModeMapLayout);

            // Create the MapViews
            _myMapViewTop = FindViewById<MapView>(Resource.Id.Top_MyMapView);
            _myMapViewBottom = FindViewById<MapView>(Resource.Id.Bottom_MyMapView);

            // Create the Zoom button
            Button zoomButton = FindViewById<Button>(Resource.Id.ZoomButton);

            // Set Zoom method to run on button click 
            zoomButton.Click += OnZoomClick;
        }

        private void Initialize()
        {
            // Set the Map property of both MapViews
            _myMapViewTop.Map = new Map();
            _myMapViewBottom.Map = new Map();

            // Create service feature table using a point, polyline, and polygon service.
            ServiceFeatureTable pointServiceFeatureTable = new ServiceFeatureTable(new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Energy/Geology/FeatureServer/0"));
            ServiceFeatureTable polylineServiceFeatureTable = new ServiceFeatureTable(new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Energy/Geology/FeatureServer/8"));
            ServiceFeatureTable polygonServiceFeatureTable = new ServiceFeatureTable(new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Energy/Geology/FeatureServer/9"));

            // Create feature layers from service feature tables
            List<FeatureLayer> featureLayers = new List<FeatureLayer>
            {
                new FeatureLayer(pointServiceFeatureTable),
                new FeatureLayer(polylineServiceFeatureTable),
                new FeatureLayer(polygonServiceFeatureTable)
            };

            // Add each layer to the map as a static layer and a dynamic layer
            foreach (FeatureLayer layer in featureLayers)
            {
                // Add the static layer to the top map view
                layer.RenderingMode = FeatureRenderingMode.Static;
                _myMapViewTop.Map.OperationalLayers.Add(layer);

                // Add the dynamic layer to the bottom map view
                FeatureLayer dynamicLayer = (FeatureLayer)layer.Clone();
                dynamicLayer.RenderingMode = FeatureRenderingMode.Dynamic;
                _myMapViewBottom.Map.OperationalLayers.Add(dynamicLayer);
            }

            // Set the view point of both MapViews.
            _myMapViewTop.SetViewpoint(_zoomOutPoint);
            _myMapViewBottom.SetViewpoint(_zoomOutPoint);
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