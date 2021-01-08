// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Shared.Managers;
using System;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ArcGISRuntime
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ApiKeyPrompt : ContentPage
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

        private void SetKeyButton_Clicked(object sender, EventArgs e)
        {
            // Set the developer Api key.
            ApiKeyManager.ArcGISDeveloperApiKey = KeyEntryBox.Text;
            UpdateValidiyText();
        }

        private void DeleteKeyButton_Clicked(object sender, EventArgs e)
        {
            Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey = ApiKeyManager.ArcGISDeveloperApiKey = null;
            CurrentKeyText.Text = string.Empty;
            Status.Text = "API key removed";
        }

        private void StoreClicked(object sender, EventArgs e)
        {
            bool stored = ApiKeyManager.StoreCurrentKey();
            Status.Text = stored ? "Current API key stored on device" : "API key could not be stored locally";
        }
    }
}