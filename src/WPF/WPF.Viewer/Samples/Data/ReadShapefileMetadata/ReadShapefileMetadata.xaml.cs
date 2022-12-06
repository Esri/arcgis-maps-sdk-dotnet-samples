// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.ReadShapefileMetadata
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Read shapefile metadata",
        category: "Data",
        description: "Read a shapefile and display its metadata.",
        instructions: "The shapefile's metadata will be displayed when you open the sample.",
        tags: new[] { "credits", "description", "metadata", "package", "shape file", "shapefile", "summary", "symbology", "tags", "visualization" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("d98b3e5293834c5f852f13c569930caa")]
    public partial class ReadShapefileMetadata
    {
        public ReadShapefileMetadata()
        {
            InitializeComponent();

            // Open a shapefile stored locally and add it to the map as a feature layer
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create a new map to display in the map view with a streets basemap
            Map streetMap = new Map(BasemapStyle.ArcGISStreets);

            // Get the path to the downloaded shapefile
            string filepath = GetShapefilePath();

            try
            {
                // Open the shapefile
                ShapefileFeatureTable myShapefile = await ShapefileFeatureTable.OpenAsync(filepath);

                // Read metadata about the shapefile and display it in the UI
                ShapefileInfo fileInfo = myShapefile.Info;
                InfoPanel.DataContext = fileInfo;

                // Display the shapefile thumbnail in an image control
                ShapefileThumbnailImage.Source = await RuntimeImageExtensions.ToImageSourceAsync(fileInfo.Thumbnail);

                // Create a feature layer to display the shapefile
                FeatureLayer newFeatureLayer = new FeatureLayer(myShapefile);
                await newFeatureLayer.LoadAsync();

                // Zoom the map to the extent of the shapefile
                MyMapView.SpatialReferenceChanged += (s, e) => { MyMapView.SetViewpoint(new Viewpoint(newFeatureLayer.FullExtent)); };

                // Add the feature layer to the map
                streetMap.OperationalLayers.Add(newFeatureLayer);

                // Show the map in the MapView
                MyMapView.Map = streetMap;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
            }
        }

        private static string GetShapefilePath()
        {
            return DataManager.GetDataFolder("d98b3e5293834c5f852f13c569930caa", "TrailBikeNetwork.shp");
        }
    }
}