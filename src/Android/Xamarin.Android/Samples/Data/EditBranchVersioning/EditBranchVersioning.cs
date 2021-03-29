// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using ArcGISRuntime;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGISRuntimeXamarin.Samples.EditBranchVersioning
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Edit with branch versioning",
        category: "Data",
        description: "Create, query and edit a specific server version using service geodatabase.",
        instructions: "Once loaded, the map will zoom to the extent of the feature layer. The current version is indicated at the top of the map. Tap \"Create Version\" to open a dialog to specify the version information (name, access, and description). See the *Additional information* section for restrictions on the version name.",
        tags: new[] { "branch versioning", "edit", "version control", "version management server" })]
    [ArcGISRuntime.Samples.Shared.Attributes.AndroidLayout("EditBranchVersioning.xml")]
    public class EditBranchVersioning : Activity
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private TextView _versionLabel;
        private TextView _moveLabel;
        private Button _versionButton;
        private Button _cancelButton;
        private Button _closeButton;
        private Button _confirmButton;
        private Spinner _protectionSpinner;
        private Spinner _damageSpinner;
        private EditText _nameEntry;
        private EditText _descriptionEntry;

        private View _defaultView;
        private View _versionView;
        private View _damageView;

        private ArcGISFeature _selectedFeature;
        private FeatureLayer _featureLayer;
        private ServiceFeatureTable _featureTable;
        private ServiceGeodatabase _serviceGeodatabase;

        private string _userCreatedVersionName;
        private string _attributeFieldName = "typdamage";

        private List<VersionAccess> _accessLevels = new List<VersionAccess> { VersionAccess.Public, VersionAccess.Protected, VersionAccess.Private };
        private List<string> _damageLevels = new List<string> { "Destroyed", "Inaccessible", "Major", "Minor", "Affected" };

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Edit with branch versioning";

            CreateLayout();
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
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    return null;
                }
            });

            try
            {
                // Create a map.
                _myMapView.Map = new Map(BasemapStyle.ArcGISStreets);

                _damageSpinner.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, _damageLevels);
                _protectionSpinner.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, _accessLevels.Select(l => Enum.GetName(typeof(VersionAccess), l)).ToList());

                // Create and load a service geodatabase.
                _serviceGeodatabase = new ServiceGeodatabase(new Uri("https://sampleserver7.arcgisonline.com/arcgis/rest/services/DamageAssessment/FeatureServer/0"));
                await _serviceGeodatabase.LoadAsync();

                // When the service geodatabase has loaded get the default version name.
                _versionLabel.Text = $"Current version: {_serviceGeodatabase.DefaultVersionName}";

                // Get the service feature table from the service geodatabase.
                _featureTable = _serviceGeodatabase.GetTable(0);

                // Create a feature layer from the service feature table and add it to the map.
                _featureLayer = new FeatureLayer(_featureTable);
                _myMapView.Map.OperationalLayers.Add(_featureLayer);
                await _featureLayer.LoadAsync();

                // When the feature layer has loaded set the viewpoint and update the UI.
                await _myMapView.SetViewpointAsync(new Viewpoint(_featureLayer.FullExtent));

                // Enable the UI.
                _versionButton.Enabled = true;
                _myMapView.GeoViewTapped += MapView_GeoViewTapped;
            }
            catch (Exception ex)
            {
                ShowAlert(ex.Message, ex.GetType().Name);
            }
        }

        private void VersionButtonClick(object sender, EventArgs e)
        {
            // Check if user version has been created.
            if (_userCreatedVersionName != null)
            {
                _ = SwitchVersion();
            }
            else
            {
                // Display UI for creating a new version.
                SwitchView(_versionView);
            }

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
                _versionLabel.Text = $"Current version: {_serviceGeodatabase.VersionName}";
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
                _versionLabel.Text = $"Current version: {_serviceGeodatabase.VersionName}";
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
            new AlertDialog.Builder(this).SetMessage(alertText).SetTitle(titleText).Show();
        }

        private async void MapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
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
                    // Clear the selection.
                    _featureLayer.ClearSelection();
                    _selectedFeature = null;

                    // Perform an identify to determine if a user tapped on a feature.
                    IdentifyLayerResult identifyResult = await _myMapView.IdentifyLayerAsync(_featureLayer, e.Position, 2, false);

                    // Do nothing if there are no results.
                    if (!identifyResult.GeoElements.Any())
                    {
                        SwitchView(_defaultView);
                        return;
                    }

                    // Get the tapped feature.
                    _selectedFeature = (ArcGISFeature)identifyResult.GeoElements.First();

                    // Select the feature.
                    _featureLayer.SelectFeature(_selectedFeature);

                    // Get the current value.
                    string currentAttributeValue = _selectedFeature.Attributes[_attributeFieldName].ToString();

                    bool editable = _serviceGeodatabase.VersionName != _serviceGeodatabase.DefaultVersionName;
                    _moveLabel.Visibility = editable ? ViewStates.Visible : ViewStates.Invisible;
                    _damageSpinner.Enabled = editable;

                    // Update the UI for the selection.
                    SwitchView(_damageView);
                }
                catch (Exception ex)
                {
                    ShowAlert(ex.Message, ex.GetType().Name);
                }
            }
        }

        private async Task ApplyDamageChange()
        {
            try
            {
                // Get the value from the UI.
                string selectedAttributeValue = _damageSpinner.SelectedItem.ToString();

                // Check if the new value is the same as the existing value.
                if (_selectedFeature.Attributes[_attributeFieldName].ToString() == selectedAttributeValue)
                {
                    return;
                }

                // Load the feature.
                await _selectedFeature.LoadAsync();

                // Update the attribute value.
                _selectedFeature.SetAttributeValue(_attributeFieldName, selectedAttributeValue);

                // Update the table.
                await _selectedFeature.FeatureTable.UpdateFeatureAsync(_selectedFeature);

                // Update the service.
                await ((ServiceFeatureTable)_selectedFeature.FeatureTable).ApplyEditsAsync();

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

        private async void ConfirmButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Validate name and access input.
                if (!VersionNameValid(_nameEntry.Text))
                {
                    return;
                }

                if (_protectionSpinner.SelectedItem != null)
                {
                    VersionAccess access = (VersionAccess)Enum.Parse(typeof(VersionAccess), _protectionSpinner.SelectedItem.ToString());

                    // Set the user defined name, access level and description as service version parameters
                    ServiceVersionParameters newVersionParameters = new ServiceVersionParameters();
                    newVersionParameters.Name = _nameEntry.Text;
                    newVersionParameters.Access = access;
                    newVersionParameters.Description = _descriptionEntry.Text ?? "";

                    ServiceVersionInfo newVersion = await _serviceGeodatabase.CreateVersionAsync(newVersionParameters);
                    _userCreatedVersionName = newVersion.Name;
                    _ = SwitchVersion();

                    _versionButton.Text = "Switch version";
                }
                else
                {
                    ShowAlert("Please select an access level");
                    return;
                }
            }
            catch (Exception ex)
            {
                ShowAlert(ex.Message, ex.GetType().Name);
            }
            finally
            {
                SwitchView(_defaultView);
            }
        }

        private void CancelButtonClick(object sender, EventArgs e) => SwitchView(_defaultView);

        private async void CloseDamage(object sender, EventArgs e)
        {
            SwitchView(_defaultView);

            if (_serviceGeodatabase.VersionName != _serviceGeodatabase.DefaultVersionName)
            {
                await ApplyDamageChange();
            }

            // Clear the selection.
            _featureLayer.ClearSelection();
            _selectedFeature = null;
        }

        private void SwitchView(View view)
        {
            _defaultView.Visibility = _versionView.Visibility = _damageView.Visibility = ViewStates.Gone;
            view.Visibility = ViewStates.Visible;
        }

        private void CreateLayout()
        {
            // Load the layout from the axml resource.
            SetContentView(Resource.Layout.EditBranchVersioning);

            _myMapView = FindViewById<MapView>(Resource.Id.MapView);
            _versionLabel = FindViewById<TextView>(Resource.Id.versionLabel);
            _moveLabel = FindViewById<TextView>(Resource.Id.moveLabel);
            _versionButton = FindViewById<Button>(Resource.Id.versionButton);
            _cancelButton = FindViewById<Button>(Resource.Id.cancelButton);
            _closeButton = FindViewById<Button>(Resource.Id.closeButton);
            _confirmButton = FindViewById<Button>(Resource.Id.createButton);
            _protectionSpinner = FindViewById<Spinner>(Resource.Id.protectionSpinner);
            _damageSpinner = FindViewById<Spinner>(Resource.Id.damageSpinner);
            _nameEntry = FindViewById<EditText>(Resource.Id.nameEditText);
            _descriptionEntry = FindViewById<EditText>(Resource.Id.descriptionEditText);

            _defaultView = FindViewById<LinearLayout>(Resource.Id.defaultView);
            _damageView = FindViewById<LinearLayout>(Resource.Id.damageView);
            _versionView = FindViewById<LinearLayout>(Resource.Id.versionView);

            _versionButton.Click += VersionButtonClick;
            _cancelButton.Click += CancelButtonClick;
            _closeButton.Click += CloseDamage;
            _confirmButton.Click += ConfirmButtonClick;
        }
    }
}