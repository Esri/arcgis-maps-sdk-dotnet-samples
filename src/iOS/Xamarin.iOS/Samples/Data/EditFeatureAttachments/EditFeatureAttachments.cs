// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.EditFeatureAttachments
{
    [Register("EditFeatureAttachments")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Edit feature attachments",
        "Data",
        "Add, delete, and download attachments for features from a service.",
        "")]
    public class EditFeatureAttachments : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;

        // Hold a reference to the layer.
        private FeatureLayer _damageLayer;

        // URL to the feature service.
        private const string FeatureServiceUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/DamageAssessment/FeatureServer/0";

        public EditFeatureAttachments()
        {
            Title = "Edit feature attachments";
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

            // Listen for user taps on the map - on tap, a callout will be shown.
            _myMapView.GeoViewTapped += MapView_Tapped;

            // Zoom to the United States.
            _myMapView.SetViewpointCenterAsync(new MapPoint(-10800000, 4500000, SpatialReferences.WebMercator), 3e7);
        }

        private async void MapView_Tapped(object sender, GeoViewInputEventArgs e)
        {
            // Clear any existing selection.
            _damageLayer.ClearSelection();

            try
            {
                // Perform an identify to determine if a user tapped on a feature.
                IdentifyLayerResult identifyResult = await _myMapView.IdentifyLayerAsync(_damageLayer, e.Position, 2, false);

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
                ArcGISFeature tappedFeature = (ArcGISFeature) queryResult.First();

                // Select the feature.
                _damageLayer.SelectFeature(tappedFeature);

                // Create the view controller.
                AttachmentsTableView attachmentsTableViewController = new AttachmentsTableView(tappedFeature);

                // Present the view controller.
                NavigationController.PushViewController(attachmentsTableViewController, true);

                // Deselect the feature.
                _damageLayer.ClearSelection();
            }
            catch (Exception ex)
            {
                ShowMessage(ex.ToString(), "Error selecting feature");
            }
        }

        private void ShowMessage(string message, string title)
        {
            // Create the alert controller.
            UIAlertController alertController = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
            alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));

            // Show the alert.
            PresentViewController(alertController, true, null);
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = UIColor.White};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            UILabel helpLabel = new UILabel
            {
                Text = "Tap to select features.",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Center,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f),
                TextColor = UIColor.White,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Add the views.
            View.AddSubviews(_myMapView, helpLabel);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                helpLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                helpLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                helpLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                helpLabel.HeightAnchor.ConstraintEqualTo(40)
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }
    }

    public class AttachmentsTableView : UITableViewController
    {
        private ArcGISFeature _feature;

        public AttachmentsTableView(ArcGISFeature feature)
        {
            _feature = feature;
            Title = $"Attachments for #{feature.Attributes["objectid"]}";
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Load the feature.
                await _feature.LoadAsync();

                // Get the attachments.
                IReadOnlyList<Attachment> attachments = await _feature.GetAttachmentsAsync();

                // Configure the view source.
                AttachmentsTableSource tableViewSource = new AttachmentsTableSource(this, _feature, attachments);

                // Add the source to the tableview.
                TableView.Source = tableViewSource;

                // Show the edit button in the navigation bar.
                NavigationItem.RightBarButtonItem = EditButtonItem;

                // Reload the tableview.
                TableView.ReloadData();
            }
            catch (Exception e)
            {
                ShowMessage(e.ToString(), "Error loading attachments");
            }
        }

        private void ShowMessage(string message, string title)
        {
            // Create the alert controller.
            UIAlertController alertController = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
            alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));

            // Show the alert.
            PresentViewController(alertController, true, null);
        }

        private class AttachmentsTableSource : UITableViewSource
        {
            private IReadOnlyList<Attachment> _attachments;
            private ArcGISFeature _selectedFeature;
            private TaskCompletionSource<Stream> _taskCompletionSource;
            private UIImagePickerController _imagePicker;
            private string _filename;
            private AttachmentsTableView _viewController;

            public AttachmentsTableSource(AttachmentsTableView controller, ArcGISFeature selectedFeature, IReadOnlyList<Attachment> attachments)
            {
                _attachments = attachments;
                _selectedFeature = selectedFeature;
                _viewController = controller;
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                // Gets a cell for the specified section and row.
                if (indexPath.Row < _attachments.Count)
                {
                    var cell = new UITableViewCell(UITableViewCellStyle.Default, "AttachmentCell");
                    cell.TextLabel.Text = _attachments[indexPath.Row].Name;
                    cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
                    return cell;
                }
                else
                {
                    var cell = new UITableViewCell(UITableViewCellStyle.Default, "InsertCell");
                    cell.TextLabel.Text = "Add attachment";
                    cell.Accessory = UITableViewCellAccessory.Checkmark;
                    return cell;
                }
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                if (section == 0)
                {
                    return _attachments?.Count + 1 ?? 0;
                }

                return 0;
            }

            public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
            {
                // Commit editing - in this case, editing means insertion or deletion.
                switch (editingStyle)
                {
                    case UITableViewCellEditingStyle.Delete:
                        Attachment selectedAttachment = _attachments[indexPath.Row];
                        DeleteAttachment(selectedAttachment, tableView);
                        break;
                    case UITableViewCellEditingStyle.Insert:
                        AddAttachment(tableView);
                        break;
                }

                // Force the view to reload its data.
                tableView.ReloadData();
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                // Add a new attachment.
                if (indexPath.Row >= _attachments.Count)
                {
                    AddAttachment(tableView);
                }
                else
                {
                    PreviewAttachment(_attachments[indexPath.Row]);
                }
            }

            private async void PreviewAttachment(Attachment selectedAttachment)
            {
                if (selectedAttachment.ContentType.Contains("image"))
                {
                    // Create a preview and show it.
                    // Get the image data.
                    Stream contentStream = await selectedAttachment.GetDataAsync();
                    var imageData = NSData.FromStream(contentStream);
                    var image = UIImage.LoadFromData(imageData);
                    UIImageView imageView = new UIImageView(image);
                    imageView.TranslatesAutoresizingMaskIntoConstraints = false;
                    imageView.ContentMode = UIViewContentMode.ScaleAspectFit;
                    var imagePreviewVC = new UIViewController();
                    imagePreviewVC.View = new UIView {BackgroundColor = UIColor.White};
                    imagePreviewVC.View.AddSubview(imageView);
                    imagePreviewVC.Title = "Attachment preview";
                    NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]
                    {
                        imageView.TopAnchor.ConstraintEqualTo(imagePreviewVC.View.SafeAreaLayoutGuide.TopAnchor),
                        imageView.LeftAnchor.ConstraintEqualTo(imagePreviewVC.View.SafeAreaLayoutGuide.LeftAnchor),
                        imageView.RightAnchor.ConstraintEqualTo(imagePreviewVC.View.SafeAreaLayoutGuide.RightAnchor),
                        imageView.BottomAnchor.ConstraintEqualTo(imagePreviewVC.View.SafeAreaLayoutGuide.BottomAnchor)
                    });

                    // Show the preview.
                    _viewController.NavigationController.PushViewController(imagePreviewVC, true);
                }
                else
                {
                    _viewController.ShowMessage("This sample can only show image attachments", "Can't show attachment");
                }
            }

            public override string TitleForDeleteConfirmation(UITableView tableView, NSIndexPath indexPath)
            {
                return "Delete attachment";
            }

            public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
            {
                return true;
            }

            public override UITableViewCellEditingStyle EditingStyleForRow(UITableView tableView, NSIndexPath indexPath)
            {
                if (indexPath.Row < _attachments.Count)
                {
                    return UITableViewCellEditingStyle.Delete;
                }
                else
                {
                    return UITableViewCellEditingStyle.Insert;
                }
            }

            #region attachment actions
            private async void AddAttachment(UITableView tableView)
            {
                byte[] attachmentData;
                string filename;
                string contentType = "image/jpg";

                Stream imageStream = await GetImageStreamAsync();
                if (imageStream == null)
                {
                    return;
                }

                attachmentData = new byte[imageStream.Length];
                imageStream.Read(attachmentData, 0, attachmentData.Length);
                filename = _filename ?? "file1.jpg";

                // Add the attachment.
                await _selectedFeature.AddAttachmentAsync(filename, contentType, attachmentData);

                // Update the table.
                await ((ServiceFeatureTable) _selectedFeature.FeatureTable).ApplyEditsAsync();

                // Update UI.
                _selectedFeature.Refresh();
                _attachments = await _selectedFeature.GetAttachmentsAsync();
                tableView.ReloadData();
                _viewController.ShowMessage("Successfully added attachment", "Success!");
            }

            private async void DeleteAttachment(Attachment attachmentToDelete, UITableView tableView)
            {
                try
                {
                    // Delete the attachment.
                    await _selectedFeature.DeleteAttachmentAsync(attachmentToDelete);

                    // Update the table.
                    await ((ServiceFeatureTable) _selectedFeature.FeatureTable).ApplyEditsAsync();

                    // Update UI.
                    _selectedFeature.Refresh();
                    _attachments = await _selectedFeature.GetAttachmentsAsync();
                    tableView.ReloadData();
                    _viewController.ShowMessage("Successfully deleted attachment", "Success!");
                }
                catch (Exception exception)
                {
                    _viewController.ShowMessage(exception.ToString(), "Error deleting attachment");
                }
            }
            #endregion attachment actions

            #region file picker
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
            #endregion file picker
        }
    }
}