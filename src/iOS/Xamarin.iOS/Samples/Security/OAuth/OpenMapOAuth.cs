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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Auth;

namespace ArcGISRuntimeXamarin.Samples.OpenMapOAuth
{
    [Register("OpenMapOAuth")]
    public class OpenMapOAuth : UIViewController, IOAuthAuthorizeHandler
    {
        // Create a MapView to display in the app
        private MapView _myMapView = new MapView();

        // Use a TaskCompletionSource to track the completion of the authorization
        private TaskCompletionSource<IDictionary<string, string>> _taskCompletionSource;

        // Constants for OAuth-related values ...
        // TODO: URL of the portal to authenticate with
        private const string PortalUrl = "https://www.arcgis.com/sharing/rest";

        // TODO: Add item ID for a web map on the portal (secured with OAuth)
        private const string WebMapId = "";

        // TODO: Add Client ID for an app registered with the portal
        private const string AppClientId = "";

        // TODO: [optional] Provide the client secret for the app (only needed for the OAuthAuthorizationCode auth type)
        private const string ClientSecret = "";

        // TODO: Add URL for redirecting after a successful authorization
        //       Note - this must be a URL configured as a valid Redirect URI with your app
        private const string OAuthRedirectUrl = "";

        // URL used by the server for authorization
        private const string AuthorizeUrl = "https://www.arcgis.com/sharing/oauth2/authorize";

        public OpenMapOAuth()
        {
            Title = "Open a secured web map";
        }
        
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the layout
            CreateLayout();

            // Initialize the app
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with a light gray canvas basemap
            Map myMap = new Map(Basemap.CreateLightGrayCanvas());

            // Add the Map to the MapView
            _myMapView.Map = myMap;

            // Set up the challenge handler and OAuth authorization 
            UpdateAuthenticationManager();
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
            catch (OperationCanceledException)
            {
                // User canceled the login
                var alert = UIAlertController.Create("Cancel", "Portal login was canceled.", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);
            }
            catch (Exception ex)
            {
                // Other exception
                var alert = UIAlertController.Create("Error", ex.Message, UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);
            }
        }

        private void CreateLayout()
        {
            // Define an offset from the top of the page (to account for the iOS status bar)
            var yPageOffset = 60;

            // Create a new MapView control
            _myMapView = new MapView();

            // Define the visual frame for the MapView
            _myMapView.Frame = new CoreGraphics.CGRect(0, yPageOffset, View.Bounds.Width, View.Bounds.Height - yPageOffset);


            // Add a button control to add a portal web map
            var loadMapButton = new UIBarButtonItem() { Title = "Load Web Map", Style = UIBarButtonItemStyle.Plain, Enabled = true };

            // Handle the button click event
            loadMapButton.Clicked += OnLoadMapClicked;

            // Add the button to the toolbar
            SetToolbarItems(new UIBarButtonItem[] { loadMapButton }, false);

            // Show the toolbar
            NavigationController.ToolbarHidden = false;

            // Add the MapView 
            View.AddSubviews(_myMapView);
        }

        #region OAuth helpers
        private void UpdateAuthenticationManager()
        {
            // Register the server information with the AuthenticationManager
            ServerInfo portalServerInfo = new ServerInfo
            {
                ServerUri = new Uri(PortalUrl),
                OAuthClientInfo = new OAuthClientInfo
                {
                    ClientId = AppClientId,
                    RedirectUri = new Uri(OAuthRedirectUrl)
                },
                // Specify OAuthAuthorizationCode if you need a refresh token (and have specified a valid client secret)
                // Otherwise, use OAuthImplicit
                TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
            };

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
            Xamarin.Auth.OAuth2Authenticator auth = new OAuth2Authenticator(
                clientId: AppClientId,
                scope: "",
                authorizeUrl: new Uri(AuthorizeUrl),
                redirectUrl: new Uri(OAuthRedirectUrl));

            // Allow the user to cancel the OAuth attempt
            auth.AllowCancel = true;

            // Define a handler for the OAuth2Authenticator.Completed event
            auth.Completed += (sender, authArgs) =>
            {
                try
                {
                    // Dismiss the OAuth UI when complete
                    this.DismissViewController(true, null);

                    // Throw an exception if the user could not be authenticated
                    if (!authArgs.IsAuthenticated)
                    {
                        throw new Exception("Unable to authenticate user.");
                    }

                    // If authorization was successful, get the user's account
                    Xamarin.Auth.Account authenticatedAccount = authArgs.Account;
                  
                    // Set the result (Credential) for the TaskCompletionSource
                    _taskCompletionSource.SetResult(authenticatedAccount.Properties);
                }
                catch (Exception ex)
                {
                    // If authentication failed, set the exception on the TaskCompletionSource
                    _taskCompletionSource.SetException(ex);
                }
            };

            // If an error was encountered when authenticating, set the exception on the TaskCompletionSource
            auth.Error += (sndr, errArgs) =>
            {
                if (errArgs.Exception != null)
                {
                    _taskCompletionSource.TrySetException(errArgs.Exception);
                }
                else
                {
                    _taskCompletionSource.TrySetException(new Exception(errArgs.Message));
                }
            };

            // Present the OAuth UI (on the app's UI thread) so the user can enter user name and password
            InvokeOnMainThread(() =>
            {
                this.PresentViewController(auth.GetUI(), true, null);
            });

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