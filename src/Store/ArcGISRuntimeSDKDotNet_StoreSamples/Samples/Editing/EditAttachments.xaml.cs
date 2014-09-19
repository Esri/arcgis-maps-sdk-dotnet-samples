using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Imaging;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates how attachments can be queried and modified from a ServiceFeatureTable and how this type of edit is pushed to the server or canceled.
    /// </summary>
    /// <title>Edit Attachments</title>
    /// <category>Editing</category>
    public partial class EditAttachments : Page
    {
        public EditAttachments()
        {
            InitializeComponent();
        }
        
        private FeatureLayer GetFeatureLayer()
        {
            if (MyMapView.Map == null || MyMapView.Map.Layers == null)
                return null;
            var layer = MyMapView.Map.Layers["IncidentsLayer"] as FeatureLayer;
            return layer;
        }

        private ServiceFeatureTable GetFeatureTable(FeatureLayer ownerLayer)
        {
            var layer = ownerLayer ?? GetFeatureLayer();
            if (layer == null || !(layer.FeatureTable is ServiceFeatureTable))
                return null;
            var table = (ServiceFeatureTable)layer.FeatureTable;
            return table;
        }
        
        private async Task QueryAttachmentsAsync(ServiceFeatureTable table, long featureID)
        {
            if (table == null)
                return;
            // By default, the result is a union of local and server query for attachments.
            var queryAttachmentResult = await table.QueryAttachmentsAsync(featureID);
            AttachmentList.ItemsSource = queryAttachmentResult != null ? queryAttachmentResult.Infos : null;
            AttachmentsButton.IsEnabled = AttachmentList.Items != null && AttachmentList.Items.Any();
        }

        private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            var layer = GetFeatureLayer();
            var table = GetFeatureTable(layer);
            if (layer == null || table == null)
                return;
            // Updates layer selection and attachment list.
            layer.ClearSelection();
            AttachmentList.ItemsSource = null;
            string message = null;
            try
            {
                // Selects feature based on hit-test  
                // and performs query attachments on the first selected feature.
                var featureIDs = await layer.HitTestAsync(MyMapView, e.Position);
                if (featureIDs != null)
                {
                    layer.SelectFeatures(featureIDs);
                    var featureID = featureIDs.FirstOrDefault();
                    await QueryAttachmentsAsync(table, featureID);
                }
                AddButton.IsEnabled = layer.SelectedFeatureIDs.Any();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();
        }

        private async Task<StorageFile> GetFileAsync()
        {
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.FileTypeFilter.Add(".tif");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".gif");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".bmp");
            return await picker.PickSingleFileAsync();
        }

        private enum EditType
        {
            Add,
            Update,
            Delete
        }

        private async Task UpdateAttachmentListAsync(AttachmentResult attachmentResult, EditType editType)
        {
            if (attachmentResult == null)
                return;
            var layer = GetFeatureLayer();
            var table = GetFeatureTable(layer);
            if (layer == null || layer.SelectedFeatureIDs == null || !layer.SelectedFeatureIDs.Any() ||
                table == null)
                return;
            string message = null;
            try
            {
                    if (attachmentResult.Error != null)
                        message = attachmentResult.Error.Message;
                    else
                    {
                        message = string.Format("{0} attachment [{1}] {2} feature [{3}]",
                            editType == EditType.Add ? "Add" : (editType == EditType.Update ? "Update" : "Delete"),
                            attachmentResult.ObjectID,
                            editType == EditType.Add ? "to" : "from",
                            attachmentResult.ParentID);
                    }
                    // Performs another query on attachments.
                    await QueryAttachmentsAsync(table, attachmentResult.ParentID);
                    SaveButton.IsEnabled = table.HasEdits;
                }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var layer = GetFeatureLayer();
            var table = GetFeatureTable(layer);
            if (layer == null || layer.SelectedFeatureIDs == null || !layer.SelectedFeatureIDs.Any() ||
                table == null)
                return;
            var featureID = layer.SelectedFeatureIDs.FirstOrDefault();           
            string message = null;
            try
            {
                var file = await GetFileAsync();
                // Adds attachment to the specified feature.
                var attachmentResult = await table.AddAttachmentAsync(featureID, file);
                await UpdateAttachmentListAsync(attachmentResult, EditType.Add);
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();
        }

        private async void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var layer = GetFeatureLayer();
            var table = GetFeatureTable(layer);
            if (layer == null || layer.SelectedFeatureIDs == null || !layer.SelectedFeatureIDs.Any() ||
                table == null)
                return;
            var info = (sender as Button).DataContext as AttachmentInfoItem;
            if (info == null)
                return;
            var featureID = layer.SelectedFeatureIDs.FirstOrDefault();
            var stream = Stream.Null;
            string name = null;
            string message = null;
            try
            {
                var file = await GetFileAsync();
                // Updates the specified attachment.
                var attachmentResult = await table.UpdateAttachmentAsync(featureID, info.ID, file);
                await UpdateAttachmentListAsync(attachmentResult, EditType.Update);
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                new MessageDialog(message).ShowAsync();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var layer = GetFeatureLayer();
            var table = GetFeatureTable(layer);
            if (layer == null || layer.SelectedFeatureIDs == null || !layer.SelectedFeatureIDs.Any() ||
                table == null || !table.HasEdits)
                return;
            var info = (sender as Button).DataContext as AttachmentInfoItem;
            if (info == null)
                return;
            var featureID = layer.SelectedFeatureIDs.FirstOrDefault();
            string message = null;
            try
            {
                // Deletes the specified attachment.
                var deleteAttachmentResult = await table.DeleteAttachmentsAsync(featureID, new long[] { info.ID });
                var attachmentResult = deleteAttachmentResult != null && deleteAttachmentResult.Results != null ?
                    deleteAttachmentResult.Results.FirstOrDefault() : null;
                await UpdateAttachmentListAsync(attachmentResult, EditType.Delete);
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();
        }

        private async void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            var info = (sender as Button).DataContext as AttachmentInfoItem;
            if (info == null)
                return;
            Image image = null;
            string message = null;
            try
            {
                // OnlineAttachmentInfoItem is returned if attachment was not marked for edit.
                if (info is OnlineAttachmentInfoItem)
                {
                    var onlineItem = (OnlineAttachmentInfoItem)info;
                    // Uri property is only available on OnlineAttachmentInfoItem.
                    image = new Image() { Source = new BitmapImage(onlineItem.Uri) };
                }
                else
                {
                    // Otherwise, retrieve the local attachment stream data.
                    var data = await info.GetDataAsync();
                    if (data != Stream.Null)
                    {
                        var source = new BitmapImage();
                        await source.SetSourceAsync(data.AsRandomAccessStream());                        
                        image = new Image() { Source = source };
                    }
                }
                // Displays the attachment as an image onto a new dialog window.
                if (image != null)
                {
                    var panel = new StackPanel();
                    panel.Children.Add(new TextBlock() { Text = info.Name, TextAlignment = TextAlignment.Left });
                    panel.Children.Add(image);
                    var flyout = new Flyout() { Content = panel };
                    flyout.ShowAt(AttachmentsButton);
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();
        }

        private static string GetResultMessage(IEnumerable<AttachmentResult> attachmentResults, EditType editType)
        {
            var sb = new StringBuilder();
            var operation = editType == EditType.Add ? "adds" :
                (editType == EditType.Update ? "updates" : "deletes");
            if (attachmentResults.Any(r => r.Error != null))
            {
                sb.AppendLine(string.Format("Failed {0} : [{1}]", operation, string.Join(", ", from r in attachmentResults
                                                                                               where r.Error != null
                                                                                               select string.Format("{0} : {1}", r.ObjectID, r.Error != null ? r.Error.Message : string.Empty))));
            }
            if (attachmentResults.Any(r => r.Error == null))
            {
                sb.AppendLine(string.Format("Successful {0} : [{1}]", operation, string.Join(", ", from r in attachmentResults
                                                                                                   where r.Error == null
                                                                                                   select r.ObjectID)));
            }
            return sb.ToString();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var layer = GetFeatureLayer();
            var table = GetFeatureTable(layer);
            if (layer == null || table == null || !table.HasEdits)
                return;
            string message = null;
            try
            {
                // Submits the attachment edits to server.
                var saveResult = await table.ApplyAttachmentEditsAsync();
                if (saveResult != null)
                {
                    var sb = new StringBuilder();
                    var editMessage = GetResultMessage(saveResult.AddResults, EditType.Add);
                    if (!string.IsNullOrWhiteSpace(editMessage))
                        sb.AppendLine(editMessage);
                    editMessage = GetResultMessage(saveResult.UpdateResults, EditType.Update);
                    if (!string.IsNullOrWhiteSpace(editMessage))
                        sb.AppendLine(editMessage);
                    if (saveResult.DeleteResults != null)
                    {
                        foreach (var deleteResult in saveResult.DeleteResults)
                        {
                            editMessage = GetResultMessage(deleteResult.Results, EditType.Delete);
                            if (!string.IsNullOrWhiteSpace(editMessage))
                                sb.AppendLine(editMessage);
                        }
                    }
                    message = sb.ToString();
                }
                SaveButton.IsEnabled = table.HasEdits;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();
        }

        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var layer = GetFeatureLayer();
            var table = GetFeatureTable(layer);
            if (layer == null || layer.SelectedFeatureIDs == null || !layer.SelectedFeatureIDs.Any() ||
                table == null || !table.HasEdits)
                return;
            string message = null;
            try
            {
                // Cancels the local edits by refreshing features with preserveEdits=false.
                table.RefreshFeatures(false);
                var featureID = layer.SelectedFeatureIDs.FirstOrDefault();
                await QueryAttachmentsAsync(table, featureID);
                SaveButton.IsEnabled = table.HasEdits;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();
        }
    }
}
