// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntimeXamarin.Managers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.KmlLayerFile
{
    public partial class KmlLayerFile : ContentPage
    {
        public KmlLayerFile()
        {
            InitializeComponent();

            Title = "KML layer (file)";

            Initialize();
        }

        private async void Initialize()
        {
            // Initialize the map with a dark gray basemap
            MyMapView.Map = new Map(Basemap.CreateDarkGrayCanvasVector());

            // Get the path of the KML file
            Uri filePath = new Uri(await GetKmlPath(), UriKind.RelativeOrAbsolute);

            // Create a KML dataset
            KmlDataset fileDataSource = new KmlDataset(filePath);

            // Create a KML layer from the dataset
            KmlLayer displayLayer = new KmlLayer(fileDataSource);

            // Add the layer to the map
            MyMapView.Map.OperationalLayers.Add(displayLayer);
        }

        // Get the file path for the KML file
        private async Task<String> GetKmlPath()
        {
            #region offlinedata

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "KmlLayerFile", "US_State_Capitals.kml");

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                // Download the file
                await DataManager.GetData("324e4742820e46cfbe5029ff2c32cb1f", "KmlLayerFile");
            }

            return filepath;

            #endregion offlinedata
        }
    }
}