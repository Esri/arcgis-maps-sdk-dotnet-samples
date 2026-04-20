// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Security;

#if ANDROID

using Android.App;
using Application = Microsoft.Maui.Controls.Application;
using Android.Content;
using Android.Content.PM;

#endif

namespace ArcGIS.Helpers
{
    internal static class ArcGISLoginPrompt
    {
        private const string ArcGISOnlineUrl = "https://www.arcgis.com/sharing/rest";

        // - The Client ID for an app registered with the server (the ID below is for a public app created by the ArcGIS Maps SDK for Native Apps team).
        public const string AppClientId = @"NDiGU6O6UiscRDPw";

        // - An optional client secret for the app (only needed for the OAuthClientCredentials authorization type).
        private const string ClientSecret = "";

        // - A URL for redirecting after a successful authorization (this must be a URL configured with the app).
        private const string OAuthRedirectUrl = @"maui-ags-app://auth";

        public static async Task<bool> EnsureAGOLCredentialAsync()
        {
            Credential currentCredential = AuthenticationManager.Current.FindCredential(new Uri(ArcGISOnlineUrl), AuthenticationType.Token);
            if (currentCredential != null)
            {
                return true; // already logged in
            }

            try
            {
                var userConfig = new OAuthUserConfiguration(new Uri(ArcGISOnlineUrl), AppClientId, new Uri(OAuthRedirectUrl));
                Credential cred = await OAuthUserCredential.CreateAsync(userConfig);
                AuthenticationManager.Current.AddCredential(cred);
            }
            catch (OperationCanceledException)
            {
                // OAuth login was canceled, no need to display error to user.
            }
            catch (Exception ex)
            {
                await Application.Current.Windows[0].Page.DisplayAlert("Login failed", ex.Message, "OK");
            }
            return false;
        }

        public static void SetChallengeHandler()
        {
            var userConfig = new OAuthUserConfiguration(new Uri(ArcGISOnlineUrl), AppClientId, new Uri(OAuthRedirectUrl));
            AuthenticationManager.Current.OAuthUserConfigurations.Add(userConfig);
            AuthenticationManager.Current.OAuthHandler = new OAuthAuthorize();
        }
    }

    #region IOAuthAuthorizationHandler implementation

    public class OAuthAuthorize : IOAuthHandler
    {
#if IOS || MACCATALYST
		TaskCompletionSource<IDictionary<string, string>> _taskCompletionSource;
		
        public Task<IDictionary<string, string>> LoginAsync(OAuthLoginParameters parameters)
        {
            _taskCompletionSource = new TaskCompletionSource<IDictionary<string, string>>();
            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    var result = await WebAuthenticator.AuthenticateAsync(parameters.AuthorizeUri, parameters.RedirectUri);
                    _taskCompletionSource.TrySetResult(result.Properties);
                }
                catch (Exception ex)
                {
                    _taskCompletionSource.TrySetException(ex);
                }
            });
            return _taskCompletionSource.Task;
		}
#elif ANDROID
        public async Task<IDictionary<string, string>> LoginAsync(OAuthLoginParameters parameters)
        {
            var result = await WebAuthenticator.AuthenticateAsync(parameters.AuthorizeUri, parameters.RedirectUri);
            return result.Properties;
        }
#elif WINDOWS
        public async Task<IDictionary<string, string>> LoginAsync(OAuthLoginParameters parameters)
        {
            var result = await WinUIEx.WebAuthenticator.AuthenticateAsync(parameters.AuthorizeUri, parameters.RedirectUri);
            return result.Properties;
        }
#else
        public Task<IDictionary<string, string>> LoginAsync(OAuthLoginParameters parameters)
        {
            throw new NotImplementedException();
        }
#endif
}

#if ANDROID

    [Activity(NoHistory = true, Exported = true, LaunchMode = LaunchMode.SingleTop)]
    [IntentFilter(new[] { Intent.ActionView },
       Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
       DataScheme = "maui-ags-app", DataHost = "auth")]
    public class WebAuthenticationCallbackActivity : WebAuthenticatorCallbackActivity
    {
    }

#endif

    #endregion IOAuthAuthorizationHandler implementation
}