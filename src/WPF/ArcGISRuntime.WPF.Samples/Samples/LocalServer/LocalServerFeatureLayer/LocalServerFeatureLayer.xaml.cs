﻿// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.LocalServices;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGISRuntime.WPF.Samples.LocalServerFeatureLayer
{
    public partial class LocalServerFeatureLayer
    {
        // Hold a reference to the local feature service; the ServiceFeatureTable will be loaded from this service
        private LocalFeatureService _localFeatureService;

        public LocalServerFeatureLayer()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private async void Initialize()
        {
            // Listen for the shutdown and unloaded events so that the local server can be shut down when sample is closed
            this.Dispatcher.ShutdownStarted += ShutdownSample;
            this.Unloaded += ShutdownSample;

            // Create a map and add it to the view
            MyMapView.Map = new Map(Basemap.CreateStreetsWithReliefVector());

            try
            {
                // Start the local server instance
                await LocalServer.Instance.StartAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Local Server load failed");
                return;
            }

            // Load the sample data and get the path
            string myfeatureServicePath = await GetFeatureLayerPath();

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

                // Wait for the layer to load
                await myFeatureLayer.LoadAsync();

                // Set the viewpoint on the MapView to show the layer data
                MyMapView.SetViewpoint(new Viewpoint(myFeatureLayer.FullExtent));
            }
        }

        private async Task<string> GetFeatureLayerPath()
        {
            #region offlinedata

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "LocalServerFeatureLayer", "PointsOfInterest.mpk");

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                // Download the file
                await DataManager.GetData("4e94fec734434d1288e6ebe36c3c461f", "LocalServerFeatureLayer");
            }

            return filepath;

            #endregion offlinedata
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
    }
}