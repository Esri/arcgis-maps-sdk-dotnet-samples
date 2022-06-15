// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// Language governing permissions and limitations under the License.

using System;
using System.IO;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.GeodatabaseTransactions
{
    [Register("GeodatabaseTransactions")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Geodatabase transactions",
        category: "Data",
        description: "Use transactions to manage how changes are committed to a geodatabase.",
        instructions: "When the sample loads, a feature service is taken offline as a geodatabase. When the geodatabase is ready, you can add multiple types of features. To apply edits directly, uncheck the 'Require a transaction for edits' checkbox. When using transactions, use the buttons to start editing and stop editing. When you stop editing, you can choose to commit the changes or roll them back. At any point, you can synchronize the local geodatabase with the feature service.",
        tags: new[] { "commit", "database", "geodatabase", "transact", "transactions" })]
    public class GeodatabaseTransactions : UIViewController
    {
        // Hold references to UI controls.
        private MapView _mapView;
        private UIProgressView _progressBar;
        private UILabel _statusLabel;
        private UISwitch _transactionSwitch;
        private UIBarButtonItem _transactionButton;
        private UIBarButtonItem _syncButton;
        private UIBarButtonItem _addButton;

        // URL for the editable feature service.
        private const string SyncServiceUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Sync/SaveTheBaySync/FeatureServer/";

        // Work in a small extent south of Galveston, TX.
        private readonly Envelope _extent = new Envelope(-95.3035, 29.0100, -95.1053, 29.1298, SpatialReferences.Wgs84);

        // Store the local geodatabase to edit.
        private Geodatabase _localGeodatabase;

        // Store references to two tables to edit: Birds and Marine points.
        private GeodatabaseFeatureTable _birdTable;
        private GeodatabaseFeatureTable _marineTable;

        public GeodatabaseTransactions()
        {
            Title = "Geodatabase transactions";
        }

        private void Initialize()
        {
            // When the spatial reference changes (the map loads) add the local geodatabase tables as feature layers.
            _mapView.SpatialReferenceChanged += MapView_SpatialReferenceChanged;

            // Create a new map with the oceans basemap and add it to the map view.
            _mapView.Map = new Map(BasemapStyle.ArcGISOceans);
        }

        private async void MapView_SpatialReferenceChanged(object sender, EventArgs e)
        {
            // Unsubscribe from the event.
            _mapView.SpatialReferenceChanged -= MapView_SpatialReferenceChanged;

            // Call a function (and await it) to get the local geodatabase (or generate it from the feature service).
            await GetLocalGeodatabase();

            // Once the local geodatabase is available, load the tables as layers to the map.
            await LoadLocalGeodatabaseTables();
        }

        private void ShowMessage(string title, string message, string buttonText)
        {
            // Display the message to the user.
            UIAlertView alertView = new UIAlertView(title, message, (IUIAlertViewDelegate) null, buttonText, null);
            alertView.Show();
        }

        private async Task GetLocalGeodatabase()
        {
            string localGeodatabasePath = GetGdbPath();

            try
            {
                // See if the geodatabase file is already present.
                if (File.Exists(localGeodatabasePath))
                {
                    // If the geodatabase is already available, open it, hide the progress control, and update the message.
                    _localGeodatabase = await Geodatabase.OpenAsync(localGeodatabasePath);
                    _progressBar.Hidden = true;
                    _statusLabel.Text = "Using local geodatabase.";
                }
                else
                {
                    // Create a new GeodatabaseSyncTask with the URI of the feature server to pull from.
                    Uri uri = new Uri(SyncServiceUrl);
                    GeodatabaseSyncTask gdbTask = await GeodatabaseSyncTask.CreateAsync(uri);

                    // Create parameters for the task: layers and extent to include, out spatial reference, and sync model.
                    GenerateGeodatabaseParameters gdbParams = await gdbTask.CreateDefaultGenerateGeodatabaseParametersAsync(_extent);
                    gdbParams.OutSpatialReference = _mapView.SpatialReference;
                    gdbParams.SyncModel = SyncModel.Layer;
                    gdbParams.LayerOptions.Clear();
                    gdbParams.LayerOptions.Add(new GenerateLayerOption(0));
                    gdbParams.LayerOptions.Add(new GenerateLayerOption(1));

                    // Create a geodatabase job that generates the geodatabase.
                    GenerateGeodatabaseJob generateGdbJob = gdbTask.GenerateGeodatabase(gdbParams, localGeodatabasePath);

                    // Handle the job changed event and check the status of the job; store the geodatabase when it's ready.
                    void GenerateGdbJob_JobStatusChanged(object jobSender, JobStatus e)
                    {
                        // Call a function to update the progress bar.
                        InvokeOnMainThread(() => UpdateProgressBar(generateGdbJob.Progress));

                        switch (generateGdbJob.Status)
                        {
                            // See if the job succeeded.
                            case JobStatus.Succeeded:
                                // Unsubscribe from event.
                                generateGdbJob.StatusChanged -= GenerateGdbJob_JobStatusChanged;

                                InvokeOnMainThread(() =>
                                {
                                    // Hide the progress control and update the message.
                                    _progressBar.Hidden = true;
                                    _statusLabel.Text = "Created local geodatabase";
                                });
                                break;
                            case JobStatus.Failed:
                                // Unsubscribe from event.
                                generateGdbJob.StatusChanged -= GenerateGdbJob_JobStatusChanged;

                                InvokeOnMainThread(() =>
                                {
                                    // Hide the progress control and report the exception.
                                    _progressBar.Hidden = true;
                                    _statusLabel.Text = "Unable to create local geodatabase: " + generateGdbJob.Error.Message;
                                });
                                break;
                        }
                    }

                    // Subscribe to job change event.
                    generateGdbJob.StatusChanged += GenerateGdbJob_JobStatusChanged;

                    // Start the generate geodatabase job.
                    _localGeodatabase = await generateGdbJob.GetResultAsync();
                }
            }
            catch (Exception ex)
            {
                // Show a message for the exception encountered.
                InvokeOnMainThread(() => { ShowMessage("Generate Geodatabase", "Unable to create offline database: " + ex.Message, "OK"); });
            }
        }

        private static string GetGdbPath()
        {
            // Get the path to the local geodatabase for this platform (temp directory, for example).
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "savethebay.geodatabase");
        }

        // Function that loads the two point tables from the local geodatabase and displays them as feature layers.
        private async Task LoadLocalGeodatabaseTables()
        {
            if (_localGeodatabase == null)
            {
                return;
            }

            // Read the geodatabase tables and add them as layers.
            foreach (GeodatabaseFeatureTable table in _localGeodatabase.GeodatabaseFeatureTables)
            {
                try
                {
                    // Load the table so the TableName can be read.
                    await table.LoadAsync();

                    // Store a reference to the Birds table.
                    if (table.TableName.ToLower().Contains("birds"))
                    {
                        _birdTable = table;
                    }

                    // Store a reference to the Marine table.
                    if (table.TableName.ToLower().Contains("marine"))
                    {
                        _marineTable = table;
                    }

                    // Create a new feature layer to show the table in the map.
                    FeatureLayer layer = new FeatureLayer(table);
                    InvokeOnMainThread(() => _mapView.Map.OperationalLayers.Add(layer));
                }
                catch (Exception e)
                {
                    new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
                }
            }

            // Handle the transaction status changed event.
            _localGeodatabase.TransactionStatusChanged += GdbTransactionStatusChanged;

            // Zoom the map view to the extent of the generated local datasets.
            InvokeOnMainThread(() =>
            {
                _mapView.SetViewpoint(new Viewpoint(_marineTable.Extent));
                _transactionButton.Enabled = true;
            });
        }

        private void GdbTransactionStatusChanged(object sender, TransactionStatusChangedEventArgs e)
        {
            // Update UI controls based on whether the geodatabase has a current transaction.
            InvokeOnMainThread(() =>
            {
                // These buttons should be enabled when there IS a transaction.
                _addButton.Enabled = e.IsInTransaction;

                // These buttons should be enabled when there is NOT a transaction.
                _syncButton.Enabled = !e.IsInTransaction;
            });
        }

        private void BeginTransaction()
        {
            try
            {
                // See if there is a transaction active for the geodatabase.
                if (!_localGeodatabase.IsInTransaction)
                {
                    // If not, begin a transaction.
                    _localGeodatabase.BeginTransaction();
                    _statusLabel.Text = "Transaction started";
                }
            }
            catch (Exception)
            {
                _statusLabel.Text = "The geodatabase isn't ready yet.";
            }
        }

        private async void AddNewFeature(bool isMarine)
        {
            try
            {
                // Cancel execution of the sketch task if it is already active.
                if (_mapView.SketchEditor.CancelCommand.CanExecute(null))
                {
                    _mapView.SketchEditor.CancelCommand.Execute(null);
                }

                // Store the correct table to edit (for the button clicked).
                GeodatabaseFeatureTable editTable = isMarine ? _marineTable : _birdTable;

                // Inform the user which table is being edited.
                _statusLabel.Text = "Click the map to add a new feature to the geodatabase table '" + editTable.TableName + "'";

                // Create a random value for the 'type' attribute (integer between 1 and 7).
                Random random = new Random(DateTime.Now.Millisecond);
                int featureType = random.Next(1, 7);

                // Use the sketch editor to allow the user to draw a point on the map.
                MapPoint clickPoint = await _mapView.SketchEditor.StartAsync(SketchCreationMode.Point, false) as MapPoint;

                // Create a new feature (row) in the selected table.
                Feature newFeature = editTable.CreateFeature();

                // Set the geometry with the point the user clicked and the 'type' with the random integer.
                newFeature.Geometry = clickPoint;
                newFeature.SetAttributeValue("type", featureType);

                // Add the new feature to the table.
                await editTable.AddFeatureAsync(newFeature);

                // Clear the message.
                _statusLabel.Text = "New feature added to the '" + editTable.TableName + "' table";
            }
            catch (TaskCanceledException)
            {
                // Ignore if the edit was canceled.
            }
            catch (Exception ex)
            {
                // Report other exception messages.
                _statusLabel.Text = ex.Message;
            }
        }

        private void StopEditTransaction()
        {
            // Create an alert to ask the user if they want to commit or rollback the transaction (or cancel to keep working in the transaction).
            UIAlertController endTransactionAlertController = UIAlertController.Create("Stop Editing", "Commit your edits or roll back?", UIAlertControllerStyle.Alert);

            // Action when user chooses to commit.
            endTransactionAlertController.AddAction(UIAlertAction.Create("Commit", UIAlertActionStyle.Default,
                alert =>
                {
                    // See if there is a transaction active for the geodatabase.
                    if (_localGeodatabase.IsInTransaction)
                    {
                        // If there is, commit the transaction to store the edits (this will also end the transaction).
                        _localGeodatabase.CommitTransaction();
                        _statusLabel.Text = "Edits were committed to the local geodatabase.";
                    }
                }));

            // Action when user chooses to rollback.
            endTransactionAlertController.AddAction(UIAlertAction.Create("Rollback", UIAlertActionStyle.Default,
                alert =>
                {
                    // See if there is a transaction active for the geodatabase.
                    if (_localGeodatabase.IsInTransaction)
                    {
                        // If there is, rollback the transaction to discard the edits (this will also end the transaction).
                        _localGeodatabase.RollbackTransaction();
                        _statusLabel.Text = "Edits were rolled back and not stored to the local geodatabase.";
                    }
                }));

            // Action for cancel (ignore).
            endTransactionAlertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

            // Present the alert to the user.
            PresentViewController(endTransactionAlertController, true, null);
        }

        // Change which controls are enabled if the user chooses to require/not require transactions for edits.
        private void RequireTransactionChanged(object sender, EventArgs e)
        {
            // If the local geodatabase isn't created yet, return.
            if (_localGeodatabase == null)
            {
                return;
            }

            // Get the value of the "require transactions" switch.
            bool mustHaveTransaction = _transactionSwitch.On;

            // Warn the user if disabling transactions while a transaction is active.
            if (!mustHaveTransaction && _localGeodatabase.IsInTransaction)
            {
                ShowMessage("Stop editing to end the current transaction.", "Current Transaction", "OK");
                _transactionSwitch.On = true;
                return;
            }

            // Enable or disable controls according to the switch value.
            _addButton.Enabled = !mustHaveTransaction;
        }

        // Synchronize edits in the local geodatabase with the service.
        private async void SynchronizeEdits(object sender, EventArgs e)
        {
            // Show the progress bar while the sync is working.
            _progressBar.Hidden = false;

            try
            {
                // Create a sync task with the URL of the feature service to sync.
                GeodatabaseSyncTask syncTask = await GeodatabaseSyncTask.CreateAsync(new Uri(SyncServiceUrl));

                // Create sync parameters.
                SyncGeodatabaseParameters taskParameters = await syncTask.CreateDefaultSyncGeodatabaseParametersAsync(_localGeodatabase);

                // Create a synchronize geodatabase job, pass in the parameters and the geodatabase.
                SyncGeodatabaseJob job = syncTask.SyncGeodatabase(taskParameters, _localGeodatabase);

                void Job_JobStatusChanged(object sendingJob, JobStatus args)
                {
                    InvokeOnMainThread(() =>
                    {
                        // Update the progress bar.
                        UpdateProgressBar(job.Progress);

                        switch (job.Status)
                        {
                            // Report changes in the job status.
                            case JobStatus.Succeeded:
                                // Unsubscribe
                                ((SyncGeodatabaseJob) sendingJob).StatusChanged -= Job_JobStatusChanged;
                                // Report success.
                                _statusLabel.Text = "Synchronization is complete!";
                                _progressBar.Hidden = true;
                                break;
                            case JobStatus.Failed:
                                // Unsubscribe
                                ((SyncGeodatabaseJob) sendingJob).StatusChanged -= Job_JobStatusChanged;
                                // Report failure.
                                _statusLabel.Text = job.Error.Message;
                                _progressBar.Hidden = true;
                                break;
                            default:
                                _statusLabel.Text = "Sync in progress ...";
                                break;
                        }
                    });
                }

                // Handle the JobChanged event for the job.
                job.StatusChanged += Job_JobStatusChanged;

                // Await the completion of the job.
                await job.GetResultAsync();
            }
            catch (Exception ex)
            {
                // Show the message if an exception occurred.
                _statusLabel.Text = "Error when synchronizing: " + ex.Message;
            }
        }

        private void UpdateProgressBar(int jobProgress)
        {
            // Needed because this could be called from a non-UI thread.
            InvokeOnMainThread(() =>
            {
                // Update the progress bar value.
                _progressBar.Progress = (float) (jobProgress / 100.0);
            });
        }

        private void HandleAddButton_Click(object sender, EventArgs e)
        {
            // Create the alert controller with a title.
            UIAlertController alertController = UIAlertController.Create("Choose a feature to add", "", UIAlertControllerStyle.Alert);

            // Actions can be default, cancel, or destructive
            alertController.AddAction(UIAlertAction.Create("Bird", UIAlertActionStyle.Default, action => AddNewFeature(false)));
            alertController.AddAction(UIAlertAction.Create("Marine", UIAlertActionStyle.Default, action => AddNewFeature(true)));
            alertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

            // Show the alert.
            PresentViewController(alertController, true, null);
        }

        private void HandleTransaction_Click(object sender, EventArgs e)
        {
            if (_transactionButton.Title == "Start transaction")
            {
                BeginTransaction();
                _transactionButton.Title = "End transaction";
            }
            else
            {
                StopEditTransaction();
                _transactionButton.Title = "Start transaction";
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = ApplicationTheme.BackgroundColor};

            _mapView = new MapView();
            _mapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _transactionButton = new UIBarButtonItem();
            _transactionButton.Title = "Start transaction";
            _syncButton = new UIBarButtonItem();
            _syncButton.Title = "Sync";
            _addButton = new UIBarButtonItem(UIBarButtonSystemItem.Add);
            _addButton.Enabled = false;

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                _transactionButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _syncButton,
                _addButton
            };

            _transactionSwitch = new UISwitch();
            _transactionSwitch.TranslatesAutoresizingMaskIntoConstraints = false;
            _transactionSwitch.On = true;

            _statusLabel = new UILabel
            {
                Text = "Preparing sample data...",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Center,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f),
                TextColor = UIColor.White,
                Lines = 2,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            _statusLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            UILabel requireTransactionsLabel = new UILabel();
            requireTransactionsLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            requireTransactionsLabel.Text = "Require transaction";
            requireTransactionsLabel.TextAlignment = UITextAlignment.Right;
            requireTransactionsLabel.SetContentCompressionResistancePriority((float) UILayoutPriority.DefaultHigh, UILayoutConstraintAxis.Horizontal);

            UIStackView requireTransactionRow = new UIStackView(new UIView[] {requireTransactionsLabel, _transactionSwitch});
            requireTransactionRow.TranslatesAutoresizingMaskIntoConstraints = false;
            requireTransactionRow.Axis = UILayoutConstraintAxis.Horizontal;
            requireTransactionRow.Distribution = UIStackViewDistribution.Fill;
            requireTransactionRow.Spacing = 8;
            requireTransactionRow.LayoutMarginsRelativeArrangement = true;
            requireTransactionRow.LayoutMargins = new UIEdgeInsets(8, 8, 8, 8);

            _progressBar = new UIProgressView(UIProgressViewStyle.Bar);
            _progressBar.TranslatesAutoresizingMaskIntoConstraints = false;

            // Add the views.
            View.AddSubviews(_mapView, _statusLabel, requireTransactionRow, toolbar, _progressBar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _mapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _mapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _mapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _mapView.BottomAnchor.ConstraintEqualTo(requireTransactionRow.TopAnchor),

                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),

                requireTransactionRow.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor),
                requireTransactionRow.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor),
                requireTransactionRow.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),

                _statusLabel.TopAnchor.ConstraintEqualTo(_mapView.TopAnchor),
                _statusLabel.LeadingAnchor.ConstraintEqualTo(_mapView.LeadingAnchor),
                _statusLabel.TrailingAnchor.ConstraintEqualTo(_mapView.TrailingAnchor),
                _statusLabel.HeightAnchor.ConstraintEqualTo(80),

                _progressBar.TopAnchor.ConstraintEqualTo(_statusLabel.BottomAnchor),
                _progressBar.LeadingAnchor.ConstraintEqualTo(_statusLabel.LeadingAnchor),
                _progressBar.TrailingAnchor.ConstraintEqualTo(_statusLabel.TrailingAnchor),
                _progressBar.HeightAnchor.ConstraintEqualTo(8)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            if (_localGeodatabase != null) _localGeodatabase.TransactionStatusChanged += GdbTransactionStatusChanged;
            _transactionSwitch.ValueChanged += RequireTransactionChanged;
            _transactionButton.Clicked += HandleTransaction_Click;
            _syncButton.Clicked += SynchronizeEdits;
            _addButton.Clicked += HandleAddButton_Click;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            if (_localGeodatabase != null) _localGeodatabase.TransactionStatusChanged -= GdbTransactionStatusChanged;
            _transactionSwitch.ValueChanged -= RequireTransactionChanged;
            _transactionButton.Clicked -= HandleTransaction_Click;
            _syncButton.Clicked -= SynchronizeEdits;
            _addButton.Clicked -= HandleAddButton_Click;
        }
    }
}