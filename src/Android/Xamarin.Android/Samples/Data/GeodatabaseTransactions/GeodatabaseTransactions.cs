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

namespace ArcGISRuntimeXamarin.Samples.GeodatabaseTransactions
{
    [Activity]
    public class GeodatabaseTransactions : Activity
    {
        // url for the editable feature service
        private const string SyncServiceUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Sync/SaveTheBaySync/FeatureServer/";

        // map view
        private MapView _mapView;

        // work in a small extent south of Galveston, TX
        private Envelope _extent = new Envelope(-95.3035, 29.0100, -95.1053, 29.1298, SpatialReferences.Wgs84);

        // store the local geodatabase to edit
        private Geodatabase _localGeodatabase;

        // store references to two tables to edit: Birds and Marine points
        private GeodatabaseFeatureTable _birdTable;
        private GeodatabaseFeatureTable _marineTable;

        // switch to control whether or not transactions are required for edits
        private Switch _requireTransactionSwitch;

        // buttons to start/stop an edit transaction
        private Button _startEditingButton;
        private Button _stopEditingButton;

        // buttons to add bird or marine features
        private Button _addBirdButton;
        private Button _addMarineButton;

        // button to synchronize local edits with the service
        private Button _syncEditsButton;

        // text view to show status messages
        private TextView _messageTextBlock;

        // Progress bar
        private ProgressBar _progressBar;

        // dialog for choosing how to end the transaction (commit, rollback, cancel)
        private AlertDialog _stopEditDialog;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Title = "Geodatabase transactions";

            // create the UI
            CreateLayout();

            // initialize the map and load local data (generate a local geodatabase if necessary)
            Initialize();
        }

        private void CreateLayout()
        {
            //TODO: Create the UI

            // Create the layout
            LinearLayout layout = new LinearLayout(this);
            layout.Orientation = Orientation.Vertical;


            // Add the progress bar
            _progressBar = new ProgressBar(this);
            _progressBar.Visibility = Android.Views.ViewStates.Gone;
            layout.AddView(_progressBar);


            // Add the map view
            _mapView = new MapView(this);
            layout.AddView(_mapView);

            // Add the layout to the view
            SetContentView(layout);
        }

        private void Initialize()
        {
            // when the spatial reference changes (the map loads) add the local geodatabase tables as feature layers
            _mapView.SpatialReferenceChanged += async (s, e) =>
            {
                // call a function (and await it) to get the local geodatabase (or generate it from the feature service)
                await GetLocalGeodatabase();

                // once the local geodatabase is available, load the tables as layers to the map
                LoadLocalGeodatabaseTables();
            };

            // create a new map with the oceans basemap and add it to the map view
            var map = new Map(Basemap.CreateOceans());
            _mapView.Map = map;
        }

        private async Task GetLocalGeodatabase()
        {
            // get the path to the local geodatabase for this platform (temp directory, for example)
            var localGeodatabasePath = GetGdbPath();

            try
            {
                // see if the geodatabase file is already present
                if (File.Exists(localGeodatabasePath))
                {
                    // if the geodatabase is already available, open it, hide the progress control, and update the message
                    _localGeodatabase = await Geodatabase.OpenAsync(localGeodatabasePath);
                    _progressBar.Visibility = Android.Views.ViewStates.Gone;
                    _messageTextBlock.Text = "Using local geodatabase from '" + _localGeodatabase.Path + "'";
                }
                else
                {
                    // create a new GeodatabaseSyncTask with the uri of the feature server to pull from
                    var uri = new Uri(SyncServiceUrl);
                    var gdbTask = await GeodatabaseSyncTask.CreateAsync(uri);

                    // create parameters for the task: layers and extent to include, out spatial reference, and sync model
                    var gdbParams = await gdbTask.CreateDefaultGenerateGeodatabaseParametersAsync(_extent);
                    gdbParams.OutSpatialReference = _mapView.SpatialReference;
                    gdbParams.SyncModel = SyncModel.Layer;
                    gdbParams.LayerOptions.Clear();
                    gdbParams.LayerOptions.Add(new GenerateLayerOption(0));
                    gdbParams.LayerOptions.Add(new GenerateLayerOption(1));

                    // create a geodatabase job that generates the geodatabase
                    GenerateGeodatabaseJob generateGdbJob = gdbTask.GenerateGeodatabase(gdbParams, localGeodatabasePath);

                    // handle the job changed event and check the status of the job; store the geodatabase when it's ready
                    generateGdbJob.JobChanged += (s, e) =>
                    {
                        // call a function to update the progress bar
                        RunOnUiThread(() => UpdateProgressBar(generateGdbJob.Progress));

                        // see if the job succeeded
                        if (generateGdbJob.Status == JobStatus.Succeeded)
                        {
                            RunOnUiThread(() =>
                            {
                                // hide the progress control and update the message
                                _progressBar.Visibility = Android.Views.ViewStates.Gone;
                                _messageTextBlock.Text = "Created local geodatabase";
                            });
                        }
                        else if (generateGdbJob.Status == JobStatus.Failed)
                        {
                            RunOnUiThread(() =>
                            {
                                // hide the progress control and report the exception
                                _progressBar.Visibility = Android.Views.ViewStates.Gone;
                                _messageTextBlock.Text = "Unable to create local geodatabase: " + generateGdbJob.Error.Message;
                            });
                        }
                    };

                    // start the generate geodatabase job
                    _localGeodatabase = await generateGdbJob.GetResultAsync();
                }
            }
            catch (Exception ex)
            {
                // show a message for the exception encountered
                RunOnUiThread(() =>
                {
                    ShowStatusMessage("Generate Geodatabase", "Unable to create offline database: " + ex.Message);
                });
            }
        }

