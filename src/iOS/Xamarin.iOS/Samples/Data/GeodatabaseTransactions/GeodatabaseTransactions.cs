// Copyright 2017 Esri.
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
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.IO;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.GeodatabaseTransactions
{
    [Register("GeodatabaseTransactions")]
    public class GeodatabaseTransactions : UIViewController
    {
        // url for the editable feature service
        private const string SyncServiceUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Sync/SaveTheBaySync/FeatureServer/";

        // work in a small extent south of Galveston, TX
        private Envelope _extent = new Envelope(-95.3035, 29.0100, -95.1053, 29.1298, SpatialReferences.Wgs84);

        // store the local geodatabase to edit
        private Geodatabase _localGeodatabase;

        // store references to two tables to edit: Birds and Marine points
        private GeodatabaseFeatureTable _birdTable;
        private GeodatabaseFeatureTable _marineTable;

        // MapView control
        private MapView _mapView = new MapView();

        // stack view to contain the edit UI
        private UIStackView _editToolsView = new UIStackView();

        private nfloat _mapViewHeight;
        private nfloat _editToolsHeight;

        // progress view to show status of generate geodatabase and synchronization jobs
        private UIProgressView _progressBar = new UIProgressView();

        // switch to control whether or not transactions are required for edits
        private UISwitch _requireTransactionSwitch = new UISwitch();

        // buttons to start/stop an edit transaction
        private UIButton _startEditingButton = new UIButton();
        private UIButton _stopEditingButton = new UIButton();

        // buttons to add bird or marine features
        private UIButton _addBirdButton = new UIButton();
        private UIButton _addMarineButton = new UIButton();

        // button to synchronize local edits with the service
        private UIButton _syncEditsButton = new UIButton();

        // text view to show status messages
        private UITextView _messageTextBlock = new UITextView();

        public GeodatabaseTransactions()
        {
            Title = "Geodatabase transactions";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // get the height of the map view (two thirds of the total) and the edit tools view (one third)
            _mapViewHeight = (nfloat)(View.Bounds.Height * (2.0 / 3.0));
            _editToolsHeight = (nfloat)(View.Bounds.Height * (1.0 / 3.0));

            // create the UI
            CreateLayout();

            // initialize the map and load local data (generate a local geodatabase if necessary)
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            // place the MapView (top 2/3 of the view)
            _mapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, _mapViewHeight);

            // place the edit tools (bottom 1/3 of the view)
            _editToolsView.Frame = new CoreGraphics.CGRect(0, _mapViewHeight, View.Bounds.Width, _editToolsHeight);
        }

        private void CreateLayout()
        {
            // place the map view in the upper two thirds of the display
            _mapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, _mapViewHeight);
            View.BackgroundColor = UIColor.Gray;

            // fit three buttons on the row
            double buttonWidth = View.Bounds.Width / 3.0;

            // button to start an edit transaction
            _startEditingButton.SetTitle("Start", UIControlState.Normal);
            _startEditingButton.SetTitleColor(UIColor.LightGray, UIControlState.Disabled);
            _startEditingButton.Frame = new CoreGraphics.CGRect(0, 0, buttonWidth, 30);
            _startEditingButton.TouchUpInside += BeginTransaction;

            // button to stop a transaction
            _stopEditingButton.SetTitle("Stop", UIControlState.Normal);
            _stopEditingButton.Enabled = false;
            _stopEditingButton.SetTitleColor(UIColor.LightGray, UIControlState.Disabled);
            _stopEditingButton.Frame = new CoreGraphics.CGRect(buttonWidth, 0, buttonWidth, 30);
            _stopEditingButton.TouchUpInside += StopEditTransaction;

            // button to synchronize local edits with the service
            _syncEditsButton.SetTitle("Sync", UIControlState.Normal);
            _syncEditsButton.SetTitleColor(UIColor.LightGray, UIControlState.Disabled);
            _syncEditsButton.Enabled = false;
            _syncEditsButton.Frame = new CoreGraphics.CGRect(buttonWidth * 2, 0, buttonWidth, 30);
            _syncEditsButton.TouchUpInside += SynchronizeEdits;

            // two buttons on this row
            buttonWidth = View.Bounds.Width / 2.0;

            // button to add bird features
            _addBirdButton.SetTitle("Add Bird", UIControlState.Normal);
            _addBirdButton.Enabled = false;
            _addBirdButton.SetTitleColor(UIColor.LightGray, UIControlState.Disabled);
            _addBirdButton.Frame = new CoreGraphics.CGRect(0, 0, buttonWidth, 30);
            _addBirdButton.TouchUpInside += AddNewFeature;

            // button to add marine features
            _addMarineButton.SetTitle("Add Marine", UIControlState.Normal);
            _addMarineButton.Enabled = false;
            _addMarineButton.SetTitleColor(UIColor.LightGray, UIControlState.Disabled);
            _addMarineButton.Frame = new CoreGraphics.CGRect(buttonWidth, 0, buttonWidth, 30);
            _addMarineButton.TouchUpInside += AddNewFeature;

            _editToolsView.Axis = UILayoutConstraintAxis.Vertical;
            _editToolsView.Frame = new CoreGraphics.CGRect(0, _mapViewHeight, View.Bounds.Width, _editToolsHeight);

            // view to hold the first row of buttons (start, stop, sync)
            UIStackView editButtonsRow1 = new UIStackView();
            editButtonsRow1.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, 35);
            editButtonsRow1.Axis = UILayoutConstraintAxis.Horizontal;
            editButtonsRow1.Add(_startEditingButton);
            editButtonsRow1.Add(_stopEditingButton);
            editButtonsRow1.Add(_syncEditsButton);
            
            // view to hold the second row of buttons (add bird, add marine)
            UIStackView editButtonsRow2 = new UIStackView();
            editButtonsRow2.Frame = new CoreGraphics.CGRect(0, 35, View.Bounds.Width, 35);
            editButtonsRow2.Axis = UILayoutConstraintAxis.Horizontal;
            editButtonsRow2.Add(_addBirdButton);
            editButtonsRow2.Add(_addMarineButton);

            // view for the 'require transaction' switch
            UIStackView editSwitchRow = new UIStackView();
            editSwitchRow.Frame = new CoreGraphics.CGRect(0, 70, View.Bounds.Width, 35);
            editSwitchRow.Axis = UILayoutConstraintAxis.Horizontal;
            _requireTransactionSwitch.ValueChanged += RequireTransactionChanged;
            editSwitchRow.Add(_requireTransactionSwitch);

            // create a label that describes the switch value
            UILabel switchLabel = new UILabel();
            switchLabel.Text = "Require transaction";
            switchLabel.Frame = new CoreGraphics.CGRect(70, 0, View.Bounds.Width - 70, 30);
            editSwitchRow.Add(switchLabel);
            
            // progress bar
            _progressBar.Frame = new CoreGraphics.CGRect(0, 105, View.Bounds.Width, 10);

            // use the rest of the view to show status messages
            _messageTextBlock.Editable = false;
            _messageTextBlock.Frame = new CoreGraphics.CGRect(0, 115, View.Bounds.Width, _editToolsHeight);

            // add the first row of buttons
            _editToolsView.Add(editButtonsRow1);

            // add the second row of buttons
            _editToolsView.Add(editButtonsRow2);

            // add the 'require transaction' switch and label
            _editToolsView.Add(editSwitchRow);

            // add the messages text view
            _editToolsView.Add(_messageTextBlock);

            // add the progress bar
            _editToolsView.Add(_progressBar);

            // Add the views
            View.AddSubviews(_mapView, _editToolsView);
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
            // get the path to the local geodatabase for this platform (temp directory, for example)
            var localGeodatabasePath = GetGdbPath();

            try
            {
                // see if the geodatabase file is already present
                if (File.Exists(localGeodatabasePath))
                {
                    // if the geodatabase is already available, open it, hide the progress control, and update the message
                    _localGeodatabase = await Geodatabase.OpenAsync(localGeodatabasePath);
                    _progressBar.Hidden = true;
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
                        InvokeOnMainThread(() => UpdateProgressBar(generateGdbJob.Progress));

                        // see if the job succeeded
                        if (generateGdbJob.Status == JobStatus.Succeeded)
                        {
                            InvokeOnMainThread(() =>
                            {
                                // hide the progress control and update the message
                                _progressBar.Hidden = true;
                                _messageTextBlock.Text = "Created local geodatabase";
                            });
                        }
                        else if (generateGdbJob.Status == JobStatus.Failed)
                        {
                            InvokeOnMainThread(() =>
                            {
                                // hide the progress control and report the exception
                                _progressBar.Hidden = true;
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
                InvokeOnMainThread(() =>
                {
                    ShowMessage("Generate Geodatabase", "Unable to create offline database: " + ex.Message, "OK");
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
                InvokeOnMainThread(() => _mapView.Map.OperationalLayers.Add(layer));
            }

            // handle the transaction status changed event
            _localGeodatabase.TransactionStatusChanged += GdbTransactionStatusChanged;

            // zoom the map view to the extent of the generated local datasets
            InvokeOnMainThread(() =>
            {
                _mapView.SetViewpointGeometryAsync(_marineTable.Extent);
                _startEditingButton.Enabled = true;
            });
        }

        private void GdbTransactionStatusChanged(object sender, TransactionStatusChangedEventArgs e)
        {
            // update UI controls based on whether the geodatabase has a current transaction
            InvokeOnMainThread(() =>
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
            UIButton addFeatureButton = sender as UIButton;

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
            // create an alert to ask the user if they want to commit or rollback the transaction (or cancel to keep working in the transaction)
            UIAlertController endTransactionAlertController = UIAlertController.Create("Stop Editing", "Commit your edits or roll back?", UIAlertControllerStyle.Alert);

            // action when user chooses to commit
            endTransactionAlertController.AddAction(UIAlertAction.Create("Commit", UIAlertActionStyle.Default, 
                alert => {
                    // see if there is a transaction active for the geodatabase
                    if (_localGeodatabase.IsInTransaction)
                    {
                        // if there is, commit the transaction to store the edits (this will also end the transaction)
                        _localGeodatabase.CommitTransaction();
                        _messageTextBlock.Text = "Edits were committed to the local geodatabase.";
                    }
                }));

            // action when user chooses to rollback
            endTransactionAlertController.AddAction(UIAlertAction.Create("Rollback", UIAlertActionStyle.Default, 
                alert => {
                    // see if there is a transaction active for the geodatabase
                    if (_localGeodatabase.IsInTransaction)
                    {
                        // if there is, rollback the transaction to discard the edits (this will also end the transaction)
                        _localGeodatabase.RollbackTransaction();
                        _messageTextBlock.Text = "Edits were rolled back and not stored to the local geodatabase.";
                    }
                }));

            // action for cancel (ignore)
            endTransactionAlertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

            // present the alert to the user
            PresentViewController(endTransactionAlertController, true, null);
        }

        // change which controls are enabled if the user chooses to require/not require transactions for edits
        private void RequireTransactionChanged(object sender, EventArgs e)
        {
            // if the local geodatabase isn't created yet, return
            if (_localGeodatabase == null) { return; }

            // get the value of the "require transactions" switch
            bool mustHaveTransaction = _requireTransactionSwitch.On;

            // warn the user if disabling transactions while a transaction is active
            if (!mustHaveTransaction && _localGeodatabase.IsInTransaction)
            {
                ShowMessage("Stop editing to end the current transaction.", "Current Transaction", "OK");
                _requireTransactionSwitch.On = true;
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
            _progressBar.Hidden = false;
            
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
                    InvokeOnMainThread(() =>
                    {
                        // update the progress bar
                        UpdateProgressBar(job.Progress);

                        // report changes in the job status
                        if (job.Status == JobStatus.Succeeded)
                        {
                                _messageTextBlock.Text = "Synchronization is complete!";
                                _progressBar.Hidden = true;
                        }
                        else if (job.Status == JobStatus.Failed)
                        {
                            // report failure ...
                                _messageTextBlock.Text = job.Error.Message;
                                _progressBar.Hidden = true;
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