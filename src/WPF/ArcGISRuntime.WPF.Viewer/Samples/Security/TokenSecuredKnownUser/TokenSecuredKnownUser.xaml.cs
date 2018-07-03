// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Security;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGISRuntime.WPF.Samples.TokenSecuredKnownUser
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
           "ArcGIS token with a known user",
           "Security",
           "This sample demonstrates how to authenticate with ArcGIS Server using ArcGIS Tokens to access a secure service. Accessing secured services requires a login that's been defined on the server.",
           "1. When you run the sample, the app will load a map that contains a layer from a secured service.\n2. You will NOT be challenged for a user name and password to view that layer because that info has been hard-coded into the app.\n3. If the credentials in the code are correct, the secured layer will display, otherwise the map will contain only the public layers.",
           "Authentication, Security, ArcGIS Token")]
    public partial class TokenSecuredKnownUser
    {
        public TokenSecuredKnownUser()
        {
            InitializeComponent();
            // Define a method that will try to create the required credentials when a secured resource is encountered.
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateKnownCredentials);
        }

        // Challenge method that checks for service access with known (hard coded) credentials.
        private async Task<Credential> CreateKnownCredentials(CredentialRequestInfo info)
        {
            // If this isn't the expected resource, the credential will stay null.
            Credential knownCredential = null;

            try
            {
                // Check the URL of the requested resource.
                if (info.ServiceUri.AbsoluteUri.ToLower().Contains("usa_secure_user1"))
                {
                    // Username and password is hard-coded for this resource (it would be better to read this from a secure source).
                    string username = "user1";
                    string password = "user1";

                    // Create a credential for this resource.
                    knownCredential = await AuthenticationManager.Current.GenerateCredentialAsync
                                            (info.ServiceUri,
                                             username,
                                             password,
                                             info.GenerateTokenOptions);
                }
                else
                {
                    // TODO: could prompt the user here for other ArcGIS Token-secured resources.
                }
            }
            catch (Exception ex)
            {
                // Report error accessing a secured resource.
                MessageBox.Show("Access to " + info.ServiceUri.AbsoluteUri + " denied. " + ex.Message, "Credential Error");
            }

            // Return the credential.
            return knownCredential;
        }
    }
}