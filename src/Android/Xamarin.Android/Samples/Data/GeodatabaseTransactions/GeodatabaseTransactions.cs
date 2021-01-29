// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// Language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.IO;
using System.Threading.Tasks;
using Android.Views;

namespace ArcGISRuntime.Samples.GeodatabaseTransactions
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Geodatabase transactions",
        category: "Data",
        description: "Use transactions to manage how changes are committed to a geodatabase.",
        instructions: "When the sample loads, a feature service is taken offline as a geodatabase. When the geodatabase is ready, you can add multiple types of features. To apply edits directly, uncheck the 'Require a transaction for edits' checkbox. When using transactions, use the buttons to start editing and stop editing. When you stop editing, you can choose to commit the changes or roll them back. At any point, you can synchronize the local geodatabase with the feature service.",
        tags: new[] { "commit", "database", "geodatabase", "transact", "transactions" })]
    public class GeodatabaseTransactions : Activity
    {
        // URL for the editable feature service
        private const string SyncServiceUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Sync/SaveTheBaySync/FeatureServer/";

        // Map view
        private MapView _mapView;

        // Work in a small extent south of Galveston, TX
        private Envelope _extent = new Envelope(-95.3035, 29.0100, -95.1053, 29.1298, SpatialReferences.Wgs84);

        // Store the local geodatabase to edit
        private Geodatabase _localGeodatabase;

        // Store references to two tables to edit: Birds and Marine points
        private GeodatabaseFeatureTable _birdTable;
        private GeodatabaseFeatureTable _marineTable;

        // Switch to control whether or not transactions are required for edits
        private Switch _requireTransactionSwitch;

        // Buttons to start/stop an edit transaction
        private Button _startEditingButton;
        private Button _stopEditingButton;

        // Buttons to add bird or marine features
        private Button _addBirdButton;
        private Button _addMarineButton;

        // Button to synchronize local edits with the service
        private Button _syncEditsButton;

        // Text view to show status messages
        private TextView _messageTextBlock;

        // Progress bar
        private ProgressBar _progressBar;

        // Dialog for choosing how to end the transaction (commit, rollback, cancel)
        private AlertDialog _stopEditDialog;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Title = "Geodatabase transactions";

            // Create the UI
            CreateLayout();

            // Initialize the map and load local data (generate a local geodatabase if necessary)
            Initialize();
        }

        private void CreateLayout()
        {
            // Layout params for making buttons fill page width
            LinearLayout.LayoutParams param = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent,
                1.0f
            );

            // Button to start an edit transaction
            _startEditingButton = new Button(this)
            {
                Text = "Start",
                LayoutParameters = param
            };
            _startEditingButton.Click += BeginTransaction;

            // Button to stop a transaction
            _stopEditingButton = new Button(this)
            {
                Text = "Stop",
                Enabled = false,
                LayoutParameters = param
            };
            _stopEditingButton.Click += StopEditTransaction;

            // Button to synchronize local edits with the service
            _syncEditsButton = new Button(this)
            {
                Text = "Sync",
                Enabled = false,
                LayoutParameters = param
            };
            _syncEditsButton.Click += SynchronizeEdits;

            // Button to add bird features
            _addBirdButton = new Button(this)
            {
                Text = "Add Bird",
                Enabled = false,
                LayoutParameters = param
            };
            _addBirdButton.Click += AddNewFeature;

            // Button to add marine features
            _addMarineButton = new Button(this)
            {
                Text = "Add Marine",
                Enabled = false,
                LayoutParameters = param
            };
            _addMarineButton.Click += AddNewFeature;

            // Layout to hold the first row of buttons (start, stop, sync)
            LinearLayout editButtonsRow1 = new LinearLayout(this)
            {
                Orientation = Orientation.Horizontal
            };
            editButtonsRow1.AddView(_startEditingButton);
            editButtonsRow1.AddView(_stopEditingButton);
            editButtonsRow1.AddView(_syncEditsButton);

            // Layout to hold the second row of buttons (add bird, add marine)
            LinearLayout editButtonsRow2 = new LinearLayout(this)
            {
                Orientation = Orientation.Horizontal
            };
            editButtonsRow2.AddView(_addBirdButton);
            editButtonsRow2.AddView(_addMarineButton);

            // Layout for the 'require transaction' switch
            LinearLayout editSwitchRow = new LinearLayout(this)
            {
                Orientation = Orientation.Horizontal
            };
            _requireTransactionSwitch = new Switch(this)
            {
                Checked = true,
                Text = "Require transaction"
            };
            _requireTransactionSwitch.CheckedChange += RequireTransactionChanged;
            editSwitchRow.AddView(_requireTransactionSwitch);

            // Progress bar
            _progressBar = new ProgressBar(this)
            {
                Visibility = Android.Views.ViewStates.Gone
            };

            // Use the rest of the view to show status messages
            _messageTextBlock = new TextView(this);

            // Create the main layout
            LinearLayout layout = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical
            };

            // Add the first row of buttons
            layout.AddView(editButtonsRow1);

            // Add the second row of buttons
            layout.AddView(editButtonsRow2);

            // Add the 'require transaction' switch and label
            layout.AddView(editSwitchRow);

            // Add the messages text view
            layout.AddView(_messageTextBlock);

            // Add the progress bar
            layout.AddView(_progressBar);            
            
            // Add the map view
            _mapView = new MapView(this);
            layout.AddView(_mapView);

            // Add the layout to the view
            SetContentView(layout);
        }

        private void Initialize()
        {
            // When the spatial reference changes (the map loads) add the local geodatabase tables as feature layers
            _mapView.SpatialReferenceChanged += async (s, e) =>
            {
                // Call a function (and await it) to get the local geodatabase (or generate it from the feature service)
                await GetLocalGeodatabase();

                // Once the local geodatabase is available, load the tables as layers to the map
                LoadLocalGeodatabaseTables();
            };

            // Create a new map with the oceans basemap and add it to the map view
            _mapView.Map = new Map(BasemapStyle.ArcGISOceans);
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
                    _progressBar.Visibility = Android.Views.ViewStates.Gone;
                    _messageTextBlock.Text = "Using local geodatabase from '" + _localGeodatabase.Path + "'";
                }
                else
                {
                    // Create a new GeodatabaseSyncTask with the uri of the feature server to pull from
                    GeodatabaseSyncTask gdbTask = await GeodatabaseSyncTask.CreateAsync(new Uri(SyncServiceUrl));

                    // Create parameters for the task: layers and extent to include, out spatial reference, and sync model
                    GenerateGeodatabaseParameters gdbParams = await gdbTask.CreateDefaultGenerateGeodatabaseParametersAsync(_extent);
                    gdbParams.OutSpatialReference = _mapView.SpatialReference;
                    gdbParams.SyncModel = SyncModel.Layer;
                    gdbParams.LayerOptions.Clear();
                    gdbParams.LayerOptions.Add(new GenerateLayerOption(0));
                    gdbParams.LayerOptions.Add(new GenerateLayerOption(1));

                    // Create a geodatabase job that generates the geodatabase
                    GenerateGeodatabaseJob generateGdbJob = gdbTask.GenerateGeodatabase(gdbParams, localGeodatabasePath);

                    // Handle the job changed event and check the status of the job; store the geodatabase when it's ready
                    generateGdbJob.JobChanged += (s, e) =>
                    {
                        // Call a function to update the progress bar
                        RunOnUiThread(() => UpdateProgressBar(generateGdbJob.Progress));

                        // See if the job succeeded
                        if (generateGdbJob.Status == JobStatus.Succeeded)
                        {
                            RunOnUiThread(() =>
                            {
                                // Hide the progress control and update the message
                                _progressBar.Visibility = Android.Views.ViewStates.Gone;
                                _messageTextBlock.Text = "Created local geodatabase";
                            });
                        }
                        else if (generateGdbJob.Status == JobStatus.Failed)
                        {
                            RunOnUiThread(() =>
                            {
                                // Hide the progress control and report the exception
                                _progressBar.Visibility = Android.Views.ViewStates.Gone;
                                _messageTextBlock.Text = "Unable to create local geodatabase: " + generateGdbJob.Error.Message;
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
                RunOnUiThread(() =>
                {
                    ShowStatusMessage("Generate Geodatabase", "Unable to create offline database: " + ex.Message);
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
                    RunOnUiThread(() => _mapView.Map.OperationalLayers.Add(layer));
                }
                catch (Exception e)
                {
                    new AlertDialog.Builder(this).SetMessage(e.ToString()).SetTitle("Error").Show();
                }
            }

            // Handle the transaction status changed event
            _localGeodatabase.TransactionStatusChanged += GdbTransactionStatusChanged;

            // Zoom the map view to the extent of the generated local datasets
            RunOnUiThread(() =>
            {
                _mapView.SetViewpoint(new Viewpoint(_marineTable.Extent));
                _startEditingButton.Enabled = true;
            });
        }

        private void GdbTransactionStatusChanged(object sender, TransactionStatusChangedEventArgs e)
        {
            // Update UI controls based on whether the geodatabase has a current transaction
            RunOnUiThread(() =>
            {
                // These buttons should be enabled when there IS a transaction
                _addBirdButton.Enabled = e.IsInTransaction;
                _addMarineButton.Enabled = e.IsInTransaction;
                _stopEditingButton.Enabled = e.IsInTransaction;

                // These buttons should be enabled when there is NOT a transaction
                _startEditingButton.Enabled = !e.IsInTransaction;
                _syncEditsButton.Enabled = !e.IsInTransaction;
            });
        }

        private void BeginTransaction(object sender, EventArgs e)
        {
            // See if there is a transaction active for the geodatabase
            if (_localGeodatabase!= null && !_localGeodatabase.IsInTransaction)
            {
                // If not, begin a transaction
                _localGeodatabase.BeginTransaction();
                _messageTextBlock.Text = "Transaction started";
            }
            else
            {
                _messageTextBlock.Text = "Not yet ready for new transaction.";
            }
        }

        private async void AddNewFeature(object sender, EventArgs args)
        {
            // See if it was the "Birds" or "Marine" button that was clicked
            Button addFeatureButton = (Button)sender;

            try
            {
                // Cancel execution of the sketch task if it is already active
                if (_mapView.SketchEditor.CancelCommand.CanExecute(null))
                {
                    _mapView.SketchEditor.CancelCommand.Execute(null);
                }

                // Store the correct table to edit (for the button clicked)
                GeodatabaseFeatureTable editTable = null;
                if (addFeatureButton == _addBirdButton)
                {
                    editTable = _birdTable;
                }
                else
                {
                    editTable = _marineTable;
                }

                // Inform the user which table is being edited
                _messageTextBlock.Text = "Click the map to add a new feature to the geodatabase table '" + editTable.TableName + "'";

                // Create a random value for the 'type' attribute (integer between 1 and 7)
                Random random = new Random(DateTime.Now.Millisecond);
                int featureType = random.Next(1, 7);

                // Use the sketch editor to allow the user to draw a point on the map
                MapPoint clickPoint = await _mapView.SketchEditor.StartAsync(SketchCreationMode.Point, false) as MapPoint;

                // Create a new feature (row) in the selected table
                Feature newFeature = editTable.CreateFeature();

                // Set the geometry with the point the user clicked and the 'type' with the random integer
                newFeature.Geometry = clickPoint;
                newFeature.SetAttributeValue("type", featureType);

                // Add the new feature to the table
                await editTable.AddFeatureAsync(newFeature);

                // Clear the message
                _messageTextBlock.Text = "New feature added to the '" + editTable.TableName + "' table";
            }
            catch (TaskCanceledException)
            {
                // Ignore if the edit was canceled
            }
            catch (Exception ex)
            {
                // Report other exception messages
                _messageTextBlock.Text = ex.Message;
            }
        }

        private void StopEditTransaction(object sender, EventArgs e)
        {
            // Create a dialog to prompt the user to commit or rollback
            AlertDialog.Builder dialogBuilder = new AlertDialog.Builder(this);

            // Create the layout
            LinearLayout dialogLayout = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical
            };

            // Create a button to commit edits
            Button commitButton = new Button(this)
            {
                Text = "Commit"
            };

            // Handle the click event for the Commit button
            commitButton.Click += (s, args) =>
            {
                // See if there is a transaction active for the geodatabase
                if (_localGeodatabase.IsInTransaction)
                {
                    // If there is, commit the transaction to store the edits (this will also end the transaction)
                    _localGeodatabase.CommitTransaction();
                    _messageTextBlock.Text = "Edits were committed to the local geodatabase.";
                }

                _stopEditDialog.Dismiss();
            };

            // Create a button to rollback edits
            Button rollbackButton = new Button(this)
            {
                Text = "Rollback"
            };

            // Handle the click event for the Rollback button
            rollbackButton.Click += (s, args) => 
            {
                // See if there is a transaction active for the geodatabase
                if (_localGeodatabase.IsInTransaction)
                {
                    // If there is, rollback the transaction to discard the edits (this will also end the transaction)
                    _localGeodatabase.RollbackTransaction();
                    _messageTextBlock.Text = "Edits were rolled back and not stored to the local geodatabase.";
                }

                _stopEditDialog.Dismiss();
            };

            // Create a button to cancel and return to the transaction
            Button cancelButton = new Button(this)
            {
                Text = "Cancel"
            };

            // Handle the click event for the Cancel button
            rollbackButton.Click += (s, args) => _stopEditDialog.Dismiss();

            // Add the controls to the dialog
            dialogLayout.AddView(cancelButton);
            dialogLayout.AddView(rollbackButton);
            dialogLayout.AddView(commitButton);
            dialogBuilder.SetView(dialogLayout);
            dialogBuilder.SetTitle("Stop Editing");

            // Show the dialog
            _stopEditDialog = dialogBuilder.Show();
        }

        // Change which controls are enabled if the user chooses to require/not require transactions for edits
        private void RequireTransactionChanged(object sender, EventArgs e)
        {
            // If the local geodatabase isn't created yet, return
            if (_localGeodatabase == null) { return; }

            // Get the value of the "require transactions" switch
            bool mustHaveTransaction = _requireTransactionSwitch.Checked;

            // Warn the user if disabling transactions while a transaction is active
            if (!mustHaveTransaction && _localGeodatabase.IsInTransaction)
            {
                ShowStatusMessage("Stop editing to end the current transaction.", "Current Transaction");
                _requireTransactionSwitch.Checked = true;
                return;
            }

            // Enable or disable controls according to the switch value
            _startEditingButton.Enabled = mustHaveTransaction;
            _stopEditingButton.Enabled = mustHaveTransaction && _localGeodatabase.IsInTransaction;
            _addBirdButton.Enabled = !mustHaveTransaction;
            _addMarineButton.Enabled = !mustHaveTransaction;
        }

        // Synchronize edits in the local geodatabase with the service
        public async void SynchronizeEdits(object sender, EventArgs e)
        {
            // Show the progress bar while the sync is working
            _progressBar.Visibility = Android.Views.ViewStates.Visible;

            try
            {
                // Create a sync task with the URL of the feature service to sync
                GeodatabaseSyncTask syncTask = await GeodatabaseSyncTask.CreateAsync(new Uri(SyncServiceUrl));

                // Create sync parameters
                SyncGeodatabaseParameters taskParameters = await syncTask.CreateDefaultSyncGeodatabaseParametersAsync(_localGeodatabase);

                // Create a synchronize geodatabase job, pass in the parameters and the geodatabase
                SyncGeodatabaseJob job = syncTask.SyncGeodatabase(taskParameters, _localGeodatabase);

                // Handle the JobChanged event for the job
                job.JobChanged += (s, arg) =>
                {
                    RunOnUiThread(() =>
                    {
                        // Update the progress bar
                        UpdateProgressBar(job.Progress);

                        // Report changes in the job status
                        if (job.Status == JobStatus.Succeeded)
                        {
                            _messageTextBlock.Text = "Synchronization is complete!";
                            _progressBar.Visibility = Android.Views.ViewStates.Gone;
                        }
                        else if (job.Status == JobStatus.Failed)
                        {
                            // Report failure ...
                            _messageTextBlock.Text = job.Error.Message;
                            _progressBar.Visibility = Android.Views.ViewStates.Gone;
                        }
                        else
                        {
                            // Report that the job is in progress ...
                            _messageTextBlock.Text = "Sync in progress ...";
                        }

                    });
                };

                // Await the completion of the job
                 await job.GetResultAsync();
            }
            catch (Exception ex)
            {
                // Show the message if an exception occurred
                _messageTextBlock.Text = "Error when synchronizing: " + ex.Message;
            }
        }

        private string GetGdbPath()
        {
            return GetFileStreamPath("wildfire.geodatabase").AbsolutePath;
        }

        private void ShowStatusMessage(string title, string message)
        {
            // Display the message to the user
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetMessage(message).SetTitle(title).Show();
        }

        private void UpdateProgressBar(int progress)
        {
            // Due to the nature of the threading implementation,
            //     the dispatcher needs to be used to interact with the UI
            RunOnUiThread(() =>
            {
                // Update the progress bar value
                _progressBar.Progress = progress;
            });
        }
    }
}