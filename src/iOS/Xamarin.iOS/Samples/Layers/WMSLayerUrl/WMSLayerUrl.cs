﻿// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.WMSLayerUrl
{
    [Register("WMSLayerUrl")]
    public class WMSLayerUrl : UIViewController
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Hold the URL to the service
        private Uri wmsUrl = new Uri("https://certmapper.cr.usgs.gov/arcgis/services/geology/africa/MapServer/WMSServer?request=GetCapabilities&service=WMS");

        // Hold a list of unique identifiers of WMS layers that will be displayed
        private List<String> wmsLayerNames = new List<string> { "0" };

        public WMSLayerUrl()
        {
            Title = "WMS layer (URL)";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references
            CreateLayout();

            // Initialize the map
            Initialize();
        }

        private async void Initialize()
        {
            // Apply an imagery basemap to the map
            _myMapView.Map = new Map(Basemap.CreateImagery());

            // Create the layer
            WmsLayer myWmsLayer = new WmsLayer(wmsUrl, wmsLayerNames);

            // Load the layer
            await myWmsLayer.LoadAsync();

            // Add the layer to the map
            _myMapView.Map.OperationalLayers.Add(myWmsLayer);
        }

        public override void ViewDidLayoutSubviews()
        {
            // Setup the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);

            base.ViewDidLayoutSubviews();
        }

        private void CreateLayout()
        {
            // Add the mapview to the view
            View.AddSubviews(_myMapView);
        }
    }
}