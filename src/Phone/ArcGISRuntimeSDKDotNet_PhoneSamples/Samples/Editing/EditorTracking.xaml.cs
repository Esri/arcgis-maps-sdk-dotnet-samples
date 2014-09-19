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
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// Demonstrates how Identity Manager is used to log-in a token-secured service to identify the user performing edits on feature and how these edits are pushed to the server or canceled.
    /// </summary>
    /// <title>Editor Tracking</title>
    /// <category>Editing</category>
    public partial class EditorTracking : Page
    {
        private TaskCompletionSource<Credential> loginTcs; // Used for logging in.
        private Flyout dataForm; // Used for attribute editing.        
        private bool isAdding;

        public EditorTracking()
        {
            InitializeComponent();
            var table = GetFeatureTable();
            table.OutFields = OutFields.All;
            // You may also use SignInDialog from the Toolkit.
            IdentityManager.Current.ChallengeHandler = new ChallengeHandler(OnChallenge);
        }

        private async Task<Credential> OnChallenge(CredentialRequestInfo requestInfo)
        {
            // Use Dispatcher to access UIElements
            var tcs = new TaskCompletionSource<Credential>();
            if (Dispatcher == null)
                tcs.TrySetResult(await ChallengeUI(requestInfo));
            else
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    new DispatchedHandler(async () =>
                    {
                        tcs.TrySetResult(await ChallengeUI(requestInfo));
                    }));
            }
            var credential = await tcs.Task;
            return credential;
        }

        private async Task<Credential> ChallengeUI(CredentialRequestInfo requestInfo)
        {
            try
            {
                // Check ArcGIS Token Secured Services sample for the rest of login code.
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
                
        private FeatureLayer GetFeatureLayer()
        {
            if (MyMapView == null || MyMapView.Map == null || MyMapView.Map.Layers == null)
                return null;
            var layer = MyMapView.Map.Layers["WildfireLayer"] as FeatureLayer;
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
            attemptCount = 0;
            var table = GetFeatureTable();
            if (table == null)
                return;
            var credential = IdentityManager.Current.FindCredential(table.ServiceUri);
            if (credential == null)
                return;
            IdentityManager.Current.RemoveCredential(credential);
            AddButton.IsEnabled = false;
        }

        int attemptCount = 0;
        private async void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            if (loginTcs == null || loginTcs.Task == null)
                return;
            try
            {
                var requestInfo = LoginInfo.Tag as CredentialRequestInfo;
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
                        var fieldCount = table.ServiceInfo.Fields.Count + 1; // Fields + Apply/Close button
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
                        var buttonGrid = new Grid() { HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(2d) };
                        buttonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                        buttonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                        buttonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                        buttonGrid.SetValue(Grid.ColumnSpanProperty, 2);
                        buttonGrid.SetValue(Grid.RowProperty, row);
                        var applyButton = new Button() { Content = "Apply", Margin = new Thickness(2d) };
                        applyButton.Click += ApplyButton_Click;
                        buttonGrid.Children.Add(applyButton);
                        var deleteButton = new Button() { Content = "Delete Feature", Margin = new Thickness(2d) };
                        deleteButton.SetValue(Grid.ColumnProperty, 1);
                        deleteButton.Click += DeleteButton_Click;
                        buttonGrid.Children.Add(deleteButton);
                        var closeButton = new Button() { Content = "Close", Margin = new Thickness(2d) };
                        closeButton.SetValue(Grid.ColumnProperty, 2);
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
                dialog.Commands.Add(new UICommand("OK", new UICommandInvokedHandler( async (command) => 
                    {
                    // Deletes the feature from the table.
                    await table.DeleteAsync(feature);
                    SaveButton.IsEnabled = table.HasEdits;
                    CloseDataForm();
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
            if (isAdding)
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
            isAdding = true;
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
            finally
            {
                isAdding = false;
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