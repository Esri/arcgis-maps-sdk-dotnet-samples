// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Shared.Managers;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace ArcGISRuntime
{
    public sealed partial class ApiKeyPrompt : Page
    {
        public ApiKeyPrompt()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            CurrentKeyText.Text = Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey;
            UpdateValidiyText();
        }

        private void SetKeyButton_Click(object sender, RoutedEventArgs e)
        {
            // Set the developer Api key.
            ApiKeyManager.ArcGISDeveloperApiKey = KeyEntryBox.Text;
            UpdateValidiyText();
        }

        private async void UpdateValidiyText()
        {
            ApiKeyStatus status = await ApiKeyManager.CheckKeyValidity();
            if (status == ApiKeyStatus.Valid)
            {
                Status.Text = "API key is valid";
            }
            else
            {
                Status.Text = "API key is invalid";
            }
            CurrentKeyText.Text = Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey;
        }

        private void DeleteKeyButton_Click(object sender, RoutedEventArgs e)
        {
            Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey = ApiKeyManager.ArcGISDeveloperApiKey = null;
            CurrentKeyText.Text = string.Empty;
            Status.Text = "API key removed";
        }

        private void StoreClick(object sender, RoutedEventArgs e)
        {
            bool stored = ApiKeyManager.StoreCurrentKey();
            Status.Text = stored ? "Current API key stored on device" : "API key could not be stored locally";
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }
    }
}