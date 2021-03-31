// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.EditBranchVersioning
{
    [Register("EditBranchVersioning")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Edit with branch versioning",
        category: "Data",
        description: "Create, query and edit a specific server version using service geodatabase.",
        instructions: "Once loaded, the map will zoom to the extent of the feature layer. The current version is indicated at the top of the map. Tap \"Create Version\" to open a dialog to specify the version information (name, access, and description). See the *Additional information* section for restrictions on the version name.",
        tags: new[] { "branch versioning", "edit", "version control", "version management server" })]
    public class EditBranchVersioning : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;

        private UIStackView _damageView;
        private UIStackView _defaultView;
        private UIStackView _versionView;
        private UIView _topView;

        private UILabel _versionLabel;
        private UIButton _versionButton;

        private UITextField _nameField;
        private UIButton _proButton;
        private UITextField _descriptionField;
        private UIButton _confirmButton;
        private UIButton _cancelButton;

        private UILabel _moveText;
        private UIButton _damageButton;
        private UIButton _closeButton;

        private ArcGISFeature _selectedFeature;
        private FeatureLayer _featureLayer;
        private ServiceFeatureTable _featureTable;
        private ServiceGeodatabase _serviceGeodatabase;

        private VersionAccess? _userVersionAccess;

        private string _userCreatedVersionName;
        private string _userDamageLevel;
        private string _attributeFieldName = "typdamage";

        private List<VersionAccess> _accessLevels = new List<VersionAccess> { VersionAccess.Public, VersionAccess.Protected, VersionAccess.Private };
        private List<string> _damageLevels = new List<string> { "Destroyed", "Inaccessible", "Major", "Minor", "Affected" };

        public EditBranchVersioning()
        {
            Title = "Edit with branch versioning";
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
                _myMapView.Map = new Map(BasemapStyle.ArcGISStreets);

                // Create and load a service geodatabase.
                _serviceGeodatabase = new ServiceGeodatabase(new Uri("https://sampleserver7.arcgisonline.com/server/rest/services/DamageAssessment/FeatureServer/0"));
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
            new UIAlertView(titleText, alertText, (IUIAlertViewDelegate)null, "Ok", null).Show();
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

                    // Update the damage button.
                    _damageButton.SetTitle($"Damage: {currentAttributeValue}", UIControlState.Normal);

                    _moveText.Hidden = _serviceGeodatabase.VersionName == _serviceGeodatabase.DefaultVersionName;
                    _damageButton.Enabled = !_moveText.Hidden;

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
                string selectedAttributeValue = _userDamageLevel;

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
                if (!VersionNameValid(_nameField.Text))
                {
                    return;
                }

                if (_userVersionAccess is VersionAccess access)
                {
                    // Set the user defined name, access level and description as service version parameters
                    ServiceVersionParameters newVersionParameters = new ServiceVersionParameters();
                    newVersionParameters.Name = _nameField.Text;
                    newVersionParameters.Access = access;
                    newVersionParameters.Description = _descriptionField.Text ?? "";

                    ServiceVersionInfo newVersion = await _serviceGeodatabase.CreateVersionAsync(newVersionParameters);
                    _userCreatedVersionName = newVersion.Name;
                    _ = SwitchVersion();

                    _versionButton.SetTitle("Switch version", UIControlState.Normal);
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

        private void ProtectionButtonClick(object sender, EventArgs e)
        {
            // Create prompt for the protection level.
            UIAlertController prompt = UIAlertController.Create("Choose protection level", string.Empty, UIAlertControllerStyle.ActionSheet);
            foreach (VersionAccess access in _accessLevels) prompt.AddAction(UIAlertAction.Create(Enum.GetName(typeof(VersionAccess), access), UIAlertActionStyle.Default, (o) => SetVersionAccess(access)));
            prompt.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

            // Needed to prevent crash on iPad.
            UIPopoverPresentationController ppc = prompt.PopoverPresentationController;
            if (ppc != null)
            {
                ppc.SourceView = _proButton;
                ppc.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Present the prompt to the user.
            PresentViewController(prompt, true, null);
        }

        private void SetVersionAccess(VersionAccess access)
        {
            _userVersionAccess = access;
            _proButton.SetTitle($"Protection: {access}", UIControlState.Normal);
        }

        private void ChangeDamage(object sender, EventArgs e)
        {
            // Create prompt for the damage level.
            UIAlertController prompt = UIAlertController.Create("Change damage level", string.Empty, UIAlertControllerStyle.ActionSheet);
            foreach (string damageLevel in _damageLevels) prompt.AddAction(UIAlertAction.Create(damageLevel, UIAlertActionStyle.Default, (o) => ChangeUserDamage(damageLevel)));
            prompt.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

            // Needed to prevent crash on iPad.
            UIPopoverPresentationController ppc = prompt.PopoverPresentationController;
            if (ppc != null)
            {
                ppc.SourceView = _proButton;
                ppc.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            // Present the prompt to the user.
            PresentViewController(prompt, true, null);
        }

        private void ChangeUserDamage(string damageLevel)
        {
            _userDamageLevel = damageLevel;
            _damageButton.SetTitle($"Damage: {damageLevel}", UIControlState.Normal);
        }

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

        private void SwitchView(UIView view)
        {
            foreach (var sv in _topView.Subviews) sv.Hidden = true;
            view.Hidden = false;
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView { BackgroundColor = ApplicationTheme.BackgroundColor };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _topView = new UIView
            {
                BackgroundColor = ApplicationTheme.BackgroundColor,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            _defaultView = new UIStackView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Spacing = 8,
                LayoutMarginsRelativeArrangement = true,
                Alignment = UIStackViewAlignment.Top,
                LayoutMargins = new UIEdgeInsets(8, 8, 8, 8),
                Axis = UILayoutConstraintAxis.Vertical,
            };

            UILabel instructions = new UILabel { TranslatesAutoresizingMaskIntoConstraints = false, Text = "Tap to select a feature." };

            _versionLabel = new UILabel { TranslatesAutoresizingMaskIntoConstraints = false, };

            _versionButton = new UIButton { TranslatesAutoresizingMaskIntoConstraints = false, Enabled = false };
            _versionButton.SetTitle("Create version", UIControlState.Normal);
            _versionButton.SetTitleColor(View.TintColor, UIControlState.Normal);

            _defaultView.AddArrangedSubview(instructions);
            _defaultView.AddArrangedSubview(_versionLabel);
            _defaultView.AddArrangedSubview(_versionButton);

            _versionView = new UIStackView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Spacing = 8,
                LayoutMarginsRelativeArrangement = true,
                Alignment = UIStackViewAlignment.Top,
                LayoutMargins = new UIEdgeInsets(8, 8, 8, 8),
                Axis = UILayoutConstraintAxis.Vertical,
                Hidden = true,
            };

            _nameField = new UITextField { Placeholder = "Name", };

            _proButton = new UIButton { };
            _proButton.SetTitle("Protection:", UIControlState.Normal);
            _proButton.SetTitleColor(View.TintColor, UIControlState.Normal);

            _descriptionField = new UITextField { Placeholder = "Description" };

            _confirmButton = new UIButton { };
            _confirmButton.SetTitle("Confirm", UIControlState.Normal);
            _confirmButton.SetTitleColor(View.TintColor, UIControlState.Normal);

            _cancelButton = new UIButton { };
            _cancelButton.SetTitle("Cancel", UIControlState.Normal);
            _cancelButton.SetTitleColor(UIColor.Red, UIControlState.Normal);

            _versionView.AddArrangedSubview(_nameField);
            _versionView.AddArrangedSubview(_proButton);
            _versionView.AddArrangedSubview(_descriptionField);
            _versionView.AddArrangedSubview(getRowStackView(new UIView[] { _confirmButton, _cancelButton }));

            _damageView = new UIStackView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Spacing = 8,
                LayoutMarginsRelativeArrangement = true,
                Alignment = UIStackViewAlignment.Top,
                LayoutMargins = new UIEdgeInsets(8, 8, 8, 8),
                Axis = UILayoutConstraintAxis.Vertical,
                Hidden = true,
            };

            _moveText = new UILabel { TranslatesAutoresizingMaskIntoConstraints = false, Text = "Tap to move feature." };

            _damageButton = new UIButton { };
            _damageButton.SetTitle("Damage:", UIControlState.Normal);
            _damageButton.SetTitleColor(View.TintColor, UIControlState.Normal);

            _closeButton = new UIButton { };
            _closeButton.SetTitle("Close", UIControlState.Normal);
            _closeButton.SetTitleColor(UIColor.Red, UIControlState.Normal);

            _damageView.AddArrangedSubview(_moveText);
            _damageView.AddArrangedSubview(_damageButton);
            _damageView.AddArrangedSubview(_closeButton);

            _topView.AddSubviews(_defaultView, _versionView, _damageView);

            // Add the views.
            View.AddSubviews(_myMapView, _topView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _topView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _topView.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor),
                _topView.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor),
                _topView.HeightAnchor.ConstraintEqualTo(150),

                _myMapView.TopAnchor.ConstraintEqualTo(_topView.BottomAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
            });
        }

        private UIStackView getRowStackView(UIView[] views)
        {
            UIStackView row = new UIStackView(views);
            row.TranslatesAutoresizingMaskIntoConstraints = false;
            row.Spacing = 8;
            row.Axis = UILayoutConstraintAxis.Horizontal;
            row.Distribution = UIStackViewDistribution.FillEqually;
            return row;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _ = Initialize();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            // Subscribe to events.
            _versionButton.TouchUpInside += VersionButtonClick;
            _proButton.TouchUpInside += ProtectionButtonClick;
            _confirmButton.TouchUpInside += ConfirmButtonClick;
            _cancelButton.TouchUpInside += CancelButtonClick;
            _damageButton.TouchUpInside += ChangeDamage;
            _closeButton.TouchUpInside += CloseDamage;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _versionButton.TouchUpInside -= VersionButtonClick;
            _proButton.TouchUpInside -= ProtectionButtonClick;
            _confirmButton.TouchUpInside -= ConfirmButtonClick;
            _cancelButton.TouchUpInside -= CancelButtonClick;
            _damageButton.TouchUpInside -= ChangeDamage;
            _closeButton.TouchUpInside -= CloseDamage;
            _myMapView.GeoViewTapped -= MapView_GeoViewTapped;
        }
    }
}