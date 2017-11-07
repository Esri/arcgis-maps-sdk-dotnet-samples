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
using Esri.ArcGISRuntime.Tasks.Offline;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ArcGISRuntime.WPF.Samples.GeodatabaseTransactions
{
    public partial class GeodatabaseTransactions
    {
        // url for the editable feature service
        private const string SyncServiceUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Sync/SaveTheBaySync/FeatureServer/";

        // work in a small extent south of Galveston, TX
        private Envelope _extent = new Envelope(-95.3035, 29.0100, -95.1053, 29.1298, SpatialReferences.Wgs84);

        // store the local geodatabase to edit with transactions
        private Geodatabase _localGeodatabase;

        public GeodatabaseTransactions()
        {
            InitializeComponent();

            // when the map view loads, add a new map and start generating a local geodatabase
            MyMapView.Loaded += (s, e) =>
            {
                // create a new map with the oceans basemap and add it to the map view
                var map = new Map(Basemap.CreateOceans());
                MyMapView.Map = map;

                // call a function to get the local geodatabase (or generate it from the feature service)
                GetLocalGeodatabase();
            };
        }

        private async void GetLocalGeodatabase()
        {
            // get the path to the local geodatabase for this platform (temp directory, for example)
            string localGeodatabasePath = GetGdbPath();

            try
            {
                // See if the geodatabase file is already present
                if (System.IO.File.Exists(localGeodatabasePath))
                {
                    // if the geodatabase is already available, open it and hide the progress controls
                    _localGeodatabase = await Geodatabase.OpenAsync(localGeodatabasePath);
                    LoadingProgressBar.Visibility = Visibility.Collapsed;
                    LoadingText.Visibility = Visibility.Collapsed;
                    return;
                }

                // create a new GeodatabaseSyncTask with the uri of the feature server to pull from
                var uri = new Uri(SyncServiceUrl);
                var gdbTask = await GeodatabaseSyncTask.CreateAsync(uri);

                // create parameters for the task: layers and extent to include, out spatial reference, and sync model
                var gdbParams = await gdbTask.CreateDefaultGenerateGeodatabaseParametersAsync(_extent);
                gdbParams.OutSpatialReference = MyMapView.SpatialReference;
                gdbParams.SyncModel = SyncModel.Layer;
                gdbParams.LayerOptions.Clear();
                gdbParams.LayerOptions.Add(new GenerateLayerOption(0));
                gdbParams.LayerOptions.Add(new GenerateLayerOption(1));

                // create a geodatabase job that generates the geodatabase
                GenerateGeodatabaseJob generateGdbJob = gdbTask.GenerateGeodatabase(gdbParams, localGeodatabasePath);

                // handle the job changed event and check the status of the job; store the geodatabase when it's ready
                generateGdbJob.JobChanged += async (s, e) =>
                {
                    // see if the job succeeded
                    if (generateGdbJob.Status == Esri.ArcGISRuntime.Tasks.JobStatus.Succeeded)
                    {
                        // store the local geodatabase from the job result
                        _localGeodatabase = await generateGdbJob.GetResultAsync();
                        this.Dispatcher.Invoke(() =>
                        {
                            // hide the progress controls
                            LoadingProgressBar.Visibility = Visibility.Collapsed;
                            LoadingText.Visibility = Visibility.Collapsed;
                        });
                    }
                };

                // start the generate geodatabase job
                generateGdbJob.Start();
            }
            catch (Exception ex)
            {
                // show a message for the exception encountered
                this.Dispatcher.Invoke(() => MessageBox.Show("Unable to create offline database: " + ex.Message));
            }
        }

        // function that loads the two point tables from the local geodatabase and displays them as feature layers
        private void LoadLocalGeodatabaseTables()
        {
            // read the geodatabase tables and add them as layers
            foreach (var table in _localGeodatabase.GeodatabaseFeatureTables)
            {
                var layer = new FeatureLayer(table);
                this.Dispatcher.Invoke(() => MyMapView.Map.OperationalLayers.Add(layer));
            }

            // handle the transaction status changed event
            _localGeodatabase.TransactionStatusChanged += GdbTransactionStatusChanged;

            // show the geodatabase tables in a combo box so the user can choose which one to edit
            TableComboBox.ItemsSource = _localGeodatabase.GeodatabaseFeatureTables;
            TableComboBox.DisplayMemberPath = "TableName";

            // zoom the map view to the extent of the generated local datasets
            this.Dispatcher.Invoke(() => MyMapView.SetViewpointGeometryAsync(_extent));
        }

        private void GdbTransactionStatusChanged(object sender, TransactionStatusChangedEventArgs e)
        {
            // update the UI to show the status of geodatabase transactions
            if (e.IsInTransaction)
            {
                // update label to state the geodatabase has a current transaction, make the label green
                this.Dispatcher.Invoke(() =>
                {
                    TransactionStatusLabel.Background = new SolidColorBrush(Colors.Green);
                    TransactionStatusLabel.Foreground = new SolidColorBrush(Colors.Black);
                    TransactionStatusLabel.Text = "IN TRANSACTION";
                });
            }
            else
            {
                // update label to state the geodatabase does not have a current transaction, make the label gray
                this.Dispatcher.Invoke(() =>
                {
                    TransactionStatusLabel.Background = new SolidColorBrush(Colors.Transparent);
                    TransactionStatusLabel.Foreground = new SolidColorBrush(Colors.LightGray);
                    TransactionStatusLabel.Text = "Not in transaction";
                });
            }
        }

        private string GetGdbPath()
        {
            // Return the WPF-specific path for storing the geodatabase
            return Environment.ExpandEnvironmentVariables("%TEMP%\\savethebay.geodatabase");
        }

        // handle the click event for the begin transaction button
        private void BeginTransactionClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // clear the message text
                MessageTextBlock.Text = "";

                // if the box for checking for an existing transaction is checked, see if there's a current transaction
                if (CheckTransactionCheckBox.IsChecked == true)
                {
                    // see if there is a transaction active for the geodatabase
                    if (!_localGeodatabase.IsInTransaction)
                    {
                        // if not, begin a transaction
                        _localGeodatabase.BeginTransaction();
                    }
                }
                else
                {
                    // don't check for existing transaction (if there is one, an exception will be thrown and shown in the error text block)
                    _localGeodatabase.BeginTransaction();
                }
            }
            catch (Exception ex)
            {
                MessageTextBlock.Text = ex.Message;
            }
        }

        private void CommitTransactionClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // clear the message text in the UI
                MessageTextBlock.Text = "";

                // if the box for checking for an existing transaction is checked, see if there's a current transaction
                if (CheckTransactionCheckBox.IsChecked == true)
                {
                    // see if there is a transaction active for the geodatabase
                    if (_localGeodatabase.IsInTransaction)
                    {
                        // if there is, commit the transaction to store the edits (this will also end the transaction)
                        _localGeodatabase.CommitTransaction();
                    }
                }
                else
                {
                    // don't check for existing transaction (if there is one, an exception will be thrown and shown in the error text block)
                    _localGeodatabase.CommitTransaction();
                }
            }
            catch (Exception ex)
            {
                MessageTextBlock.Text = ex.Message;
            }
        }

        private void RollbackTransactionClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // clear the message text
                MessageTextBlock.Text = "";

                // if the box for checking for an existing transaction is checked, see if there's a current transaction
                if (CheckTransactionCheckBox.IsChecked == true)
                {
                    // see if there is a transaction active for the geodatabase
                    if (_localGeodatabase.IsInTransaction)
                    {
                        // if there is, rollback the transaction to discard the edits (this will also end the transaction)
                        _localGeodatabase.RollbackTransaction();
                    }
                }
                else
                {
                    // don't check for existing transaction (if there is one, an exception will be thrown and shown in the error text block)
                    _localGeodatabase.RollbackTransaction();
                }
            }
            catch (Exception ex)
            {
                MessageTextBlock.Text = ex.Message;
            }
        }

        // handle the button to load local geodatabase tables as layers in the map
        private void LoadLocalData(object sender, RoutedEventArgs e)
        {
            // call a function to load local geodatabase tables
            LoadLocalGeodatabaseTables();

            // disable the button for loading local data
            LoadLocalDataButton.IsEnabled = false;
        }

        // handle the button for adding new point features 
        private async void AddNewFeature(object sender, RoutedEventArgs args)
        {
            try
            {
                // cancel execution of the sketch task if it is already active
                if (MyMapView.SketchEditor.CancelCommand.CanExecute(null))
                {
                    MyMapView.SketchEditor.CancelCommand.Execute(null);
                }

                // get the selected table to edit
                GeodatabaseFeatureTable table = TableComboBox.SelectedItem as GeodatabaseFeatureTable;
                if (table == null)
                {
                    // if no table is selected, warn the user to select one
                    MessageBox.Show("Select a feature table to edit.", "New Feature");
                    return;
                }

                // create a random value for the 'type' attribute (integer between 1 and 7)
                Random random = new Random(DateTime.Now.Millisecond);
                int featureType = random.Next(1, 7);

                // use the sketch editor to allow the user to draw a point on the map
                MapPoint clickPoint = await MyMapView.SketchEditor.StartAsync(Esri.ArcGISRuntime.UI.SketchCreationMode.Point, false) as MapPoint;

                // create a new feature (row) in the selected table
                Feature newFeature = table.CreateFeature();

                // set the geometry with the point the user clicked and the 'type' with the random integer
                newFeature.Geometry = clickPoint;
                newFeature.SetAttributeValue("type", featureType);

                // add the new feature to the table
                await table.AddFeatureAsync(newFeature);
            }
            catch (TaskCanceledException)
            {
                // ignore if the edit was canceled
            }
            catch (Exception ex)
            {
                // report other exception messages
                MessageTextBlock.Text = ex.Message;
            }
        }
    }
}