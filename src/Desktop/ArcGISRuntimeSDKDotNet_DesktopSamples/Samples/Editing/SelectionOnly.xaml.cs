using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Edit;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Demonstrates how selection is used so only features of interests can be imported and edited in the local geodatabase.
    /// </summary>
    /// <title>Selection Only</title>
    /// <category>Editing</category>
    public partial class SelectionOnly : UserControl
    {
        public SelectionOnly()
        {
            InitializeComponent();
        }

        private Grid formGrid;        
        private ServiceFeatureTable table;

        private GraphicsLayer GetGraphicsLayer()
        {
            if (MyMapView.Map == null || MyMapView.Map.Layers == null)
                return null;
            var layer = MyMapView.Map.Layers["WildFirePolygons"] as GraphicsLayer;
            return layer;
        }

        private ArcGISDynamicMapServiceLayer GetArcGISDynamicMapServiceLayer()
        {
            if (MyMapView.Map == null || MyMapView.Map.Layers == null)
                return null;
            var layer = MyMapView.Map.Layers["WildFireDynamic"] as ArcGISDynamicMapServiceLayer;
            return layer;
        }

        private long GetFeatureID(GeodatabaseFeature feature)
        {
            if (feature == null || table == null || string.IsNullOrWhiteSpace(table.ObjectIDField) || !feature.Attributes.ContainsKey(table.ObjectIDField))
                return 0;
            var featureID = Convert.ToInt64(feature.Attributes[table.ObjectIDField], CultureInfo.InvariantCulture);
            return featureID;
        }

        private Graphic GetGraphic(GeodatabaseFeature feature, long featureID = 0)
        {
            var layer = GetGraphicsLayer();
            if (feature == null || layer == null || layer.Graphics == null || table == null || string.IsNullOrWhiteSpace(table.ObjectIDField))
                return null;
            if (featureID == 0)
                featureID = GetFeatureID(feature);
            var graphic = layer.Graphics.FirstOrDefault(g=>Convert.ToInt64(g.Attributes[table.ObjectIDField] , CultureInfo.InvariantCulture) == featureID);
            return graphic;
        }

        private object ConvertToDataType(Type dataType, object value)
        {
            if (value != null && value.GetType() != dataType)
            { 
                if (dataType == typeof(Int16))
                    return Convert.ToInt16(value, CultureInfo.InvariantCulture);
            }
            return value;
        }

        private async void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
        {
            if (e.LoadError != null)
                return;
            if (e.Layer is ArcGISDynamicMapServiceLayer)
            {
                var layer = (ArcGISDynamicMapServiceLayer)e.Layer;                
                var layerID = layer.VisibleLayers.FirstOrDefault();
                var featureServiceUri = layer.ServiceUri.Replace("MapServer", "FeatureServer");
                featureServiceUri = string.Format("{0}/{1}", featureServiceUri, layerID);
                table = await ServiceFeatureTable.OpenAsync(new Uri(featureServiceUri), null, MyMapView.SpatialReference);
                table.OutFields = OutFields.All;
                if (!table.IsInitialized)
                    await table.InitializeAsync();
                if(table.ServiceInfo == null)
                 return;
                if (table.ServiceInfo.DrawingInfo != null)
                {                    
                    var graphicsLayer = GetGraphicsLayer();
                    graphicsLayer.Renderer = table.ServiceInfo.DrawingInfo.Renderer;
                }
                // Builds the Attribute Editor based on FieldInfo (i.e. Editable, Domain, Length, DataType)
                // For better validation and customization support use FeatureDataForm from the Toolkit.
                if (table.ServiceInfo.Fields != null)
                {
                    AddButton.IsEnabled = true;
                    var itemtTemplate = this.Resources["MyItemTemplate"] as DataTemplate;
                    formGrid = new Grid() { Margin = new Thickness(2d) };
                    formGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    formGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    var fieldCount = table.ServiceInfo.Fields.Count + 1; // Fields + Apply/Delete/Edit/Close button
                    for (int i = 0; i < fieldCount; i++)
                        formGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                    var hasFeatureTypes = !string.IsNullOrWhiteSpace(table.ServiceInfo.TypeIdField) && table.ServiceInfo.Types != null && table.ServiceInfo.Types.Any();
                    int row = 0;
                    foreach (var field in table.ServiceInfo.Fields)
                    {
                        var label = new TextBlock() { Text = field.Alias ?? field.Name, Margin = new Thickness(2d) };
                        label.SetValue(Grid.RowProperty, row);
                        formGrid.Children.Add(label);
                        FrameworkElement value = null;
                        // This binding will be resolved once the DataContext for formGrid is set to feature.
                        var binding = new Binding(string.Format("Attributes[{0}]", field.Name));
                        if (field.IsEditable)
                        {
                            binding.Mode = BindingMode.TwoWay;
                            var keyValueConverter = this.Resources["KeyValueConverter"] as KeyValueConverter;
                            if (hasFeatureTypes && table.ServiceInfo.TypeIdField == field.Name)
                            {
                                value = new ComboBox() { ItemTemplate = itemtTemplate, Margin = new Thickness(2d) };
                                ((ComboBox)value).ItemsSource = from t in table.ServiceInfo.Types                                                                   
                                                                select new KeyValuePair<object, string>(ConvertToDataType(field.DataType, t.ID), t.Name);
                                binding.Converter = keyValueConverter;
                                binding.ConverterParameter = ((ComboBox)value).ItemsSource;
                                ((ComboBox)value).SetBinding(ComboBox.SelectedItemProperty, binding);
                            }
                            else if (field.Domain != null)
                            {
                                value = new ComboBox() { ItemTemplate = itemtTemplate, Margin = new Thickness(2d) };
                                if (field.Domain is CodedValueDomain)
                                {
                                    ((ComboBox)value).ItemsSource = ((CodedValueDomain)field.Domain).CodedValues;
                                    binding.Converter = keyValueConverter;
                                    binding.ConverterParameter = ((ComboBox)value).ItemsSource;
                                }
                                else if (field.Domain is RangeDomain<IComparable>)
                                {
                                    var rangeDomain = (RangeDomain<IComparable>)field.Domain;
                                    ((ComboBox)value).ItemsSource = new IComparable[] { rangeDomain.MinValue, rangeDomain.MaxValue };
                                }
                                ((ComboBox)value).SetBinding(ComboBox.SelectedItemProperty, binding);
                            }
                            else
                            {
                                value = new TextBox() { Margin = new Thickness(2d) };
                                // Fields of DataType than string will need a converter.
                                ((TextBox)value).SetBinding(TextBox.TextProperty, binding);
                                if (field.Length.HasValue)
                                    ((TextBox)value).MaxLength = field.Length.Value;
                            }
                        }
                        else
                        {
                            value = new TextBlock() { Margin = new Thickness(2d) };
                            ((TextBlock)value).SetBinding(TextBlock.TextProperty, binding);
                        }
                        value.SetValue(Grid.ColumnProperty, 1);
                        value.SetValue(Grid.RowProperty, row);
                        formGrid.Children.Add(value);
                        row++;
                    }
                    var buttonGrid = new Grid() { Name = "ButtonGrid", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(2d) };
                    buttonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    buttonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    buttonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    buttonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    buttonGrid.SetValue(Grid.ColumnSpanProperty, 2);
                    buttonGrid.SetValue(Grid.RowProperty, row);
                    var applyButton = new Button() { Name = "ApplyButton", Content = "Apply", Margin = new Thickness(2d) };
                    applyButton.Click += ApplyButton_Click;
                    buttonGrid.Children.Add(applyButton);
                    var editButton = new Button() { Name = "EditButton", Content = "Edit Geometry", Margin = new Thickness(2d) };
                    editButton.SetValue(Grid.ColumnProperty, 1);
                    editButton.Click += EditButton_Click;
                    buttonGrid.Children.Add(editButton);
                    var deleteButton = new Button() { Name = "DeleteButton", Content = "Delete Feature", Margin = new Thickness(2d) };
                    deleteButton.SetValue(Grid.ColumnProperty, 2);
                    deleteButton.Click += DeleteButton_Click;
                    buttonGrid.Children.Add(deleteButton);
                    var closeButton = new Button() { Content = "Close", Margin = new Thickness(2d) };
                    closeButton.SetValue(Grid.ColumnProperty, 3);
                    closeButton.Click += CloseButton_Click;
                    buttonGrid.Children.Add(closeButton);
                    formGrid.Children.Add(buttonGrid);
                }
            }
        }

        private void UpdateGraphicAttributes(GeodatabaseFeature feature)
        {
            var graphic = GetGraphic(feature);
            if (graphic == null)
                return;
            foreach (var item in feature.Attributes)
                graphic.Attributes[item.Key] = item.Value;
        }

        private async void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            var feature = (sender as Button).DataContext as GeodatabaseFeature;
            if (feature == null || table == null)
                return;
            string message = null;
            try
            {
                // Updates the feature with its Attributes already modified by the two-way binding.
                await table.UpdateAsync(feature);
                // Update also the graphic attributes as it may affect symbology since renderer is used.
                UpdateGraphicAttributes(feature);
                SaveButton.IsEnabled = table.HasEdits;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(message);
        }
        
        private void UpdateGraphicGeometry(GeodatabaseFeature feature, Graphic localFeature)
        {
            var graphic = localFeature ?? GetGraphic(feature);
            if (graphic == null || feature == null)
                return;
            graphic.Geometry = feature.Geometry;
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var feature = (sender as Button).DataContext as GeodatabaseFeature;
            Graphic graphic = GetGraphic(feature);                    
            if (feature == null || table == null || string.IsNullOrWhiteSpace(table.ObjectIDField) || graphic == null)
                return;            
            string message = null;
            try
            {
                var editPrompt = MessageBox.Show("Drag any of the edit tool to update the geometry.", "Edit feature geometry", MessageBoxButton.OKCancel);
                if (editPrompt == MessageBoxResult.OK)
                {
                    CloseDataForm();
                    graphic.IsVisible = false;
                    // Updates the geometry of the feature.
                    var geometry = await MyMapView.Editor.EditGeometryAsync(feature.Geometry);
                    geometry = GeometryEngine.Simplify(geometry);
                    feature.Geometry = geometry;
                    await table.UpdateAsync(feature);
                    UpdateGraphicGeometry(feature, graphic);
                    graphic.IsVisible = true;
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
                MessageBox.Show(message);

        }
        
        private void DeleteFromLayers(GeodatabaseFeature feature)
        {
            var layer = GetGraphicsLayer();
            var featureID = GetFeatureID(feature);
            var graphic = GetGraphic(feature, featureID);
            if (layer == null || layer.Graphics == null || graphic == null)
                return;
            layer.Graphics.Remove(graphic);
            if (featureID > 0)
                HideFromDynamicLayer(table.ServiceInfo.ID, featureID);
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var feature = (sender as Button).DataContext as GeodatabaseFeature;
            if (feature == null || table == null)
                return;
            string message = null;
            try
            {
                var deletePrompt = MessageBox.Show("Are you sure you want to delete this feature from the table?", "Delete feature", MessageBoxButton.OKCancel);
                if (deletePrompt == MessageBoxResult.OK)
                {
                    CloseDataForm();
                    // Deletes the feature from the table.
                    await table.DeleteAsync(feature);
                    DeleteFromLayers(feature);
                    SaveButton.IsEnabled = table.HasEdits;
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(message);
        }
        
        private void CloseDataForm()
        {
            if (formGrid != null && formGrid.Parent is Window)
            {
                var dataForm = (Window)formGrid.Parent;
                dataForm.Close();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseDataForm();
        }
                
        private void AddToGraphicsLayer(Feature feature)
        {
            var layer = GetGraphicsLayer();
            if (layer == null || feature == null)
                return;
            var graphic = new Graphic() { Geometry = feature.Geometry };
            foreach (var item in feature.Attributes)
            {
                var key = item.Key;
                // IdentifyResultItem.Feature will return Alias, use ServiceInfo.Fields to get Name.
                if (feature is Graphic) 
                {
                    var field = table.ServiceInfo.Fields.FirstOrDefault(f => f.Alias == item.Key);
                    if (field != null)
                        key = field.Name;
                }
                graphic.Attributes[key] = item.Value;
            }
            graphic.IsSelected = true;
            layer.Graphics.Add(graphic);
        }

        private void HideFromDynamicLayer(int layerID, long featureID)
        {
            var layer = GetArcGISDynamicMapServiceLayer();
            if (layer == null || table == null || string.IsNullOrWhiteSpace(table.ObjectIDField))
                return;
            var definitionExpression = string.Format("{0} <> {1}", table.ObjectIDField, featureID);
            if (layer.LayerDefinitions == null)
                layer.LayerDefinitions = new ObservableCollection<LayerDefinition>();
            
                var layerDefinition = layer.LayerDefinitions.FirstOrDefault(d => d.LayerID == layerID);
                if(layerDefinition != null && !string.IsNullOrWhiteSpace(layerDefinition.Definition))
                    layerDefinition.Definition = string.Format("({0}) AND {1}", layerDefinition.Definition, definitionExpression);
                else           
                    layer.LayerDefinitions.Add(new LayerDefinition() { LayerID = layerID,  Definition = definitionExpression});
        }

        private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            if (MyMapView.Editor.IsActive)
                return;
            var layer = GetGraphicsLayer();
            var dynamicLayer = GetArcGISDynamicMapServiceLayer();
            if (layer == null || dynamicLayer == null || table == null || string.IsNullOrWhiteSpace(table.ObjectIDField))
                return;
            string message = null;
            try
            {
                layer.ClearSelection();                
                var graphic = await layer.HitTestAsync(MyMapView, e.Position);
                long featureID = 0;
                if (graphic != null && graphic.Attributes.ContainsKey(table.ObjectIDField))
                {
                    featureID = Convert.ToInt64(graphic.Attributes[table.ObjectIDField], CultureInfo.InvariantCulture);
                    graphic.IsSelected = true;
                }
                else
                {
                    // Identify the feature.
                    var mapPoint = MyMapView.ScreenToLocation(e.Position);
                    var parameters = new IdentifyParameters(mapPoint, MyMapView.Extent, 2, (int)MyMapView.ActualHeight, (int)MyMapView.ActualWidth);
                    var task = new IdentifyTask(new Uri(dynamicLayer.ServiceUri));
                    var result = await task.ExecuteAsync(parameters);
                    if (result != null && result.Results != null && result.Results.Count > 0)
                    {
                        var item = result.Results.FirstOrDefault();
                        var objectIdAlias = table.ServiceInfo.Fields.FirstOrDefault(f => f.Name == table.ObjectIDField).Alias;
                        featureID = Convert.ToInt64(item.Feature.Attributes[objectIdAlias], CultureInfo.InvariantCulture);
                        // Add to GraphicsLayer
                        AddToGraphicsLayer(item.Feature);
                        // Hide from DynamicLayer
                        HideFromDynamicLayer(item.LayerID, featureID);
                    }
                }
                // Imports to local geodatabase table if forceLocal = false.
                var forceLocal = featureID < 0;
                var features = await table.QueryAsync(new long[] { featureID }, forceLocal);
                if (features != null)
                {
                    var feature = features.FirstOrDefault();
                    if (feature != null && formGrid != null)
                    {
                        var dataForm = new Window() { Content = formGrid, Height = 350, Width = 500, Title = "Attribute Editor" };
                        dataForm.DataContext = feature;
                        dataForm.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(message);
        }
        
        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (table == null)
                return;
            var typeId = Convert.ToInt32((sender as Button).Tag, CultureInfo.InvariantCulture);
            string message = null;
            try
            {
                // Retrieves geometry from the Editor.
                var geometry = await MyMapView.Editor.RequestShapeAsync(DrawShape.Freehand);
                var polyline = (Polyline)geometry;
                var polygon = new Polygon(polyline.Parts, polyline.SpatialReference);
              
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
                // Adds to GraphicsLayer.
                AddToGraphicsLayer(feature);
                              
                if (formGrid != null)
                {
                    var dataForm = new Window() { Content = formGrid, Height = 350, Width = 500, Title = "Attribute Editor" };
                    dataForm.DataContext = feature;
                    dataForm.ShowDialog();
                }

                SaveButton.IsEnabled = table.HasEdits;
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

        private static string GetResultMessage(IEnumerable<FeatureEditResultItem> editResults)
        {
            var sb = new StringBuilder();
            var operation = "updates";
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
        
        private void RefreshLayers()
        {
            // Clears features from local layer.
            var layer = GetGraphicsLayer();
            if (layer != null && layer.Graphics != null)
                layer.Graphics.Clear();
            // Allows features to be rendered by server.
            var dynamicLayer = GetArcGISDynamicMapServiceLayer();
            if (dynamicLayer != null && dynamicLayer.LayerDefinitions != null)
                dynamicLayer.LayerDefinitions.Clear();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
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
                    var editMessage = GetResultMessage(saveResult.UpdateResults);
                    if (!string.IsNullOrWhiteSpace(editMessage))
                        sb.AppendLine(editMessage);
                    message = sb.ToString();
                }
                RefreshLayers();
                SaveButton.IsEnabled = table.HasEdits;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(message);
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
            if (table == null || !table.HasEdits)
                return;
            // Cancels the local edits by refreshing features with preserveEdits=false 
            // and awaits for UpdatedCompleted before performing query and updating the layers.
            var cancelResult = await CancelEditsAsync(table);
            if (cancelResult)
            {                
                if (formGrid != null && formGrid.Parent is Window)
                {
                    var feature = formGrid.DataContext as GeodatabaseFeature;
                    var featureID = Convert.ToInt64(feature.Attributes[table.ObjectIDField], CultureInfo.InvariantCulture);
                    // To reflect the changes in fields, query feature and update form data context.
                    feature = (GeodatabaseFeature)await table.QueryAsync(featureID);
                    formGrid.DataContext = feature;
                }
                RefreshLayers();
                SaveButton.IsEnabled = table.HasEdits;
            }
        }
    }
}