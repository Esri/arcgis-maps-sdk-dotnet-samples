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
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.WPF.Samples.LocalServerServices
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Local Server Services",
        "LocalServer",
        "This sample demonstrates how to control local server and manage running services.",
        "This sample depends on the local server being installed and configured. See https://developers.arcgis.com/net/latest/wpf/guide/local-server.htm for details and instructions. \n Sample data is downloaded automatically once local server is started. It may take some time for sample data to load. The list of services will be enabled once the download has finished.",
        "Featured")]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData("dee5d8060a6048a4b063484199a9546b", "4e94fec734434d1288e6ebe36c3c461f", "da9e565a52ca41c1937cff1a01017068")]
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

            // Set up the sample
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                // Subscribe to event notification for the local server instance
                LocalServer.Instance.StatusChanged += (o, e) =>
                {
                    UpdateUiWithServiceUpdate("Local Server", e.Status);
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Please ensure that local server is installed prior to using the sample. See instructions in readme.md or metadata.json. Message: {0}", ex.Message), "Local Server failed to start");
            }
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

        private void CreateServices()
        {
            try
            {
                // Arrange the data before starting the services
                string mapServicePath = GetMpkPath();
                string featureServicePath = GetFeatureLayerPath();
                string geoprocessingPath = GetGpPath();

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

        private async void StartServiceButtonClicked(object sender, RoutedEventArgs e)
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

        private async void StopServiceButtonClicked(object sender, RoutedEventArgs e)
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

        private static string GetMpkPath()
        {
            return DataManager.GetDataFolder("dee5d8060a6048a4b063484199a9546b", "RelationshipID.mpk");
        }

        private static string GetFeatureLayerPath()
        {
            return DataManager.GetDataFolder("4e94fec734434d1288e6ebe36c3c461f", "PointsOfInterest.mpk");
        }

        private static string GetGpPath()
        {
            return DataManager.GetDataFolder("da9e565a52ca41c1937cff1a01017068", "Contour.gpk");
        }

        private async void StartServerButtonClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                // Start the server
                await LocalServer.Instance.StartAsync();

                // Create the services
                CreateServices();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Local Server Failed to start");
            }

            // Update the UI
            LocalServerStopButton.IsEnabled = true;
            LocalServerStartButton.IsEnabled = false;
        }

        private async void StopServerButtonClicked(object sender, RoutedEventArgs e)
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

        private void NavigateButtonClicked(object sender, RoutedEventArgs e)
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
    }
}
