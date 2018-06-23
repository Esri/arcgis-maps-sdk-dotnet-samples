// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ServiceFeatureTableNoCache
{
    [Register("ServiceFeatureTableNoCache")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Service feature table (no cache)",
        "Data",
        "This sample demonstrates how to use a feature service in on interaction no cache mode.",
        "")]
    public class ServiceFeatureTableNoCache : UIViewController
    {
        // Create and hold a reference to the MapView.
        private readonly MapView _myMapView = new MapView();

        public ServiceFeatureTableNoCache()
        {
            Title = "Service feature table (no cache)";
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

        private void Initialize()
        {
            // Create new Map with basemap.
            Map myMap = new Map(Basemap.CreateTopographic());

            // Create and set initial map area.
            Envelope initialLocation = new Envelope(-1.30758164047166E7, 4014771.46954516, -1.30730056797177E7, 4016869.78617381, SpatialReferences.WebMercator);
            myMap.InitialViewpoint = new Viewpoint(initialLocation);

            // Create URI to the feature service.
            var serviceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/PoolPermits/FeatureServer/0");

            // Create feature table for the pools feature service.
            ServiceFeatureTable poolsFeatureTable = new ServiceFeatureTable(serviceUri)
            {
                // Define the request mode.
                FeatureRequestMode = FeatureRequestMode.OnInteractionNoCache
            };

            // Create FeatureLayer that uses the created table.
            FeatureLayer poolsFeatureLayer = new FeatureLayer(poolsFeatureTable);

            // Add created layer to the map.
            myMap.OperationalLayers.Add(poolsFeatureLayer);

            // Assign the map to the MapView.
            _myMapView.Map = myMap;

            // Feature table initialization.
            poolsFeatureTable.RetryLoadAsync();
        }

        private void CreateLayout()
        {
            // Add MapView to the page.
            View.AddSubviews(_myMapView);
        }
    }
}