// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.LocalServices;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGISRuntime.WPF.Samples.LocalServerMapImageLayer
{
    public partial class LocalServerMapImageLayer
    {
        // Hold a reference to the local map service
        private LocalMapService _localMapService;

        public LocalServerMapImageLayer()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private async void Initialize()
        {
            // Create a map and add it to the view
            MyMapView.Map = new Map(Basemap.CreateLightGrayCanvasVector());

            try
            {
                // Start the local server instance
                await LocalServer.Instance.StartAsync();

                // Get the path to the map package that will be served
                string datapath = await GetDataPath();

                // Create the Map Service from the data
                _localMapService = new LocalMapService(datapath);

                // Be notified when the map service is loaded
                _localMapService.StatusChanged += _localMapService_StatusChanged;

                // Start the feature service
                await _localMapService.StartAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Local Server failed to start");
            }
            // Listen for the shutdown and unloaded events so that the local server can be shut down
            this.Dispatcher.ShutdownStarted += ShutdownSample;
            this.Unloaded += ShutdownSample;
        }

        private async void _localMapService_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            // Load a MapImageLayer from the service once it has started
            if (e.Status == LocalServerStatus.Started)
            {
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
        }

        private async void ShutdownSample(object sender, EventArgs e)
        {
            try
            {
                // Shut down the local server if it has started
                if (LocalServer.Instance.Status == LocalServerStatus.Started)
                {
                    await LocalServer.Instance.StopAsync();
                }
            }
            catch (InvalidOperationException)
            {
                // Local server isn't installed, just return
                return;
            }
        }

        private async Task<string> GetDataPath()
        {
            #region offlinedata

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "LocalServerMapImageLayer", "RelationshipID.mpk");

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                // Download the file
                await DataManager.GetData("dee5d8060a6048a4b063484199a9546b", "LocalServerMapImageLayer");
            }

            return filepath;

            #endregion offlinedata
        }
    }
}