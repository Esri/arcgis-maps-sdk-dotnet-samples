// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Auth;

namespace ArcGISRuntimeXamarin.Samples.OpenMapOAuth
{
    [Activity(Label = "OAuth", MainLauncher = true, Icon = "@drawable/icon")]
    public class OpenMapOAuth : Activity, IOAuthAuthorizeHandler
    {
        // Create a MapView to display in the app
        private MapView _myMapView = new MapView();
        
        // Use a TaskCompletionSource to track the completion of the authorization
        private TaskCompletionSource<IDictionary<string, string>> _taskCompletionSource;
        
        // Constants for OAuth-related values ...
        // Note: the default values are subject to change, in which case the sample will not work 'out-of-the-box'       
        // TODO: URL of the portal to authenticate with
        private const string PortalUrl = "https://www.arcgis.com/sharing/rest";

        // TODO: Add item ID for a web map on the portal (secured with OAuth)
        private const string WebMapId = "72b6671b89194220b44c4361a17f4fcb";

        // TODO: Add Client ID for an app registered with the portal
        private const string AppClientId = "2Gh53JRzkPtOENQq";

        // TODO: [optional] Provide the client secret for the app (only needed for the OAuthAuthorizationCode auth type)
        private const string ClientSecret = "";

        // TODO: Add URL for redirecting after a successful authorization
        //       Note - this must be a URL configured as a valid Redirect URI with your app
        private const string OAuthRedirectUrl = "https://developers.arcgis.com";

        // URL used by the server for authorization
        private const string AuthorizeUrl = "https://www.arcgis.com/sharing/oauth2/authorize";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Open a map secured with OAuth";

            // Create the UI, setup the control references and execute initialization 
            CreateLayout();
            Initialize();
            UpdateAuthenticationManager();
        }

