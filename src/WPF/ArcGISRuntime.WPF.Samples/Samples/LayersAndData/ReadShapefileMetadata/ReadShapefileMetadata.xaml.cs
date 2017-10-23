// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using System.IO;
using System.Threading.Tasks;

namespace ArcGISRuntime.WPF.Samples.ReadShapefileMetadata
{
    public partial class ReadShapefileMetadata
    {
        public ReadShapefileMetadata()
        {
            InitializeComponent();
            
            // Open a shapefile stored locally and add it to the map as a feature layer
            Initialize();
        }

        private async void Initialize()
        {
            // Create a new map to display in the map view with a streets basemap
            Map streetMap = new Map(Basemap.CreateStreetsVector());

            // Get the path to the downloaded shapefile
            string filepath = await GetShapefilePath();

            // Open the shapefile
            ShapefileFeatureTable myShapefile = await ShapefileFeatureTable.OpenAsync(filepath);

            // Read metadata about the shapefile and display it in the UI
            ShapefileInfo fileInfo = myShapefile.Info;
            InfoPanel.DataContext = fileInfo;

            // Display the shapefile thumbnail in an image control
            ShapefileThumbnailImage.Source = await Esri.ArcGISRuntime.UI.RuntimeImageExtensions.ToImageSourceAsync(fileInfo.Thumbnail);

            // Create a feature layer to display the shapefile
            FeatureLayer newFeatureLayer = new FeatureLayer(myShapefile);
            await newFeatureLayer.LoadAsync();

            // Zoom the map to the extent of the shapefile
            MyMapView.SpatialReferenceChanged += async (s, e) =>
            {
                await MyMapView.SetViewpointGeometryAsync(newFeatureLayer.FullExtent);
            };

            // Add the feature layer to the map
            streetMap.OperationalLayers.Add(newFeatureLayer);

            // Show the map in the MapView
            MyMapView.Map = streetMap;
        }

        private async Task<string> GetShapefilePath()
        {
            #region offlinedata
            // The shapefile will be downloaded from ArcGIS Online
            // The data manager (a component of the sample viewer, *NOT* the runtime
            //     handles the offline data process

            // The desired shapefile is expected to be called "TrailBikeNetwork.shp"
            string filename = "TrailBikeNetwork.shp";

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "ReadShapefileMetadata", filename);

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                // Download the shapefile
                await DataManager.GetData("d98b3e5293834c5f852f13c569930caa", "ReadShapefileMetadata");
            }

            // Return the path
            return filepath;
            #endregion offlinedata
        }
    }
}