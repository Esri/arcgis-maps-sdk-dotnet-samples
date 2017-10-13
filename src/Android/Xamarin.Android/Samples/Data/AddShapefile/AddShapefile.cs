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
using ArcGISRuntimeXamarin.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System.IO;

namespace ArcGISRuntimeXamarin.Samples.AddShapefile
{
    [Activity]
    public class AddShapefile : Activity
    {
        private MapView _myMapView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Add shapefile";
            CreateLayout();

            // Download (if necessary) and add a local shapefile dataset to the map
            Initialize();
        }

        private async void Initialize()
        {
            // Create a new map to display in the map view with a streets basemap
            _myMapView.Map = new Map(Basemap.CreateStreets());

            // The shapefile will be downloaded from ArcGIS Online
            // The data manager (a component of the sample viewer, *NOT* the runtime
            //     handles the offline data process

            // The desired shapefile is expected to be called "Public_Art.shp"
            string filename = "Public_Art.shp";

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "AddShapefile", filename);

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                // Download the shapefile
                await DataManager.GetData("d98b3e5293834c5f852f13c569930caa", "AddShapefile");
            }

            // Open the shapefile
            ShapefileFeatureTable myShapefile = await ShapefileFeatureTable.OpenAsync(filepath);

            // Create a feature layer to display the shapefile
            FeatureLayer newFeatureLayer = new FeatureLayer(myShapefile);

            // Add the feature layer to the map
            _myMapView.Map.OperationalLayers.Add(newFeatureLayer);

            // Zoom the map to the extent of the shapefile
            await _myMapView.SetViewpointGeometryAsync(newFeatureLayer.FullExtent);
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add a map view to the layout
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}