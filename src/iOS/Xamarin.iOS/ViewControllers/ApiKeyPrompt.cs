// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Shared.Managers;
using CoreGraphics;
using Foundation;
using System;
using System.IO;
using System.Threading.Tasks;
using UIKit;
using WebKit;

namespace ArcGISRuntime
{
    [Register("ApiKeyPrompt")]
    public class ApiKeyPrompt : UIViewController
    {
        private UIButton _setKeyButton;
        private UIButton _deleteKeyButton;
        private UIButton _storeKeyButton;

        private UILabel _currentKeyLabel;
        private UILabel _statusLabel;

        private WKWebView _infoText;

        // Directory for loading HTML locally.
        private string _contentDirectoryPath = Path.Combine(NSBundle.MainBundle.BundlePath, "Content/");

        private UITextField _keyEntry;

        public ApiKeyPrompt()
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _currentKeyLabel.Text = Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey;
            _ = UpdateValidityText();

            LoadHTML();
        }

        private void LoadHTML()
        {
            // Create the API key info section.
            string keyInfoPath = Path.Combine(NSBundle.MainBundle.BundlePath, "ApiKeyInfo.md");
            string keyInfoContent = File.ReadAllText(keyInfoPath);
            string keyInfoHTML = HTMLHelpers.MarkdownToHTML(keyInfoContent, TraitCollection);

            _infoText.LoadHtmlString(keyInfoHTML, new NSUrl(_contentDirectoryPath, true));
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            // Reload the html pages when switching to and from dark mode.
            if (previousTraitCollection.UserInterfaceStyle != TraitCollection.UserInterfaceStyle) LoadHTML();
        }

        private async Task UpdateValidityText()
        {
            ApiKeyStatus status = await ApiKeyManager.CheckKeyValidity();
            if (status == ApiKeyStatus.Valid)
            {
                _statusLabel.Text = "API key is valid";
            }
            else
            {
                _statusLabel.Text = "API key is invalid";
            }
            _currentKeyLabel.Text = Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey;
        }

        private void SetKey(object sender, EventArgs e)
        {
            // Set the developer Api key.
            ApiKeyManager.ArcGISDeveloperApiKey = _keyEntry.Text;
            _ = UpdateValidityText();
        }

        private void DeleteKey(object sender, EventArgs e)
        {
            _currentKeyLabel.Text = Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey = ApiKeyManager.ArcGISDeveloperApiKey = null;
            _statusLabel.Text = "API key removed";
        }

        private void StoreKey(object sender, EventArgs e)
        {
            bool stored = ApiKeyManager.StoreCurrentKey();
            _statusLabel.Text = stored ? "Current API key stored on device" : "API key could not be stored locally";
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

            UIStackView stackView = new UIStackView();
            stackView.Axis = UILayoutConstraintAxis.Vertical;
            stackView.TranslatesAutoresizingMaskIntoConstraints = false;
            stackView.Distribution = UIStackViewDistribution.Fill;
            stackView.Alignment = UIStackViewAlignment.Top;
            stackView.Spacing = 5;
            stackView.LayoutMarginsRelativeArrangement = true;
            //stackView.DirectionalLayoutMargins = new NSDirectionalEdgeInsets(10, 10, 10, 0);
            stackView.LayoutMargins = new UIEdgeInsets(10, 10, 10, 10);

            View.AddSubviews(stackView);

            _infoText = new WKWebView(new CGRect(), new WKWebViewConfiguration()) { BackgroundColor = UIColor.Clear, Opaque = false };
            _infoText.TranslatesAutoresizingMaskIntoConstraints = false;
            _infoText.NavigationDelegate = new BrowserLinksNavigationDelegate();
            stackView.AddArrangedSubview(_infoText);

            UILabel currentKeyPre = new UILabel() { Text = "Current key: ", TranslatesAutoresizingMaskIntoConstraints = false };
            _currentKeyLabel = new UILabel() { Text = string.Empty, TranslatesAutoresizingMaskIntoConstraints = false };
            stackView.AddArrangedSubview(GetRowStackView(new UIView[] { currentKeyPre, _currentKeyLabel }));

            _keyEntry = new UITextField() { TranslatesAutoresizingMaskIntoConstraints = false, KeyboardType = UIKeyboardType.Default, KeyboardAppearance = UIKeyboardAppearance.Default };
            _keyEntry.Frame = new CGRect(10, 10, 300, 40);
            _keyEntry.Placeholder = "Type API key here";
            stackView.AddArrangedSubview(_keyEntry);

            _setKeyButton = new UIButton() { TranslatesAutoresizingMaskIntoConstraints = false };
            _setKeyButton.SetTitle("Set key", UIControlState.Normal);
            _setKeyButton.SetTitleColor(View.TintColor, UIControlState.Normal);

            _deleteKeyButton = new UIButton() { TranslatesAutoresizingMaskIntoConstraints = false };
            _deleteKeyButton.SetTitle("Delete key", UIControlState.Normal);
            _deleteKeyButton.SetTitleColor(View.TintColor, UIControlState.Normal);

            _storeKeyButton = new UIButton() { TranslatesAutoresizingMaskIntoConstraints = false };
            _storeKeyButton.SetTitle("Remember key", UIControlState.Normal);
            _storeKeyButton.SetTitleColor(View.TintColor, UIControlState.Normal);

            stackView.AddArrangedSubview(GetRowStackView(new UIView[] { _setKeyButton, _deleteKeyButton, _storeKeyButton }));

            _statusLabel = new UILabel() { Text = string.Empty, TranslatesAutoresizingMaskIntoConstraints = false };
            stackView.AddArrangedSubview(_statusLabel);

            NSLayoutConstraint.ActivateConstraints(new[]
            {
                stackView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                stackView.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor),
                stackView.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor),

                _infoText.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor),
                _infoText.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor),
                _infoText.HeightAnchor.ConstraintEqualTo(100),
            });
        }

        private UIStackView GetRowStackView(UIView[] views)
        {
            UIStackView row = new UIStackView(views);
            row.TranslatesAutoresizingMaskIntoConstraints = false;
            row.Spacing = 8;
            row.Axis = UILayoutConstraintAxis.Horizontal;
            row.Distribution = UIStackViewDistribution.EqualCentering;
            row.WidthAnchor.ConstraintEqualTo(350).Active = true;
            return row;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _storeKeyButton.TouchUpInside += StoreKey;
            _deleteKeyButton.TouchUpInside += DeleteKey;
            _setKeyButton.TouchUpInside += SetKey;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _storeKeyButton.TouchUpInside -= StoreKey;
            _deleteKeyButton.TouchUpInside -= DeleteKey;
            _setKeyButton.TouchUpInside -= SetKey;
        }
    }
}