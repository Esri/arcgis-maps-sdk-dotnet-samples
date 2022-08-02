// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Microsoft.UI.Xaml;
using Esri.ArcGISRuntime.ArcGISServices;

namespace ArcGISRuntime.WinUI.Samples.GenerateGeodatabaseReplica
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Generate geodatabase",
        category: "Data",
        description: "Generate a local geodatabase from an online feature service.",
        instructions: "Zoom to any extent. Then click the generate button to generate a geodatabase of features from a feature service filtered to the current extent. A red outline will show the extent used. The job's progress is shown while the geodatabase is generated.",
        tags: new[] { "disconnected", "local geodatabase", "offline", "sync" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("e4a398afe9a945f3b0f4dca1e4faccb5")]
    public partial class GenerateGeodatabaseReplica
    {
        // URL for a feature service that supports geodatabase generation.
        private Uri _featureServiceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Sync/WildfireSync/FeatureServer");

        // Path to the geodatabase file on disk.
        private string _gdbPath;

        // Task to be used for generating the geodatabase.
        private GeodatabaseSyncTask _gdbSyncTask;

        // Job used to generate the geodatabase.
        private GenerateGeodatabaseReplicaJob _generateGdbJob;

        public GenerateGeodatabaseReplica()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                // Create a tile cache and load it with the SanFrancisco streets tpk.
                TileCache _tileCache = new TileCache(DataManager.GetDataFolder("e4a398afe9a945f3b0f4dca1e4faccb5", "SanFrancisco.tpkx"));

                // Create the corresponding layer based on the tile cache.
                ArcGISTiledLayer _tileLayer = new ArcGISTiledLayer(_tileCache);

                // Create the basemap based on the tile cache.
                Basemap _sfBasemap = new Basemap(_tileLayer);

                // Create the map with the tile-based basemap.
                Map myMap = new Map(_sfBasemap);

                // Assign the map to the MapView.
                MyMapView.Map = myMap;

                // Create a new symbol for the extent graphic.
                SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Red, 2);

                // Create a graphics overlay for the extent graphic and apply a renderer.
                GraphicsOverlay extentOverlay = new GraphicsOverlay
                {
                    Renderer = new SimpleRenderer(lineSymbol)
                };

                // Add graphics overlay to the map view.
                MyMapView.GraphicsOverlays.Add(extentOverlay);

                // Set up an event handler for when the viewpoint (extent) changes.
                MyMapView.ViewpointChanged += MapViewExtentChanged;

                // Create a task for generating a geodatabase (GeodatabaseSyncTask).
                _gdbSyncTask = await GeodatabaseSyncTask.CreateAsync(_featureServiceUri);

                // Add all layers from the service to the map.
                foreach (IdInfo layer in _gdbSyncTask.ServiceInfo.LayerInfos)
                {
                    // Create the ServiceFeatureTable for this particular layer.
                    ServiceFeatureTable onlineTable = new ServiceFeatureTable(new Uri(_featureServiceUri + "/" + layer.Id));

                    // Wait for the table to load.
                    await onlineTable.LoadAsync();

                    // Add the layer to the map's operational layers if load succeeds.
                    if (onlineTable.LoadStatus == Esri.ArcGISRuntime.LoadStatus.Loaded)
                    {
                        myMap.OperationalLayers.Add(new FeatureLayer(onlineTable));
                    }
                }

                // Update the extent graphic so that it is valid before user interaction.
                UpdateMapExtent();

                // Enable the generate button now that the sample is ready.
                GenerateButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                ShowStatusMessage(ex.ToString());
            }
        }

        private void UpdateMapExtent()
        {
            // Return if mapview is null.
            if (MyMapView == null) { return; }

            // Get the new viewpoint.
            Viewpoint myViewPoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);

            // Return if viewpoint is null.
            if (myViewPoint == null) { return; }

            // Get the updated extent for the new viewpoint.
            Envelope extent = myViewPoint.TargetGeometry as Envelope;

            // Return if extent is null.
            if (extent == null) { return; }

            // Create an envelope that is a bit smaller than the extent.
            EnvelopeBuilder envelopeBldr = new EnvelopeBuilder(extent);
            envelopeBldr.Expand(0.80);

            // Get the (only) graphics overlay in the map view.
            GraphicsOverlay extentOverlay = MyMapView.GraphicsOverlays.FirstOrDefault();

            // Return if the extent overlay is null.
            if (extentOverlay == null) { return; }

            // Get the extent graphic.
            Graphic extentGraphic = extentOverlay.Graphics.FirstOrDefault();

            // Create the extent graphic and add it to the overlay if it doesn't exist.
            if (extentGraphic == null)
            {
                extentGraphic = new Graphic(envelopeBldr.ToGeometry());
                extentOverlay.Graphics.Add(extentGraphic);
            }
            else
            {
                // Otherwise, update the graphic's geometry.
                extentGraphic.Geometry = envelopeBldr.ToGeometry();
            }
        }

        private async Task StartGeodatabaseGeneration()
        {
            // Update the geodatabase path.
            _gdbPath = $"{Path.GetTempFileName()}.geodatabase";

            // Create a task for generating a geodatabase (GeodatabaseSyncTask).
            _gdbSyncTask = await GeodatabaseSyncTask.CreateAsync(_featureServiceUri);

            // Get the current extent of the red preview box.
            Envelope extent = MyMapView.GraphicsOverlays[0].Graphics.First().Geometry as Envelope;

            // Get the default parameters for the generate geodatabase task.
            GenerateGeodatabaseReplicaParameters generateParams = await _gdbSyncTask.CreateDefaultGenerateGeodatabaseReplicaParametersAsync(extent);

            // Create a generate geodatabase job.
            _generateGdbJob = _gdbSyncTask.GenerateGeodatabaseReplica(generateParams, _gdbPath);

            // Handle the progress changed event (to show progress bar).
            _generateGdbJob.ProgressChanged += (sender, e) =>
            {
                UpdateProgressBar();
            };

            // Show the progress bar.
            GenerateProgressBar.Visibility = Visibility.Visible;

            // Start the job.
            _generateGdbJob.Start();

            // Get the result.
            Geodatabase resultGdb = await _generateGdbJob.GetResultAsync();

            // Hide the progress bar.
            GenerateProgressBar.Visibility = Visibility.Collapsed;

            // Do the rest of the work.
            await HandleGenerationCompleted(resultGdb);
        }

        private async Task HandleGenerationCompleted(Geodatabase resultGdb)
        {
            // If the job completed successfully, add the geodatabase data to the map.
            if (_generateGdbJob.Status == JobStatus.Succeeded)
            {
                // Clear out the existing layers.
                MyMapView.Map.OperationalLayers.Clear();

                // Loop through all feature tables in the geodatabase and add a new layer to the map.
                foreach (GeodatabaseFeatureTable table in resultGdb.GeodatabaseFeatureTables)
                {
                    // Create a new feature layer for the table.
                    FeatureLayer _layer = new FeatureLayer(table);

                    // Add the new layer to the map.
                    MyMapView.Map.OperationalLayers.Add(_layer);
                }
                // Best practice is to unregister the geodatabase.
                await _gdbSyncTask.UnregisterGeodatabaseAsync(resultGdb);

                // Tell the user that the geodatabase was unregistered.
                ShowStatusMessage("Since no edits will be made, the local geodatabase has been unregistered per best practice.");

                // Re-enable the generate button.
                GenerateButton.IsEnabled = true;
            }

            // See if the job failed.
            if (_generateGdbJob.Status == JobStatus.Failed)
            {
                // Create a message to show the user.
                string message = "Generate geodatabase job failed";

                // Show an error message (if there is one).
                if (_generateGdbJob.Error != null)
                {
                    message += ": " + _generateGdbJob.Error.Message;
                }
                else
                {
                    // If no error, show messages from the job.
                    message += ": " + string.Join("\n", _generateGdbJob.Messages.Select(m => m.Message));
                }

                ShowStatusMessage(message);
            }
        }

        private void ShowStatusMessage(string message)
        {
            // Display the message to the user.
            _ = new MessageDialog2(message).ShowAsync();
        }

        private async void GenerateButton_Clicked(object sender, RoutedEventArgs e)
        {
            // Fix the extent of the graphic.
            MyMapView.ViewpointChanged -= MapViewExtentChanged;

            // Disable the generate button.
            try
            {
                GenerateButton.IsEnabled = false;

                // Call the geodatabase generation method.
                await StartGeodatabaseGeneration();
            }
            catch (Exception ex)
            {
                ShowStatusMessage(ex.ToString());
            }
        }

        private void MapViewExtentChanged(object sender, EventArgs e)
        {
            // Call the map extent update method.
            UpdateMapExtent();
        }

        private void UpdateProgressBar()
        {
            // Due to the nature of the threading implementation,
            //     the dispatcher needs to be used to interact with the UI.
            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
            {
                // Update the progress bar value.
                GenerateProgressBar.Value = _generateGdbJob.Progress;
            });
        }
    }
}