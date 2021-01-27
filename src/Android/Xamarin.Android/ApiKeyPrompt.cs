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
using Android.Text.Method;
using Android.Widget;
using ArcGISRuntime.Samples.Shared.Managers;
using System;
using System.Threading.Tasks;

namespace ArcGISRuntime
{
    [Activity(Label = "ApiKeyPrompt")]
    public class ApiKeyPrompt : Activity
    {
        private Button _setKeyButton;
        private Button _deleteKeyButton;

        private TextView _currentKeyText;
        private TextView _statusText;

        private EditText _keyEntry;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Setup the UI.
            SetContentView(Resource.Layout.ApiKeyPrompt);

            var linkTextView = FindViewById<TextView>(Resource.Id.ApiKeyInstructions);
            linkTextView.MovementMethod = LinkMovementMethod.Instance;

            _setKeyButton = FindViewById<Button>(Resource.Id.setKeyButton);
            _deleteKeyButton = FindViewById<Button>(Resource.Id.deleteKeyButton);

            _setKeyButton.Click += SetKey;
            _deleteKeyButton.Click += DeleteKey;

            _currentKeyText = FindViewById<TextView>(Resource.Id.currentKeyText);
            _statusText = FindViewById<TextView>(Resource.Id.statusText);

            _keyEntry = FindViewById<EditText>(Resource.Id.keyEntry);

            _ = UpdateValidityText();
        }

        private async Task UpdateValidityText()
        {
            ApiKeyStatus status = await ApiKeyManager.CheckKeyValidity();
            if (status == ApiKeyStatus.Valid)
            {
                _statusText.Text = "API key is valid";
            }
            else
            {
                _statusText.Text = "API key is invalid";
            }
            _currentKeyText.Text = Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey;
        }

        private void SetKey(object sender, EventArgs e)
        {
            // Set the developer Api key.
            ApiKeyManager.ArcGISDeveloperApiKey = _keyEntry.Text;
            ApiKeyManager.StoreCurrentKey();
            _ = UpdateValidityText();
        }

        private void DeleteKey(object sender, EventArgs e)
        {
            ApiKeyManager.ArcGISDeveloperApiKey = null;
            _currentKeyText.Text = string.Empty;
            ApiKeyManager.StoreCurrentKey();
            _statusText.Text = "API key removed";
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _setKeyButton.Click -= SetKey;
            _deleteKeyButton.Click -= DeleteKey;
        }
    }
}