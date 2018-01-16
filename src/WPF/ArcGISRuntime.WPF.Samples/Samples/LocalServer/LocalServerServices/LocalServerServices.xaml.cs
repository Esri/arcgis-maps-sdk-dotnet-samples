// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.LocalServices;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.WPF.Samples.LocalServerServices
{
    public partial class LocalServerServices
    {
        // Hold references to the individual services
        private LocalMapService _localMapService;
        private LocalFeatureService _localFeatureService;
        private LocalGeoprocessingService _localGeoprocessingService;

        // Generic reference to the service selected in the UI
        private LocalService _selectedService;

        public LocalServerServices()
        {
            InitializeComponent();

            // set up the sample
            Initialize();
        }

        private void Initialize()
        {
            // Subscribe to event notification for the local server instance
            LocalServer.Instance.StatusChanged += (o, e) => { UpdateUiWithServiceUpdate("Local Server", e.Status); };

            // Listen for the shutdown and unloaded events so that the local server can be shut down
            this.Dispatcher.ShutdownStarted += ShutdownSample;
            this.Unloaded += ShutdownSample;
        }

        private void UpdateUiWithServiceUpdate(string server, LocalServerStatus status)
        {
            // Construct the new status text
            string updateStatus = String.Format("{0} status: {1} \t\t {2}\n{3}", server, status,
                DateTime.Now.ToShortTimeString(), txtStatusBox.Text);

            // Update the status box text
            txtStatusBox.Text = updateStatus;

            // Update the list of running services
            lstServices.ItemsSource = LocalServer.Instance.Services.Select(m => m.Name + " : " + m.Url);
        }

        private async Task CreateServices()
        {
            try
            {
                // Arrange the data before starting the services
                string mapServicePath = await GetMpkPath();
                string featureServicePath = await GetFeatureLayerPath();
                string geoprocessingPath = await GetGpPath();

                // Create each service but don't start any 
                _localMapService = new LocalMapService(mapServicePath);
                _localFeatureService = new LocalFeatureService(featureServicePath);
                _localGeoprocessingService = new LocalGeoprocessingService(geoprocessingPath);

                // Subscribe to status updates for each service
                _localMapService.StatusChanged += (o, e) => { UpdateUiWithServiceUpdate("Map Service", e.Status); };
                _localFeatureService.StatusChanged += (o, e) => { UpdateUiWithServiceUpdate("Feature Service", e.Status); };
                _localGeoprocessingService.StatusChanged += (o, e) => { UpdateUiWithServiceUpdate("Geoprocessing Service", e.Status); };

                // Enable the UI to select services
                comboServiceSelect.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Failed to create services");
            }
        }

        

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the text of the selected item
            string selection = ((ComboBoxItem)comboServiceSelect.SelectedItem).Content.ToString();

            // Update the selection
            switch (selection)
            {
                case "Map Service":
                    _selectedService = _localMapService;
                    break;

                case "Feature Service":
                    _selectedService = _localFeatureService;
                    break;

                case "Geoprocessing Service":
                    _selectedService = _localGeoprocessingService;
                    break;
            }

            // Return if selection is invalid
            if (_selectedService == null)
            {
                return;
            }

            // Update the state of the start and stop buttons
            UpdateServiceControlUi();
        }

        private void UpdateServiceControlUi()
        {
            if (_selectedService == null)
            {
                return;
            }

            // Update the UI
            if (_selectedService.Status == LocalServerStatus.Started)
            {
                btnServiceStop.IsEnabled = true;
                btnServiceStart.IsEnabled = false;
            }
            else
            {
                btnServiceStop.IsEnabled = false;
                btnServiceStart.IsEnabled = true;
            }
        }

        private async void BtnStartServiceClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Start the selected service
                await _selectedService.StartAsync();

                // Update the UI
                UpdateServiceControlUi();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Failed to start service");
            }
        }

        private async void BtnStopServiceClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Stop the selected service
                await _selectedService.StopAsync();

                // Update the UI
                UpdateServiceControlUi();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Failed to stop service");
            }
        }

        private async Task<string> GetMpkPath()
        {
            #region offlinedata

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "LocalServerServices", "RelationshipID.mpk");

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                // Download the file
                await DataManager.GetData("dee5d8060a6048a4b063484199a9546b", "LocalServerServices");
            }

            return filepath;

            #endregion offlinedata
        }

        private async Task<string> GetFeatureLayerPath()
        {
            #region offlinedata

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "LocalServerServices", "PointsOfInterest.mpk");

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                // Download the file
                await DataManager.GetData("4e94fec734434d1288e6ebe36c3c461f", "LocalServerServices");
            }

            return filepath;

            #endregion offlinedata
        }

        private async Task<string> GetGpPath()
        {
            #region offlinedata

            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "LocalServerServices", "Contour.gpk");

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                // Download the file
                await DataManager.GetData("da9e565a52ca41c1937cff1a01017068", "LocalServerServices");
            }

            return filepath;

            #endregion offlinedata
        }

        private async void BtnStartServer(object sender, RoutedEventArgs e)
        {
            try
            {
                // Start the server
                await LocalServer.Instance.StartAsync();

                // Create the services
                await CreateServices();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Local Server Failed to start");
            }

            // Update the UI
            LocalServerStopButton.IsEnabled = true;
            LocalServerStartButton.IsEnabled = false;
        }

        private async void BtnStopServer(object sender, RoutedEventArgs e)
        {
            // Update the UI
            btnServiceStart.IsEnabled = false;
            btnServiceStop.IsEnabled = false;
            LocalServerStartButton.IsEnabled = true;
            LocalServerStopButton.IsEnabled = false;

            // Stop the server
            await LocalServer.Instance.StopAsync();

            // Clear references to the services
            _localFeatureService = null;
            _localMapService = null;
            _localGeoprocessingService = null;
        }

        private void NavgiateButtonOnClick(object sender, RoutedEventArgs e)
        {
            // Return if selection is empty
            if (lstServices.SelectedItems.Count < 1) { return; }

            try
            {
                // Get the full text in the selection
                string strFullName = lstServices.SelectedItems[0].ToString();

                // Create array of characters to split text by; ':' separates the service name and the URI
                char[] splitChars = { ':' };

                // Extract the service URL
                string serviceUri = strFullName.Split(splitChars, 2)[1].Trim();

                // Navigate to the service
                System.Diagnostics.Process.Start(serviceUri);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Couldn't navigate to service");
            }
        }

        private async void ShutdownSample(object sender, EventArgs e)
        {
            try
            {
                // Shut down the local server if it has started
                if (LocalServer.Instance.Status == LocalServerStatus.Started)
                {
                    await LocalServer.Instance.StopAsync();
                }
            }
            catch (InvalidOperationException)
            {
                // Local server isn't installed, just return
                return;
            }
        }
    }
}