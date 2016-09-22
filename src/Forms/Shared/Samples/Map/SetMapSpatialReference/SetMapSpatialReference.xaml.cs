// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using System;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.SetMapSpatialReference
{
    public partial class SetMapSpatialReference : ContentPage
    {
        public SetMapSpatialReference()
        {
            InitializeComponent ();

            Title = "Set map spatial reference";

            // Create the UI, setup the control references and execute initialization 
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map using spatial reference as world bonne (54024)
            Map myMap = new Map(SpatialReference.Create(54024));

            // Adding a map image layer which can reproject itself to the map's spatial reference
            // Note: Some layer such as tiled layer cannot reproject and will fail to draw if their spatial 
            // reference is not the same as the map's spatial reference
            ArcGISMapImageLayer operationalLayer = new ArcGISMapImageLayer(new Uri(
                "http://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer"));
           
            // Add operational layer to the Map
            myMap.OperationalLayers.Add(operationalLayer);

            // Assign the map to the MapView
            MyMapView.Map = myMap;
        }
    }
}
