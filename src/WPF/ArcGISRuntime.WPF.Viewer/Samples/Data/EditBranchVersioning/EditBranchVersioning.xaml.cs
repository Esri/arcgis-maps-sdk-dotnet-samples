// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGISRuntime.WPF.Samples.EditBranchVersioning
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Edit with branch versioning",
        "Data",
        "Create, query and edit a specific server version using service geodatabase.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public partial class EditBranchVersioning
    {
        //https://sampleserver7.arcgisonline.com/arcgis/rest/services/DamageAssessment/FeatureServer/0

        private Feature _selectedFeature;
        private FeatureLayer _featureLayer;
        private ServiceFeatureTable _featureTable;
        private ServiceGeodatabase _serviceGeodatabase;

        public EditBranchVersioning()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Handle the login to the feature service.
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(async (info) =>
            {
                try
                {
                    // WARNING: Never hardcode login information in a production application. This is done solely for the sake of the sample.
                    string sampleServer7User = "editor01";
                    string sampleServer7Pass = "editor01.password";
                    return await AuthenticationManager.Current.GenerateCredentialAsync(info.ServiceUri, sampleServer7User, sampleServer7Pass);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return null;
                }
            });

            try
            {
                // Create a map.
                MyMapView.Map = new Map(BasemapStyle.ArcGISStreets);

                // Populate the combo boxes.
                AccessBox.ItemsSource = new List<VersionAccess> { VersionAccess.Public, VersionAccess.Protected, VersionAccess.Private };
                DamageBox.ItemsSource = new List<string> { "Destroyed", "Inaccessible", "Major", "Minor", "Affected" };

                // Create and load a service geodatabase.
                _serviceGeodatabase = new ServiceGeodatabase(new Uri("https://sampleserver7.arcgisonline.com/arcgis/rest/services/DamageAssessment/FeatureServer/0"));
                await _serviceGeodatabase.LoadAsync();

                // When the service geodatabase has loaded get the default version name.
                CurrentVersionLabel.Content = $"Current version: {_serviceGeodatabase.DefaultVersionName}";

                // Get the service feature table from the service geodatabase.
                _featureTable = _serviceGeodatabase.GetTable(0);

                // Create a feature layer from the service feature table and add it to the map.
                _featureLayer = new FeatureLayer(_featureTable);
                MyMapView.Map.OperationalLayers.Add(_featureLayer);
                await _featureLayer.LoadAsync();

                // When the feature layer has loaded set the viewpoint and update the UI.
                await MyMapView.SetViewpointAsync(new Viewpoint(_featureLayer.FullExtent));
                CurrentVersionLabel.Content = $"Current version: {_serviceGeodatabase.VersionName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SwitchVersion(object sender, RoutedEventArgs e)
        {
            VersionPicker.Visibility = Visibility.Visible;
        }

        private void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            
        }

        private void DamageBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
        }

        private void ConfirmClick(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            finally
            {
                VersionPicker.Visibility = Visibility.Collapsed;
            }
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            finally
            {
                VersionPicker.Visibility = Visibility.Collapsed;
            }
        }
    }
}