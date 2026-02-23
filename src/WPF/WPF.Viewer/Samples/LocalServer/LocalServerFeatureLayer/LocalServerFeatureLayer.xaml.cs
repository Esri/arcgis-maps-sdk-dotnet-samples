// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.LocalServices;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.LocalServerFeatureLayer
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Local server feature layer",
        category: "Local Server",
        description: "Start a local feature service and display its features in a map.",
        instructions: "A Local Server and Local Feature Service will automatically be started. Once started then a `FeatureLayer` will be created and added to the map.",
        tags: new[] { "feature service", "local", "offline", "server", "service" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("92ca5cdb3ff1461384bf80dc008e297b")]
    public partial class LocalServerFeatureLayer
    {
        // Hold a reference to the local feature service; the ServiceFeatureTable will be loaded from this service
        private LocalFeatureService _localFeatureService;

        public LocalServerFeatureLayer()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create a map and add it to the view
            MyMapView.Map = new Map(BasemapStyle.ArcGISStreetsRelief);

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
            }
            catch (Exception ex)
            {
                var localServerTypeInfo = typeof(LocalMapService).GetTypeInfo();
                var localServerVersion = FileVersionInfo.GetVersionInfo(localServerTypeInfo.Assembly.Location);

                MessageBox.Show($"Please ensure that local server {localServerVersion.FileVersion} is installed prior to using the sample. The download link is in the description. Message: {ex.Message}", "Local Server failed to start");
                return;
            }

            // Load the sample data and get the path
            string myfeatureServicePath = GetFeatureLayerPath();

            // Create the feature service to serve the local data
            _localFeatureService = new LocalFeatureService(myfeatureServicePath);

            // Listen to feature service status changes
            _localFeatureService.StatusChanged += _localFeatureService_StatusChanged;

            // Start the feature service
            try
            {
                await _localFeatureService.StartAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "The feature service failed to load");
            }
        }

        private async void _localFeatureService_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            // Load the map from the service once ready
            if (e.Status == LocalServerStatus.Started)
            {
                // Get the path to the first layer - the local feature service url + layer ID
                string layerUrl = _localFeatureService.Url + "/0";

                // Create the ServiceFeatureTable
                ServiceFeatureTable myFeatureTable = new ServiceFeatureTable(new Uri(layerUrl));

                // Create the Feature Layer from the table
                FeatureLayer myFeatureLayer = new FeatureLayer(myFeatureTable);

                // Add the layer to the map
                MyMapView.Map.OperationalLayers.Add(myFeatureLayer);

                try
                {
                    // Wait for the layer to load
                    await myFeatureLayer.LoadAsync();

                    // Set the viewpoint on the MapView to show the layer data
                    await MyMapView.SetViewpointGeometryAsync(myFeatureLayer.FullExtent, 50);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error");
                }
            }
        }

        private static string GetFeatureLayerPath()
        {
            return DataManager.GetDataFolder("92ca5cdb3ff1461384bf80dc008e297b", "PointsOfInterest.mpkx");
        }
    }
}