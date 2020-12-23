// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Shared.Managers;
using System.Windows;

namespace ArcGISRuntime
{
    public partial class ApiKeyPrompt : Window
    {
        public ApiKeyPrompt()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            CurrentKeyText.Text = Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey;
        }

        private async void SetKeyButton_Click(object sender, RoutedEventArgs e)
        {
            // Set the developer Api key.
            ApiKeyManager.ArcGISDeveloperApiKey = KeyEntryBox.Text;
            ApiKeyStatus status = await ApiKeyManager.CheckKeyValidity();
            if (status == ApiKeyStatus.Valid)
            {
                Status.Text = "Valid key";
            }
            else
            {
                Status.Text = "Invalid key";
            }
            CurrentKeyText.Text = Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey;
        }

        private void DeleteKeyButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentKeyText.Text = Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey = ApiKeyManager.ArcGISDeveloperApiKey = null;
            Status.Text = "API key removed";
        }
    }
}