// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using ArcGISRuntimeXamarin.Managers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ArcGISRuntimeXamarin.Samples.GenerateOfflineMap
{
    public partial class GenerateOfflineMap : ContentPage
    {
        // Cache to enable canceling current job
        private GenerateOfflineMapJob _job;

        // Create task that will generate credential on-demand
        private TaskCompletionSource<Credential> _loginTask;

        // Cache so we can fallback to online map
        private Map _onlineMap;

        public GenerateOfflineMap()
        {
            InitializeComponent();

            Title = "Generate an offline map";

            // To take tiled layer offline, this sample requires ArcGIS Portal login
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);

            // Map is created with specified portal item
            _onlineMap = new Map(new Uri("https://www.arcgis.com/sharing/rest/content/items/acc027394bc84c2fb04d1ed317aac674"));
            MyMapView.Map = _onlineMap;
        }

        /// <summary>
        /// Generates an offline map from current map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnTakeMapOfflineClicked(object sender, EventArgs e)
        {
            try
            {
                // Updates UI visibility to match current operation
                Status.Text = "Generating offline map...";
                Progress.IsVisible = true;
                Cancel.IsVisible = true;
                TakeOffline.IsVisible = false;

                // Creates download directory for resulting MobileMapPackage
                var downloadDirectoryPath = Path.Combine(DataManager.GetDataFolder(), "TemporaryData", "NaperilleWaterNetwork_mmpk");
                if (Directory.Exists(downloadDirectoryPath))
                    Directory.Delete(downloadDirectoryPath, true);
                else
                    Directory.CreateDirectory(downloadDirectoryPath);

                // Creates the task that will generate offline map job
                var task = await OfflineMapTask.CreateAsync(MyMapView.Map);

                // Creates and updates parameters with current viewpoint 
                // and overrides portal metadata (i.e. Thumbnail, Title)
                var areaOfInterest = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry)?.TargetGeometry;
                var parameters = await task.CreateDefaultGenerateOfflineMapParametersAsync(areaOfInterest);
                parameters.ItemInfo.Thumbnail = await MyMapView.ExportImageAsync(); 
                parameters.ItemInfo.Title = $"{parameters.ItemInfo.Title} Central";

                // Creates the generate offline map job
                _job = task.GenerateOfflineMap(parameters, downloadDirectoryPath);
                _job.ProgressChanged += OnJobProgressChanged;
                var result = await _job.GetResultAsync();

                // Interrogates result for errors to display
                if (result.HasErrors)
                {
                    var errors = new StringBuilder();
                    foreach (var layerError in result.LayerErrors)
                    {
                        var layer = layerError.Key;
                        var exception = layerError.Value;
                        errors.AppendLine(string.Format("{0} : {1}", layer.Name, exception.Message));
                    }
                    foreach (var tableError in result.TableErrors)
                    {
                        var table = tableError.Key;
                        var exception = tableError.Value;
                        errors.AppendLine(string.Format("{0} : {1}", table.TableName, exception.Message));
                    }
                    await DisplayAlert("Error", errors.ToString(), "OK");
                }
                // Displays offline map
                MyMapView.Map = result.OfflineMap;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
                // Fallback to online map 
                MyMapView.Map = _onlineMap;
            }
            finally
            {
                // Offline map will no longer be associated with a portal item
                var isOnline = MyMapView.Map.Item != null;

                // Update status/progress/button visibility to initial value
                Status.Text = isOnline ? "Loading online map..." : "Map is now offline!";
                Progress.IsVisible = false;
                Cancel.IsVisible = false;
                TakeOffline.IsVisible = isOnline;
            }
        }

        /// <summary>
        /// Updates UI to display progress towards job completion
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnJobProgressChanged(object sender, EventArgs e)
        {
            var job = (GenerateOfflineMapJob)sender;
            if (job.Status == JobStatus.Failed || job.Status == JobStatus.Succeeded)
                job.ProgressChanged -= OnJobProgressChanged;
            Device.BeginInvokeOnMainThread(() =>
            {
                Status.Text = $"Generating offline map... {job.Progress}%";
                Progress.Progress = job.Progress;
            });
        }

        /// <summary>
        /// Cancels current generate offline map job
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCancelClicked(object sender, EventArgs e)
        {
            if (_job == null)
                return;
            _job.Cancel();

            // Update status/progress/button visibility to match current operation.
            Status.Text = "Job canceled";
            Cancel.IsVisible = false;
            Progress.IsVisible = false;
            // Offline map will no longer be associated with a portal item
            var isOnline = MyMapView.Map.Item != null;
            TakeOffline.IsVisible = isOnline;
        }

        /// <summary>
        ///  Attempts to generate credential for ArcGIS Portal
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnSignInClicked(object sender, System.EventArgs e)
        {
            try
            {
                var username = Username.Text.Trim();
                var password = Password.Text.Trim();
                var credential = await AuthenticationManager.Current.GenerateCredentialAsync(new Uri("https://www.arcgis.com/sharing/rest"), username, password);
                _loginTask?.TrySetResult(credential);
            }
            catch (Exception ex)
            {
                _loginTask?.TrySetException(ex);
            }
        }

        /// <summary>
        /// Invokes ArcGIS Portal Login
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                LoginView.IsVisible = true;
            });
            Credential credential = null;
            try
            {
                if (_loginTask == null || _loginTask.Task.IsCompleted)
                    _loginTask = new TaskCompletionSource<Credential>();
                credential = await _loginTask.Task;
            }
            catch (TaskCanceledException)
            {
            }
            finally
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    LoginView.IsVisible = false;
                });
            }
            return credential;
        }
    }
}
