// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.WinUI;
using Esri.ArcGISRuntime.Security;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ArcGISRuntime.Helpers
{
    internal static class ArcGISLoginPrompt
    {
        private const string ArcGISOnlineUrl = "https://www.arcgis.com/sharing/rest";

        // - The Client ID for an app registered with the server (the ID below is for a public app created by the ArcGIS Runtime team).
        private const string AppClientId = "lgAdHkYZYlwwfAhC";

        // - An optional client secret for the app (only needed for the OAuthClientCredentials authorization type).
        private const string ClientSecret = "";

        // - A URL for redirecting after a successful authorization (this must be a URL configured with the app).
        private const string OAuthRedirectUrl = "my-ags-app://auth";

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
                // Login failure
                await new MessageDialog2(ex.Message, "Login failed").ShowAsync();
            }

            return loggedIn;
        }

        // ChallengeHandler function that will be called whenever access to a secured resource is attempted
        public static async Task<Credential> PromptCredentialAsync(CredentialRequestInfo info)
        {
            Credential credential = null;

            try
            {
                // IOAuthAuthorizeHandler will challenge the user for OAuth credentials
                credential = await AuthenticationManager.Current.GenerateCredentialAsync(info.ServiceUri);
            }
            catch (OperationCanceledException)
            {
                // OAuth login was canceled, no need to display error to user.
            }

            return credential;
        }

        public static void SetChallengeHandler(UserControl sample)
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

            // Register the ArcGIS Online server information with the AuthenticationManager
            AuthenticationManager.Current.RegisterServer(portalServerInfo);

            // Create a new ChallengeHandler that uses a method in this class to challenge for credentials
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(PromptCredentialAsync);

            // Use the OAuthAuthorize class in this project to create a new web view to show the login UI
            AuthenticationManager.Current.OAuthAuthorizeHandler = new OAuthAuthorize(sample);
        }
    }

    #region OAuth handler

    public class OAuthAuthorize : IOAuthAuthorizeHandler
    {
        // Window to contain the OAuth UI.
        private ContentDialog _authWindow;

        // Use a TaskCompletionSource to track the completion of the authorization.
        private TaskCompletionSource<IDictionary<string, string>> _tcs;

        // URL for the authorization callback result (the redirect URI configured for your application).
        private string _callbackUrl;

        // URL that handles the OAuth request.
        private string _authorizeUrl;
        private UserControl sample;

        public OAuthAuthorize(UserControl sample)
        {
            this.sample = sample;
        }

        // Function to handle authorization requests, takes the URIs for the secured service, the authorization endpoint, and the redirect URI.
        Task<IDictionary<string, string>> IOAuthAuthorizeHandler.AuthorizeAsync(Uri serviceUri, Uri authorizeUri, Uri callbackUri)
        {
            if (_tcs != null && !_tcs.Task.IsCompleted)
                throw new Exception("Task in progress");

            _tcs = new TaskCompletionSource<IDictionary<string, string>>();

            // Store the authorization and redirect URLs.
            _authorizeUrl = authorizeUri.AbsoluteUri;
            _callbackUrl = callbackUri.AbsoluteUri;

            // Call a function to show the login controls, make sure it runs on the UI thread for this app.
            sample.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
            {
                _ = AuthorizeOnUIThread(authorizeUri);
            });

            // Return the task associated with the TaskCompletionSource.
            return _tcs.Task;
        }

        // Challenge for OAuth credentials on the UI thread.
        private async Task AuthorizeOnUIThread(Uri authorizeUri)
        {
            // Create a WebBrowser control to display the authorize page.
            WebView2 webBrowser = new WebView2 { Width = 500, Height = 500, RequestedTheme = Microsoft.UI.Xaml.ElementTheme.Light };

            // Handle the navigation event for the browser to check for a response to the redirect URL.
            webBrowser.NavigationStarting += NavigationStarted;

            // Display the web browser in a new window.
            _authWindow = new ContentDialog
            {
                Content = webBrowser,
                XamlRoot = sample.XamlRoot,
                CloseButtonText = "Close",
            };

            // Handle the window closed event then navigate to the authorize url.
            _authWindow.Closed += OnWindowClosed2;

            try
            {
                // Load the web view and navigate to the Uri.
                await webBrowser.EnsureCoreWebView2Async();
                webBrowser.Source = authorizeUri;

                // Display the window.
                await _authWindow.ShowAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _tcs.SetCanceled();
            }
        }

        // Handle browser navigation (content changing).
        private void NavigationStarted(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        {
            // Check for a response to the callback url.
            const string portalApprovalMarker = "/oauth2/approval";

            Uri uri = new Uri(args.Uri);

            // If no browser, uri, or an empty url, return.
            if (sender == null || uri == null || string.IsNullOrEmpty(uri.AbsoluteUri))
                return;

            // Check for redirect.
            bool isRedirected = uri.AbsoluteUri.StartsWith(_callbackUrl) ||
                _callbackUrl.Contains(portalApprovalMarker) && uri.AbsoluteUri.Contains(portalApprovalMarker);

            // Check if browser was redirected to the callback URL. (This indicates succesful authentication.)
            if (isRedirected)
            {
                args.Cancel = true;

                // Call a helper function to decode the response parameters.
                IDictionary<string, string> authResponse = DecodeParameters(uri);

                // Set the result for the task completion source.
                _tcs.SetResult(authResponse);

                // Close the window.
                if (_authWindow != null)
                {
                    _authWindow.Hide();
                }
            }
        }

        // Handle the browser window closing.
        private void OnWindowClosed2(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            // If the task wasn't completed, the user must have closed the window without logging in.
            if (!_tcs.Task.IsCompleted)
            {
                // Set the task completion source exception to indicate a canceled operation.
                _tcs.SetCanceled();
            }

            _authWindow = null;
        }

        private static IDictionary<string, string> DecodeParameters(Uri uri)
        {
            // Create a dictionary of key value pairs returned in an OAuth authorization response URI query string.
            string answer = "";

            // Get the values from the URI fragment or query string.
            if (!string.IsNullOrEmpty(uri.Fragment))
            {
                answer = uri.Fragment.Substring(1);
            }
            else
            {
                if (!string.IsNullOrEmpty(uri.Query))
                {
                    answer = uri.Query.Substring(1);
                }
            }

            // Parse parameters into key / value pairs.
            Dictionary<string, string> keyValueDictionary = new Dictionary<string, string>();
            string[] keysAndValues = answer.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string kvString in keysAndValues)
            {
                string[] pair = kvString.Split('=');
                string key = pair[0];
                string value = string.Empty;
                if (key.Length > 1)
                {
                    value = Uri.UnescapeDataString(pair[1]);
                }

                keyValueDictionary.Add(key, value);
            }

            // Return the dictionary of string keys/values.
            return keyValueDictionary;
        }
    }

    #endregion OAuth handler
}