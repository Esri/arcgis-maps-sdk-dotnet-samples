﻿// Copyright 2017 Esri.
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
using System.Threading.Tasks;

namespace ArcGISRuntime.UWP.Samples.OAuth
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
           "Authenticate with OAuth",
           "Security",
           "This sample demonstrates how to authenticate with ArcGIS Online (or your own portal) using OAuth2 to access a secure web map (or the secured layers it contains). Accessing secured items requires a login on the portal (an ArcGIS Online account, for example).",
           "1. When you run the sample, the app will load a web map that contains premium content.\n2. You will be challenged for an ArcGIS Online login to view that layer (world traffic).\n3. Enter your ArcGIS Online user name and password.\n4. If you authenticate successfully, the traffic layer will display, otherwise the map will contain only the public basemap layer.\n5. You can alter the code to supply OAuth configuration settings specific to your app.",
           "Authentication, Security, OAuth")]
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
            
            // Call a function to initialize the app.
            Initialize();
        }

        private async void Initialize()
        {
            // Set up the AuthenticationManager to use OAuth for secure ArcGIS Online requests.
            SetOAuthInfo();

            // Connect to the portal (ArcGIS Online, for example).
            ArcGISPortal arcgisPortal = await ArcGISPortal.CreateAsync(new Uri(ServerUrl));

            // Get a web map portal item using its ID.
            // If the item contains layers not shared publicly, the user will be challenged for credentials at this point.
            PortalItem portalItem = await PortalItem.CreateAsync(arcgisPortal, WebMapId);

            // Get a web map portal item using its ID.
            // If the item contains layers not shared publicly, the user will be challenged for credentials at this point.
            Map myMap = new Map(portalItem);
            MyMapView.Map = myMap;
        }

        private void SetOAuthInfo()
        {
            // Register the server information with the AuthenticationManager.
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

            // Use a function in this class to challenge for credentials.
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);

            // Note: In a WPF app, you need to associate a custom IOAuthAuthorizeHandler component with the AuthenticationManager to 
            //     handle showing OAuth login controls (AuthenticationManager.Current.OAuthAuthorizeHandler = new MyOAuthAuthorize();).
            //     The UWP AuthenticationManager, however, uses a built-in IOAuthAuthorizeHandler (based on WebAuthenticationBroker).
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
}