        private void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateLightGrayCanvas());

            // Show map in the MapView
            _myMapView.Map = myMap;
        }

        private async void OnLoadMapClicked(object sender, EventArgs e)
        {
            // Challenge the user for portal credentials
            CredentialRequestInfo loginInfo = new CredentialRequestInfo();

            // Use the OAuth implicit grant flow
            loginInfo.GenerateTokenOptions = new GenerateTokenOptions
            {
                TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
            };

            // Indicate the url (portal) to authenticate with 
            loginInfo.ServiceUri = new Uri(PortalUrl);

            try
            {
                // Get a reference to the (singleton) AuthenticationManager for the app
                AuthenticationManager thisAuthenticationManager = AuthenticationManager.Current;

                // Call GetCredentialAsync on the AuthenticationManager to invoke the challenge handler
                await thisAuthenticationManager.GetCredentialAsync(loginInfo, false);

                // Access the portal
                ArcGISPortal portal = await ArcGISPortal.CreateAsync(new Uri(PortalUrl));

                // Get the item (web map) from the portal
                PortalItem item = await PortalItem.CreateAsync(portal, WebMapId);

                // Make sure it's a web map
                if (item.Type != PortalItemType.WebMap)
                {
                    return;
                }

                // Display the web map in the map view
                Map webMap = new Map(item);
                _myMapView.Map = webMap;
            }
            catch (System.OperationCanceledException)
            {
                // User canceled the login
                var alertBuilder = new AlertDialog.Builder(this);
                alertBuilder.SetTitle("Cancel");
                alertBuilder.SetMessage("Portal login was canceled.");
                alertBuilder.Show();
            }
            catch(Exception ex)
            {
                // Other exception
                var alertBuilder = new AlertDialog.Builder(this);
                alertBuilder.SetTitle("Error");
                alertBuilder.SetMessage(ex.Message);
                alertBuilder.Show();
            }
        }

        private void CreateLayout()
        {
            // Create a button to load the web map from the portal
            var loadMapButton = new Button(this);
            loadMapButton.Text = "Open web map";
            loadMapButton.Click += OnLoadMapClicked;

            // Create a new vertical layout for the app (button followed by map view)
            var mainLayout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the load map button
            mainLayout.AddView(loadMapButton);

            // Add the map view to the layout
            mainLayout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(mainLayout);
        }

        #region OAuth helpers
        private void UpdateAuthenticationManager()
        {
            // OAuth client info
            OAuthClientInfo oauthInfo = new OAuthClientInfo
            {
                ClientId = AppClientId,
                RedirectUri = new Uri(OAuthRedirectUrl)
            };
            
            // If a client secret has been included, add it
            if (!string.IsNullOrEmpty(ClientSecret))
            {
                oauthInfo.ClientSecret = ClientSecret;
            }

            // Register the server information (and OAuth info) with the AuthenticationManager
            ServerInfo portalServerInfo = new ServerInfo
            {
                ServerUri = new Uri(PortalUrl),
                OAuthClientInfo = oauthInfo,
            };

            // Specify OAuthAuthorizationCode if a valid client secret has been specified (need a refresh token, e.g.)            
            if (!string.IsNullOrEmpty(ClientSecret))
            {
                portalServerInfo.TokenAuthenticationType = TokenAuthenticationType.OAuthAuthorizationCode;
            }
            else
            {
                // Otherwise, use OAuthImplicit
                portalServerInfo.TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit;
            }

            // Get a reference to the (singleton) AuthenticationManager for the app
            AuthenticationManager thisAuthenticationManager = AuthenticationManager.Current;

            // Register the server information
            thisAuthenticationManager.RegisterServer(portalServerInfo);
            
            // Assign the method that AuthenticationManager will call to challenge for secured resources
            thisAuthenticationManager.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);

            // Set the OAuth authorization handler to this class (Implements IOAuthAuthorize interface)
            thisAuthenticationManager.OAuthAuthorizeHandler = this;
        }

        // ChallengeHandler function for AuthenticationManager, called whenever access to a secured resource is attempted
        private async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
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

        // IOAuthAuthorizeHandler.AuthorizeAsync implementation
        public Task<IDictionary<string, string>> AuthorizeAsync(Uri serviceUri, Uri authorizeUri, Uri callbackUri)
        {
            // If the TaskCompletionSource is not null, authorization is in progress
            if (_taskCompletionSource != null)
            {
                // Allow only one authorization process at a time
                throw new Exception();
            }

            // Create a task completion source
            _taskCompletionSource = new TaskCompletionSource<IDictionary<string, string>>();

            // Create a new Xamarin.Auth.OAuth2Authenticator using the information passed in
            Xamarin.Auth.OAuth2Authenticator authenticator = new OAuth2Authenticator(
                clientId:  AppClientId,
                scope: "",
                authorizeUrl: authorizeUri,
                redirectUrl: callbackUri);

            // Allow the user to cancel the OAuth attempt
            authenticator.AllowCancel = true;

            // Define a handler for the OAuth2Authenticator.Completed event
            authenticator.Completed += (sender, authArgs) =>
            {
                try
                {
                    // Check if the user is authenticated
                    if (authArgs.IsAuthenticated)
                    {
                        // If authorization was successful, get the user's account
                        Xamarin.Auth.Account authenticatedAccount = authArgs.Account;

                        // Set the result (Credential) for the TaskCompletionSource
                        _taskCompletionSource.SetResult(authenticatedAccount.Properties);
                    }
                }
                catch (Exception ex)
                {
                    // If authentication failed, set the exception on the TaskCompletionSource
                    _taskCompletionSource.SetException(ex);
                }
                finally
                {
                    // End the OAuth login activity
                    this.FinishActivity(99);
                }
            };

            // If an error was encountered when authenticating, set the exception on the TaskCompletionSource
            authenticator.Error += (sndr, errArgs) =>
            {
                // If the user cancels, the Error event is raised but there is no exception ... best to check first
                if (errArgs.Exception != null)
                {
                    _taskCompletionSource.SetException(errArgs.Exception);
                }
                else
                {
                    // Login canceled: end the OAuth login activity
                    if(_taskCompletionSource != null)
                    {
                        _taskCompletionSource.TrySetCanceled();
                        this.FinishActivity(99);
                    }
                }
            };

            // Present the OAuth UI (Activity) so the user can enter user name and password
            var intent = authenticator.GetUI(this);
            this.StartActivityForResult(intent, 99);

            // Return completion source task so the caller can await completion
            return _taskCompletionSource.Task;
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
        #endregion
    }
}