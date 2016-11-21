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
using Xamarin.Forms;

namespace TokenChallenge
{
    public partial class TokenChallengePage : ContentPage
    {
        // Constants for the public and secured map service URLs
        private const string PublicMapServiceUrl = "http://services.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer";
        private const string SecureMapServiceUrl = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/USA_secure_user1/MapServer";

        // Constants for the public and secured layer names
        private const string PublicLayerName = "World Street Map - Public";
        private const string SecureLayerName = "USA - Secure";

        // Use a TaskCompletionSource to store the result of a login task
        TaskCompletionSource<Credential> _loginTaskCompletionSrc;

        // Page for the user to enter login information
        LoginPage _loginPage;

        public TokenChallengePage()
        {
            InitializeComponent();

            // Call a function to display a simple map and set up the AuthenticationManager
            Initialize();
        }

        private async void Initialize()
        {
            // Define a challenge handler method for the AuthenticationManager 
            // (this method handles getting credentials when a secured resource is encountered)
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);

            // Create the public layer and provide a name
            var publicLayer = new ArcGISTiledLayer(new Uri(PublicMapServiceUrl));
            publicLayer.Name = PublicLayerName;             

            // Create the secured layer and provide a name
            var tokenSecuredLayer = new ArcGISMapImageLayer(new Uri(SecureMapServiceUrl));
            tokenSecuredLayer.Name = SecureLayerName;

            // Bind the layer to UI controls to show the load status
            PublicLayerStatusPanel.BindingContext = publicLayer;
            SecureLayerStatusPanel.BindingContext = tokenSecuredLayer;
            
            // Create a new map and add the layers
            var myMap = new Map();
            myMap.OperationalLayers.Add(publicLayer);
            myMap.OperationalLayers.Add(tokenSecuredLayer);

            // Add the map to the map view
            MyMapView.Map = myMap;

            // Create the login UI (will display when the user accesses a secured resource)
            _loginPage = new LoginPage();

            // Set up event handlers for when the user completes the login entry or cancels
            _loginPage.OnLoginInfoEntered += LoginInfoEntered;
            _loginPage.OnCanceled += LoginCanceled;
        }

        // AuthenticationManager.ChallengeHandler function that prompts the user for login information to create a credential
        private async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            // Return if authentication is already in process
            if (_loginTaskCompletionSrc != null && !_loginTaskCompletionSrc.Task.IsCanceled) { return null; }

            // Create a new TaskCompletionSource for the login operation
            // (passing the CredentialRequestInfo object to the constructor will make it available from its AsyncState property)
            _loginTaskCompletionSrc = new TaskCompletionSource<Credential>(info);

            // Provide a title for the login form (show which service needs credentials)
#if WINDOWS_UWP
            // UWP doesn't have ServiceUri.GetLeftPart (could use ServiceUri.AbsoluteUri for all, but why not use a compilation condition?)
            _loginPage.TitleText = "Login for " + info.ServiceUri.AbsoluteUri;
#else
            _loginPage.TitleText = "Login for " + info.ServiceUri.GetLeftPart(UriPartial.Path);
#endif
            // Show the login controls on the UI thread
            // OnLoginInfoEntered event will return the values entered (username, password, and domain)
            Device.BeginInvokeOnMainThread(async () => await Navigation.PushAsync(_loginPage));

            // Return the login task, the result will be ready when completed (user provides login info and clicks the "Login" button)
            return await _loginTaskCompletionSrc.Task;
        }

        // Handle the OnLoginEntered event from the login UI
        // LoginEventArgs contains the username, password, and domain that were entered
        private async void LoginInfoEntered(object sender, LoginEventArgs e)
        {
            // Make sure the task completion source has all the information needed
            if (_loginTaskCompletionSrc == null ||
                _loginTaskCompletionSrc.Task == null ||
                _loginTaskCompletionSrc.Task.AsyncState == null)
            {
                return;
            }

            try
            {
                // Get the associated CredentialRequestInfo (will need the URI of the service being accessed)
                CredentialRequestInfo requestInfo = _loginTaskCompletionSrc.Task.AsyncState as CredentialRequestInfo;

                // Create a token credential using the provided username and password
                TokenCredential userCredentials = await AuthenticationManager.Current.GenerateCredentialAsync
                                            (requestInfo.ServiceUri,
                                             e.Username,
                                             e.Password,
                                             requestInfo.GenerateTokenOptions);

                // Set the task completion source result with the ArcGIS network credential
                // AuthenticationManager is waiting for this result and will add it to its Credentials collection
                _loginTaskCompletionSrc.TrySetResult(userCredentials);
            }
            catch (Exception ex)
            {
                // Unable to create credential, set the exception on the task completion source
                _loginTaskCompletionSrc.TrySetException(ex);
            }
            finally
            {
                // Dismiss the login controls
                Navigation.PopToRootAsync();
            }
        }

        private void LoginCanceled(object sender, EventArgs e)
        {
            // Dismiss the login controls
            Navigation.PopToRootAsync();

            // Cancel the task completion source task
            _loginTaskCompletionSrc.TrySetCanceled();
        }
    }
}
