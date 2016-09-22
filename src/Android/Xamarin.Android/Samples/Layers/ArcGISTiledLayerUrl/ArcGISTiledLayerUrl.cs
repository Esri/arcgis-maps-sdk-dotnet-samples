// Copyright 2016 Esri.
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
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using System;

namespace ArcGISRuntimeXamarin.Samples.ArcGISTiledLayerUrl
{
    [Activity]
    public class ArcGISTiledLayerUrl : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "ArcGIS tiled layer (URL)";

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map
            Map myMap = new Map();

            // Create uri to the tiled service
            var serviceUri = new Uri(
               "http://services.arcgisonline.com/arcgis/rest/services/World_Topo_Map/MapServer");

            // Create new tiled layer from the url
            ArcGISTiledLayer imageLayer = new ArcGISTiledLayer(serviceUri);

            // Add created layer to the basemaps collection
            myMap.Basemap.BaseLayers.Add(imageLayer);

            // Assign the map to the MapView
            _myMapView.Map = myMap;
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
    }
}