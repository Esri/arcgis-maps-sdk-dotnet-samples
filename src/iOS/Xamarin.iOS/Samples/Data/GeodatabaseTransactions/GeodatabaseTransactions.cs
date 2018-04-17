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
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.IO;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntime.Samples.GeodatabaseTransactions
{
    [Register("GeodatabaseTransactions")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Geodatabase transactions",
        "Data",
        "This sample demonstrates how to manage edits to a local geodatabase inside of transactions.",
        "When the sample loads, a local geodatabase will be generated for a small area from the 'SaveTheBay' feature service. When the geodatabase is ready, its tables are added as feature layers and the map view zooms to the extent of the local data. Use the UI controls to make edits either inside or outside of a transaction. If made in a transaction, you can rollback or commit your edits as a single unit when you choose to stop editing. To allow edits without a transaction, set 'Require transaction' to false. You can then add features directly into the local geodatabase. When done adding features, you can synchronize your local edits with the service.")]
    public class GeodatabaseTransactions : UIViewController
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

        // MapView control
        private MapView _mapView = new MapView();

        // Stack view to contain the edit UI
        private UIStackView _editToolsView = new UIStackView();

        private nfloat _mapViewHeight;
        private nfloat _editToolsHeight;

        // Progress view to show status of generate geodatabase and synchronization jobs
        private UIProgressView _progressBar = new UIProgressView();

        // Switch to control whether or not transactions are required for edits
        private UISwitch _requireTransactionSwitch = new UISwitch();

        // Buttons to start/stop an edit transaction
        private UIButton _startEditingButton = new UIButton();
        private UIButton _stopEditingButton = new UIButton();

        // Buttons to add bird or marine features
        private UIButton _addBirdButton = new UIButton();
        private UIButton _addMarineButton = new UIButton();

        // Button to synchronize local edits with the service
        private UIButton _syncEditsButton = new UIButton();

        // Text view to show status messages
        private UITextView _messageTextBlock = new UITextView();

        public GeodatabaseTransactions()
        {
            Title = "Geodatabase transactions";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Get the height of the map view (two thirds of the total) and the edit tools view (one third)
            _mapViewHeight = (nfloat)(View.Bounds.Height * (2.0 / 3.0));
            _editToolsHeight = (nfloat)(View.Bounds.Height * (1.0 / 3.0));

            // Create the UI
            CreateLayout();

            // Initialize the map and load local data (generate a local geodatabase if necessary)
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            // Place the MapView (top 2/3 of the view)
            _mapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, _mapViewHeight);

            // Place the edit tools (bottom 1/3 of the view)
            _editToolsView.Frame = new CoreGraphics.CGRect(0, _mapViewHeight, View.Bounds.Width, _editToolsHeight);
        }

        private void CreateLayout()
        {
            // Place the map view in the upper two thirds of the display
            _mapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, _mapViewHeight);
            View.BackgroundColor = UIColor.Gray;

            // Fit three buttons on the row
            double buttonWidth = View.Bounds.Width / 3.0;

            // Button to start an edit transaction
            _startEditingButton.SetTitle("Start", UIControlState.Normal);
            _startEditingButton.SetTitleColor(UIColor.LightGray, UIControlState.Disabled);
            _startEditingButton.Frame = new CoreGraphics.CGRect(0, 0, buttonWidth, 30);
            _startEditingButton.TouchUpInside += BeginTransaction;

            // Button to stop a transaction
            _stopEditingButton.SetTitle("Stop", UIControlState.Normal);
            _stopEditingButton.Enabled = false;
            _stopEditingButton.SetTitleColor(UIColor.LightGray, UIControlState.Disabled);
            _stopEditingButton.Frame = new CoreGraphics.CGRect(buttonWidth, 0, buttonWidth, 30);
            _stopEditingButton.TouchUpInside += StopEditTransaction;

            // Button to synchronize local edits with the service
            _syncEditsButton.SetTitle("Sync", UIControlState.Normal);
            _syncEditsButton.SetTitleColor(UIColor.LightGray, UIControlState.Disabled);
            _syncEditsButton.Enabled = false;
            _syncEditsButton.Frame = new CoreGraphics.CGRect(buttonWidth * 2, 0, buttonWidth, 30);
            _syncEditsButton.TouchUpInside += SynchronizeEdits;

            // Two buttons on this row
            buttonWidth = View.Bounds.Width / 2.0;

            // Button to add bird features
            _addBirdButton.SetTitle("Add Bird", UIControlState.Normal);
            _addBirdButton.Enabled = false;
            _addBirdButton.SetTitleColor(UIColor.LightGray, UIControlState.Disabled);
            _addBirdButton.Frame = new CoreGraphics.CGRect(0, 0, buttonWidth, 30);
            _addBirdButton.TouchUpInside += AddNewFeature;

            // Button to add marine features
            _addMarineButton.SetTitle("Add Marine", UIControlState.Normal);
            _addMarineButton.Enabled = false;
            _addMarineButton.SetTitleColor(UIColor.LightGray, UIControlState.Disabled);
            _addMarineButton.Frame = new CoreGraphics.CGRect(buttonWidth, 0, buttonWidth, 30);
            _addMarineButton.TouchUpInside += AddNewFeature;

            _editToolsView.Axis = UILayoutConstraintAxis.Vertical;
            _editToolsView.Frame = new CoreGraphics.CGRect(0, _mapViewHeight, View.Bounds.Width, _editToolsHeight);

            // View to hold the first row of buttons (start, stop, sync)
            UIStackView editButtonsRow1 = new UIStackView();
            editButtonsRow1.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, 35);
            editButtonsRow1.Axis = UILayoutConstraintAxis.Horizontal;
            editButtonsRow1.Add(_startEditingButton);
            editButtonsRow1.Add(_stopEditingButton);
            editButtonsRow1.Add(_syncEditsButton);
            
            // View to hold the second row of buttons (add bird, add marine)
            UIStackView editButtonsRow2 = new UIStackView();
            editButtonsRow2.Frame = new CoreGraphics.CGRect(0, 35, View.Bounds.Width, 35);
            editButtonsRow2.Axis = UILayoutConstraintAxis.Horizontal;
            editButtonsRow2.Add(_addBirdButton);
            editButtonsRow2.Add(_addMarineButton);

            // View for the 'require transaction' switch
            UIStackView editSwitchRow = new UIStackView();
            editSwitchRow.Frame = new CoreGraphics.CGRect(0, 70, View.Bounds.Width, 35);
            editSwitchRow.Axis = UILayoutConstraintAxis.Horizontal;
            _requireTransactionSwitch.On = true;
            _requireTransactionSwitch.ValueChanged += RequireTransactionChanged;
            editSwitchRow.Add(_requireTransactionSwitch);

            // Create a label that describes the switch value
            UILabel switchLabel = new UILabel();
            switchLabel.Text = "Require transaction";
            switchLabel.Frame = new CoreGraphics.CGRect(70, 0, View.Bounds.Width - 70, 30);
            editSwitchRow.Add(switchLabel);
            
            // Progress bar
            _progressBar.Frame = new CoreGraphics.CGRect(0, 105, View.Bounds.Width, 10);

            // Use the rest of the view to show status messages
            _messageTextBlock.Editable = false;
            _messageTextBlock.Frame = new CoreGraphics.CGRect(0, 115, View.Bounds.Width, _editToolsHeight);

            // Add the first row of buttons
            _editToolsView.Add(editButtonsRow1);

            // Add the second row of buttons
            _editToolsView.Add(editButtonsRow2);

            // Add the 'require transaction' switch and label
            _editToolsView.Add(editSwitchRow);

            // Add the messages text view
            _editToolsView.Add(_messageTextBlock);

            // Add the progress bar
            _editToolsView.Add(_progressBar);

            // Add the views
            View.AddSubviews(_mapView, _editToolsView);
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
            var map = new Map(Basemap.CreateOceans());
            _mapView.Map = map;
        }

        private string GetGdbPath()
        {
            // Get the platform-specific path for storing the geodatabase
            string folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            
            // Return the final path
            return Path.Combine(folder, "savethebay.geodatabase");
        }

        private void ShowMessage(string title, string message, string buttonText)
        {
            // Display the message to the user
            UIAlertView alertView = new UIAlertView(title, message, null, buttonText, null);
            alertView.Show();
        }
        
        private async Task GetLocalGeodatabase()
        {
            // Get the path to the local geodatabase for this platform (temp directory, for example)
            var localGeodatabasePath = GetGdbPath();

            try
            {
                // See if the geodatabase file is already present
                if (File.Exists(localGeodatabasePath))
                {
                    // If the geodatabase is already available, open it, hide the progress control, and update the message
                    _localGeodatabase = await Geodatabase.OpenAsync(localGeodatabasePath);
                    _progressBar.Hidden = true;
                    _messageTextBlock.Text = "Using local geodatabase from '" + _localGeodatabase.Path + "'";
                }
                else
                {
                    // Create a new GeodatabaseSyncTask with the uri of the feature server to pull from
                    var uri = new Uri(SyncServiceUrl);
                    var gdbTask = await GeodatabaseSyncTask.CreateAsync(uri);

                    // Create parameters for the task: layers and extent to include, out spatial reference, and sync model
                    var gdbParams = await gdbTask.CreateDefaultGenerateGeodatabaseParametersAsync(_extent);
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
                        InvokeOnMainThread(() => UpdateProgressBar(generateGdbJob.Progress));

                        // See if the job succeeded
                        if (generateGdbJob.Status == JobStatus.Succeeded)
                        {
                            InvokeOnMainThread(() =>
                            {
                                // Hide the progress control and update the message
                                _progressBar.Hidden = true;
                                _messageTextBlock.Text = "Created local geodatabase";
                            });
                        }
                        else if (generateGdbJob.Status == JobStatus.Failed)
                        {
                            InvokeOnMainThread(() =>
                            {
                                // Hide the progress control and report the exception
                                _progressBar.Hidden = true;
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
                InvokeOnMainThread(() =>
                {
                    ShowMessage("Generate Geodatabase", "Unable to create offline database: " + ex.Message, "OK");
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
                var layer = new FeatureLayer(table);
                InvokeOnMainThread(() => _mapView.Map.OperationalLayers.Add(layer));
            }

            // Handle the transaction status changed event
            _localGeodatabase.TransactionStatusChanged += GdbTransactionStatusChanged;

            // Zoom the map view to the extent of the generated local datasets
            InvokeOnMainThread(() =>
            {
                _mapView.SetViewpointGeometryAsync(_marineTable.Extent);
                _startEditingButton.Enabled = true;
            });
        }

        private void GdbTransactionStatusChanged(object sender, TransactionStatusChangedEventArgs e)
        {
            // Update UI controls based on whether the geodatabase has a current transaction
            InvokeOnMainThread(() =>
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
            if (!_localGeodatabase.IsInTransaction)
            {
                // If not, begin a transaction
                _localGeodatabase.BeginTransaction();
                _messageTextBlock.Text = "Transaction started";
            }
        }

        private async void AddNewFeature(object sender, EventArgs args)
        {
            // See if it was the "Birds" or "Marine" button that was clicked
            UIButton addFeatureButton = sender as UIButton;

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
                else if (addFeatureButton == _addMarineButton)
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
            // Create an alert to ask the user if they want to commit or rollback the transaction (or cancel to keep working in the transaction)
            UIAlertController endTransactionAlertController = UIAlertController.Create("Stop Editing", "Commit your edits or roll back?", UIAlertControllerStyle.Alert);

            // Action when user chooses to commit
            endTransactionAlertController.AddAction(UIAlertAction.Create("Commit", UIAlertActionStyle.Default, 
                alert => {
                    // See if there is a transaction active for the geodatabase
                    if (_localGeodatabase.IsInTransaction)
                    {
                        // If there is, commit the transaction to store the edits (this will also end the transaction)
                        _localGeodatabase.CommitTransaction();
                        _messageTextBlock.Text = "Edits were committed to the local geodatabase.";
                    }
                }));

            // Action when user chooses to rollback
            endTransactionAlertController.AddAction(UIAlertAction.Create("Rollback", UIAlertActionStyle.Default, 
                alert => {
                    // See if there is a transaction active for the geodatabase
                    if (_localGeodatabase.IsInTransaction)
                    {
                        // If there is, rollback the transaction to discard the edits (this will also end the transaction)
                        _localGeodatabase.RollbackTransaction();
                        _messageTextBlock.Text = "Edits were rolled back and not stored to the local geodatabase.";
                    }
                }));

            // Action for cancel (ignore)
            endTransactionAlertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

            // Present the alert to the user
            PresentViewController(endTransactionAlertController, true, null);
        }

        // Change which controls are enabled if the user chooses to require/not require transactions for edits
        private void RequireTransactionChanged(object sender, EventArgs e)
        {
            // If the local geodatabase isn't created yet, return
            if (_localGeodatabase == null) { return; }

            // Get the value of the "require transactions" switch
            bool mustHaveTransaction = _requireTransactionSwitch.On;

            // Warn the user if disabling transactions while a transaction is active
            if (!mustHaveTransaction && _localGeodatabase.IsInTransaction)
            {
                ShowMessage("Stop editing to end the current transaction.", "Current Transaction", "OK");
                _requireTransactionSwitch.On = true;
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
            _progressBar.Hidden = false;
            
            try
            {
                // Create a sync task with the URL of the feature service to sync
                var syncTask = await GeodatabaseSyncTask.CreateAsync(new Uri(SyncServiceUrl));

                // Create sync parameters
                var taskParameters = await syncTask.CreateDefaultSyncGeodatabaseParametersAsync(_localGeodatabase);

                // Create a synchronize geodatabase job, pass in the parameters and the geodatabase
                SyncGeodatabaseJob job = syncTask.SyncGeodatabase(taskParameters, _localGeodatabase);

                // Handle the JobChanged event for the job
                job.JobChanged += (s, arg) =>
                {
                    InvokeOnMainThread(() =>
                    {
                        // Update the progress bar
                        UpdateProgressBar(job.Progress);

                        // Report changes in the job status
                        if (job.Status == JobStatus.Succeeded)
                        {
                                _messageTextBlock.Text = "Synchronization is complete!";
                                _progressBar.Hidden = true;
                        }
                        else if (job.Status == JobStatus.Failed)
                        {
                            // Report failure ...
                                _messageTextBlock.Text = job.Error.Message;
                                _progressBar.Hidden = true;
                        }
                        else
                        {
                            // Report that the job is in progress ...
                            _messageTextBlock.Text = "Sync in progress ...";
                        }
                    });
                };

                // Await the completion of the job
                var result = await job.GetResultAsync();
            }
            catch (Exception ex)
            {
                // Show the message if an exception occurred
                _messageTextBlock.Text = "Error when synchronizing: " + ex.Message;
            }
        }

        private void UpdateProgressBar(int jobProgress)
        {
            // Due to the nature of the threading implementation,
            //     the dispatcher needs to be used to interact with the UI
            InvokeOnMainThread(() =>
            {
                // Update the progress bar value
                _progressBar.Progress = (float)(jobProgress / 100.0);
            });
        }
    }
}