using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Edit;
using System;
using System.Collections.Generic;
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
    /// Demonstrates how attributes of a feature can be modified from a ServiceFeatureTable and how this type of edit is pushed to the server or canceled.
    /// </summary>
    /// <title>Edit Attributes</title>
    /// <category>Editing</category>
    public partial class EditAttributes : UserControl
    {
        private Grid formGrid;
        public EditAttributes()
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

        private ServiceFeatureTable GetFeatureTable(FeatureLayer ownerLayer = null)
        {
            var layer = ownerLayer ?? GetFeatureLayer();
            if (layer == null || !(layer.FeatureTable is ServiceFeatureTable))
                return null;
            var table = (ServiceFeatureTable)layer.FeatureTable;
            return table;
        }

        private async void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
        {
            if (e.LoadError != null)
                return;
            if (e.Layer is FeatureLayer)
            {
                var layer = (FeatureLayer)e.Layer;
                var table = layer.FeatureTable as ServiceFeatureTable;
                if (table != null)
                {
                    if (!table.IsInitialized)
                        await table.InitializeAsync();
                    // Builds the Attribute Editor based on FieldInfo (i.e. Editable, Domain, Length, DataType)
                    // For better validation and customization support use FeatureDataForm from the Toolkit.
                    if (table.ServiceInfo != null && table.ServiceInfo.Fields != null)
                    {
                        var itemtTemplate = this.Resources["MyItemTemplate"] as DataTemplate;
                        formGrid = new Grid() { Margin = new Thickness(2d) };
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
                            var binding = new Binding(string.Format("Attributes[{0}]", field.Name));
                            if (field.IsEditable)
                            {
                                binding.Mode = BindingMode.TwoWay;
                                var keyValueConverter = this.Resources["KeyValueConverter"] as KeyValueConverter;
                                if (hasFeatureTypes && table.ServiceInfo.TypeIdField == field.Name)
                                {
                                    value = new ComboBox() { ItemTemplate = itemtTemplate, Margin = new Thickness(2d) };
                                    ((ComboBox)value).ItemsSource = from t in table.ServiceInfo.Types
                                                                    select new KeyValuePair<object, string>(t.ID, t.Name);
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
                    }
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (formGrid != null && formGrid.Parent is Window)
            {
                var dataForm = (Window)formGrid.Parent;
                dataForm.Close();
        }
        }
        
        private async void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            var feature = (sender as Button).DataContext as GeodatabaseFeature;
            if(feature == null)
                return;
               var table = GetFeatureTable();
            if (table == null)
                return;
            string message = null;
            try
            {
                // Updates the feature with its Attributes already modified by the two-way binding.
                await table.UpdateAsync(feature);
                SaveButton.IsEnabled = table.HasEdits;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(message);        
        }

        private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            var layer = GetFeatureLayer();
            var table = GetFeatureTable(layer);
            if (layer == null || table == null)
                return;
            layer.ClearSelection();
            string message = null;
            try
            {
                // Selects feature based on hit-test 
                // and performs local query to get the feature and display its attributes
                var featureIDs = await layer.HitTestAsync(MyMapView, e.Position);
                if (featureIDs != null)
                {
                    layer.SelectFeatures(featureIDs);
                    var featureID = featureIDs.FirstOrDefault();
                    var features = await table.QueryAsync(new long[] { featureID }, true);
                    if (features != null)
                    {
                        var feature = features.FirstOrDefault();
                        if (feature != null && formGrid != null)
                        {
                            var dataForm = new Window() { Content = formGrid, Height = 300, Width = 500, Title = "Attribute Editor" };
                            dataForm.DataContext = feature;
                            dataForm.Show();
                        }
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
                    var hasAdds = saveResult.AddResults != null && saveResult.AddResults.Any();
                    var hasDeletes = saveResult.DeleteResults != null && saveResult.DeleteResults.Any();
                    if (hasAdds || hasDeletes)
                        throw new Exception("This sample only updates attributes of existing features and should not result in any add nor delete.");
                    var sb = new StringBuilder();
                    var editMessage = GetResultMessage(saveResult.UpdateResults);
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
            var layer = GetFeatureLayer();
            var table = GetFeatureTable(layer);
            if (layer == null || table == null || !table.HasEdits)
                return;
            // Cancels the local edits by refreshing features with preserveEdits=false 
            // and awaits for UpdatedCompleted before performing query.
            var cancelResult = await CancelEditsAsync(table);
            if (cancelResult)
            {
                var featureID = layer.SelectedFeatureIDs.FirstOrDefault();
                if (featureID != null && formGrid != null && formGrid.Parent is Window)
                {
                    // To reflect the changes in fields, query feature and update form data context.
                    var feature = (GeodatabaseFeature)await table.QueryAsync(featureID);
                    formGrid.DataContext = feature;
                }
                SaveButton.IsEnabled = table.HasEdits;
            }
        }
    }
}