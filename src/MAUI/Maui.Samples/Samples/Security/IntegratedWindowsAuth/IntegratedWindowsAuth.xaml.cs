// Copyright 2018 Esri.
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
using System.Runtime.InteropServices;
using System.Text;

namespace ArcGIS.Samples.IntegratedWindowsAuth
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Integrated Windows Authentication",
        category: "Security",
        description: "Connect to an IWA secured Portal and search for maps.",
        instructions: "1. Enter the URL to your IWA-secured portal.",
        tags: new[] { "Portal", "Windows", "authentication", "security" })]
    public partial class IntegratedWindowsAuth : ContentPage
    {
        // An Integrated Windows Authentication secured portal.
        private ArcGISPortal _iwaSecuredPortal = null;

        private List<String> _itemIDs;

        // A TaskCompletionSource to store the result of a login task.
        private TaskCompletionSource<Credential> _loginTaskCompletionSrc;

        // Page for the user to enter login information.
        private IWALoginPage _loginPage;

        public IntegratedWindowsAuth()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Define a challenge handler method for the AuthenticationManager.
            // This method handles getting credentials when a secured resource is encountered.
            // Note: for IWA-secured services, the current system credentials will be used by default.
            // The user will only be challenged for resources the current account doesn't have permissions for.
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(CreateCredentialAsync);

            // Create the login UI.
            _loginPage = new IWALoginPage();

            // Set up event handlers for when the user completes the login entry or cancels.
            _loginPage.OnLoginInfoEntered += LoginInfoEntered;
            _loginPage.OnCanceled += LoginCanceled;
        }

        // AuthenticationManager.ChallengeHandler function that prompts the user for login information to create a credential.
        private async Task<Credential> CreateCredentialAsync(CredentialRequestInfo info)
        {
            // Create a new TaskCompletionSource for the login operation.
            // Passing the CredentialRequestInfo object to the constructor will make it available from its AsyncState property.
            _loginTaskCompletionSrc = new TaskCompletionSource<Credential>(info);

            // Provide a title for the login form (show which service needs credentials).
            _loginPage.TitleText = "Login for " + info.ServiceUri.GetLeftPart(UriPartial.Authority);

            // Show the login controls on the UI thread.
            // OnLoginInfoEntered event will return the values entered (username, password, and domain).
            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(async () => await Application.Current.MainPage.Navigation.PushAsync(_loginPage));

            // Return the login task, the result will be ready when completed (user provides login info and clicks the "Login" button).
            return await _loginTaskCompletionSrc.Task;
        }

        // Handle the OnLoginEntered event from the login UI.
        private async void LoginInfoEntered(object sender, IWALoginEventArgs e)
        {
            try
            {
                // Get the associated CredentialRequestInfo.
                CredentialRequestInfo requestInfo = (CredentialRequestInfo)_loginTaskCompletionSrc.Task.AsyncState;

                // Obtain credentials.
                Credential cred = new ArcGISNetworkCredential(requestInfo.ServiceUri, e.Username, e.Password, e.Domain);

                // Set the task completion source result with the ArcGIS network credential.
                // AuthenticationManager is waiting for this result and will add it to its Credentials collection.
                _loginTaskCompletionSrc.TrySetResult(cred);
                await Application.Current.MainPage.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                // Unable to create credential, set the exception on the task completion source.
                _loginTaskCompletionSrc.TrySetException(ex);
                await Application.Current.MainPage.Navigation.PopAsync();
            }
        }

        private void LoginCanceled(object sender, EventArgs e)
        {
            // Dismiss the login controls.
            Application.Current.MainPage.Navigation.PopAsync();

            // Cancel the task completion source task.
            _loginTaskCompletionSrc.TrySetResult(null);

            MessagesLabel.Text = "Login canceled.";
        }

        // Search the IWA-secured portal for web maps and display the results in a list.
        private async void SearchSecureMapsButtonClick(object sender, EventArgs e)
        {
            var messageBuilder = new StringBuilder();

            try
            {
                // Get the string entered for the secure portal URL.
                string securedPortalUrl = SecurePortalUrlEntry.Text.Trim();

                // Make sure a portal URL has been entered in the text box.
                if (string.IsNullOrEmpty(securedPortalUrl))
                {
                    await Application.Current.MainPage.DisplayAlert("Missing URL", "Please enter the URL of the secured portal.", "OK");
                    return;
                }

                // Determine the OS - if not on Windows, an explicit login is required.
                bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

                // Create an instance of the IWA-secured portal - the user may be challenged for access.
                _iwaSecuredPortal = await ArcGISPortal.CreateAsync(new Uri(securedPortalUrl), !isWindows);

                MyMapView.Map = null;

                // Show a status message.
                MessagesLabel.Text = "Searching for web map items on the portal at " + _iwaSecuredPortal.Uri.AbsoluteUri;

                // Report connection info.
                messageBuilder.AppendLine("Connected to the portal on " + _iwaSecuredPortal.Uri.Host);

                // Report the user name used for this connection.
                if (_iwaSecuredPortal.User != null)
                {
                    messageBuilder.AppendLine("Connected as: " + _iwaSecuredPortal.User.UserName);
                }
                else
                {
                    // Note: This shouldn't happen for a secure portal!
                    messageBuilder.AppendLine("Anonymous");
                }

                // Search the portal for web maps.
                var items = await _iwaSecuredPortal.FindItemsAsync(new PortalQueryParameters("type:(\"web map\" NOT \"web mapping application\")"));

                // Build a list of items from the results that shows the map name.
                var titleItems = from r in items.Results select r.Title;

                // Add the items to the picker.
                foreach (var itm in titleItems)
                {
                    MapPicker.Items.Add(itm);
                }

                // Build a list of portal item IDs for when the user changes the map.
                var portalItems = from r in items.Results select r.ItemId;
                _itemIDs = new List<string>();
                foreach (var itm in portalItems)
                {
                    _itemIDs.Add(itm);
                }
                
                // Workaround a MAUI bug.
                MapPicker.ItemsSource = MapPicker.GetItemsAsArray();

                // Enable the picker and raise a PickerSelectedIndexChanged event.
                MapPicker.IsVisible = true;
                MapPicker.SelectedIndex = 0;

                // Hide UI elements which aren't being furthered utilized.
                InstructionLabel.IsVisible = false;
                SecurePortalUrlEntry.IsVisible = false;
                // User can restart sample if they'd like to use another portal.
                SearchSecureMapsButton.IsVisible = false;
            }
            catch (Exception ex)
            {
                // Report errors searching the portal.
                messageBuilder.AppendLine(ex.Message);
            }
            finally
            {
                // Show messages.
                MessagesLabel.Text = messageBuilder.ToString();
            }
        }

        private async void PickerSelectedIndexChanged(object sender, EventArgs e)
        {
            // Clear status messages.
            MessagesLabel.Text = string.Empty;

            // Store status (or errors) when adding the map.
            var statusInfo = new StringBuilder();

            try
            {
                MyMapView.Map = null;

                // Get the portal item ID index from the selected picker item.
                string itemID = _itemIDs[(sender as Picker).SelectedIndex];

                // Use the item ID to create a PortalItem from the portal.
                var portalItem = await PortalItem.CreateAsync(_iwaSecuredPortal, itemID);

                if (portalItem != null)
                {
                    // Create a Map using the web map (portal item).
                    MyMapView.Map = new Map(portalItem);

                    // Report success.
                    statusInfo.AppendLine("Successfully loaded web map from item #" + itemID + " from " + portalItem.Url.Host);
                }
            }
            catch (Exception ex)
            {
                // Add an error message.
                statusInfo.AppendLine("Error accessing web map: " + ex.Message);
            }
            finally
            {
                // Show messages.
                MessagesLabel.Text = statusInfo.ToString();
            }
        }
    }
}
