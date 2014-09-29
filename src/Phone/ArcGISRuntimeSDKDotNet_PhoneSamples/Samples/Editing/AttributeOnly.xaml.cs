using ArcGISRuntimeSDKDotNet_StoreSamples.Common;
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
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates how to selectively import and update feature attributes to local geodatabase and update the dynamic layer to reflect these changes.
    /// </summary>
    /// <title>Attribute Only</title>
    /// <category>Editing</category>
    public partial class AttributeOnly : Page
    {
        private Flyout dataForm;
        private ServiceFeatureTable table;

        public AttributeOnly()
        {
            InitializeComponent();
            var layer = GetArcGISDynamicMapServiceLayer();
            if (layer != null)
                layer.VisibleLayers = new ObservableCollection<int>(new int[] { 0 });
        }

        private ArcGISDynamicMapServiceLayer GetArcGISDynamicMapServiceLayer()
        {
            if (MyMapView.Map == null || MyMapView.Map.Layers == null)
                return null;
            var layer = MyMapView.Map.Layers["PoolPermitDynamicLayer"] as ArcGISDynamicMapServiceLayer;
            return layer;
        }

        private GraphicsLayer GetGraphicsLayer()
        {
            if (MyMapView.Map == null || MyMapView.Map.Layers == null)
                return null;
            var layer = MyMapView.Map.Layers["PoolPermitFeatureLayer"] as GraphicsLayer;
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
                var featureServiceUri = dynamicLayer.ServiceUri.Replace("MapServer", "FeatureServer");
                featureServiceUri = string.Format("{0}/{1}", featureServiceUri, layerID);
                table = await ServiceFeatureTable.OpenAsync(new Uri(featureServiceUri), null, MyMapView.SpatialReference);
                table.OutFields = OutFields.All;
                if(!table.IsInitialized)
                    await table.InitializeAsync();
                if(table.ServiceInfo != null)
                {
                    // Builds the Attribute Editor based on FieldInfo (i.e. Editable, Domain, Length, DataType)
                    // For better validation and customization support use FeatureDataForm from the Toolkit.   
                    if (table.ServiceInfo.Fields != null)
                    {
                        var formGrid = new Grid() { Margin = new Thickness(2d) };
                        formGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                        formGrid.ColumnDefinitions.Add(new ColumnDefinition());
                        var fieldCount = table.ServiceInfo.Fields.Count + 1; // Fields + Apply/Close button
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
                            var binding = new Binding() { Path = new PropertyPath(string.Format("Attributes[{0}]", field.Name)) };
                            if (field.IsEditable)
                            {
                                binding.Mode = BindingMode.TwoWay;
                                var keyValueConverter = this.Resources["KeyValueConverter"] as KeyValueConverter;
                                if (hasFeatureTypes && table.ServiceInfo.TypeIdField == field.Name)
                                {
                                    value = new ComboBox() { Margin = new Thickness(2d) };
                                    var lookup = from t in table.ServiceInfo.Types
                                                 select new KeyValuePair<object, string>(t.ID, t.Name);
                                    ((ComboBox)value).ItemsSource = from item in lookup
                                                                    select item.Value;
                                    binding.Converter = keyValueConverter;
                                    binding.ConverterParameter = lookup;
                                    ((ComboBox)value).SetBinding(ComboBox.SelectedItemProperty, binding);
                                }
                                else if (field.Domain != null)
                                {
                                    value = new ComboBox() { Margin = new Thickness(2d) };
                                    if (field.Domain is CodedValueDomain)
                                    {
                                        var lookup = ((CodedValueDomain)field.Domain).CodedValues;
                                        ((ComboBox)value).ItemsSource = from item in lookup
                                                                        select item.Value;
                                        binding.Converter = keyValueConverter;
                                        binding.ConverterParameter = lookup;
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
                                    // Fields of DataType other than string will need a converter.
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
                        var closeButton = new Button() { Content = "Close", Margin = new Thickness(2d) };
                        closeButton.SetValue(Grid.ColumnProperty, 1);
                        closeButton.Click += CloseButton_Click;
                        buttonGrid.Children.Add(closeButton);
                        formGrid.Children.Add(buttonGrid);
                        var formPanel = new StackPanel();
                        formPanel.Children.Add(new TextBlock() { Text = "Attribute Editor" });
                        formPanel.Children.Add(formGrid);
                        dataForm = new Flyout() { Content = formPanel, Placement = FlyoutPlacementMode.Full };
                    }
                }
            }
        }
        
        private long GetFeatureID(Graphic graphic)
        {
            if (graphic == null || table == null ||
                string.IsNullOrWhiteSpace(table.ObjectIDField) ||
                !graphic.Attributes.ContainsKey(table.ObjectIDField))
                return 0;
            var featureID = Convert.ToInt64(graphic.Attributes[table.ObjectIDField], CultureInfo.InvariantCulture);
            return featureID;
        }

        private object ConvertToDataType(object value, Type dataType, Domain domain)
        {
            if (value == null || dataType == value.GetType())
                return value;
            if (domain is CodedValueDomain)
            {
                var cvd = (CodedValueDomain)domain;
                var stringValue = Convert.ToString(value, CultureInfo.InvariantCulture);
                if (cvd.CodedValues != null)
                    return cvd.CodedValues.FirstOrDefault(kvp => kvp.Value == stringValue).Key;
            }
            if (dataType == typeof(int))
                return Convert.ToInt32(value, CultureInfo.InvariantCulture);
            if (dataType == typeof(string))
                return Convert.ToString(value, CultureInfo.InvariantCulture);
            return value;
        }

        private void ClearLocalGraphics()
        {
            var layer = GetGraphicsLayer();
            if (layer != null && layer.Graphics != null)
                layer.Graphics.Clear();
        }

        private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            if (MyMapView.Editor.IsActive)
                return;
            var dynamicLayer = GetArcGISDynamicMapServiceLayer();
            var layer = GetGraphicsLayer();
            if (dynamicLayer == null || layer == null || table == null || string.IsNullOrWhiteSpace(table.ObjectIDField) ||
                table.ServiceInfo == null || table.ServiceInfo.Fields == null)
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
                if (result != null && result.Results != null && result.Results.Count > 0)
                {
                    var item = result.Results.FirstOrDefault();
                    var graphic = item.Feature as Graphic;
                    if (graphic != null)
                    {
                        // Identify result use alias so add an entry that use field name.
                        foreach (var field in table.ServiceInfo.Fields)
                        {
                            if (graphic.Attributes.ContainsKey(field.Alias))
                            {
                                var value =  graphic.Attributes[field.Alias];
                                graphic.Attributes[field.Name] = ConvertToDataType(value, field.DataType, field.Domain);
                            }
                        }
                        graphic.IsSelected = true;
                        // Add selected feature to local layer.
                        layer.Graphics.Add(graphic);
                        if (dataForm != null)
                        {
                            if (dataForm.Content is FrameworkElement)
                                ((FrameworkElement)dataForm.Content).DataContext = graphic;
                            dataForm.ShowAt(MyMapView);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();
        }

        private async Task UpdateFeatureAttributesAsync(Graphic graphic)
        {
            var featureID = GetFeatureID(graphic);
            if (featureID == 0 || table == null)
                return;
            var feature = await table.QueryAsync(featureID);
            if (feature != null)
            {
                foreach (var item in graphic.Attributes)
                {
                    var field = table.ServiceInfo.Fields.FirstOrDefault(f => f.Name == item.Key);
                    if(field != null && field.IsEditable)
                        feature.Attributes[item.Key] = item.Value;
                }
                await table.UpdateAsync(feature);
            }
            if (table.HasEdits)
            {
                await table.ApplyEditsAsync();
                // Refresh the dynamic layer to reflect the attribute change if it should change the feature type.
                var dynamicLayer = GetArcGISDynamicMapServiceLayer();
                if (dynamicLayer != null)
                    dynamicLayer.Invalidate();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (dataForm != null)
                dataForm.Hide();
        }

        private async void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            var graphic = (sender as Button).DataContext as Graphic;
            if (graphic == null || table == null)
                return;
            string message = null;
            try
            {
                var dialog = new MessageDialog("Do you want to apply the changes to your database?", "Apply edits");
                dialog.Commands.Add(new UICommand("OK", new UICommandInvokedHandler(async (command) =>
                {
                    await UpdateFeatureAttributesAsync(graphic);
                    ClearLocalGraphics();
                })));
                dialog.Commands.Add(new UICommand("Cancel", new UICommandInvokedHandler(async (command) =>
                {
                    ClearLocalGraphics();
                })));
                await dialog.ShowAsync();
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