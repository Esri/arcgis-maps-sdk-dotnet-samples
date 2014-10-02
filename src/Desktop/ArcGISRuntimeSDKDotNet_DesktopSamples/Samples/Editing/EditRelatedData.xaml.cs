using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Demonstrates how to query and update graphic attributes of the related relatedTable.
    /// </summary>
    /// <title>Edit Related Data</title>
    /// <category>Editing</category>
    public partial class EditRelatedData : UserControl
    {
        private Grid formGrid;
        private Esri.ArcGISRuntime.ArcGISServices.Relationship relationship;
        private ServiceFeatureTable relatedTable;
        private FieldInfo objectIdField;
        private FieldInfo keyField;

        public EditRelatedData()
        {
            InitializeComponent();
        }

        private ArcGISDynamicMapServiceLayer GetArcGISDynamicMapServiceLayer()
        {
            if (MyMapView.Map == null || MyMapView.Map.Layers == null)
                return null;
            return MyMapView.Map.Layers["ServiceRequestDynamicLayer"] as ArcGISDynamicMapServiceLayer;
        }

        private GraphicsLayer GetGraphicsLayer()
        {
            if (MyMapView.Map == null || MyMapView.Map.Layers == null)
                return null;
            return MyMapView.Map.Layers["ServiceRequestLayer"] as GraphicsLayer;
        }

        private IEnumerable<int> GetRangeValues(int min, int max)
        {
            for (var i = min; i <= max; i++)
                yield return i;
        }

        private void BuildAttributeEditor()
        {
            if (relatedTable == null || relatedTable.ServiceInfo == null || relatedTable.ServiceInfo.Fields == null)
                return;
            // Builds the Attribute Editor based on FieldInfo (i.e. Editable, Domain, Length, DataType)
            // For better validation and customization support,
            // use FeatureDataForm from the Toolkit: https://github.com/Esri/arcgis-toolkit-dotnet.              
            formGrid = new Grid() { Margin = new Thickness(2d) };
            formGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            formGrid.ColumnDefinitions.Add(new ColumnDefinition());
            var fieldCount = relatedTable.ServiceInfo.Fields.Count + 1; // Fields + Apply/Delete/Edit/Close button
            for (int i = 0; i < fieldCount; i++)
                formGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            int row = 0;
            var hasFeatureTypes = !string.IsNullOrWhiteSpace(relatedTable.ServiceInfo.TypeIdField) && relatedTable.ServiceInfo.Types != null && relatedTable.ServiceInfo.Types.Count > 1;
            foreach (var field in relatedTable.ServiceInfo.Fields)
            {

                var label = new TextBlock() { Text = field.Alias ?? field.Name, Margin = new Thickness(2d) };
                label.SetValue(Grid.RowProperty, row);
                formGrid.Children.Add(label);
                FrameworkElement value = null;
                // This binding will be resolved once the DataContext is set to a feature object.
                var binding = new Binding(string.Format("Attributes[{0}]", field.Name));
                if (field.IsEditable)
                {
                    binding.Mode = BindingMode.TwoWay;
                    // This service only contains RangeDomain.
                    // Depending on your service, you might consider handling item selection for CodedValueDomain or FeatureTypes.                   
                    if (field.Domain is RangeDomain<IComparable>)
                    {
                        value = new ComboBox() { Margin = new Thickness(2d) };
                        var rangeDomain = (RangeDomain<IComparable>)field.Domain;
                        // The field in this service is of Integer type.
                        ((ComboBox)value).ItemsSource = GetRangeValues((int)rangeDomain.MinValue, (int)rangeDomain.MaxValue);
                        ((ComboBox)value).SetBinding(ComboBox.SelectedItemProperty, binding);
                    }
                    else
                    {
                        value = new TextBox() { Margin = new Thickness(2d) };
                        // Fields of DataType than string will need a converter.
                        if (field.DataType == typeof(DateTime))
                            binding.Converter = this.Resources["StringToDateConverter"] as StringToDateConverter;
                        else if (field.DataType == typeof(short))
                            binding.Converter = this.Resources["StringToShortConverter"] as StringToShortConverter;
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
            var buttonGrid = new Grid() { HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(2d) };
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            buttonGrid.SetValue(Grid.ColumnSpanProperty, 2);
            buttonGrid.SetValue(Grid.RowProperty, row);
            var applyButton = new Button() { Content = "Apply", Margin = new Thickness(2d) };
            applyButton.Click += ApplyButton_Click;
            buttonGrid.Children.Add(applyButton); 
            var deleteButton = new Button() { Content = "Delete", Margin = new Thickness(2d) };
            deleteButton.SetValue(Grid.ColumnProperty, 1);
            deleteButton.Click += DeleteButton_Click;
            var closeButton = new Button() { Content = "Close", Margin = new Thickness(2d) };
            closeButton.SetValue(Grid.ColumnProperty, 2);
            closeButton.Click += CloseButton_Click;
            buttonGrid.Children.Add(closeButton);
            formGrid.Children.Add(buttonGrid);
        }

        private async void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
        {
            if (e.LoadError != null || !(e.Layer is ArcGISDynamicMapServiceLayer))
                return;
            // Creates the related relatedTable based on dynamic layer details.
            var dynamicLayer = (ArcGISDynamicMapServiceLayer)e.Layer;
            string message = null;
            try
            {
                var allDetails = await dynamicLayer.GetAllDetailsAsync();
                if (allDetails == null || allDetails.Layers == null || allDetails.Layers.Count == 0)
                    return;
                var relatedLayer = allDetails.Layers.FirstOrDefault();
                if (relatedLayer.Fields == null)
                    return;
                // To retrieve ID from IdentifyResult.
                objectIdField = relatedLayer.Fields.FirstOrDefault(f => f.Type == FieldType.Oid);
                // Used to query and create related relatedTable.
                relationship = relatedLayer.Relationships.FirstOrDefault();
                var relatedTableID = relationship.RelatedTableID;
                // To retrieve related attribute value from IdentifyResult.
                if (!string.IsNullOrWhiteSpace(relationship.KeyField))
                    keyField = relatedLayer.Fields.FirstOrDefault(f => f.Name == relationship.KeyField);
                var featureServiceUri = dynamicLayer.ServiceUri.Replace("MapServer", "FeatureServer");
                featureServiceUri = string.Format("{0}/{1}", featureServiceUri, relatedTableID);
                relatedTable = await ServiceFeatureTable.OpenAsync(new Uri(featureServiceUri));
                relatedTable.OutFields = OutFields.All;
                if (!relatedTable.IsInitialized)
                    await relatedTable.InitializeAsync(); ;
                BuildAttributeEditor();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(message);
        }

        private object GetDefaultValue(FieldInfo field)
        {
            if (field.Domain is CodedValueDomain)
            {
                var cvd = (CodedValueDomain)field.Domain;
                return cvd.CodedValues.FirstOrDefault().Key;
            }
            if (field.DataType == typeof(DateTime))
                return DateTime.UtcNow;
            if (field.DataType == typeof(int))
                return default(int);
            if (field.DataType == typeof(string))
                return default(string);
            if(field.IsNullable)
                return null;
            return string.Empty;
        }

        private void ClearLocalGraphics()
        {
            var layer = GetGraphicsLayer();
            if (layer != null && layer.Graphics != null)
                layer.Graphics.Clear();
            AddButton.DataContext = null;
            AddButton.IsEnabled = false;
        }

        private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            var dynamicLayer = GetArcGISDynamicMapServiceLayer();
            var layer = GetGraphicsLayer();
            if (dynamicLayer == null || layer == null || objectIdField == null || keyField == null)
                return;
            string message = null;
            try
            {
                ClearLocalGraphics();
                // Perform an identify to pull graphic into local layer.
                var mapPoint = MyMapView.ScreenToLocation(e.Position);
                var parameters = new IdentifyParameters(mapPoint, MyMapView.Extent, 2, (int)MyMapView.ActualHeight, (int)MyMapView.ActualWidth);
                var task = new IdentifyTask(new Uri(dynamicLayer.ServiceUri));
                var result = await task.ExecuteAsync(parameters);
                if (result == null || result.Results == null || result.Results.Count == 0)
                    return;
                var item = result.Results.FirstOrDefault();
                var graphic = (Graphic)item.Feature;
                if (graphic == null)
                    return;
                // Retrieve graphic ID using Field.Alias. Identify result use Alias.
                long featureID = 0;
                if (graphic.Attributes.ContainsKey(objectIdField.Alias))
                    featureID = Convert.ToInt64(graphic.Attributes[objectIdField.Alias], CultureInfo.InvariantCulture);
                string requestID = null;
                if(graphic.Attributes.ContainsKey(keyField.Alias))
                    requestID = Convert.ToString(graphic.Attributes[objectIdField.Alias], CultureInfo.InvariantCulture);
                if (featureID == 0 || requestID == null)
                    return;
                // Add selected graphic to local layer.
                layer.Graphics.Add(graphic);
                AddButton.DataContext = new KeyValuePair<long, string>(featureID, requestID);
                AddButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(message);
        }
        
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (relatedTable == null || relatedTable.ServiceInfo == null ||
                relatedTable.ServiceInfo.Fields == null || relationship == null)
                return;
            var item = (KeyValuePair<long, string>)((Button)sender).DataContext;
            var requestID = item.Value;
            RelationshipList.ItemsSource = null;
            string message = null;
            try
            {
                // Create a new graphic and populate its fields.
                // To create relationship, use Relationship.KeyField and featureID from related layer.
                var feature = new GeodatabaseFeature(relatedTable.Schema);
                var keyField = relationship.KeyField;
                foreach (var field in relatedTable.ServiceInfo.Fields)
                {
                    if (!field.IsEditable)
                        continue;
                    if (field.Name == keyField)
                        feature.Attributes[field.Name] = requestID;
                    else
                        feature.Attributes[field.Name] = GetDefaultValue(field);
                }
                var dataForm = new Window() { Content = formGrid, Height = 300, Width = 500, Title = "Attribute Editor" };
                dataForm.DataContext = feature;
                dataForm.Show();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(message);
        }

        private async void QueryButton_Click(object sender, RoutedEventArgs e)
        {
            var dynamicLayer = GetArcGISDynamicMapServiceLayer();
            if (dynamicLayer == null || relationship == null)
                return;
            var item = (KeyValuePair<long, string>)((Button)sender).DataContext;            
            var featureID = item.Key;
            RelationshipList.ItemsSource = null;
            string message = null;
            try
            {
                var task = new QueryTask(new Uri(string.Format("{0}/{1}", dynamicLayer.ServiceUri, 0)));
                var parameters = new RelationshipParameters(new List<long>(new long[] { featureID }), relationship.ID);
                parameters.OutFields = new OutFields(new string[] { relatedTable.ObjectIDField });
                var result = await task.ExecuteRelationshipQueryAsync(parameters);
                if (result == null || result.RelatedRecordGroups == null || result.RelatedRecordGroups.Count == 0)
                    return;
                RelationshipList.ItemsSource = result.RelatedRecordGroups[0];
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(message);
        }
        
        private async void RelatedFeatureButton_Click(object sender, RoutedEventArgs e)
        {
            if (relatedTable == null || string.IsNullOrWhiteSpace(relatedTable.ObjectIDField))
                return;
            var graphic = (Feature)((Button)sender).DataContext;
            if (!graphic.Attributes.ContainsKey(relatedTable.ObjectIDField))
                return;
            var featureID = Convert.ToInt64(graphic.Attributes[relatedTable.ObjectIDField], CultureInfo.InvariantCulture);
            string message = null;
            try
            {
                var feature = await relatedTable.QueryAsync(featureID);
                if (feature == null)
                    return;
                // Display attributes for this feature.
                var dataForm = new Window() { Content = formGrid, Height = 300, Width = 500, Title = "Attribute Editor" };
                dataForm.DataContext = feature;
                dataForm.Show();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(message);
        }

        private async Task UpdateRelatedTableAsync(GeodatabaseFeature feature)
        {
            if (feature == null || relatedTable == null || string.IsNullOrWhiteSpace(relatedTable.ObjectIDField))
                return;
            if (!feature.Attributes.ContainsKey(relatedTable.ObjectIDField))
                await relatedTable.AddAsync(feature);
            else
                await relatedTable.UpdateAsync(feature);
            if (relatedTable.HasEdits)
                await relatedTable.ApplyEditsAsync();
        }

        private async void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (relatedTable == null)
                return;
            var feature = (GeodatabaseFeature)((Button)sender).DataContext;  
            string message = null;
            try
            {

                var editPrompt = MessageBox.Show("Do you want to save the attribute change?", "Update attributes", MessageBoxButton.OKCancel);
                if (editPrompt == MessageBoxResult.OK)
                    await UpdateRelatedTableAsync(feature);
                ClearLocalGraphics();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(message);
        }

        private async Task DeleteFeatureAsync(GeodatabaseFeature feature)
        {
            if (feature == null || relatedTable == null || string.IsNullOrWhiteSpace(relatedTable.ObjectIDField))
                return;
            await relatedTable.DeleteAsync(feature);
            if (relatedTable.HasEdits)
                await relatedTable.ApplyEditsAsync();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (relatedTable == null)
                return;
            var feature = (GeodatabaseFeature)((Button)sender).DataContext;            
            string message = null;
            try
            {
                var deletePrompt = MessageBox.Show("Are you sure you want to delete this feature from the relatedTable?", "Delete graphic", MessageBoxButton.OKCancel);
                if (deletePrompt == MessageBoxResult.OK)
                    await DeleteFeatureAsync(feature);
                ClearLocalGraphics();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(message);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (formGrid != null && formGrid.Parent is Window)
            {
                var dataForm = (Window)formGrid.Parent;
                dataForm.Close();
            }
        }
    }
}