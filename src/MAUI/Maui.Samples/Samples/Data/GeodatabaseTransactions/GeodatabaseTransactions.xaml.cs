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

#if WINDOWS_UWP
using Colors = Windows.UI.Colors;
#else
#endif

namespace ArcGISMapsSDKMaui.Samples.GeodatabaseTransactions
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Geodatabase transactions",
        category: "Data",
        description: "Use transactions to manage how changes are committed to a geodatabase.",
        instructions: "When the sample loads, a feature service is taken offline as a geodatabase. When the geodatabase is ready, you can add multiple types of features. To apply edits directly, uncheck the 'Require a transaction for edits' checkbox. When using transactions, use the buttons to start editing and stop editing. When you stop editing, you can choose to commit the changes or roll them back. At any point, you can synchronize the local geodatabase with the feature service.",
        tags: new[] { "commit", "database", "geodatabase", "transact", "transactions" })]
    public partial class GeodatabaseTransactions : ContentPage
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

            // When the spatial reference changes (the map loads) add the local geodatabase tables as feature layers
            MyMapView.SpatialReferenceChanged += async (s, e) =>
            {
                // Call a function (and await it) to get the local geodatabase (or generate it from the feature service)
                await GetLocalGeodatabase();

                // Once the local geodatabase is available, load the tables as layers to the map
                await LoadLocalGeodatabaseTables();
            };

            // Create a new map with the oceans basemap and add it to the map view
            Map map = new Map(BasemapStyle.ArcGISOceans);
            MyMapView.Map = map;
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
                    LoadingProgressBar.IsVisible = false;
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
                            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
                            {
                                // Hide the progress control and update the message
                                LoadingProgressBar.IsVisible = false;
                                MessageTextBlock.Text = "Created local geodatabase";
                            });
                        }
                        else if (generateGdbJob.Status == JobStatus.Failed)
                        {
                            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
                            {
                                // Hide the progress control and report the exception
                                LoadingProgressBar.IsVisible = false;
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
                Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
                {
                    Application.Current.MainPage.DisplayAlert("Generate Geodatabase", "Unable to create offline database: " + ex.Message, "OK");
                });
            }
        }

        // Function that loads the two point tables from the local geodatabase and displays them as feature layers
        private async Task LoadLocalGeodatabaseTables()
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
                    Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() => MyMapView.Map.OperationalLayers.Add(layer));
                }
                catch (Exception e)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", e.ToString(), "OK");
                }
            }

            // Handle the transaction status changed event
            _localGeodatabase.TransactionStatusChanged += GdbTransactionStatusChanged;

            // Zoom the map view to the extent of the generated local datasets
            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
            {
                MyMapView.SetViewpoint(new Viewpoint(_marineTable.Extent));
                StartEditingButton.IsEnabled = true;
                SyncEditsButton.IsEnabled = true;
            });
        }

        private void GdbTransactionStatusChanged(object sender, TransactionStatusChangedEventArgs e)
        {
            // Update UI controls based on whether the geodatabase has a current transaction
            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
            {
                // These buttons should be enabled when there IS a transaction
                AddBirdButton.IsEnabled = e.IsInTransaction;
                AddMarineButton.IsEnabled = e.IsInTransaction;
                StopEditingButton.IsEnabled = e.IsInTransaction;

                // These buttons should be enabled when there is NOT a transaction
                StartEditingButton.IsEnabled = !e.IsInTransaction;
                SyncEditsButton.IsEnabled = !e.IsInTransaction;
                RequireTransactionCheckBox.IsEnabled = !e.IsInTransaction;
            });
        }

        private string GetGdbPath()
        {
            // Set the platform-specific path for storing the geodatabase
            string folder = string.Empty;
#if WINDOWS
            folder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#elif IOS || ANDROID
            folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#endif
            // Set the final path
            return Path.Combine(folder, "savethebay.geodatabase");
        }

        private void BeginTransaction(object sender, EventArgs e)
        {
            // See if there is a transaction active for the geodatabase
            if (!_localGeodatabase.IsInTransaction)
            {
                // If not, begin a transaction
                _localGeodatabase.BeginTransaction();
                MessageTextBlock.Text = "Transaction started";
            }
        }

        private async void AddNewFeature(object sender, EventArgs args)
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
                MapPoint clickPoint = await MyMapView.SketchEditor.StartAsync(SketchCreationMode.Point, false) as MapPoint;

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

        private async void StopEditTransaction(object sender, EventArgs e)
        {
            try
            {
                // Ask the user if they want to commit or rollback the transaction (or cancel to keep working in the transaction)
                string choice = await Application.Current.MainPage.DisplayActionSheet("Transaction", "Cancel", null, "Commit", "Rollback");

                if (choice == "Commit")
                {
                    // See if there is a transaction active for the geodatabase
                    if (_localGeodatabase.IsInTransaction)
                    {
                        // If there is, commit the transaction to store the edits (this will also end the transaction)
                        _localGeodatabase.CommitTransaction();
                        MessageTextBlock.Text = "Edits were committed to the local geodatabase.";
                    }
                }
                else if (choice == "Rollback")
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
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        // Change which controls are enabled if the user chooses to require/not require transactions for edits
        private void RequireTransactionChanged(object sender, EventArgs e)
        {
            // If the local geodatabase isn't created yet, return
            if (_localGeodatabase == null) { return; }

            // Get the value of the "require transactions" checkbox
            bool mustHaveTransaction = RequireTransactionCheckBox.IsToggled;

            // Warn the user if disabling transactions while a transaction is active
            if (!mustHaveTransaction && _localGeodatabase.IsInTransaction)
            {
                Application.Current.MainPage.DisplayAlert("Stop editing to end the current transaction.", "Current Transaction", "OK");
                RequireTransactionCheckBox.IsToggled = true;
                return;
            }

            // Enable or disable controls according to the checkbox value
            StartEditingButton.IsEnabled = mustHaveTransaction;
            StopEditingButton.IsEnabled = mustHaveTransaction && _localGeodatabase.IsInTransaction;
            AddBirdButton.IsEnabled = !mustHaveTransaction;
            AddMarineButton.IsEnabled = !mustHaveTransaction;
        }

        // Synchronize edits in the local geodatabase with the service
        public async void SynchronizeEdits(object sender, EventArgs e)
        {
            // Show the progress bar while the sync is working
            LoadingProgressBar.IsVisible = true;

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
                        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() => MessageTextBlock.Text = "Synchronization is complete!");
                    }
                    else if (job.Status == JobStatus.Failed)
                    {
                        // Report failure ...
                        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() => MessageTextBlock.Text = job.Error.Message);
                    }
                    else
                    {
                        // Report that the job is in progress ...
                        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() => MessageTextBlock.Text = "Sync in progress ...");
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
                LoadingProgressBar.IsVisible = false;
            }
        }
    }
}