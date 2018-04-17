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
using Android.Widget;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;

namespace ArcGISRuntime.Samples.WMSLayerUrl
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "WMS layer (URL)",
        "Layers",
        "This sample demonstrates how to add a layer from a WMS service to a map.",
        "")]
    public class WMSLayerUrl : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        // Hold the URL to the WMS service showing the geology of Africa
        private Uri wmsUrl = new Uri("https://certmapper.cr.usgs.gov/arcgis/services/geology/africa/MapServer/WMSServer?request=GetCapabilities&service=WMS");

        // Hold a list of uniquely-identifying WMS layer names to display
        private List<String> wmsLayerNames = new List<string> { "0" };

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "WMS layer (URL)";

            // Create the UI, setup the control references
            CreateLayout();

            // Initialize the map
            Initialize();
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the map view to the layout
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }

        private void Initialize()
        {
            // Apply an imagery basemap to the map
            Map myMap = new Map(Basemap.CreateImagery());

            // Set the initial viewpoint
            myMap.InitialViewpoint = new Viewpoint(
                new MapPoint(25.450, -4.59, new SpatialReference(4326)), 1000000);

            // Add the map to the mapview
            _myMapView.Map = myMap;

            // Create a new WMS layer displaying the specified layers from the service
            WmsLayer myWmsLayer = new WmsLayer(wmsUrl, wmsLayerNames);

            // Add the layer to the map
            myMap.OperationalLayers.Add(myWmsLayer);
        }
    }
}