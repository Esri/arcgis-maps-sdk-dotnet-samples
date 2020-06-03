// Copyright 2017 Esri.
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
using System.Windows.Threading;

namespace ArcGISRuntime.WPF.Samples.OAuth
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Authenticate with OAuth",
        category: "Security",
        description: "Authenticate with ArcGIS Online (or your own portal) using OAuth2 to access secured resources (such as private web maps or layers).",
        instructions: "When you run the sample, the app will load a web map which contains premium content. You will be challenged for an ArcGIS Online login to view the private layers. Enter a user name and password for an ArcGIS Online named user account (such as your ArcGIS for Developers account). If you authenticate successfully, the traffic layer will display, otherwise the map will contain only the public basemap layer.",
        tags: new[] { "OAuth", "OAuth2", "authentication", "cloud", "credential", "portal", "security" })]
    public partial class OAuth
    {
        // Constants for OAuth-related values.
        // - The URL of the portal to authenticate with
        private const string ServerUrl = "https://www.arcgis.com/sharing/rest";
        // - The Client ID for an app registered with the server (the ID below is for a public app created by the ArcGIS Runtime team).
        private const string AppClientId = @"lgAdHkYZYlwwfAhC";
        // - An optional client secret for the app (only needed for the OAuthAuthorizationCode authorization type).
        private const string ClientSecret = "";
        // - A URL for redirecting after a successful authorization (this must be a URL configured with the app).
        private const string OAuthRedirectUrl = @"my-ags-app://auth";
        // - The ID for a web map item hosted on the server (the ID below is for a traffic map of Paris).
        private const string WebMapId = "e5039444ef3c48b8a8fdc9227f9be7c1";

        public OAuth()
        {
            InitializeComponent();

            // Call a function to initialize the app and request a web map (with a secured layer).
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Set up the AuthenticationManager to use OAuth for secure ArcGIS Online requests.
                SetOAuthInfo();

                // Connect to the portal (ArcGIS Online, for example).
                ArcGISPortal arcgisPortal = await ArcGISPortal.CreateAsync(new Uri(ServerUrl));

                // Get a web map portal item using its ID.
                // If the item contains layers not shared publicly, the user will be challenged for credentials at this point.
                PortalItem portalItem = await PortalItem.CreateAsync(arcgisPortal, WebMapId);

                // Create a new map with the portal item and display it in the map view.
                // If authentication fails, only the public layers are displayed.
                Map myMap = new Map(portalItem);
                MyMapView.Map = myMap;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error starting sample");
            }
        }

        private void SetOAuthInfo()
        {
            // Register the server information with the AuthenticationManager, including the OAuth settings.
            ServerInfo serverInfo = new ServerInfo
            {
                ServerUri = new Uri(ServerUrl),
                TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit,
                OAuthClientInfo = new OAuthClientInfo
                {
                    ClientId = AppClientId,
                    RedirectUri = new Uri(OAuthRedirectUrl)
                }
            };

            // If a client secret has been configured, set the authentication type to OAuthAuthorizationCode.
            if (!String.IsNullOrEmpty(ClientSecret))
            {
                // Use OAuthAuthorizationCode if you need a refresh token (and have specified a valid client secret).
                serverInfo.TokenAuthenticationType = TokenAuthenticationType.OAuthAuthorizationCode;
                serverInfo.OAuthClientInfo.ClientSecret = ClientSecret;
            }

            // Register this server with AuthenticationManager.
            AuthenticationManager.Current.RegisterServer(serverInfo);

            // Use the custom OAuthAuthorize class (defined in this module) to handle OAuth communication.
            AuthenticationManager.Current.OAuthAuthorizeHandler = new OAuthAuthorize();

            // Use a function in this class to challenge for credentials.
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);
        }
    
        public async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            // ChallengeHandler function for AuthenticationManager that will be called whenever a secured resource is accessed.
            Credential credential = null;

            try
            {
                // AuthenticationManager will handle challenging the user for credentials.
                credential = await AuthenticationManager.Current.GenerateCredentialAsync(info.ServiceUri);
            }
            catch (Exception)
            {
                // Exception will be reported in calling function.
                throw;
            }

            return credential;
        }
    }

    // In a desktop (WPF) app, an IOAuthAuthorizeHandler component is used to handle some of the OAuth details. Specifically, it
    //     implements AuthorizeAsync to show the login UI (generated by the server that hosts secure content) in a web control.
    //     When the user logs in successfully, cancels the login, or closes the window without continuing, the IOAuthAuthorizeHandler
    //     is responsible for obtaining the authorization from the server or raising an OperationCanceledException.
    // Note: a custom IOAuthAuthorizeHandler component is not necessary when using OAuth in an ArcGIS Runtime Universal Windows app.
    //     The UWP AuthenticationManager uses a built-in IOAuthAuthorizeHandler that is based on WebAuthenticationBroker.
    public class OAuthAuthorize : IOAuthAuthorizeHandler
    {
        // A window to contain the OAuth UI.
        private Window _authWindow;

        // A TaskCompletionSource to track the completion of the authorization.
        private TaskCompletionSource<IDictionary<string, string>> _taskCompletionSource;

        // URL for the authorization callback result (the redirect URI configured for the application).
        private string _callbackUrl;

        // URL that handles the OAuth request.
        private string _authorizeUrl;

        // A function to handle authorization requests. It takes the URIs for the secured service, the authorization endpoint, and the redirect URI.
        public Task<IDictionary<string, string>> AuthorizeAsync(Uri serviceUri, Uri authorizeUri, Uri callbackUri)
        {
            // If the TaskCompletionSource.Task has not completed, authorization is in progress.
            if (_taskCompletionSource != null || _authWindow != null)
            {
                // Allow only one authorization process at a time.
                throw new Exception("Authorization is in progress");
            }

            // Store the authorization and redirect URLs.
            _authorizeUrl = authorizeUri.AbsoluteUri;
            _callbackUrl = callbackUri.AbsoluteUri;

            // Create a task completion source to track completion.
            _taskCompletionSource = new TaskCompletionSource<IDictionary<string, string>>();

            // Call a function to show the login controls, make sure it runs on the UI thread.
            Dispatcher dispatcher = Application.Current.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
                AuthorizeOnUIThread(_authorizeUrl);
            else
            {
                Action authorizeOnUIAction = () => AuthorizeOnUIThread(_authorizeUrl);
                dispatcher.BeginInvoke(authorizeOnUIAction);
            }

            // Return the task associated with the TaskCompletionSource.
            return _taskCompletionSource.Task;
        }

        // A function to challenge for OAuth credentials on the UI thread.
        private void AuthorizeOnUIThread(string authorizeUri)
        {
            // Create a WebBrowser control to display the authorize page.
            WebBrowser authBrowser = new WebBrowser();

            // Handle the navigating event for the browser to check for a response sent to the redirect URL.
            authBrowser.Navigating += WebBrowserOnNavigating;

            // Display the web browser in a new window.
            _authWindow = new Window
            {
                Content = authBrowser,
                Height = 420,
                Width = 350,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            // Set the app's window as the owner of the browser window (if main window closes, so will the browser).
            if (Application.Current != null && Application.Current.MainWindow != null)
            {
                _authWindow.Owner = Application.Current.MainWindow;
            }

            // Handle the window closed event then navigate to the authorize url.
            _authWindow.Closed += OnWindowClosed;
            authBrowser.Navigate(authorizeUri);

            // Display the Window.
            if (_authWindow != null)
            {
                _authWindow.ShowDialog();
            }
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            // If the browser window closes, return the focus to the main window.
            if (_authWindow != null && _authWindow.Owner != null)
            {
                _authWindow.Owner.Focus();
            }

            // If the task wasn't completed, the user must have closed the window without logging in.
            if (_taskCompletionSource != null && !_taskCompletionSource.Task.IsCompleted)
            {
                // Set the task completion to indicate a canceled operation.
                _taskCompletionSource.TrySetCanceled();
            }

            _taskCompletionSource = null;
            _authWindow = null;
        }

        // Handle browser navigation (page content changing).
        private void WebBrowserOnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            // Check for a response to the callback url.
            WebBrowser webBrowser = sender as WebBrowser;
            Uri uri = e.Uri;

            // If no browser, uri, or an empty url return.
            if (webBrowser == null || uri == null || _taskCompletionSource == null || String.IsNullOrEmpty(uri.AbsoluteUri))
            { 
                return;
            }

            // Check if the new content is from the callback url.
            bool isRedirected = uri.AbsoluteUri.StartsWith(_callbackUrl);

            if (isRedirected)
            {
                // Cancel the event to prevent it from being handled elsewhere.
                e.Cancel = true;

                // Get a local copy of the task completion source.
                TaskCompletionSource<IDictionary<string,string>> tcs = _taskCompletionSource;
                _taskCompletionSource = null;

                // Close the window.
                if (_authWindow != null)
                {
                    _authWindow.Close();
                }

                // Call a helper function to decode the response parameters (which includes the authorization key).
                IDictionary<string,string> authResponse = DecodeParameters(uri);

                // Set the result for the task completion source.
                tcs.SetResult(authResponse);
            }
        }

        // A helper function that decodes values from a querystring into a dictionary of keys and values.
        private static IDictionary<string, string> DecodeParameters(Uri uri)
        {
            // Create a dictionary of key value pairs returned in an OAuth authorization response URI query string.
            string answer = "";

            // Get the values from the URI fragment or query string.
            if (!String.IsNullOrEmpty(uri.Fragment))
            {
                answer = uri.Fragment.Substring(1);
            }
            else
            {
                if (!String.IsNullOrEmpty(uri.Query))
                {
                    answer = uri.Query.Substring(1);
                }
            }

            // Parse parameters into key / value pairs.
            Dictionary<string,string> keyValueDictionary = new Dictionary<string, string>();
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
}lue);
            }

            // Return the dictionary of string keys/values.
            return keyValueDictionary;
        }
    }
}