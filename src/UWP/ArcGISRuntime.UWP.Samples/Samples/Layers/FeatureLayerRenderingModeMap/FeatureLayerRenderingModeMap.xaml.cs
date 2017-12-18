// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using System;
using System.Threading.Tasks;

namespace ArcGISRuntime.UWP.Samples.FeatureLayerRenderingModeMap
{
    public partial class FeatureLayerRenderingModeMap
    {
        // Viewpoint locations for map view to zoom in and out to.
        Viewpoint _zoomOutPoint = new Viewpoint(new MapPoint(-118.37, 34.46, SpatialReferences.Wgs84), 650000, 0);
        Viewpoint _zoomInPoint = new Viewpoint(new MapPoint(-118.45, 34.395, SpatialReferences.Wgs84), 50000, 90);

        public FeatureLayerRenderingModeMap()
        {
            InitializeComponent();

            // Setup the control references and execute initialization. 
            Initialize();
        }

        private void Initialize()
        {
            // Set the top map to render all features in static rendering mode
            MyMapViewTop.Map.LoadSettings.PreferredPointFeatureRenderingMode = FeatureRenderingMode.Static;
            MyMapViewTop.Map.LoadSettings.PreferredPolylineFeatureRenderingMode = FeatureRenderingMode.Static;
            MyMapViewTop.Map.LoadSettings.PreferredPolygonFeatureRenderingMode = FeatureRenderingMode.Static;

            // Set the bottom map to render all features in dynamic rendering mode
            MyMapViewBottom.Map.LoadSettings.PreferredPointFeatureRenderingMode = FeatureRenderingMode.Dynamic;
            MyMapViewBottom.Map.LoadSettings.PreferredPolylineFeatureRenderingMode = FeatureRenderingMode.Dynamic;
            MyMapViewBottom.Map.LoadSettings.PreferredPolygonFeatureRenderingMode = FeatureRenderingMode.Dynamic;

            // Create service feature table using a point, polyline, and polygon service.
            ServiceFeatureTable pointServiceFeatureTable = new ServiceFeatureTable(new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Energy/Geology/FeatureServer/0"));
            ServiceFeatureTable polylineServiceFeatureTable = new ServiceFeatureTable(new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Energy/Geology/FeatureServer/8"));
            ServiceFeatureTable polygonServiceFeatureTable = new ServiceFeatureTable(new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Energy/Geology/FeatureServer/9"));

            // Create feature layer from service feature tables.
            FeatureLayer pointFeatureLayer = new FeatureLayer(pointServiceFeatureTable);
            FeatureLayer polylineFeatureLayer = new FeatureLayer(polylineServiceFeatureTable);
            FeatureLayer polygonFeatureLayer = new FeatureLayer(polygonServiceFeatureTable);

            // Add each layer to top map.
            MyMapViewTop.Map.OperationalLayers.Add(pointFeatureLayer.Clone());
            MyMapViewTop.Map.OperationalLayers.Add(polylineFeatureLayer.Clone());
            MyMapViewTop.Map.OperationalLayers.Add(polygonFeatureLayer.Clone());

            // Add each layer to top map.
            MyMapViewBottom.Map.OperationalLayers.Add(pointFeatureLayer);
            MyMapViewBottom.Map.OperationalLayers.Add(polylineFeatureLayer);
            MyMapViewBottom.Map.OperationalLayers.Add(polygonFeatureLayer);

            // Set the view point of both MapViews.
            MyMapViewTop.SetViewpoint(_zoomOutPoint);
            MyMapViewBottom.SetViewpoint(_zoomOutPoint);
        }

        private async void OnZoomClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Initiate task to zoom both map views in.  
            Task t1 = MyMapViewTop.SetViewpointAsync(_zoomInPoint, TimeSpan.FromSeconds(5));
            Task t2 = MyMapViewBottom.SetViewpointAsync(_zoomInPoint, TimeSpan.FromSeconds(5));
            await Task.WhenAll(t1, t2);

            // Delay start of next set of zoom tasks.
            await Task.Delay(2000);

            // Initiate task to zoom both map views out. 
            Task t3 = MyMapViewTop.SetViewpointAsync(_zoomOutPoint, TimeSpan.FromSeconds(5));
            Task t4 = MyMapViewBottom.SetViewpointAsync(_zoomOutPoint, TimeSpan.FromSeconds(5));
            await Task.WhenAll(t3, t4);

        }
    }
}