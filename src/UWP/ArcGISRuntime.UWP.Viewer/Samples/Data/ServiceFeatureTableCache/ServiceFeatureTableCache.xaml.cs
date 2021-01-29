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

namespace ArcGISRuntime.UWP.Samples.ServiceFeatureTableCache
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Service feature table (on interaction cache)",
        category: "Data",
        description: "Display a feature layer from a service using the **on interaction cache** feature request mode.",
        instructions: "Run the sample and pan and zoom around the map. With each interaction, features will be requested and stored in a local cache. Each subsequent interaction will display features from the cache and only request new features from the service.",
        tags: new[] { "cache", "feature request mode", "performance" })]
    public sealed partial class ServiceFeatureTableCache 
    {
        public ServiceFeatureTableCache()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(BasemapStyle.ArcGISTopographic);

            // Create and set initial map area
            Envelope initialLocation = new Envelope(
                -1.30758164047166E7, 4014771.46954516, -1.30730056797177E7, 4016869.78617381,
                SpatialReferences.WebMercator);
            myMap.InitialViewpoint = new Viewpoint(initialLocation);

            // Create uri to the used feature service
            Uri serviceUri = new Uri(
               "https://sampleserver6.arcgisonline.com/arcgis/rest/services/PoolPermits/FeatureServer/0");

            // Create feature table for the pools feature service
            ServiceFeatureTable poolsFeatureTable = new ServiceFeatureTable(serviceUri)
            {

                // Define the request mode
                FeatureRequestMode = FeatureRequestMode.OnInteractionCache
            };

            // Create FeatureLayer that uses the created table
            FeatureLayer poolsFeatureLayer = new FeatureLayer(poolsFeatureTable);

            // Add created layer to the map
            myMap.OperationalLayers.Add(poolsFeatureLayer);

            // Assign the map to the MapView
            MyMapView.Map = myMap;
        }
    }
}
