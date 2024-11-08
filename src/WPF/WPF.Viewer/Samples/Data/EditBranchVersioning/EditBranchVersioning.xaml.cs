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
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.EditBranchVersioning
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Edit with branch versioning",
        category: "Data",
        description: "Create, query and edit a specific server version using service geodatabase.",
        instructions: "Once loaded, the map will zoom to the extent of the feature layer. The current version is indicated at the top of the map. Click \"Create Version\" to open a dialog to specify the version information (name, access, and description). See the *Additional information* section for restrictions on the version name.",
        tags: new[] { "branch versioning", "edit", "version control", "version management server" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class EditBranchVersioning
    {
        private ArcGISFeature _selectedFeature;
        private FeatureLayer _featureLayer;
        private ServiceFeatureTable _featureTable;
        private ServiceGeodatabase _serviceGeodatabase;

        private string _userCreatedVersionName;
        private string _attributeFieldName = "TYPDAMAGE";

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
                    string sampleServer7Pass = "S7#i2LWmYH75";
                    return await AccessTokenCredential.CreateAsync(info.ServiceUri, sampleServer7User, sampleServer7Pass);
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
                _serviceGeodatabase = new ServiceGeodatabase(new Uri("https://sampleserver7.arcgisonline.com/server/rest/services/DamageAssessment/FeatureServer/0"));
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

                // Enable the UI.
                CreateVersionButton.IsEnabled = true;
                MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
            }
            catch (Exception ex)
            {
                ShowAlert(ex.Message, ex.GetType().Name);
            }
        }

        private void VersionButtonPressed(object sender, RoutedEventArgs e)
        {
            // Check if user version has been created.
            if (_userCreatedVersionName != null)
            {
                _ = SwitchVersion();
            }
            else
            {
                // Display UI for creating a new version.
                VersionCreator.Visibility = Visibility.Visible;
            }

            // Ensure the attribute picker is closed.
            AttributePicker.Visibility = Visibility.Collapsed;

            // Clear the selection.
            _featureLayer.ClearSelection();
            _selectedFeature = null;
        }

        private async Task SwitchVersion()
        {
            // Check if the default version is loaded.
            if (_serviceGeodatabase.VersionName == _serviceGeodatabase.DefaultVersionName)
            {
                // Switch to the user created version.
                await _serviceGeodatabase.SwitchVersionAsync(_userCreatedVersionName);
                CurrentVersionLabel.Content = $"Current version: {_serviceGeodatabase.VersionName}";
            }
            else
            {
                if (_serviceGeodatabase.HasLocalEdits())
                {
                    // Apply the edits made to the service geodatabase.
                    IReadOnlyList<FeatureTableEditResult> edits = await _serviceGeodatabase.ApplyEditsAsync();
                    if (!(edits?.Count > 0))
                    {
                        // Verify that there were no errors when applying edits.
                        if (!edits.ToArray()[0].EditResults[0].CompletedWithErrors)
                        {
                            ShowAlert("Applied edits successfully on the server");
                        }
                        else
                        {
                            ShowAlert(edits.ToArray()[0].EditResults[0].Error.Message);
                            return;
                        }
                    }
                }

                // Switch to the default version.
                await _serviceGeodatabase.SwitchVersionAsync(_serviceGeodatabase.DefaultVersionName);
                CurrentVersionLabel.Content = $"Current version: {_serviceGeodatabase.VersionName}";
            }
        }

        private bool VersionNameValid(string versionName)
        {
            // Verify that the version name is valid.
            if (versionName.Contains('.') || versionName.Contains(';') || versionName.Contains('\'') || versionName.Contains('\"'))
            {
                ShowAlert("Please enter a valid version name.\nThe name cannot contain the following characters:\n. ; ' \" ");
                return false;
            }
            else if (versionName.Length > 0 && versionName.StartsWith(" "))
            {
                ShowAlert("Version name cannot begin with a space");
                return false;
            }
            else if (versionName.Length > 62)
            {
                ShowAlert("Version name must not exceed 62 characters");
                return false;
            }
            else if (versionName.Length == 0)
            {
                ShowAlert("Please enter a version name");
                return false;
            }
            return true;
        }

        private void ShowAlert(string alertText, string titleText = "Alert")
        {
            MessageBox.Show(alertText, titleText, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Check if a feature is selected and the service geodatabase is not on the default version.
            if (_selectedFeature is ArcGISFeature && _serviceGeodatabase.VersionName != _serviceGeodatabase.DefaultVersionName)
            {
                try
                {
                    // Load the feature.
                    await _selectedFeature.LoadAsync();

                    // Update the feature geometry.
                    _selectedFeature.Geometry = e.Location;

                    // Update the table.
                    await _selectedFeature.FeatureTable.UpdateFeatureAsync(_selectedFeature);

                    // Update the service.
                    await ((ServiceFeatureTable)_selectedFeature.FeatureTable).ApplyEditsAsync();

                    ShowAlert("Moved feature " + _selectedFeature.Attributes["objectid"]);
                }
                catch (Exception ex)
                {
                    ShowAlert(ex.Message, "Failed to edit feature");
                }
            }
            else
            {
                try
                {
                    // Perform an identify to determine if a user tapped on a feature.
                    IdentifyLayerResult identifyResult = await MyMapView.IdentifyLayerAsync(_featureLayer, e.Position, 2, false);

                    // Do nothing if there are no results.
                    if (!identifyResult.GeoElements.Any())
                    {
                        return;
                    }

                    // Get the tapped feature.
                    _selectedFeature = (ArcGISFeature)identifyResult.GeoElements.First();

                    // Select the feature.
                    _featureLayer.SelectFeature(_selectedFeature);

                    // Get the current value.
                    string currentAttributeValue = _selectedFeature.Attributes[_attributeFieldName].ToString();

                    // Update the combobox selection without triggering the event.
                    DamageBox.SelectionChanged -= DamageBox_SelectionChanged;
                    DamageBox.SelectedValue = currentAttributeValue;
                    DamageBox.SelectionChanged += DamageBox_SelectionChanged;

                    // Update the UI for the selection.
                    AttributePicker.Visibility = Visibility.Visible;

                    DamageBox.IsEnabled = _serviceGeodatabase.VersionName != _serviceGeodatabase.DefaultVersionName;
                }
                catch (Exception ex)
                {
                    ShowAlert(ex.Message, ex.GetType().Name);
                }
            }
        }

        private async void DamageBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                // Get the new value.
                string selectedAttributeValue = DamageBox.SelectedValue.ToString();

                // Load the feature.
                await _selectedFeature.LoadAsync();

                // Update the attribute value.
                _selectedFeature.SetAttributeValue(_attributeFieldName, selectedAttributeValue);

                // Update the table.
                await _selectedFeature.FeatureTable.UpdateFeatureAsync(_selectedFeature);

                // Update the service.
                await ((ServiceFeatureTable)_selectedFeature.FeatureTable).ApplyEditsAsync();

                AttributePicker.Visibility = Visibility.Collapsed;
                ShowAlert("Edited feature " + _selectedFeature.Attributes["objectid"]);

                // Clear the selection.
                _featureLayer.ClearSelection();
                _selectedFeature = null;
            }
            catch (Exception ex)
            {
                ShowAlert(ex.Message, "Failed to edit feature");
            }
        }

        private async void ConfirmVersionClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate name and access input.
                if (!VersionNameValid(NameEntryBox.Text))
                {
                    return;
                }

                if (!(AccessBox.SelectedItem is VersionAccess))
                {
                    ShowAlert("Please select an access level");
                    return;
                }

                // Set the user defined name, access level and description as service version parameters
                ServiceVersionParameters newVersionParameters = new ServiceVersionParameters();
                newVersionParameters.Name = NameEntryBox.Text;
                newVersionParameters.Access = (VersionAccess)AccessBox.SelectedItem;
                newVersionParameters.Description = DescriptionBox.Text ?? "";

                ServiceVersionInfo newVersion = await _serviceGeodatabase.CreateVersionAsync(newVersionParameters);
                _userCreatedVersionName = newVersion.Name;
                _ = SwitchVersion();

                CreateVersionButton.Content = "Switch version";
            }
            catch (Exception ex)
            {
                ShowAlert(ex.Message, ex.GetType().Name);
            }
            finally
            {
                VersionCreator.Visibility = Visibility.Collapsed;
            }
        }

        private void CancelVersionClick(object sender, RoutedEventArgs e) => VersionCreator.Visibility = Visibility.Collapsed;

        private void CloseAttributeClick(object sender, RoutedEventArgs e)
        {
            AttributePicker.Visibility = Visibility.Collapsed;

            // Clear the selection.
            _featureLayer.ClearSelection();
            _selectedFeature = null;
        }
    }
}