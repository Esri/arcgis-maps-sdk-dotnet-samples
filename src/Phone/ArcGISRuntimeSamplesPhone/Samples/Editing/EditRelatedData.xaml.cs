using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
    /// <summary>
    /// Demonstrates how to query and edit related records.
    /// </summary>
    /// <title>Edit Related Data</title>
    /// <category>Editing</category>
    public partial class EditRelatedData : Page
    {
        // Query is done through this relationship.
        Esri.ArcGISRuntime.ArcGISServices.Relationship relationship;
        // Editing is done through this table.
        ServiceFeatureTable table;

        public EditRelatedData()
        {
            InitializeComponent();
            var layer = MyMapView.Map.Layers["ServiceRequests"] as ArcGISDynamicMapServiceLayer;
            layer.VisibleLayers = new ObservableCollection<int>(new int[] { 0 });
        }

        /// <summary>
        /// Identifies graphic to highlight and query its related records.
        /// </summary>
        private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            // Get current viewpoints extent from the MapView
            var currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
            var viewpointExtent = currentViewpoint.TargetGeometry.Extent;

            var layer = MyMapView.Map.Layers["ServiceRequests"] as ArcGISDynamicMapServiceLayer;
            var task = new IdentifyTask(new Uri(layer.ServiceUri));
            var mapPoint = MyMapView.ScreenToLocation(e.Position);
            var parameter = new IdentifyParameters(mapPoint, viewpointExtent, 2, (int)MyMapView.ActualHeight, (int)MyMapView.ActualWidth);

            // Clears map of any highlights.
            var overlay = MyMapView.GraphicsOverlays["Highlighter"] as GraphicsOverlay;
            overlay.Graphics.Clear();

            SetRelatedRecordEditor();

            string message = null;
            try
            {
                // Performs an identify and adds graphic result into overlay.
                var result = await task.ExecuteAsync(parameter);
                if (result == null || result.Results == null || result.Results.Count < 1)
                    return;
                var graphic = (Graphic)result.Results[0].Feature;
                overlay.Graphics.Add(graphic);

                // Prepares related records editor.
                var featureID = Convert.ToInt64(graphic.Attributes["OBJECTID"], CultureInfo.InvariantCulture);
                string requestID = null;
                if (graphic.Attributes["Service Request ID"] != null)
                    requestID = Convert.ToString(graphic.Attributes["Service Request ID"], CultureInfo.InvariantCulture);
                SetRelatedRecordEditor(featureID, requestID);

                await QueryRelatedRecordsAsync();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();
        }

        /// <summary>
        /// Prepares related record editor for add and query.
        /// </summary>
        private void SetRelatedRecordEditor(long featureID = 0, string requestID = null)
        {
            AddButton.Tag = featureID;
            AddButton.IsEnabled = featureID > 0 && !string.IsNullOrWhiteSpace(requestID);
            RelatedRecords.Tag = requestID;
            if (featureID == 0)
            {
                Records.IsEnabled = false;
                RelatedRecords.ItemsSource = null;
                RelatedRecords.SelectedItem = null;
            }
            SetAttributeEditor();
        }

        /// <summary>
        /// Prepares attribute editor for update or delete of existing related record.
        /// </summary>
        private void SetAttributeEditor(Feature feature = null)
        {
            if (ChoiceList.ItemsSource == null && table != null && table.ServiceInfo != null && table.ServiceInfo.Fields != null)
            {
                var rangeDomain = (RangeDomain<IComparable>)table.ServiceInfo.Fields.FirstOrDefault(f => f.Name == "rank").Domain;
                ChoiceList.ItemsSource = GetRangeValues((int)rangeDomain.MinValue, (int)rangeDomain.MaxValue);
            }
            AttributeEditor.Tag = feature;
            if (feature != null)
            {
                if (feature.Attributes.ContainsKey("rank") && feature.Attributes["rank"] != null)
                    ChoiceList.SelectedItem = Convert.ToInt32(feature.Attributes["rank"], CultureInfo.InvariantCulture);
                if (feature.Attributes.ContainsKey("comments") && feature.Attributes["comments"] != null)
                    Comments.Text = Convert.ToString(feature.Attributes["comments"], CultureInfo.InvariantCulture);
                AttributeEditor.Visibility = Visibility.Visible;
                Records.Flyout.ShowAt(Records);
            }
            else
            {
                ChoiceList.SelectedItem = null;
                Comments.Text = string.Empty;
                AttributeEditor.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Gets exclusive values between minimum and maximum values.
        /// </summary>
        private IEnumerable<int> GetRangeValues(int min, int max)
        {
            for (int i = min; i <= max; i++)
                yield return i;
        }

        /// <summary>
        /// Gets relationship information using service metadata.
        /// </summary>
        private async Task<Esri.ArcGISRuntime.ArcGISServices.Relationship> GetRelationshipAsync()
        {
            var layer = MyMapView.Map.Layers["ServiceRequests"] as ArcGISDynamicMapServiceLayer;
            if (layer == null || layer.VisibleLayers == null)
                return null;
            var id = layer.VisibleLayers.FirstOrDefault();
            var details = await layer.GetDetailsAsync(id);
            if (details != null && details.Relationships != null)
                return details.Relationships.FirstOrDefault();
            return null;
        }

        /// <summary>
        /// Queries related records for specified feature ID.
        /// </summary>
        private async Task QueryRelatedRecordsAsync()
        {
            var featureID = (Int64)AddButton.Tag;
            var layer = MyMapView.Map.Layers["ServiceRequests"] as ArcGISDynamicMapServiceLayer;
            var id = layer.VisibleLayers.FirstOrDefault();
            var task = new QueryTask(new Uri(string.Format("{0}/{1}", layer.ServiceUri, id)));
            if (relationship == null)
                relationship = await GetRelationshipAsync();
            var parameters = new RelationshipParameters(new List<long>(new long[] { featureID }), relationship.ID);
            parameters.OutFields = new OutFields(new string[] { "objectid" });
            var result = await task.ExecuteRelationshipQueryAsync(parameters);
            if (result != null && result.RelatedRecordGroups != null && result.RelatedRecordGroups.ContainsKey(featureID))
            {
                RelatedRecords.ItemsSource = result.RelatedRecordGroups[featureID];
                Records.IsEnabled = true;
            }
        }

        /// <summary>
        /// Submits related record edits back to server.
        /// </summary>
        private async Task SaveEditsAsync()
        {
            if (table == null || !table.HasEdits)
                return;
            if (table is ServiceFeatureTable)
            {
                var serviceTable = (ServiceFeatureTable)table;
                // Pushes new graphic back to the server.
                var result = await serviceTable.ApplyEditsAsync();
                if (result.AddResults == null || result.AddResults.Count < 1)
                    return;
                var addResult = result.AddResults[0];
                if (addResult.Error != null)
                    throw addResult.Error;
            }
        }

        /// <summary>
        /// Gets related table for editing.
        /// </summary>
        private async Task<ServiceFeatureTable> GetRelatedTableAsync()
        {
            var layer = MyMapView.Map.Layers["ServiceRequests"] as ArcGISDynamicMapServiceLayer;
            // Creates table based on related table ID of the visible layer in dynamic layer 
            // using FeatureServer specifying rank and comments fields to enable editing.
            var id = relationship.RelatedTableID.Value;
            var url = layer.ServiceUri.Replace("MapServer", "FeatureServer");
            url = string.Format("{0}/{1}", url, id);
            var table = await ServiceFeatureTable.OpenAsync(new Uri(url), null, MyMapView.SpatialReference);
            table.OutFields = new OutFields(new string[] { relationship.KeyField, "rank", "comments", "submitdt" });
            return table;
        }

        /// <summary>
        /// Displays current attribute values of related record.
        /// </summary>
        private async void RelatedRecords_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RelatedRecords.SelectedItem == null)
                return;
            var graphic = (Graphic)RelatedRecords.SelectedItem;
            SetAttributeEditor();
            string message = null;
            try
            {
                if (table == null)
                    table = await GetRelatedTableAsync();
                var featureID = Convert.ToInt64(graphic.Attributes[table.ObjectIDField], CultureInfo.InvariantCulture);
                var feature = await table.QueryAsync(featureID);
                SetAttributeEditor(feature);
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();
        }

        /// <summary>
        /// Adds a new related record to highlighted feature.
        /// </summary>
        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            SetAttributeEditor();
            var featureID = (Int64)AddButton.Tag;
            var requestID = (string)RelatedRecords.Tag;
            string message = null;
            try
            {
                if (table == null)
                    table = await GetRelatedTableAsync();
                var feature = new GeodatabaseFeature(table.Schema);
                feature.Attributes[relationship.KeyField] = requestID;
                feature.Attributes["rank"] = 5;
                feature.Attributes["comments"] = "Describe service requirement here.";
                feature.Attributes["submitdt"] = DateTime.UtcNow;
                var relatedFeatureID = await table.AddAsync(feature);
                await SaveEditsAsync();
                await QueryRelatedRecordsAsync();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();
        }

        /// <summary>
        /// Updates attributes of related record.
        /// </summary>
        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (AttributeEditor.Tag == null) return;
            var feature = (GeodatabaseFeature)AttributeEditor.Tag;
            string message = null;
            try
            {
                if (ChoiceList.SelectedItem != null)
                    feature.Attributes["rank"] = (int)ChoiceList.SelectedItem;
                feature.Attributes["comments"] = Comments.Text.Trim();
                await table.UpdateAsync(feature);
                await SaveEditsAsync();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();
        }

        /// <summary>
        /// Deletes related record.
        /// </summary>
        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (RelatedRecords.SelectedItem == null) return;
            var graphic = (Graphic)RelatedRecords.SelectedItem;
            string message = null;
            try
            {
                if (table == null)
                    table = await GetRelatedTableAsync();
                var featureID = Convert.ToInt64(graphic.Attributes[table.ObjectIDField], CultureInfo.InvariantCulture);
                await table.DeleteAsync(featureID);
                await SaveEditsAsync();
                await QueryRelatedRecordsAsync();
                SetAttributeEditor();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();
        }

        private void ChoiceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Records.Flyout.ShowAt(Records);
        }
    }
}
