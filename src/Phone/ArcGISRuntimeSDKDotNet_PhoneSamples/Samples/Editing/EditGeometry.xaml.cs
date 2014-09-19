using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Edit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates how geometry of a feature can be modified from a ServiceFeatureTable
    /// and how this type of edit is pushed to the server or canceled.
    /// </summary>
    /// <title>Edit Geometry</title>
    /// <category>Editing</category>
    public partial class EditGeometry : Page
    {
        public EditGeometry()
        {
            InitializeComponent();
        }
        
        private enum EditType
        {
            Add,
            Update,
            Delete
        }

        private FeatureLayer GetFeatureLayer()
        {
            if (MyMapView == null || MyMapView.Map == null || MyMapView.Map.Layers == null)
                return null;
            var layer = MyMapView.Map.Layers["ThreatAreas"] as FeatureLayer;
            if (layer == null)
                return null;
            return layer;
        }

        private ServiceFeatureTable GetFeatureTable(FeatureLayer owner = null)
        {
            var layer = owner ?? GetFeatureLayer();
            if (layer == null)
                return null;
            var table = layer.FeatureTable as ServiceFeatureTable;
            if (table == null)
                return null;
            return table;
        }
        
        private async Task<Polygon> GetPolygonAsync()
        {
            // Determine if Freehand is enabled.
            var isFreehand = IsFreehand.IsChecked.HasValue && IsFreehand.IsChecked.Value;
            var drawShape = isFreehand ? DrawShape.Freehand : DrawShape.Polygon;

            // Builds geometry using Editor.
            var geometry = await MyMapView.Editor.RequestShapeAsync(drawShape);
            var polygon = geometry as Polygon;

            // DrawShape.Freehand will return Polyline, convert to Polygon since service expects Polygon type.
            if (isFreehand && geometry is Polyline)
            {
                var polyline = (Polyline)geometry;
                polygon = new Polygon(polyline.Parts, polyline.SpatialReference);
            }

            // Simplify Polygon to correct ring orientation.
            polygon = GeometryEngine.Simplify(polygon) as Polygon;
            return polygon;
        }
        
        private async Task<IEnumerable<Feature>> GetSelectedFeaturesAsync(ServiceFeatureTable table, IEnumerable<long> selectedFeatureIDs)
        {
            IEnumerable<Feature> selectedFeatures = null;
            if (selectedFeatureIDs != null && selectedFeatureIDs.Any())
                selectedFeatures = await table.QueryAsync(selectedFeatureIDs, true);
            else
            {
                var isAutoSelect = IsAutoSelect.IsChecked.HasValue && IsAutoSelect.IsChecked.Value;
                if (isAutoSelect)
                    selectedFeatures = await table.QueryAsync(new SpatialQueryFilter()
                    {
                        Geometry = MyMapView.Extent,
                        SpatialRelationship = SpatialRelationship.Intersects
                    }, true);
            }
            return selectedFeatures;
        }
        
        private async Task<Polygon> GetAutoCompletedPolygonAsync(Polygon polygon, IEnumerable<Feature> selectedFeatures)
        {

            // Perform auto-complete on selected features.
            if (selectedFeatures != null && selectedFeatures.Any())
            {
                var existingBoundaries = from f in selectedFeatures
                                         select f.Geometry as Polygon;
                var newBoundaries = new Polyline[] { polygon.ToPolyline() };
                var autoCompleteResult = GeometryEngine.AutoComplete(existingBoundaries, newBoundaries);
                if (autoCompleteResult != null && autoCompleteResult.Any())
                {
                    var polygonBuilder = new PolygonBuilder(MyMapView.SpatialReference);
                    foreach (var geom in autoCompleteResult)
                    {
                        if (geom is Polygon)
                        {
                            var p = (Polygon)geom;
                            foreach (var i in p.Parts)
                                polygonBuilder.Parts.AddPart(i);
                        }
                    }
                    polygon = polygonBuilder.ToGeometry();
                }
            }
            return polygon;
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var layer = GetFeatureLayer();
            if (layer == null)
                return;
            var table = GetFeatureTable(layer);
            if (table == null)
                return;
            var typeId = Convert.ToInt32((sender as Button).Tag, CultureInfo.InvariantCulture);
            string message = null;
            try
            {
                while (true)
                {
                    // Retrieves a polygon from the Editor and simplifies geometry.
                    var polygon = await GetPolygonAsync();

                    // Determine if AutoComplete is enabled.
                    var isAutoComplete = IsAutoComplete.IsChecked.HasValue && IsAutoComplete.IsChecked.Value;
                    if (isAutoComplete)
                    {
                        // Use selected features if any; otherwise, query features using spatial filter on current map extent.
                        var selectedFeatures = await GetSelectedFeaturesAsync(table, layer.SelectedFeatureIDs);
                        // Retrieves auto-completed polygon against selected features.
                        polygon = await GetAutoCompletedPolygonAsync(polygon, selectedFeatures);
                    }

                    // Creates GeodatabaseFeature based on Table.Schema and Geometry.
                    var feature = new GeodatabaseFeature(table.Schema)
                    {
                        Geometry = polygon
                    };

                    // Updates its Attributes based on desired FeatureType.
                    if (table.ServiceInfo != null && table.ServiceInfo.Types != null)
                    {
                        var featureType = table.ServiceInfo.Types.FirstOrDefault(t => (int)t.ID == typeId);
                        if (featureType != null && featureType.Templates != null)
                        {
                            var template = featureType.Templates.FirstOrDefault();
                            if (template != null && template.Prototype != null && template.Prototype.Attributes != null)
                            {
                                foreach (var item in template.Prototype.Attributes)
                                    feature.Attributes[item.Key] = item.Value;
                            }
                        }
                    }

                    // Adds feature to local geodatabase.
                    var id = await table.AddAsync(feature);

                    SaveButton.IsEnabled = table.HasEdits;
                    var isContinuous = IsContinuous.IsChecked.HasValue && IsContinuous.IsChecked.Value;
                    if (!isContinuous)
                        break;
                }
            }
            catch (TaskCanceledException tcex)
            {

            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();

        }
        
        private async void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            var layer = GetFeatureLayer();
            if (layer == null)
                return;
            var table = GetFeatureTable(layer);
            if (table == null)
                return;
            string message = null;
            var tag = Convert.ToString((sender as MenuFlyoutItem).Tag, CultureInfo.InvariantCulture);
            try
            {
                while (true)
                {
                    long[] selectionFeatureIDs = null;
                    // Determine if Freehand is enabled.
                    var isFreehand = IsFreehand.IsChecked.HasValue && IsFreehand.IsChecked.Value;
                    var drawShape = isFreehand ? DrawShape.Freehand : DrawShape.Envelope;

                    // Builds the geometry used for selection using Editor.
                    var geometry = await MyMapView.Editor.RequestShapeAsync(drawShape);

                    // Clear selection based on type of selection.
                    long[] selectedFeatureIDs = null;
                    if (tag == "New")
                        layer.ClearSelection();

                    // DrawShape.Freehand will return Polyline, convert to Polygon and use Query with Spatial filter to select.
                    if (isFreehand && geometry is Polyline)
                    {
                        var polyline = (Polyline)geometry;
                        var polygon = new Polygon(polyline.Parts, polyline.SpatialReference);
                        var features = await table.QueryAsync(new SpatialQueryFilter()
                        {
                            Geometry = polygon,
                            SpatialRelationship = SpatialRelationship.Intersects
                        }, true);
                        if (features != null && features.Any() && !string.IsNullOrWhiteSpace(table.ServiceInfo.ObjectIdField))
                        {
                            selectedFeatureIDs = (from f in features
                                                  where f.Attributes.ContainsKey(table.ServiceInfo.ObjectIdField)
                                                  select Convert.ToInt64(f.Attributes[table.ServiceInfo.ObjectIdField], CultureInfo.InvariantCulture)).ToArray();
                        }
                    }
                    else if (geometry is Envelope) // use HitTest
                    {
                        var envelope = (Envelope)geometry;
                        var upperLeft = MyMapView.LocationToScreen(new MapPoint(envelope.XMin, envelope.YMax, geometry.SpatialReference));
                        var lowerRight = MyMapView.LocationToScreen(new MapPoint(envelope.XMax, envelope.YMin, geometry.SpatialReference));
                        var rect = new Rect(upperLeft, lowerRight);
                        var maxHits = Convert.ToInt32(table.RowCount, CultureInfo.InvariantCulture);
                        selectedFeatureIDs = await layer.HitTestAsync(MyMapView, rect, maxHits);
                    }
                    // Update layer selection based on tag.
                    if (selectedFeatureIDs != null && selectedFeatureIDs.Any())
                    {
                        if (tag == "Remove")
                            layer.UnselectFeatures(selectedFeatureIDs);
                        else
                        {
                            // Filter out features that are already selected.
                            if (layer.SelectedFeatureIDs != null && layer.SelectedFeatureIDs.Any())
                            {
                                selectedFeatureIDs = (from f in selectedFeatureIDs
                                                      where !layer.SelectedFeatureIDs.Any(i => i == f)
                                                      select f).ToArray();
                            }
                            if (selectedFeatureIDs.Any())
                                layer.SelectFeatures(selectedFeatureIDs);
                        }
                    }
                    var selectionCount = layer.SelectedFeatureIDs.Count();
                    DeleteSelectedButton.IsEnabled = selectionCount > 0;
                    UnionButton.IsEnabled = selectionCount > 1;
                    var isContinuous = IsContinuous.IsChecked.HasValue && IsContinuous.IsChecked.Value;
                    if (!isContinuous)
                        break;
                }
            }
            catch (TaskCanceledException tcex)
            {

            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();
        }
        
        private void ClearSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            var layer = GetFeatureLayer();
            if (layer == null)
                return;
            // Clears selection on layer if any.
            if (layer.SelectedFeatureIDs != null && layer.SelectedFeatureIDs.Any())
                layer.ClearSelection();
        }

        private async void DeleteSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            var layer = GetFeatureLayer();
            if (layer == null)
                return;
            var table = GetFeatureTable(layer);
            if (table == null)
                return;
            string message = null;
            try
            {
                if (layer.SelectedFeatureIDs != null && layer.SelectedFeatureIDs.Any())
                {
                    // Delete the selected features by passing the IDs
                    await table.DeleteAsync(layer.SelectedFeatureIDs);
                    SaveButton.IsEnabled = table.HasEdits;
                }
            }
            catch (TaskCanceledException tcex)
            {
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();
        }

        private async void EditVerticesButton_Click(object sender, RoutedEventArgs e)
        {
            var layer = GetFeatureLayer();
            if (layer == null)
                return;
            var table = GetFeatureTable(layer);
            if (table == null)
                return;
            string message = null;
            long? id = null;
            try
            {
                while (true)
                {
                    // Select feature for editing by performing hit test on tap.
                    var mapPoint = await MyMapView.Editor.RequestPointAsync() as MapPoint;
                    var screenPoint = MyMapView.LocationToScreen(mapPoint);
                    var featureIDs = await layer.HitTestAsync(MyMapView, screenPoint);
                    if (featureIDs == null || !featureIDs.Any())
                        continue;
                    id = featureIDs.FirstOrDefault();
                    if (id.HasValue)
                    {
                        // Hide the original feature using its ID.
                        layer.SetFeatureVisibility(new long[] { id.Value }, false);
                        // Query based on ID to get the feature.
                        var features = await table.QueryAsync(new long[] { id.Value }, true);
                        if (features != null)
                        {
                            var feature = features.FirstOrDefault();
                            if (feature != null)
                            {
                                // Use Editor to update its Geometry.
                                var geometry = await MyMapView.Editor.EditGeometryAsync(feature.Geometry);
                                var polygon = geometry as Polygon;
                                // Simplify Polygon to make geometry topologically correct.
                                polygon = GeometryEngine.Simplify(polygon) as Polygon;
                                feature.Geometry = polygon;
                                await table.UpdateAsync(feature);
                                // Unhide the original feature. 
                                if (id.HasValue)
                                    layer.SetFeatureVisibility(new long[] { id.Value }, true);
                                SaveButton.IsEnabled = table.HasEdits;
                            }
                        }
                    }
                    var isContinuous = IsContinuous.IsChecked.HasValue && IsContinuous.IsChecked.Value;
                    if (!isContinuous)
                        break;
                }
            }
            catch (TaskCanceledException tcex)
            {
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            finally
            {
                // Unhide the original feature. 
                if (id.HasValue)
                    layer.SetFeatureVisibility(new long[] { id.Value }, true);
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();
        }

        private async Task<Polyline> GetPolylineAsync()
        {
            // Determine if Freehand is enabled.
            var isFreehand = IsFreehand.IsChecked.HasValue && IsFreehand.IsChecked.Value;
            var drawShape = isFreehand ? DrawShape.Freehand : DrawShape.Polyline;

            // Builds reshaper using Editor.
            var geometry = await MyMapView.Editor.RequestShapeAsync(drawShape);
            var polyline = geometry as Polyline;

            // Simplify Polyline to correct path orientation.
            polyline = GeometryEngine.Simplify(polyline) as Polyline;
            return polyline;
        }

        private async void CutButton_Click(object sender, RoutedEventArgs e)
        {
            var layer = GetFeatureLayer();
            if (layer == null)
                return;
            var table = GetFeatureTable(layer);
            if (table == null)
                return;
            string message = null;
            try
            {
                while (true)
                {
                    // Get reshaper cutter.
                    var cutter = await GetPolylineAsync();
                    
                    // Use selected features if any; otherwise, query features using spatial filter on current map extent.
                    var selectedFeatures = await GetSelectedFeaturesAsync(table, layer.SelectedFeatureIDs);

                    // Perform cut on selected features.
                    if (selectedFeatures != null && selectedFeatures.Any())
                    {
                        
                        foreach (var f in selectedFeatures)
                        {
                            var cutResult = GeometryEngine.Cut(f.Geometry, cutter);
                            if (cutResult != null && cutResult.Any())
                            {
                                bool isFirstUpdated = false;
                                foreach (var geom in cutResult)
                                {
                                    var p = (Polygon)geom;
                                    if (!isFirstUpdated)
                                    {
                                        f.Geometry = p;
                                        await table.UpdateAsync(f);
                                        isFirstUpdated = true;
                                    }
                                    else
                                    {
                                        var n = new GeodatabaseFeature(table.Schema) { Geometry = p };                                        
                                        foreach (var item in f.Attributes)
                                            n.Attributes[item.Key] = item.Value;
                                        var id = await table.AddAsync(n);
                                        layer.SelectFeatures(new long[] { id });
                                    }
                                    SaveButton.IsEnabled = table.HasEdits;
                                }
                            }
                        }
                    }
                    var isContinuous = IsContinuous.IsChecked.HasValue && IsContinuous.IsChecked.Value;
                    if (!isContinuous)
                        break;
                }
            }
            catch (TaskCanceledException tcex)
            {
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();
        }

        private async void ReshapeButton_Click(object sender, RoutedEventArgs e)
        {
            if (MyMapView == null || MyMapView.Map == null || MyMapView.Map.Layers == null)
                return;
            var layer = MyMapView.Map.Layers["ThreatAreas"] as FeatureLayer;
            if (layer == null)
                return;
            var table = layer.FeatureTable as ServiceFeatureTable;
            if (table == null)
                return;
            if (MyMapView.Editor == null)
                return;
            string message = null;
            try
            {
                while (true)
                {
                    // Get polyline reshaper.
                    var reshaper = await GetPolylineAsync();
                    
                    // Use selected features if any; otherwise, query features using spatial filter on current map extent.
                    var selectedFeatures = await GetSelectedFeaturesAsync(table, layer.SelectedFeatureIDs);

                    if (selectedFeatures != null && selectedFeatures.Any())
                    {
                        foreach (var f in selectedFeatures)
                        {
                            var reshapeResult = GeometryEngine.Reshape(f.Geometry, reshaper);
                            if (reshapeResult is Polygon)
                            {
                                var polygon = (Polygon)reshapeResult;
                                f.Geometry = polygon;
                                await table.UpdateAsync(f);
                                SaveButton.IsEnabled = table.HasEdits;
                            }
                        }
                    }
                    var isContinuous = IsContinuous.IsChecked.HasValue && IsContinuous.IsChecked.Value;
                    if (!isContinuous)
                        break;
                }
            }
            catch (TaskCanceledException tcex)
            {
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();

        }

        private async void UnionButton_Click(object sender, RoutedEventArgs e)
        {
            var layer = GetFeatureLayer();
            if (layer == null)
                return;
            var table = GetFeatureTable(layer);
            if (table == null)
                return;
            string message = null;
            try
            {
                while (true)
                {
                    // Use selected features if any; otherwise, query features using spatial filter on current map extent.
                    var selectedFeatures = await GetSelectedFeaturesAsync(table, layer.SelectedFeatureIDs);

                    if (selectedFeatures != null && selectedFeatures.Any())
                    {
                        var unionResult = GeometryEngine.Union(from f in selectedFeatures select f.Geometry);
                        var firstID = layer.SelectedFeatureIDs.FirstOrDefault();
                        var firstFeature = (from f in selectedFeatures
                                            where Convert.ToInt64(f.Attributes[table.ObjectIDField], CultureInfo.InvariantCulture) == firstID
                                            select f).FirstOrDefault();
                        if (firstFeature != null)
                        {
                            firstFeature.Geometry = unionResult;
                            await table.UpdateAsync(firstFeature);
                            var otherFeatureIDs = from i in layer.SelectedFeatureIDs
                                                  where i != firstID
                                                  select i;
                            await table.DeleteAsync(otherFeatureIDs);
                            SaveButton.IsEnabled = table.HasEdits;
                        }
                    }
                    var isContinuous = IsContinuous.IsChecked.HasValue && IsContinuous.IsChecked.Value;
                    if (!isContinuous)
                        break;
                }
            }
            catch (TaskCanceledException tcex)
            {
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();

        }
               
        private void IsAutoSelect_Click(object sender, RoutedEventArgs e)
        {
            var layer = GetFeatureLayer();
            if (layer == null)
                return;
            var count = layer.SelectedFeatureIDs.Count();
            if (count == 0)
            {
                var isAutoSelect = IsAutoSelect.IsChecked.HasValue && IsAutoSelect.IsChecked.Value;
                if (isAutoSelect)
                    CutButton.IsEnabled = true;
            }
            else
            {
                CutButton.IsEnabled = count > 0;
                UnionButton.IsEnabled = count > 1;
            }
        }

        private static string GetResultMessage(IEnumerable<FeatureEditResultItem> editResults, EditType editType)
        {
            var sb = new StringBuilder();
            var operation = editType == EditType.Add ? "adds" :
                (editType == EditType.Update ? "updates" : "deletes");
            if (editResults.Any(r => r.Error != null))
            {
                sb.AppendLine(string.Format("Failed {0} : [{1}]", operation, string.Join(", ", from r in editResults
                                                                                               where r.Error != null
                                                                                               select string.Format("{0} : {1}", r.ObjectID, r.Error != null ? r.Error.Message : string.Empty))));
            }
            if (editResults.Any(r => r.Error == null))
            {
                sb.AppendLine(string.Format("Successful {0} : [{1}]", operation, string.Join(", ", from r in editResults
                                                                                                   where r.Error == null
                                                                                                   select r.ObjectID)));
            }
            return sb.ToString();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var table = GetFeatureTable();
            if (table == null)
                return;
            string message = null;
            try
            {
                // Submits the feature edits to server.
                var saveResult = await table.ApplyEditsAsync();
                if (saveResult != null)
                {
                    var sb = new StringBuilder();
                    var editMessage = GetResultMessage(saveResult.AddResults, EditType.Add);
                    if (!string.IsNullOrWhiteSpace(editMessage))
                        sb.AppendLine(editMessage);
                    editMessage = GetResultMessage(saveResult.UpdateResults, EditType.Update);
                    if (!string.IsNullOrWhiteSpace(editMessage))
                        sb.AppendLine(editMessage); 
                    editMessage = GetResultMessage(saveResult.DeleteResults, EditType.Delete);
                    if (!string.IsNullOrWhiteSpace(editMessage))
                        sb.AppendLine(editMessage);                   
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

        private Task<bool> CancelEditsAsync(ServiceFeatureTable table)
        {
            if (table == null)
                return Task.FromResult(false);
            var tcs = new TaskCompletionSource<bool>();
            EventHandler<UpdateCompletedEventArgs> updatedCompletedHandler = null;
            updatedCompletedHandler = (s, e) =>
            {
                table.UpdateCompleted -= updatedCompletedHandler;
                if (e.Error != null)
                    tcs.TrySetException(e.Error);
                else
                    tcs.TrySetResult(true);
            };
            table.UpdateCompleted += updatedCompletedHandler;
            table.RefreshFeatures(false);
            return tcs.Task;
        }

        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Cancel edits on SketchLayer.
            if (MyMapView.Editor != null)
            {
                if (MyMapView.Editor.Cancel.CanExecute(null))
                    MyMapView.Editor.Cancel.Execute(null);
            }

            var table = GetFeatureTable();
            if (table == null || !table.HasEdits)
                return;
            string message = null;
            try
            {
                // Cancels the local edits by refreshing features with preserveEdits=false 
                // and awaits for UpdatedCompleted before checking HasEdits.
                var cancelResult = await CancelEditsAsync(table);
                if (cancelResult)
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
