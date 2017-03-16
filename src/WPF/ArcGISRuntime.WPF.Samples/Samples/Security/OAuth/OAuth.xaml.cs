// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace OAuth
{
    public partial class MainWindow : Window
    {
        // Constants for OAuth-related values ...
        // URL of the server to authenticate with
        private const string ServerUrl = "https://www.arcgis.com/sharing/rest";
        // TODO: Provide the client ID for your app (registered with the server)
        private const string ClientId = "";
        // TODO: [optional] Provide the client secret for the app (only needed for the OAuthAuthorizationCode auth type)
        private const string ClientSecret = "";
        // TODO: Provide a URL registered for the app for redirecting after a successful authorization
        private const string RedirectUrl = "http://my.redirect.url";
        // TODO: Provide an ID for a secured web map item hosted on the server
        private const string WebMapId = "";

        public MainWindow()
        {
            InitializeComponent();

            // Call a function to initialize the app
            Initialize();
        }

        private void Initialize()
        {
            // Set up the AuthenticationManager to use OAuth for secure ArcGIS Online requests
            UpdateAuthenticationManager();

            // Display a secured web map from ArcGIS Online (will be challenged to log in)
            DisplaySecureMap();
        }

        private void UpdateAuthenticationManager()
        {
            // Register the server information with the AuthenticationManager
            Esri.ArcGISRuntime.Security.ServerInfo serverInfo = new ServerInfo
            {
                ServerUri = new Uri(ServerUrl),
                OAuthClientInfo = new OAuthClientInfo
                {
                    ClientId = ClientId,
                    RedirectUri = new Uri(RedirectUrl)
                }
            };

            // If a client secret has been configured, set the authentication type to OAuthAuthorizationCode
            if (!string.IsNullOrEmpty(ClientSecret))
            {
                // Use OAuthAuthorizationCode if you need a refresh token (and have specified a valid client secret)
                serverInfo.TokenAuthenticationType = TokenAuthenticationType.OAuthAuthorizationCode;
                serverInfo.OAuthClientInfo.ClientSecret = ClientSecret;
            }
            else
            {
                // Otherwise, use OAuthImplicit
                serverInfo.TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit;
            }

            // Register this server with AuthenticationManager
            AuthenticationManager.Current.RegisterServer(serverInfo);

            // Use the OAuthAuthorize class in this project to handle OAuth communication
            AuthenticationManager.Current.OAuthAuthorizeHandler = new OAuthAuthorize();

            // Use a function in this class to challenge for credentials
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);
        }

        private async void DisplaySecureMap()
        {
            // Display a web map hosted in a portal. If the web map item is secured, AuthenticationManager will
            // challenge for credentials
            try
            {
                // Connect to a portal (ArcGIS Online, for example)
                ArcGISPortal arcgisPortal = await ArcGISPortal.CreateAsync(new Uri(ServerUrl));
                // Get a web map portal item using its ID
                // If the item is secured (not shared publicly) the user will be challenged for credentials at this point
                PortalItem portalItem = await PortalItem.CreateAsync(arcgisPortal, WebMapId);
                // Create a new map with the portal item
                Map myMap = new Map(portalItem);

                // Assign the map to the MapView.Map property to display it in the app
                MyMapView.Map = myMap;
                await myMap.RetryLoadAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error displaying map: " + ex.Message);
            }
        }

        public async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            // ChallengeHandler function for AuthenticationManager that will be called whenever access to a secured
            // resource is attempted
            OAuthTokenCredential credential = null;

            try
            {
                // Create generate token options if necessary
                if (info.GenerateTokenOptions == null)
                {
                    info.GenerateTokenOptions = new GenerateTokenOptions();
                }

                // AuthenticationManager will handle challenging the user for credentials
                credential = await AuthenticationManager.Current.GenerateCredentialAsync
                    (
                            info.ServiceUri,
                            info.GenerateTokenOptions
                    ) as OAuthTokenCredential;
            }
            catch (Exception ex)
            {
                // Exception will be reported in calling function
                throw (ex);
            }

            return credential;
        }
    }

    // In a desktop (WPF) app, an IOAuthAuthorizeHandler component is used to handle some of the OAuth details. Specifically, it
    //     implements AuthorizeAsync to show the login UI (generated by the server that hosts secure content) in a web control.
    //     When the user logs in successfully, cancels the login, or closes the window without continuing, the IOAuthAuthorizeHandler
    //     is responsible for obtaining the authorization from the server or raising an OperationCanceledException.
    // Note: a custom IOAuthAuthorizeHandler component is not necessary when using OAuth in an ArcGIS Runtime Universal Windows app.
    //     The UWP AuthenticationManager uses a built-in IOAuthAuthorizeHandler that is based on WebAuthenticationBroker
    public class OAuthAuthorize : IOAuthAuthorizeHandler
    {
        // Window to contain the OAuth UI
        private Window _window;
        // Use a TaskCompletionSource to track the completion of the authorization
        private TaskCompletionSource<IDictionary<string, string>> _tcs;
        // URL for the authorization callback result (the redirect URI configured for your application)
        private string _callbackUrl;
        // URL that handles the OAuth request
        private string _authorizeUrl;

        // Function to handle authorization requests, takes the URIs for the secured service, the authorization endpoint, and the redirect URI
        public Task<IDictionary<string, string>> AuthorizeAsync(Uri serviceUri, Uri authorizeUri, Uri callbackUri)
        {
            // If the TaskCompletionSource.Task has not completed, authorization is in progress
            if (_tcs != null && !_tcs.Task.IsCompleted)
            { 
                // Allow only one authorization process at a time
                throw new Exception("Task in progress");
            }

            // Store the authorization and redirect URLs
            _authorizeUrl = authorizeUri.AbsoluteUri;
            _callbackUrl = callbackUri.AbsoluteUri;

            // Create a task completion source
            _tcs = new TaskCompletionSource<IDictionary<string, string>>();

            // Call a function to show the login controls, make sure it runs on the UI thread for this app
            var dispatcher = Application.Current.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
                AuthorizeOnUIThread(_authorizeUrl);
            else
            {
                var authorizeOnUIAction = new Action((() => AuthorizeOnUIThread(_authorizeUrl)));
                dispatcher.BeginInvoke(authorizeOnUIAction);
            }

            // Return the task associated with the TaskCompletionSource
            return _tcs.Task;
        }

        // Challenge for OAuth credentials on the UI thread
        private void AuthorizeOnUIThread(string authorizeUri)
        {
            // Create a WebBrowser control to display the authorize page
            var webBrowser = new WebBrowser();
            // Handle the navigation event for the browser to check for a response to the redirect URL
            webBrowser.Navigating += WebBrowserOnNavigating;

            // Display the web browser in a new window 
            _window = new Window
            {
                Content = webBrowser,
                Height = 400,
                Width = 330,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };

            // Set the app's window as the owner of the browser window (if main window closes, so will the browser)
            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                _window.Owner = Application.Current.MainWindow;
            }

            // Handle the window closed event then navigate to the authorize url
            _window.Closed += OnWindowClosed;
            webBrowser.Navigate(authorizeUri);

            // Display the Window
            _window.ShowDialog();
        }

        void OnWindowClosed(object sender, EventArgs e)
        {
            // If the browser window closes, return the focus to the main window
            if (_window != null && _window.Owner != null)
            {
                _window.Owner.Focus();
            }

            // If the task wasn't completed, the user must have closed the window without logging in
            if (!_tcs.Task.IsCompleted)
            {
                // Set the task completion source exception to indicate a canceled operation
                _tcs.SetCanceled();
            }

            _window = null;
        }

        // Handle browser navigation (content changing)
        void WebBrowserOnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            // Check for a response to the callback url
            const string portalApprovalMarker = "/oauth2/approval";
            var webBrowser = sender as WebBrowser;
            Uri uri = e.Uri;

            // If no browser, uri, or an empty url, return
            if (webBrowser == null || uri == null || string.IsNullOrEmpty(uri.AbsoluteUri))
                return;

            // Check for redirect
            bool isRedirected = uri.AbsoluteUri.StartsWith(_callbackUrl) ||
                _callbackUrl.Contains(portalApprovalMarker) && uri.AbsoluteUri.Contains(portalApprovalMarker);

            if (isRedirected)
            {
                // Browser was redirected to the callbackUrl (success!)
                //    -close the window 
                //    -decode the parameters (returned as fragments or query)
                //    -return these parameters as result of the Task
                e.Cancel = true;

                // Call a helper function to decode the response parameters
                var authResponse = DecodeParameters(uri);

                // Set the result for the task completion source
                _tcs.SetResult(authResponse);

                if (_window != null)
                {
                    _window.Close();
                }
            }
        }

        private static IDictionary<string, string> DecodeParameters(Uri uri)
        {
            // Create a dictionary of key value pairs returned in an OAuth authorization response URI query string
            var answer = string.Empty;

            // Get the values from the URI fragment or query string
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

            // Parse parameters into key / value pairs
            var keyValueDictionary = new Dictionary<string, string>();
            var keysAndValues = answer.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var kvString in keysAndValues)
            {
                var pair = kvString.Split('=');
                string key = pair[0];
                string value = string.Empty;
                if (key.Length > 1)
                {
                    value = Uri.UnescapeDataString(pair[1]);
                }

                keyValueDictionary.Add(key, value);
            }

            // Return the dictionary of string keys/values
            return keyValueDictionary;
        }
    }
}
