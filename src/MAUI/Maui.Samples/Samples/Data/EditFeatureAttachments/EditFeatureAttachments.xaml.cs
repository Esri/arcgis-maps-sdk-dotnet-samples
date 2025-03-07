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
using System.Diagnostics;

namespace ArcGIS.Samples.EditFeatureAttachments
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Edit feature attachments",
        category: "Data",
        description: "Add, delete, and download attachments for features from a service.",
        instructions: "Tap a feature to load its attachments. Use the buttons to save, delete, or add attachments.",
        tags: new[] { "JPEG", "PDF", "PNG", "TXT", "data", "image", "picture" })]
    public partial class EditFeatureAttachments : ContentPage
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
            _ = Initialize();
        }

        private async Task Initialize()
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
                await DisplayAlert("Error", ex.ToString(), "OK");
            }
        }

        private async void MapView_Tapped(object sender, Esri.ArcGISRuntime.Maui.GeoViewInputEventArgs e)
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

                // Populate the UI with a list of attachments that have a content type of image/jpeg.
                AttachmentsListBox.ItemsSource = await GetJpegAttachmentsAsync(tappedFeature);
                AttachmentsListBox.IsEnabled = true;
                AddAttachmentButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error selecting feature", ex.ToString(), "OK");
            }
        }

        private async void AddAttachment_Click(object sender, EventArgs e)
        {
            if (_selectedFeature == null)
            {
                return;
            }

            // Adjust the UI.
            AddAttachmentButton.IsEnabled = false;

            // Get the file.
            string contentType = "image/jpeg";

            try
            {
                byte[] attachmentData;
                string filename;

                // Show a photo picker.
                FileResult fileData = await MediaPicker.PickPhotoAsync(new MediaPickerOptions { Title = "Please select a jpeg photo." });

                if (fileData == null)
                {
                    return;
                }

                if (!fileData.FileName.EndsWith(".jpg") && !fileData.FileName.EndsWith(".jpeg"))
                {
                    Debug.WriteLine("Sample only suports jpeg images.");
                    return;
                }

                using (Stream fileStream = await fileData.OpenReadAsync())
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await fileStream.CopyToAsync(memoryStream);
                        attachmentData = memoryStream.ToArray();
                    }
                }
                filename = fileData.FileName;

                // Add the attachment.
                // The contentType string is the MIME type for JPEG files, image/jpeg.
                await _selectedFeature.AddAttachmentAsync(filename, contentType, attachmentData);

                // Get a reference to the feature's service feature table.
                ServiceFeatureTable serviceTable = (ServiceFeatureTable)_selectedFeature.FeatureTable;

                // Apply the edits to the service feature table.
                await serviceTable.ApplyEditsAsync();

                // Update UI.
                _selectedFeature.Refresh();
                AttachmentsListBox.ItemsSource = await GetJpegAttachmentsAsync(_selectedFeature);

                Debug.WriteLine("Successfully added attachment.");
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Error adding attachment: {exception.Message}");
            }
            finally
            {
                // Adjust the UI.
                AddAttachmentButton.IsEnabled = true;
            }
        }

        private async void DeleteAttachment_Click(object sender, EventArgs e)
        {
            try
            {
                // Get the attachment that should be deleted.
                Button sendingButton = (Button)sender;
                Attachment selectedAttachment = (Attachment)sendingButton.BindingContext;

                // Delete the attachment.
                await _selectedFeature.DeleteAttachmentAsync(selectedAttachment);

                // Get a reference to the feature's service feature table.
                ServiceFeatureTable serviceTable = (ServiceFeatureTable)_selectedFeature.FeatureTable;

                // Apply the edits to the service feature table.
                await serviceTable.ApplyEditsAsync();

                // Update UI.
                _selectedFeature.Refresh();
                AttachmentsListBox.ItemsSource = await GetJpegAttachmentsAsync(_selectedFeature);

                Debug.WriteLine("Successfully deleted attachment.");
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"Error deleting attachment: {exception.Message}");
            }
        }

        private async void DownloadAttachment_Click(object sender, EventArgs e)
        {
            try
            {
                // Get a reference to the button that raised the event.
                Button sendingButton = (Button)sender;

                // Get the attachment from the button's DataContext. The button's DataContext is set by the list view.
                Attachment selectedAttachment = (Attachment)sendingButton.BindingContext;

                if (selectedAttachment.ContentType.Contains("image"))
                {
                    // Create a preview and show it.
                    ContentPage previewPage = new ContentPage();
                    previewPage.Title = "Attachment preview";
                    Image imageView = new Image();
                    Stream contentStream = await selectedAttachment.GetDataAsync();
                    imageView.Source = ImageSource.FromStream(() => contentStream);
                    previewPage.Content = imageView;
                    await Navigation.PushAsync(previewPage);
                }
                else
                {
                    await DisplayAlert("Can't show attachment", "This sample can only show image attachments.", "OK");
                }
            }
            catch (Exception exception)
            {
                await DisplayAlert("Error reading attachment", exception.ToString(), "OK");
            }
        }

        private static async Task<IEnumerable<Attachment>> GetJpegAttachmentsAsync(ArcGISFeature feature)
        {
            IReadOnlyList<Attachment> attachments = await feature.GetAttachmentsAsync();
            return attachments.Where(attachment => attachment.ContentType == "image/jpeg").ToList();
        }
    }
}