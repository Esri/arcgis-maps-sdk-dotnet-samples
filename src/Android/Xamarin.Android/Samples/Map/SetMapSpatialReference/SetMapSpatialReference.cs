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
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using System;

namespace ArcGISRuntimeXamarin.Samples.SetMapSpatialReference
{
    [Activity]
    public class SetMapSpatialReference : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView = new MapView();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Set map spatial reference";
            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map using spatial reference as world bonne (54024)
            Map myMap = new Map(SpatialReference.Create(54024));

            // Adding a map image layer which can re-project itself to the map's spatial reference
            // Note: Some layer such as tiled layer cannot re-project and will fail to draw if their spatial 
            // reference is not the same as the map's spatial reference
            ArcGISMapImageLayer operationalLayer = new ArcGISMapImageLayer(new Uri(
                "http://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer"));

            // Add operational layer to the Map
            myMap.OperationalLayers.Add(operationalLayer);

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