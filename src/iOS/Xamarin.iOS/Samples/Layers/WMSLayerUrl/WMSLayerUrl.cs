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
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.WMSLayerUrl
{
    [Register("WMSLayerUrl")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "WMS layer (URL)",
        "Layers",
        "This sample demonstrates how to add a layer from a WMS service to a map.",
        "")]
    public class WMSLayerUrl : UIViewController
    {
        // Create and hold a reference to the MapView.
        private MapView _myMapView;

        // Hold the URL to the WMS service showing the geology of Africa.
        private readonly Uri _wmsUrl = new Uri("https://certmapper.cr.usgs.gov/arcgis/services/geology/africa/MapServer/WMSServer?request=GetCapabilities&service=WMS");

        // Hold a list of uniquely-identifying WMS layer names to display.
        private readonly List<string> _wmsLayerNames = new List<string> {"0"};

        public WMSLayerUrl()
        {
            Title = "WMS layer (URL)";
        }

        public override void LoadView()
        {
            base.LoadView();

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubviews(_myMapView);

            _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
            _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Initialize();
        }

        private void Initialize()
        {
            // Apply an imagery basemap to the map and set the viewpoint to a zoomed-in part of Africa.
            Map myMap = new Map(Basemap.CreateImagery())
            {
                InitialViewpoint = new Viewpoint(new MapPoint(25.450, -4.59, SpatialReferences.Wgs84), 1000000)
            };

            // Show the map.
            _myMapView.Map = myMap;

            // Create a new WMS layer displaying the specified layers from the service.
            WmsLayer myWmsLayer = new WmsLayer(_wmsUrl, _wmsLayerNames);

            // Add the layer to the map.
            myMap.OperationalLayers.Add(myWmsLayer);
        }
    }
}