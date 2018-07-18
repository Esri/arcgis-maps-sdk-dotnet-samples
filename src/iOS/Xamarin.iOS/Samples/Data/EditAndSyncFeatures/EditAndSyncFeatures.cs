// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ArcGISRuntime.Samples.Managers;
using CoreGraphics;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.EditAndSyncFeatures
{
    [Register("EditAndSyncFeatures")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("3f1bbf0ec70b409a975f5c91f363fe7d")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Edit and sync features",
        "Data",
        "This sample demonstrates how to synchronize offline edits with a feature service.",
        "1. Pan and zoom to the area you would like to download point features for, ensuring that all point features are within the rectangle.\n2. Tap the 'generate' button. This will start the process of generating the offline geodatabase.\n3. Tap on a point feature within the area of the generated geodatabase. Then tap on the screen (anywhere within the range of the local geodatabase) to move the point to that location.\n4. Tap the 'Sync Geodatabase' button to synchronize the changes back to the feature service.\n\n Note that the basemap for this sample is downloaded from ArcGIS Online automatically.")]
    public class EditAndSyncFeatures : UIViewController
    {
        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UIProgressView _progressBar = new UIProgressView();
        private readonly UIButton _syncButton = new UIButton();
        private readonly UILabel _helpLabel = new UILabel();
        private readonly UIToolbar _helpToolbar = new UIToolbar();
        private readonly UIToolbar _controlsToolbar = new UIToolbar();

        private readonly UIButton _generateButton = new UIButton
        {
            Enabled = false
        };

        // Enumeration to track which phase of the workflow the sample is in.
        private enum EditState
        {
            NotReady, // Geodatabase has not yet been generated.
            Editing, // A feature is in the process of being moved.
            Ready // The geodatabase is ready for synchronization or further edits.
        }

        // URL for a feature service that supports geodatabase generation.
        private readonly Uri _featureServiceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Sync/WildfireSync/FeatureServer");

        // Path to the geodatabase file on disk.
        private string _gdbPath;

        // Task to be used for generating the geodatabase.
        private GeodatabaseSyncTask _gdbSyncTask;

        // Flag to indicate which stage of the edit process the sample is in.
        private EditState _readyForEdits = EditState.NotReady;

        // Hold a reference to the generated geodatabase.
        private Geodatabase _resultGdb;

        public EditAndSyncFeatures()
        {
            Title = "Edit and Sync Features";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat pageOffset = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat margin = 5;
                nfloat controlHeight = 30;

                // Reposition the controls.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _helpToolbar.Frame = new CGRect(0, pageOffset, View.Bounds.Width, controlHeight + 2 * margin);
                _controlsToolbar.Frame = new CGRect(0, View.Bounds.Height - controlHeight - 2 * margin, View.Bounds.Width, controlHeight + 2 * margin);
                _generateButton.Frame = new CGRect(margin, View.Bounds.Height - controlHeight - margin, View.Bounds.Width / 2 - 2 * margin, controlHeight);
                _syncButton.Frame = new CGRect(View.Bounds.Width / 2 + margin, View.Bounds.Height - controlHeight - margin, View.Bounds.Width / 2 - 2 * margin, controlHeight);
                _progressBar.Frame = new CGRect(0, View.Bounds.Height - 42, View.Bounds.Width, 2);
                _helpLabel.Frame = new CGRect(margin, pageOffset + margin, View.Bounds.Width - 2 * margin, controlHeight);
                _myMapView.ViewInsets = new UIEdgeInsets(_helpToolbar.Frame.Bottom, 0, _controlsToolbar.Frame.Height, 0);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private void CreateLayout()
        {
            // Update the Generate Button.
            _generateButton.SetTitle("Generate", UIControlState.Normal);
            _generateButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _generateButton.SetTitleColor(UIColor.Gray, UIControlState.Disabled);
            _generateButton.TouchUpInside += GenerateButton_Clicked;

            // Update the Sync Button.
            _syncButton.SetTitle("Synchronize", UIControlState.Normal);
            _syncButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _syncButton.SetTitleColor(UIColor.Gray, UIControlState.Disabled);
            _syncButton.TouchUpInside += SyncButton_Click;
            _syncButton.Enabled = false;

            // Update the help label.
            _helpLabel.Text = "1. Tap 'Generate'.";
            _helpLabel.TextAlignment = UITextAlignment.Center;
            _helpLabel.Lines = 1;
            _helpLabel.AdjustsFontSizeToFitWidth = true;

            // Add the views.
            View.AddSubviews(_myMapView, _helpToolbar, _controlsToolbar, _syncButton, _generateButton, _helpLabel);
        }

        private async void Initialize()
        {
            try
            {
                // Create a tile cache and load it with the SanFrancisco streets tpk.
                TileCache tileCache = new TileCache(DataManager.GetDataFolder("3f1bbf0ec70b409a975f5c91f363fe7d", "SanFrancisco.tpk"));

                // Create the corresponding layer based on the tile cache.
                ArcGISTiledLayer tileLayer = new ArcGISTiledLayer(tileCache);

                // Create the basemap based on the tile cache.
                Basemap sfBasemap = new Basemap(tileLayer);

                // Create the map with the tile-based basemap.
                Map map = new Map(sfBasemap);

                // Assign the map to the MapView.
                _myMapView.Map = map;

                // Create a new symbol for the extent graphic.
                SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Red, 2);

                // Create graphics overlay for the extent graphic and apply a renderer.
                GraphicsOverlay extentOverlay = new GraphicsOverlay
                {
                    Renderer = new SimpleRenderer(lineSymbol)
                };

                // Add graphics overlay to the map view.
                _myMapView.GraphicsOverlays.Add(extentOverlay);

                // Set up an event handler for 'tapped' events.
                _myMapView.GeoViewTapped += GeoViewTapped;

                // Set up an event handler for when the viewpoint (extent) changes.
                _myMapView.ViewpointChanged += MapViewExtentChanged;

                // Create a task for generating a geodatabase (GeodatabaseSyncTask).
                _gdbSyncTask = await GeodatabaseSyncTask.CreateAsync(_featureServiceUri);

                // Add all graphics from the service to the map.
                foreach (IdInfo layer in _gdbSyncTask.ServiceInfo.LayerInfos)
                {
                    // Get the Uri for this particular layer.
                    Uri onlineTableUri = new Uri(_featureServiceUri + "/" + layer.Id);

                    // Create the ServiceFeatureTable.
                    ServiceFeatureTable onlineTable = new ServiceFeatureTable(onlineTableUri);

                    // Wait for the table to load.
                    await onlineTable.LoadAsync();

                    // Add the layer to the map's operational layers if load succeeds.
                    if (onlineTable.LoadStatus == Esri.ArcGISRuntime.LoadStatus.Loaded)
                    {
                        map.OperationalLayers.Add(new FeatureLayer(onlineTable));
                    }
                }

                // Update the graphic - needed in case the user decides not to interact before pressing the button.
                UpdateMapExtent();

                // Enable the generate button now that the sample is ready.
                _generateButton.Enabled = true;
            }
            catch (Exception ex)
            {
                ShowStatusMessage(ex.ToString());
            }
        }

        private async void GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                switch (_readyForEdits)
                {
                    // Disregard if not ready for edits.
                    case EditState.NotReady:
                        return;
                    // If an edit is in process, finish it.
                    case EditState.Editing:
                        // Hold a list of any selected features.
                        List<Feature> selectedFeatures = new List<Feature>();

                        // Get all selected features then clear selection.
                        foreach (FeatureLayer layer in _myMapView.Map.OperationalLayers)
                        {
                            // Get the selected features.
                            FeatureQueryResult layerFeatures = await layer.GetSelectedFeaturesAsync();

                            // FeatureQueryResult implements IEnumerable, so it can be treated as a collection of features.
                            selectedFeatures.AddRange(layerFeatures);

                            // Clear the selection.
                            layer.ClearSelection();
                        }

                        // Update all selected features' geometry.
                        foreach (Feature feature in selectedFeatures)
                        {
                            // Get a reference to the correct feature table for the feature.
                            GeodatabaseFeatureTable table = (GeodatabaseFeatureTable)feature.FeatureTable;

                            // Ensure the geometry type of the table is point.
                            if (table.GeometryType != GeometryType.Point)
                            {
                                continue;
                            }

                            // Set the new geometry.
                            feature.Geometry = e.Location;
                            try
                            {
                                // Update the feature in the table.
                                await table.UpdateFeatureAsync(feature);
                            }
                            catch (Esri.ArcGISRuntime.ArcGISException)
                            {
                                ShowStatusMessage("Feature must be within extent of geodatabase.");
                            }
                        }

                        // Update the edit state.
                        _readyForEdits = EditState.Ready;

                        // Enable the sync button.
                        _syncButton.Enabled = true;

                        // Update the help label.
                        _helpLabel.Text = "4. Tap 'Synchronize' or edit more features.";
                        break;
                    // Otherwise, start an edit.
                    case EditState.Ready:
                        // Define a tolerance for use with identifying the feature.
                        double tolerance = 15 * _myMapView.UnitsPerPixel;

                        // Define the selection envelope.
                        Envelope selectionEnvelope = new Envelope(e.Location.X - tolerance, e.Location.Y - tolerance, e.Location.X + tolerance, e.Location.Y + tolerance);

                        // Define query parameters for feature selection.
                        QueryParameters query = new QueryParameters()
                        {
                            Geometry = selectionEnvelope
                        };

                        // Select the feature in all applicable tables.
                        foreach (FeatureLayer layer in _myMapView.Map.OperationalLayers)
                        {
                            await layer.SelectFeaturesAsync(query, SelectionMode.New);
                        }

                        // Set the edit state.
                        _readyForEdits = EditState.Editing;

                        // Update the help label.
                        _helpLabel.Text = "3. Tap on the map to move the point.";
                        break;
                }
            }
            catch (Exception ex)
            {
                ShowStatusMessage(ex.ToString());
            }
        }

        private void UpdateMapExtent()
        {
            // Get the new viewpoint.
            Viewpoint myViewPoint = _myMapView?.GetCurrentViewpoint(ViewpointType.BoundingGeometry);

            // Get the updated extent for the new viewpoint.
            Envelope extent = myViewPoint?.TargetGeometry as Envelope;

            // Return if extent is null.
            if (extent == null)
            {
                return;
            }

            // Create an envelope that is a bit smaller than the extent.
            EnvelopeBuilder envelopeBldr = new EnvelopeBuilder(extent);
            envelopeBldr.Expand(0.70);

            // Get the (only) graphics overlay in the map view.
            GraphicsOverlay extentOverlay = _myMapView.GraphicsOverlays.FirstOrDefault();

            // Return if the extent overlay is null.
            if (extentOverlay == null)
            {
                return;
            }

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
            _generateButton.Enabled = false;

            // Update geodatabase path.
            _gdbPath = $"{Path.GetTempFileName()}.geodatabase";

            // Create a task for generating a geodatabase (GeodatabaseSyncTask).
            _gdbSyncTask = await GeodatabaseSyncTask.CreateAsync(_featureServiceUri);

            // Get the (only) graphic in the map view.
            Graphic redPreviewBox = _myMapView.GraphicsOverlays.First().Graphics.First();

            // Get the current extent of the red preview box.
            Envelope extent = redPreviewBox.Geometry as Envelope;

            // Get the default parameters for the generate geodatabase task.
            GenerateGeodatabaseParameters generateParams = await _gdbSyncTask.CreateDefaultGenerateGeodatabaseParametersAsync(extent);

            // Create a generate geodatabase job.
            GenerateGeodatabaseJob generateGdbJob = _gdbSyncTask.GenerateGeodatabase(generateParams, _gdbPath);

            // Handle the progress changed event with an inline (lambda) function to show the progress bar.
            generateGdbJob.ProgressChanged += (sender, e) =>
            {
                // Get the job.
                GenerateGeodatabaseJob job = (GenerateGeodatabaseJob)sender;

                // Update the progress bar.
                UpdateProgressBar(job.Progress);
            };

            // Show the progress bar.
            View.AddSubview(_progressBar);

            // Start the job.
            generateGdbJob.Start();

            // Get the result.
            _resultGdb = await generateGdbJob.GetResultAsync();

            // Do the rest of the work.
            HandleGenerationCompleted(generateGdbJob);
        }

        private void HandleGenerationCompleted(GenerateGeodatabaseJob job)
        {
            // Hide the progress bar.
            _progressBar.RemoveFromSuperview();

            switch (job.Status)
            {
                // If the job completed successfully, add the geodatabase data to the map.
                case JobStatus.Succeeded:
                    // Clear out the existing layers.
                    _myMapView.Map.OperationalLayers.Clear();

                    // Loop through all feature tables in the geodatabase and add a new layer to the map.
                    foreach (GeodatabaseFeatureTable table in _resultGdb.GeodatabaseFeatureTables)
                    {
                        // Create a new feature layer for the table.
                        FeatureLayer layer = new FeatureLayer(table);

                        // Add the new layer to the map.
                        _myMapView.Map.OperationalLayers.Add(layer);
                    }

                    // Enable editing features.
                    _readyForEdits = EditState.Ready;

                    // Update the help label.
                    _helpLabel.Text = "2. Tap a point feature to select.";
                    break;
                // See if the job failed.
                case JobStatus.Failed:
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
                        message = job.Messages.Aggregate(message, (current, m) => current + "\n" + m.Message);
                    }

                    // Show the message.
                    ShowStatusMessage(message);
                    break;
            }
        }

        private async Task SyncGeodatabase()
        {
            // Return if not ready.
            if (_readyForEdits != EditState.Ready)
            {
                return;
            }

            // Disable the sync button.
            _syncButton.Enabled = false;

            // Disable editing.
            _myMapView.GeoViewTapped -= GeoViewTapped;

            // Create parameters for the sync task.
            SyncGeodatabaseParameters parameters = new SyncGeodatabaseParameters
            {
                GeodatabaseSyncDirection = SyncDirection.Bidirectional,
                RollbackOnFailure = false
            };

            // Get the layer Id for each feature table in the geodatabase, then add to the sync job.
            foreach (GeodatabaseFeatureTable table in _resultGdb.GeodatabaseFeatureTables)
            {
                // Get the ID for the layer.
                long id = table.ServiceLayerId;

                // Create the SyncLayerOption.
                SyncLayerOption option = new SyncLayerOption(id);

                // Add the option.
                parameters.LayerOptions.Add(option);
            }

            // Create job.
            SyncGeodatabaseJob job = _gdbSyncTask.SyncGeodatabase(parameters, _resultGdb);

            // Subscribe to progress updates.
            job.ProgressChanged += (o, e) =>
            {
                // Update the progress bar.
                UpdateProgressBar(job.Progress);
            };

            // Show the progress bar.
            View.AddSubview(_progressBar);

            // Start the sync.
            job.Start();

            // Get the result.
            await job.GetResultAsync();

            // Do the rest of the work.
            HandleSyncCompleted(job);
        }

        private void HandleSyncCompleted(SyncGeodatabaseJob job)
        {
            // Hide the progress bar & enable the sync button.
            _progressBar.RemoveFromSuperview();
            _syncButton.Enabled = true;

            // Re-enable editing.
            _myMapView.GeoViewTapped += GeoViewTapped;

            switch (job.Status)
            {
                // Tell the user about job completion.
                case JobStatus.Succeeded:
                    ShowStatusMessage("Sync task completed");
                    break;
                // See if the job failed.
                case JobStatus.Failed:
                    // Create a message to show the user.
                    string message = "Sync geodatabase job failed";

                    // Show an error message (if there is one).
                    if (job.Error != null)
                    {
                        message += ": " + job.Error.Message;
                    }
                    else
                    {
                        // If no error, show messages from the job.
                        message = job.Messages.Aggregate(message, (current, m) => current + "\n" + m.Message);
                    }

                    // Show the message.
                    ShowStatusMessage(message);
                    break;
            }
        }

        private void ShowStatusMessage(string message)
        {
            // Display the message to the user.
            UIAlertView alertView = new UIAlertView("alert", message, (IUIAlertViewDelegate) null, "OK", null);
            alertView.Show();
        }

        private async void GenerateButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Disable the generate button.
                _generateButton.Enabled = false;

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

        private void UpdateProgressBar(int progress)
        {
            // Due to the nature of the threading implementation,
            //     the dispatcher needs to be used to interact with the UI.
            // The dispatcher takes an Action, provided here as a lambda function.
            InvokeOnMainThread(() =>
            {
                // Update the progress bar value.
                _progressBar.Progress = progress / 100.0f;
            });
        }

        private async void SyncButton_Click(object sender, EventArgs e)
        {
            try
            {
                await SyncGeodatabase();
            }
            catch (Exception ex)
            {
                ShowStatusMessage(ex.ToString());
            }
        }
    }
}