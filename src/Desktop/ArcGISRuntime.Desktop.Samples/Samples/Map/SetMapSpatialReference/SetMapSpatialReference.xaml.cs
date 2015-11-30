//Copyright 2015 Esri.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.
using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Windows;

namespace ArcGISRuntime.Desktop.Samples.SetMapSpatialReference
{
    public partial class SetMapSpatialReference
    {

        public SetMapSpatialReference()
        {
            InitializeComponent();
            loadMap();
            //var initVP = MyMapView.Map.InitialViewpoint;
            var vp = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
        }

        private async void loadMap()
        {
            try
            {
                //Create a map with World_Bonne projection
                var map = new Map(SpatialReference.Create(54024));
                //Create a map image layer which can re-project itself to the map's spatial reference
                var layer = new ArcGISMapImageLayer(new Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer"));
                //Set the map image layer as basemap
                map.Basemap.BaseLayers.Add(layer);
                //Set the map to be displayed in this view
                MyMapView.Map = map;
            }
            catch (Exception ex)
            {
                var errorMessage = "Map cannot be loaded. " + ex.Message;
                MessageBox.Show(errorMessage, "Sample error");
            }
        }
    }
}