﻿// Copyright 2019 Esri.
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
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.WPF.Samples.EditFeatureAttachments
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Edit feature attachments",
        "Data",
        "Add, delete, and download attachments for features from a service.",
        "")]
    public partial class EditFeatureAttachments
    {
        // URL to the feature service.
        private const string FeatureServiceUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/DamageAssessment/FeatureServer/0";

        // Hold a reference to the feature layer.
        private FeatureLayer _damageLayer;

        // Hold a reference to the recently selected feature.
        private ArcGISFeature _selectedFeature;

        public EditFeatureAttachments()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create the map with streets basemap.
            MyMapView.Map = new Map(Basemap.CreateStreets());

            // Create the feature table, referring to the Damage Assessment feature service.
            ServiceFeatureTable damageTable = new ServiceFeatureTable(new Uri(FeatureServiceUrl));

            // Create a feature layer to visualize the features in the table.
            _damageLayer = new FeatureLayer(damageTable);

            // Add the layer to the map.
            MyMapView.Map.OperationalLayers.Add(_damageLayer);

            // Listen for user taps on the map - on tap, a callout will be shown.
            MyMapView.GeoViewTapped += MapView_Tapped;

            // Zoom to the United States.
            MyMapView.SetViewpointCenterAsync(new MapPoint(-10800000, 4500000, SpatialReferences.WebMercator), 3e7);
        }

        private async void MapView_Tapped(object sender, GeoViewInputEventArgs e)
        {
            // Clear any existing selection.
            _damageLayer.ClearSelection();
            _selectedFeature = null;

            // Reset the UI.
            AttachmentsListBox.IsEnabled = false;
            AttachmentsListBox.ItemsSource = null;
            AddAttachmentButton.IsEnabled = false;

            try
            {
                // Perform an identify to determine if a user tapped on a feature.
                IdentifyLayerResult identifyResult = await MyMapView.IdentifyLayerAsync(_damageLayer, e.Position, 2, false);

                // Do nothing if there are no results.
                if (!identifyResult.GeoElements.Any())
                {
                    return;
                }

                // Otherwise, get the ID of the first result.
                long featureId = (long) identifyResult.GeoElements.First().Attributes["objectid"];

                // Get the feature by constructing a query and running it.
                QueryParameters qp = new QueryParameters();
                qp.ObjectIds.Add(featureId);
                FeatureQueryResult queryResult = await _damageLayer.FeatureTable.QueryFeaturesAsync(qp);
                ArcGISFeature tappedFeature = (ArcGISFeature)queryResult.First();

                // Select the feature.
                _damageLayer.SelectFeature(tappedFeature);
                _selectedFeature = tappedFeature;

                // Load the feature.
                await tappedFeature.LoadAsync();

                // Get the attachments.
                IReadOnlyList<Attachment> attachments = await tappedFeature.GetAttachmentsAsync();

                // Populate the UI.
                AttachmentsListBox.ItemsSource = attachments;
                AttachmentsListBox.IsEnabled = true;
                AddAttachmentButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "There was a problem.");
            }
        }

        private async void AddAttachment_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFeature == null)
            {
                return;
            }

            // Adjust the UI.
            AddAttachmentButton.IsEnabled = false;
            ActivityIndicator.Visibility = Visibility.Visible;

            // Get the file.
            string filename;
            string contentType = "image/jpg";
            byte[] attachmentData;

            try
            {
                // Show a file dialog.
                // Allow the user to specify a file path - create the dialog.
                OpenFileDialog dlg = new OpenFileDialog
                {
                    DefaultExt = ".jpg",
                    Filter = "Image Files(*.JPG;*.JPEG)|*.JPG;*.JPEG",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
                };

                // Show the dialog and get the results.
                bool? result = dlg.ShowDialog();

                // Take action if the user selected a file.
                if (result != true)
                {
                    return;
                }

                // Get the name of the file from the full path (dlg.FileName is the full path).
                filename = Path.GetFileName(dlg.FileName);

                // Create a stream for reading the file.
                FileStream fs = new FileStream(dlg.FileName,
                    FileMode.Open,
                    FileAccess.Read);

                // Create a binary reader from the stream.
                BinaryReader br = new BinaryReader(fs);

                // Populate the attachment data with the binary content.
                long numBytes = new FileInfo(dlg.FileName).Length;
                attachmentData = br.ReadBytes((int) numBytes);

                // Add the attachment.
                await _selectedFeature.AddAttachmentAsync(filename, contentType, attachmentData);

                // Update the table.
                await ((ServiceFeatureTable) _selectedFeature.FeatureTable).ApplyEditsAsync();

                // Update UI.
                _selectedFeature.Refresh();
                AttachmentsListBox.ItemsSource = await _selectedFeature.GetAttachmentsAsync();

                MessageBox.Show("Successfully added attachment", "Success!");
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), "Error adding attachment");
            }
            finally
            {
                // Adjust the UI.
                AddAttachmentButton.IsEnabled = true;
                ActivityIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private async void DeleteAttachment_Click(object sender, RoutedEventArgs e)
        {
            ActivityIndicator.Visibility = Visibility.Visible;

            try
            {
                // Get the attachment that should be deleted.
                Button sendingButton = (Button) sender;
                Attachment selectedAttachment = (Attachment) sendingButton.DataContext;

                // Delete the attachment.
                await _selectedFeature.DeleteAttachmentAsync(selectedAttachment);

                // Update the table.
                await ((ServiceFeatureTable) _selectedFeature.FeatureTable).ApplyEditsAsync();

                // Update UI.
                _selectedFeature.Refresh();
                AttachmentsListBox.ItemsSource = await _selectedFeature.GetAttachmentsAsync();

                // Show success message.
                MessageBox.Show("Successfully delete attachment", "Success!");
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), "Error deleting attachment");
            }
            finally
            {
                ActivityIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private async void DownloadAttachment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get the attachment that should be downloaded.
                Button sendingButton = (Button) sender;
                Attachment selectedAttachment = (Attachment) sendingButton.DataContext;

                // Load the data into a byte array.
                Stream attachmentDataStream = await selectedAttachment.GetDataAsync();
                byte[] attachmentData = new byte[attachmentDataStream.Length];
                attachmentDataStream.Read(attachmentData, 0, attachmentData.Length);

                // Show a file dialog.
                // Allow the user to specify a file path - create the dialog.
                SaveFileDialog dlg = new SaveFileDialog
                {
                    FileName = selectedAttachment.Name,
                    Filter = selectedAttachment.ContentType + "|*.*",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal)
                };

                // Show the dialog and get the results.
                bool? result = dlg.ShowDialog();

                // Take action if the user selected a file.
                if (result != true)
                {
                    return;
                }

                // Write out the file.
                FileStream fs = new FileStream(dlg.FileName,
                    FileMode.OpenOrCreate,
                    FileAccess.Write);
                fs.Write(attachmentData, 0, attachmentData.Length);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), "Error reading attachment");
            }
        }
    }
}
