using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Demonstrates how to add and selectively import, update or delete features to local geodatabase while the rest of the features are rendered by dynamic layer.
    /// </summary>
    /// <title>Selection Only</title>
    /// <category>Editing</category>
    public partial class SelectionOnly : UserControl
    {
        private FieldInfo objectIdField;
        private FieldInfo typeIdField;
        private ServiceFeatureTable table;

        public SelectionOnly()
        {
            InitializeComponent();
        }
        
        private ArcGISDynamicMapServiceLayer GetArcGISDynamicMapServiceLayer()
        {
            if (MyMapView.Map == null || MyMapView.Map.Layers == null)
                return null;
            var layer = MyMapView.Map.Layers["WildFireDynamic"] as ArcGISDynamicMapServiceLayer;
            return layer;
        }

        private GraphicsLayer GetGraphicsLayer()
        {
            if (MyMapView.Map == null || MyMapView.Map.Layers == null)
                return null;
            var layer = MyMapView.Map.Layers["WildFirePolygons"] as GraphicsLayer;
            return layer;
        }

        private async void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
        {
            if (e.LoadError != null)
                return;
            if (e.Layer is ArcGISDynamicMapServiceLayer)
            {
                var dynamicLayer = (ArcGISDynamicMapServiceLayer)e.Layer;
                var layerID = dynamicLayer.VisibleLayers.FirstOrDefault();
                // Get service metadata for visible layer.
                var details = await dynamicLayer.GetDetailsAsync(layerID);
                if (details != null)
                {
                    // Prepare GraphicsLayer by letting it share the same renderer.
                    var layer = GetGraphicsLayer();
                    string rendererField = null;
                    if (layer != null)
                    {
                        if (details.DrawingInfo != null)
                        {
                            layer.Renderer = details.DrawingInfo.Renderer;
                            if (layer.Renderer is UniqueValueRenderer)
                            {
                                var uvr = (UniqueValueRenderer)layer.Renderer;
                                rendererField = uvr.Fields.FirstOrDefault();
                            }
                        }
                    }
                    // Get field information for object and type ID's
                    if (details.Fields != null)
                    {
                        objectIdField = details.Fields.FirstOrDefault(f => f.Type == FieldType.Oid);
                        if (!string.IsNullOrWhiteSpace(rendererField))
                            typeIdField = details.Fields.FirstOrDefault(f => f.Name == rendererField);
                    }
                    if (typeIdField != null && AddButton != null)
                        AddButton.IsEnabled = true;
                }
            }
        }

        private void ClearLocalGraphics()
        {
            var layer = GetGraphicsLayer();
            if (layer != null && layer.Graphics != null)
                layer.Graphics.Clear();
            var dynamicLayer = GetArcGISDynamicMapServiceLayer();
            if (dynamicLayer != null && dynamicLayer.LayerDefinitions != null)
            {
                dynamicLayer.LayerDefinitions.Clear();
                dynamicLayer.Invalidate();
            }
            EditButton.DataContext = null;
            EditButton.IsEnabled = false;
        }
        
        private long GetFeatureID(Graphic graphic)
        {
            if (graphic == null || objectIdField == null ||
                string.IsNullOrWhiteSpace(objectIdField.Name) ||
                !graphic.Attributes.ContainsKey(objectIdField.Name))
                return 0;
            var featureID = Convert.ToInt64(graphic.Attributes[objectIdField.Name], CultureInfo.InvariantCulture);
            return featureID;
        }

        private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            if (MyMapView.Editor.IsActive)
                return;
            var dynamicLayer = GetArcGISDynamicMapServiceLayer();
            var layer = GetGraphicsLayer();
            if (dynamicLayer == null || layer == null)
                return;
            string message = null;
            try
            {
                // Clear local graphics and let server render all the features.
                ClearLocalGraphics();
                // Perform an identify to pull graphic into local layer.
                var mapPoint = MyMapView.ScreenToLocation(e.Position);
                var parameters = new IdentifyParameters(mapPoint, MyMapView.Extent, 2, (int)MyMapView.ActualHeight, (int)MyMapView.ActualWidth);
                var task = new IdentifyTask(new Uri(dynamicLayer.ServiceUri));
                var result = await task.ExecuteAsync(parameters);
                if (result != null && result.Results != null && result.Results.Count > 0)
                {
                    var item = result.Results.FirstOrDefault();
                    var graphic = item.Feature as Graphic;
                    if (graphic != null)
                    {
                        // Identify result use alias so add an entry that use field name.
                        if (objectIdField != null && graphic.Attributes.ContainsKey(objectIdField.Alias))
                            graphic.Attributes[objectIdField.Name] = graphic.Attributes[objectIdField.Alias];
                        if (typeIdField != null && graphic.Attributes.ContainsKey(typeIdField.Alias))
                            graphic.Attributes[typeIdField.Name] = graphic.Attributes[typeIdField.Alias];
                        graphic.IsSelected = true;
                        // Add selected feature to local layer.
                        layer.Graphics.Add(graphic);
                        var featureID = GetFeatureID(graphic);
                        if (featureID > 0 && objectIdField != null)
                        {
                            if (dynamicLayer.LayerDefinitions == null)
                                dynamicLayer.LayerDefinitions = new ObservableCollection<LayerDefinition>();
                            // Hide selected feature from dynamic layer.
                            dynamicLayer.LayerDefinitions.Add(new LayerDefinition()
                            {
                                LayerID = item.LayerID,
                                Definition = string.Format("{0} <> {1}", objectIdField.Name, featureID)
                            });
                            dynamicLayer.Invalidate();
                        }
                    }
                }
                EditButton.DataContext = layer.Graphics.FirstOrDefault();
                EditButton.IsEnabled = layer.Graphics.Any();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(message);
        }

        private async Task<ServiceFeatureTable> GetFeatureTableAsync()
        {
            if (table == null)
            {
                var layer = GetArcGISDynamicMapServiceLayer();
                var layerID = layer.VisibleLayers.FirstOrDefault();
                var featureServiceUri = layer.ServiceUri.Replace("MapServer", "FeatureServer");
                featureServiceUri = string.Format("{0}/{1}", featureServiceUri, layerID);
                // Create table with the minimum required fields.
                // objectId to identify feature, typeId to render the feature.
                table = await ServiceFeatureTable.OpenAsync(new Uri(featureServiceUri), null, MyMapView.SpatialReference);
                table.OutFields = new OutFields();
                if(objectIdField != null)
                    table.OutFields.Add(objectIdField.Name);
                if (typeIdField != null)
                    table.OutFields.Add(typeIdField.Name);
            }
            return table;
        }

        private async Task AddFeatureAsync(Graphic graphic)
        {
            var table = await GetFeatureTableAsync();
            if (table != null)
            {
                var feature = new GeodatabaseFeature(table.Schema)
                {
                    Geometry = graphic.Geometry
                };
                foreach (var item in graphic.Attributes)
                    feature.Attributes[item.Key] = item.Value;
                await table.AddAsync(feature);
                if (table.HasEdits)
                    await table.ApplyEditsAsync();
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var layer = GetGraphicsLayer();
            if (layer == null || typeIdField == null)
                return;
            var typeId = Convert.ToInt16((sender as Button).Tag, CultureInfo.InvariantCulture);     
            string message = null;
            try
            {
                ClearLocalGraphics();
                // Retrieves geometry from the Editor.
                var geometry = await MyMapView.Editor.RequestShapeAsync(DrawShape.Freehand);
                var polyline = (Polyline)geometry;
                var polygon = new Polygon(polyline.Parts, polyline.SpatialReference);
                polygon = GeometryEngine.Simplify(polygon) as Polygon;
                // Create graphic with typeId field.
                var graphic = new Graphic(polygon);
                graphic.Attributes[typeIdField.Name] = typeId;
                graphic.IsSelected = true;
                layer.Graphics.Add(graphic);
                EditButton.DataContext = layer.Graphics.FirstOrDefault();
                EditButton.IsEnabled = true;
                var addPrompt = MessageBox.Show("Do you want to add new feature to your database?", "Add feature", MessageBoxButton.OKCancel);
                if (addPrompt == MessageBoxResult.OK)
                    await AddFeatureAsync(graphic);                
                ClearLocalGraphics();
            }
            catch (TaskCanceledException tcex)
            {

            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(message);
        }

        private async Task UpdateFeatureGeometryAsync(Graphic graphic)
        {
            var featureID = GetFeatureID(graphic);
            if (featureID == 0)
                return;
            var table = await GetFeatureTableAsync();
            if (table != null)
            {
                var feature = await table.QueryAsync(featureID);
                if (feature != null)
                {
                    feature.Geometry = graphic.Geometry;
                    await table.UpdateAsync(feature);
                }
                if (table.HasEdits)
                    await table.ApplyEditsAsync();
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var graphic = (sender as Button).DataContext as Graphic;
            if (graphic == null)
                return;
            string message = null;
            try
            {
                graphic.IsVisible = false;
                var geometry = await MyMapView.Editor.EditGeometryAsync(graphic.Geometry);
                geometry = GeometryEngine.Simplify(geometry);
                graphic.Geometry = geometry;
                graphic.IsVisible = true;
                var editPrompt = MessageBox.Show("Do you want to apply the changes to your database?", "Apply edits", MessageBoxButton.OKCancel);
                if (editPrompt == MessageBoxResult.OK)
                    await UpdateFeatureGeometryAsync(graphic);
                ClearLocalGraphics();
            }
            catch (TaskCanceledException tcex)
            {
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(message);

        }
        
        private async Task DeleteFeatureAsync(Graphic graphic)
        {
            var featureID = GetFeatureID(graphic);
            if (featureID == 0)
                return;
            var table = await GetFeatureTableAsync();
            if (table != null)
            {
                var feature = await table.QueryAsync(featureID);
                await table.DeleteAsync(featureID);
                if (table.HasEdits)
                    await table.ApplyEditsAsync();
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var graphic = (sender as Button).DataContext as Graphic;
            if (graphic == null || table == null)
                return;
            string message = null;
            try
            {
                var deletePrompt = MessageBox.Show("Are you sure you want to delete feature from the database?", "Delete feature", MessageBoxButton.OKCancel);
                if (deletePrompt == MessageBoxResult.OK)
                    await DeleteFeatureAsync(graphic);
                ClearLocalGraphics();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(message);
        }
    }
}