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
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinForms.IWA
{
	public partial class IWAForm : ContentPage
	{
        //TODO - Add the URL for your IWA-secured portal
        const string SecuredPortalUrl = "https://my.secure.portal.com/gis/sharing";

        //TODO - Add the ID for a web map item stored on the secure portal 
        const string WebMapId = "";

        //TODO [optional] - Add hard-coded account information (if present, a network credential will be created on app initialize)
        const string NetworkUsername = "";
        const string NetworkPassword = "";
        const string NetworkDomain = "";

        // Use a TaskCompletionSource to store the result of a login task
        TaskCompletionSource<Credential> _loginTaskCompletionSrc;

        // Page for the user to enter login information
        LoginPage _loginPage;

        public IWAForm ()
		{
			InitializeComponent ();

            // Call a function to display a simple map and set up the AuthenticationManager
            Initialize();
		}

        private async void Initialize()
        {
            // Display a map with an imagery basemap
            MyMapView.Map = new Map(Basemap.CreateImagery());

            // Define a challenge handler method for the AuthenticationManager 
            // (this method handles getting credentials when a secured resource is encountered)
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);

            // Check for hard-coded user name, password, and domain values
            if (!string.IsNullOrEmpty(NetworkUsername) &&
                !string.IsNullOrEmpty(NetworkPassword) &&
                !string.IsNullOrEmpty(NetworkDomain))
            {
                // Create a hard-coded network credential
                ArcGISNetworkCredential hardcodedCredential = new ArcGISNetworkCredential
                {
                    Credentials = new System.Net.NetworkCredential(NetworkUsername, NetworkPassword, NetworkDomain),
                    ServiceUri = new Uri(SecuredPortalUrl)
                };

                // Add the credential to the AuthenticationManager
                AuthenticationManager.Current.AddCredential(hardcodedCredential);
            }

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

            // Show the login controls on the UI thread
            // OnLoginInfoEntered event will return the values entered (username, password, and domain)
            Device.BeginInvokeOnMainThread(async() => await Navigation.PushAsync(_loginPage)); 

            // Return the login task, the result will be ready when completed (user provides login info and clicks the "Login" button)
            return await _loginTaskCompletionSrc.Task;
        }

        // Handle the OnLoginEntered event from the login UI
        // LoginEventArgs contains the username, password, and domain that were entered
        private void LoginInfoEntered(object sender, LoginEventArgs e)
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

                // Create a new network credential using the values entered by the user
                var netCred = new System.Net.NetworkCredential(e.Username, e.Password, e.Domain);

                // Create a new ArcGIS network credential to hold the network credential and service URI
                var arcgisCred = new ArcGISNetworkCredential
                {
                    Credentials = netCred,
                    ServiceUri = requestInfo.ServiceUri
                };

                // Set the task completion source result with the ArcGIS network credential
                // AuthenticationManager is waiting for this result and will add it to its Credentials collection
                _loginTaskCompletionSrc.TrySetResult(arcgisCred);
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

        private async void LoadSecureMapButtonClicked(object sender, EventArgs e)
        {
            // Store messages that describe success or errors connecting to the secured portal and opening the web map
            var messageBuilder = new System.Text.StringBuilder();

            try
            {
                // See if a credential exists for this portal in the AuthenticationManager
                // If a credential is not found, the user will be prompted for login info
                CredentialRequestInfo info = new CredentialRequestInfo
                {
                    ServiceUri = new Uri(SecuredPortalUrl),
                    AuthenticationType = AuthenticationType.NetworkCredential
                };
                Credential cred = await AuthenticationManager.Current.GetCredentialAsync(info, false);

                // Create an instance of the IWA-secured portal
                ArcGISPortal iwaSecuredPortal = await ArcGISPortal.CreateAsync(new Uri(SecuredPortalUrl));

                // Report a successful connection
                messageBuilder.AppendLine("Connected to the portal on " + iwaSecuredPortal.Uri.Host);

                // Report the username for this connection
                if (iwaSecuredPortal.User != null)
                {
                    messageBuilder.AppendLine("Connected as: " + iwaSecuredPortal.User.UserName);
                }
                else
                {
                    // This shouldn't happen (if the portal is truly secured)!
                    messageBuilder.AppendLine("Connected anonymously");
                }

                // Get the web map (portal item) to display                
                var webMap = await PortalItem.CreateAsync(iwaSecuredPortal, WebMapId);
                if (webMap != null)
                {
                    // Create a new map from the portal item and display it in the map view
                    var map = new Map(webMap);
                    MyMapView.Map = map;
                }
            }
            catch (TaskCanceledException)
            {
                // Report canceled login
                messageBuilder.AppendLine("Login was canceled");
            }
            catch (Exception ex)
            {
                // Report error
                messageBuilder.AppendLine("Exception: " + ex.Message);
            }
            finally
            {
                // Set the task completion source to null so user can attempt another login (if it failed)
                _loginTaskCompletionSrc = null;

                // Display the status of the login
                DisplayAlert("Status", messageBuilder.ToString(),"OK");
            }
        }
	}
}
