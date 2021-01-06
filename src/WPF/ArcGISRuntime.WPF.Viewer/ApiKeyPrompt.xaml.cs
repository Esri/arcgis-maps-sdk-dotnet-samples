// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Shared.Managers;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime
{
    public partial class ApiKeyPrompt : UserControl
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
            CurrentKeyText.Text = Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey = ApiKeyManager.ArcGISDeveloperApiKey = null;
            Status.Text = "API key removed";
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void StoreClick(object sender, RoutedEventArgs e)
        {
            bool stored = ApiKeyManager.StoreCurrentKey();
            Status.Text = stored ? "Current API key stored on device" : "API key could not be stored locally";
        }
    }
}