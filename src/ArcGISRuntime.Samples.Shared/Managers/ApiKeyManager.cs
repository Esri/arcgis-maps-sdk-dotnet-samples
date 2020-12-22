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
                _key = value;
            }
        }

        public async static Task<ApiKeyStatus> CheckKeyValidity()
        {
            // Check that a key has been set.
            if (_key == null) return ApiKeyStatus.Missing;

            // Check that key is valid for loading a basemap.
            try
            {
                await new Map(BasemapStyle.ArcGISTopographic).LoadAsync();
                return ApiKeyStatus.Valid;
            }
            catch(Exception)
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