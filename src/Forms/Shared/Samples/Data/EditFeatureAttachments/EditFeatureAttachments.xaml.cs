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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
// Custom code is needed for presenting the image picker on iOS.
#if __IOS__
using Foundation;
using UIKit;
#else
using Xamarin.Essentials;
using Map = Esri.ArcGISRuntime.Mapping.Map; // avoid ambiguity with Xamarin.Essentials.Map
#endif

namespace ArcGISRuntimeXamarin.Samples.EditFeatureAttachments
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
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
            Initialize();
        }

        private void Initialize()
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
            MyMapView.SetViewpointCenterAsync(new MapPoint(-10800000, 4500000, SpatialReferences.WebMercator), 3e7);
        }

        private async void MapView_Tapped(object sender, Esri.ArcGISRuntime.Xamarin.Forms.GeoViewInputEventArgs e)
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
                await Application.Current.MainPage.DisplayAlert("Error selecting feature", ex.ToString(), "OK");
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
            AttachmentActivityIndicator.IsVisible = true;

            // Get the file.
            string contentType = "image/jpeg";

            try
            {
                byte[] attachmentData;
                string filename;

                // Xamarin.Plugin.FilePicker shows the iCloud picker (not photo picker) on iOS.
                // This iOS code shows the photo picker.
#if __IOS__
                Stream imageStream = await GetImageStreamAsync();
                if (imageStream == null)
                {
                    return;
                }

                attachmentData = new byte[imageStream.Length];
                imageStream.Read(attachmentData, 0, attachmentData.Length);
                filename = _filename ?? "file1.jpeg";
#else
                // Show a file picker - this uses the Xamarin.Plugin.FilePicker NuGet package.
                FileResult fileData = await FilePicker.PickAsync(new PickOptions { FileTypes = FilePickerFileType.Jpeg });
                if (fileData == null)
                {
                    return;
                }

                if (!fileData.FileName.EndsWith(".jpg") && !fileData.FileName.EndsWith(".jpeg"))
                {
                    await Application.Current.MainPage.DisplayAlert("Try again!", "This sample only allows uploading jpg files.", "OK");
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
#endif
                // Add the attachment.
                // The contentType string is the MIME type for JPEG files, image/jpeg.
                await _selectedFeature.AddAttachmentAsync(filename, contentType, attachmentData);

                // Get a reference to the feature's service feature table.
                ServiceFeatureTable serviceTable = (ServiceFeatureTable) _selectedFeature.FeatureTable;

                // Apply the edits to the service feature table.
                await serviceTable.ApplyEditsAsync();

                // Update UI.
                _selectedFeature.Refresh();
                AttachmentsListBox.ItemsSource = await _selectedFeature.GetAttachmentsAsync();

                await Application.Current.MainPage.DisplayAlert("Success!", "Successfully added attachment", "OK");
            }
            catch (Exception exception)
            {
                await Application.Current.MainPage.DisplayAlert("Error adding attachment", exception.ToString(), "OK");
            }
            finally
            {
                // Adjust the UI.
                AddAttachmentButton.IsEnabled = true;
                AttachmentActivityIndicator.IsVisible = false;
            }
        }

        private async void DeleteAttachment_Click(object sender, EventArgs e)
        {
            AttachmentActivityIndicator.IsVisible = true;

            try
            {
                // Get the attachment that should be deleted.
                Button sendingButton = (Button) sender;
                Attachment selectedAttachment = (Attachment) sendingButton.BindingContext;

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
                await Application.Current.MainPage.DisplayAlert("Success!", "Successfully deleted attachment", "OK");
            }
            catch (Exception exception)
            {
                await Application.Current.MainPage.DisplayAlert("Error deleting attachment", exception.ToString(), "OK");
            }
            finally
            {
                AttachmentActivityIndicator.IsVisible = false;
            }
        }

        private async void DownloadAttachment_Click(object sender, EventArgs e)
        {
            try
            {
                // Get a reference to the button that raised the event.
                Button sendingButton = (Button) sender;

                // Get the attachment from the button's DataContext. The button's DataContext is set by the list view.
                Attachment selectedAttachment = (Attachment) sendingButton.BindingContext;

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
                    await Application.Current.MainPage.DisplayAlert("Can't show attachment", "This sample can only show image attachments.", "OK");
                }
            }
            catch (Exception exception)
            {
                await Application.Current.MainPage.DisplayAlert("Error reading attachment", exception.ToString(), "OK");
            }
        }

        // Image picker implementation.
        // Xamarin.Plugin.FilePicker shows an iCloud file picker; comment this out
        // and use the cross-platform implementation if that's what you want.
        // Note: code adapted from https://docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/dependency-service/photo-picker
#if __IOS__
        private TaskCompletionSource<Stream> _taskCompletionSource;
        private UIImagePickerController _imagePicker;
        private string _filename;

        private Task<Stream> GetImageStreamAsync()
        {
            // Create and define UIImagePickerController.
            _imagePicker = new UIImagePickerController
            {
                SourceType = UIImagePickerControllerSourceType.PhotoLibrary,
                MediaTypes = UIImagePickerController.AvailableMediaTypes(UIImagePickerControllerSourceType.PhotoLibrary)
            };

            // Set event handlers.
            _imagePicker.FinishedPickingMedia += OnImagePickerFinishedPickingMedia;
            _imagePicker.Canceled += OnImagePickerCancelled;

            // Present UIImagePickerController.
            UIWindow window = UIApplication.SharedApplication.KeyWindow;
            var viewController = window.RootViewController;
            viewController.PresentModalViewController(_imagePicker, true);

            // Return Task object.
            _taskCompletionSource = new TaskCompletionSource<Stream>();
            return _taskCompletionSource.Task;
        }

        void OnImagePickerFinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs args)
        {
            UIImage image = args.EditedImage ?? args.OriginalImage;
            _filename = args.ImageUrl.LastPathComponent;
            if (image != null)
            {
                // Convert UIImage to .NET Stream object.
                NSData data = image.AsJPEG(1);
                Stream stream = data.AsStream();

                UnregisterEventHandlers();

                // Set the Stream as the completion of the Task.
                _taskCompletionSource.SetResult(stream);
            }
            else
            {
                UnregisterEventHandlers();
                _taskCompletionSource.SetResult(null);
            }

            _imagePicker.DismissModalViewController(true);
        }

        void OnImagePickerCancelled(object sender, EventArgs args)
        {
            UnregisterEventHandlers();
            _taskCompletionSource.SetResult(null);
            _imagePicker.DismissModalViewController(true);
        }

        void UnregisterEventHandlers()
        {
            _imagePicker.FinishedPickingMedia -= OnImagePickerFinishedPickingMedia;
            _imagePicker.Canceled -= OnImagePickerCancelled;
        }
#endif
    }
}