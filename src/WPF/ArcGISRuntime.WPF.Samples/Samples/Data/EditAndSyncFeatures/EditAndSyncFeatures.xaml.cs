// Copyright 2017 Esri.
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ArcGISRuntime.WPF.Samples.EditAndSyncFeatures
{
    public partial class EditAndSyncFeatures
    {
        // Enumeration to track which phase of the workflow the sample is in
        private enum EditState
        {
            NotReady, // Geodatabase has not yet been generated
            Editing, // A feature is in the process of being moved
            Ready // The geodatabase is ready for synchronization or further edits
        }

        // URI for a feature service that supports geodatabase generation
        private Uri _featureServiceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Sync/WildfireSync/FeatureServer");

        // Path to the geodatabase file on disk
        private string _gdbPath;

        // Task to be used for generating the geodatabase
        private GeodatabaseSyncTask _gdbSyncTask;

        // Flag to indicate which stage of the edit process we're in
        private EditState _readyForEdits = EditState.NotReady;

        // Hold a reference to the generated geodatabase
        private Geodatabase _resultGdb;

        public EditAndSyncFeatures()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private async void Initialize()
        {
            // Create a tile cache and load it with the SanFrancisco streets tpk
            TileCache _tileCache = new TileCache(await GetTpkPath());

            // Create the corresponding layer based on the tile cache
            ArcGISTiledLayer _tileLayer = new ArcGISTiledLayer(_tileCache);

            // Create the basemap based on the tile cache
            Basemap _sfBasemap = new Basemap(_tileLayer);

            // Create the map with the tile-based basemap
            Map myMap = new Map(_sfBasemap);

            // Assign the map to the MapView
            MyMapView.Map = myMap;

            // Create a new symbol for the extent graphic
            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.Red, 2);

            // Create graphics overlay for the extent graphic and apply a renderer
            GraphicsOverlay extentOverlay = new GraphicsOverlay();
            extentOverlay.Renderer = new SimpleRenderer(lineSymbol);

            // Add graphics overlay to the map view
            MyMapView.GraphicsOverlays.Add(extentOverlay);

            // Set up an event handler for when the viewpoint (extent) changes
            MyMapView.ViewpointChanged += MapViewExtentChanged;

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

        private async void GeoViewTapped(object sender, Esri.ArcGISRuntime.UI.Controls.GeoViewInputEventArgs e)
        {
            // Disregard if not ready for edits
            if (_readyForEdits == EditState.NotReady) { return; }

            // If an edit is in process, finish it
            if (_readyForEdits == EditState.Editing)
            {
                // Hold a list of any selected features
                List<Feature> selectedFeatures = new List<Feature>();

                // Get all selected features then clear selection
                foreach (FeatureLayer layer in MyMapView.Map.OperationalLayers)
                {
                    // Get the selected features
                    FeatureQueryResult layerFeatures = await layer.GetSelectedFeaturesAsync();

                    // FeatureQueryResult implements IEnumerable, so it can be treated as a collection of features
                    selectedFeatures.AddRange(layerFeatures);

                    // Clear the selection
                    layer.ClearSelection();
                }

                // Update all selected features' geometry
                foreach (Feature feature in selectedFeatures)
                {
                    // Get a reference to the correct feature table for the feature
                    GeodatabaseFeatureTable table = feature.FeatureTable as GeodatabaseFeatureTable;

                    // Ensure the geometry type of the table is point
                    if (table.GeometryType != GeometryType.Point)
                    {
                        continue;
                    }

                    // Set the new geometry
                    feature.Geometry = e.Location;

                    // Update the feature in the table
                    await table.UpdateFeatureAsync(feature);
                }

                // Update the edit state
                _readyForEdits = EditState.Ready;

                // Enable the sync button
                MySyncButton.IsEnabled = true;
            }
            // Otherwise, start an edit
            else
            {
                // Define a tolerance for use with identifying the feature
                double tolerance = 15 * MyMapView.UnitsPerPixel;

                // Define the selection envelope
                Envelope selectionEnvelope = new Envelope(e.Location.X - tolerance, e.Location.Y - tolerance, e.Location.X + tolerance, e.Location.Y + tolerance);

                // Define query parameters for feature selection
                QueryParameters query = new QueryParameters()
                {
                    Geometry = selectionEnvelope
                };

                // Select the feature in all applicable tables
                foreach (FeatureLayer layer in MyMapView.Map.OperationalLayers)
                {
                    await layer.SelectFeaturesAsync(query, SelectionMode.New);
                }

                // Set the edit state
                _readyForEdits = EditState.Editing;
            }
        }

        private void UpdateMapExtent()
        {
            // Return if mapview is null
            if (MyMapView == null) { return; }

            // Get the new viewpoint
            Viewpoint myViewPoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);

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
            var extentOverlay = MyMapView.GraphicsOverlays.FirstOrDefault();

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
            Envelope extent = MyMapView.GraphicsOverlays.FirstOrDefault().Extent as Envelope;

            // Get the default parameters for the generate geodatabase task
            GenerateGeodatabaseParameters generateParams = await _gdbSyncTask.CreateDefaultGenerateGeodatabaseParametersAsync(extent);

            // Create a generate geodatabase job
            GenerateGeodatabaseJob _generateGdbJob = _gdbSyncTask.GenerateGeodatabase(generateParams, _gdbPath);

            // Handle the job changed event
            _generateGdbJob.JobChanged += GenerateGdbJobChanged;

            // Handle the progress changed event (to show progress bar)
            _generateGdbJob.ProgressChanged += ((object sender, EventArgs e) =>
            {
                // Get the job
                GenerateGeodatabaseJob job = sender as GenerateGeodatabaseJob;

                // Update the progress bar
                UpdateProgressBar(job.Progress);
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
                MyMapView.Map.OperationalLayers.Clear();

                // Get the new geodatabase
                _resultGdb = await job.GetResultAsync();

                // Loop through all feature tables in the geodatabase and add a new layer to the map
                foreach (GeodatabaseFeatureTable table in _resultGdb.GeodatabaseFeatureTables)
                {
                    // Create a new feature layer for the table
                    FeatureLayer _layer = new FeatureLayer(table);

                    // Add the new layer to the map
                    MyMapView.Map.OperationalLayers.Add(_layer);
                }

                // Enable editing features
                _readyForEdits = EditState.Ready;
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

                // Show the message
                ShowStatusMessage(message);
            }
        }

        private void HandleSyncStatusChange(SyncGeodatabaseJob job)
        {
            JobStatus status = job.Status;

            // Tell the user about job completion
            if (status == JobStatus.Succeeded)
            {
                ShowStatusMessage("Sync task completed");
            }

            // See if the job failed
            if (status == JobStatus.Failed)
            {
                // Create a message to show the user
                string message = "Sync geodatabase job failed";

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

                // Show the message
                ShowStatusMessage(message);
            }
        }

        private void SyncGeodatabase()
        {
            // Return if not ready
            if (_readyForEdits != EditState.Ready) { return; }

            // Create parameters for the sync task
            SyncGeodatabaseParameters parameters = new SyncGeodatabaseParameters()
            {
                GeodatabaseSyncDirection = SyncDirection.Bidirectional,
                RollbackOnFailure = false
            };

            // Add each layer to the sync job
            foreach (GeodatabaseFeatureTable table in _resultGdb.GeodatabaseFeatureTables)
            {
                // Get the ID for the layer
                long id = table.ServiceLayerId;

                // Create the SyncLayerOption
                SyncLayerOption option = new SyncLayerOption(id);

                // Add the option
                parameters.LayerOptions.Add(option);
            }

            // Create job
            SyncGeodatabaseJob job = _gdbSyncTask.SyncGeodatabase(parameters, _resultGdb);

            // Subscribe to status updates
            job.JobChanged += Job_JobChanged;

            // Subscribe to progress updates
            job.ProgressChanged += Job_ProgressChanged;

            // Start the sync
            
            job.Start();
        }

        private void Job_ProgressChanged(object sender, EventArgs e)
        {
            // Get the job object
            SyncGeodatabaseJob job = sender as SyncGeodatabaseJob;

            // Update the progress bar
            UpdateProgressBar(job.Progress);
        }

        // Get the path to the tile package used for the basemap
        private async Task<string> GetTpkPath()
        {
            // The desired tpk is expected to be called SanFrancisco.tpk
            string filename = "SanFrancisco.tpk";

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "GenerateGeodatabase", filename);

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                // Download the map package file
                await DataManager.GetData("3f1bbf0ec70b409a975f5c91f363fe7d", "GenerateGeodatabase");
            }
            return filepath;
        }

        private string GetGdbPath()
        {
            // Return the WPF-specific path for storing the geodatabase
            return Environment.ExpandEnvironmentVariables("%TEMP%\\wildfire.geodatabase");
        }

        private void ShowStatusMessage(string message)
        {
            // Display the message to the user
            MessageBox.Show(message);
        }

        // Handler for the generate button clicked event
        private void GenerateButton_Clicked(object sender, RoutedEventArgs e)
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
            this.Dispatcher.Invoke(() =>
            {
                // Hide the progress bar if the job is finished
                if (job.Status == JobStatus.Succeeded || job.Status == JobStatus.Failed)
                {
                    MyProgressBar.Visibility = Visibility.Collapsed;
                }
                else // Show it otherwise
                {
                    MyProgressBar.Visibility = Visibility.Visible;
                }

                // Do the remainder of the job status changed work
                HandleGenerationStatusChange(job);
            });
        }

        private void UpdateProgressBar(int progress)
        {
            // Due to the nature of the threading implementation,
            //     the dispatcher needs to be used to interact with the UI
            this.Dispatcher.Invoke(() =>
            {
                // Update the progress bar value
                MyProgressBar.Value = progress;
            });
        }

        private void SyncButton_Click(object sender, RoutedEventArgs e)
        {
            SyncGeodatabase();
        }

        private void Job_JobChanged(object sender, EventArgs e)
        {
            // Get the job object; will be passed to HandleGenerationStatusChange
            SyncGeodatabaseJob job = sender as SyncGeodatabaseJob;

            // Due to the nature of the threading implementation,
            //     the dispatcher needs to be used to interact with the UI
            this.Dispatcher.Invoke(() =>
            {
                // Update the progress bar as appropriate
                if (job.Status == JobStatus.Succeeded || job.Status == JobStatus.Failed)
                {
                    // Update the progress bar's value
                    UpdateProgressBar(0);

                    // Hide the progress bar
                    MyProgressBar.Visibility = Visibility.Collapsed;
                }
                else
                {
                    // Show the progress bar
                    MyProgressBar.Visibility = Visibility.Visible;
                }

                // Do the remainder of the job status changed work
                HandleSyncStatusChange(job);
            });
        }
    }
}