using ArcGISRuntimeSDKDotNet_StoreSamples.Common;
using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Tasks.Edit;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates the ability of feature services to restrict editing operations based on feature ownership.  The owner is defined as the user who created the feature.  In this example, owners can update or delete their features.  Feature not owned by the current user cannot be updated or deleted.  
    /// </summary>
    /// <title>Ownership-based Editing</title>
    /// <category>Editing</category>
    public partial class OwnershipBasedEditing : Page
    {
        private Flyout dataForm; // Used for attribute editing.   

        public OwnershipBasedEditing()
        {
            InitializeComponent();
            var table = GetFeatureTable();
            table.OutFields = OutFields.All;
        }
                
        private FeatureLayer GetFeatureLayer()
        {
            if (MyMapView == null || MyMapView.Map == null || MyMapView.Map.Layers == null)
                return null;
            var layer = MyMapView.Map.Layers["SaveTheBayMarineLayer"] as FeatureLayer;
            if (layer == null)
                return null;
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
        
        private void RemoveCredential()
        {
            var table = GetFeatureTable();
            if (table == null)
                return;
            var credential = IdentityManager.Current.FindCredential(table.ServiceUri);
            if (credential == null)
                return;
            IdentityManager.Current.RemoveCredential(credential);
            AddButton.IsEnabled = false;
        }

        private void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveCredential();
            var layer = GetFeatureLayer();
            var table = GetFeatureTable(layer);
            if (layer == null || table == null)
                return;
            MyMapView.Map.Layers.Remove(layer);
            table = new ServiceFeatureTable() { Mode = table.Mode, ServiceUri = table.ServiceUri, OutFields = table.OutFields };
            layer = new FeatureLayer() { ID = layer.ID, FeatureTable = table };
            MyMapView.Map.Layers.Add(layer);
        }
        
        private void MyMapView_Unloaded(object sender, RoutedEventArgs e)
        {
            // To sign-out user when switching between samples.
            RemoveCredential();
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
                        AddButton.IsEnabled = true;
                        DisplayName.Text = table.ServiceInfo.Name;
                        var credential = IdentityManager.Current.FindCredential(table.ServiceUri);
                        UserName.Text = credential != null ? credential.UserName : string.Empty;
                        var formGrid = new Grid() { Margin = new Thickness(2d) };
                        formGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                        formGrid.ColumnDefinitions.Add(new ColumnDefinition());
                        var fieldCount = table.ServiceInfo.Fields.Count + 1; // Fields + Apply/Delete/EditClose button
                        for (int i = 0; i < fieldCount; i++)
                            formGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                        var hasFeatureTypes = !string.IsNullOrWhiteSpace(table.ServiceInfo.TypeIdField) && table.ServiceInfo.Types != null && table.ServiceInfo.Types.Any();
                        int row = 0;
                        foreach (var field in table.ServiceInfo.Fields)
                        {
                            var label = new TextBlock() { Text = field.Alias ?? field.Name, Margin = new Thickness(2d)};
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
                        var buttonGrid = new Grid() {Name = "ButtonGrid", HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(2d) };
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
                        var formPanel = new StackPanel();
                        formPanel.Children.Add(new TextBlock() { Text = "Attribute Editor" });
                        formPanel.Children.Add(formGrid);
                        dataForm = new Flyout() { Content = formPanel, Placement = FlyoutPlacementMode.Full };
                    }
                }
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var feature = (sender as Button).DataContext as GeodatabaseFeature;
            if (feature == null)
                return;
            var layer = GetFeatureLayer();
            var table = GetFeatureTable(layer);
            if (layer == null || table == null || string.IsNullOrWhiteSpace(table.ObjectIDField))
                return;
            string message = null;
            try
            {
                var dialog = new MessageDialog("Tap on a new location to update the geometry.", "Edit feature geometry");
                dialog.Commands.Add(new UICommand("OK", new UICommandInvokedHandler(async (command) =>
                {
                    CloseDataForm();
                    var featureID = Convert.ToInt64(feature.Attributes[table.ObjectIDField], CultureInfo.InvariantCulture);
                    layer.SetFeatureVisibility(new long[] { featureID }, false);
                    // Updates the geometry of the feature.
                    var mapPoint = await MyMapView.Editor.RequestPointAsync();
                    feature.Geometry = mapPoint;
                    layer.SetFeatureVisibility(new long[] { featureID }, true);
                    await table.UpdateAsync(feature);
                    SaveButton.IsEnabled = table.HasEdits;
                })));
                await dialog.ShowAsync();
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

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var feature = (sender as Button).DataContext as GeodatabaseFeature;
            if (feature == null)
                return;
            var table = GetFeatureTable();
            if (table == null)
                return;
            string message = null;
            try
            {
                var dialog = new MessageDialog("Are you sure you want to delete this feature from the table?", "Delete feature");
                dialog.Commands.Add(new UICommand("OK", new UICommandInvokedHandler(async (command) =>
                {
                    CloseDataForm();
                    // Deletes the feature from the table.
                    await table.DeleteAsync(feature);
                    SaveButton.IsEnabled = table.HasEdits;
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
        
        private void CloseDataForm()
        {
            if (dataForm != null)
                dataForm.Hide();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseDataForm();
        }
        
        private async void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            var feature = (sender as Button).DataContext as GeodatabaseFeature;
            if (feature == null)
                return;
            var table = GetFeatureTable();
            if (table == null)
                return;
            string message = null;
            try
            {
                // Updates the feature with its Attributes already modified by the two-way binding.
                await table.UpdateAsync(feature);
                // To reflect the changes in auto-populated fields, query feature and update form data context.
                feature = (GeodatabaseFeature)await table.QueryAsync(Convert.ToInt64(feature.Attributes[table.ObjectIDField], CultureInfo.InvariantCulture));
                if (dataForm != null && dataForm.Content is FrameworkElement)
                    ((FrameworkElement)dataForm.Content).DataContext = feature;
                SaveButton.IsEnabled = table.HasEdits;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();
        }
                
        private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            if (MyMapView.Editor.IsActive)
                return;
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
                        if (feature != null && dataForm != null)
                        {
                            if (dataForm.Content is FrameworkElement)
                                ((FrameworkElement)dataForm.Content).DataContext = feature;
                            // Determines whether currently logged-in user can update/delete the feature.                            
                            // See also CanAdd/Update/DeleteAttachment.
                            var geodatabaseFeature = (GeodatabaseFeature)feature;
                            var canUpdateAttributes = table.CanUpdateFeature(geodatabaseFeature);
                            var canUpdateGeometry = table.CanUpdateGeometry(geodatabaseFeature);
                            var canDeleteFeature = table.CanDeleteFeature(geodatabaseFeature);
                            var formPanel = (StackPanel)dataForm.Content;
                            if (formPanel != null && formPanel.Children.Count > 1 && formPanel.Children[1] is Grid)
                            {
                                var formGrid = (Grid)formPanel.Children[1];
                                foreach (var child in formGrid.Children)
                                {
                                    var element = (FrameworkElement)child;
                                    if (element.Name == "ButtonGrid")
                                    {
                                        var buttonGrid = (Grid)element;
                                        foreach (var button in buttonGrid.Children)
                                        {
                                            var buttonElement = (Control)button;                                            
                                            if (buttonElement.Name == "ApplyButton")
                                                buttonElement.IsEnabled = canUpdateAttributes;
                                            else if (buttonElement.Name == "EditButton")
                                                buttonElement.IsEnabled = canUpdateGeometry;
                                            else if (buttonElement.Name == "DeleteButton")
                                                buttonElement.IsEnabled = canDeleteFeature;
                                        }
                                    }
                                    else if(element is Control)        
                                        ((Control)element).IsEnabled = canUpdateAttributes;
                                }
                            }
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

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var table = GetFeatureTable();
            if (table == null)
                return;
            var typeId = Convert.ToInt32((sender as Button).Tag, CultureInfo.InvariantCulture);
            string message = null;
            try
            {
                // Retrieves a MapPoint from the Editor.
                var mapPoint = await MyMapView.Editor.RequestPointAsync();

                // Creates GeodatabaseFeature based on Table.Schema and Geometry.
                var feature = new GeodatabaseFeature(table.Schema)
                {
                    Geometry = mapPoint
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

                if (!table.CanAddFeature(feature))
                    throw new Exception("The service does not allow adding new feature.");
                // Adds feature to local geodatabase.
                var id = await table.AddAsync(feature);
                if (dataForm != null)
                {
                    if (dataForm.Content is FrameworkElement)
                        ((FrameworkElement)dataForm.Content).DataContext = feature;
                    dataForm.ShowAt(MyMapView);
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
                await new MessageDialog(message).ShowAsync();
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
            var table = GetFeatureTable();
            if (table == null || !table.HasEdits)
                return;
            // Cancels the local edits by refreshing features with preserveEdits=false 
            // and awaits for UpdatedCompleted before checking HasEdits.
            var cancelResult = await CancelEditsAsync(table);
            if (cancelResult)
                SaveButton.IsEnabled = table.HasEdits;
        }       
    }
}