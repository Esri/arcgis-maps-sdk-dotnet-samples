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
using System;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.ServiceFeatureTableNoCache
{
    public partial class ServiceFeatureTableNoCache : ContentPage
    {
        public ServiceFeatureTableNoCache()
        {
            InitializeComponent ();

            Title = "Service feature table (no cache)";
            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateTopographic());

            // Create and set initial map area
            Envelope initialLocation = new Envelope(
                -1.30758164047166E7, 4014771.46954516, -1.30730056797177E7, 4016869.78617381,
                SpatialReferences.WebMercator);
            myMap.InitialViewpoint = new Viewpoint(initialLocation);

            // Create uri to the used feature service
            var serviceUri = new Uri(
               "http://sampleserver6.arcgisonline.com/arcgis/rest/services/PoolPermits/FeatureServer/0");

            // Create feature table for the pools feature service
            ServiceFeatureTable poolsFeatureTable = new ServiceFeatureTable(serviceUri);

            // Define the request mode
            poolsFeatureTable.FeatureRequestMode = FeatureRequestMode.OnInteractionNoCache;

            // Create FeatureLayer that uses the created table
            FeatureLayer poolsFeatureLayer = new FeatureLayer(poolsFeatureTable);

            // Add created layer to the map
            myMap.OperationalLayers.Add(poolsFeatureLayer);

            // Assign the map to the MapView
            MyMapView.Map = myMap;
        }
    }
}
