// Copyright 2021 Esri.
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ArcGIS.WPF.Samples.EditFeatureAttachments
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Edit feature attachments",
        category: "Data",
        description: "Add, delete, and download attachments for features from a service.",
        instructions: "Tap a feature to load its attachments. Use the buttons to save, delete, or add attachments.",
        tags: new[] { "JPEG", "PDF", "PNG", "TXT", "data", "image", "picture" })]
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
            try
            {
                // Create the map with streets basemap.
                MyMapView.Map = new Map(BasemapStyle.ArcGISStreets);

                // Create the feature table, referring to the Damage Assessment feature service.
                ServiceFeatureTable damageTable = new ServiceFeatureTable(new Uri(FeatureServiceUrl));

                // Create a feature layer to visualize the features in the table.
                _damageLayer = new FeatureLayer(damageTable);

                // Add the layer to the map.
                MyMapView.Map.OperationalLayers.Add(_damageLayer);

                // Listen for user taps on the map.
                MyMapView.GeoViewTapped += MapView_Tapped;

                // Zoom to the United States.
                _ = MyMapView.SetViewpointCenterAsync(new MapPoint(-10800000, 4500000, SpatialReferences.WebMercator), 3e7);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

                // Get the selected feature as an ArcGISFeature. It is assumed that all GeoElements in the result are of type ArcGISFeature.
                GeoElement tappedElement = identifyResult.GeoElements.First();
                ArcGISFeature tappedFeature = (ArcGISFeature)tappedElement;

                // Select the feature in the UI and hold a reference to the tapped feature in a field.
                _damageLayer.SelectFeature(tappedFeature);
                _selectedFeature = tappedFeature;

                // Load the feature.
                await tappedFeature.LoadAsync();

                // Get the attachments.
                IReadOnlyList<Attachment> attachments = await tappedFeature.GetAttachmentsAsync();

                // Populate the UI with a list of attachments that have a content type of image/jpeg.
                AttachmentsListBox.ItemsSource = attachments.Where(attachment => attachment.ContentType == "image/jpeg");
                AttachmentsListBox.IsEnabled = true;
                AddAttachmentButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error loading feature");
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
                string filename = Path.GetFileName(dlg.FileName);

                // Create a stream for reading the file.
                FileStream fs = new FileStream(dlg.FileName,
                    FileMode.Open,
                    FileAccess.Read);

                // Create a binary reader from the stream.
                BinaryReader br = new BinaryReader(fs);

                // Populate the attachment data with the binary content.
                long numBytes = new FileInfo(dlg.FileName).Length;
                byte[] attachmentData = br.ReadBytes((int)numBytes);

                // Close the stream.
                fs.Close();

                // Add the attachment.
                // The contentType string is the MIME type for JPEG files, image/jpeg.
                await _selectedFeature.AddAttachmentAsync(filename, "image/jpeg", attachmentData);

                // Get a reference to the feature's service feature table.
                ServiceFeatureTable serviceTable = (ServiceFeatureTable)_selectedFeature.FeatureTable;

                // Apply the edits to the service feature table.
                await serviceTable.ApplyEditsAsync();

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
                // Get a reference to the button that raised the event.
                Button sendingButton = (Button)sender;

                // Get the attachment from the button's DataContext. The button's DataContext is set by the list view.
                Attachment selectedAttachment = (Attachment)sendingButton.DataContext;

                // Delete the attachment.
                await _selectedFeature.DeleteAttachmentAsync(selectedAttachment);

                // Get a reference to the feature's service feature table.
                ServiceFeatureTable serviceTable = (ServiceFeatureTable)_selectedFeature.FeatureTable;

                // Apply the edits to the service feature table.
                await serviceTable.ApplyEditsAsync();

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
                Button sendingButton = (Button)sender;
                Attachment selectedAttachment = (Attachment)sendingButton.DataContext;

                // Show a file dialog.
                // Allow the user to specify a file path - create the dialog.
                SaveFileDialog dlg = new SaveFileDialog
                {
                    FileName = selectedAttachment.Name,
                    Filter = selectedAttachment.ContentType + "|*.jpg",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal)
                };

                // Show the dialog and get the results.
                bool? result = dlg.ShowDialog();

                // Take action if the user selected a file.
                if (result != true)
                {
                    return;
                }

                // Load the data into a byte array.
                Stream attachmentDataStream = await selectedAttachment.GetDataAsync();
                byte[] attachmentData = new byte[attachmentDataStream.Length];
                attachmentDataStream.Read(attachmentData, 0, attachmentData.Length);

                // Write out the file.
                FileStream fs = new FileStream(dlg.FileName,
                    FileMode.OpenOrCreate,
                    FileAccess.Write);
                fs.Write(attachmentData, 0, attachmentData.Length);
                fs.Close();

                // Launch the file.
                Process.Start(new ProcessStartInfo(dlg.FileName) { UseShellExecute = true });
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), "Error reading attachment");
            }
        }
    }
}