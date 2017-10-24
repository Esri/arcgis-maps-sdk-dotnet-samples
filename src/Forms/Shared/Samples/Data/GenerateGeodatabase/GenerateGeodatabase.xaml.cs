﻿// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using ArcGISRuntimeXamarin.Managers;
using System;
using System.Linq;
using System.IO;
using System.Reflection;
using Xamarin.Forms;

#if WINDOWS_UWP
using Colors = Windows.UI.Colors;
#else

using Colors = System.Drawing.Color;

#endif

namespace ArcGISRuntimeXamarin.Samples.GenerateGeodatabase
{
    public partial class GenerateGeodatabase : ContentPage
    {
        // URI for a feature service that supports geodatabase generation
        private Uri _featureServiceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Sync/WildfireSync/FeatureServer");

        // Path to the geodatabase file on disk
        private string _gdbPath;

        // Task to be used for generating the geodatabase
        private GeodatabaseSyncTask _gdbSyncTask;

        // Job used to generate the geodatabase
        private GenerateGeodatabaseJob _generateGdbJob;

        public GenerateGeodatabase()
        {
            InitializeComponent();

            Title = "Generate geodatabase";

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private async void Initialize()
        {
            // Create a tile cache and load it with the SanFrancisco streets tpk
            TileCache _tileCache = new TileCache(GetTpkPath());

            // Create the corresponding layer based on the tile cache
            ArcGISTiledLayer _tileLayer = new ArcGISTiledLayer(_tileCache);

            // Create the basemap based on the tile cache
            Basemap _sfBasemap = new Basemap(_tileLayer);

            // Create the map with the tile-based basemap
            Map myMap = new Map(_sfBasemap);

            // Assign the map to the MapView
            myMapView.Map = myMap;

            // Create a new symbol for the extent graphic
            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.Red, 2);

            // Create graphics overlay for the extent graphic and apply a renderer
            GraphicsOverlay extentOverlay = new GraphicsOverlay();
            extentOverlay.Renderer = new SimpleRenderer(lineSymbol);

            // Add graphics overlay to the map view
            myMapView.GraphicsOverlays.Add(extentOverlay);

            // Set up an event handler for when the viewpoint (extent) changes
            myMapView.ViewpointChanged += MapViewExtentChanged;

            // Update the local data path for the geodatabase file
            _gdbPath = GetGdbPath();

            // Create a task for generating a geodatabase (GeodatabaseSyncTask)
            _gdbSyncTask = await GeodatabaseSyncTask.CreateAsync(_featureServiceUri);

            // Add all graphics from the service to the map
            foreach (var layer in _gdbSyncTask.ServiceInfo.LayerInfos)
            {
                // Create the ServiceFeatureTable for this particular layer
                ServiceFeatureTable onlineTable = new ServiceFeatureTable(new Uri(_featureServiceUri + "/" + layer.Id));

                // Wait for the table to load
                await onlineTable.LoadAsync();

                // Add the layer to the map's operational layers if load succeeds
                if (onlineTable.LoadStatus == Esri.ArcGISRuntime.LoadStatus.Loaded)
                {
                    myMap.OperationalLayers.Add(new FeatureLayer(onlineTable));
                }
            }
        }

        private void UpdateMapExtent()
        {
            // Return if mapview is null
            if (myMapView == null) { return; }

            // Get the new viewpoint
            Viewpoint myViewPoint = myMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);

            // Return if viewpoint is null
            if (myViewPoint == null) { return; }

            // Get the updated extent for the new viewpoint
            Envelope extent = myViewPoint.TargetGeometry as Envelope;

            // Return if extent is null
            if (extent == null) { return; }

            // Create an envelope that is a bit smaller than the extent
            EnvelopeBuilder envelopeBldr = new EnvelopeBuilder(extent);
            envelopeBldr.Expand(0.80);

            // Get the (only) graphics overlay in the map view
            var extentOverlay = myMapView.GraphicsOverlays.FirstOrDefault();

            // Return if the extent overlay is null
            if (extentOverlay == null) { return; }

            // Get the extent graphic
            Graphic extentGraphic = extentOverlay.Graphics.FirstOrDefault();

            // Create the extent graphic and add it to the overlay if it doesn't exist
            if (extentGraphic == null)
            {
                extentGraphic = new Graphic(envelopeBldr.ToGeometry());
                extentOverlay.Graphics.Add(extentGraphic);
            }
            else
            {
                // Otherwise, simply update the graphic's geometry
                extentGraphic.Geometry = envelopeBldr.ToGeometry();
            }
        }

