// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Data;

namespace ArcGISRuntime.UWP.Samples.FeatureLayerShapefile
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Feature layer (shapefile)",
        "Data",
        "This sample demonstrates how to open a shapefile stored on the device and display it as a feature layer with default symbology.",
        "The shapefile will be downloaded from an ArcGIS Online portal automatically.",
        "Featured")]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData("d98b3e5293834c5f852f13c569930caa")]
    public partial class FeatureLayerShapefile
    {
        public FeatureLayerShapefile()
        {
            InitializeComponent();

            // Open a shapefile stored locally and add it to the map as a feature layer
            Initialize();
        }

        private async void Initialize()
        {
            // Create a new map to display in the map view with a streets basemap
            MyMapView.Map = new Map(Basemap.CreateStreetsVector());

            // Get the path to the downloaded shapefile
            string filepath = GetShapefilePath();

            // Open the shapefile
            ShapefileFeatureTable myShapefile = await ShapefileFeatureTable.OpenAsync(filepath);

            // Create a feature layer to display the shapefile
            FeatureLayer newFeatureLayer = new FeatureLayer(myShapefile);

            // Add the feature layer to the map
            MyMapView.Map.OperationalLayers.Add(newFeatureLayer);

            // Zoom the map to the extent of the shapefile
            await MyMapView.SetViewpointGeometryAsync(newFeatureLayer.FullExtent);
        }

        private static string GetShapefilePath()
        {
            return DataManager.GetDataFolder("d98b3e5293834c5f852f13c569930caa", "Public_Art.shp");
        }
    }
}