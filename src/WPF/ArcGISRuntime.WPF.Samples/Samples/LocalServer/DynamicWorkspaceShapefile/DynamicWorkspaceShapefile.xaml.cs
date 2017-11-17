﻿// Copyright 2017 Esri.
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
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ArcGISRuntime.WPF.Samples.DynamicWorkspaceShapefile
{
    public partial class DynamicWorkspaceShapefile
    {
        // Hold a reference to the local map service
        private LocalMapService _localMapService;

        // Hold a reference to the layer that will display the shapefile data
        private ArcGISMapImageSublayer _shapefileSublayer;

        public DynamicWorkspaceShapefile()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();

            // Listen for the shutdown and unloaded events so that the local server can be shut down
            this.Dispatcher.ShutdownStarted += ShutdownSample;
            this.Unloaded += ShutdownSample;
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

        private async void Initialize()
        {
            // Create a map and add it to the view
            MyMapView.Map = new Map(BasemapType.Topographic, 39.7294, -104.8319, 12);
            
            try
            {
                // Handle the StatusChanged event to react when the server is started
                LocalServer.Instance.StatusChanged += ServerStatusChanged;

                // Start the local server instance
                await LocalServer.Instance.StartAsync();

                // Load the sample data
                await LoadShapefilePaths();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(String.Format("Please ensure that local server is installed prior to using the sample. See instructions in readme.me or metadata.json. Message: {0}", ex.Message));
            }
        }

        private void ServerStatusChanged(object sender, StatusChangedEventArgs e)
        {
            // Check if the server started successfully
            if (e.Status == LocalServerStatus.Started)
            {
                // Enable the 'choose shapefile' button
                MyChooseButton.IsEnabled = true;
            }
        }

        private async void StartLocalMapService(string filename, string path)
        {
            // Start a service from the blank MPK
            String mapServiceUrl = await GetMpkPath();

            // Create the local map service
            _localMapService = new LocalMapService(mapServiceUrl);

            // Create the shapefile workspace
            ShapefileWorkspace _shapefileWorkspace = new ShapefileWorkspace("shp_wkspc", path);

            // Create the layer source that represents the shapefile on disk
            TableSublayerSource source = new TableSublayerSource(_shapefileWorkspace.Id, filename);

            // Create a sublayer instance from the table source
            _shapefileSublayer = new ArcGISMapImageSublayer(0, source);

            // Add the dynamic workspace to the map service
            _localMapService.SetDynamicWorkspaces(new List<DynamicWorkspace>() { _shapefileWorkspace });

            // Subscribe to notifications about service status changes
            _localMapService.StatusChanged += _localMapService_StatusChanged;

            // Start the map service
            await _localMapService.StartAsync();
        }

        private async void _localMapService_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            // Add the shapefile layer to the map once the service finishes starting
            if (e.Status == LocalServerStatus.Started)
            {
                // Create the imagery layer
                ArcGISMapImageLayer imageryLayer = new ArcGISMapImageLayer(_localMapService.Url);

                // Subscribe to image layer load status change events
                // Only set up the sublayer renderer for the shapefile after the parent layer has finished loading
                imageryLayer.LoadStatusChanged += (q, ex) =>
                {
                    // Add the layer to the map once loaded
                    if (ex.Status == Esri.ArcGISRuntime.LoadStatus.Loaded)
                    {
                        // Create a default symbol style
                        SimpleLineSymbol _lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.Red, 3);

                        // Apply the symbol style with a renderer
                        _shapefileSublayer.Renderer = new SimpleRenderer(_lineSymbol);

                        // Add the shapefile sublayer to the imagery layer
                        imageryLayer.Sublayers.Add(_shapefileSublayer);
                    }
                };

                // Load the layer
                await imageryLayer.LoadAsync();

                // Clear any existing layers
                MyMapView.Map.OperationalLayers.Clear();

                // Add the image layer to the map
                MyMapView.Map.OperationalLayers.Add(imageryLayer);
            }
        }

        private async Task<String> GetMpkPath()
        {
            // Gets the path to the blank map package

            #region offlinedata

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "DynamicWorkspaceShapefile", "mpk_blank.mpk");

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                // Download the file
                await DataManager.GetData("ea619b4f0f8f4d108c5b87e90c1b5be0", "DynamicWorkspaceShapefile");
            }

            return filepath;

            #endregion offlinedata
        }

        private async Task<String> LoadShapefilePaths()
        {
            // Gets the path to the shapefile package

            #region offlinedata

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "DynamicWorkspaceShapefile", "TrailBikeNetwork.shp");

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                // Download the file
                await DataManager.GetData("d98b3e5293834c5f852f13c569930caa", "DynamicWorkspaceShapefile");
            }

            return filepath;

            #endregion offlinedata
        }

        private async void MyChooseButton_Click(object sender, RoutedEventArgs e)
        {
            // Allow the user to specify a file path - create the dialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog()
            {
                DefaultExt = ".shp",
                Filter = "Shapefiles|*.shp",
                InitialDirectory = Path.GetDirectoryName(await LoadShapefilePaths())
            };

            // Show the dialog and get the results
            bool? result = dlg.ShowDialog();

            // Take action if the user selected a file
            if (result == true)
            {
                string filename = Path.GetFileName(dlg.FileName);
                string path = Path.GetDirectoryName(dlg.FileName);
                StartLocalMapService(filename, path);
            }
        }
    }
}