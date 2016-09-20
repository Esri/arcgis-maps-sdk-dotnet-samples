// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace TokenSecuredKnownUser
{
    public sealed partial class MainPage : Page
    {
        // Constants for the public and secured map service URLs
        private const string PublicMapServiceUrl = "http://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer";
        private const string SecureMapServiceUrl = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/USA_secure_user1/MapServer";

        // Constants for the public and secured layer names
        private const string PublicLayerName = "World Street Map - Public";
        private const string SecureLayerName = "USA - Secure";

        public MainPage()
        {
            this.InitializeComponent();

            // Define a method that will try to create the required credentials when a secured resource is encountered
            // (Access to the secure resource will be seamless to the user)
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateKnownCredentials);

            // Call a function to create a new map and load a public and token-secured layer
            Initialize();
        }

        private void Initialize()
        {
            // Create the public layer and provide a name
            var publicLayer = new ArcGISTiledLayer(new Uri(PublicMapServiceUrl));
            publicLayer.Name = PublicLayerName;

            // Set the data context for the public layer stack panel controls (to report name and load status)
            PublicLayerPanel.DataContext = publicLayer;

            // Create the secured layer and provide a name
            var tokenSecuredLayer = new ArcGISMapImageLayer(new Uri(SecureMapServiceUrl));
            tokenSecuredLayer.Name = SecureLayerName;

            // Set the data context for the secure layer stack panel controls (to report name and load status)
            SecureLayerPanel.DataContext = tokenSecuredLayer;

            // Create a new map and add the layers
            var myMap = new Map();
            myMap.OperationalLayers.Add(publicLayer);
            myMap.OperationalLayers.Add(tokenSecuredLayer);
            var t = new TextBlock();

            // Add the map to the map view
            MyMapView.Map = myMap;
        }

        // Challenge method that checks for service access with known (hard coded) credentials
        private async Task<Credential> CreateKnownCredentials(CredentialRequestInfo info)
        {
            // If this isn't the expected resource, the credential will stay null
            Credential knownCredential = null;

            try
            {
                // Check the URL of the requested resource
                if (info.ServiceUri.AbsoluteUri.ToLower().Contains("usa_secure_user1"))
                {
                    // Username and password is hard-coded for this resource
                    // (Would be better to read them from a secure source)
                    string username = "user1";
                    string password = "user1";

                    // Create a credential for this resource
                    knownCredential = await AuthenticationManager.Current.GenerateCredentialAsync
                                            (info.ServiceUri,
                                             username,
                                             password,
                                             info.GenerateTokenOptions);
                }
                else
                {
                    // Another option would be to prompt the user here if the username and password is not known
                }
            }
            catch (Exception ex)
            {
                // Report error accessing a secured resource
                var messageDlg = new MessageDialog("Access to " + info.ServiceUri.AbsoluteUri + " denied. " + ex.Message, "Credential Error");
                messageDlg.ShowAsync();
            }

            // Return the credential
            return knownCredential;
        }
    }
}