        // function that loads the two point tables from the local geodatabase and displays them as feature layers
        private async void LoadLocalGeodatabaseTables()
        {
            if (_localGeodatabase == null) { return; }

            // read the geodatabase tables and add them as layers
            foreach (GeodatabaseFeatureTable table in _localGeodatabase.GeodatabaseFeatureTables)
            {
                // load the table so the TableName can be read
                await table.LoadAsync();

                // store a reference to the Birds table
                if (table.TableName.ToLower().Contains("birds"))
                {
                    _birdTable = table;
                }

                // store a reference to the Marine table
                if (table.TableName.ToLower().Contains("marine"))
                {
                    _marineTable = table;
                }

                // create a new feature layer to show the table in the map
                var layer = new FeatureLayer(table);
                RunOnUiThread(() => _mapView.Map.OperationalLayers.Add(layer));
            }

            // handle the transaction status changed event
            _localGeodatabase.TransactionStatusChanged += GdbTransactionStatusChanged;

            // zoom the map view to the extent of the generated local datasets
            RunOnUiThread(() =>
            {
                _mapView.SetViewpointGeometryAsync(_marineTable.Extent);
                _startEditingButton.Enabled = true;
            });
        }

        private void GdbTransactionStatusChanged(object sender, TransactionStatusChangedEventArgs e)
        {
            // update UI controls based on whether the geodatabase has a current transaction
            RunOnUiThread(() =>
            {
                // these buttons should be enabled when there IS a transaction
                _addBirdButton.Enabled = e.IsInTransaction;
                _addMarineButton.Enabled = e.IsInTransaction;
                _stopEditingButton.Enabled = e.IsInTransaction;

                // these buttons should be enabled when there is NOT a transaction
                _startEditingButton.Enabled = !e.IsInTransaction;
                _syncEditsButton.Enabled = !e.IsInTransaction;
            });
        }

        private void BeginTransaction(object sender, EventArgs e)
        {
            // see if there is a transaction active for the geodatabase
            if (!_localGeodatabase.IsInTransaction)
            {
                // if not, begin a transaction
                _localGeodatabase.BeginTransaction();
                _messageTextBlock.Text = "Transaction started";
            }
        }

        private async void AddNewFeature(object sender, EventArgs args)
        {
            // See if it was the "Birds" or "Marine" button that was clicked
            Button addFeatureButton = sender as Button;

            try
            {
                // cancel execution of the sketch task if it is already active
                if (_mapView.SketchEditor.CancelCommand.CanExecute(null))
                {
                    _mapView.SketchEditor.CancelCommand.Execute(null);
                }

                // store the correct table to edit (for the button clicked)
                GeodatabaseFeatureTable editTable = null;
                if (addFeatureButton == _addBirdButton)
                {
                    editTable = _birdTable;
                }
                else if (addFeatureButton == _addMarineButton)
                {
                    editTable = _marineTable;
                }

                // inform the user which table is being edited
                _messageTextBlock.Text = "Click the map to add a new feature to the geodatabase table '" + editTable.TableName + "'";

                // create a random value for the 'type' attribute (integer between 1 and 7)
                Random random = new Random(DateTime.Now.Millisecond);
                int featureType = random.Next(1, 7);

                // use the sketch editor to allow the user to draw a point on the map
                MapPoint clickPoint = await _mapView.SketchEditor.StartAsync(SketchCreationMode.Point, false) as MapPoint;

                // create a new feature (row) in the selected table
                Feature newFeature = editTable.CreateFeature();

                // set the geometry with the point the user clicked and the 'type' with the random integer
                newFeature.Geometry = clickPoint;
                newFeature.SetAttributeValue("type", featureType);

                // add the new feature to the table
                await editTable.AddFeatureAsync(newFeature);

                // clear the message
                _messageTextBlock.Text = "New feature added to the '" + editTable.TableName + "' table";
            }
            catch (TaskCanceledException)
            {
                // ignore if the edit was canceled
            }
            catch (Exception ex)
            {
                // report other exception messages
                _messageTextBlock.Text = ex.Message;
            }
        }

