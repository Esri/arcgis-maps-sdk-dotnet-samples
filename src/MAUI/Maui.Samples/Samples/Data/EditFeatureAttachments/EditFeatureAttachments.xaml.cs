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

// Custom code is needed for presenting the image picker on iOS.
#if IOS || MACCATALYST
using Foundation;
using UIKit;
#endif

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
                await Application.Current.MainPage.DisplayAlert("Error", ex.ToString(), "OK");
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

                // Microsoft.Maui.Storage.FilePicker shows the iCloud picker (not photo picker) on iOS.
                
#if IOS || MACCATALYST
                attachmentData = await GetImageBytesAsync();

                // Return if no image was loaded.
                if (attachmentData == null) return;

                if (!_filename.EndsWith(".jpg") && !_filename.EndsWith(".jpeg"))
                {
                    await Application.Current.MainPage.DisplayAlert("Try again!", "This sample only allows uploading jpg files.", "OK");
                    return;
                }
                filename = _filename;
#else
                // Show a file picker
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
                ServiceFeatureTable serviceTable = (ServiceFeatureTable)_selectedFeature.FeatureTable;

                // Apply the edits to the service feature table.
                await serviceTable.ApplyEditsAsync();

                // Update UI.
                _selectedFeature.Refresh();
                AttachmentsListBox.ItemsSource = await GetJpegAttachmentsAsync(_selectedFeature);

                await Application.Current.MainPage.DisplayAlert("Success!", "Successfully added attachment", "OK");
            }
            catch (Exception exception)
            {
                await Application.Current.MainPage.DisplayAlert("Error adding attachment", exception.Message, "OK");
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
                    await Application.Current.MainPage.Navigation.PushAsync(previewPage);
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

        private static async Task<IEnumerable<Attachment>> GetJpegAttachmentsAsync(ArcGISFeature feature)
        {
            IReadOnlyList<Attachment> attachments = await feature.GetAttachmentsAsync();
            return attachments.Where(attachment => attachment.ContentType == "image/jpeg").ToList();
        }

        // Image picker implementation for iOS using UIImagePickerController.
        // Microsoft.Maui.Storage.FilePicker shows an iCloud file picker; comment this out
        // and use the cross-platform implementation if that's what you want.
#if IOS
        private TaskCompletionSource<byte[]> _taskCompletionSource;
        private UIImagePickerController _imagePicker;
        private string _filename;

        private Task<byte[]> GetImageBytesAsync()
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
            viewController.PresentViewController(_imagePicker, true, null);

            // Return Task object.
            _taskCompletionSource = new TaskCompletionSource<byte[]>();
            return _taskCompletionSource.Task;
        }

        private void OnImagePickerFinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs args)
        {
            UIImage image = args.EditedImage ?? args.OriginalImage;
            _filename = args.ImageUrl.LastPathComponent;
            if (image != null)
            {
                // Get the image data.
                NSData data = image.AsJPEG(1);

                // Set the byte array as the completion of the Task.
                _taskCompletionSource.SetResult(data.ToArray());
            }
            else
            {
                _taskCompletionSource.SetResult(null);
            }

            UnregisterEventHandlers();
        }

        private void OnImagePickerCancelled(object sender, EventArgs args)
        {
            UnregisterEventHandlers();
            _taskCompletionSource.SetResult(null);
        }

        void UnregisterEventHandlers()
        {
            _imagePicker.FinishedPickingMedia -= OnImagePickerFinishedPickingMedia;
            _imagePicker.Canceled -= OnImagePickerCancelled;
            _imagePicker.DismissViewController(true, null);
        }
#endif

        // Image picker implementation for Mac using UIDocumentPickerViewController.
#if MACCATALYST
        private TaskCompletionSource<byte[]> _taskCompletionSource;
        private UIDocumentPickerViewController _imagePicker;
        private string _filename;

        public async Task<byte[]> GetImageBytesAsync()
        {
            var allowedTypes = new UniformTypeIdentifiers.UTType[]
            {
              UniformTypeIdentifiers.UTTypes.Image,
            };

            _imagePicker = new UIDocumentPickerViewController(allowedTypes) { AllowsMultipleSelection = false };
            _taskCompletionSource = new TaskCompletionSource<byte[]>();

            _imagePicker.DidPickDocument += DocumentPicked;
            _imagePicker.DidPickDocumentAtUrls += DocumentAtUrlsPicked;
            _imagePicker.WasCancelled += DocumentCancelled;

            // Present the UIDocumentPickerViewController.
            UIWindow window = UIApplication.SharedApplication.KeyWindow;
            var viewController = window.RootViewController;
            viewController.PresentViewController(_imagePicker, true, null);

            return await _taskCompletionSource.Task;
        }

        private void DocumentPicked(object sender, UIDocumentPickedEventArgs e)
        {
            PickDocumentAsync(e.Url);
        }
        private void DocumentAtUrlsPicked(object sender, UIDocumentPickedAtUrlsEventArgs e)
        {
            PickDocumentAsync(e.Urls[0]);
        }

        private void DocumentCancelled(object sender, EventArgs e)
        {
            // A null result will cancel without presenting an error message.
            _taskCompletionSource.TrySetResult(null);
            UnregisterEventHandlers();
        }

        private void PickDocumentAsync(NSUrl url)
        {
            try
            {
                // Set the filename for later use.
                _filename = url.LastPathComponent;

                // Load the data from the local file.
                NSData data = NSData.FromUrl(url);

                // Set the byte array as the completion of the Task.
                _taskCompletionSource.TrySetResult(data.ToArray());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
                _taskCompletionSource.TrySetException(ex);
            }
            finally
            {
                UnregisterEventHandlers();
            }
        }
        void UnregisterEventHandlers()
        {
            _imagePicker.DidPickDocument -= DocumentPicked;
            _imagePicker.DidPickDocumentAtUrls -= DocumentAtUrlsPicked;
            _imagePicker.WasCancelled -= DocumentCancelled;
            _imagePicker.DismissViewController(true, null);
        }
#endif
    }
}