        private async void StartGeodatabaseGeneration()
        {
            // Create a task for generating a geodatabase (GeodatabaseSyncTask)
            _gdbSyncTask = await GeodatabaseSyncTask.CreateAsync(_featureServiceUri);

            // Get the current extent of the red preview box
            Envelope extent = myMapView.GraphicsOverlays.FirstOrDefault().Extent as Envelope;

            // Get the default parameters for the generate geodatabase task
            GenerateGeodatabaseParameters generateParams = await _gdbSyncTask.CreateDefaultGenerateGeodatabaseParametersAsync(extent);

            // Create a generate geodatabase job
            _generateGdbJob = _gdbSyncTask.GenerateGeodatabase(generateParams, _gdbPath);

            // Handle the job changed event
            _generateGdbJob.JobChanged += GenerateGdbJobChanged;

            // Handle the progress changed event (to show progress bar)
            _generateGdbJob.ProgressChanged += ((object sender, EventArgs e) =>
            {
                UpdateProgressBar();
            });

            // Start the job
            _generateGdbJob.Start();
        }

        private async void HandleGenerationStatusChange(GenerateGeodatabaseJob job)
        {
            JobStatus status = job.Status;

            // If the job completed successfully, add the geodatabase data to the map
            if (status == JobStatus.Succeeded)
            {
                // Clear out the existing layers
                myMapView.Map.OperationalLayers.Clear();

                // Get the new geodatabase
                Geodatabase resultGdb = await job.GetResultAsync();

                // Loop through all feature tables in the geodatabase and add a new layer to the map
                foreach (GeodatabaseFeatureTable table in resultGdb.GeodatabaseFeatureTables)
                {
                    // Create a new feature layer for the table
                    FeatureLayer _layer = new FeatureLayer(table);

                    // Add the new layer to the map
                    myMapView.Map.OperationalLayers.Add(_layer);
                }
                // Best practice is to unregister the geodatabase
                await _gdbSyncTask.UnregisterGeodatabaseAsync(resultGdb);

                // Tell the user that the geodatabase was unregistered
                ShowStatusMessage("Since no edits will be made, the local geodatabase has been unregistered per best practice.");
            }

            // See if the job failed
            if (status == JobStatus.Failed)
            {
                // Create a message to show the user
                string message = "Generate geodatabase job failed";

                // Show an error message (if there is one)
                if (job.Error != null)
                {
                    message += ": " + job.Error.Message;
                }
                else
                {
                    // If no error, show messages from the job
                    var m = from msg in job.Messages select msg.Message;
                    message += ": " + string.Join<string>("\n", m);
                }

                ShowStatusMessage(message);
            }
        }

        // Get the path to the tile package used for the basemap
        private string GetTpkPath()
        {
            #region offlinedata
            // The desired tpk is expected to be called SanFrancisco.tpk
            string filename = "SanFrancisco.tpk";

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Return the full path; Item ID is 3f1bbf0ec70b409a975f5c91f363fe7d
            return Path.Combine(folder, "SampleData", "GenerateGeodatabase", filename);
            #endregion offlinedata
        }

        private string GetGdbPath()
        {
            // Set the platform-specific path for storing the geodatabase
            String folder = "";

#if NETFX_CORE //UWP
            folder = Windows.Storage.ApplicationData.Current.LocalFolder.Path.ToString();
#elif __IOS__
            folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#elif __ANDROID__
            folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#endif
            // Set the final path
            return Path.Combine(folder, "wildfire.geodatabase");
        }

        private void ShowStatusMessage(string message)
        {
            // Display the message to the user
            DisplayAlert("Alert", message, "OK");
        }

        // Handler for the generate button clicked event
        private void GenerateButton_Clicked(object sender, EventArgs e)
        {
            // Call the cross-platform geodatabase generation method
            StartGeodatabaseGeneration();
        }

        // Handler for the MapView Extent Changed event
        private void MapViewExtentChanged(object sender, EventArgs e)
        {
            // Call the cross-platform map extent update method
            UpdateMapExtent();
        }

        // Handler for the job changed event
        private void GenerateGdbJobChanged(object sender, EventArgs e)
        {
            // Get the job object; will be passed to HandleGenerationStatusChange
            GenerateGeodatabaseJob job = sender as GenerateGeodatabaseJob;

            // Due to the nature of the threading implementation,
            //     the dispatcher needs to be used to interact with the UI
            Device.BeginInvokeOnMainThread(() =>
            {
                // Hide the progress bar if the job is finished
                if (job.Status == JobStatus.Succeeded || job.Status == JobStatus.Failed)
                {
                    myProgressBar.IsVisible = false;
                }
                else // Show it otherwise
                {
                    myProgressBar.IsVisible = true;
                }

                // Do the remainder of the job status changed work
                HandleGenerationStatusChange(job);
            });
        }

        private async void UpdateProgressBar()
        {
            // Due to the nature of the threading implementation,
            //     the dispatcher needs to be used to interact with the UI
            Device.BeginInvokeOnMainThread(() =>
            {
                // Update the progress bar value
                myProgressBar.Progress = _generateGdbJob.Progress / 100.0;
            });
        }
    }
}