        private void StopEditTransaction(object sender, EventArgs e)
        {
            // Create a dialog to prompt the user to commit or rollback
            AlertDialog.Builder dialogBuilder = new AlertDialog.Builder(this);

            // Create the layout
            LinearLayout dialogLayout = new LinearLayout(this);
            dialogLayout.Orientation = Orientation.Vertical;

            // Create a button to commit edits
            Button commitButton = new Button(this);
            commitButton.Text = "Commit";

            // Handle the click event for the Commit button
            commitButton.Click += (s, args) =>
            {
                //see if there is a transaction active for the geodatabase
                if (_localGeodatabase.IsInTransaction)
                {
                    // if there is, commit the transaction to store the edits (this will also end the transaction)
                    _localGeodatabase.CommitTransaction();
                    _messageTextBlock.Text = "Edits were committed to the local geodatabase.";
                }

                _stopEditDialog.Dismiss();
            };

            // Create a button to rollback edits
            Button rollbackButton = new Button(this);
            rollbackButton.Text = "Rollback";

            // Handle the click event for the Rollback button
            rollbackButton.Click += (s, args) => 
            {
                // see if there is a transaction active for the geodatabase
                if (_localGeodatabase.IsInTransaction)
                {
                    // if there is, rollback the transaction to discard the edits (this will also end the transaction)
                    _localGeodatabase.RollbackTransaction();
                    _messageTextBlock.Text = "Edits were rolled back and not stored to the local geodatabase.";
                }

                _stopEditDialog.Dismiss();
            };

            // Create a button to cancel and return to the transaction
            Button cancelButton = new Button(this);
            cancelButton.Text = "Cancel";

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

        // change which controls are enabled if the user chooses to require/not require transactions for edits
        private void RequireTransactionChanged(object sender, EventArgs e)
        {
            // if the local geodatabase isn't created yet, return
            if (_localGeodatabase == null) { return; }

            // get the value of the "require transactions" switch
            bool mustHaveTransaction = _requireTransactionSwitch.Checked;

            // warn the user if disabling transactions while a transaction is active
            if (!mustHaveTransaction && _localGeodatabase.IsInTransaction)
            {
                ShowStatusMessage("Stop editing to end the current transaction.", "Current Transaction");
                _requireTransactionSwitch.Checked = true;
                return;
            }

            // enable or disable controls according to the switch value
            _startEditingButton.Enabled = mustHaveTransaction;
            _stopEditingButton.Enabled = mustHaveTransaction && _localGeodatabase.IsInTransaction;
            _addBirdButton.Enabled = !mustHaveTransaction;
            _addMarineButton.Enabled = !mustHaveTransaction;
        }

        // synchronize edits in the local geodatabase with the service
        public async void SynchronizeEdits(object sender, EventArgs e)
        {
            // show the progress bar while the sync is working
            _progressBar.Visibility = Android.Views.ViewStates.Visible;

            try
            {
                // create a sync task with the URL of the feature service to sync
                var syncTask = await GeodatabaseSyncTask.CreateAsync(new Uri(SyncServiceUrl));

                // create sync parameters
                var taskParameters = await syncTask.CreateDefaultSyncGeodatabaseParametersAsync(_localGeodatabase);

                // create a synchronize geodatabase job, pass in the parameters and the geodatabase
                SyncGeodatabaseJob job = syncTask.SyncGeodatabase(taskParameters, _localGeodatabase);

                // handle the JobChanged event for the job
                job.JobChanged += (s, arg) =>
                {
                    RunOnUiThread(() =>
                    {
                        // update the progress bar
                        UpdateProgressBar(job.Progress);

                        // report changes in the job status
                        if (job.Status == JobStatus.Succeeded)
                        {
                            _messageTextBlock.Text = "Synchronization is complete!";
                            _progressBar.Visibility = Android.Views.ViewStates.Gone;
                        }
                        else if (job.Status == JobStatus.Failed)
                        {
                            // report failure ...
                            _messageTextBlock.Text = job.Error.Message;
                            _progressBar.Visibility = Android.Views.ViewStates.Gone;
                        }
                        else
                        {
                            // report that the job is in progress ...
                            _messageTextBlock.Text = "Sync in progress ...";
                        }

                    });
                };

                // await the completion of the job
                var result = await job.GetResultAsync();
            }
            catch (Exception ex)
            {
                // show the message if an exception occurred
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
            var builder = new AlertDialog.Builder(this);
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