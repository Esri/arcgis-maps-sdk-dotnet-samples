// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Views;

namespace ArcGISRuntimeXamarin.Samples.EditFeatureAttachments
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Edit feature attachments",
        "Data",
        "Add, delete, and download attachments for features from a service.",
        "Tap a feature to load its attachments. Use the buttons to save, delete, or add attachments.",
        "JPEG", "PDF", "PNG", "TXT", "data", "image", "picture")]
    public class EditFeatureAttachments : Activity
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private ListView _attachmentsListView;
        private Button _addButton;

        // Hold a reference to the layer.
        private FeatureLayer _damageLayer;

        // Hold references to the currently selected feature & any attachments.
        private ArcGISFeature _selectedFeature;
        private IReadOnlyList<Attachment> _featureAttachments;

        // URL to the feature service.
        private const string FeatureServiceUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/DamageAssessment/FeatureServer/0";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Edit feature attachments";

            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create the map with streets basemap.
            _myMapView.Map = new Map(Basemap.CreateStreets());

            // Create the feature table, referring to the Damage Assessment feature service.
            ServiceFeatureTable damageTable = new ServiceFeatureTable(new Uri(FeatureServiceUrl));

            // Create a feature layer to visualize the features in the table.
            _damageLayer = new FeatureLayer(damageTable);

            // Add the layer to the map.
            _myMapView.Map.OperationalLayers.Add(_damageLayer);

            // Listen for user taps on the map.
            _myMapView.GeoViewTapped += MapView_Tapped;

            // Zoom to the United States.
            _myMapView.SetViewpointCenterAsync(new MapPoint(-10800000, 4500000, SpatialReferences.WebMercator), 3e7);
        }

        private async void MapView_Tapped(object sender, GeoViewInputEventArgs e)
        {
            // Clear any existing selection.
            _damageLayer.ClearSelection();
            _addButton.Enabled = false;
            _attachmentsListView.Enabled = false;

            try
            {
                // Perform an identify to determine if a user tapped on a feature.
                IdentifyLayerResult identifyResult = await _myMapView.IdentifyLayerAsync(_damageLayer, e.Position, 2, false);

                // Do nothing if there are no results.
                if (!identifyResult.GeoElements.Any())
                {
                    return;
                }

                // Get the selected feature as an ArcGISFeature. It is assumed that all GeoElements in the result are of type ArcGISFeature.
                GeoElement tappedElement = identifyResult.GeoElements.First();
                _selectedFeature = (ArcGISFeature) tappedElement;

                // Update the UI.
                UpdateUIForFeature();
                _addButton.Enabled = true;
                _attachmentsListView.Enabled = true;
            }
            catch (Exception ex)
            {
                ShowMessage(ex.ToString(), "Error selecting feature");
            }
        }

        private async void UpdateUIForFeature()
        {
            // Select the feature.
            _damageLayer.SelectFeature(_selectedFeature);

            // Get the attachments.
            _featureAttachments = await _selectedFeature.GetAttachmentsAsync();

            // Limit to only feature attachments with an image/jpeg content type.
            _featureAttachments = _featureAttachments.Where(attachment => attachment.ContentType == "image/jpeg").ToList();

            // Configure array adapter.
            ArrayAdapter attachmentAdapter = new ArrayAdapter<string>(
                this,
                Android.Resource.Layout.SimpleListItem1,
                _featureAttachments.Select(attachment => attachment.Name).ToArray());

            // Populate the list.
            _attachmentsListView.Adapter = attachmentAdapter;
        }

        private async Task PreviewAttachment(Attachment selectedAttachment)
        {
            if (selectedAttachment.ContentType.Contains("image"))
            {
                // Get the image data.
                Stream contentStream = await selectedAttachment.GetDataAsync();
                byte[] attachmentData = new byte[contentStream.Length];
                contentStream.Read(attachmentData, 0, attachmentData.Length);

                // Convert the image into a usable form on Android.
                Bitmap bmp = BitmapFactory.DecodeByteArray (attachmentData, 0, attachmentData.Length);

                // Create the view that will present the image.
                ImageView imageView = new ImageView(this);
                imageView.SetImageBitmap(bmp);

                // Show the image view in a dialog.
                ShowImageDialog(imageView);
            }
            else
            {
                ShowMessage("This sample can only show image attachments", "Can't show attachment");
            }
        }

        private void ShowImageDialog(ImageView previewImageView)
        {
            // Create the dialog.
            Dialog imageDialog = new Dialog(this);

            // Remove the title bar for the dialog.
            imageDialog.Window.RequestFeature(WindowFeatures.NoTitle);

            // Add the image to the dialog.
            imageDialog.SetContentView(previewImageView);

            // Show the dialog.
            imageDialog.Show();
        }

        private async Task DeleteAttachment(Attachment attachmentToDelete)
        {
            try
            {
                // Delete the attachment.
                await _selectedFeature.DeleteAttachmentAsync(attachmentToDelete);

                // Get a reference to the feature's service feature table.
                ServiceFeatureTable serviceTable = (ServiceFeatureTable) _selectedFeature.FeatureTable;

                // Apply the edits to the service feature table.
                await serviceTable.ApplyEditsAsync();

                // Update UI.
                _selectedFeature.Refresh();
                _featureAttachments = await _selectedFeature.GetAttachmentsAsync();
                UpdateUIForFeature();
                ShowMessage("Successfully deleted attachment", "Success!");
            }
            catch (Exception exception)
            {
                ShowMessage(exception.ToString(), "Error deleting attachment");
            }
        }

        private void RequestImage()
        {
            // Start the process of requesting an image. Will be completed in OnActivityResult.
            Intent = new Intent();
            Intent.SetType("image/*.jpg");
            Intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(Intent, "Select Picture"), 1000);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            // Method called when the image picker activity ends.
            if (requestCode == 1000 && resultCode == Result.Ok && data != null)
            {
                // Get the path to the selected image.
                Android.Net.Uri uri = data.Data;

                // Upload the image as an attachment.
                AddAttachment(uri);
            }
            else
            {
                ShowMessage("No image selected.", "Error adding attachment");
            }
        }

        private async void AddAttachment(Android.Net.Uri imageUri)
        {
            string contentType = "image/jpeg";

            // Read the image into a stream.
            Stream stream = ContentResolver.OpenInputStream(imageUri);   
            
            // Read from the stream into the byte array.
            byte[] attachmentData;
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                attachmentData = memoryStream.ToArray();
            }

            // Add the attachment.
            // The contentType string is the MIME type for JPEG files, image/jpeg.
            await _selectedFeature.AddAttachmentAsync(imageUri.LastPathSegment + ".jpg", contentType, attachmentData);

            // Get a reference to the feature's service feature table.
            ServiceFeatureTable serviceTable = (ServiceFeatureTable) _selectedFeature.FeatureTable;

            // Apply the edits to the service feature table.
            await serviceTable.ApplyEditsAsync();

            // Update UI.
            _selectedFeature.Refresh();
            _featureAttachments = await _selectedFeature.GetAttachmentsAsync();
            UpdateUIForFeature();
            ShowMessage("Successfully added attachment", "Success!");
        }

        private void Attachment_Clicked(object sender, AdapterView.ItemClickEventArgs e)
        {
            // Get the selected attachment.
            Attachment selectedAttachment = _featureAttachments[e.Position];

            // Create menu to show options.
            PopupMenu menu = new PopupMenu(this, (ListView) sender);

            // Handle the click, calling the right method depending on the command.
            menu.MenuItemClick += async (o, menuArgs) =>
            {
                menu.Dismiss();
                switch (menuArgs.Item.ToString())
                {
                    case "View":
                        await PreviewAttachment(selectedAttachment);
                        break;
                    case "Delete":
                        await DeleteAttachment(selectedAttachment);
                        break;
                }
                UpdateUIForFeature();
            };

            // Add the menu commands.
            menu.Menu.Add("View");
            menu.Menu.Add("Delete");

            // Show menu in the view.
            menu.Show();
        }

        private void ShowMessage(string message, string title)
        {
            // Display the message to the user.
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetMessage(message).SetTitle(title).Show();
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            var layout = new LinearLayout(this) {Orientation = Orientation.Vertical};

            // Create the MapView.
            _myMapView = new MapView(this);

            // Create the help label.
            TextView helpLabel = new TextView(this);
            helpLabel.Text = "Tap to select features.";
            helpLabel.TextAlignment = TextAlignment.Center;
            helpLabel.Gravity = GravityFlags.Center;

            // Add the help label to the layout.
            layout.AddView(helpLabel);

            // Create and add a listview for showing attachments;
            _attachmentsListView = new ListView(this);
            _attachmentsListView.Enabled = false;
            _attachmentsListView.SetMinimumHeight(100);
            layout.AddView(_attachmentsListView);
            _attachmentsListView.ItemClick += Attachment_Clicked;

            // Create and add an 'add attachment' button.
            _addButton = new Button(this);
            _addButton.Text = "Add attachment";
            _addButton.Enabled = false;
            _addButton.Click += AddButton_Clicked;
            layout.AddView(_addButton);

            // Add the map view to the layout.
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }

        private void AddButton_Clicked(object sender, EventArgs e)
        {
            // Do nothing if nothing selected.
            if (_selectedFeature == null)
            {
                return;
            }

            // Start the process of requesting an image to add.
            RequestImage();
        }
    }
}
