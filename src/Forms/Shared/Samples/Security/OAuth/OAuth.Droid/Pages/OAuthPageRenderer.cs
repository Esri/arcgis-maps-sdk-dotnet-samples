// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using ArcGISRuntimeXamarin.Samples.OAuth;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Auth;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ArcGISRuntimeXamarin.Samples.OAuth.OAuthPage),
                          typeof(ArcGISRuntimeXamarin.Forms.Droid.OAuthPageAndroid))]
namespace ArcGISRuntimeXamarin.Forms.Droid
{
    // A class that overrides rendering information for the "OAuthPage.xaml.cs" page to challenge for OAuth credentials
    // This allows platform-specific logic to be used for the (shared) Xamarin form page
    public class OAuthPageAndroid : PageRenderer, IOAuthAuthorizeHandler
    {

        // Use a TaskCompletionSource to track the completion of the authorization
        private TaskCompletionSource<IDictionary<string, string>> _taskCompletionSource;

        protected override async void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            // Call the method on the base
            base.OnElementChanged(e);
            
            // Exit if the element (Page) is null
            if (e.OldElement != null || Element == null)
            {
                return;
            }

            // Set up the AuthenticationManager to challenge for ArcGIS Online credentials
            UpdateAuthenticationManager();

            // Force an OAuth challenge
            CredentialRequestInfo info = new CredentialRequestInfo
            {
                AuthenticationType = AuthenticationType.Token,
                ServiceUri = new Uri(OAuthPage.PortalUrl)
            };
            await AuthenticationManager.Current.GetCredentialAsync(info, false);

            // Open the desired web map (portal item)
            ArcGISPortal portal = await ArcGISPortal.CreateAsync(new Uri(OAuthPage.PortalUrl));
            PortalItem item = await PortalItem.CreateAsync(portal, OAuthPage.WebMapId);

            // Create a map
            Map myMap = new Map(item);

            // Set the MyMap property on the shared form to display the map in the map view
            var oauthPage = e.NewElement as OAuthPage;
            oauthPage.MyMap = myMap;
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            // No UI elements are added, the unaltered (shared) form will be shown
            base.OnLayout(changed, l, t, r, b);
        }

        #region OAuth helpers
        private void UpdateAuthenticationManager()
        {
            // OAuth client info
            OAuthClientInfo oauthInfo = new OAuthClientInfo
            {
                ClientId = OAuthPage.AppClientId,
                RedirectUri = new Uri(OAuthPage.OAuthRedirectUrl)
            };

            // If a client secret has been included, add it
            if (!string.IsNullOrEmpty(OAuthPage.ClientSecret))
            {
                oauthInfo.ClientSecret = OAuthPage.ClientSecret;
            }

            // Register the server information (and OAuth info) with the AuthenticationManager
            ServerInfo portalServerInfo = new ServerInfo
            {
                ServerUri = new Uri(OAuthPage.PortalUrl),
                OAuthClientInfo = oauthInfo,
            };

            // Specify OAuthAuthorizationCode if a valid client secret has been specified (need a refresh token, e.g.)            
            if (!string.IsNullOrEmpty(OAuthPage.ClientSecret))
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

            var activity = Context as Activity;

            // Create a new Xamarin.Auth.OAuth2Authenticator using the information passed in
            var authenticator = new OAuth2Authenticator(
                clientId: OAuthPage.AppClientId,
                scope: "",
                authorizeUrl: new Uri(OAuthPage.AuthorizeUrl),
                redirectUrl: new Uri(OAuthPage.OAuthRedirectUrl));

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
                    activity.FinishActivity(99);
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
                    if (_taskCompletionSource != null)
                    {
                        _taskCompletionSource.TrySetCanceled();
                        activity.FinishActivity(99);
                    }
                }
            };

            // Present the OAuth UI (Activity) so the user can enter user name and password
            var intent = authenticator.GetUI(activity);
            activity.StartActivityForResult(intent, 99);

            return _taskCompletionSource.Task;
        }
        #endregion
    }
}