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
    /// Demonstrates how to add and selectively import, update geometry or delete features using dynamic layer to render the features.
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
            return MyMapView.Map.Layers["WildFireDynamic"] as ArcGISDynamicMapServiceLayer;
        }

        private GraphicsLayer GetGraphicsLayer()
        {
            if (MyMapView.Map == null || MyMapView.Map.Layers == null)
                return null;
            return MyMapView.Map.Layers["WildFirePolygons"] as GraphicsLayer;
        }

        private async void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
        {
            if (e.LoadError != null || !(e.Layer is ArcGISDynamicMapServiceLayer))
                return;
            var dynamicLayer = (ArcGISDynamicMapServiceLayer)e.Layer;
            var layerID = dynamicLayer.VisibleLayers.FirstOrDefault();
            // Get service metadata for visible layer.
            var details = await dynamicLayer.GetDetailsAsync(layerID);
            // Prepare GraphicsLayer by letting it share the same renderer.
            var layer = GetGraphicsLayer();
            if (details == null || details.DrawingInfo == null || details.Fields == null)
                return;
            string rendererField = null;
            layer.Renderer = details.DrawingInfo.Renderer;
            if (layer.Renderer is UniqueValueRenderer)
            {
                var uvr = (UniqueValueRenderer)layer.Renderer;
                rendererField = uvr.Fields.FirstOrDefault();
            }
            // Get field information for object and type ID's
            objectIdField = details.Fields.FirstOrDefault(f => f.Type == FieldType.Oid);
            if (!string.IsNullOrWhiteSpace(rendererField))
                typeIdField = details.Fields.FirstOrDefault(f => f.Name == rendererField);
            if (typeIdField != null)
                AddButton.IsEnabled = true;
        }

        private void ClearLocalGraphics()
        {
            var layer = GetGraphicsLayer();
            if (layer != null && layer.Graphics != null)
                layer.Graphics.Clear();
            var dynamicLayer = GetArcGISDynamicMapServiceLayer();
            if (dynamicLayer != null)
            {
                if(dynamicLayer.LayerDefinitions != null)
                    dynamicLayer.LayerDefinitions.Clear();
                dynamicLayer.Invalidate();
            }
            EditButton.DataContext = null;
            EditButton.IsEnabled = false;
        }

        private long GetFeatureID(Graphic graphic)
        {
            if (graphic == null || objectIdField == null || 
                !graphic.Attributes.ContainsKey(objectIdField.Name))
                return 0;
            return Convert.ToInt64(graphic.Attributes[objectIdField.Name], CultureInfo.InvariantCulture);
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
                // Let all features be rendered through dynamic layer.
                ClearLocalGraphics();
                // Perform an identify to pull graphic into local layer.
                var mapPoint = MyMapView.ScreenToLocation(e.Position);
                var parameters = new IdentifyParameters(mapPoint, MyMapView.Extent, 2, (int)MyMapView.ActualHeight, (int)MyMapView.ActualWidth);
                var task = new IdentifyTask(new Uri(dynamicLayer.ServiceUri));
                var result = await task.ExecuteAsync(parameters);
                if (result == null || result.Results == null || result.Results.Count < 1)
                    return;
                var item = result.Results[0];
                var graphic = (Graphic)item.Feature;
                // Identify result use alias so add an entry that use field name.
                if (objectIdField != null && graphic.Attributes.ContainsKey(objectIdField.Alias))
                    graphic.Attributes[objectIdField.Name] =  graphic.Attributes[objectIdField.Alias];                
                if (typeIdField != null && graphic.Attributes.ContainsKey(typeIdField.Alias))
                    graphic.Attributes[typeIdField.Name] = graphic.Attributes[typeIdField.Alias];
                graphic.IsSelected = true;
                // Add selected feature to local layer.
                layer.Graphics.Add(graphic);
                var featureID = GetFeatureID(graphic);
                if (objectIdField == null || featureID == 0)
                    return;
                if (dynamicLayer.LayerDefinitions == null)
                    dynamicLayer.LayerDefinitions = new ObservableCollection<LayerDefinition>();
                // Hide selected feature from dynamic layer.
                dynamicLayer.LayerDefinitions.Add(new LayerDefinition()
                {
                    LayerID = item.LayerID,
                    Definition = string.Format("{0} <> {1}", objectIdField.Name, featureID)
                });
                dynamicLayer.Invalidate();
                EditButton.DataContext = graphic;
                EditButton.IsEnabled = true;
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
            if (table != null)
                return table;
            var layer = GetArcGISDynamicMapServiceLayer();
            if (layer == null || layer.VisibleLayers == null)
                return null;
            var layerID = layer.VisibleLayers.FirstOrDefault();
            var featureServiceUri = layer.ServiceUri.Replace("MapServer", "FeatureServer");
            featureServiceUri = string.Format("{0}/{1}", featureServiceUri, layerID);
            // Create relatedTable with the minimum required fields.
            // objectId to identify feature, typeId to render the feature.
            table = await ServiceFeatureTable.OpenAsync(new Uri(featureServiceUri), null, MyMapView.SpatialReference);
            table.OutFields = new OutFields();
            if (objectIdField != null)
                table.OutFields.Add(objectIdField.Name);
            if (typeIdField != null)
                table.OutFields.Add(typeIdField.Name);
            return table;
        }

        private async Task AddFeatureAsync(Graphic graphic)
        {
            var table = await GetFeatureTableAsync();
            if (graphic == null || table == null)
                return;
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

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var layer = GetGraphicsLayer();
            if (layer == null || typeIdField == null)
                return;
            var typeId = Convert.ToInt16(((Button)sender).Tag, CultureInfo.InvariantCulture);     
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
                EditButton.DataContext = graphic;
                EditButton.IsEnabled = true;
                var addPrompt = MessageBox.Show("Do you want to save new feature?", "Add feature", MessageBoxButton.OKCancel);
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
            var table = await GetFeatureTableAsync();
            if (graphic == null || table == null)
                return;
            var featureID = GetFeatureID(graphic);
            if (featureID == 0)
                return;
            var feature = await table.QueryAsync(featureID);
            if (feature == null)
                return;
            feature.Geometry = graphic.Geometry;
            await table.UpdateAsync(feature);
            if (table.HasEdits)
                await table.ApplyEditsAsync();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var graphic = (Graphic)((Button)sender).DataContext;
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
                var editPrompt = MessageBox.Show("Do you want to save the geometry change?", "Update feature", MessageBoxButton.OKCancel);
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
            var table = await GetFeatureTableAsync();
            if (graphic == null || table == null)
                return;
            var featureID = GetFeatureID(graphic);
            if (featureID == 0)
                return;
            var feature = await table.QueryAsync(featureID);
            if (feature == null)
                return;
            await table.DeleteAsync(feature);
            if (table.HasEdits)
                await table.ApplyEditsAsync();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (table == null)
                return;
            var graphic = (Graphic)((Button)sender).DataContext;            
            string message = null;
            try
            {
                var deletePrompt = MessageBox.Show("Are you sure you want to delete feature?", "Delete feature", MessageBoxButton.OKCancel);
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