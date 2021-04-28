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
using Android.Content;
using Xamarin.Auth;

namespace ArcGISRuntimeXamarin.Samples.OAuth
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Authenticate with OAuth",
        category: "Security",
        description: "Authenticate with ArcGIS Online (or your own portal) using OAuth2 to access secured resources (such as private web maps or layers).",
        instructions: "When you run the sample, the app will load a web map which contains premium content. You will be challenged for an ArcGIS Online login to view the private layers. Enter a user name and password for an ArcGIS Online named user account (such as your ArcGIS for Developers account). If you authenticate successfully, the traffic layer will display, otherwise the map will contain only the public basemap layer.",
        tags: new[] { "OAuth", "OAuth2", "authentication", "cloud", "credential", "portal", "security" })]
    public class OAuth : Activity, IOAuthAuthorizeHandler
    {
        // Hold a reference to the map view.
        private MapView _myMapView;

        // Use a TaskCompletionSource to track the completion of the authorization.
        private TaskCompletionSource<IDictionary<string, string>> _taskCompletionSource;

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

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "OAuth authorization";

            CreateLayout();

            // Call a function to initialize the app and request a web map (with secured layers) to display.
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
                // If authentication failed, only the public layers will be displayed.
                Map myMap = new Map(portalItem);
                _myMapView.Map = myMap;
            }
            catch (Exception e)
            {
                new AlertDialog.Builder(this).SetMessage(e.ToString()).SetTitle("Error").Show();
            }
        }

        private void SetOAuthInfo()
        {
            // Register the server information with the AuthenticationManager.
            ServerInfo serverInfo = new ServerInfo(new Uri(ServerUrl))
            {
                TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit,
                OAuthClientInfo = new OAuthClientInfo(AppClientId, new Uri(OAuthRedirectUrl))
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

            // Use a function in this class to challenge for credentials.
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);

            // Set the OAuthAuthorizeHandler component (this class).
            AuthenticationManager.Current.OAuthAuthorizeHandler = this;
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            LinearLayout mainLayout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the map view to the layout.
            _myMapView = new MapView(this);
            mainLayout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(mainLayout);
        }

        // ChallengeHandler function that will be called whenever access to a secured resource is attempted.
        public async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            Credential credential = null;

            try
            {
                // IOAuthAuthorizeHandler will challenge the user for OAuth credentials.
                credential = await AuthenticationManager.Current.GenerateCredentialAsync(info.ServiceUri);
            }
            catch (TaskCanceledException) { return credential; }
            catch (Exception)
            {
                // Exception will be reported in calling function.
                throw;
            }

            return credential;
        }


        // IOAuthAuthorizeHandler.AuthorizeAsync implementation.
        public Task<IDictionary<string, string>> AuthorizeAsync(Uri serviceUri, Uri authorizeUri, Uri callbackUri)
        {
            // If the TaskCompletionSource is not null, authorization is in progress.
            if (_taskCompletionSource != null)
            {
                // Allow only one authorization process at a time.
                throw new Exception();
            }

            // Create a task completion source.
            _taskCompletionSource = new TaskCompletionSource<IDictionary<string, string>>();

            // Create a new Xamarin.Auth.OAuth2Authenticator using the information passed in.
            Xamarin.Auth.OAuth2Authenticator authenticator = new OAuth2Authenticator(
                clientId: AppClientId,
                scope: "",
                authorizeUrl: authorizeUri,
                redirectUrl: callbackUri)
            {
                // Allow the user to cancel the OAuth attempt.
                AllowCancel = true
            };

            // Define a handler for the OAuth2Authenticator.Completed event.
            authenticator.Completed += (sender, authArgs) =>
            {
                try
                {
                    // Check if the user is authenticated.
                    if (authArgs.IsAuthenticated)
                    {
                        // If authorization was successful, get the user's account.
                        Xamarin.Auth.Account authenticatedAccount = authArgs.Account;

                        // Set the result (Credential) for the TaskCompletionSource.
                        _taskCompletionSource.SetResult(authenticatedAccount.Properties);
                    }
                }
                catch (Exception ex)
                {
                    // If authentication failed, set the exception on the TaskCompletionSource.
                    _taskCompletionSource.SetException(ex);
                }
                finally
                {
                    // End the OAuth login activity.
                    this.FinishActivity(99);
                }
            };

            // If an error was encountered when authenticating, set the exception on the TaskCompletionSource.
            authenticator.Error += (sndr, errArgs) =>
            {
                // If the user cancels, the Error event is raised but there is no exception ... best to check first.
                if (errArgs.Exception != null)
                {
                    _taskCompletionSource.SetException(errArgs.Exception);
                }
                else
                {
                    // Login canceled: end the OAuth login activity.
                    if (_taskCompletionSource != null)
                    {
                        _taskCompletionSource.TrySetCanceled();
                        this.FinishActivity(99);
                    }
                }
            };

            // Present the OAuth UI (Activity) so the user can enter user name and password.
            Intent intent = authenticator.GetUI(this);
            this.StartActivityForResult(intent, 99);

            // Return completion source task so the caller can await completion.
            return _taskCompletionSource.Task;
        }

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
}