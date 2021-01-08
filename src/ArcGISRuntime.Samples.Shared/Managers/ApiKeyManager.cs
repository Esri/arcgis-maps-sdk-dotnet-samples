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
using System.Diagnostics;
using System.IO;

using System.Threading.Tasks;

#if XAMARIN
using Xamarin.Essentials;
#else
using System.Security.Cryptography;
using System.Text;
#endif

using Map = Esri.ArcGISRuntime.Mapping.Map;

namespace ArcGISRuntime.Samples.Shared.Managers
{
    public static class ApiKeyManager
    {
        private static string _key;
        private const string _apiKeyFileName = "agolResource";

        public static string ArcGISDeveloperApiKey
        {
            get
            {
                // An Application programming interface key (API key) is a unique identifier used to authenticate a user, developer, or calling program with a server portal.
                // Typically, API keys are used to authenticate a calling program within the API rather than an individual user.
                // Go to https://citra.sites.afd.arcgis.com/documentation/security-and-authentication/api-keys/ to learn how to obtain a developer API key for ArcGIS Online.

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

        public static async Task<string> GetLocalKey()
        {
#if XAMARIN
            try
            {
                return await SecureStorage.GetAsync(_apiKeyFileName);
            }
            catch(Exception ex)
            {
                return null;
            }
            
#else
            return Encoding.Default.GetString(Unprotect(File.ReadAllBytes(Path.Combine(GetDataFolder(), _apiKeyFileName))));
#endif
        }

        public static bool StoreCurrentKey()
        {
#if XAMARIN
            try
            {
                SecureStorage.SetAsync(_apiKeyFileName, Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
#else
            try
            {
                File.WriteAllBytes(Path.Combine(GetDataFolder(), _apiKeyFileName), Protect(Encoding.Default.GetBytes(Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.ApiKey)));
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
#endif
        }

        internal static string GetDataFolder()
        {
#if NETFX_CORE
            string appDataFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#elif XAMARIN
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
#else
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
#endif
            string sampleDataFolder = Path.Combine(appDataFolder, "ArcGISRuntimeSampleData");

            if (!Directory.Exists(sampleDataFolder)) { Directory.CreateDirectory(sampleDataFolder); }

            return sampleDataFolder;
        }

# if XAMARIN
# else

        #region Data Protection

        private const int EntropySize = 16;

        // Generates a cryptographically random IV
        private static byte[] GenerateEntropy()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var entropy = new byte[EntropySize];
                rng.GetBytes(entropy);
                return entropy;
            }
        }

        // Handles encrypting an array of bytes.
        private static byte[] Protect(byte[] bytes)
        {
            // Generate a random initialization vector
            var entropy = GenerateEntropy();

            // Use the DPAPI to encrypt "bytes"
            var protectedBytes = ProtectedData.Protect(bytes, entropy, DataProtectionScope.CurrentUser);

            // Return the combines IV + protected data
            var result = new byte[entropy.Length + protectedBytes.Length];
            Buffer.BlockCopy(entropy, 0, result, 0, entropy.Length);
            Buffer.BlockCopy(protectedBytes, 0, result, entropy.Length, protectedBytes.Length);
            return result;
        }

        // Handles decrypting an array of bytes.
        private static byte[] Unprotect(byte[] bytes)
        {
            if (bytes?.Length < EntropySize)
                throw new InvalidDataException(); // data too small, likely invalid

            // Copy IV from "bytes"
            var entropy = new byte[EntropySize];
            Buffer.BlockCopy(bytes, 0, entropy, 0, entropy.Length);

            // Copy protected data
            var protectedBytes = new byte[bytes.Length - EntropySize];
            Buffer.BlockCopy(bytes, EntropySize, protectedBytes, 0, protectedBytes.Length);

            // Return the unprotected data
            return ProtectedData.Unprotect(protectedBytes, entropy, DataProtectionScope.CurrentUser);
        }

        #endregion Data Protection

#endif
    }

    public enum ApiKeyStatus
    {
        Valid,
        Invalid,
        Missing
    }
}