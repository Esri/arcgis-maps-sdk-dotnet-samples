// Copyright 2021 Esri.
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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.FeatureLayerRenderingModeMap
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Feature layer rendering mode (map)",
        category: "Layers",
        description: "Render features statically or dynamically by setting the feature layer rendering mode.",
        instructions: "Click the button to trigger the same zoom animation on both static and dynamic maps.",
        tags: new[] { "dynamic", "feature layer", "features", "rendering", "static" })]
    public partial class FeatureLayerRenderingModeMap
    {
        // Viewpoint locations for map view to zoom in and out to.
        private Viewpoint _zoomOutPoint = new Viewpoint(new MapPoint(-118.37, 34.46, SpatialReferences.Wgs84), 650000, 0);
        private Viewpoint _zoomInPoint = new Viewpoint(new MapPoint(-118.45, 34.395, SpatialReferences.Wgs84), 50000, 90);

        public FeatureLayerRenderingModeMap()
        {
            InitializeComponent();

            // Setup the control references and execute initialization.
            Initialize();
        }

        private void Initialize()
        {
            // Set the initial viewpoint on the maps.
            MyStaticMapView.Map = new Map
            {
                InitialViewpoint = _zoomOutPoint
            };

            MyDynamicMapView.Map = new Map
            {
                InitialViewpoint = _zoomOutPoint
            };

            // Create service feature table using a point, polyline, and polygon service.
            ServiceFeatureTable pointServiceFeatureTable = new ServiceFeatureTable(new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Energy/Geology/FeatureServer/0"));
            ServiceFeatureTable polylineServiceFeatureTable = new ServiceFeatureTable(new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Energy/Geology/FeatureServer/8"));
            ServiceFeatureTable polygonServiceFeatureTable = new ServiceFeatureTable(new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Energy/Geology/FeatureServer/9"));

            // Create feature layers from service feature tables
            List<FeatureLayer> featureLayers = new List<FeatureLayer>
            {
                new FeatureLayer(polygonServiceFeatureTable),
                new FeatureLayer(polylineServiceFeatureTable),
                new FeatureLayer(pointServiceFeatureTable)
            };

            // Add each layer to the map as a static layer and a dynamic layer
            foreach (FeatureLayer layer in featureLayers)
            {
                // Add the static layer to the top map view
                layer.RenderingMode = FeatureRenderingMode.Static;
                MyStaticMapView.Map.OperationalLayers.Add(layer);

                // Add the dynamic layer to the bottom map view
                if (layer.FeatureTable is ServiceFeatureTable table)
                {
                    FeatureLayer dynamicLayer = new FeatureLayer(new ServiceFeatureTable(table.Source));
                    dynamicLayer.RenderingMode = FeatureRenderingMode.Dynamic;
                    MyDynamicMapView.Map.OperationalLayers.Add(dynamicLayer);
                }
            }

            // Set the view point of both MapViews.
            MyStaticMapView.SetViewpoint(_zoomOutPoint);
            MyDynamicMapView.SetViewpoint(_zoomOutPoint);
        }

        private async void OnZoomClick(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                // Initiate task to zoom both map views in.
                Task t1 = MyStaticMapView.SetViewpointAsync(_zoomInPoint, TimeSpan.FromSeconds(5));
                Task t2 = MyDynamicMapView.SetViewpointAsync(_zoomInPoint, TimeSpan.FromSeconds(5));
                await Task.WhenAll(t1, t2);

                // Delay start of next set of zoom tasks.
                await Task.Delay(2000);

                // Initiate task to zoom both map views out.
                Task t3 = MyStaticMapView.SetViewpointAsync(_zoomOutPoint, TimeSpan.FromSeconds(5));
                Task t4 = MyDynamicMapView.SetViewpointAsync(_zoomOutPoint, TimeSpan.FromSeconds(5));
                await Task.WhenAll(t3, t4);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
        }
    }
}