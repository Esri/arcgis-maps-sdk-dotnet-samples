// Copyright 2022 Esri.
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
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.CreateMobileGeodatabase
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Create mobile geodatabase",
        category: "Data",
        description: "Create and share a mobile geodatabase.",
        instructions: "Tap on the map to add a feature symbolizing the user's location. Tap \"View table\" to view the contents of the geodatabase feature table. Once you have added the location points to the map, click the \"Close\" button to retrieve the `.geodatabase` file which can then be imported into ArcGIS Pro or opened in an ArcGIS application. Click the \"Create\" button to make another geodatabase.",
        tags: new[] { "arcgis pro", "database", "feature", "feature table", "geodatabase", "mobile geodatabase", "sqlite" })]
    public partial class CreateMobileGeodatabase
    {
        private GeodatabaseFeatureTable _featureTable;
        private Geodatabase _geodatabase;

        private string _gdbPath;
        private string _directoryPath;

        public CreateMobileGeodatabase()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create a map.
            MyMapView.Map = new Map(BasemapStyle.ArcGISTopographic);
            MyMapView.SetViewpoint(new Viewpoint(39.323845, -77.733201, 10000.0));

            await CreateGeodatabase();

            Unloaded += SampleUnloaded;
        }

        private async Task CreateGeodatabase()
        {
            try
            {
                // Create a new randomly named directory for the geodatabase.
                _directoryPath = Path.Combine(Environment.ExpandEnvironmentVariables("%TEMP%"), "CreateMobileGeodatabase", Guid.NewGuid().ToString());
                if (!Directory.Exists(_directoryPath))
                {
                    Directory.CreateDirectory(_directoryPath);
                }

                // Create the geodatabase file.
                _gdbPath = Path.Combine(_directoryPath, "LocationHistory.geodatabase");

                // Delete existing file if present from previous sample run.
                if (File.Exists(_gdbPath))
                {
                    File.Delete(_gdbPath);
                }

                _geodatabase = await Geodatabase.CreateAsync(_gdbPath);

                // Construct a table description which stores features as points on a map.
                var tableDescription = new TableDescription("LocationHistory", SpatialReferences.Wgs84, GeometryType.Point)
                {
                    HasAttachments = false,
                    HasM = false,
                    HasZ = false
                };

                // Set up the fields for the table:
                // FieldType.OID is the primary key of the SQLite table.
                // FieldType.Date is a date column used to store a Calendar date.
                // FieldDescriptions can be a SHORT, INTEGER, GUID, FLOAT, DOUBLE, DATE, TEXT, OID, GLOBALID, BLOB, GEOMETRY, RASTER, or XML.
                tableDescription.FieldDescriptions.Add(new FieldDescription("oid", FieldType.OID));
                tableDescription.FieldDescriptions.Add(new FieldDescription("collection_timestamp", FieldType.Date));

                // Add a new table to the geodatabase by creating one from the table description.
                _featureTable = await _geodatabase.CreateTableAsync(tableDescription);

                // Refresh the UI for the new empty table.
                await UpdateTable();

                // Create a feature layer for the map.
                FeatureLayer featureLayer = new FeatureLayer(_featureTable);
                MyMapView.Map.OperationalLayers.Add(featureLayer);

                // Add an event handler for new feature taps.
                MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            _ = AddFeature(e.Location);
        }

        private async Task AddFeature(MapPoint location)
        {
            // Create an attributes dictionary for the feature.
            var attributes = new Dictionary<string, object>();

            // Add a timestamp of when the feature was created.
            attributes["collection_timestamp"] = DateTime.Now;

            // Create the feature.
            Feature feature = _featureTable.CreateFeature(attributes, location);
            try
            {
                // Add the feature to the feature table.
                await _featureTable.AddFeatureAsync(feature);

                // Update the UI.
                await UpdateTable();
                FeaturesLabel.Content = $"Number of features added: {_featureTable.NumberOfFeatures}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateTable()
        {
            // Query all of the features in the feature table.
            FeatureQueryResult queryFeatureResult = await _featureTable.QueryFeaturesAsync(new QueryParameters());

            // Set the items source for the data grid with the updated query result.
            TableDataGrid.ItemsSource = queryFeatureResult;
        }

        private void ViewTable(object sender, RoutedEventArgs e)
        {
            TableBorder.Visibility = Visibility.Visible;
        }

        private void CloseTable(object sender, RoutedEventArgs e)
        {
            TableBorder.Visibility = Visibility.Hidden;
        }

        private void CloseGeodatabaseClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Clear the UI.
                MyMapView.GeoViewTapped -= MyMapView_GeoViewTapped;
                CloseGdbButton.IsEnabled = false;
                CreateGdbButton.IsEnabled = true;
                MyMapView.Map.OperationalLayers.Clear();
                TableDataGrid.ItemsSource = null;
                FeaturesLabel.Content = $"Number of features added:";

                // Close the geodatabase.
                _geodatabase.Close();

                // Open the file explorer to the directory containing the geodatabase.
                Process.Start("explorer.exe", _directoryPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateGdbButton_Click(object sender, RoutedEventArgs e)
        {
            _ = CreateGeodatabase();
            CloseGdbButton.IsEnabled = true;
            CreateGdbButton.IsEnabled = false;
        }

        private void SampleUnloaded(object sender, RoutedEventArgs e)
        {
            // Close the geodatabase.
            _geodatabase?.Close();
        }
    }
}