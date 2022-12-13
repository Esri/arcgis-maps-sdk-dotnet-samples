﻿// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Security;
using Microsoft.Maui.ApplicationModel;

#if ANDROID || IOS || MACCATALYST

using System.Collections.Generic;

#endif

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
            bool loggedIn = false;

            try
            {
                // Create a challenge request for portal credentials (OAuth credential request for arcgis.com)
                CredentialRequestInfo challengeRequest = new CredentialRequestInfo
                {
                    // Use the OAuth authorization code workflow.
                    GenerateTokenOptions = new GenerateTokenOptions
                    {
                        TokenAuthenticationType = TokenAuthenticationType.OAuthAuthorizationCode
                    },

                    // Indicate the url (portal) to authenticate with (ArcGIS Online)
                    ServiceUri = new Uri(ArcGISOnlineUrl)
                };

                // Call GetCredentialAsync on the AuthenticationManager to invoke the challenge handler
                Credential cred = await AuthenticationManager.Current.GetCredentialAsync(challengeRequest, false);
                loggedIn = cred != null;
            }
            catch (OperationCanceledException)
            {
                // OAuth login was canceled, no need to display error to user.
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Login failed", ex.Message, "OK");
            }

            return loggedIn;
        }

        public static void SetChallengeHandler()
        {
            // Define the server information for ArcGIS Online
            ServerInfo portalServerInfo = new ServerInfo(new Uri(ArcGISOnlineUrl))
            {
                TokenAuthenticationType = TokenAuthenticationType.OAuthAuthorizationCode,
                OAuthClientInfo = new OAuthClientInfo(AppClientId, new Uri(OAuthRedirectUrl))
            };

            // If a client secret has been configured, set the authentication type to OAuth client credentials.
            if (!string.IsNullOrEmpty(ClientSecret))
            {
                // If a client secret is specified then use the TokenAuthenticationType.OAuthClientCredentials type.
                portalServerInfo.TokenAuthenticationType = TokenAuthenticationType.OAuthClientCredentials;
                portalServerInfo.OAuthClientInfo.ClientSecret = ClientSecret;
            }

            // Register this server with AuthenticationManager.
            AuthenticationManager.Current.RegisterServer(portalServerInfo);

            // Use a function in this class to challenge for credentials.
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(PromptCredentialAsync);

            // Set the OAuthAuthorizeHandler component (this class) for Android or iOS platforms.
            AuthenticationManager.Current.OAuthAuthorizeHandler = new OAuthAuthorize();
        }

        // ChallengeHandler function that will be called whenever access to a secured resource is attempted.
        public static async Task<Credential> PromptCredentialAsync(CredentialRequestInfo info)
        {
            Credential credential = null;

            try
            {
                // IOAuthAuthorizeHandler will challenge the user for OAuth credentials.
                credential = await AuthenticationManager.Current.GenerateCredentialAsync(info.ServiceUri);
            }
            // OAuth login was canceled, no need to display error to user.
            catch (TaskCanceledException) { }
            catch (OperationCanceledException) { }

            return credential;
        }
    }

    #region IOAuthAuthorizationHandler implementation

    public class OAuthAuthorize : IOAuthAuthorizeHandler
    {
#if IOS || MACCATALYST
		TaskCompletionSource<IDictionary<string, string>> _taskCompletionSource;
		
        public Task<IDictionary<string, string>> AuthorizeAsync(Uri serviceUri, Uri authorizeUri, Uri callbackUri)
        {
            _taskCompletionSource = new TaskCompletionSource<IDictionary<string, string>>();
            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    var result = await WebAuthenticator.AuthenticateAsync(authorizeUri, callbackUri);
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
        public async Task<IDictionary<string, string>> AuthorizeAsync(Uri serviceUri, Uri authorizeUri, Uri callbackUri)
        {
            var result = await WebAuthenticator.AuthenticateAsync(authorizeUri, callbackUri);
            return result.Properties;
        }
#elif WINDOWS
        public async Task<IDictionary<string, string>> AuthorizeAsync(Uri serviceUri, Uri authorizeUri, Uri callbackUri)
        {
            var result = await WinUIEx.WebAuthenticator.AuthenticateAsync(authorizeUri, callbackUri);
            return result.Properties;
        }
#else
        public async Task<IDictionary<string, string>> AuthorizeAsync(Uri serviceUri, Uri authorizeUri, Uri callbackUri)
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