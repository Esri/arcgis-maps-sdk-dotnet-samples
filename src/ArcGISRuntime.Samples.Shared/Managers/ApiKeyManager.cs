// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using System;
using System.Threading.Tasks;

namespace ArcGISRuntime.Samples.Shared.Managers
{
    public static class ApiKeyManager
    {
        private static string _key;

        public static string ArcGISDeveloperApiKey
        {
            get
            {
                // An Application programming interface key (API key) is a unique identifier used to authenticate a user, developer, or calling program with a server portal.
                // Typically, API keys are used to authenticate a calling program within the API rather than an individual user.
                // Go to https://citra.sites.afd.arcgis.com/documentation/security-and-authentication/api-keys/ to learn how to obtain a developer API key for ArcGIS Online.
                // You can use your developer API key here and it will work in all of the .NET sample viewers.
                string userAPIkey = null;
                if (_key == null) _key = userAPIkey;

                return _key;
            }

            set
            {
                Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey = _key = value;
            }
        }

        public async static Task<ApiKeyStatus> CheckKeyValidity()
        {
            // Check that a key has been set.
            if (Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey == null || Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey == string.Empty) return ApiKeyStatus.Missing;

            // Check that key is valid for loading a basemap.
            try
            {
                await new Map(BasemapStyle.ArcGISTopographic).LoadAsync();
                return ApiKeyStatus.Valid;
            }
            catch (Exception)
            {
                return ApiKeyStatus.Invalid;
            }
        }
    }

    public enum ApiKeyStatus
    {
        Valid,
        Invalid,
        Missing
    }
}