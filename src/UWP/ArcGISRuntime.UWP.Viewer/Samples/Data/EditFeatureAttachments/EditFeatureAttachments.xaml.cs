// Copyright 2019 Esri.
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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.UWP.Samples.EditFeatureAttachments
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
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
            // Create the map with streets basemap.
            MyMapView.Map = new Map(Basemap.CreateStreets());

            // Create the feature table, referring to the Damage Assessment feature service.
            ServiceFeatureTable damageTable = new ServiceFeatureTable(new Uri(FeatureServiceUrl));

            // Create a feature layer to visualize the features in the table.
            _damageLayer = new FeatureLayer(damageTable);

            // Add the layer to the map.
            MyMapView.Map.OperationalLayers.Add(_damageLayer);

            // Listen for user taps on the map.
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

                // Get the selected feature as an ArcGISFeature. It is assumed that all GeoElements in the result are of type ArcGISFeature.
                GeoElement tappedElement = identifyResult.GeoElements.First();
                ArcGISFeature tappedFeature = (ArcGISFeature) tappedElement;

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
                await new MessageDialog(ex.ToString(), "Error loading feature").ShowAsync();
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
            string contentType = "image/jpeg";
            byte[] attachmentData;

            try
            {
                // Show a file picker.
                // Allow the user to specify a file path - create the picker.
                FileOpenPicker openPicker = new FileOpenPicker();
                openPicker.FileTypeFilter.Add(".jpg");

                // Show the picker.
                StorageFile file = await openPicker.PickSingleFileAsync();

                // Take action if the user selected a file.
                if (file == null)
                {
                    return;
                }

                // Read the file contents into memory.
                Stream dataStream = await file.OpenStreamForReadAsync();
                attachmentData = new byte[dataStream.Length];
                dataStream.Read(attachmentData, 0, attachmentData.Length);
                dataStream.Close();

                // Add the attachment.
                // The contentType string is the MIME type for JPEG files, image/jpeg.
                await _selectedFeature.AddAttachmentAsync(file.Name, contentType, attachmentData);

                // Get a reference to the feature's service feature table.
                ServiceFeatureTable serviceTable = (ServiceFeatureTable) _selectedFeature.FeatureTable;

                // Apply the edits to the service feature table.
                await serviceTable.ApplyEditsAsync();

                // Update UI.
                _selectedFeature.Refresh();
                AttachmentsListBox.ItemsSource = await _selectedFeature.GetAttachmentsAsync();

                await new MessageDialog("Successfully added attachment", "Success!").ShowAsync();
            }
            catch (Exception exception)
            {
                await new MessageDialog(exception.ToString(), "Error adding attachment").ShowAsync();
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
                Button sendingButton = (Button) sender;

                // Get the attachment from the button's DataContext. The button's DataContext is set by the list box.
                Attachment selectedAttachment = (Attachment) sendingButton.DataContext;

                // Delete the attachment.
                await _selectedFeature.DeleteAttachmentAsync(selectedAttachment);

                // Get a reference to the feature's service feature table.
                ServiceFeatureTable serviceTable = (ServiceFeatureTable) _selectedFeature.FeatureTable;

                // Apply the edits to the service feature table.
                await serviceTable.ApplyEditsAsync();

                // Update UI.
                _selectedFeature.Refresh();
                AttachmentsListBox.ItemsSource = await _selectedFeature.GetAttachmentsAsync();

                // Show success message.
                await new MessageDialog("Successfully deleted attachment", "Success!").ShowAsync();
            }
            catch (Exception exception)
            {
                await new MessageDialog(exception.ToString(), "Error deleting attachment").ShowAsync();
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

                // Show a file dialog.
                // Allow the user to specify a file path - create the dialog.
                var savePicker = new FileSavePicker();
                savePicker.SuggestedStartLocation = PickerLocationId.Downloads;
                savePicker.FileTypeChoices.Add(selectedAttachment.ContentType, new List<string> {".jpeg", ".jpg"});
                savePicker.SuggestedFileName = selectedAttachment.Name;

                // Show the dialog and get a file to write to.
                StorageFile file = await savePicker.PickSaveFileAsync();

                // Skip if user canceled.
                if (file == null)
                {
                    return;
                }

                // Load the data into a byte array.
                Stream attachmentDataStream = await selectedAttachment.GetDataAsync();
                byte[] attachmentData = new byte[attachmentDataStream.Length];
                attachmentDataStream.Read(attachmentData, 0, attachmentData.Length);

                // Write out the file.
                await FileIO.WriteBufferAsync(file, attachmentData.AsBuffer());

                // Close the stream.
                attachmentDataStream.Close();

                // Launch the file.
                await Windows.System.Launcher.LaunchFileAsync(file);
            }
            catch (Exception exception)
            {
                await new MessageDialog(exception.ToString(), "Error reading attachment").ShowAsync();
            }
        }
    }
}