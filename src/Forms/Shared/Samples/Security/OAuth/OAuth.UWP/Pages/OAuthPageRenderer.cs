// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using ArcGISRuntimeXamarin.Samples.OAuth;
using Esri.ArcGISRuntime.Security;
using System.Threading.Tasks;
using Xamarin.Forms.Platform.UWP;
using Xamarin.Forms;
using System;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Mapping;

[assembly: ExportRenderer(typeof(OAuthPage), typeof(ArcGISRuntimeXamarin.Forms.UWP.OAuthPageUWP))]
namespace ArcGISRuntimeXamarin.Forms.UWP
{
    // A class that overrides rendering information for the "OAuthPage.xaml.cs" page to challenge for OAuth credentials
    // This allows platform-specific logic to be used for the (shared) Xamarin form page
    public class OAuthPageUWP : PageRenderer
    {
        protected async override void OnElementChanged(ElementChangedEventArgs<Page> e)
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

        #region OAuth helpers
        private void UpdateAuthenticationManager()
        {
            // Register the server information with the AuthenticationManager
            ServerInfo portalServerInfo = new ServerInfo
            {
                ServerUri = new Uri(OAuthPage.PortalUrl),
                OAuthClientInfo = new OAuthClientInfo
                {
                    ClientId = OAuthPage.AppClientId,
                    RedirectUri = new Uri(OAuthPage.OAuthRedirectUrl)
                },
                // Specify OAuthAuthorizationCode if you need a refresh token (and have specified a valid client secret)
                // Otherwise, use OAuthImplicit
                TokenAuthenticationType = TokenAuthenticationType.OAuthImplicit
            };

            // Get a reference to the (singleton) AuthenticationManager for the app
            AuthenticationManager thisAuthenticationManager = AuthenticationManager.Current;

            // Register the server information
            thisAuthenticationManager.RegisterServer(portalServerInfo);

            // Create a new ChallengeHandler that uses a method in this class to challenge for credentials
            thisAuthenticationManager.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);
        }

        // ChallengeHandler function for AuthenticationManager that will be called whenever access to a secured
        // resource is attempted
        public async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            OAuthTokenCredential credential = null;

            try
            {
                // Create generate token options if necessary
                if (info.GenerateTokenOptions == null)
                {
                    info.GenerateTokenOptions = new GenerateTokenOptions();
                }

                // IOAuthAuthorizeHandler will challenge the user for credentials
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
        #endregion        
    }
}
