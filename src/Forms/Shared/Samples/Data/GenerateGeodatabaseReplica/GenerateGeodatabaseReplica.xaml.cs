// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Colors = System.Drawing.Color;

namespace ArcGISRuntime.Samples.GenerateGeodatabaseReplica
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Generate geodatabase replica from feature service",
        category: "Data",
        description: "Generate a local geodatabase from an online feature service.",
        instructions: "Zoom to any extent. Then tap the generate button to generate a geodatabase of features from a feature service filtered to the current extent. A red outline will show the extent used. The job's progress is shown while the geodatabase is generated.",
        tags: new[] { "disconnected", "local geodatabase", "offline", "replica", "sync" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("e4a398afe9a945f3b0f4dca1e4faccb5")]
    public partial class GenerateGeodatabaseReplica : ContentPage
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

            // Create the UI, setup the control references and execute initialization.
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
                myMapView.Map = myMap;

                // Create a new symbol for the extent graphic.
                SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.Red, 2);

                // Create graphics overlay for the extent graphic and apply a renderer.
                GraphicsOverlay extentOverlay = new GraphicsOverlay
                {
                    Renderer = new SimpleRenderer(lineSymbol)
                };

                // Add graphics overlay to the map view.
                myMapView.GraphicsOverlays.Add(extentOverlay);

                // Set up an event handler for when the viewpoint (extent) changes.
                myMapView.ViewpointChanged += MapViewExtentChanged;

                // Create a task for generating a geodatabase (GeodatabaseSyncTask).
                _gdbSyncTask = await GeodatabaseSyncTask.CreateAsync(_featureServiceUri);

                // Add all graphics from the service to the map.
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

                // Update the graphic - needed in case the user decides not to interact before pressing the button.
                UpdateMapExtent();

                // Enable the generate button now that the sample is ready.
                myGenerateButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private void UpdateMapExtent()
        {
            // Return if mapview is null.
            if (myMapView == null) { return; }

            // Get the new viewpoint.
            Viewpoint myViewPoint = myMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);

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
            GraphicsOverlay extentOverlay = myMapView.GraphicsOverlays.FirstOrDefault();

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
            Envelope extent = myMapView.GraphicsOverlays[0].Graphics.First().Geometry as Envelope;

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
            myProgressBar.IsVisible = true;

            // Start the job.
            _generateGdbJob.Start();

            // Get the result.
            Geodatabase resultGdb = await _generateGdbJob.GetResultAsync();

            // Hide the progress bar.
            myProgressBar.IsVisible = false;

            // Do the rest of the work.
            await HandleGenerationStatusChange(_generateGdbJob, resultGdb);
        }

        private async Task HandleGenerationStatusChange(GenerateGeodatabaseReplicaJob job, Geodatabase resultGdb)
        {
            // If the job completed successfully, add the geodatabase data to the map.
            if (job.Status == JobStatus.Succeeded)
            {
                // Clear out the existing layers.
                myMapView.Map.OperationalLayers.Clear();

                // Loop through all feature tables in the geodatabase and add a new layer to the map.
                foreach (GeodatabaseFeatureTable table in resultGdb.GeodatabaseFeatureTables)
                {
                    // Create a new feature layer for the table.
                    FeatureLayer _layer = new FeatureLayer(table);

                    // Add the new layer to the map.
                    myMapView.Map.OperationalLayers.Add(_layer);
                }
                // Best practice is to unregister the geodatabase.
                await _gdbSyncTask.UnregisterGeodatabaseAsync(resultGdb);

                // Tell the user that the geodatabase was unregistered.
                await Application.Current.MainPage.DisplayAlert("Alert", "Since no edits will be made, the local geodatabase has been unregistered per best practice.", "OK");

                // Re-enable generate button.
                myGenerateButton.IsEnabled = true;
            }

            // See if the job failed.
            if (job.Status == JobStatus.Failed)
            {
                // Create a message to show the user.
                string message = "Generate geodatabase job failed";

                // Show an error message (if there is one).
                if (job.Error != null)
                {
                    message += ": " + job.Error.Message;
                }
                else
                {
                    // If no error, show messages from the job.
                    message += ": " + String.Join("\n", job.Messages.Select(m => m.Message));
                }

                await Application.Current.MainPage.DisplayAlert("Alert", message, "OK");
            }
        }

        private async void GenerateButton_Clicked(object sender, EventArgs e)
        {
            // Fix the extent of the graphic.
            myMapView.ViewpointChanged -= MapViewExtentChanged;

            // Disable the generate button.
            try
            {
                myGenerateButton.IsEnabled = false;

                // Call the geodatabase generation method.
                await StartGeodatabaseGeneration();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.ToString(), "OK");
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
            Device.BeginInvokeOnMainThread(() =>
            {
                // Update the progress bar value.
                myProgressBar.Progress = _generateGdbJob.Progress / 100.0;
            });
        }
    }
}