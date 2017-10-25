// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using ArcGISRuntimeXamarin.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.ArcGISServices;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ArcGISRuntimeXamarin.Samples.EditAndSyncFeatures
{
    [Activity]
    public class EditAndSyncFeatures : Activity
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

        // Mapview
        private MapView myMapView;

        // Generate Button
        private Button myGenerateButton;

        // Sync Button
        private Button mySyncButton;

        // Progress bar
        private ProgressBar myProgressBar;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Title = "Edit and Sync Features";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        private void CreateLayout()
        {
            // Create the layout
            LinearLayout layout = new LinearLayout(this);
            layout.Orientation = Orientation.Vertical;

            // Add the progress bar
            myProgressBar = new ProgressBar(this);
            myProgressBar.Visibility = Android.Views.ViewStates.Gone;
            layout.AddView(myProgressBar);

            // Add the generate button
            myGenerateButton = new Button(this);
            myGenerateButton.Text = "Generate";
            myGenerateButton.Click += GenerateButton_Clicked;
            layout.AddView(myGenerateButton);

            // Add the sync button
            mySyncButton = new Button(this);
            mySyncButton.Text = "Synchronize";
            mySyncButton.Click += SyncButton_Click;
            layout.AddView(mySyncButton);

            // Add the mapview
            myMapView = new MapView(this);
            layout.AddView(myMapView);

            // Add the layout to the view
            SetContentView(layout);
        }

        private async void Initialize()
        {
            // Create a tile cache and load it with the SanFrancisco streets tpk
            TileCache tileCache = new TileCache(GetTpkPath());

            // Create the corresponding layer based on the tile cache
            ArcGISTiledLayer tileLayer = new ArcGISTiledLayer(tileCache);

            // Create the basemap based on the tile cache
            Basemap sfBasemap = new Basemap(tileLayer);

            // Create the map with the tile-based basemap
            Map myMap = new Map(sfBasemap);

            // Assign the map to the MapView
            myMapView.Map = myMap;

            // Create a new symbol for the extent graphic
            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Red, 2);

            // Create graphics overlay for the extent graphic and apply a renderer
            GraphicsOverlay extentOverlay = new GraphicsOverlay();
            extentOverlay.Renderer = new SimpleRenderer(lineSymbol);

            // Add graphics overlay to the map view
            myMapView.GraphicsOverlays.Add(extentOverlay);

            // Set up an event handler for when the viewpoint (extent) changes
            myMapView.ViewpointChanged += MapViewExtentChanged;

            // Set up event handler for mapview taps
            myMapView.GeoViewTapped += GeoViewTapped;

            // Update the local data path for the geodatabase file
            _gdbPath = GetFileStreamPath("wildfire.geodatabase").AbsolutePath;

            // Create a task for generating a geodatabase (GeodatabaseSyncTask)
            _gdbSyncTask = await GeodatabaseSyncTask.CreateAsync(_featureServiceUri);

            // Add all graphics from the service to the map
            foreach (IdInfo layer in _gdbSyncTask.ServiceInfo.LayerInfos)
            {
                // Get the Uri for this particular layer
                Uri onlineTableUri = new Uri(_featureServiceUri + "/" + layer.Id);

                // Create the ServiceFeatureTable
                ServiceFeatureTable onlineTable = new ServiceFeatureTable(onlineTableUri);

                // Wait for the table to load
                await onlineTable.LoadAsync();

                // Add the layer to the map's operational layers if load succeeds
                if (onlineTable.LoadStatus == Esri.ArcGISRuntime.LoadStatus.Loaded)
                {
                    myMap.OperationalLayers.Add(new FeatureLayer(onlineTable));
                }
            }
        }

        private async void GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Disregard if not ready for edits
            if (_readyForEdits == EditState.NotReady) { return; }

            // If an edit is in process, finish it
            if (_readyForEdits == EditState.Editing)
            {
                // Hold a list of any selected features
                List<Feature> selectedFeatures = new List<Feature>();

                // Get all selected features then clear selection
                foreach (FeatureLayer layer in myMapView.Map.OperationalLayers)
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
                mySyncButton.Enabled = true;
            }
            // Otherwise, start an edit
            else
            {
                // Define a tolerance for use with identifying the feature
                double tolerance = 15 * myMapView.UnitsPerPixel;

                // Define the selection envelope
                Envelope selectionEnvelope = new Envelope(e.Location.X - tolerance, e.Location.Y - tolerance, e.Location.X + tolerance, e.Location.Y + tolerance);

                // Define query parameters for feature selection
                QueryParameters query = new QueryParameters()
                {
                    Geometry = selectionEnvelope
                };

                // Select the feature in all applicable tables
                foreach (FeatureLayer layer in myMapView.Map.OperationalLayers)
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

            // Get the (only) graphic in the map view
            GraphicsOverlay redPreviewBox = myMapView.GraphicsOverlays.FirstOrDefault();

            // Get the current extent of the red preview box
            Envelope extent = redPreviewBox.Extent as Envelope;

            // Get the default parameters for the generate geodatabase task
            GenerateGeodatabaseParameters generateParams = await _gdbSyncTask.CreateDefaultGenerateGeodatabaseParametersAsync(extent);

            // Create a generate geodatabase job
            GenerateGeodatabaseJob generateGdbJob = _gdbSyncTask.GenerateGeodatabase(generateParams, _gdbPath);

            // Handle the job changed event
            generateGdbJob.JobChanged += GenerateGdbJobChanged;

            // Handle the progress changed event with an inline (lambda) function to show the progress bar
            generateGdbJob.ProgressChanged += ((object sender, EventArgs e) =>
            {
                // Get the job
                GenerateGeodatabaseJob job = sender as GenerateGeodatabaseJob;

                // Update the progress bar
                UpdateProgressBar(job.Progress);
            });

            // Start the job
            generateGdbJob.Start();
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
                _resultGdb = await job.GetResultAsync();

                // Loop through all feature tables in the geodatabase and add a new layer to the map
                foreach (GeodatabaseFeatureTable table in _resultGdb.GeodatabaseFeatureTables)
                {
                    // Create a new feature layer for the table
                    FeatureLayer layer = new FeatureLayer(table);

                    // Add the new layer to the map
                    myMapView.Map.OperationalLayers.Add(layer);
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
                    foreach (JobMessage m in job.Messages)
                    {
                        // Get the text from the JobMessage and add it to the output string
                        message += "\n" + m.Message;
                    }
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
                    foreach (JobMessage m in job.Messages)
                    {
                        // Get the text from the JobMessage and add it to the output string
                        message += "\n" + m.Message;
                    }
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

            // Get the layer Id for each feature table in the geodatabase, then add to the sync job
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
        // (this is plumbing for the sample viewer)
        private string GetTpkPath()
        {
            #region offlinedata
            // The desired tpk is expected to be called SanFrancisco.tpk
            string filename = "SanFrancisco.tpk";

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

			// Return the full path; the Item ID is 3f1bbf0ec70b409a975f5c91f363fe7d
			return Path.Combine(folder, "SampleData", "EditAndSyncFeatures", filename);
            #endregion offlinedata
        }

        private void ShowStatusMessage(string message)
        {
            // Display the message to the user
            var builder = new AlertDialog.Builder(this);
            builder.SetMessage(message).SetTitle("Alert").Show();
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
            // The dispatcher takes an Action, provided here as a lambda function
            RunOnUiThread(() =>
            {
                // Update progress bar visibility
                if (job.Status == JobStatus.Started)
                {
                    myProgressBar.Visibility = Android.Views.ViewStates.Visible;
                }
                else
                {
                    myProgressBar.Visibility = Android.Views.ViewStates.Gone;
                }
                // Do the remainder of the job status changed work
                HandleGenerationStatusChange(job);
            });
        }

        private void UpdateProgressBar(int progress)
        {
            // Due to the nature of the threading implementation,
            //     the dispatcher needs to be used to interact with the UI
            // The dispatcher takes an Action, provided here as a lambda function
            RunOnUiThread(() =>
            {
                // Update the progress bar value
                myProgressBar.Progress = progress;
            });
        }

        private void SyncButton_Click(object sender, EventArgs e)
        {
            SyncGeodatabase();
        }

        private void Job_JobChanged(object sender, EventArgs e)
        {
            // Get the job object; will be passed to HandleGenerationStatusChange
            SyncGeodatabaseJob job = sender as SyncGeodatabaseJob;

            // Due to the nature of the threading implementation,
            //     the dispatcher needs to be used to interact with the UI
            // The dispatcher takes an Action, provided here as a lambda function
            RunOnUiThread(() =>
            {
                // Update the progress bar as appropriate
                if (job.Status == JobStatus.Succeeded || job.Status == JobStatus.Failed)
                {
                    // Update the progress bar's value
                    UpdateProgressBar(0);

                    // Hide the progress bar
                    myProgressBar.Visibility = Android.Views.ViewStates.Gone;
                }
                else
                {
                    // Show the progress bar
                    myProgressBar.Visibility = Android.Views.ViewStates.Visible;
                }

                // Do the remainder of the job status changed work
                HandleSyncStatusChange(job);
            });
        }
    }
}
