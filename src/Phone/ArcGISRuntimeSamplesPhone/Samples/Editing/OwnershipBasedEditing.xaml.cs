using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Security;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
    /// <summary>
    /// Demonstrates how editing of features can be restricted by ownership-based access defined in the service.
    /// </summary>
    /// <title>Ownership-based Editing</title>
    /// <category>Editing</category>
    public partial class OwnershipBasedEditing : Page
    {
        private TaskCompletionSource<Credential> loginTask;
        int loginAttempts = 0;

        public OwnershipBasedEditing()
        {
            InitializeComponent();
            // Enables application to handle credential challenge.
            IdentityManager.Current.ChallengeHandler = new ChallengeHandler(GetCredentialAsync);
        }

        /// <summary>
        /// Forwards credential request info using Dispatcher to enable access to UI elements.
        /// </summary>
        private async Task<Credential> GetCredentialAsync(CredentialRequestInfo info)
        {
            var tcs = new TaskCompletionSource<Credential>();
            if (Dispatcher == null)
                tcs.TrySetResult(await GetCredentialUIAsync(info));
            else
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    new DispatchedHandler(async () =>
                    {
                        tcs.TrySetResult(await GetCredentialUIAsync(info));
                    }));
            }
            var credential = await tcs.Task;
            return credential;
        }

        /// <summary>
        /// Initializes UI and login task with credential request info.
        /// </summary>
        private async Task<Credential> GetCredentialUIAsync(CredentialRequestInfo info)
        {
            InitalizeLoginPanel(info.ServiceUri);
            loginTask = new TaskCompletionSource<Credential>(info);
            string message = null;
            Credential credential = null;
            try
            {
                credential = await loginTask.Task;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            finally
            {
                LoginPanel.Visibility = Visibility.Collapsed;
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();
            return credential;
        }

        private void InitalizeLoginPanel(string serviceUri)
        {
            LoginInfo.Text = string.Format("Login to: {0}", serviceUri);
            Username.Text = "user2";
            Password.Text = "user2";
        }

        /// <summary>
        /// Attempts to generate credential for service to set login task result.
        /// </summary>
        private async void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            if (loginTask == null || loginTask.Task == null || !(loginTask.Task.AsyncState is CredentialRequestInfo))
                return;
            var info = (CredentialRequestInfo)loginTask.Task.AsyncState;
            var username = Username.Text.Trim();
            var password = Password.Text.Trim();
            try
            {
                var credential = await IdentityManager.Current.GenerateCredentialAsync(info.ServiceUri, username, password);
                loginTask.TrySetResult(credential);
            }
            catch (Exception ex)
            {
                loginAttempts++;
                if (loginAttempts > 3)
                    loginTask.TrySetException(ex);
            }
        }

        /// <summary>
        /// Removes credential used to access the service and adds a new instance of layer to trigger another challenge.
        /// </summary>
        private void SignOut()
        {
            AddButton.IsEnabled = false;
            LoginPanel.Visibility = Visibility.Visible;
            var layer = MyMapView.Map.Layers["Marine"] as FeatureLayer;
            if (layer == null) return;
            var table = (ServiceFeatureTable)layer.FeatureTable;
            var credential = IdentityManager.Current.FindCredential(table.ServiceUri);
            if (credential != null)
            {
                loginAttempts = 0;
                IdentityManager.Current.RemoveCredential(credential);
                MyMapView.Map.Layers.Remove(layer);
                InitalizeLoginPanel(table.ServiceUri);
                MyMapView.Map.Layers.Add(new FeatureLayer(new Uri(table.ServiceUri)) { ID = layer.ID });
            }
        }

        /// <summary>
        /// Signs out from current login so sample can trigger credential challenge.
        /// </summary>
        private void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            SignOut();
        }

        /// <summary>
        /// Updates login information based on credential.
        /// </summary>
        private void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
        {
            if (e.LoadError != null || !(e.Layer is FeatureLayer))
                return;
            var layer = (FeatureLayer)e.Layer;
            var table = (ServiceFeatureTable)layer.FeatureTable;
            if (table.ServiceInfo != null)
            {
                AddButton.IsEnabled = true;
                var credential = IdentityManager.Current.FindCredential(table.ServiceUri);
                if (credential is TokenCredential)
                    LoginInfo.Text = string.Format("Login '{0}' service as: {1}", table.ServiceInfo.Name, ((TokenCredential)credential).UserName);
            }
        }

        /// <summary>
        /// Signs out from current login so switching samples can trigger credential challenge.
        /// </summary>
        private void MyMapView_LayerUnloaded(object sender, LayerUnloadedEventArgs e)
        {
            if (!(e.Layer is FeatureLayer))
                return;
            SignOut();
        }

        /// <summary>
        /// Selects feature and checks whether update or delete is allowed.
        /// </summary>
        private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            // Ignore tap events while in edit mode so we do not interfere with add point.
            if (MyMapView.Editor.IsActive)
                return;
            var layer = MyMapView.Map.Layers["Marine"] as FeatureLayer;
            var table = (ArcGISFeatureTable)layer.FeatureTable;
            layer.ClearSelection();

            // Service metadata includes edit fields which can be used
            // to check ownership and/or tracking the last edit made.
            if (table.ServiceInfo == null || table.ServiceInfo.EditFieldsInfo == null)
                return;
            var creatorField = table.ServiceInfo.EditFieldsInfo.CreatorField;
            if (string.IsNullOrWhiteSpace(creatorField))
                return;

            string message = null;
            try
            {
                // Performs hit test on layer to select feature.
                var features = await layer.HitTestAsync(MyMapView, e.Position);
                if (features == null || !features.Any())
                    return;
                var featureID = features.FirstOrDefault();
                layer.SelectFeatures(new long[] { featureID });
                var feature = (GeodatabaseFeature)await layer.FeatureTable.QueryAsync(featureID);

                // Displays feature attributes and editing restrictions.
                if (feature.Attributes.ContainsKey(table.ObjectIDField))
                    message += string.Format("[{0}] : {1}\n", table.ObjectIDField, feature.Attributes[table.ObjectIDField]);
                if (feature.Attributes.ContainsKey(creatorField))
                    message += string.Format("[{0}] : {1}\n", creatorField, feature.Attributes[creatorField]);
                // Checks whether service allows current user to add/update/delete attachment of feature.
                message += string.Format("Attachments\n\tCanAdd - {0}\n\tCanUpdate - {1}\n\tCanDelete - {2}\n",
                    table.CanAddAttachment(feature), table.CanUpdateAttachment(feature), table.CanDeleteAttachment(feature));
                // Checks whether service allows current user to update feature's geometry or attributes; and delete feature.
                message += string.Format("Feature\n\tCanUpdateGeometry - {0}\n\tCanUpdate - {1}\n\tCanDelete - {2}\n",
                    table.CanUpdateGeometry(feature), table.CanUpdateFeature(feature), table.CanDeleteFeature(feature));
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }
            if (!string.IsNullOrWhiteSpace(message))
                await new MessageDialog(message).ShowAsync();
        }

        /// <summary>
        /// Adds a new turtle feature owned by current user.
        /// </summary>
        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var layer = MyMapView.Map.Layers["Marine"] as FeatureLayer;
            var table = (ArcGISFeatureTable)layer.FeatureTable;
            var typeID = (Int32)((Button)sender).Tag;
            string message = null;
            try
            {
                var mapPoint = await MyMapView.Editor.RequestPointAsync();
                var feature = new GeodatabaseFeature(table.Schema) { Geometry = mapPoint };
                if (table.ServiceInfo.Types == null)
                    return;
                var featureType = table.ServiceInfo.Types.FirstOrDefault(t => Int32.Equals(Convert.ToInt32(t.ID, CultureInfo.InvariantCulture), typeID));
                if (featureType == null)
                    return;
                var template = featureType.Templates.FirstOrDefault();
                if (template == null || template.Prototype == null || template.Prototype.Attributes == null)
                    return;
                foreach (var item in template.Prototype.Attributes)
                    feature.Attributes[item.Key] = item.Value;
                if (table.CanAddFeature(feature))
                    table.AddAsync(feature);
                if (table.HasEdits)
                {
                    if (table is ServiceFeatureTable)
                    {
                        var serviceTable = (ServiceFeatureTable)table;
                        // Pushes new feature back to the server.
                        var result = await serviceTable.ApplyEditsAsync();
                        if (result.AddResults == null || result.AddResults.Count < 1)
                            return;
                        var addResult = result.AddResults[0];
                        if (addResult.Error != null)
                            message = addResult.Error.Message;
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
    }
}
