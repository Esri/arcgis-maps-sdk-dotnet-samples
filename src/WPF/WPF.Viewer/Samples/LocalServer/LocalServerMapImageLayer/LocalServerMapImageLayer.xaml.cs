// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.LocalServices;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.LocalServerMapImageLayer
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Local Server map image layer",
        category: "Local Server",
        description: "Start the Local Server and Local Map Service, create an ArcGIS Map Image Layer from the Local Map Service, and add it to a map.",
        instructions: "The Local Server and local map service will automatically be started and, once running, a map image layer will be created and added to the map.",
        tags: new[] { "image", "layer", "local", "offline", "server" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("85c34847bbe1402fa115a1b9b87561ce")]
    public partial class LocalServerMapImageLayer
    {
        // Hold a reference to the local map service
        private LocalMapService _localMapService;

        public LocalServerMapImageLayer()
        {
            InitializeComponent();

            // set up the sample
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create a map and add it to the view
            MyMapView.Map = new Map(BasemapStyle.ArcGISLightGray);

            try
            {
                // LocalServer must not be running when setting the data path.
                if (LocalServer.Instance.Status == LocalServerStatus.Started)
                {
                    await LocalServer.Instance.StopAsync();
                }

                // Set the local data path - must be done before starting. On most systems, this will be C:\EsriSamples\AppData.
                // This path should be kept short to avoid Windows path length limitations.
                string tempDataPathRoot = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.Windows)).FullName;
                string tempDataPath = Path.Combine(tempDataPathRoot, "EsriSamples", "AppData");
                Directory.CreateDirectory(tempDataPath); // CreateDirectory won't overwrite if it already exists.
                LocalServer.Instance.AppDataPath = tempDataPath;

                // Start the local server instance
                await LocalServer.Instance.StartAsync();

                // Get the path to the map package that will be served
                string datapath = GetDataPath();

                // Create the Map Service from the data
                _localMapService = new LocalMapService(datapath);

                // Start the feature service
                await _localMapService.StartAsync();

                // Get the url to the map service
                Uri myServiceUri = _localMapService.Url;

                // Create the layer from the url
                ArcGISMapImageLayer myImageLayer = new ArcGISMapImageLayer(myServiceUri);

                // Add the layer to the map
                MyMapView.Map.OperationalLayers.Add(myImageLayer);

                // Wait for the layer to load
                await myImageLayer.LoadAsync();

                // Set the viewpoint on the map to show the data
                MyMapView.SetViewpoint(new Viewpoint(myImageLayer.FullExtent));
            }
            catch (Exception ex)
            {
                var localServerTypeInfo = typeof(LocalMapService).GetTypeInfo();
                var localServerVersion = FileVersionInfo.GetVersionInfo(localServerTypeInfo.Assembly.Location);

                MessageBox.Show($"Please ensure that local server {localServerVersion.FileVersion} is installed prior to using the sample. The download link is in the description. Message: {ex.Message}", "Local Server failed to start");
            }
        }

        private static string GetDataPath()
        {
            return DataManager.GetDataFolder("85c34847bbe1402fa115a1b9b87561ce", "RelationshipID.mpkx");
        }
    }
}