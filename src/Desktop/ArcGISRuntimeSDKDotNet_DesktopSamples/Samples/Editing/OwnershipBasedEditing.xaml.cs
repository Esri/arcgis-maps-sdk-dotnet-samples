using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Security;
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
    /// Demonstrates how to restrict feature edits by ownership and user login.
    /// </summary>
    /// <title>Ownership-based Editing</title>
    /// <category>Editing</category>
    public partial class OwnershipBasedEditing : UserControl
    {
        private TaskCompletionSource<Credential> loginTcs; // Used for logging in.
        private Grid formGrid; // Used for attribute editing.
        private enum EditType
        {
            Add,
            Update,
            Delete
        }

        public OwnershipBasedEditing()
        {
            InitializeComponent();
            // You may also use SignInDialog from the Toolkit: https://github.com/Esri/arcgis-toolkit-dotnet
            IdentityManager.Current.ChallengeHandler = new ChallengeHandler(OnChallenge);
        }

        private async Task<Credential> OnChallenge(CredentialRequestInfo requestInfo)
        {
            // Use Dispatcher to access UIElements
            if (Dispatcher == null)
                return await ChallengeUI(requestInfo);
            return await Dispatcher.Invoke(() => ChallengeUI(requestInfo));
        }

        private async Task<Credential> ChallengeUI(CredentialRequestInfo requestInfo)
        {

            try
            {
                string username = "user1";
                string password = "user1";
                LoginInfo.Tag = requestInfo;
                LoginInfo.Text = string.Format("Login to: {0}", requestInfo.ServiceUri);
                Username.Text = username;
                Password.Text = password;
                LoginPanel.Visibility = Visibility.Visible;
                loginTcs = new TaskCompletionSource<Credential>();
                return await loginTcs.Task;
            }
            finally
            {
                LoginPanel.Visibility = Visibility.Collapsed;
            }
        }

        int attemptCount = 0;
        private async void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            if (loginTcs == null || loginTcs.Task == null)
                return;
            try
            {
                var requestInfo = (CredentialRequestInfo)LoginInfo.Tag;
                var username = Username.Text.Trim();
                var password = Password.Text.Trim();
                var credentials = await IdentityManager.Current.GenerateCredentialAsync(requestInfo.ServiceUri,
                   username, password, requestInfo.GenerateTokenOptions);
                loginTcs.TrySetResult(credentials);
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = ex.Message;
                attemptCount++;
                if (attemptCount >= 3)
                    loginTcs.TrySetException(ex);
            }
        }
        
        private FeatureLayer GetFeatureLayer()
        {
            if (MyMapView == null || MyMapView.Map == null || MyMapView.Map.Layers == null)
                return null;
            return MyMapView.Map.Layers["SaveTheBayMarineLayer"] as FeatureLayer;
        }

        private ServiceFeatureTable GetFeatureTable(FeatureLayer ownerLayer = null)
        {
            var layer = ownerLayer ?? GetFeatureLayer();
            if (layer == null || !(layer.FeatureTable is ServiceFeatureTable))
                return null;
            return (ServiceFeatureTable)layer.FeatureTable;
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
            LoginPanel.Visibility = Visibility.Visible;
        }

        private void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            // Remove credential, table and layer. Add a new instance to trigger a new challenge.
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
            // Sign-out user when switching between samples.
            RemoveCredential();
        }

        private void BuildAttributeEditor()
        {
            var table = GetFeatureTable();
            if (table == null || table.ServiceInfo == null || table.ServiceInfo.Fields == null)
                return;
            // Builds the Attribute Editor based on FieldInfo (i.e. Editable, Domain, Length, DataType)
            // For better validation and customization support,
            // use FeatureDataForm from the Toolkit: https://github.com/Esri/arcgis-toolkit-dotnet.              
            formGrid = new Grid() { Margin = new Thickness(2d) };
            formGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            formGrid.ColumnDefinitions.Add(new ColumnDefinition());
            var fieldCount = table.ServiceInfo.Fields.Count + 1; // Fields + Apply/Delete/Edit/Close button
            for (int i = 0; i < fieldCount; i++)
                formGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            int row = 0;
            var hasFeatureTypes = !string.IsNullOrWhiteSpace(table.ServiceInfo.TypeIdField) && table.ServiceInfo.Types != null && table.ServiceInfo.Types.Count > 1;
            foreach (var field in table.ServiceInfo.Fields)
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
                    // This service only contains CodedValueDomain and FeatureTypes.
                    // Depending on your service, you might consider handling item selection for RangeDomain.                    
                    if (hasFeatureTypes && field.Name == table.ServiceInfo.TypeIdField || field.Domain is CodedValueDomain)
                    {
						var itemTemplate = this.Resources["MyItemTemplate"] as DataTemplate;
                        value = new ComboBox() { ItemTemplate = itemTemplate, Margin = new Thickness(2d) };
                        if (field.Domain is CodedValueDomain)
                            ((ComboBox)value).ItemsSource = ((CodedValueDomain)field.Domain).CodedValues;
                        else
                        {
                        ((ComboBox)value).ItemsSource = from t in table.ServiceInfo.Types
                                                        select new KeyValuePair<object, string>(t.ID, t.Name);
                    }
                        binding.Converter = this.Resources["KeyValueConverter"] as KeyValueConverter;
                        binding.ConverterParameter = ((ComboBox)value).ItemsSource;
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
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            buttonGrid.SetValue(Grid.ColumnSpanProperty, 2);
            buttonGrid.SetValue(Grid.RowProperty, row);
            var applyButton = new Button() { Content = "Apply", Margin = new Thickness(2d) };
            applyButton.Click += ApplyButton_Click;
            buttonGrid.Children.Add(applyButton);
            var editButton = new Button() { Content = "Edit Geometry", Margin = new Thickness(2d) };
            editButton.SetValue(Grid.ColumnProperty, 1);
            editButton.Click += EditButton_Click;
            buttonGrid.Children.Add(editButton);
            var deleteButton = new Button() { Content = "Delete Feature", Margin = new Thickness(2d) };
            deleteButton.SetValue(Grid.ColumnProperty, 2);
            deleteButton.Click += DeleteButton_Click;
            buttonGrid.Children.Add(deleteButton);
            var closeButton = new Button() { Content = "Close", Margin = new Thickness(2d) };
            closeButton.SetValue(Grid.ColumnProperty, 3);
            closeButton.Click += CloseButton_Click;
            buttonGrid.Children.Add(closeButton);
            formGrid.Children.Add(buttonGrid);
            AddButton.IsEnabled = true;
        }

        private async void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
        {
            if (e.LoadError != null || !(e.Layer is FeatureLayer))
                return;
            var layer = (FeatureLayer)e.Layer;
            var table = (ServiceFeatureTable)layer.FeatureTable;
            string message = null;
            try
            {
                if (!table.IsInitialized)
                    await table.InitializeAsync();
                DisplayName.Text = table.ServiceInfo.Name;
                var credential = IdentityManager.Current.FindCredential(table.ServiceUri);
                UserName.Text = credential != null ? credential.UserName : string.Empty;
                BuildAttributeEditor();
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                MessageBox.Show(message);
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var feature = (GeodatabaseFeature)((Button)sender).DataContext;
            var layer = GetFeatureLayer();
            var table = GetFeatureTable(layer);
            if (layer == null || table == null || string.IsNullOrWhiteSpace(table.ObjectIDField))
                return;            
            string message = null;
            try
            {
                var editPrompt = MessageBox.Show("Tap on a new location to update the geometry.", "Update geometry", MessageBoxButton.OKCancel);
                if (editPrompt == MessageBoxResult.OK)
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

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var feature = (GeodatabaseFeature)((Button)sender).DataContext;
            var table = GetFeatureTable();
            if (table == null)
                return;
            string message = null;
            try
            {
                var deletePrompt = MessageBox.Show("Are you sure you want to delete feature?", "Delete feature", MessageBoxButton.OKCancel);
                if (deletePrompt == MessageBoxResult.OK)
                {
                    CloseDataForm();
                    // Deletes the feature from the table.
                    await table.DeleteAsync(feature);
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
        
        private async void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            var feature = (GeodatabaseFeature)((Button)sender).DataContext;
            var table = GetFeatureTable();
            if (table == null)
                return;
            string message = null;
            try
            {
                // Updates the feature with its Attributes already modified by the two-way binding.
                await table.UpdateAsync(feature);
                if (formGrid != null && formGrid.Parent is Window)
                {
                    // To reflect the changes in auto-populated fields, query feature and update form data context.
                    feature = (GeodatabaseFeature)await table.QueryAsync(Convert.ToInt64(feature.Attributes[table.ObjectIDField], CultureInfo.InvariantCulture));
                    formGrid.DataContext = feature;
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
                        if (feature != null && formGrid != null)
                        {                            
                            var dataForm = new Window() { Content = formGrid, Height = 350, Width = 500, Title = "Attribute Editor" };
                            dataForm.DataContext = feature;
                            // Determines whether currently logged-in user can update/delete the feature.
                            // See also CanAdd/Update/DeleteAttachment.
                            var geodatabaseFeature = (GeodatabaseFeature)feature;
                            var canUpdateAttributes = table.CanUpdateFeature(geodatabaseFeature);
                            var canUpdateGeometry = table.CanUpdateGeometry(geodatabaseFeature);
                            var canDeleteFeature = table.CanDeleteFeature(geodatabaseFeature);
                            foreach (var child in formGrid.Children)
                            {
                                var element = (FrameworkElement)child;
                                if (element.Name == "ButtonGrid")
                                {
                                    var buttonGrid = (Grid)element;
                                    foreach (var button in buttonGrid.Children)
                                    {
                                        var buttonElement = (FrameworkElement)button;
                                        if (buttonElement.Name == "ApplyButton")
                                            buttonElement.IsEnabled = canUpdateAttributes;
                                        else if (buttonElement.Name == "EditButton")
                                            buttonElement.IsEnabled = canUpdateGeometry;
                                        else if (buttonElement.Name == "DeleteButton")
                                            buttonElement.IsEnabled = canDeleteFeature;
                                    }
                                }
                                else
                                    element.IsEnabled = canUpdateAttributes;
                            }
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

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var table = GetFeatureTable();
            if (table == null)
                return;
            var typeId = Convert.ToInt32(((Button)sender).Tag, CultureInfo.InvariantCulture);
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
                if (formGrid != null)
                {
                    var dataForm = new Window() { Content = formGrid, Height = 350, Width = 500, Title = "Attribute Editor" };
                    formGrid.DataContext = feature;
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

        private string GetResultMessage(IEnumerable<FeatureEditResultItem> editResults, EditType editType)
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