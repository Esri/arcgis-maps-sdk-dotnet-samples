// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// Language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Popups;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ArcGISRuntime.WinUI.Samples.GeodatabaseTransactions
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Geodatabase transactions",
        category: "Data",
        description: "Use transactions to manage how changes are committed to a geodatabase.",
        instructions: "When the sample loads, a feature service is taken offline as a geodatabase. When the geodatabase is ready, you can add multiple types of features. To apply edits directly, uncheck the 'Require a transaction for edits' checkbox. When using transactions, use the buttons to start editing and stop editing. When you stop editing, you can choose to commit the changes or roll them back. At any point, you can synchronize the local geodatabase with the feature service.",
        tags: new[] { "commit", "database", "geodatabase", "transact", "transactions" })]
    public partial class GeodatabaseTransactions
    {
        // URL for the editable feature service
        private const string SyncServiceUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Sync/SaveTheBaySync/FeatureServer/";

        // Work in a small extent south of Galveston, TX
        private Envelope _extent = new Envelope(-95.3035, 29.0100, -95.1053, 29.1298, SpatialReferences.Wgs84);

        // Store the local geodatabase to edit
        private Geodatabase _localGeodatabase;

        // Store references to two tables to edit: Birds and Marine points
        private GeodatabaseFeatureTable _birdTable;
        private GeodatabaseFeatureTable _marineTable;

        public GeodatabaseTransactions()
        {
            InitializeComponent();

            // When the map view loads, add a new map
            MyMapView.Loaded += (s, e) =>
            {
                // Create a new map with the oceans basemap and add it to the map view
                Map map = new Map(BasemapStyle.ArcGISOceans);
                MyMapView.Map = map;
            };

            // When the spatial reference changes (the map loads) add the local geodatabase tables as feature layers
            MyMapView.SpatialReferenceChanged += async (s, e) =>
            {
                // Call a function (and await it) to get the local geodatabase (or generate it from the feature service)
                await GetLocalGeodatabase();

                // Once the local geodatabase is available, load the tables as layers to the map
                LoadLocalGeodatabaseTables();
            };
        }

        private async Task GetLocalGeodatabase()
        {
            // Get the path to the local geodatabase for this platform (temp directory, for example)
            string localGeodatabasePath = GetGdbPath();

            try
            {
                // See if the geodatabase file is already present
                if (File.Exists(localGeodatabasePath))
                {
                    // If the geodatabase is already available, open it, hide the progress control, and update the message
                    _localGeodatabase = await Geodatabase.OpenAsync(localGeodatabasePath);
                    LoadingProgressBar.Visibility = Visibility.Collapsed;
                    MessageTextBlock.Text = "Using local geodatabase from '" + _localGeodatabase.Path + "'";
                }
                else
                {
                    // Create a new GeodatabaseSyncTask with the uri of the feature server to pull from
                    Uri uri = new Uri(SyncServiceUrl);
                    GeodatabaseSyncTask gdbTask = await GeodatabaseSyncTask.CreateAsync(uri);

                    // Create parameters for the task: layers and extent to include, out spatial reference, and sync model
                    GenerateGeodatabaseParameters gdbParams = await gdbTask.CreateDefaultGenerateGeodatabaseParametersAsync(_extent);
                    gdbParams.OutSpatialReference = MyMapView.SpatialReference;
                    gdbParams.SyncModel = SyncModel.Layer;
                    gdbParams.LayerOptions.Clear();
                    gdbParams.LayerOptions.Add(new GenerateLayerOption(0));
                    gdbParams.LayerOptions.Add(new GenerateLayerOption(1));

                    // Create a geodatabase job that generates the geodatabase
                    GenerateGeodatabaseJob generateGdbJob = gdbTask.GenerateGeodatabase(gdbParams, localGeodatabasePath);

                    // Handle the job changed event and check the status of the job; store the geodatabase when it's ready
                    generateGdbJob.StatusChanged += (s, e) =>
                    {
                        // See if the job succeeded
                        if (generateGdbJob.Status == JobStatus.Succeeded)
                        {
                            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
                            {
                                // Hide the progress control and update the message
                                LoadingProgressBar.Visibility = Visibility.Collapsed;
                                MessageTextBlock.Text = "Created local geodatabase";
                            });
                        }
                        else if (generateGdbJob.Status == JobStatus.Failed)
                        {
                            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
                            {
                                // Hide the progress control and report the exception
                                LoadingProgressBar.Visibility = Visibility.Collapsed;
                                MessageTextBlock.Text = "Unable to create local geodatabase: " + generateGdbJob.Error.Message;
                            });
                        }
                    };

                    // Start the generate geodatabase job
                    _localGeodatabase = await generateGdbJob.GetResultAsync();
                }
            }
            catch (Exception ex)
            {
                // Show a message for the exception encountered
                DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
                {
                    _ = new MessageDialog2("Unable to create offline database: " + ex.Message).ShowAsync();
                });
            }
        }

        // Function that loads the two point tables from the local geodatabase and displays them as feature layers
        private async void LoadLocalGeodatabaseTables()
        {
            if (_localGeodatabase == null) { return; }

            // Read the geodatabase tables and add them as layers
            foreach (GeodatabaseFeatureTable table in _localGeodatabase.GeodatabaseFeatureTables)
            {
                try
                {
                    // Load the table so the TableName can be read
                    await table.LoadAsync();

                    // Store a reference to the Birds table
                    if (table.TableName.ToLower().Contains("birds"))
                    {
                        _birdTable = table;
                    }

                    // Store a reference to the Marine table
                    if (table.TableName.ToLower().Contains("marine"))
                    {
                        _marineTable = table;
                    }

                    // Create a new feature layer to show the table in the map
                    FeatureLayer layer = new FeatureLayer(table);
                    DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () => MyMapView.Map.OperationalLayers.Add(layer));
                }
                catch (Exception e)
                {
                    await new MessageDialog2(e.ToString(), "Error").ShowAsync();
                }
            }

            // Handle the transaction status changed event
            _localGeodatabase.TransactionStatusChanged += GdbTransactionStatusChanged;

            // Zoom the map view to the extent of the generated local datasets
            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
            {
                MyMapView.SetViewpoint(new Viewpoint(_marineTable.Extent));
                StartEditingButton.IsEnabled = true;
            });
        }

        private void GdbTransactionStatusChanged(object sender, TransactionStatusChangedEventArgs e)
        {
            // Update UI controls based on whether the geodatabase has a current transaction
            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
            {
                // These buttons should be enabled when there IS a transaction
                AddBirdButton.IsEnabled = e.IsInTransaction;
                AddMarineButton.IsEnabled = e.IsInTransaction;
                StopEditingButton.IsEnabled = e.IsInTransaction;

                // These buttons should be enabled when there is NOT a transaction
                StartEditingButton.IsEnabled = !e.IsInTransaction;
                SyncEditsButton.IsEnabled = !e.IsInTransaction;
            });
        }

        private string GetGdbPath()
        {
            // Get the UWP-specific path for storing the geodatabase
            string folder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            return Path.Combine(folder, "savethebay.geodatabase");
        }

        private void BeginTransaction(object sender, RoutedEventArgs e)
        {
            // See if there is a transaction active for the geodatabase
            if (!_localGeodatabase.IsInTransaction)
            {
                // If not, begin a transaction
                _localGeodatabase.BeginTransaction();
                MessageTextBlock.Text = "Transaction started";
            }
        }

        private void AddNewFeature(object sender, RoutedEventArgs args)
        {
            _ = AddNewFeatureTask(sender, args);
        }

        private async Task AddNewFeatureTask(object sender, RoutedEventArgs args)
        {
            // See if it was the "Birds" or "Marine" button that was clicked
            Button addFeatureButton = (Button)sender;

            try
            {
                // Cancel execution of the sketch task if it is already active
                if (MyMapView.SketchEditor.CancelCommand.CanExecute(null))
                {
                    MyMapView.SketchEditor.CancelCommand.Execute(null);
                }

                // Store the correct table to edit (for the button clicked)
                GeodatabaseFeatureTable editTable = null;
                if (addFeatureButton == AddBirdButton)
                {
                    editTable = _birdTable;
                }
                else
                {
                    editTable = _marineTable;
                }

                // Inform the user which table is being edited
                MessageTextBlock.Text = "Click the map to add a new feature to the geodatabase table '" + editTable.TableName + "'";

                // Create a random value for the 'type' attribute (integer between 1 and 7)
                Random random = new Random(DateTime.Now.Millisecond);
                int featureType = random.Next(1, 7);

                // Use the sketch editor to allow the user to draw a point on the map
                MapPoint clickPoint = await MyMapView.SketchEditor.StartAsync(Esri.ArcGISRuntime.UI.SketchCreationMode.Point, false) as MapPoint;

                // Create a new feature (row) in the selected table
                Feature newFeature = editTable.CreateFeature();

                // Set the geometry with the point the user clicked and the 'type' with the random integer
                newFeature.Geometry = clickPoint;
                newFeature.SetAttributeValue("type", featureType);

                // Add the new feature to the table
                await editTable.AddFeatureAsync(newFeature);

                // Clear the message
                MessageTextBlock.Text = "New feature added to the '" + editTable.TableName + "' table";
            }
            catch (TaskCanceledException)
            {
                // Ignore if the edit was canceled
            }
            catch (Exception ex)
            {
                // Report other exception messages
                MessageTextBlock.Text = ex.Message;
            }
        }

        private void StopEditTransaction(object sender, RoutedEventArgs e)
        {
            _ = StopEditTransactionTask(sender, e);
        }

        private async Task StopEditTransactionTask(object sender, RoutedEventArgs e)
        {
            // Create a new dialog that prompts for commit, rollback, or cancel
            var promptDialog = new MessageDialog2("Commit your edits to the local geodatabase or rollback to discard them.", "Stop Editing");
            UICommand commitCommand = new UICommand("Commit");
            UICommand rollbackCommand = new UICommand("Rollback");
            UICommand cancelCommand = new UICommand("Cancel");
            promptDialog.Options = MessageDialogOptions.None;
            promptDialog.Commands.Add(commitCommand);
            promptDialog.Commands.Add(rollbackCommand);
            promptDialog.Commands.Add(cancelCommand);

            // Ask the user if they want to commit or rollback the transaction (or cancel to keep working in the transaction)
            IUICommand cmd = await promptDialog.ShowAsync();

            if (cmd.Label == commitCommand.Label)
            {
                // See if there is a transaction active for the geodatabase
                if (_localGeodatabase.IsInTransaction)
                {
                    // If there is, commit the transaction to store the edits (this will also end the transaction)
                    _localGeodatabase.CommitTransaction();
                    MessageTextBlock.Text = "Edits were committed to the local geodatabase.";
                }
            }
            else if (cmd.Label == rollbackCommand.Label)
            {
                // See if there is a transaction active for the geodatabase
                if (_localGeodatabase.IsInTransaction)
                {
                    // If there is, rollback the transaction to discard the edits (this will also end the transaction)
                    _localGeodatabase.RollbackTransaction();
                    MessageTextBlock.Text = "Edits were rolled back and not stored to the local geodatabase.";
                }
            }
            else
            {
                // For 'cancel' don't end the transaction with a commit or rollback
            }
        }

        // Change which controls are enabled if the user chooses to require/not require transactions for edits
        private void RequireTransactionChanged(object sender, RoutedEventArgs e)
        {
            _ = RequireTransactionChangedTask(sender, e);
        }

        private async Task RequireTransactionChangedTask(object sender, RoutedEventArgs e)
        {
            // If the local geodatabase isn't created yet, return
            if (_localGeodatabase == null) { return; }

            // Get the value of the "require transactions" checkbox
            bool mustHaveTransaction = RequireTransactionCheckBox.IsChecked == true;

            // Warn the user if disabling transactions while a transaction is active
            if (!mustHaveTransaction && _localGeodatabase.IsInTransaction)
            {
                await new MessageDialog2("Stop editing to end the current transaction.", "Current Transaction").ShowAsync();
                RequireTransactionCheckBox.IsChecked = true;
                return;
            }

            // Enable or disable controls according to the checkbox value
            StartEditingButton.IsEnabled = mustHaveTransaction;
            StopEditingButton.IsEnabled = mustHaveTransaction && _localGeodatabase.IsInTransaction;
            AddBirdButton.IsEnabled = !mustHaveTransaction;
            AddMarineButton.IsEnabled = !mustHaveTransaction;
        }

        // Synchronize edits in the local geodatabase with the service
        public void SynchronizeEdits(object sender, RoutedEventArgs e)
        {
            _ = SynchronizeEditsTask(sender, e);
        }

        public async Task SynchronizeEditsTask(object sender, RoutedEventArgs e)
        {
            // Show the progress bar while the sync is working
            LoadingProgressBar.Visibility = Visibility.Visible;

            try
            {
                // Create a sync task with the URL of the feature service to sync
                GeodatabaseSyncTask syncTask = await GeodatabaseSyncTask.CreateAsync(new Uri(SyncServiceUrl));

                // Create sync parameters
                SyncGeodatabaseParameters taskParameters = await syncTask.CreateDefaultSyncGeodatabaseParametersAsync(_localGeodatabase);

                // Create a synchronize geodatabase job, pass in the parameters and the geodatabase
                SyncGeodatabaseJob job = syncTask.SyncGeodatabase(taskParameters, _localGeodatabase);

                // Handle the JobChanged event for the job
                job.StatusChanged += (s, arg) =>
                {
                    // Report changes in the job status
                    if (job.Status == JobStatus.Succeeded)
                    {
                        // Report success ...
                        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () => MessageTextBlock.Text = "Synchronization is complete!");
                    }
                    else if (job.Status == JobStatus.Failed)
                    {
                        // Report failure ...
                        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () => MessageTextBlock.Text = job.Error.Message);
                    }
                    else
                    {
                        // Report that the job is in progress ...
                        DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () => MessageTextBlock.Text = "Sync in progress ...");
                    }
                };

                // Await the completion of the job
                await job.GetResultAsync();
            }
            catch (Exception ex)
            {
                // Show the message if an exception occurred
                MessageTextBlock.Text = "Error when synchronizing: " + ex.Message;
            }
            finally
            {
                // Hide the progress bar when the sync job is complete
                LoadingProgressBar.Visibility = Visibility.Collapsed;
            }
        }
    }
}